class OrderItem {
  final int id;
  final int? productId;
  final String barcode;
  final int quantity;
  final double unitPrice;
  final bool isSubstitution;
  final String? substitutedForBarcode;

  OrderItem({
    required this.id,
    required this.productId,
    required this.barcode,
    required this.quantity,
    required this.unitPrice,
    required this.isSubstitution,
    required this.substitutedForBarcode,
  });

  factory OrderItem.fromJson(Map<String, dynamic> json) => OrderItem(
        id: json['id'] as int,
        productId: json['productId'] as int?,
        barcode: json['barcode'] as String,
        quantity: json['quantity'] as int,
        unitPrice: (json['unitPrice'] as num).toDouble(),
        isSubstitution: json['isSubstitution'] as bool,
        substitutedForBarcode: json['substitutedForBarcode'] as String?,
      );
}

class OrderDetail {
  final int id;
  final String packageId;
  final String orderNumber;
  final int storeId;
  final String status;
  final String workflowStatus;
  final DateTime orderDate;
  final double? invoiceAmount;
  final double? invoiceTaxAmount;
  final int? bagCount;
  final String? receiptLink;
  final String? customerName;
  final String? customerPhone;
  final String? customerAddress;
  final int? courierId;
  final String? courierName;
  final List<OrderItem> items;

  OrderDetail({
    required this.id,
    required this.packageId,
    required this.orderNumber,
    required this.storeId,
    required this.status,
    required this.workflowStatus,
    required this.orderDate,
    required this.invoiceAmount,
    required this.invoiceTaxAmount,
    required this.bagCount,
    required this.receiptLink,
    required this.customerName,
    required this.customerPhone,
    required this.customerAddress,
    required this.courierId,
    required this.courierName,
    required this.items,
  });

  factory OrderDetail.fromJson(Map<String, dynamic> json) => OrderDetail(
        id: json['id'] as int,
        packageId: json['packageId'] as String,
        orderNumber: json['orderNumber'] as String,
        storeId: json['storeId'] as int,
        status: json['status'] as String,
        workflowStatus: json['workflowStatus'] as String,
        orderDate: DateTime.parse(json['orderDate'] as String).toLocal(),
        invoiceAmount: (json['invoiceAmount'] as num?)?.toDouble(),
        invoiceTaxAmount: (json['invoiceTaxAmount'] as num?)?.toDouble(),
        bagCount: json['bagCount'] as int?,
        receiptLink: json['receiptLink'] as String?,
        customerName: json['customerName'] as String?,
        customerPhone: json['customerPhone'] as String?,
        customerAddress: json['customerAddress'] as String?,
        courierId: json['courierId'] as int?,
        courierName: json['courierName'] as String?,
        items: (json['items'] as List<dynamic>)
            .map((e) => OrderItem.fromJson(e as Map<String, dynamic>))
            .toList(),
      );
}
