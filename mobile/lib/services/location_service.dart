import 'dart:async';
import 'dart:convert';
import 'dart:ui';

import 'package:flutter_background_service/flutter_background_service.dart';
import 'package:flutter_local_notifications/flutter_local_notifications.dart';
import 'package:geolocator/geolocator.dart';
import 'package:http/http.dart' as http;
import 'package:permission_handler/permission_handler.dart';

import 'api_client.dart';
import 'session_storage.dart';

const _notificationChannelId = 'aslsu_location';
const _notificationId = 9001;

/// Configures the background service. Call once, before runApp().
///
/// The notification channel MUST exist on the OS before the service tries to
/// post its foreground notification — skipping this makes Android kill the
/// whole app with CannotPostForegroundServiceNotificationException the moment
/// the service starts (this bit us once; don't remove it).
Future<void> initializeLocationService() async {
  const channel = AndroidNotificationChannel(
    _notificationChannelId,
    'Asl-Su Konum Takibi',
    description: 'Kurye konumu paylaşılırken gösterilen bildirim.',
    importance: Importance.low,
  );

  await FlutterLocalNotificationsPlugin()
      .resolvePlatformSpecificImplementation<AndroidFlutterLocalNotificationsPlugin>()
      ?.createNotificationChannel(channel);

  final service = FlutterBackgroundService();
  await service.configure(
    androidConfiguration: AndroidConfiguration(
      onStart: onLocationServiceStart,
      autoStart: false,
      autoStartOnBoot: false,
      isForegroundMode: true,
      notificationChannelId: _notificationChannelId,
      initialNotificationTitle: 'Asl-Su Kurye',
      initialNotificationContent: 'Konum paylaşımı hazırlanıyor...',
      foregroundServiceNotificationId: _notificationId,
      foregroundServiceTypes: [AndroidForegroundType.location],
    ),
    iosConfiguration: IosConfiguration(autoStart: false),
  );
}

/// Requests the permissions needed for continuous background tracking.
/// Returns true only if "Allow all the time" was granted.
Future<bool> requestLocationPermissions() async {
  final whenInUse = await Permission.locationWhenInUse.request();
  if (!whenInUse.isGranted) {
    return false;
  }
  await Permission.notification.request();
  final always = await Permission.locationAlways.request();
  return always.isGranted;
}

Future<void> startLocationTracking() async {
  final service = FlutterBackgroundService();
  if (await service.isRunning()) {
    return;
  }
  await service.startService();
}

Future<void> stopLocationTracking() async {
  final service = FlutterBackgroundService();
  if (await service.isRunning()) {
    service.invoke('stopService');
  }
}

/// Runs in its own isolate, independent of the app UI — this is what lets
/// location keep being reported after the courier closes the app.
@pragma('vm:entry-point')
void onLocationServiceStart(ServiceInstance service) async {
  DartPluginRegistrant.ensureInitialized();

  if (service is AndroidServiceInstance) {
    service.on('stopService').listen((event) {
      service.stopSelf();
    });
  }

  final sessionStorage = SessionStorage();

  Future<void> reportOnce() async {
    final session = await sessionStorage.getSession();
    if (session == null || session.role != 'Courier') {
      service.stopSelf();
      return;
    }

    try {
      final position = await Geolocator.getCurrentPosition(
        locationSettings: const LocationSettings(accuracy: LocationAccuracy.high),
      );

      await http.put(
        Uri.parse('${ApiClient.baseUrl}/api/couriers/me/location'),
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer ${session.accessToken}',
        },
        body: jsonEncode({'latitude': position.latitude, 'longitude': position.longitude}),
      );

      if (service is AndroidServiceInstance && await service.isForegroundService()) {
        final now = DateTime.now();
        final hh = now.hour.toString().padLeft(2, '0');
        final mm = now.minute.toString().padLeft(2, '0');
        service.setForegroundNotificationInfo(
          title: 'Asl-Su Kurye — Konum paylaşımı aktif',
          content: 'Son güncelleme: $hh:$mm',
        );
      }
    } catch (_) {
      // Best-effort: a network hiccup shouldn't crash the background isolate,
      // the next periodic tick will just try again.
    }
  }

  await reportOnce();
  Timer.periodic(const Duration(seconds: 60), (_) => reportOnce());
}
