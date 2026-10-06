import { useEffect, useState } from 'react';
import {
  ActivityIndicator,
  Image,
  Keyboard,
  KeyboardAvoidingView,
  Platform,
  Pressable,
  ScrollView,
  Text,
  TextInput,
  View,
} from 'react-native';
import type { TextInputProps } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import {
  register,
  restoreSession,
  signIn,
  signOut,
} from '../services/auth';
import type { UserProfile } from '../services/auth';
import { styles } from '../styles/auth.styles';

const logo = require('../../assets/brand/zoriqo-logo.png');
const eye = require('../../assets/icons/eye.png');
const eyeOff = require('../../assets/icons/eye-off.png');

type Mode = 'login' | 'register';

type InputFieldProps = TextInputProps & {
  label: string;
  passwordField?: boolean;
};

// Shared input for name, email, and password fields.
function InputField({
  label,
  passwordField = false,
  editable = true,
  ...inputProps
}: InputFieldProps) {
  const [visible, setVisible] = useState(false);

  return (
    <View style={styles.field}>
      <Text style={styles.label}>{label}</Text>

      <View style={styles.inputRow}>
        <TextInput
          {...inputProps}
          editable={editable}
          style={styles.input}
          accessibilityLabel={label}
          placeholderTextColor="#7A7461"
          secureTextEntry={passwordField && !visible}
        />

        {passwordField && (
          <Pressable
            onPress={() => setVisible((current) => !current)}
            disabled={!editable}
            style={styles.eyeButton}
            accessibilityRole="button"
            accessibilityLabel={
              visible ? `Hide ${label}` : `Show ${label}`
            }
          >
            <Image
              source={visible ? eyeOff : eye}
              style={styles.eye}
            />
          </Pressable>
        )}
      </View>
    </View>
  );
}

