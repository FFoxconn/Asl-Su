import { useCallback, useEffect, useState } from 'react';
import { FlatList, RefreshControl, StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { useAuth } from '../../auth/AuthContext';
import { deliverOrder, getMyOrders } from '../../api/orders';
import { ApiError } from '../../api/client';
import type { OrderListItem } from '../../types/order';

const WORKFLOW_LABEL: Record<string, string> = {
  New: 'Yeni',
  Accepted: 'Kabul Edildi',
  Preparing: 'Hazırlanıyor',
  Prepared: 'Hazırlandı',
  Delivered: 'Teslim Edildi',
};

export function MyDeliveriesScreen() {
  const { session, logout } = useAuth();
  const [orders, setOrders] = useState<OrderListItem[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [deliveringId, setDeliveringId] = useState<number | null>(null);

  const refresh = useCallback(() => {
    setError(null);
    return getMyOrders()
      .then(setOrders)
      .catch((err) => setError(err instanceof ApiError ? err.message : 'Siparişler yüklenemedi.'))
      .finally(() => setIsLoading(false));
  }, []);

  useEffect(() => {
    refresh();
  }, [refresh]);

  async function handleDeliver(orderId: number) {
    setDeliveringId(orderId);
    setError(null);
    try {
      await deliverOrder(orderId);
      await refresh();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Teslimat işaretlenemedi.');
    } finally {
      setDeliveringId(null);
    }
  }

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <Text style={styles.title}>Teslimatlarım</Text>
        <TouchableOpacity onPress={logout}>
          <Text style={styles.logout}>Çıkış Yap</Text>
        </TouchableOpacity>
      </View>
      <Text style={styles.welcome}>Hoş geldin, {session?.displayName}</Text>

      {error && <Text style={styles.error}>{error}</Text>}

      <FlatList
        data={orders}
        keyExtractor={(item) => String(item.id)}
        refreshControl={<RefreshControl refreshing={isLoading} onRefresh={refresh} />}
        contentContainerStyle={orders.length === 0 ? styles.emptyContainer : undefined}
        ListEmptyComponent={!isLoading ? <Text style={styles.empty}>Şu anda atanmış siparişin yok.</Text> : null}
        renderItem={({ item }) => (
          <View style={styles.card}>
            <View style={styles.cardHeader}>
              <Text style={styles.orderNumber}>{item.orderNumber}</Text>
              <Text style={styles.status}>{WORKFLOW_LABEL[item.workflowStatus] ?? item.workflowStatus}</Text>
            </View>
            {item.customerName && <Text style={styles.customer}>{item.customerName}</Text>}
            <Text style={styles.amount}>{item.invoiceAmount != null ? `${item.invoiceAmount.toFixed(2)} ₺` : '-'}</Text>
            {item.workflowStatus === 'Prepared' && (
              <TouchableOpacity
                style={styles.deliverButton}
                onPress={() => handleDeliver(item.id)}
                disabled={deliveringId === item.id}
              >
                <Text style={styles.deliverButtonText}>
                  {deliveringId === item.id ? 'İşleniyor...' : 'Teslim Et'}
                </Text>
              </TouchableOpacity>
            )}
          </View>
        )}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, padding: 24, backgroundColor: '#fff' },
  header: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: 8 },
  title: { fontSize: 24, fontWeight: 'bold' },
  logout: { color: '#0066cc' },
  welcome: { marginBottom: 16, color: '#555' },
  error: { color: 'crimson', marginBottom: 12 },
  empty: { color: '#777', textAlign: 'center', marginTop: 40 },
  emptyContainer: { flexGrow: 1 },
  card: {
    borderWidth: 1,
    borderColor: '#e2e5e9',
    borderRadius: 10,
    padding: 14,
    marginBottom: 12,
  },
  cardHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center' },
  orderNumber: { fontWeight: '700', fontSize: 15 },
  status: { fontSize: 12, color: '#0b6e99', fontWeight: '600' },
  customer: { marginTop: 4, color: '#555' },
  amount: { marginTop: 4, fontWeight: '600' },
  deliverButton: {
    marginTop: 10,
    backgroundColor: '#0b6e99',
    borderRadius: 8,
    paddingVertical: 10,
    alignItems: 'center',
  },
  deliverButtonText: { color: '#fff', fontWeight: '600' },
});
