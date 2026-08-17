import '../models/product.dart';
import 'api_client.dart';

class ProductService {
  final ApiClient _client;
  ProductService(this._client);

  Future<List<Product>> getProducts() async {
    final json = await _client.get('/api/products') as List<dynamic>;
    return json.map((e) => Product.fromJson(e as Map<String, dynamic>)).toList();
  }
}
