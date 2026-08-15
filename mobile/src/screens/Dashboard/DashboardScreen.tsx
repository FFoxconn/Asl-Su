import { useEffect, useState } from 'react';
import { StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { useNavigation } from '@react-navigation/native';
import { useAuth } from '../../auth/AuthContext';
import { getHealth, type HealthStatus } from '../../api/auth';
import { ApiError } from '../../api/client';

export function DashboardScreen() {
  const { session, logout } = useAuth();
  const navigation = useNavigation();
  const [health, setHealth] = useState<HealthStatus | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    getHealth()
      .then(setHealth)
      .catch((err) => setError(err instanceof ApiError ? err.message : "Backend'e ulaşılamadı."));
  }, []);

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <Text style={styles.title}>Genel Bakış</Text>
        <TouchableOpacity onPress={logout}>
          <Text style={styles.logout}>Çıkış Yap</Text>
        </TouchableOpacity>
      </View>
      <Text style={styles.welcome}>
        Hoş geldin, {session?.displayName} ({session?.role})
      </Text>
      <Text style={styles.sectionTitle}>Backend Durumu</Text>
      {error && <Text style={styles.error}>{error}</Text>}
      {health && (
        <Text>
          Durum: {health.status} — {new Date(health.timeUtc).toLocaleString()}
        </Text>
      )}
      {!health && !error && <Text>Kontrol ediliyor...</Text>}
      <TouchableOpacity style={styles.link} onPress={() => navigation.navigate('Products' as never)}>
        <Text style={styles.linkText}>Ürünler →</Text>
      </TouchableOpacity>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, padding: 24, backgroundColor: '#fff' },
  header: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 },
  title: { fontSize: 24, fontWeight: 'bold' },
  logout: { color: '#0066cc' },
  welcome: { marginBottom: 24 },
  sectionTitle: { fontSize: 18, fontWeight: '600', marginBottom: 8 },
  error: { color: 'crimson' },
  link: { marginTop: 24 },
  linkText: { color: '#0066cc', fontSize: 16 },
});
