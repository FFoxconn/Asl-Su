import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../models/product.dart';
import '../providers/auth_provider.dart';
import '../services/api_client.dart';
import '../services/product_service.dart';
import '../theme.dart';

class ProductsScreen extends StatefulWidget {
  const ProductsScreen({super.key});

  @override
  State<ProductsScreen> createState() => _ProductsScreenState();
}

class _ProductsScreenState extends State<ProductsScreen> {
  List<Product> _products = [];
  bool _isLoading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() {
      _isLoading = true;
      _error = null;
    });
    final client = context.read<AuthProvider>().apiClient;
    try {
      final products = await ProductService(client).getProducts();
      if (mounted) setState(() => _products = products);
    } catch (e) {
      if (mounted) setState(() => _error = e is ApiException ? e.message : "Backend'e ulaşılamadı.");
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Ürünler')),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : _error != null
              ? Center(child: Padding(padding: const EdgeInsets.all(24), child: Text(_error!, style: const TextStyle(color: dangerColor))))
              : RefreshIndicator(
                  onRefresh: _load,
                  child: _products.isEmpty
                      ? ListView(
                          children: const [
                            SizedBox(height: 80),
                            Center(child: Text('Henüz ürün eklenmemiş.', style: TextStyle(color: mutedColor))),
                          ],
                        )
                      : ListView.separated(
                          padding: const EdgeInsets.all(16),
                          itemCount: _products.length,
                          separatorBuilder: (context, index) => const SizedBox(height: 10),
                          itemBuilder: (context, index) {
                            final p = _products[index];
                            return Card(
                              child: Padding(
                                padding: const EdgeInsets.all(14),
                                child: Column(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    Text(p.name, style: const TextStyle(fontWeight: FontWeight.w700, fontSize: 15)),
                                    const SizedBox(height: 4),
                                    Text('SKU: ${p.sku} · Barkod: ${p.barcode}', style: const TextStyle(color: mutedColor, fontSize: 12.5)),
                                    const SizedBox(height: 6),
                                    Row(
                                      children: [
                                        _Chip(label: p.isActive ? 'Aktif' : 'Pasif', color: p.isActive ? successColor : mutedColor, bg: p.isActive ? successBg : Colors.grey.shade200),
                                        const SizedBox(width: 6),
                                        _Chip(label: p.tgoSyncStatus, color: primaryColor, bg: infoBg),
                                      ],
                                    ),
                                  ],
                                ),
                              ),
                            );
                          },
                        ),
                ),
    );
  }
}

class _Chip extends StatelessWidget {
  final String label;
  final Color color;
  final Color bg;
  const _Chip({required this.label, required this.color, required this.bg});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 9, vertical: 4),
      decoration: BoxDecoration(color: bg, borderRadius: BorderRadius.circular(999)),
      child: Text(label, style: TextStyle(color: color, fontSize: 11.5, fontWeight: FontWeight.w600)),
    );
  }
}
