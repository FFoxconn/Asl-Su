import { useCallback, useState } from 'react';
import { useFocusEffect } from '@react-navigation/native';
import { ActivityIndicator, FlatList, StyleSheet, Text, View } from 'react-native';
import { getProducts } from '../../api/products';
import type { Product } from '../../types/product';
import { ApiError } from '../../api/client';

export function ProductsScreen() {
  const [products, setProducts] = useState<Product[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useFocusEffect(
    useCallback(() => {
      setIsLoading(true);
      getProducts()
        .then(setProducts)
        .catch((err) => setError(err instanceof ApiError ? err.message : "Backend'e ulaşılamadı."))
        .finally(() => setIsLoading(false));
    }, []),
  );

  if (isLoading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator />
      </View>
    );
  }

  if (error) {
    return (
      <View style={styles.center}>
        <Text style={styles.error}>{error}</Text>
      </View>
    );
  }

  return (
    <FlatList
      style={styles.container}
      data={products}
      keyExtractor={(item) => String(item.id)}
      ListEmptyComponent={<Text style={styles.empty}>Henüz ürün eklenmemiş.</Text>}
      renderItem={({ item }) => (
        <View style={styles.row}>
          <Text style={styles.name}>{item.name}</Text>
          <Text style={styles.meta}>
            SKU: {item.sku} · Barkod: {item.barcode}
          </Text>
          <Text style={styles.meta}>
            {item.isActive ? 'Aktif' : 'Pasif'} · Trendyol Go: {item.tgoSyncStatus}
          </Text>
        </View>
      )}
    />
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#fff' },
  center: { flex: 1, justifyContent: 'center', alignItems: 'center' },
  error: { color: 'crimson', padding: 24, textAlign: 'center' },
  empty: { padding: 24, textAlign: 'center', color: '#666' },
  row: { padding: 16, borderBottomWidth: 1, borderBottomColor: '#eee' },
  name: { fontSize: 16, fontWeight: '600' },
  meta: { fontSize: 13, color: '#666', marginTop: 2 },
});
