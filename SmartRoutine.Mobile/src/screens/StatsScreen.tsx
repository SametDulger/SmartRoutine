import React, { useState, useEffect } from 'react';
import {
  View,
  StyleSheet,
  ScrollView,
  RefreshControl,
  Alert,
  Text,
  StatusBar,
  ActivityIndicator,
} from 'react-native';

import { LinearGradient } from 'expo-linear-gradient';
import { Ionicons } from '@expo/vector-icons';
import { Stats } from '../types';
import apiService from '../services/apiService';

const StatsScreen: React.FC = () => {
  const [stats, setStats] = useState<Stats | null>(null);
  const [weeklyStats, setWeeklyStats] = useState<Stats | null>(null);
  const [streak, setStreak] = useState<number>(0);
  const [isLoading, setIsLoading] = useState(true);
  const [isRefreshing, setIsRefreshing] = useState(false);

  useEffect(() => {
    loadStatsData();
  }, []);

  const loadStatsData = async () => {
    try {
      setIsLoading(true);
      const [statsData, weeklyData, streakData] = await Promise.all([
        apiService.getStats(),
        apiService.getWeeklyStats(),
        apiService.getStreak(),
      ]);
      setStats(statsData);
      setWeeklyStats(weeklyData);
      setStreak(streakData);
    } catch (error) {
      console.error('Stats load failed:', error);
      Alert.alert('Hata', 'İstatistikler yüklenirken bir hata oluştu');
    } finally {
      setIsLoading(false);
    }
  };

  const handleRefresh = async () => {
    setIsRefreshing(true);
    await loadStatsData();
    setIsRefreshing(false);
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
              <View style={styles.headerInfo}>
                <Ionicons name="stats-chart" size={32} color="#fff" />
                <View style={styles.headerText}>
                  <Text style={styles.title}>İstatistikler</Text>
                  <Text style={styles.subtitle}>Rutin performansınızı takip edin</Text>
                </View>
              </View>
            </View>
          </View>

          {/* Current Streak */}
          <View style={styles.streakContainer}>
            <View style={styles.streakCard}>
              <View style={styles.streakIcon}>
                <Ionicons name="flame" size={32} color="#FF6B6B" />
              </View>
              <View style={styles.streakInfo}>
                <Text style={styles.streakNumber}>{streak}</Text>
                <Text style={styles.streakLabel}>Günlük Seri</Text>
                <Text style={styles.streakDescription}>
                  {streak > 0 ? 'Harika gidiyorsunuz!' : 'Seriye başlayın'}
                </Text>
              </View>
            </View>
          </View>

          {/* Overall Stats */}
          {stats && (
            <View style={styles.statsContainer}>
              <Text style={styles.sectionTitle}>Genel İstatistikler</Text>
              
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
                  <Text style={styles.statLabel}>Bugün Tamamlanan</Text>
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
                    <Ionicons name="calendar" size={24} color="#9C27B0" />
                  </View>
                  <Text style={styles.statNumber}>{stats.currentStreak}</Text>
                  <Text style={styles.statLabel}>Mevcut Seri</Text>
                </View>
              </View>
            </View>
          )}

          {/* Weekly Progress */}
          {weeklyStats && weeklyStats.weeklyStats && weeklyStats.weeklyStats.length > 0 && (
            <View style={styles.weeklyContainer}>
              <Text style={styles.sectionTitle}>Haftalık İlerleme</Text>
              
              <View style={styles.weeklyChart}>
                {weeklyStats.weeklyStats.map((day, index) => (
                  <View key={index} style={styles.dayColumn}>
                    <View style={styles.dayBar}>
                      <View 
                        style={[
                          styles.progressBar,
                          { 
                            height: day.total > 0 ? (day.completed / day.total) * 100 : 0,
                            backgroundColor: day.completed === day.total ? '#4CAF50' : '#2196F3'
                          }
                        ]} 
                      />
                    </View>
                    <Text style={styles.dayLabel}>
                      {new Date(day.date).toLocaleDateString('tr-TR', { weekday: 'short' })}
                    </Text>
                    <Text style={styles.dayStats}>
                      {day.completed}/{day.total}
                    </Text>
                  </View>
                ))}
              </View>
            </View>
          )}

          {/* Achievement Section */}
          <View style={styles.achievementsContainer}>
            <Text style={styles.sectionTitle}>Başarılar</Text>
            
            <View style={styles.achievementsGrid}>
              <View style={styles.achievementCard}>
                <View style={styles.achievementIcon}>
                  <Ionicons name="star" size={24} color="#FFD700" />
                </View>
                <Text style={styles.achievementTitle}>İlk Adım</Text>
                <Text style={styles.achievementDesc}>İlk rutininizi oluşturun</Text>
              </View>

              <View style={styles.achievementCard}>
                <View style={styles.achievementIcon}>
                  <Ionicons name="checkmark-done" size={24} color="#4CAF50" />
                </View>
                <Text style={styles.achievementTitle}>Tutarlılık</Text>
                <Text style={styles.achievementDesc}>7 gün üst üste rutin tamamlayın</Text>
              </View>

              <View style={styles.achievementCard}>
                <View style={styles.achievementIcon}>
                  <Ionicons name="trophy" size={24} color="#FF9800" />
                </View>
                <Text style={styles.achievementTitle}>Mükemmellik</Text>
                <Text style={styles.achievementDesc}>%100 başarı oranına ulaşın</Text>
              </View>

              <View style={styles.achievementCard}>
                <View style={styles.achievementIcon}>
                  <Ionicons name="flame" size={24} color="#F44336" />
                </View>
                <Text style={styles.achievementTitle}>Ateş Gibi</Text>
                <Text style={styles.achievementDesc}>30 gün üst üste devam edin</Text>
              </View>
            </View>
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
  streakContainer: {
    paddingHorizontal: 20,
    marginBottom: 30,
  },
  streakCard: {
    backgroundColor: 'rgba(255, 255, 255, 0.1)',
    borderRadius: 16,
    padding: 20,
    flexDirection: 'row',
    alignItems: 'center',
  },
  streakIcon: {
    width: 60,
    height: 60,
    borderRadius: 30,
    backgroundColor: 'rgba(255, 107, 107, 0.2)',
    justifyContent: 'center',
    alignItems: 'center',
    marginRight: 20,
  },
  streakInfo: {
    flex: 1,
  },
  streakNumber: {
    fontSize: 32,
    fontWeight: 'bold',
    color: '#fff',
    marginBottom: 4,
  },
  streakLabel: {
    fontSize: 16,
    color: 'rgba(255, 255, 255, 0.9)',
    marginBottom: 4,
  },
  streakDescription: {
    fontSize: 14,
    color: 'rgba(255, 255, 255, 0.7)',
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
  weeklyContainer: {
    paddingHorizontal: 20,
    marginBottom: 30,
  },
  weeklyChart: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'flex-end',
    height: 120,
    backgroundColor: 'rgba(255, 255, 255, 0.1)',
    borderRadius: 12,
    padding: 20,
  },
  dayColumn: {
    alignItems: 'center',
    flex: 1,
  },
  dayBar: {
    width: 20,
    height: 60,
    backgroundColor: 'rgba(255, 255, 255, 0.2)',
    borderRadius: 10,
    overflow: 'hidden',
    marginBottom: 8,
  },
  progressBar: {
    width: '100%',
    backgroundColor: '#4CAF50',
    position: 'absolute',
    bottom: 0,
    borderRadius: 10,
  },
  dayLabel: {
    fontSize: 10,
    color: 'rgba(255, 255, 255, 0.8)',
    marginBottom: 4,
  },
  dayStats: {
    fontSize: 10,
    color: 'rgba(255, 255, 255, 0.6)',
  },
  achievementsContainer: {
    paddingHorizontal: 20,
    paddingBottom: 30,
  },
  achievementsGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    justifyContent: 'space-between',
  },
  achievementCard: {
    width: '48%',
    backgroundColor: 'rgba(255, 255, 255, 0.1)',
    borderRadius: 12,
    padding: 15,
    marginBottom: 15,
    alignItems: 'center',
  },
  achievementIcon: {
    width: 40,
    height: 40,
    borderRadius: 20,
    backgroundColor: 'rgba(255, 255, 255, 0.2)',
    justifyContent: 'center',
    alignItems: 'center',
    marginBottom: 10,
  },
  achievementTitle: {
    fontSize: 14,
    fontWeight: 'bold',
    color: '#fff',
    marginBottom: 4,
    textAlign: 'center',
  },
  achievementDesc: {
    fontSize: 11,
    color: 'rgba(255, 255, 255, 0.7)',
    textAlign: 'center',
    lineHeight: 14,
  },
});

export default StatsScreen; 