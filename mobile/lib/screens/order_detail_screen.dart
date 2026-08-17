import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:url_launcher/url_launcher.dart';

import '../models/order_detail.dart';
import '../providers/auth_provider.dart';
import '../services/api_client.dart';
import '../services/order_service.dart';
import '../theme.dart';

const _workflowLabel = {
  'New': 'Yeni',
  'Accepted': 'Kabul Edildi',
  'Preparing': 'Hazırlanıyor',
  'Prepared': 'Hazırlandı',
  'Delivered': 'Teslim Edildi',
};

const _workflowColor = {
  'New': mutedColor,
  'Accepted': primaryColor,
  'Preparing': warningColor,
  'Prepared': primaryColor,
  'Delivered': successColor,
};

class OrderDetailScreen extends StatefulWidget {
  final int orderId;
  const OrderDetailScreen({super.key, required this.orderId});

  @override
  State<OrderDetailScreen> createState() => _OrderDetailScreenState();
}

class _OrderDetailScreenState extends State<OrderDetailScreen> {
  OrderDetail? _order;
  bool _isLoading = true;
  bool _isDelivering = false;
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
      final order = await OrderService(client).getOrder(widget.orderId);
      if (mounted) setState(() => _order = order);
    } catch (e) {
      if (mounted) setState(() => _error = e is ApiException ? e.message : 'Sipariş yüklenemedi.');
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  Future<void> _deliver() async {
    setState(() => _isDelivering = true);
    final client = context.read<AuthProvider>().apiClient;
    try {
      await OrderService(client).deliverOrder(widget.orderId);
      await _load();
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Sipariş teslim edildi olarak işaretlendi.'), backgroundColor: successColor),
        );
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(e is ApiException ? e.message : 'Teslimat işaretlenemedi.'), backgroundColor: dangerColor),
        );
      }
    } finally {
      if (mounted) setState(() => _isDelivering = false);
    }
  }

  Future<void> _call(String phone) async {
    final uri = Uri(scheme: 'tel', path: phone);
    if (await canLaunchUrl(uri)) {
      await launchUrl(uri);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text(_order?.orderNumber ?? 'Sipariş')),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : _error != null
              ? _ErrorState(message: _error!, onRetry: _load)
              : _buildContent(),
    );
  }

  Widget _buildContent() {
    final order = _order!;
    final canDeliver = order.workflowStatus == 'Prepared';

    return ListView(
      padding: const EdgeInsets.all(20),
      children: [
        Container(
          padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
          decoration: BoxDecoration(
            color: (_workflowColor[order.workflowStatus] ?? mutedColor).withValues(alpha: 0.12),
            borderRadius: BorderRadius.circular(999),
          ),
          child: Text(
            _workflowLabel[order.workflowStatus] ?? order.workflowStatus,
            style: TextStyle(color: _workflowColor[order.workflowStatus] ?? mutedColor, fontWeight: FontWeight.w700, fontSize: 13),
          ),
        ),
        const SizedBox(height: 18),
        Card(
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Text('Müşteri', style: TextStyle(color: mutedColor, fontSize: 12.5, fontWeight: FontWeight.w600)),
                const SizedBox(height: 6),
                Text(order.customerName ?? '-', style: const TextStyle(fontWeight: FontWeight.w700, fontSize: 16)),
                if (order.customerAddress != null) ...[
                  const SizedBox(height: 8),
                  Row(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const Icon(Icons.location_on_outlined, size: 18, color: mutedColor),
                      const SizedBox(width: 6),
                      Expanded(child: Text(order.customerAddress!, style: const TextStyle(color: textColor))),
                    ],
                  ),
                ],
                if (order.customerPhone != null) ...[
                  const SizedBox(height: 14),
                  SizedBox(
                    width: double.infinity,
                    child: OutlinedButton.icon(
                      onPressed: () => _call(order.customerPhone!),
                      icon: const Icon(Icons.call_outlined),
                      label: Text('Ara: ${order.customerPhone}'),
                    ),
                  ),
                ],
              ],
            ),
          ),
        ),
        const SizedBox(height: 14),
        Card(
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Text('Sipariş Bilgisi', style: TextStyle(color: mutedColor, fontSize: 12.5, fontWeight: FontWeight.w600)),
                const SizedBox(height: 10),
                _InfoRow(label: 'Sipariş No', value: order.orderNumber),
                _InfoRow(label: 'Paket ID', value: order.packageId),
                _InfoRow(
                  label: 'Tarih',
                  value: '${order.orderDate.day.toString().padLeft(2, '0')}.${order.orderDate.month.toString().padLeft(2, '0')}.${order.orderDate.year} ${order.orderDate.hour.toString().padLeft(2, '0')}:${order.orderDate.minute.toString().padLeft(2, '0')}',
                ),
                _InfoRow(label: 'Tutar', value: order.invoiceAmount != null ? '${order.invoiceAmount!.toStringAsFixed(2)} ₺' : '-'),
                if (order.bagCount != null) _InfoRow(label: 'Poşet Sayısı', value: '${order.bagCount}'),
              ],
            ),
          ),
        ),
        const SizedBox(height: 14),
        Card(
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text('Ürünler (${order.items.length})', style: const TextStyle(color: mutedColor, fontSize: 12.5, fontWeight: FontWeight.w600)),
                const SizedBox(height: 8),
                ...order.items.map(
                  (item) => Padding(
                    padding: const EdgeInsets.symmetric(vertical: 6),
                    child: Row(
                      children: [
                        Expanded(
                          child: Text(
                            item.isSubstitution ? '${item.barcode} (ikame)' : item.barcode,
                            style: const TextStyle(fontSize: 13.5),
                          ),
                        ),
                        Text('${item.quantity} x ${item.unitPrice.toStringAsFixed(2)} ₺', style: const TextStyle(color: mutedColor, fontSize: 13)),
                      ],
                    ),
                  ),
                ),
                if (order.items.isEmpty) const Text('Ürün bilgisi yok.', style: TextStyle(color: mutedColor)),
              ],
            ),
          ),
        ),
        if (canDeliver) ...[
          const SizedBox(height: 22),
          SizedBox(
            width: double.infinity,
            child: ElevatedButton(
              onPressed: _isDelivering ? null : _deliver,
              child: _isDelivering
                  ? const SizedBox(width: 20, height: 20, child: CircularProgressIndicator(strokeWidth: 2.4, color: Colors.white))
                  : const Text('Teslim Et'),
            ),
          ),
        ],
        const SizedBox(height: 20),
      ],
    );
  }
}

class _InfoRow extends StatelessWidget {
  final String label;
  final String value;
  const _InfoRow({required this.label, required this.value});

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(label, style: const TextStyle(color: mutedColor, fontSize: 13.5)),
          Text(value, style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 13.5)),
        ],
      ),
    );
  }
}

class _ErrorState extends StatelessWidget {
  final String message;
  final VoidCallback onRetry;
  const _ErrorState({required this.message, required this.onRetry});

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Icon(Icons.error_outline, color: dangerColor, size: 36),
            const SizedBox(height: 10),
            Text(message, textAlign: TextAlign.center, style: const TextStyle(color: dangerColor)),
            const SizedBox(height: 16),
            OutlinedButton(onPressed: onRetry, child: const Text('Tekrar Dene')),
          ],
        ),
      ),
    );
  }
}
