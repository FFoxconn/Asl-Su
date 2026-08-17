import '../models/auth.dart';
import 'api_client.dart';

class AuthService {
  final ApiClient _client;
  AuthService(this._client);

  Future<AuthResponse> login(String email, String password) async {
    final json = await _client.post('/api/auth/login', body: {'email': email, 'password': password});
    return AuthResponse.fromJson(json as Map<String, dynamic>);
  }
}

class HealthStatus {
  final String status;
  final String timeUtc;
  HealthStatus({required this.status, required this.timeUtc});

  factory HealthStatus.fromJson(Map<String, dynamic> json) =>
      HealthStatus(status: json['status'] as String, timeUtc: json['timeUtc'] as String);
}

class HealthService {
  final ApiClient _client;
  HealthService(this._client);

  Future<HealthStatus> getHealth() async {
    final json = await _client.get('/api/health');
    return HealthStatus.fromJson(json as Map<String, dynamic>);
  }
}
