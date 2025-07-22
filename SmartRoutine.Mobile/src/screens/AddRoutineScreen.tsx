import React, { useState, useEffect } from 'react';
import {
  View,
  StyleSheet,
  ScrollView,
  Alert,
  TouchableOpacity,
  Text,
  StatusBar,
  TextInput,
  Platform,
  RefreshControl,
} from 'react-native';

import { LinearGradient } from 'expo-linear-gradient';
import { Ionicons } from '@expo/vector-icons';
import { useNavigation } from '@react-navigation/native';
import apiService from '../services/apiService';
import { Routine } from '../types';

const AddRoutineScreen: React.FC = () => {
  const navigation = useNavigation();
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [timeOfDay, setTimeOfDay] = useState('09:00');
  const [repeatType, setRepeatType] = useState<'Daily' | 'Weekly' | 'CustomDays' | 'IntervalBased'>('Daily');
  const [isLoading, setIsLoading] = useState(false);
  const [routines, setRoutines] = useState<Routine[]>([]);
  const [isLoadingRoutines, setIsLoadingRoutines] = useState(true);
  const [isRefreshing, setIsRefreshing] = useState(false);

  useEffect(() => {
    loadRoutines();
  }, []);

  const loadRoutines = async () => {
    try {
      setIsLoadingRoutines(true);
      const routinesData = await apiService.getRoutines();
      setRoutines(routinesData || []);
    } catch (error: any) {
      console.error('Error loading routines:', error);
      Alert.alert('Hata', 'Rutinler yüklenirken bir hata oluştu');
    } finally {
      setIsLoadingRoutines(false);
    }
  };

  const handleRefresh = async () => {
    setIsRefreshing(true);
    await loadRoutines();
    setIsRefreshing(false);
  };

  const handleTimeSelect = () => {
    if (Platform.OS === 'ios') {
      // iOS için Alert.prompt kullanarak saat girişi
      Alert.prompt(
        'Saat Seçin',
        'Saat formatı: HH:MM (örn: 09:30)',
        [
          { text: 'İptal', style: 'cancel' },
          {
            text: 'Tamam',
            onPress: (newTime) => {
              if (newTime && /^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$/.test(newTime)) {
                setTimeOfDay(newTime);
              } else {
                Alert.alert('Hata', 'Lütfen geçerli bir saat formatı girin (HH:MM)');
              }
            }
          }
        ],
        'plain-text',
        timeOfDay
      );
    } else {
      // Android için basit input
      Alert.prompt(
        'Saat Seçin',
        'Saat formatı: HH:MM (örn: 09:30)',
        [
          { text: 'İptal', style: 'cancel' },
          {
            text: 'Tamam',
            onPress: (newTime) => {
              if (newTime && /^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$/.test(newTime)) {
                setTimeOfDay(newTime);
              } else {
                Alert.alert('Hata', 'Lütfen geçerli bir saat formatı girin (HH:MM)');
              }
            }
          }
        ],
        'plain-text',
        timeOfDay
      );
    }
  };

  const handleSave = async () => {
    if (!title.trim()) {
      Alert.alert('Hata', 'Rutin başlığı gereklidir');
      return;
    }

    try {
      setIsLoading(true);
      const routineData = {
        title: title.trim(),
        description: description.trim(),
        timeOfDay: timeOfDay,
        repeatType,
      };
      await apiService.createRoutine(routineData);
      Alert.alert('Başarılı', 'Rutin başarıyla oluşturuldu', [
        { text: 'Tamam', onPress: () => navigation.goBack() }
      ]);
    } catch (error: any) {
      console.error('Routine creation failed:', error);
      console.error('Error response:', error.response?.data);
      const errorMessage = error.response?.data?.message || error.response?.data?.details || 'Rutin oluşturulurken bir hata oluştu';
      Alert.alert('Hata', errorMessage);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <View style={styles.container}>
      <StatusBar barStyle="light-content" backgroundColor="transparent" translucent />
      
      <LinearGradient
        colors={['#667eea', '#764ba2']}
        style={styles.gradient}
      >
        {/* Header */}
        <View style={styles.header}>
          <TouchableOpacity
            style={styles.backButton}
            onPress={() => navigation.goBack()}
          >
            <Ionicons name="arrow-back" size={24} color="#fff" />
          </TouchableOpacity>
          <Text style={styles.headerTitle}>Yeni Rutin</Text>
          <TouchableOpacity
            style={[styles.saveButton, isLoading && styles.saveButtonDisabled]}
            onPress={handleSave}
            disabled={isLoading}
          >
            <Text style={styles.saveButtonText}>
              {isLoading ? 'Kaydediliyor...' : 'Kaydet'}
            </Text>
          </TouchableOpacity>
        </View>

        <ScrollView style={styles.scrollView}>
          {/* Form */}
          <View style={styles.formContainer}>
            {/* Title Input */}
            <View style={styles.inputGroup}>
              <Text style={styles.inputLabel}>Rutin Başlığı *</Text>
              <View style={styles.inputContainer}>
                <Ionicons name="create-outline" size={20} color="rgba(255,255,255,0.7)" />
                <TextInput
                  style={styles.textInput}
                  value={title}
                  onChangeText={setTitle}
                  placeholder="Örn: Sabah Egzersizi"
                  placeholderTextColor="rgba(255,255,255,0.5)"
                />
              </View>
            </View>

            {/* Description Input */}
            <View style={styles.inputGroup}>
              <Text style={styles.inputLabel}>Açıklama</Text>
              <View style={styles.inputContainer}>
                <Ionicons name="document-text-outline" size={20} color="rgba(255,255,255,0.7)" />
                <TextInput
                  style={[styles.textInput, styles.textArea]}
                  value={description}
                  onChangeText={setDescription}
                  placeholder="Rutin hakkında açıklama ekleyin"
                  placeholderTextColor="rgba(255,255,255,0.5)"
                  multiline
                  numberOfLines={3}
                />
              </View>
            </View>

            {/* Time Input */}
            <View style={styles.inputGroup}>
              <Text style={styles.inputLabel}>Saat</Text>
              <TouchableOpacity
                style={styles.timeInputContainer}
                onPress={handleTimeSelect}
                activeOpacity={0.7}
              >
                <Ionicons name="time-outline" size={20} color="rgba(255,255,255,0.7)" />
                <Text style={styles.timeInputText}>
                  {timeOfDay}
                </Text>
                <Ionicons name="chevron-down" size={16} color="rgba(255,255,255,0.5)" />
              </TouchableOpacity>
            </View>

            {/* Repeat Type */}
            <View style={styles.inputGroup}>
              <Text style={styles.inputLabel}>Tekrar Türü</Text>
              <View style={styles.repeatTypeContainer}>
                <TouchableOpacity
                  style={[
                    styles.repeatTypeButton,
                    repeatType === 'Daily' && styles.repeatTypeButtonActive
                  ]}
                  onPress={() => setRepeatType('Daily')}
                >
                  <Ionicons 
                    name="calendar-outline" 
                    size={20} 
                    color={repeatType === 'Daily' ? '#fff' : 'rgba(255,255,255,0.7)'} 
                  />
                  <Text style={[
                    styles.repeatTypeText,
                    repeatType === 'Daily' && styles.repeatTypeTextActive
                  ]}>
                    Günlük
                  </Text>
                </TouchableOpacity>

                <TouchableOpacity
                  style={[
                    styles.repeatTypeButton,
                    repeatType === 'Weekly' && styles.repeatTypeButtonActive
                  ]}
                  onPress={() => setRepeatType('Weekly')}
                >
                  <Ionicons 
                    name="calendar-outline" 
                    size={20} 
                    color={repeatType === 'Weekly' ? '#fff' : 'rgba(255,255,255,0.7)'} 
                  />
                  <Text style={[
                    styles.repeatTypeText,
                    repeatType === 'Weekly' && styles.repeatTypeTextActive
                  ]}>
                    Haftalık
                  </Text>
                </TouchableOpacity>

                <TouchableOpacity
                  style={[
                    styles.repeatTypeButton,
                    repeatType === 'CustomDays' && styles.repeatTypeButtonActive
                  ]}
                  onPress={() => setRepeatType('CustomDays')}
                >
                  <Ionicons 
                    name="calendar-outline" 
                    size={20} 
                    color={repeatType === 'CustomDays' ? '#fff' : 'rgba(255,255,255,0.7)'} 
                  />
                  <Text style={[
                    styles.repeatTypeText,
                    repeatType === 'CustomDays' && styles.repeatTypeTextActive
                  ]}>
                    Özel Günler
                  </Text>
                </TouchableOpacity>
              </View>
            </View>

            {/* Routines List */}
            <View style={styles.routinesSection}>
              <Text style={styles.routinesSectionTitle}>Mevcut Rutinler</Text>
              {isLoadingRoutines ? (
                <View style={styles.loadingContainer}>
                  <Text style={styles.loadingText}>Rutinler yükleniyor...</Text>
                </View>
              ) : routines && routines.length > 0 ? (
                <View style={styles.routinesList}>
                  {routines.map((routine) => (
                    <View key={routine.id} style={styles.routineItem}>
                      <View style={styles.routineItemContent}>
                        <Text style={styles.routineItemTitle}>{routine.title}</Text>
                        {routine.description && (
                          <Text style={styles.routineItemDescription}>{routine.description}</Text>
                        )}
                        <View style={styles.routineItemMeta}>
                          <Text style={styles.routineItemTime}>
                            {new Date(routine.scheduledTime).toLocaleTimeString('tr-TR', {
                              hour: '2-digit',
                              minute: '2-digit',
                            })}
                          </Text>
                          <Text style={styles.routineItemType}>
                            {routine.repeatType === 'Daily' ? 'Günlük' : 
                             routine.repeatType === 'Weekly' ? 'Haftalık' : 
                             routine.repeatType === 'CustomDays' ? 'Özel Günler' : 'Aralıklı'}
                          </Text>
                        </View>
                      </View>
                    </View>
                  ))}
                </View>
              ) : (
                <View style={styles.emptyRoutines}>
                  <Text style={styles.emptyRoutinesText}>Henüz rutin eklenmemiş</Text>
                </View>
              )}
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
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: 20,
    paddingTop: 50,
    paddingBottom: 15,
  },
  backButton: {
    width: 40,
    height: 40,
    borderRadius: 20,
    backgroundColor: 'rgba(255, 255, 255, 0.2)',
    justifyContent: 'center',
    alignItems: 'center',
  },
  headerTitle: {
    fontSize: 18,
    fontWeight: 'bold',
    color: '#fff',
  },
  saveButton: {
    backgroundColor: 'rgba(76, 175, 80, 0.8)',
    paddingHorizontal: 20,
    paddingVertical: 10,
    borderRadius: 20,
  },
  saveButtonDisabled: {
    backgroundColor: 'rgba(255, 255, 255, 0.3)',
  },
  saveButtonText: {
    color: '#fff',
    fontWeight: '600',
    fontSize: 14,
  },
  scrollView: {
    flex: 1,
  },
  formContainer: {
    paddingHorizontal: 20,
    paddingBottom: 30,
  },
  inputGroup: {
    marginBottom: 25,
  },
  inputLabel: {
    fontSize: 16,
    fontWeight: '600',
    color: '#fff',
    marginBottom: 10,
  },
  inputContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: 'rgba(255, 255, 255, 0.1)',
    borderRadius: 12,
    paddingHorizontal: 15,
    paddingVertical: 12,
  },
  textInput: {
    flex: 1,
    color: '#fff',
    fontSize: 16,
    marginLeft: 10,
  },
  textArea: {
    height: 80,
    textAlignVertical: 'top',
  },
  timeInputContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: 'rgba(255, 255, 255, 0.1)',
    borderRadius: 12,
    paddingHorizontal: 15,
    paddingVertical: 12,
  },
  timeInputText: {
    flex: 1,
    color: '#fff',
    fontSize: 16,
    marginLeft: 10,
    fontWeight: '500',
  },
  repeatTypeContainer: {
    flexDirection: 'row',
    justifyContent: 'space-between',
  },
  repeatTypeButton: {
    flex: 1,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: 'rgba(255, 255, 255, 0.1)',
    borderRadius: 12,
    paddingVertical: 15,
    marginHorizontal: 5,
  },
  repeatTypeButtonActive: {
    backgroundColor: 'rgba(76, 175, 80, 0.8)',
  },
  repeatTypeText: {
    color: 'rgba(255, 255, 255, 0.7)',
    fontSize: 14,
    fontWeight: '500',
    marginLeft: 5,
  },
  repeatTypeTextActive: {
    color: '#fff',
  },
  routinesSection: {
    marginTop: 30,
    paddingTop: 20,
    borderTopWidth: 1,
    borderTopColor: 'rgba(255, 255, 255, 0.2)',
  },
  routinesSectionTitle: {
    fontSize: 18,
    fontWeight: 'bold',
    color: '#fff',
    marginBottom: 15,
  },
  loadingContainer: {
    alignItems: 'center',
    paddingVertical: 20,
  },
  loadingText: {
    color: 'rgba(255, 255, 255, 0.7)',
    fontSize: 14,
  },
  routinesList: {
    gap: 10,
  },
  routineItem: {
    backgroundColor: 'rgba(255, 255, 255, 0.1)',
    borderRadius: 12,
    padding: 15,
    marginBottom: 10,
  },
  routineItemContent: {
    gap: 8,
  },
  routineItemTitle: {
    fontSize: 16,
    fontWeight: '600',
    color: '#fff',
  },
  routineItemDescription: {
    fontSize: 14,
    color: 'rgba(255, 255, 255, 0.7)',
  },
  routineItemMeta: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  routineItemTime: {
    fontSize: 12,
    color: 'rgba(255, 255, 255, 0.6)',
  },
  routineItemType: {
    fontSize: 12,
    color: 'rgba(255, 255, 255, 0.6)',
  },
  emptyRoutines: {
    alignItems: 'center',
    paddingVertical: 20,
  },
  emptyRoutinesText: {
    color: 'rgba(255, 255, 255, 0.5)',
    fontSize: 14,
  },
});

export default AddRoutineScreen; 