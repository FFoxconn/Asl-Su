class OrderListItem {
  final int id;
  final String packageId;
  final String orderNumber;
  final int storeId;
  final String status;
  final String workflowStatus;
  final DateTime orderDate;
  final double? invoiceAmount;
  final String? customerName;
  final int? courierId;
  final String? courierName;
  final DateTime updatedAt;

  OrderListItem({
    required this.id,
    required this.packageId,
    required this.orderNumber,
    required this.storeId,
    required this.status,
    required this.workflowStatus,
    required this.orderDate,
    required this.invoiceAmount,
    required this.customerName,
    required this.courierId,
    required this.courierName,
    required this.updatedAt,
  });

  bool get isActive => workflowStatus != 'Delivered' && status != 'Cancelled' && status != 'Returned';

  factory OrderListItem.fromJson(Map<String, dynamic> json) => OrderListItem(
        id: json['id'] as int,
        packageId: json['packageId'] as String,
        orderNumber: json['orderNumber'] as String,
        storeId: json['storeId'] as int,
        status: json['status'] as String,
        workflowStatus: json['workflowStatus'] as String,
        orderDate: DateTime.parse(json['orderDate'] as String).toLocal(),
        invoiceAmount: (json['invoiceAmount'] as num?)?.toDouble(),
        customerName: json['customerName'] as String?,
        courierId: json['courierId'] as int?,
        courierName: json['courierName'] as String?,
        updatedAt: DateTime.parse(json['updatedAt'] as String).toLocal(),
      );
}

class DeliverResult {
  final bool success;
  final String? workflowStatus;
  final bool trendyolNotified;
  final String? trendyolMessage;

  DeliverResult({
    required this.success,
    required this.workflowStatus,
    required this.trendyolNotified,
    required this.trendyolMessage,
  });

  factory DeliverResult.fromJson(Map<String, dynamic> json) => DeliverResult(
        success: json['success'] as bool,
        workflowStatus: json['workflowStatus'] as String?,
        trendyolNotified: json['trendyolNotified'] as bool,
        trendyolMessage: json['trendyolMessage'] as String?,
      );
}

bool isSameDay(DateTime a, DateTime b) => a.year == b.year && a.month == b.month && a.day == b.day;
