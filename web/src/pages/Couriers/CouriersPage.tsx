import { useEffect, useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { createCourier, getCouriers } from '../../api/couriers';
import type { Courier } from '../../types/courier';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Chip from '@mui/material/Chip';
import Paper from '@mui/material/Paper';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';

export function CouriersPage() {
  const navigate = useNavigate();
  const [couriers, setCouriers] = useState<Courier[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [name, setName] = useState('');
  const [phone, setPhone] = useState('');

  function refresh() {
    getCouriers().then(setCouriers).catch((e) => setError(e instanceof Error ? e.message : 'Kuryeler yüklenemedi.'));
  }

  useEffect(refresh, []);

  async function handleCreate(event: FormEvent) {
    event.preventDefault();
    if (!name.trim()) return;
    setError(null);
    try {
      await createCourier(name, phone.trim() || null);
      setName('');
      setPhone('');
      refresh();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Kurye eklenemedi.');
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
        <Box component="form" onSubmit={handleCreate} sx={{ display: 'flex', gap: 1.5, flexWrap: 'wrap' }}>
          <TextField size="small" label="Ad Soyad" value={name} onChange={(e) => setName(e.target.value)} required />
          <TextField size="small" label="Telefon" value={phone} onChange={(e) => setPhone(e.target.value)} />
          <Button type="submit" variant="contained">
            Ekle
          </Button>
        </Box>
      </Paper>

      <Paper variant="outlined">
        <TableContainer>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Ad Soyad</TableCell>
                <TableCell>Telefon</TableCell>
                <TableCell>Durum</TableCell>
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
                  <TableCell>
                    <Chip size="small" label={courier.isActive ? 'Aktif' : 'Pasif'} color={courier.isActive ? 'success' : 'default'} />
                  </TableCell>
                </TableRow>
              ))}
              {couriers.length === 0 && (
                <TableRow>
                  <TableCell colSpan={3}>
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
    </Box>
  );
}
