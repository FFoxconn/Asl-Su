import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import 'providers/auth_provider.dart';
import 'screens/root_screen.dart';
import 'services/api_client.dart';
import 'services/auth_service.dart';
import 'services/location_service.dart';
import 'services/session_storage.dart';
import 'theme.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  try {
    await initializeLocationService();
  } catch (_) {
    // Location tracking is a courier-only extra — a setup failure here must
    // never take the whole app down with it.
  }
  runApp(const AslSuApp());
}

class AslSuApp extends StatelessWidget {
  const AslSuApp({super.key});

  @override
  Widget build(BuildContext context) {
    final sessionStorage = SessionStorage();
    final apiClient = ApiClient(sessionStorage);
    final authService = AuthService(apiClient);

    return ChangeNotifierProvider(
      create: (_) => AuthProvider(sessionStorage, authService, apiClient),
      child: MaterialApp(
        title: 'Asl-Su',
        debugShowCheckedModeBanner: false,
        theme: buildAppTheme(),
        home: const RootScreen(),
      ),
    );
  }
}
