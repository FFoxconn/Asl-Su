import { createTheme } from '@mui/material/styles';

export const theme = createTheme({
  palette: {
    mode: 'light',
    primary: { main: '#0b6e99', dark: '#084d6e', light: '#4b93b5' },
    secondary: { main: '#0f9d8a' },
    background: { default: '#f4f7f9', paper: '#ffffff' },
  },
  shape: { borderRadius: 8 },
  typography: {
    fontFamily: 'system-ui, "Segoe UI", Roboto, sans-serif',
    h1: { fontSize: '1.75rem', fontWeight: 600 },
    h2: { fontSize: '1.25rem', fontWeight: 600 },
  },
  components: {
    MuiAppBar: {
      styleOverrides: {
        root: { backgroundColor: '#0b6e99' },
      },
    },
    MuiPaper: {
      styleOverrides: {
        root: { backgroundImage: 'none' },
      },
    },
  },
});
