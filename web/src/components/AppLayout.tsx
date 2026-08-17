import { useState, type ReactNode } from 'react';
import { Link, useLocation } from 'react-router-dom';
import AppBar from '@mui/material/AppBar';
import Avatar from '@mui/material/Avatar';
import Box from '@mui/material/Box';
import Divider from '@mui/material/Divider';
import Drawer from '@mui/material/Drawer';
import IconButton from '@mui/material/IconButton';
import Stack from '@mui/material/Stack';
import Toolbar from '@mui/material/Toolbar';
import Tooltip from '@mui/material/Tooltip';
import Typography from '@mui/material/Typography';
import DashboardOutlinedIcon from '@mui/icons-material/DashboardOutlined';
import Inventory2OutlinedIcon from '@mui/icons-material/Inventory2Outlined';
import ReceiptLongOutlinedIcon from '@mui/icons-material/ReceiptLongOutlined';
import SettingsOutlinedIcon from '@mui/icons-material/SettingsOutlined';
import LocalShippingOutlinedIcon from '@mui/icons-material/LocalShippingOutlined';
import MapOutlinedIcon from '@mui/icons-material/MapOutlined';
import EmojiEventsOutlinedIcon from '@mui/icons-material/EmojiEventsOutlined';
import LogoutIcon from '@mui/icons-material/Logout';
import MenuIcon from '@mui/icons-material/Menu';
import WaterDropIcon from '@mui/icons-material/WaterDrop';
import { useAuth } from '../auth/AuthContext';

const DRAWER_WIDTH = 250;
const NAVY = '#0f2237';
const NAVY_ACTIVE = '#1b3554';
const ACCENT = '#4fd1c5';

const NAV_ITEMS = [
  { to: '/', label: 'Genel Bakış', icon: <DashboardOutlinedIcon fontSize="small" /> },
  { to: '/products', label: 'Ürünler', icon: <Inventory2OutlinedIcon fontSize="small" /> },
  { to: '/orders', label: 'Siparişler', icon: <ReceiptLongOutlinedIcon fontSize="small" /> },
  { to: '/couriers', label: 'Kuryeler', icon: <LocalShippingOutlinedIcon fontSize="small" /> },
  { to: '/courier-map', label: 'Kurye Haritası', icon: <MapOutlinedIcon fontSize="small" /> },
  { to: '/courier-leaderboard', label: 'Performans Sıralaması', icon: <EmojiEventsOutlinedIcon fontSize="small" /> },
  { to: '/api-settings', label: 'API Ayarları', icon: <SettingsOutlinedIcon fontSize="small" /> },
];

