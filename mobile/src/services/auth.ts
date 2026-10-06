import {
  api,
  ApiError,
  publicApi,
} from './api';
import type { TokenResponse } from './api';

import {
  clearSession,
  getSession,
  getSessionVersion,
  saveSession,
} from './token-storage';

export type UserProfile = {
  id: string;
  fullName: string;
  email: string;
};

export async function register(
  fullName: string,
  email: string,
  password: string,
): Promise<void> {
  await publicApi.post('/api/auth/register', {
    fullName: fullName.trim(),
    email: email.trim(),
    password,
  });
}

export async function signIn(
  email: string,
  password: string,
): Promise<UserProfile> {
  await clearSession();
  const expectedVersion = getSessionVersion();

  const response = await publicApi.post<TokenResponse>(
    '/api/auth/login',
    {
      email: email.trim(),
      password,
    },
  );

  const { accessToken, refreshToken } = response.data;

  if (!accessToken || !refreshToken) {
    throw new Error('Sign-in returned an unexpected response.');
  }

  await saveSession(
    { accessToken, refreshToken },
    expectedVersion,
  );

  return getCurrentUser();
}

export async function getCurrentUser(): Promise<UserProfile> {
  // api attaches the token and refreshes it if necessary.
  const response = await api.get<UserProfile>('/api/auth/me');

  return response.data;
}

export async function restoreSession(): Promise<UserProfile | null> {
  if (!(await getSession())) {
    return null;
  }

  try {
    return await getCurrentUser();
  } catch (error) {
    if (error instanceof ApiError && error.status === 401) {
      return null;
    }

    throw error;
  }
}

export async function signOut(): Promise<void> {
  await clearSession();
}