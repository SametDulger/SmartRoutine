import React, { useState } from 'react';
import {
  View,
  StyleSheet,
  ScrollView,
  Alert,
  TouchableOpacity,
  Text,
  StatusBar,
  Linking,
  Share,
} from 'react-native';
import { LinearGradient } from 'expo-linear-gradient';
import { Ionicons } from '@expo/vector-icons';
import { useAuth } from '../context/AuthContext';
import { useNavigation } from '@react-navigation/native';
import apiService from '../services/apiService';

const ProfileScreen: React.FC = () => {
  const { user, logout, updateUser } = useAuth();
  const navigation = useNavigation();
  const [isLoading, setIsLoading] = useState(false);

  const handleLogout = () => {
    Alert.alert(
      'Çıkış Yap',
      'Hesabınızdan çıkış yapmak istediğinizden emin misiniz?',
      [
        { text: 'İptal', style: 'cancel' },
        {
          text: 'Çıkış Yap',
          style: 'destructive',
          onPress: async () => {
            try {
              setIsLoading(true);
              await logout();
            } catch (error) {
              Alert.alert('Hata', 'Çıkış yapılırken bir hata oluştu');
            } finally {
              setIsLoading(false);
            }
          },
        },
      ]
    );
  };

  const handleEditProfile = () => {
    Alert.prompt(
      'Profil Düzenle',
      'Yeni görünen adınızı girin:',
      [
        { text: 'İptal', style: 'cancel' },
        {
          text: 'Güncelle',
          onPress: async (newDisplayName) => {
            if (newDisplayName && newDisplayName.trim()) {
              try {
                setIsLoading(true);
                const updatedUser = await apiService.updateProfile({
                  displayName: newDisplayName.trim()
                });
                // Update local user state
                updateUser(updatedUser);
                Alert.alert('Başarılı', 'Profil bilgileriniz güncellendi.');
              } catch (error: any) {
                const errorMessage = error.response?.data?.message || 'Profil güncellenirken bir hata oluştu';
                Alert.alert('Hata', errorMessage);
              } finally {
                setIsLoading(false);
              }
            } else {
              Alert.alert('Hata', 'Lütfen geçerli bir görünen ad girin.');
            }
          }
        }
      ],
      'plain-text',
      user?.displayName || ''
    );
  };

  const handleChangePassword = () => {
    Alert.prompt(
      'Mevcut Şifre',
      'Mevcut şifrenizi girin:',
      [
        { text: 'İptal', style: 'cancel' },
        {
          text: 'Devam',
          onPress: (currentPassword) => {
            if (currentPassword) {
              Alert.prompt(
                'Yeni Şifre',
                'Yeni şifrenizi girin (en az 6 karakter):',
                [
                  { text: 'İptal', style: 'cancel' },
                  {
                    text: 'Değiştir',
                    onPress: async (newPassword) => {
                      if (newPassword && newPassword.length >= 6) {
                        try {
                          setIsLoading(true);
                          await apiService.changePassword(currentPassword, newPassword);
                          Alert.alert('Başarılı', 'Şifreniz başarıyla değiştirildi.');
                        } catch (error: any) {
                          const errorMessage = error.response?.data?.message || 'Şifre değiştirilirken bir hata oluştu';
                          Alert.alert('Hata', errorMessage);
                        } finally {
                          setIsLoading(false);
                        }
                      } else {
                        Alert.alert('Hata', 'Yeni şifre en az 6 karakter olmalıdır.');
                      }
                    }
                  }
                ],
                'secure-text'
              );
            } else {
              Alert.alert('Hata', 'Lütfen mevcut şifrenizi girin.');
            }
          }
        }
      ],
      'secure-text'
    );
  };

  const handleNotificationSettings = () => {
    Alert.alert(
      'Bildirim Ayarları',
      'Bildirim ayarlarınızı yönetmek için cihaz ayarlarına gidin.',
      [
        { text: 'İptal', style: 'cancel' },
        { text: 'Ayarlar', onPress: () => Linking.openSettings() }
      ]
    );
  };

  const handleThemeSettings = () => {
    Alert.alert(
      'Tema Ayarları',
      'Tema ayarları yakında eklenecek. Şu anda varsayılan tema kullanılıyor.',
      [{ text: 'Tamam' }]
    );
  };

  const handleHelp = () => {
    Alert.alert(
      'Yardım',
      'SmartRoutine kullanımı hakkında yardım almak için bizimle iletişime geçin.',
      [
        { text: 'İptal', style: 'cancel' },
        { text: 'E-posta Gönder', onPress: () => Linking.openURL('mailto:support@smartroutine.com') }
      ]
    );
  };

  const handleContact = () => {
    Alert.alert(
      'İletişim',
      'Bizimle iletişime geçmek için e-posta gönderebilirsiniz.',
      [
        { text: 'İptal', style: 'cancel' },
        { text: 'E-posta Gönder', onPress: () => Linking.openURL('mailto:contact@smartroutine.com') }
      ]
    );
  };

  const handleAbout = () => {
    Alert.alert(
      'Hakkında',
      'SmartRoutine v1.0.0\n\nGünlük rutinlerinizi takip etmenizi ve hedeflerinize ulaşmanızı sağlayan akıllı uygulama.\n\n© 2024 SmartRoutine. Tüm hakları saklıdır.',
      [{ text: 'Tamam' }]
    );
  };

  const handleShareApp = async () => {
    try {
      await Share.share({
        message: 'SmartRoutine uygulamasını deneyin! Günlük rutinlerinizi takip etmenizi sağlar.',
        title: 'SmartRoutine',
      });
    } catch (error) {
      Alert.alert('Hata', 'Uygulama paylaşılırken bir hata oluştu');
    }
  };

  const handleExportData = () => {
    Alert.alert(
      'Veri Dışa Aktar',
      'Bu özellik yakında eklenecek. Şu anda verilerinizi dışa aktaramazsınız.',
      [{ text: 'Tamam' }]
    );
  };

  const handlePrivacyPolicy = () => {
    Alert.alert(
      'Gizlilik Politikası',
      'Gizlilik politikamız yakında eklenecek.',
      [{ text: 'Tamam' }]
    );
  };

  return (
    <View style={styles.container}>
      <StatusBar barStyle="light-content" backgroundColor="transparent" translucent />
      
      <LinearGradient
        colors={['#667eea', '#764ba2']}
        style={styles.gradient}
      >
        <ScrollView style={styles.scrollView}>
          {/* Header */}
      <View style={styles.header}>
            <View style={styles.headerContent}>
              <View style={styles.headerInfo}>
                <Ionicons name="person" size={32} color="#fff" />
                <View style={styles.headerText}>
                  <Text style={styles.title}>Profil</Text>
                  <Text style={styles.subtitle}>Hesap bilgilerinizi yönetin</Text>
                </View>
              </View>
            </View>
      </View>

      {/* User Info Card */}
          <View style={styles.userCard}>
          <View style={styles.userInfo}>
              <View style={styles.avatar}>
                <Ionicons name="person" size={32} color="#667eea" />
              </View>
            <View style={styles.userDetails}>
                <Text style={styles.userName}>
                  {user?.displayName || (user?.firstName && user?.lastName 
                    ? `${user.firstName} ${user.lastName}` 
                    : user?.firstName || user?.userName || (user?.email ? user.email.split('@')[0] : 'Kullanıcı'))
                  }
                </Text>
                <Text style={styles.userEmail}>
                {user?.email || 'email@example.com'}
                </Text>
              <Text style={styles.userSince}>
                Üye olma tarihi: {user?.createdAt ? 
                    new Date(user.createdAt).toLocaleDateString('tr-TR', {
                      year: 'numeric',
                      month: 'long',
                      day: 'numeric'
                    }) : 
                  'Bilinmiyor'
                }
              </Text>
                <Text style={styles.userId}>
                  Kullanıcı ID: {user?.id || 'N/A'}
                </Text>
              </View>
            </View>
          </View>

          {/* Settings Section */}
          <View style={styles.settingsContainer}>
            <Text style={styles.sectionTitle}>Ayarlar</Text>
            
            <View style={styles.settingsList}>
              <TouchableOpacity
                style={styles.settingItem}
                onPress={handleEditProfile}
              >
                <View style={styles.settingIcon}>
                  <Ionicons name="person-outline" size={24} color="#4CAF50" />
                </View>
                <View style={styles.settingContent}>
                  <Text style={styles.settingTitle}>Profil Bilgilerini Düzenle</Text>
                  <Text style={styles.settingDescription}>
                    Ad, e-posta ve diğer bilgilerinizi güncelleyin
                  </Text>
                </View>
                <Ionicons name="chevron-forward" size={20} color="rgba(255,255,255,0.6)" />
              </TouchableOpacity>

              <TouchableOpacity
                style={styles.settingItem}
                onPress={handleChangePassword}
              >
                <View style={styles.settingIcon}>
                  <Ionicons name="lock-closed-outline" size={24} color="#2196F3" />
                </View>
                <View style={styles.settingContent}>
                  <Text style={styles.settingTitle}>Şifre Değiştir</Text>
                  <Text style={styles.settingDescription}>
                    Hesap güvenliğinizi artırın
                  </Text>
                </View>
                <Ionicons name="chevron-forward" size={20} color="rgba(255,255,255,0.6)" />
              </TouchableOpacity>

              <TouchableOpacity
                style={styles.settingItem}
                onPress={handleNotificationSettings}
              >
                <View style={styles.settingIcon}>
                  <Ionicons name="notifications-outline" size={24} color="#FF9800" />
                </View>
                <View style={styles.settingContent}>
                  <Text style={styles.settingTitle}>Bildirim Ayarları</Text>
                  <Text style={styles.settingDescription}>
                    Bildirim tercihlerinizi yönetin
                  </Text>
                </View>
                <Ionicons name="chevron-forward" size={20} color="rgba(255,255,255,0.6)" />
              </TouchableOpacity>

              <TouchableOpacity
                style={styles.settingItem}
                onPress={handleThemeSettings}
              >
                <View style={styles.settingIcon}>
                  <Ionicons name="color-palette-outline" size={24} color="#9C27B0" />
                </View>
                <View style={styles.settingContent}>
                  <Text style={styles.settingTitle}>Tema</Text>
                  <Text style={styles.settingDescription}>
                    Uygulama temasını değiştirin
                  </Text>
                </View>
                <Ionicons name="chevron-forward" size={20} color="rgba(255,255,255,0.6)" />
              </TouchableOpacity>

              <TouchableOpacity
                style={styles.settingItem}
                onPress={handleExportData}
              >
                <View style={styles.settingIcon}>
                  <Ionicons name="download-outline" size={24} color="#607D8B" />
                </View>
                <View style={styles.settingContent}>
                  <Text style={styles.settingTitle}>Veri Dışa Aktar</Text>
                  <Text style={styles.settingDescription}>
                    Rutin verilerinizi dışa aktarın
                  </Text>
                </View>
                <Ionicons name="chevron-forward" size={20} color="rgba(255,255,255,0.6)" />
              </TouchableOpacity>
            </View>
          </View>

          {/* Support Section */}
          <View style={styles.supportContainer}>
            <Text style={styles.sectionTitle}>Destek</Text>
            
            <View style={styles.settingsList}>
              <TouchableOpacity
                style={styles.settingItem}
                onPress={handleHelp}
              >
                <View style={styles.settingIcon}>
                  <Ionicons name="help-circle-outline" size={24} color="#607D8B" />
                </View>
                <View style={styles.settingContent}>
                  <Text style={styles.settingTitle}>Yardım</Text>
                  <Text style={styles.settingDescription}>
                    Sık sorulan sorular ve destek
                  </Text>
                </View>
                <Ionicons name="chevron-forward" size={20} color="rgba(255,255,255,0.6)" />
              </TouchableOpacity>

              <TouchableOpacity
                style={styles.settingItem}
                onPress={handleContact}
              >
                <View style={styles.settingIcon}>
                  <Ionicons name="mail-outline" size={24} color="#795548" />
                </View>
                <View style={styles.settingContent}>
                  <Text style={styles.settingTitle}>İletişim</Text>
                  <Text style={styles.settingDescription}>
                    Bizimle iletişime geçin
                  </Text>
                </View>
                <Ionicons name="chevron-forward" size={20} color="rgba(255,255,255,0.6)" />
              </TouchableOpacity>

              <TouchableOpacity
                style={styles.settingItem}
                onPress={handleAbout}
              >
                <View style={styles.settingIcon}>
                  <Ionicons name="information-circle-outline" size={24} color="#FF5722" />
                </View>
                <View style={styles.settingContent}>
                  <Text style={styles.settingTitle}>Hakkında</Text>
                  <Text style={styles.settingDescription}>
                    Uygulama bilgileri ve lisans
                  </Text>
                </View>
                <Ionicons name="chevron-forward" size={20} color="rgba(255,255,255,0.6)" />
              </TouchableOpacity>

              <TouchableOpacity
                style={styles.settingItem}
                onPress={handlePrivacyPolicy}
              >
                <View style={styles.settingIcon}>
                  <Ionicons name="shield-checkmark-outline" size={24} color="#4CAF50" />
                </View>
                <View style={styles.settingContent}>
                  <Text style={styles.settingTitle}>Gizlilik Politikası</Text>
                  <Text style={styles.settingDescription}>
                    Veri kullanımı ve gizlilik
                  </Text>
                </View>
                <Ionicons name="chevron-forward" size={20} color="rgba(255,255,255,0.6)" />
              </TouchableOpacity>

              <TouchableOpacity
                style={styles.settingItem}
                onPress={handleShareApp}
              >
                <View style={styles.settingIcon}>
                  <Ionicons name="share-social-outline" size={24} color="#2196F3" />
                </View>
                <View style={styles.settingContent}>
                  <Text style={styles.settingTitle}>Uygulamayı Paylaş</Text>
                  <Text style={styles.settingDescription}>
                    Arkadaşlarınızla paylaşın
                  </Text>
                </View>
                <Ionicons name="chevron-forward" size={20} color="rgba(255,255,255,0.6)" />
              </TouchableOpacity>
            </View>
          </View>

      {/* Logout Button */}
      <View style={styles.logoutContainer}>
            <TouchableOpacity
              style={[styles.logoutButton, isLoading && styles.logoutButtonDisabled]}
          onPress={handleLogout}
              disabled={isLoading}
              activeOpacity={0.8}
            >
              <Ionicons name="log-out-outline" size={20} color="#FF6B6B" />
              <Text style={styles.logoutText}>
                {isLoading ? 'Çıkış Yapılıyor...' : 'Çıkış Yap'}
              </Text>
            </TouchableOpacity>
      </View>
    </ScrollView>
      </LinearGradient>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  gradient: {
    flex: 1,
  },
  scrollView: {
    flex: 1,
  },
  header: {
    paddingHorizontal: 20,
    paddingTop: 50,
    paddingBottom: 30,
  },
  headerContent: {
    backgroundColor: 'rgba(255, 255, 255, 0.1)',
    borderRadius: 16,
    padding: 20,
  },
  headerInfo: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  headerText: {
    marginLeft: 15,
    flex: 1,
  },
  title: {
    fontSize: 24,
    fontWeight: 'bold',
    color: '#fff',
    marginBottom: 4,
  },
  subtitle: {
    fontSize: 14,
    color: 'rgba(255, 255, 255, 0.8)',
  },
  userCard: {
    backgroundColor: 'rgba(255, 255, 255, 0.1)',
    borderRadius: 16,
    marginHorizontal: 20,
    marginBottom: 30,
    padding: 20,
  },
  userInfo: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  avatar: {
    width: 60,
    height: 60,
    borderRadius: 30,
    backgroundColor: 'rgba(255, 255, 255, 0.2)',
    justifyContent: 'center',
    alignItems: 'center',
    marginRight: 15,
  },
  userDetails: {
    flex: 1,
  },
  userName: {
    fontSize: 18,
    fontWeight: 'bold',
    color: '#fff',
    marginBottom: 4,
  },
  userEmail: {
    fontSize: 14,
    color: 'rgba(255, 255, 255, 0.9)',
    marginBottom: 4,
  },
  userSince: {
    fontSize: 12,
    color: 'rgba(255, 255, 255, 0.7)',
    marginBottom: 2,
  },
  userId: {
    fontSize: 11,
    color: 'rgba(255, 255, 255, 0.5)',
  },
  settingsContainer: {
    marginBottom: 30,
  },
  supportContainer: {
    marginBottom: 30,
  },
  sectionTitle: {
    fontSize: 18,
    fontWeight: 'bold',
    color: '#fff',
    marginBottom: 15,
    paddingHorizontal: 20,
  },
  settingsList: {
    paddingHorizontal: 20,
  },
  settingItem: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: 'rgba(255, 255, 255, 0.1)',
    borderRadius: 12,
    padding: 15,
    marginBottom: 10,
  },
  settingIcon: {
    width: 40,
    height: 40,
    borderRadius: 20,
    backgroundColor: 'rgba(255, 255, 255, 0.2)',
    justifyContent: 'center',
    alignItems: 'center',
    marginRight: 15,
  },
  settingContent: {
    flex: 1,
  },
  settingTitle: {
    fontSize: 16,
    fontWeight: '600',
    color: '#fff',
    marginBottom: 2,
  },
  settingDescription: {
    fontSize: 12,
    color: 'rgba(255, 255, 255, 0.7)',
  },
  logoutContainer: {
    paddingHorizontal: 20,
    paddingBottom: 30,
  },
  logoutButton: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: 'rgba(255, 107, 107, 0.2)',
    borderRadius: 12,
    padding: 15,
    borderWidth: 1,
    borderColor: 'rgba(255, 107, 107, 0.3)',
  },
  logoutButtonDisabled: {
    backgroundColor: 'rgba(255, 255, 255, 0.1)',
    borderColor: 'rgba(255, 255, 255, 0.2)',
  },
  logoutText: {
    fontSize: 16,
    fontWeight: '600',
    color: '#FF6B6B',
    marginLeft: 8,
  },
});

export default ProfileScreen; 