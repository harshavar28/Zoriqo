import axios from 'axios';
import type { InternalAxiosRequestConfig } from 'axios';

import {
  clearSession,
  getSession,
  getSessionVersion,
  saveSession,
} from './token-storage';

export class ApiError extends Error {
  constructor(
    message: string,
    public readonly status: number,
  ) {
    super(message);
    this.name = 'ApiError';
  }
}

export type TokenResponse = {
  tokenType: string;
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
};

type ErrorResponse = {
  message?: string;
  errors?: string[] | Record<string, string[]>;
};

type SessionRequest = InternalAxiosRequestConfig & {
  _retried?: boolean;
  _sessionVersion?: number;
};

const settings = {
  baseURL: process.env.EXPO_PUBLIC_API_URL?.replace(/\/+$/, ''),
  timeout: 15000,
  headers: {
    Accept: 'application/json',
  },
};

export const publicApi = axios.create(settings);
export const api = axios.create(settings);

function rejectApiError(error: unknown): Promise<never> {
  if (!axios.isAxiosError<ErrorResponse>(error)) {
    return Promise.reject(error);
  }

  const status = error.response?.status ?? 0;
  const data = error.response?.data;

  let message = 'The request failed. Please try again.';

  if (!error.response) {
    message =
      error.code === 'ECONNABORTED' ||
      error.code === 'ETIMEDOUT'
        ? 'The request took too long. Please try again.'
        : 'Cannot reach Zoriqo. Check your connection.';
  } else if (data?.errors) {
    const messages = Object.values(data.errors)
      .flat()
      .filter(
        (value): value is string => typeof value === 'string',
      );

    message = messages.join('\n') || data.message || message;
  } else if (data?.message) {
    message = data.message;
  } else if (status === 401) {
    message = 'Your session expired. Please sign in again.';
  } else if (status === 403) {
    message = 'You do not have permission for this action.';
  } else if (status === 429) {
    message = 'Too many requests. Please try again shortly.';
  }

  return Promise.reject(new ApiError(message, status));
}

// Check the API address for both clients.
for (const client of [publicApi, api]) {
  client.interceptors.request.use((config) => {
    if (!config.baseURL) {
      throw new ApiError(
        'API address is missing. Check .env.local.',
        0,
      );
    }

    return config;
  });
}

publicApi.interceptors.response.use(
  (response) => response,
  rejectApiError,
);

// Protected requests receive the saved access token.
api.interceptors.request.use(async (originalConfig) => {
  const config = originalConfig as SessionRequest;

  const expectedVersion =
    config._sessionVersion ?? getSessionVersion();

  const session = await getSession();

  if (expectedVersion !== getSessionVersion()) {
    throw new ApiError('Your session changed.', 401);
  }

  if (!session) {
    throw new ApiError('Please sign in again.', 401);
  }

  config._sessionVersion = expectedVersion;
  config.headers.set(
    'Authorization',
    `Bearer ${session.accessToken}`,
  );

  return config;
});

// All simultaneous failures share one refresh request.
let refreshPromise: Promise<void> | null = null;

function renewSession(expectedVersion: number): Promise<void> {
  if (expectedVersion !== getSessionVersion()) {
    return Promise.reject(
      new ApiError('Your session changed.', 401),
    );
  }

  if (!refreshPromise) {
    refreshPromise = (async () => {
      const session = await getSession();

      if (!session) {
        throw new ApiError('Please sign in again.', 401);
      }

      try {
        const response = await publicApi.post<TokenResponse>(
          '/api/auth/refresh',
          {
            refreshToken: session.refreshToken,
          },
        );

        const { accessToken, refreshToken } = response.data;

        if (!accessToken || !refreshToken) {
          throw new Error('The server returned an invalid session.');
        }

        await saveSession(
          { accessToken, refreshToken },
          expectedVersion,
        );
      } catch (error) {
        if (error instanceof ApiError && error.status === 401) {
          await clearSession(expectedVersion);
        }

        // A network failure does not erase the session.
        throw error;
      }
    })().finally(() => {
      refreshPromise = null;
    });
  }

  return refreshPromise;
}

api.interceptors.response.use(
  (response) => response,
  async (error: unknown) => {
    if (!axios.isAxiosError(error)) {
      return Promise.reject(error);
    }

    const config = error.config as SessionRequest | undefined;

    if (
      error.response?.status !== 401 ||
      !config ||
      config._sessionVersion === undefined
    ) {
      return Promise.reject(error);
    }

    if (config._sessionVersion !== getSessionVersion()) {
      throw new ApiError('Your session changed.', 401);
    }

    // Never retry the same request indefinitely.
    if (config._retried) {
      await clearSession(config._sessionVersion);
      throw new ApiError('Please sign in again.', 401);
    }

    config._retried = true;

    const current = await getSession();
    const sentAuthorization = config.headers.get('Authorization');

    // Another request may already have refreshed the token.
    if (
      !current ||
      sentAuthorization === `Bearer ${current.accessToken}`
    ) {
      await renewSession(config._sessionVersion);
    }

    return api.request(config);
  },
);

api.interceptors.response.use(
  (response) => response,
  rejectApiError,
);