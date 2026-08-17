import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../providers/auth_provider.dart';
import '../services/api_client.dart';
import '../services/auth_service.dart';
import '../theme.dart';
import 'products_screen.dart';

class DashboardScreen extends StatefulWidget {
  const DashboardScreen({super.key});

  @override
  State<DashboardScreen> createState() => _DashboardScreenState();
}

class _DashboardScreenState extends State<DashboardScreen> {
  HealthStatus? _health;
  String? _error;

  @override
  void initState() {
    super.initState();
    final client = context.read<AuthProvider>().apiClient;
    HealthService(client).getHealth().then((h) {
      if (mounted) setState(() => _health = h);
    }).catchError((e) {
      if (mounted) setState(() => _error = e is ApiException ? e.message : "Backend'e ulaşılamadı.");
    });
  }

  @override
  Widget build(BuildContext context) {
    final auth = context.watch<AuthProvider>();
    final session = auth.session;

    return Scaffold(
      appBar: AppBar(
        title: const Text('Genel Bakış'),
        actions: [
          TextButton(onPressed: () => context.read<AuthProvider>().logout(), child: const Text('Çıkış Yap')),
        ],
      ),
      body: ListView(
        padding: const EdgeInsets.all(20),
        children: [
          Text(
            'Hoş geldin, ${session?.displayName ?? ''} (${session?.role ?? ''})',
            style: const TextStyle(color: mutedColor, fontSize: 15),
          ),
          const SizedBox(height: 20),
          Card(
            child: Padding(
              padding: const EdgeInsets.all(18),
              child: Row(
                children: [
                  Container(
                    width: 42,
                    height: 42,
                    decoration: BoxDecoration(
                      color: _error != null ? dangerBg : (_health != null ? successBg : Colors.grey.shade200),
                      borderRadius: BorderRadius.circular(12),
                    ),
                    child: Icon(
                      _error != null ? Icons.error_outline : Icons.check_circle_outline,
                      color: _error != null ? dangerColor : successColor,
                    ),
                  ),
                  const SizedBox(width: 14),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const Text('Backend Durumu', style: TextStyle(color: mutedColor, fontSize: 13)),
                        const SizedBox(height: 2),
                        if (_error != null)
                          Text(_error!, style: const TextStyle(color: dangerColor, fontWeight: FontWeight.w600)),
                        if (_health != null)
                          Text(_health!.status.toUpperCase(), style: const TextStyle(fontWeight: FontWeight.w700, fontSize: 16)),
                        if (_health == null && _error == null) const Text('Kontrol ediliyor...'),
                      ],
                    ),
                  ),
                ],
              ),
            ),
          ),
          const SizedBox(height: 20),
          InkWell(
            borderRadius: BorderRadius.circular(14),
            onTap: () => Navigator.of(context).push(MaterialPageRoute(builder: (_) => const ProductsScreen())),
            child: Card(
              child: Padding(
                padding: const EdgeInsets.all(18),
                child: Row(
                  children: [
                    Container(
                      width: 40,
                      height: 40,
                      decoration: BoxDecoration(color: infoBg, borderRadius: BorderRadius.circular(10)),
                      child: const Icon(Icons.inventory_2_outlined, color: primaryColor),
                    ),
                    const SizedBox(width: 14),
                    const Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text('Ürünler', style: TextStyle(fontWeight: FontWeight.w700, fontSize: 15)),
                          Text('Ürün, stok ve fiyat yönetimi', style: TextStyle(color: mutedColor, fontSize: 13)),
                        ],
                      ),
                    ),
                    const Icon(Icons.chevron_right, color: mutedColor),
                  ],
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}
