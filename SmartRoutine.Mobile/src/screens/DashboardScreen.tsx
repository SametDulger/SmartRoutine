import React, { useState, useEffect } from 'react';
import {
  View,
  StyleSheet,
  ScrollView,
  RefreshControl,
  Alert,
  TouchableOpacity,
  Text,
  StatusBar,
  ActivityIndicator,
} from 'react-native';

import { LinearGradient } from 'expo-linear-gradient';
import { Ionicons } from '@expo/vector-icons';
import { Routine, Stats } from '../types';
import apiService from '../services/apiService';
import { useAuth } from '../context/AuthContext';

const DashboardScreen: React.FC = () => {
  const [stats, setStats] = useState<Stats | null>(null);
  const [todayRoutines, setTodayRoutines] = useState<Routine[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isRefreshing, setIsRefreshing] = useState(false);
  const { user } = useAuth();

  useEffect(() => {
    loadDashboardData();
  }, []);

  const loadDashboardData = async () => {
    try {
      setIsLoading(true);
      const [statsData, routinesData] = await Promise.all([
        apiService.getStats(),
        apiService.getTodayRoutines(),
      ]);
      
      setStats(statsData);
      setTodayRoutines(routinesData);
    } catch (error) {
      console.error('Dashboard data load failed:', error);
      Alert.alert('Hata', 'Veriler yüklenirken bir hata oluştu');
    } finally {
      setIsLoading(false);
    }
  };

  const handleRefresh = async () => {
    setIsRefreshing(true);
    await loadDashboardData();
    setIsRefreshing(false);
  };

  const handleCompleteRoutine = async (routineId: string) => {
    try {
      await apiService.completeRoutine(routineId);
      Alert.alert('Başarılı', 'Rutin başarıyla tamamlandı!');
      loadDashboardData();
    } catch (error) {
      Alert.alert('Hata', 'Rutin tamamlanırken bir hata oluştu');
    }
  };

  if (isLoading) {
    return (
      <View style={styles.loadingContainer}>
        <StatusBar barStyle="light-content" backgroundColor="transparent" translucent />
        <LinearGradient
          colors={['#667eea', '#764ba2']}
          style={styles.loadingGradient}
        >
          <ActivityIndicator size="large" color="#fff" />
          <Text style={styles.loadingText}>Yükleniyor...</Text>
        </LinearGradient>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <StatusBar barStyle="light-content" backgroundColor="transparent" translucent />
      
      <LinearGradient
        colors={['#667eea', '#764ba2']}
        style={styles.gradient}
      >
        <ScrollView
          style={styles.scrollView}
          refreshControl={
            <RefreshControl 
              refreshing={isRefreshing} 
              onRefresh={handleRefresh}
              tintColor="#fff"
              colors={['#fff']}
            />
          }
        >
          {/* Header */}
          <View style={styles.header}>
            <View style={styles.headerContent}>
              <View style={styles.userInfo}>
                <View style={styles.avatar}>
                  <Ionicons name="person" size={24} color="#667eea" />
                </View>
                <View style={styles.userText}>
                  <Text style={styles.welcomeText}>
                    Merhaba, {user?.displayName || user?.firstName || user?.userName || (user?.email ? user.email.split('@')[0] : 'Kullanıcı')}!
                  </Text>
                  <Text style={styles.dateText}>
                    {new Date().toLocaleDateString('tr-TR', {
                      weekday: 'long',
                      year: 'numeric',
                      month: 'long',
                      day: 'numeric',
                    })}
                  </Text>
                </View>
              </View>
            </View>
          </View>

          {/* Stats Cards */}
          {stats && (
            <View style={styles.statsContainer}>
              <Text style={styles.sectionTitle}>Bugünkü İstatistikler</Text>
              
              <View style={styles.statsGrid}>
                <View style={styles.statCard}>
                  <View style={styles.statIcon}>
                    <Ionicons name="list" size={24} color="#4CAF50" />
                  </View>
                  <Text style={styles.statNumber}>{stats.totalRoutines}</Text>
                  <Text style={styles.statLabel}>Toplam Rutin</Text>
                </View>

                <View style={styles.statCard}>
                  <View style={styles.statIcon}>
                    <Ionicons name="checkmark-circle" size={24} color="#2196F3" />
                  </View>
                  <Text style={styles.statNumber}>{stats.completedToday}</Text>
                  <Text style={styles.statLabel}>Tamamlanan</Text>
                </View>

                <View style={styles.statCard}>
                  <View style={styles.statIcon}>
                    <Ionicons name="trending-up" size={24} color="#FF9800" />
                  </View>
                  <Text style={styles.statNumber}>%{Math.round(stats.completionRate)}</Text>
                  <Text style={styles.statLabel}>Başarı Oranı</Text>
                </View>

                <View style={styles.statCard}>
                  <View style={styles.statIcon}>
                    <Ionicons name="flame" size={24} color="#F44336" />
                  </View>
                  <Text style={styles.statNumber}>{stats.currentStreak}</Text>
                  <Text style={styles.statLabel}>Günlük Seri</Text>
                </View>
              </View>
            </View>
          )}

          {/* Today's Routines */}
          <View style={styles.routinesContainer}>
            <Text style={styles.sectionTitle}>Bugünkü Rutinler</Text>
            
            {!todayRoutines || todayRoutines.length === 0 ? (
              <View style={styles.emptyState}>
                <Ionicons name="checkmark-done-circle" size={64} color="rgba(255,255,255,0.5)" />
                <Text style={styles.emptyText}>Bugün için rutin bulunmuyor</Text>
                <Text style={styles.emptySubtext}>Tüm rutinlerinizi tamamladınız!</Text>
              </View>
            ) : (
              todayRoutines.map((routine) => (
                <View key={routine.id} style={styles.routineCard}>
                  <View style={styles.routineContent}>
                    <View style={styles.routineInfo}>
                      <Text style={styles.routineTitle}>{routine.title}</Text>
                      {routine.description && (
                        <Text style={styles.routineDescription}>{routine.description}</Text>
                      )}
                      <View style={styles.routineMeta}>
                        <View style={styles.routineTime}>
                          <Ionicons name="time" size={16} color="rgba(255,255,255,0.7)" />
                          <Text style={styles.routineTimeText}>
                            {new Date(routine.scheduledTime).toLocaleTimeString('tr-TR', {
                              hour: '2-digit',
                              minute: '2-digit',
                            })}
                          </Text>
                        </View>
                        <View style={styles.routineType}>
                          <Ionicons name="repeat" size={16} color="rgba(255,255,255,0.7)" />
                          <Text style={styles.routineTypeText}>
                            {routine.repeatType === 'Daily' ? 'Günlük' : 
                             routine.repeatType === 'Weekly' ? 'Haftalık' : 
                             routine.repeatType === 'CustomDays' ? 'Özel Günler' : 'Aralıklı'}
                          </Text>
                        </View>
                      </View>
                    </View>
                    
                    <TouchableOpacity
                      style={[
                        styles.completeButton,
                        routine.isCompletedToday && styles.completedButton
                      ]}
                      onPress={() => handleCompleteRoutine(routine.id)}
                      disabled={routine.isCompletedToday}
                    >
                      <Ionicons 
                        name={routine.isCompletedToday ? "checkmark-circle" : "checkmark-circle-outline"} 
                        size={24} 
                        color={routine.isCompletedToday ? "#4CAF50" : "#fff"} 
                      />
                    </TouchableOpacity>
                  </View>
                </View>
              ))
            )}
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
  loadingContainer: {
    flex: 1,
  },
  loadingGradient: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
  loadingText: {
    color: '#fff',
    fontSize: 16,
    marginTop: 16,
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
  userInfo: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  avatar: {
    width: 50,
    height: 50,
    borderRadius: 25,
    backgroundColor: 'rgba(255, 255, 255, 0.2)',
    justifyContent: 'center',
    alignItems: 'center',
    marginRight: 15,
  },
  userText: {
    flex: 1,
  },
  welcomeText: {
    fontSize: 20,
    fontWeight: 'bold',
    color: '#fff',
    marginBottom: 4,
  },
  dateText: {
    fontSize: 14,
    color: 'rgba(255, 255, 255, 0.8)',
  },
  statsContainer: {
    paddingHorizontal: 20,
    marginBottom: 30,
  },
  sectionTitle: {
    fontSize: 18,
    fontWeight: 'bold',
    color: '#fff',
    marginBottom: 15,
  },
  statsGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    justifyContent: 'space-between',
  },
  statCard: {
    width: '48%',
    backgroundColor: 'rgba(255, 255, 255, 0.1)',
    borderRadius: 12,
    padding: 20,
    marginBottom: 15,
    alignItems: 'center',
  },
  statIcon: {
    width: 40,
    height: 40,
    borderRadius: 20,
    backgroundColor: 'rgba(255, 255, 255, 0.2)',
    justifyContent: 'center',
    alignItems: 'center',
    marginBottom: 10,
  },
  statNumber: {
    fontSize: 24,
    fontWeight: 'bold',
    color: '#fff',
    marginBottom: 5,
  },
  statLabel: {
    fontSize: 12,
    color: 'rgba(255, 255, 255, 0.8)',
    textAlign: 'center',
  },
  routinesContainer: {
    paddingHorizontal: 20,
    paddingBottom: 30,
  },
  emptyState: {
    alignItems: 'center',
    paddingVertical: 40,
  },
  emptyText: {
    fontSize: 16,
    color: 'rgba(255, 255, 255, 0.8)',
    marginTop: 16,
    textAlign: 'center',
  },
  emptySubtext: {
    fontSize: 14,
    color: 'rgba(255, 255, 255, 0.6)',
    marginTop: 8,
    textAlign: 'center',
  },
  routineCard: {
    backgroundColor: 'rgba(255, 255, 255, 0.1)',
    borderRadius: 12,
    marginBottom: 15,
    overflow: 'hidden',
  },
  routineContent: {
    flexDirection: 'row',
    alignItems: 'center',
    padding: 20,
  },
  routineInfo: {
    flex: 1,
  },
  routineTitle: {
    fontSize: 16,
    fontWeight: 'bold',
    color: '#fff',
    marginBottom: 4,
  },
  routineDescription: {
    fontSize: 14,
    color: 'rgba(255, 255, 255, 0.8)',
    marginBottom: 8,
  },
  routineMeta: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  routineTime: {
    flexDirection: 'row',
    alignItems: 'center',
    marginRight: 20,
  },
  routineTimeText: {
    fontSize: 12,
    color: 'rgba(255, 255, 255, 0.7)',
    marginLeft: 4,
  },
  routineType: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  routineTypeText: {
    fontSize: 12,
    color: 'rgba(255, 255, 255, 0.7)',
    marginLeft: 4,
  },
  completeButton: {
    width: 44,
    height: 44,
    borderRadius: 22,
    backgroundColor: 'rgba(255, 255, 255, 0.2)',
    justifyContent: 'center',
    alignItems: 'center',
  },
  completedButton: {
    backgroundColor: 'rgba(76, 175, 80, 0.2)',
  },
});

export default DashboardScreen; 