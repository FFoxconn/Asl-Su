import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../models/order.dart';
import '../providers/auth_provider.dart';
import '../services/api_client.dart';
import '../services/order_service.dart';
import '../theme.dart';
import 'order_detail_screen.dart';

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

class CourierHomeScreen extends StatefulWidget {
  final VoidCallback onViewAllOrders;
  const CourierHomeScreen({super.key, required this.onViewAllOrders});

  @override
  State<CourierHomeScreen> createState() => _CourierHomeScreenState();
}

class _CourierHomeScreenState extends State<CourierHomeScreen> {
  List<OrderListItem> _orders = [];
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
      final orders = await OrderService(client).getMyOrders();
      if (mounted) setState(() => _orders = orders);
    } catch (e) {
      if (mounted) setState(() => _error = e is ApiException ? e.message : 'Veriler yüklenemedi.');
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final session = context.watch<AuthProvider>().session;
    final name = session?.displayName ?? '';
    final initial = name.isNotEmpty ? name.substring(0, 1).toUpperCase() : '?';
    final now = DateTime.now();

    final active = _orders.where((o) => o.isActive).toList()
      ..sort((a, b) => b.orderDate.compareTo(a.orderDate));
    final todayOrders = _orders.where((o) => isSameDay(o.orderDate, now)).toList();
    final deliveredToday = _orders.where((o) => o.workflowStatus == 'Delivered' && isSameDay(o.updatedAt, now)).length;
    final cancelledToday = _orders.where((o) => o.status == 'Cancelled' && isSameDay(o.updatedAt, now)).length;
    final revenueToday = todayOrders.fold<double>(0, (sum, o) => sum + (o.invoiceAmount ?? 0));
    final finishedToday = deliveredToday + cancelledToday;
    final successRate = finishedToday == 0 ? null : deliveredToday / finishedToday;

    return Scaffold(
      appBar: AppBar(
        automaticallyImplyLeading: false,
        titleSpacing: 20,
        title: Row(
          children: [
            CircleAvatar(
              radius: 20,
              backgroundColor: primaryColor,
              child: Text(initial, style: const TextStyle(color: Colors.white, fontWeight: FontWeight.w700)),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                mainAxisSize: MainAxisSize.min,
                children: [
                  Text('Merhaba, $name 👋', style: const TextStyle(fontSize: 15, fontWeight: FontWeight.w700), overflow: TextOverflow.ellipsis),
                  const Text('Bugün iyi çalışmalar', style: TextStyle(fontSize: 12, color: mutedColor)),
                ],
              ),
            ),
          ],
        ),
        actions: [
          IconButton(
            tooltip: 'Çıkış Yap',
            onPressed: () => context.read<AuthProvider>().logout(),
            icon: const Icon(Icons.logout_outlined),
          ),
        ],
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : _error != null
              ? _ErrorState(message: _error!, onRetry: _load)
              : RefreshIndicator(
                  onRefresh: _load,
                  child: ListView(
                    padding: const EdgeInsets.only(bottom: 24),
                    children: [
                      const Padding(
                        padding: EdgeInsets.fromLTRB(20, 16, 20, 8),
                        child: Text('Bugün', style: TextStyle(fontSize: 16, fontWeight: FontWeight.w700)),
                      ),
                      SizedBox(
                        height: 92,
                        child: ListView(
                          scrollDirection: Axis.horizontal,
                          padding: const EdgeInsets.symmetric(horizontal: 20),
                          children: [
                            _StatCard(icon: Icons.local_shipping_outlined, label: 'Aktif Sipariş', value: '${active.length}', color: primaryColor, bg: infoBg),
                            const SizedBox(width: 10),
                            _StatCard(icon: Icons.inventory_2_outlined, label: 'Bugünkü Sipariş', value: '${todayOrders.length}', color: primaryColor, bg: infoBg),
                            const SizedBox(width: 10),
                            _StatCard(icon: Icons.check_circle_outline, label: 'Teslim Edilen', value: '$deliveredToday', color: successColor, bg: successBg),
                            const SizedBox(width: 10),
                            _StatCard(icon: Icons.cancel_outlined, label: 'İptal', value: '$cancelledToday', color: dangerColor, bg: dangerBg),
                            const SizedBox(width: 10),
                            _StatCard(icon: Icons.payments_outlined, label: 'Bugünkü Ciro', value: '${revenueToday.toStringAsFixed(0)} ₺', color: primaryColor, bg: infoBg),
                          ],
                        ),
                      ),
                      const SizedBox(height: 8),
                      Padding(
                        padding: const EdgeInsets.fromLTRB(20, 12, 20, 8),
                        child: Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            Text('Aktif Siparişler${active.isNotEmpty ? ' (${active.length})' : ''}', style: const TextStyle(fontSize: 16, fontWeight: FontWeight.w700)),
                            if (active.length > 5)
                              TextButton(onPressed: widget.onViewAllOrders, child: const Text('Tümünü Gör')),
                          ],
                        ),
                      ),
                      if (active.isEmpty)
                        const Padding(
                          padding: EdgeInsets.symmetric(horizontal: 20, vertical: 24),
                          child: Center(
                            child: Text('Şu anda aktif siparişin yok. 🎉', style: TextStyle(color: mutedColor)),
                          ),
                        )
                      else
                        ...active.take(5).map((order) => Padding(
                              padding: const EdgeInsets.fromLTRB(20, 0, 20, 10),
                              child: _ActiveOrderCard(order: order),
                            )),
                      const SizedBox(height: 8),
                      Padding(
                        padding: const EdgeInsets.fromLTRB(20, 12, 20, 8),
                        child: Text('Bugünkü Performans', style: const TextStyle(fontSize: 16, fontWeight: FontWeight.w700)),
                      ),
                      Padding(
                        padding: const EdgeInsets.symmetric(horizontal: 20),
                        child: Card(
                          child: Padding(
                            padding: const EdgeInsets.all(16),
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Row(
                                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                  children: [
                                    const Text('Teslimat Başarı Oranı', style: TextStyle(color: mutedColor, fontSize: 13)),
                                    Text(
                                      successRate != null ? '%${(successRate * 100).round()}' : '-',
                                      style: const TextStyle(fontWeight: FontWeight.w700),
                                    ),
                                  ],
                                ),
                                const SizedBox(height: 8),
                                ClipRRect(
                                  borderRadius: BorderRadius.circular(999),
                                  child: LinearProgressIndicator(
                                    value: successRate ?? 0,
                                    minHeight: 8,
                                    backgroundColor: const Color(0xFFEEF1F4),
                                    color: successColor,
                                  ),
                                ),
                                const SizedBox(height: 14),
                                Row(
                                  children: [
                                    Expanded(
                                      child: _PerformanceStat(label: 'Teslim Edilen', value: '$deliveredToday sipariş'),
                                    ),
                                    Expanded(
                                      child: _PerformanceStat(label: 'Toplam Ciro', value: '${revenueToday.toStringAsFixed(2)} ₺'),
                                    ),
                                  ],
                                ),
                              ],
                            ),
                          ),
                        ),
                      ),
                    ],
                  ),
                ),
    );
  }
}

