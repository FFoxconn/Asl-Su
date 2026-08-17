import { createTheme } from '@mui/material/styles';

export const theme = createTheme({
  palette: {
    mode: 'light',
    primary: { main: '#0b6e99', dark: '#084d6e', light: '#4b93b5' },
    secondary: { main: '#0f9d8a' },
    background: { default: '#f4f7f9', paper: '#ffffff' },
    divider: '#e2e5e9',
    text: { primary: '#1c2530', secondary: '#6b7683' },
  },
  shape: { borderRadius: 10 },
  typography: {
    fontFamily: 'system-ui, "Segoe UI", Roboto, sans-serif',
    h1: { fontSize: '1.6rem', fontWeight: 700, letterSpacing: '-0.01em' },
    h2: { fontSize: '1.05rem', fontWeight: 700 },
  },
  components: {
    MuiAppBar: {
      styleOverrides: {
        root: { backgroundColor: '#0f2237' },
      },
    },
    MuiPaper: {
      styleOverrides: {
        root: { backgroundImage: 'none' },
      },
    },
    MuiCard: {
      styleOverrides: {
        root: { borderRadius: 12 },
      },
    },
    MuiButton: {
      styleOverrides: {
        root: { borderRadius: 8, textTransform: 'none', fontWeight: 600 },
      },
    },
    MuiChip: {
      styleOverrides: {
        root: { fontWeight: 600 },
      },
    },
    MuiTextField: {
      defaultProps: {
        size: 'medium',
      },
    },
  },
});
