import 'dart:async';

import 'package:flutter/foundation.dart';

import '../models/auth.dart';
import '../services/api_client.dart';
import '../services/auth_service.dart';
import '../services/location_service.dart';
import '../services/session_storage.dart';

class AuthProvider extends ChangeNotifier {
  final SessionStorage _sessionStorage;
  final AuthService _authService;
  final ApiClient apiClient;

  AuthResponse? _session;
  bool _isLoading = true;

  AuthProvider(this._sessionStorage, this._authService, this.apiClient) {
    apiClient.onUnauthorized = _handleUnauthorized;
    _restoreSession();
  }

  AuthResponse? get session => _session;
  bool get isAuthenticated => _session != null;
  bool get isLoading => _isLoading;

  Future<void> _restoreSession() async {
    _session = await _sessionStorage.getSession();
    _isLoading = false;
    notifyListeners();
    if (_session?.role == 'Courier') {
      unawaited(_ensureLocationTracking());
    }
  }

  Future<void> login(String email, String password) async {
    final result = await _authService.login(email, password);
    await _sessionStorage.setSession(result);
    _session = result;
    notifyListeners();
    if (result.role == 'Courier') {
      unawaited(_ensureLocationTracking());
    }
  }

  Future<void> logout() async {
    await _sessionStorage.clearSession();
    _session = null;
    notifyListeners();
    unawaited(stopLocationTracking());
  }

  void _handleUnauthorized() {
    _session = null;
    notifyListeners();
    unawaited(stopLocationTracking());
  }

  Future<void> _ensureLocationTracking() async {
    final granted = await requestLocationPermissions();
    if (granted) {
      await startLocationTracking();
    }
  }
}