function SidebarContent({ onNavigate }: { onNavigate?: () => void }) {
  const { session, logout } = useAuth();
  const location = useLocation();

  return (
    <Box sx={{ height: '100%', display: 'flex', flexDirection: 'column', bgcolor: NAVY, color: '#c7d4e6' }}>
      <Stack direction="row" spacing={1.25} alignItems="center" sx={{ px: 3, pt: 3, pb: 0.5 }}>
        <Box
          sx={{
            width: 34,
            height: 34,
            borderRadius: '10px',
            bgcolor: 'primary.main',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            flexShrink: 0,
          }}
        >
          <WaterDropIcon sx={{ color: '#fff', fontSize: 20 }} />
        </Box>
        <Typography sx={{ color: '#fff', fontWeight: 700, fontSize: '1.05rem' }}>Asl-Su</Typography>
      </Stack>
      <Typography sx={{ px: 3, pb: 2.5, fontSize: '0.72rem', color: '#7c93b5', letterSpacing: '.03em', fontWeight: 600 }}>
        YÖNETİM PANELİ
      </Typography>

      <Stack component="nav" sx={{ px: 1.5, gap: 0.5, flex: 1, overflowY: 'auto' }}>
        {NAV_ITEMS.map((item) => {
          const active = location.pathname === item.to;
          return (
            <Box
              key={item.to}
              component={Link}
              to={item.to}
              onClick={onNavigate}
              sx={{
                display: 'flex',
                alignItems: 'center',
                gap: 1.25,
                pl: 1.75,
                pr: 1.5,
                py: 1.05,
                borderRadius: '8px',
                textDecoration: 'none',
                fontSize: '0.87rem',
                fontWeight: 600,
                color: active ? '#fff' : '#aebdd4',
                bgcolor: active ? NAVY_ACTIVE : 'transparent',
                borderLeft: active ? `3px solid ${ACCENT}` : '3px solid transparent',
                transition: 'background-color .15s ease, color .15s ease',
                '&:hover': { bgcolor: NAVY_ACTIVE, color: '#fff' },
              }}
            >
              <Box sx={{ display: 'flex', color: active ? ACCENT : 'inherit' }}>{item.icon}</Box>
              {item.label}
            </Box>
          );
        })}
      </Stack>

      <Divider sx={{ borderColor: 'rgba(255,255,255,0.08)', mx: 2 }} />
      <Stack direction="row" spacing={1.25} alignItems="center" sx={{ px: 2.25, py: 2 }}>
        <Avatar sx={{ width: 34, height: 34, bgcolor: 'secondary.main', fontSize: 14, fontWeight: 700 }}>
          {session?.displayName?.charAt(0)?.toUpperCase() ?? '?'}
        </Avatar>
        <Box sx={{ minWidth: 0, flex: 1 }}>
          <Typography noWrap sx={{ color: '#fff', fontSize: '0.83rem', fontWeight: 600 }}>
            {session?.displayName}
          </Typography>
          <Typography noWrap sx={{ color: '#7c93b5', fontSize: '0.72rem' }}>
            {session?.role}
          </Typography>
        </Box>
        <Tooltip title="Çıkış Yap">
          <IconButton
            size="small"
            onClick={logout}
            sx={{ color: '#aebdd4', '&:hover': { color: '#fff', bgcolor: NAVY_ACTIVE } }}
          >
            <LogoutIcon fontSize="small" />
          </IconButton>
        </Tooltip>
      </Stack>
    </Box>
  );
}

export function AppLayout({ children }: { children: ReactNode }) {
  const [mobileOpen, setMobileOpen] = useState(false);

  return (
    <Box sx={{ display: 'flex', minHeight: '100vh', bgcolor: 'background.default' }}>
      <AppBar
        position="fixed"
        elevation={0}
        sx={{ display: { xs: 'block', sm: 'none' }, zIndex: (t) => t.zIndex.drawer + 1 }}
      >
        <Toolbar sx={{ gap: 1 }}>
          <IconButton color="inherit" edge="start" onClick={() => setMobileOpen(true)}>
            <MenuIcon />
          </IconButton>
          <WaterDropIcon />
          <Typography variant="h6" sx={{ fontWeight: 700, fontSize: '1.05rem' }}>
            Asl-Su
          </Typography>
        </Toolbar>
      </AppBar>

      <Box component="nav" sx={{ width: { sm: DRAWER_WIDTH }, flexShrink: { sm: 0 } }}>
        <Drawer
          variant="temporary"
          open={mobileOpen}
          onClose={() => setMobileOpen(false)}
          ModalProps={{ keepMounted: true }}
          sx={{
            display: { xs: 'block', sm: 'none' },
            '& .MuiDrawer-paper': { width: DRAWER_WIDTH, border: 'none' },
          }}
        >
          <SidebarContent onNavigate={() => setMobileOpen(false)} />
        </Drawer>
        <Drawer
          variant="permanent"
          sx={{
            display: { xs: 'none', sm: 'block' },
            '& .MuiDrawer-paper': { width: DRAWER_WIDTH, border: 'none' },
          }}
          open
        >
          <SidebarContent />
        </Drawer>
      </Box>

      <Box
        component="main"
        sx={{ flexGrow: 1, minHeight: '100vh', width: { sm: `calc(100% - ${DRAWER_WIDTH}px)` } }}
      >
        <Toolbar sx={{ display: { xs: 'flex', sm: 'none' } }} />
        <Box sx={{ p: { xs: 2.5, sm: 4 }, maxWidth: 1240, mx: 'auto' }}>{children}</Box>
      </Box>
    </Box>
  );
}
