class Product {
  final int id;
  final String sku;
  final String barcode;
  final String name;
  final String? description;
  final int? categoryId;
  final int? brandId;
  final double vatRate;
  final String? imageUrl;
  final bool isActive;
  final String tgoSyncStatus;

  Product({
    required this.id,
    required this.sku,
    required this.barcode,
    required this.name,
    required this.description,
    required this.categoryId,
    required this.brandId,
    required this.vatRate,
    required this.imageUrl,
    required this.isActive,
    required this.tgoSyncStatus,
  });

  factory Product.fromJson(Map<String, dynamic> json) => Product(
        id: json['id'] as int,
        sku: json['sku'] as String,
        barcode: json['barcode'] as String,
        name: json['name'] as String,
        description: json['description'] as String?,
        categoryId: json['categoryId'] as int?,
        brandId: json['brandId'] as int?,
        vatRate: (json['vatRate'] as num).toDouble(),
        imageUrl: json['imageUrl'] as String?,
        isActive: json['isActive'] as bool,
        tgoSyncStatus: json['tgoSyncStatus'] as String,
      );
}
