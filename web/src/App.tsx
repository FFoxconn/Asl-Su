import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { ThemeProvider } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { theme } from './theme';
import { AuthProvider } from './auth/AuthContext';
import { ProtectedRoute } from './auth/ProtectedRoute';
import { AppLayout } from './components/AppLayout';
import { LoginPage } from './pages/Login/LoginPage';
import { DashboardPage } from './pages/Dashboard/DashboardPage';
import { ProductsPage } from './pages/Products/ProductsPage';
import { ApiSettingsPage } from './pages/ApiSettings/ApiSettingsPage';
import { OrdersPage } from './pages/Orders/OrdersPage';
import { CouriersPage } from './pages/Couriers/CouriersPage';
import { CourierDetailPage } from './pages/Couriers/CourierDetailPage';
import { CourierMapPage } from './pages/CourierMap/CourierMapPage';
import { CourierLeaderboardPage } from './pages/CourierLeaderboard/CourierLeaderboardPage';

function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <AuthProvider>
        <BrowserRouter>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route
              path="/"
              element={
                <ProtectedRoute>
                  <AppLayout>
                    <DashboardPage />
                  </AppLayout>
                </ProtectedRoute>
              }
            />
            <Route
              path="/products"
              element={
                <ProtectedRoute>
                  <AppLayout>
                    <ProductsPage />
                  </AppLayout>
                </ProtectedRoute>
              }
            />
            <Route
              path="/api-settings"
              element={
                <ProtectedRoute>
                  <AppLayout>
                    <ApiSettingsPage />
                  </AppLayout>
                </ProtectedRoute>
              }
            />
            <Route
              path="/orders"
              element={
                <ProtectedRoute>
                  <AppLayout>
                    <OrdersPage />
                  </AppLayout>
                </ProtectedRoute>
              }
            />
            <Route
              path="/couriers"
              element={
                <ProtectedRoute>
                  <AppLayout>
                    <CouriersPage />
                  </AppLayout>
                </ProtectedRoute>
              }
            />
            <Route
              path="/couriers/:id"
              element={
                <ProtectedRoute>
                  <AppLayout>
                    <CourierDetailPage />
                  </AppLayout>
                </ProtectedRoute>
              }
            />
            <Route
              path="/courier-map"
              element={
                <ProtectedRoute>
                  <AppLayout>
                    <CourierMapPage />
                  </AppLayout>
                </ProtectedRoute>
              }
            />
            <Route
              path="/courier-leaderboard"
              element={
                <ProtectedRoute>
                  <AppLayout>
                    <CourierLeaderboardPage />
                  </AppLayout>
                </ProtectedRoute>
              }
            />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </BrowserRouter>
      </AuthProvider>
    </ThemeProvider>
  );
}

export default App;
