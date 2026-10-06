import * as SecureStore from 'expo-secure-store';
import { Platform } from 'react-native';

export type SessionTokens = {
  accessToken: string;
  refreshToken: string;
};

const SESSION_KEY = 'zoriqo.session.v1';
const OLD_TOKEN_KEY = 'zoriqo.accessToken';

let browserSession: string | null = null;
let version = 0;
let queue: Promise<unknown> = Promise.resolve();

function sequential<T>(
  operation: () => Promise<T>,
): Promise<T> {
  const result = queue.then(operation);

  queue = result.then(
    () => undefined,
    () => undefined,
  );

  return result;
}

export function getSessionVersion(): number {
  return version;
}

export function getSession(): Promise<SessionTokens | null> {
  return sequential(async () => {
    const saved =
      Platform.OS === 'web'
        ? browserSession
        : await SecureStore.getItemAsync(SESSION_KEY);

    if (!saved) return null;

    try {
      const data = JSON.parse(saved);

      if (
        typeof data?.accessToken === 'string' &&
        typeof data?.refreshToken === 'string'
      ) {
        return data;
      }

      return null;
    } catch {
      return null;
    }
  });
}

export function saveSession(
  session: SessionTokens,
  expectedVersion: number,
): Promise<void> {
  return sequential(async () => {
    if (version !== expectedVersion) {
      throw new Error('Session changed. Please sign in again.');
    }

    const value = JSON.stringify(session);

    if (Platform.OS === 'web') {
      browserSession = value;
    } else {
      await SecureStore.setItemAsync(SESSION_KEY, value);
    }

    if (version !== expectedVersion) {
      throw new Error('Session changed. Please sign in again.');
    }
  });
}

export function clearSession(
  expectedVersion = version,
): Promise<void> {
  if (expectedVersion !== version) {
    return Promise.resolve();
  }

  version += 1;

  return sequential(async () => {
    browserSession = null;

    if (Platform.OS !== 'web') {
      await SecureStore.deleteItemAsync(SESSION_KEY);
      await SecureStore.deleteItemAsync(OLD_TOKEN_KEY);
    }
  });
}