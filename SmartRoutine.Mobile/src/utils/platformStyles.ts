import { Platform, StyleSheet } from 'react-native';

export const platformStyles = {
  // Platform spesifik renkler
  colors: {
    primary: Platform.OS === 'ios' ? '#007AFF' : '#007AFF',
    secondary: Platform.OS === 'ios' ? '#5856D6' : '#FF5722',
    background: Platform.OS === 'ios' ? '#F2F2F7' : '#F5F5F5',
    surface: Platform.OS === 'ios' ? '#FFFFFF' : '#FFFFFF',
    text: Platform.OS === 'ios' ? '#000000' : '#000000',
    textSecondary: Platform.OS === 'ios' ? '#8E8E93' : '#666666',
    border: Platform.OS === 'ios' ? '#C6C6C8' : '#E0E0E0',
    success: '#4CAF50',
    warning: '#FF9800',
    error: '#F44336',
  },

  // Platform spesifik boyutlar
  spacing: {
    xs: Platform.OS === 'ios' ? 4 : 4,
    sm: Platform.OS === 'ios' ? 8 : 8,
    md: Platform.OS === 'ios' ? 16 : 16,
    lg: Platform.OS === 'ios' ? 24 : 24,
    xl: Platform.OS === 'ios' ? 32 : 32,
  },

  // Platform spesifik font boyutları
  typography: {
    h1: Platform.OS === 'ios' ? 34 : 32,
    h2: Platform.OS === 'ios' ? 28 : 26,
    h3: Platform.OS === 'ios' ? 22 : 20,
    h4: Platform.OS === 'ios' ? 20 : 18,
    body: Platform.OS === 'ios' ? 17 : 16,
    caption: Platform.OS === 'ios' ? 12 : 12,
  },

  // Platform spesifik gölgeler
  shadows: {
    small: Platform.OS === 'ios' 
      ? {
          shadowColor: '#000',
          shadowOffset: { width: 0, height: 1 },
          shadowOpacity: 0.2,
          shadowRadius: 2,
        }
      : {
          elevation: 2,
        },
    medium: Platform.OS === 'ios'
      ? {
          shadowColor: '#000',
          shadowOffset: { width: 0, height: 2 },
          shadowOpacity: 0.25,
          shadowRadius: 4,
        }
      : {
          elevation: 4,
        },
    large: Platform.OS === 'ios'
      ? {
          shadowColor: '#000',
          shadowOffset: { width: 0, height: 4 },
          shadowOpacity: 0.3,
          shadowRadius: 8,
        }
      : {
          elevation: 8,
        },
  },

  // Platform spesifik border radius
  borderRadius: {
    small: Platform.OS === 'ios' ? 6 : 4,
    medium: Platform.OS === 'ios' ? 12 : 8,
    large: Platform.OS === 'ios' ? 16 : 12,
    round: Platform.OS === 'ios' ? 50 : 50,
  },
};

// Platform spesifik stil helper fonksiyonları
export const createPlatformStyle = (iosStyle: any, androidStyle: any) => {
  return Platform.OS === 'ios' ? iosStyle : androidStyle;
};

export const getPlatformColor = (iosColor: string, androidColor: string) => {
  return Platform.OS === 'ios' ? iosColor : androidColor;
};

export const getPlatformSpacing = (iosSpacing: number, androidSpacing: number) => {
  return Platform.OS === 'ios' ? iosSpacing : androidSpacing;
};

// Ortak stiller
export const commonStyles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: platformStyles.colors.background,
  },
  card: {
    backgroundColor: platformStyles.colors.surface,
    borderRadius: platformStyles.borderRadius.medium,
    ...platformStyles.shadows.small,
    margin: platformStyles.spacing.md,
  },
  button: {
    borderRadius: platformStyles.borderRadius.medium,
    paddingVertical: platformStyles.spacing.sm,
    paddingHorizontal: platformStyles.spacing.md,
  },
  input: {
    backgroundColor: platformStyles.colors.surface,
    borderColor: platformStyles.colors.border,
    borderRadius: platformStyles.borderRadius.small,
    borderWidth: 1,
    paddingHorizontal: platformStyles.spacing.md,
    paddingVertical: platformStyles.spacing.sm,
  },
}); 