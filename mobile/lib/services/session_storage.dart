import 'dart:convert';

import 'package:flutter_secure_storage/flutter_secure_storage.dart';

import '../models/auth.dart';

class SessionStorage {
  static const _key = 'aslsu.session';
  final _storage = const FlutterSecureStorage();

  Future<AuthResponse?> getSession() async {
    final raw = await _storage.read(key: _key);
    if (raw == null) return null;
    try {
      return AuthResponse.fromJson(jsonDecode(raw) as Map<String, dynamic>);
    } catch (_) {
      return null;
    }
  }

  Future<void> setSession(AuthResponse session) async {
    await _storage.write(key: _key, value: jsonEncode(session.toJson()));
  }

  Future<void> clearSession() async {
    await _storage.delete(key: _key);
  }
}
