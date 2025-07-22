import axios, { AxiosInstance, AxiosResponse } from 'axios';
import AsyncStorage from '@react-native-async-storage/async-storage';
import {
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  Routine,
  Stats,
  ApiResponse,
  PagedResult,
  User,
  RoutineCreateRequest
} from '../types';

const API_BASE_URL = 'http://192.168.1.100:5000/api';

class ApiService {
  private api: AxiosInstance;

  constructor() {
    this.api = axios.create({
      baseURL: API_BASE_URL,
      timeout: 10000,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    // Request interceptor to add auth token
    this.api.interceptors.request.use(
      async (config) => {
        const token = await AsyncStorage.getItem('accessToken');
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => {
        return Promise.reject(error);
      }
    );

    // Response interceptor to handle token refresh
    this.api.interceptors.response.use(
      (response) => response,
      async (error) => {
        const originalRequest = error.config;
        if (error.response?.status === 401 && !originalRequest._retry) {
          originalRequest._retry = true;
          try {
            const refreshToken = await AsyncStorage.getItem('refreshToken');
            if (refreshToken) {
              const response = await this.refreshToken(refreshToken);
              await AsyncStorage.setItem('accessToken', response.accessToken);
              originalRequest.headers.Authorization = `Bearer ${response.accessToken}`;
              return this.api(originalRequest);
            }
          } catch (refreshError) {
            await AsyncStorage.multiRemove(['accessToken', 'refreshToken', 'user']);
          }
        }
        return Promise.reject(error);
      }
    );
  }

  // Auth endpoints
  async login(credentials: LoginRequest): Promise<LoginResponse> {
    const response: AxiosResponse<LoginResponse> = await this.api.post('/auth/login', credentials);
    return response.data;
  }

  async register(userData: RegisterRequest): Promise<ApiResponse<User>> {
    const response: AxiosResponse<ApiResponse<User>> = await this.api.post('/auth/register', userData);
    return response.data;
  }

  async refreshToken(refreshToken: string): Promise<LoginResponse> {
    const response: AxiosResponse<LoginResponse> = await this.api.post('/auth/refresh', { refreshToken });
    return response.data;
  }

  async getCurrentUser(): Promise<User> {
    const response: AxiosResponse<User> = await this.api.get('/auth/me');
    return response.data;
  }

  async updateProfile(profileData: {
    displayName?: string;
    email?: string;
    currentPassword?: string;
    newPassword?: string;
  }): Promise<User> {
    const response: AxiosResponse<User> = await this.api.put('/auth/profile', profileData);
    return response.data;
  }

  async changePassword(currentPassword: string, newPassword: string): Promise<User> {
    const response: AxiosResponse<User> = await this.api.put('/auth/profile', {
      currentPassword,
      newPassword
    });
    return response.data;
  }

  // Routines endpoints
  async getRoutines(): Promise<Routine[]> {
    const response: AxiosResponse<Routine[]> = await this.api.get('/routines/today');
    return response.data;
  }

  async getTodayRoutines(): Promise<Routine[]> {
    const response: AxiosResponse<Routine[]> = await this.api.get('/routines/today');
    return response.data;
  }

  async createRoutine(routine: RoutineCreateRequest): Promise<Routine> {
    const response: AxiosResponse<Routine> = await this.api.post('/routines', routine);
    return response.data;
  }

  async updateRoutine(id: string, routine: Partial<Routine>): Promise<Routine> {
    const response: AxiosResponse<Routine> = await this.api.put(`/routines/${id}`, routine);
    return response.data;
  }

  async deleteRoutine(id: string): Promise<void> {
    await this.api.delete(`/routines/${id}`);
  }

  async completeRoutine(id: string): Promise<ApiResponse<string>> {
    const response: AxiosResponse<ApiResponse<string>> = await this.api.post(`/routines/${id}/complete`);
    return response.data;
  }

  // Stats endpoints
  async getStats(): Promise<Stats> {
    const response: AxiosResponse<Stats> = await this.api.get('/stats/summary');
    return response.data;
  }

  async getWeeklyStats(): Promise<Stats> {
    // API'de weekly endpoint yok, summary kullanıyoruz
    const response: AxiosResponse<Stats> = await this.api.get('/stats/summary');
    return response.data;
  }

  async getStreak(): Promise<number> {
    const response: AxiosResponse<{ streak: number }> = await this.api.get('/stats/streak');
    return response.data.streak;
  }
}

export default new ApiService(); 