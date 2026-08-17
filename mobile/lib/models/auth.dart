class AuthResponse {
  final String accessToken;
  final String accessTokenExpiresAtUtc;
  final String refreshToken;
  final String displayName;
  final String role;

  AuthResponse({
    required this.accessToken,
    required this.accessTokenExpiresAtUtc,
    required this.refreshToken,
    required this.displayName,
    required this.role,
  });

  factory AuthResponse.fromJson(Map<String, dynamic> json) => AuthResponse(
        accessToken: json['accessToken'] as String,
        accessTokenExpiresAtUtc: json['accessTokenExpiresAtUtc'] as String,
        refreshToken: json['refreshToken'] as String,
        displayName: json['displayName'] as String,
        role: json['role'] as String,
      );

  Map<String, dynamic> toJson() => {
        'accessToken': accessToken,
        'accessTokenExpiresAtUtc': accessTokenExpiresAtUtc,
        'refreshToken': refreshToken,
        'displayName': displayName,
        'role': role,
      };
}
