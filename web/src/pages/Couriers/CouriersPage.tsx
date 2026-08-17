import { useEffect, useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { createCourier, getCouriers, setCourierActive, setCourierLogin } from '../../api/couriers';
import type { Courier } from '../../types/courier';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Checkbox from '@mui/material/Checkbox';
import Chip from '@mui/material/Chip';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';
import FormControlLabel from '@mui/material/FormControlLabel';
import IconButton from '@mui/material/IconButton';
import Paper from '@mui/material/Paper';
import Stack from '@mui/material/Stack';
import Switch from '@mui/material/Switch';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import TextField from '@mui/material/TextField';
import Tooltip from '@mui/material/Tooltip';
import Typography from '@mui/material/Typography';
import VpnKeyOutlinedIcon from '@mui/icons-material/VpnKeyOutlined';

export function CouriersPage() {
  const navigate = useNavigate();
  const [couriers, setCouriers] = useState<Courier[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [name, setName] = useState('');
  const [phone, setPhone] = useState('');
  const [grantLogin, setGrantLogin] = useState(false);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loginDialogCourier, setLoginDialogCourier] = useState<Courier | null>(null);
  const [togglingId, setTogglingId] = useState<number | null>(null);

  function refresh() {
    getCouriers().then(setCouriers).catch((e) => setError(e instanceof Error ? e.message : 'Kuryeler yüklenemedi.'));
  }

  useEffect(refresh, []);

  async function handleCreate(event: FormEvent) {
    event.preventDefault();
    if (!name.trim()) return;
    if (grantLogin && (!email.trim() || !password.trim())) return;
    setError(null);
    try {
      await createCourier(
        name,
        phone.trim() || null,
        grantLogin ? email.trim() : null,
        grantLogin ? password : null,
      );
      setName('');
      setPhone('');
      setGrantLogin(false);
      setEmail('');
      setPassword('');
      refresh();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Kurye eklenemedi.');
    }
  }

  async function handleToggleActive(courier: Courier) {
    setTogglingId(courier.id);
    setError(null);
    try {
      await setCourierActive(courier.id, !courier.isActive);
      refresh();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Durum değiştirilemedi.');
    } finally {
      setTogglingId(null);
    }
  }

  return (
    <Box>
      <Typography variant="h1" gutterBottom>
        Kuryeler
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      <Paper variant="outlined" sx={{ p: 2, mb: 3 }}>
        <Typography variant="h2" gutterBottom>
          Yeni Kurye
        </Typography>
        <Box component="form" onSubmit={handleCreate} sx={{ display: 'flex', flexDirection: 'column', gap: 1.5 }}>
          <Box sx={{ display: 'flex', gap: 1.5, flexWrap: 'wrap' }}>
            <TextField size="small" label="Ad Soyad" value={name} onChange={(e) => setName(e.target.value)} required />
            <TextField size="small" label="Telefon" value={phone} onChange={(e) => setPhone(e.target.value)} />
          </Box>

          <FormControlLabel
            control={<Checkbox checked={grantLogin} onChange={(e) => setGrantLogin(e.target.checked)} />}
            label="Mobil uygulamaya giriş yetkisi ver"
          />

          {grantLogin && (
            <Box sx={{ display: 'flex', gap: 1.5, flexWrap: 'wrap' }}>
              <TextField
                size="small"
                label="E-posta"
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required={grantLogin}
              />
              <TextField
                size="small"
                label="Şifre"
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required={grantLogin}
              />
            </Box>
          )}

          <Box>
            <Button type="submit" variant="contained">
              Ekle
            </Button>
          </Box>
        </Box>
      </Paper>

      <Paper variant="outlined">
        <TableContainer>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Ad Soyad</TableCell>
                <TableCell>Telefon</TableCell>
                <TableCell align="right">Toplam Sipariş</TableCell>
                <TableCell align="right">Toplam Ciro</TableCell>
                <TableCell>Durum</TableCell>
                <TableCell>Giriş</TableCell>
                <TableCell align="right">İşlemler</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {couriers.map((courier) => (
                <TableRow
                  key={courier.id}
                  hover
                  onClick={() => navigate(`/couriers/${courier.id}`)}
                  sx={{ cursor: 'pointer' }}
                >
                  <TableCell>{courier.name}</TableCell>
                  <TableCell>{courier.phone ?? '-'}</TableCell>
                  <TableCell align="right">{courier.totalOrders}</TableCell>
                  <TableCell align="right">{courier.totalRevenue.toFixed(2)} ₺</TableCell>
                  <TableCell onClick={(e) => e.stopPropagation()}>
                    <Stack direction="row" spacing={0.5} alignItems="center">
                      <Switch
                        size="small"
                        checked={courier.isActive}
                        disabled={togglingId === courier.id}
                        onChange={() => handleToggleActive(courier)}
                      />
                      <Chip
                        size="small"
                        label={courier.isActive ? 'Aktif' : 'Pasif'}
                        color={courier.isActive ? 'success' : 'default'}
                      />
                    </Stack>
                  </TableCell>
                  <TableCell>
                    {courier.hasLogin ? (
                      <Chip size="small" label="Var" color="info" variant="outlined" />
                    ) : (
                      <Chip size="small" label="Yok" variant="outlined" />
                    )}
                  </TableCell>
                  <TableCell align="right" onClick={(e) => e.stopPropagation()}>
                    <Tooltip title={courier.hasLogin ? 'Şifreyi Sıfırla' : 'Giriş Yetkisi Ver'}>
                      <IconButton size="small" onClick={() => setLoginDialogCourier(courier)}>
                        <VpnKeyOutlinedIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                  </TableCell>
                </TableRow>
              ))}
              {couriers.length === 0 && (
                <TableRow>
                  <TableCell colSpan={7}>
                    <Typography variant="body2" color="text.secondary" sx={{ py: 2 }}>
                      Henüz kurye eklenmemiş.
                    </Typography>
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </TableContainer>
      </Paper>

      {loginDialogCourier && (
        <CourierLoginDialog
          courier={loginDialogCourier}
          onClose={() => setLoginDialogCourier(null)}
          onSaved={() => {
            setLoginDialogCourier(null);
            refresh();
          }}
        />
      )}
    </Box>
  );
}

function CourierLoginDialog({
  courier,
  onClose,
  onSaved,
}: {
  courier: Courier;
  onClose: () => void;
  onSaved: () => void;
}) {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isSaving, setIsSaving] = useState(false);

  async function handleSave() {
    if (!email.trim() || !password.trim()) return;
    setIsSaving(true);
    setError(null);
    try {
      await setCourierLogin(courier.id, email.trim(), password);
      onSaved();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'İşlem başarısız oldu.');
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <Dialog open onClose={onClose} maxWidth="xs" fullWidth>
      <DialogTitle>
        {courier.hasLogin ? 'Şifreyi Sıfırla' : 'Giriş Yetkisi Ver'} — {courier.name}
      </DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ mt: 1 }}>
          {courier.hasLogin && (
            <Alert severity="info">
              Bu kuryenin zaten bir hesabı var. Yeni e-posta/şifre girersen mevcut giriş bilgileri güncellenir.
            </Alert>
          )}
          {error && <Alert severity="error">{error}</Alert>}
          <TextField
            label="E-posta"
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            autoFocus
            fullWidth
          />
          <TextField
            label="Şifre"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            fullWidth
          />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Vazgeç</Button>
        <Button onClick={handleSave} variant="contained" disabled={isSaving}>
          {isSaving ? 'Kaydediliyor...' : 'Kaydet'}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
