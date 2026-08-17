import '../models/order.dart';
import 'api_client.dart';

class OrderService {
  final ApiClient _client;
  OrderService(this._client);

  Future<List<OrderListItem>> getMyOrders() async {
    final json = await _client.get('/api/orders/my') as List<dynamic>;
    return json.map((e) => OrderListItem.fromJson(e as Map<String, dynamic>)).toList();
  }

  Future<DeliverResult> deliverOrder(int orderId) async {
    final json = await _client.post('/api/orders/$orderId/deliver');
    return DeliverResult.fromJson(json as Map<String, dynamic>);
  }
}
