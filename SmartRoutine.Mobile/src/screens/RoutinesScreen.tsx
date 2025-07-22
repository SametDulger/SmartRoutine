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
import { useNavigation } from '@react-navigation/native';
import { Routine } from '../types';
import apiService from '../services/apiService';

const RoutinesScreen: React.FC = () => {
  const navigation = useNavigation();
  const [routines, setRoutines] = useState<Routine[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isRefreshing, setIsRefreshing] = useState(false);

  useEffect(() => {
    loadRoutines();
  }, []);

  const loadRoutines = async () => {
    try {
      setIsLoading(true);
      const routines = await apiService.getRoutines();
      setRoutines(routines || []);
    } catch (error: any) {
      console.error('Routines load failed:', error);
      Alert.alert('Hata', 'Rutinler yüklenirken bir hata oluştu');
    } finally {
      setIsLoading(false);
    }
  };

  const handleRefresh = async () => {
    setIsRefreshing(true);
    await loadRoutines();
    setIsRefreshing(false);
  };

  const handleDeleteRoutine = async (routineId: string) => {
    Alert.alert(
      'Rutin Sil',
      'Bu rutini silmek istediğinizden emin misiniz?',
      [
        { text: 'İptal', style: 'cancel' },
        {
          text: 'Sil',
          style: 'destructive',
          onPress: async () => {
            try {
              await apiService.deleteRoutine(routineId);
              Alert.alert('Başarılı', 'Rutin başarıyla silindi');
              loadRoutines();
            } catch (error) {
              Alert.alert('Hata', 'Rutin silinirken bir hata oluştu');
            }
          },
        },
      ]
    );
  };

  const handleCompleteRoutine = async (routineId: string) => {
    try {
      await apiService.completeRoutine(routineId);
      Alert.alert('Başarılı', 'Rutin başarıyla tamamlandı!');
      loadRoutines();
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
              <View style={styles.headerInfo}>
                <Ionicons name="list" size={32} color="#fff" />
                <View style={styles.headerText}>
                  <Text style={styles.title}>Rutinlerim</Text>
                  <Text style={styles.subtitle}>
                    {routines?.length || 0} rutin bulundu
                  </Text>
                </View>
              </View>
            </View>
          </View>

          {/* Routines List */}
          <View style={styles.routinesContainer}>
            {!routines || routines.length === 0 ? (
              <View style={styles.emptyState}>
                <Ionicons name="add-circle-outline" size={64} color="rgba(255,255,255,0.5)" />
                <Text style={styles.emptyText}>Henüz rutin eklenmemiş</Text>
                <Text style={styles.emptySubtext}>Yeni rutin ekleyerek başlayın</Text>
                <TouchableOpacity 
                  style={styles.emptyStateAddButton} 
                  onPress={() => navigation.navigate('AddRoutine' as never)}
                >
                  <Ionicons name="add" size={24} color="#fff" />
                  <Text style={styles.emptyStateAddButtonText}>Rutin Ekle</Text>
                </TouchableOpacity>
              </View>
            ) : (
              routines.map((routine) => (
                <View key={routine.id} style={styles.routineCard}>
                  <View style={styles.routineContent}>
                    <View style={styles.routineInfo}>
                      <View style={styles.routineHeader}>
                        <Text style={styles.routineTitle}>{routine.title}</Text>
                        <View style={styles.routineStatus}>
                          {routine.isActive ? (
                            <View style={styles.activeStatus}>
                              <Ionicons name="checkmark-circle" size={16} color="#4CAF50" />
                              <Text style={styles.activeText}>Aktif</Text>
                            </View>
                          ) : (
                            <View style={styles.inactiveStatus}>
                              <Ionicons name="pause-circle" size={16} color="#FF9800" />
                              <Text style={styles.inactiveText}>Pasif</Text>
                            </View>
                          )}
                        </View>
                      </View>
                      
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
                        
                        <View style={styles.routineDate}>
                          <Ionicons name="calendar" size={16} color="rgba(255,255,255,0.7)" />
                          <Text style={styles.routineDateText}>
                            {new Date(routine.createdAt).toLocaleDateString('tr-TR')}
                          </Text>
                        </View>
                      </View>
                    </View>
                    
                    <View style={styles.routineActions}>
                      <TouchableOpacity
                        style={[
                          styles.actionButton,
                          styles.completeButton,
                          routine.isCompletedToday && styles.completedButton
                        ]}
                        onPress={() => handleCompleteRoutine(routine.id)}
                        disabled={routine.isCompletedToday}
                      >
                        <Ionicons 
                          name={routine.isCompletedToday ? "checkmark-circle" : "checkmark-circle-outline"} 
                          size={20} 
                          color={routine.isCompletedToday ? "#4CAF50" : "#fff"} 
                        />
                      </TouchableOpacity>
                      
                      <TouchableOpacity
                        style={[styles.actionButton, styles.deleteButton]}
                        onPress={() => handleDeleteRoutine(routine.id)}
                      >
                        <Ionicons name="trash-outline" size={20} color="#FF6B6B" />
                      </TouchableOpacity>
                    </View>
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
  routinesContainer: {
    paddingHorizontal: 20,
    paddingBottom: 30,
  },
  emptyState: {
    alignItems: 'center',
    paddingVertical: 60,
  },
  emptyText: {
    fontSize: 18,
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
  routineHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 8,
  },
  routineTitle: {
    fontSize: 16,
    fontWeight: 'bold',
    color: '#fff',
    flex: 1,
  },
  routineStatus: {
    marginLeft: 10,
  },
  activeStatus: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: 'rgba(76, 175, 80, 0.2)',
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: 12,
  },
  activeText: {
    fontSize: 12,
    color: '#4CAF50',
    marginLeft: 4,
    fontWeight: '600',
  },
  inactiveStatus: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: 'rgba(255, 152, 0, 0.2)',
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: 12,
  },
  inactiveText: {
    fontSize: 12,
    color: '#FF9800',
    marginLeft: 4,
    fontWeight: '600',
  },
  routineDescription: {
    fontSize: 14,
    color: 'rgba(255, 255, 255, 0.8)',
    marginBottom: 12,
  },
  routineMeta: {
    flexDirection: 'row',
    alignItems: 'center',
    flexWrap: 'wrap',
  },
  routineTime: {
    flexDirection: 'row',
    alignItems: 'center',
    marginRight: 15,
    marginBottom: 4,
  },
  routineTimeText: {
    fontSize: 12,
    color: 'rgba(255, 255, 255, 0.7)',
    marginLeft: 4,
  },
  routineType: {
    flexDirection: 'row',
    alignItems: 'center',
    marginRight: 15,
    marginBottom: 4,
  },
  routineTypeText: {
    fontSize: 12,
    color: 'rgba(255, 255, 255, 0.7)',
    marginLeft: 4,
  },
  routineDate: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 4,
  },
  routineDateText: {
    fontSize: 12,
    color: 'rgba(255, 255, 255, 0.7)',
    marginLeft: 4,
  },
  routineActions: {
    flexDirection: 'row',
    alignItems: 'center',
    marginLeft: 15,
  },
  actionButton: {
    width: 40,
    height: 40,
    borderRadius: 20,
    justifyContent: 'center',
    alignItems: 'center',
    marginLeft: 8,
  },
  completeButton: {
    backgroundColor: 'rgba(255, 255, 255, 0.2)',
  },
  completedButton: {
    backgroundColor: 'rgba(76, 175, 80, 0.2)',
  },
  deleteButton: {
    backgroundColor: 'rgba(255, 107, 107, 0.2)',
  },
  addButton: {
    width: 44,
    height: 44,
    borderRadius: 22,
    backgroundColor: 'rgba(255, 255, 255, 0.2)',
    justifyContent: 'center',
    alignItems: 'center',
  },
  emptyStateAddButton: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: 'rgba(255, 255, 255, 0.2)',
    borderRadius: 25,
    paddingHorizontal: 20,
    paddingVertical: 12,
    marginTop: 20,
  },
  emptyStateAddButtonText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: '600',
    marginLeft: 8,
  },
});

export default RoutinesScreen; 