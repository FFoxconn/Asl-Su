import { ActivityIndicator, View } from 'react-native';
import { NavigationContainer } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { useAuth } from '../auth/AuthContext';
import { LoginScreen } from '../screens/Login/LoginScreen';
import { DashboardScreen } from '../screens/Dashboard/DashboardScreen';
import { ProductsScreen } from '../screens/Products/ProductsScreen';
import { MyDeliveriesScreen } from '../screens/MyDeliveries/MyDeliveriesScreen';

const Stack = createNativeStackNavigator();

export function RootNavigator() {
  const { session, isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return (
      <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center' }}>
        <ActivityIndicator />
      </View>
    );
  }

  return (
    <NavigationContainer>
      <Stack.Navigator>
        {isAuthenticated && session?.role === 'Courier' ? (
          <Stack.Screen name="MyDeliveries" component={MyDeliveriesScreen} options={{ headerShown: false }} />
        ) : isAuthenticated ? (
          <>
            <Stack.Screen name="Dashboard" component={DashboardScreen} options={{ headerShown: false }} />
            <Stack.Screen name="Products" component={ProductsScreen} options={{ title: 'Ürünler' }} />
          </>
        ) : (
          <Stack.Screen name="Login" component={LoginScreen} options={{ headerShown: false }} />
        )}
      </Stack.Navigator>
    </NavigationContainer>
  );
}