class _ActiveOrderCard extends StatelessWidget {
  final OrderListItem order;
  const _ActiveOrderCard({required this.order});

  @override
  Widget build(BuildContext context) {
    final color = _workflowColor[order.workflowStatus] ?? mutedColor;
    return Card(
      child: InkWell(
        borderRadius: BorderRadius.circular(14),
        onTap: () => Navigator.of(context).push(MaterialPageRoute(builder: (_) => OrderDetailScreen(orderId: order.id))),
        child: Padding(
          padding: const EdgeInsets.all(14),
          child: Row(
            children: [
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        Expanded(
                          child: Text(order.orderNumber, style: const TextStyle(fontWeight: FontWeight.w700, fontSize: 14.5), overflow: TextOverflow.ellipsis),
                        ),
                        Container(
                          padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                          decoration: BoxDecoration(color: color.withValues(alpha: 0.12), borderRadius: BorderRadius.circular(999)),
                          child: Text(
                            _workflowLabel[order.workflowStatus] ?? order.workflowStatus,
                            style: TextStyle(color: color, fontSize: 11, fontWeight: FontWeight.w700),
                          ),
                        ),
                      ],
                    ),
                    if (order.customerName != null) ...[
                      const SizedBox(height: 4),
                      Text(order.customerName!, style: const TextStyle(color: mutedColor, fontSize: 13), overflow: TextOverflow.ellipsis),
                    ],
                    const SizedBox(height: 4),
                    Text(
                      order.invoiceAmount != null ? '${order.invoiceAmount!.toStringAsFixed(2)} ₺' : '-',
                      style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 13.5),
                    ),
                  ],
                ),
              ),
              const SizedBox(width: 6),
              const Icon(Icons.chevron_right, color: mutedColor),
            ],
          ),
        ),
      ),
    );
  }
}

class _StatCard extends StatelessWidget {
  final IconData icon;
  final String label;
  final String value;
  final Color color;
  final Color bg;

  const _StatCard({required this.icon, required this.label, required this.value, required this.color, required this.bg});

  @override
  Widget build(BuildContext context) {
    return Container(
      width: 132,
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: surfaceColor,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: borderColor),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Container(
            width: 30,
            height: 30,
            decoration: BoxDecoration(color: bg, borderRadius: BorderRadius.circular(8)),
            child: Icon(icon, color: color, size: 17),
          ),
          const Spacer(),
          Text(value, style: const TextStyle(fontWeight: FontWeight.w700, fontSize: 16), maxLines: 1, overflow: TextOverflow.ellipsis),
          Text(label, style: const TextStyle(color: mutedColor, fontSize: 11), maxLines: 1, overflow: TextOverflow.ellipsis),
        ],
      ),
    );
  }
}

class _PerformanceStat extends StatelessWidget {
  final String label;
  final String value;
  const _PerformanceStat({required this.label, required this.value});

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(label, style: const TextStyle(color: mutedColor, fontSize: 12)),
        const SizedBox(height: 2),
        Text(value, style: const TextStyle(fontWeight: FontWeight.w700, fontSize: 14)),
      ],
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