export default function AccountScreen() {
  const [mode, setMode] = useState<Mode>('login');

  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmation, setConfirmation] = useState('');

  const [error, setError] = useState('');
  const [notice, setNotice] = useState('');
  const [busy, setBusy] = useState(false);
  const [checkingSession, setCheckingSession] = useState(true);

  const [user, setUser] = useState<UserProfile | null>(null);

  const registering = mode === 'register';

  // Check the saved Android token when this screen opens.
  useEffect(() => {
    let active = true;

    async function restore() {
      try {
        const savedUser = await restoreSession();

        if (active) {
          setUser(savedUser);
        }
      } catch (caught) {
        if (active) {
          setError(
            caught instanceof Error
              ? caught.message
              : 'Unable to restore your session.',
          );
        }
      } finally {
        if (active) {
          setCheckingSession(false);
        }
      }
    }

    void restore();

    return () => {
      active = false;
    };
  }, []);

  function changeMode(nextMode: Mode) {
    if (busy || nextMode === mode) return;

    setMode(nextMode);
    setError('');
    setNotice('');
    setPassword('');
    setConfirmation('');
  }

  function validateForm(): string | null {
    if (
      registering &&
      (name.trim().length < 2 || name.trim().length > 100)
    ) {
      return 'Your name must contain between 2 and 100 characters.';
    }

    if (
      email.trim().length > 254 ||
      !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.trim())
    ) {
      return 'Please enter a valid email address.';
    }

    if (!password) {
      return 'Please enter your password.';
    }

    if (password.length > 128) {
      return 'Your password must not exceed 128 characters.';
    }

    if (registering && password.length < 8) {
      return 'Use at least 8 characters for your password.';
    }

    if (registering && password !== confirmation) {
      return 'Your passwords do not match.';
    }

    return null;
  }

  async function submit() {
    if (busy || checkingSession) return;

    setError('');
    setNotice('');

    const validationError = validateForm();

    if (validationError) {
      setError(validationError);
      return;
    }

    Keyboard.dismiss();
    setBusy(true);

    try {
      if (registering) {
        await register(name, email, password);

        setMode('login');
        setPassword('');
        setConfirmation('');
        setNotice('Account created. Sign in to continue.');
      } else {
        const profile = await signIn(email, password);

        setPassword('');
        setConfirmation('');
        setUser(profile);
      }
    } catch (caught) {
  console.error('Sign-in/register failed:', caught);

  setError(
    caught instanceof Error
      ? caught.message
      : 'Something went wrong. Please try again.',
  );
} finally {
  setBusy(false);
}
  }

  async function handleSignOut() {
    if (busy) return;

    setBusy(true);
    setError('');

    try {
      await signOut();

      setUser(null);
      setMode('login');
      setName('');
      setEmail('');
      setPassword('');
      setConfirmation('');
      setNotice('');
    } catch {
      setError('Unable to sign out. Please try again.');
    } finally {
      setBusy(false);
    }
  }

  if (checkingSession) {
    return (
      <SafeAreaView style={styles.page}>
        <View style={styles.loading}>
          <ActivityIndicator size="large" color="#0E5C48" />
          <Text style={styles.loadingText}>Opening Zoriqo…</Text>
        </View>
      </SafeAreaView>
    );
  }

  // Temporary screen shown after successful login.
  if (user) {
    return (
      <SafeAreaView style={styles.page}>
        <ScrollView contentContainerStyle={styles.scroll}>
          <View style={styles.content}>
            <View style={styles.card}>
              <View style={styles.profile}>
                <Image
                  source={logo}
                  style={styles.logo}
                  resizeMode="contain"
                  accessibilityLabel="Zoriqo"
                />

                <Text style={styles.profileName}>
                  Welcome, {user.fullName}
                </Text>

                <Text style={styles.profileEmail}>
                  {user.email}
                </Text>
              </View>

              <Text style={styles.notice}>You’re signed in.</Text>

              {!!error && (
                <Text
                  style={styles.error}
                  accessibilityLiveRegion="polite"
                >
                  {error}
                </Text>
              )}

              <Pressable
                onPress={handleSignOut}
                disabled={busy}
                accessibilityRole="button"
                accessibilityState={{ disabled: busy, busy }}
                style={({ pressed }) => [
                  styles.submit,
                  (pressed || busy) && styles.pressed,
                ]}
              >
                <Text style={styles.submitText}>
                  {busy ? 'Signing out…' : 'Sign out'}
                </Text>
              </Pressable>
            </View>
          </View>
        </ScrollView>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.page}>
      <KeyboardAvoidingView
        style={styles.page}
        behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
      >
        <ScrollView
          contentContainerStyle={styles.scroll}
          keyboardShouldPersistTaps="handled"
        >
          <View style={styles.content}>
            <View style={styles.brand}>
              <Image
                source={logo}
                style={styles.logo}
                resizeMode="contain"
                accessibilityLabel="Zoriqo"
              />

              <Text style={styles.brandName}>Zoriqo</Text>

              <Text style={styles.tagline}>
                Every skill deserves to shine.
              </Text>
            </View>

            <View style={styles.card}>
              <View style={styles.switch}>
                {(['login', 'register'] as const).map((item) => (
                  <Pressable
                    key={item}
                    onPress={() => changeMode(item)}
                    disabled={busy}
                    accessibilityRole="button"
                    accessibilityState={{
                      selected: mode === item,
                      disabled: busy,
                    }}
                    style={[
                      styles.switchItem,
                      mode === item && styles.switchSelected,
                    ]}
                  >
                    <Text
                      style={[
                        styles.switchText,
                        mode === item && styles.switchTextSelected,
                      ]}
                    >
                      {item === 'login' ? 'Sign in' : 'Create account'}
                    </Text>
                  </Pressable>
                ))}
              </View>

              <Text style={styles.heading}>
                {registering ? 'Your next chapter.' : 'Welcome back.'}
              </Text>

              <Text style={styles.description}>
                {registering
                  ? 'Create your account and discover new possibilities.'
                  : 'Sign in to continue your Zoriqo journey.'}
              </Text>

              {!!notice && (
                <Text
                  style={styles.notice}
                  accessibilityLiveRegion="polite"
                >
                  {notice}
                </Text>
              )}

              {/* Changing mode also resets password visibility. */}
              <View key={mode}>
                {registering && (
                  <InputField
                    label="Full name"
                    value={name}
                    onChangeText={setName}
                    placeholder="Your name"
                    autoCapitalize="words"
                    autoComplete="name"
                    maxLength={100}
                    editable={!busy}
                  />
                )}

                <InputField
                  label="Email"
                  value={email}
                  onChangeText={setEmail}
                  placeholder="you@example.com"
                  keyboardType="email-address"
                  autoCapitalize="none"
                  autoCorrect={false}
                  autoComplete="email"
                  maxLength={254}
                  editable={!busy}
                />

                <InputField
                  label="Password"
                  passwordField
                  value={password}
                  onChangeText={setPassword}
                  placeholder={
                    registering ? 'At least 8 characters' : 'Your password'
                  }
                  autoCapitalize="none"
                  autoCorrect={false}
                  autoComplete={
                    registering ? 'new-password' : 'current-password'
                  }
                  returnKeyType={registering ? 'next' : 'done'}
                  onSubmitEditing={registering ? undefined : submit}
                  editable={!busy}
                />

                {registering && (
                  <InputField
                    label="Confirm password"
                    passwordField
                    value={confirmation}
                    onChangeText={setConfirmation}
                    placeholder="Enter your password again"
                    autoCapitalize="none"
                    autoCorrect={false}
                    autoComplete="new-password"
                    returnKeyType="done"
                    onSubmitEditing={submit}
                    editable={!busy}
                  />
                )}
              </View>

              {!!error && (
                <Text
                  style={styles.error}
                  accessibilityLiveRegion="polite"
                >
                  {error}
                </Text>
              )}

              <Pressable
                onPress={submit}
                disabled={busy}
                accessibilityRole="button"
                accessibilityState={{ disabled: busy, busy }}
                style={({ pressed }) => [
                  styles.submit,
                  (pressed || busy) && styles.pressed,
                ]}
              >
                <Text style={styles.submitText}>
                  {busy
                    ? registering
                      ? 'Creating account…'
                      : 'Signing in…'
                    : registering
                      ? 'Create account'
                      : 'Sign in'}
                </Text>
              </Pressable>
            </View>

            <Text style={styles.footer}>
              Skills. People. Possibilities.
            </Text>
          </View>
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}