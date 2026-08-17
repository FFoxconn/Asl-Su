import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';
import CheckCircleOutlineIcon from '@mui/icons-material/CheckCircleOutline';
import WaterDropIcon from '@mui/icons-material/WaterDrop';
import { useAuth } from '../../auth/AuthContext';
import { ApiError } from '../../api/client';

const FEATURES = [
  'Ürün, stok ve fiyat yönetimi tek ekranda',
  'Trendyol Go sipariş akışı canlı takip',
  'Kurye atama ve teslimat durumu',
];

export function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);
    try {
      await login(email, password);
      navigate('/', { replace: true });
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Giriş yapılamadı.');
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <Box sx={{ minHeight: '100vh', display: 'flex', bgcolor: 'background.default' }}>
      <Box
        sx={{
          display: { xs: 'none', md: 'flex' },
          flex: 1.15,
          flexDirection: 'column',
          justifyContent: 'center',
          position: 'relative',
          overflow: 'hidden',
          px: 8,
          color: '#fff',
          background:
            'radial-gradient(circle at 16% 14%, rgba(79,209,197,0.16), transparent 45%), linear-gradient(155deg, #0f2237 0%, #081422 100%)',
        }}
      >
        <Box
          sx={{
            position: 'absolute',
            width: 440,
            height: 440,
            borderRadius: '50%',
            top: -160,
            right: -160,
            bgcolor: 'rgba(255,255,255,0.035)',
          }}
        />
        <Box
          sx={{
            position: 'absolute',
            width: 320,
            height: 320,
            borderRadius: '50%',
            bottom: -130,
            left: -90,
            bgcolor: 'rgba(79,209,197,0.07)',
          }}
        />

        <Stack direction="row" spacing={1.5} alignItems="center" sx={{ position: 'relative', mb: 7 }}>
          <Box
            sx={{
              width: 38,
              height: 38,
              borderRadius: '10px',
              bgcolor: 'primary.main',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
            }}
          >
            <WaterDropIcon sx={{ color: '#fff' }} />
          </Box>
          <Typography sx={{ fontSize: '1.3rem', fontWeight: 700 }}>Asl-Su</Typography>
        </Stack>

        <Typography
          sx={{
            position: 'relative',
            fontSize: '0.78rem',
            fontWeight: 600,
            letterSpacing: '.08em',
            textTransform: 'uppercase',
            color: '#4fd1c5',
            mb: 1.5,
          }}
        >
          Yönetim Paneli
        </Typography>
        <Typography sx={{ position: 'relative', fontSize: '2rem', fontWeight: 700, lineHeight: 1.3, maxWidth: 420 }}>
          Ürün, sipariş ve kurye yönetimini tek panelden yürütün
        </Typography>
        <Typography
          sx={{ position: 'relative', mt: 2, maxWidth: 400, fontSize: '0.96rem', lineHeight: 1.6, color: 'rgba(255,255,255,0.68)' }}
        >
          Trendyol Go Market entegrasyonuyla stok, fiyat ve sipariş akışınızı gerçek zamanlı takip edin.
        </Typography>

        <Stack spacing={2} sx={{ position: 'relative', mt: 6 }}>
          {FEATURES.map((f) => (
            <Stack key={f} direction="row" spacing={1.5} alignItems="center">
              <Box
                sx={{
                  width: 22,
                  height: 22,
                  borderRadius: '50%',
                  bgcolor: 'rgba(79,209,197,0.16)',
                  color: '#4fd1c5',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  flexShrink: 0,
                }}
              >
                <CheckCircleOutlineIcon sx={{ fontSize: 14 }} />
              </Box>
              <Typography sx={{ fontSize: '0.9rem', color: 'rgba(255,255,255,0.85)' }}>{f}</Typography>
            </Stack>
          ))}
        </Stack>
      </Box>

      <Box
        sx={{
          flex: 1,
          minWidth: { xs: '100%', md: 340 },
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          px: 3,
          py: 5,
        }}
      >
        <Box
          sx={{
            width: '100%',
            maxWidth: 388,
            bgcolor: 'background.paper',
            borderRadius: '18px',
            border: '1px solid',
            borderColor: 'divider',
            boxShadow: '0 16px 40px rgba(15,34,55,0.08)',
            px: 5,
            pt: 5.5,
            pb: 4,
          }}
        >
          <Stack direction="row" spacing={1.25} alignItems="center" sx={{ display: { xs: 'flex', md: 'none' }, mb: 3.5 }}>
            <WaterDropIcon color="primary" />
            <Typography sx={{ fontWeight: 700, fontSize: '1.1rem' }}>Asl-Su</Typography>
          </Stack>

          <Typography variant="h1" sx={{ fontSize: '1.4rem', fontWeight: 700, mb: 0.75 }}>
            Giriş Yap
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 3.5 }}>
            Yönetim paneline erişmek için bilgilerinizi girin
          </Typography>

          <Box component="form" onSubmit={handleSubmit}>
            <Stack spacing={2.25}>
              <TextField
                id="email"
                label="E-posta"
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                fullWidth
                autoFocus
              />
              <TextField
                id="password"
                label="Şifre"
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                fullWidth
              />
              {error && <Alert severity="error">{error}</Alert>}
              <Button type="submit" variant="contained" size="large" disabled={isSubmitting} fullWidth sx={{ py: 1.4 }}>
                {isSubmitting ? 'Giriş yapılıyor...' : 'Giriş Yap'}
              </Button>
            </Stack>
          </Box>
        </Box>
      </Box>
    </Box>
  );
}
