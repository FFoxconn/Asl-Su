import 'dart:convert';

import 'package:http/http.dart' as http;

import 'session_storage.dart';

class ApiException implements Exception {
  final int status;
  final String message;
  ApiException(this.status, this.message);

  @override
  String toString() => message;
}

class ApiClient {
  static const String baseUrl = String.fromEnvironment(
    'API_BASE_URL',
    defaultValue: 'https://aslsu.169-58-78-230.sslip.io',
  );

  final SessionStorage sessionStorage;
  void Function()? onUnauthorized;

  ApiClient(this.sessionStorage);

  Future<dynamic> get(String path) => _request('GET', path);

  Future<dynamic> post(String path, {Map<String, dynamic>? body}) =>
      _request('POST', path, body: body);

  Future<dynamic> _request(String method, String path, {Map<String, dynamic>? body}) async {
    final session = await sessionStorage.getSession();
    final headers = <String, String>{
      'Content-Type': 'application/json',
      if (session != null) 'Authorization': 'Bearer ${session.accessToken}',
    };
    final uri = Uri.parse('$baseUrl$path');
    final encodedBody = body != null ? jsonEncode(body) : null;

    http.Response response;
    try {
      response = method == 'POST'
          ? await http.post(uri, headers: headers, body: encodedBody)
          : await http.get(uri, headers: headers);
    } catch (_) {
      throw ApiException(0, "Backend'e ulaşılamadı.");
    }

    if (response.statusCode == 401) {
      await sessionStorage.clearSession();
      onUnauthorized?.call();
      throw ApiException(401, 'Oturum süresi doldu, lütfen tekrar giriş yapın.');
    }

    if (response.statusCode < 200 || response.statusCode >= 300) {
      var message = 'İstek başarısız oldu.';
      try {
        final decoded = jsonDecode(response.body);
        message = (decoded['title'] ?? decoded['message'] ?? message) as String;
      } catch (_) {
        // response body wasn't JSON — keep the generic message.
      }
      throw ApiException(response.statusCode, message);
    }

    if (response.body.isEmpty) {
      return null;
    }

    return jsonDecode(response.body);
  }
}
