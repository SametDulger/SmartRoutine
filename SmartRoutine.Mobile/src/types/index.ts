export interface User {
  id: string;
  email: string;
  userName: string;
  displayName?: string;
  firstName?: string;
  lastName?: string;
  createdAt: string;
}

export interface Routine {
  id: string;
  title: string;
  description?: string;
  isActive: boolean;
  repeatType: 'Daily' | 'Weekly' | 'CustomDays' | 'IntervalBased';
  scheduledTime: string;
  userId: string;
  createdAt: string;
  updatedAt: string;
  isCompletedToday?: boolean;
}

export interface RoutineLog {
  id: string;
  routineId: string;
  completedAt: string;
  userId: string;
}

export interface Stats {
  totalRoutines: number;
  completedToday: number;
  completionRate: number;
  currentStreak: number;
  weeklyStats: WeeklyStats[];
}

export interface WeeklyStats {
  date: string;
  completed: number;
  total: number;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  confirmPassword: string;
  displayName: string;
}

export interface LoginResponse {
  accessToken: string;
  tokenType: string;
  expiresAt: string;
  refreshToken: string;
  user: User;
}

export interface ApiResponse<T> {
  data: T;
  message?: string;
  success: boolean;
}

export interface PagedResult<T> {
  data: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface RoutineCreateRequest {
  title: string;
  description?: string;
  timeOfDay: string;
  repeatType: 'Daily' | 'Weekly' | 'CustomDays' | 'IntervalBased';
} 