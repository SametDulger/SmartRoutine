import React from 'react';
import { NavigationContainer } from '@react-navigation/native';
import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';
import { createStackNavigator } from '@react-navigation/stack';
import { Provider as PaperProvider } from 'react-native-paper';
import { Ionicons } from '@expo/vector-icons';
import { Platform, StatusBar, ActivityIndicator, View } from 'react-native';

// Screens
import DashboardScreen from './src/screens/DashboardScreen';
import RoutinesScreen from './src/screens/RoutinesScreen';
import StatsScreen from './src/screens/StatsScreen';
import ProfileScreen from './src/screens/ProfileScreen';
import LoginScreen from './src/screens/LoginScreen';
import RegisterScreen from './src/screens/RegisterScreen';
import WelcomeScreen from './src/screens/WelcomeScreen';
import AddRoutineScreen from './src/screens/AddRoutineScreen';

// Context
import { AuthProvider, useAuth } from './src/context/AuthContext';

const Tab = createBottomTabNavigator();
const Stack = createStackNavigator();

function TabNavigator() {
  return (
    <Tab.Navigator
      screenOptions={({ route }) => ({
        tabBarIcon: ({ focused, color, size }) => {
          let iconName: keyof typeof Ionicons.glyphMap;

          if (route.name === 'Dashboard') {
            iconName = focused ? 'home' : 'home-outline';
          } else if (route.name === 'Routines') {
            iconName = focused ? 'list' : 'list-outline';
          } else if (route.name === 'Stats') {
            iconName = focused ? 'stats-chart' : 'stats-chart-outline';
          } else if (route.name === 'Profile') {
            iconName = focused ? 'person' : 'person-outline';
          } else {
            iconName = 'help-outline';
          }

          return <Ionicons name={iconName} size={size} color={color} />;
        },
        tabBarActiveTintColor: Platform.OS === 'ios' ? '#007AFF' : '#007AFF',
        tabBarInactiveTintColor: 'gray',
        tabBarStyle: {
          height: Platform.OS === 'ios' ? 88 : 60,
          paddingBottom: Platform.OS === 'ios' ? 20 : 8,
          paddingTop: Platform.OS === 'ios' ? 8 : 8,
          backgroundColor: 'rgba(255, 255, 255, 0.95)',
          borderTopWidth: 0,
          elevation: 0,
          shadowOpacity: 0,
        },
        headerStyle: {
          backgroundColor: Platform.OS === 'ios' ? '#fff' : '#007AFF',
        },
        headerTintColor: Platform.OS === 'ios' ? '#007AFF' : '#fff',
        headerTitleStyle: {
          fontWeight: 'bold',
        },
      })}
    >
      <Tab.Screen 
        name="Dashboard" 
        component={DashboardScreen}
        options={{
          title: 'Ana Sayfa',
          headerShown: false,
        }}
      />
      <Tab.Screen 
        name="Routines" 
        component={RoutinesScreen}
        options={{
          title: 'Rutinler',
          headerShown: false,
        }}
      />
      <Tab.Screen 
        name="Stats" 
        component={StatsScreen}
        options={{
          title: 'İstatistikler',
          headerShown: false,
        }}
      />
      <Tab.Screen 
        name="Profile" 
        component={ProfileScreen}
        options={{
          title: 'Profil',
          headerShown: false,
        }}
      />
    </Tab.Navigator>
  );
}

function AuthNavigator() {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return (
      <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center' }}>
        <ActivityIndicator size="large" color="#007AFF" />
      </View>
    );
  }

  return (
    <Stack.Navigator screenOptions={{ headerShown: false }}>
      {isAuthenticated ? (
        <>
          <Stack.Screen name="Main" component={TabNavigator} />
          <Stack.Screen name="AddRoutine" component={AddRoutineScreen} />
        </>
      ) : (
        <>
          <Stack.Screen name="Welcome" component={WelcomeScreen} />
          <Stack.Screen name="Login" component={LoginScreen} />
          <Stack.Screen name="Register" component={RegisterScreen} />
        </>
      )}
    </Stack.Navigator>
  );
}

export default function App() {
  return (
    <PaperProvider>
      <StatusBar 
        barStyle="light-content"
        backgroundColor="transparent"
        translucent
        hidden={false}
      />
      <AuthProvider>
        <NavigationContainer>
          <AuthNavigator />
        </NavigationContainer>
      </AuthProvider>
    </PaperProvider>
  );
}
