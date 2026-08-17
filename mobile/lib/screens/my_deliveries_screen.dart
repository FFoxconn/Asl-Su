import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../models/order.dart';
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

class MyDeliveriesScreen extends StatefulWidget {
  const MyDeliveriesScreen({super.key});

  @override
  State<MyDeliveriesScreen> createState() => _MyDeliveriesScreenState();
}

class _MyDeliveriesScreenState extends State<MyDeliveriesScreen> {
  List<OrderListItem> _orders = [];
  bool _isLoading = true;
  String? _error;
  int? _deliveringId;

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
      if (mounted) setState(() => _error = e is ApiException ? e.message : 'Siparişler yüklenemedi.');
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  Future<void> _deliver(int orderId) async {
    setState(() {
      _deliveringId = orderId;
      _error = null;
    });
    final client = context.read<AuthProvider>().apiClient;
    try {
      await OrderService(client).deliverOrder(orderId);
      await _load();
    } catch (e) {
      if (mounted) setState(() => _error = e is ApiException ? e.message : 'Teslimat işaretlenemedi.');
    } finally {
      if (mounted) setState(() => _deliveringId = null);
    }
  }

  @override
  Widget build(BuildContext context) {
    final session = context.watch<AuthProvider>().session;
    final deliveries = _orders.where((o) => o.status != 'Returned').toList();
    final returns = _orders.where((o) => o.status == 'Returned').toList();

    return DefaultTabController(
      length: 2,
      child: Scaffold(
        appBar: AppBar(
          title: const Text('Teslimatlarım'),
          actions: [
            TextButton(onPressed: () => context.read<AuthProvider>().logout(), child: const Text('Çıkış Yap')),
          ],
          bottom: TabBar(
            tabs: [
              const Tab(text: 'Teslimatlarım'),
              Tab(text: returns.isEmpty ? 'İadeler' : 'İadeler (${returns.length})'),
            ],
          ),
        ),
        body: Column(
          children: [
            Padding(
              padding: const EdgeInsets.fromLTRB(20, 14, 20, 6),
              child: Align(
                alignment: Alignment.centerLeft,
                child: Text('Hoş geldin, ${session?.displayName ?? ''}', style: const TextStyle(color: mutedColor)),
              ),
            ),
            if (_error != null)
              Padding(
                padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 6),
                child: Container(
                  width: double.infinity,
                  padding: const EdgeInsets.all(12),
                  decoration: BoxDecoration(color: dangerBg, borderRadius: BorderRadius.circular(10)),
                  child: Text(_error!, style: const TextStyle(color: dangerColor)),
                ),
              ),
            Expanded(
              child: _isLoading
                  ? const Center(child: CircularProgressIndicator())
                  : TabBarView(
                      children: [
                        _OrderList(
                          orders: deliveries,
                          emptyText: 'Şu anda atanmış siparişin yok.',
                          onRefresh: _load,
                          deliveringId: _deliveringId,
                          onDeliver: _deliver,
                        ),
                        _OrderList(
                          orders: returns,
                          emptyText: 'Henüz bir iade yok.',
                          onRefresh: _load,
                          deliveringId: _deliveringId,
                          onDeliver: _deliver,
                        ),
                      ],
                    ),
            ),
          ],
        ),
      ),
    );
  }
}

class _OrderList extends StatelessWidget {
  final List<OrderListItem> orders;
  final String emptyText;
  final Future<void> Function() onRefresh;
  final int? deliveringId;
  final Future<void> Function(int) onDeliver;

  const _OrderList({
    required this.orders,
    required this.emptyText,
    required this.onRefresh,
    required this.deliveringId,
    required this.onDeliver,
  });

  @override
  Widget build(BuildContext context) {
    return RefreshIndicator(
      onRefresh: onRefresh,
      child: orders.isEmpty
          ? ListView(
              children: [
                const SizedBox(height: 100),
                Center(child: Text(emptyText, style: const TextStyle(color: mutedColor))),
              ],
            )
          : ListView.separated(
              padding: const EdgeInsets.fromLTRB(20, 6, 20, 20),
              itemCount: orders.length,
              separatorBuilder: (context, index) => const SizedBox(height: 12),
              itemBuilder: (context, index) {
                final order = orders[index];
                final canDeliver = order.workflowStatus == 'Prepared';
                final isDelivering = deliveringId == order.id;
                final isReturned = order.status == 'Returned';
                return Card(
                  child: Padding(
                    padding: const EdgeInsets.all(16),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            Text(order.orderNumber, style: const TextStyle(fontWeight: FontWeight.w700, fontSize: 15)),
                            if (isReturned)
                              Container(
                                padding: const EdgeInsets.symmetric(horizontal: 9, vertical: 4),
                                decoration: BoxDecoration(color: dangerBg, borderRadius: BorderRadius.circular(999)),
                                child: const Text('İade', style: TextStyle(color: dangerColor, fontSize: 11.5, fontWeight: FontWeight.w600)),
                              )
                            else
                              Text(
                                _workflowLabel[order.workflowStatus] ?? order.workflowStatus,
                                style: const TextStyle(color: primaryColor, fontWeight: FontWeight.w600, fontSize: 12.5),
                              ),
                          ],
                        ),
                        if (order.customerName != null) ...[
                          const SizedBox(height: 4),
                          Text(order.customerName!, style: const TextStyle(color: mutedColor)),
                        ],
                        const SizedBox(height: 4),
                        Text(
                          order.invoiceAmount != null ? '${order.invoiceAmount!.toStringAsFixed(2)} ₺' : '-',
                          style: const TextStyle(fontWeight: FontWeight.w600),
                        ),
                        if (canDeliver && !isReturned) ...[
                          const SizedBox(height: 12),
                          SizedBox(
                            width: double.infinity,
                            child: ElevatedButton(
                              onPressed: isDelivering ? null : () => onDeliver(order.id),
                              child: isDelivering
                                  ? const SizedBox(
                                      width: 18,
                                      height: 18,
                                      child: CircularProgressIndicator(strokeWidth: 2.2, color: Colors.white),
                                    )
                                  : const Text('Teslim Et'),
                            ),
                          ),
                        ],
                      ],
                    ),
                  ),
                );
              },
            ),
    );
  }
}
