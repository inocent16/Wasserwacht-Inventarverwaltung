import { createSignal, onMount } from "solid-js";
import { createFileRoute } from "@tanstack/solid-router";
import {
  getCurrentUser,
  signInWithPassword,
  signOut,
  type AuthenticatedUser
} from "../lib/auth/auth.functions";

export const Route = createFileRoute("/")({ component: LoginPage });

function LoginPage() {
  const [user, setUser] = createSignal<AuthenticatedUser | null>(null);
  const [checkingSession, setCheckingSession] = createSignal(true);
  const [busy, setBusy] = createSignal(false);
  const [email, setEmail] = createSignal("");
  const [password, setPassword] = createSignal("");
  const [error, setError] = createSignal("");

  onMount(() => {
    void getCurrentUser()
      .then(setUser)
      .catch(() => setUser(null))
      .finally(() => setCheckingSession(false));
  });

  async function handleSubmit(event: SubmitEvent) {
    event.preventDefault();
    if (busy()) return;

    setBusy(true);
    setError("");

    try {
      const result = await signInWithPassword({
        data: { email: email().trim(), password: password() }
      });

      if (!result.ok) {
        setError(result.message);
        return;
      }

      setPassword("");
      setUser(result.user);
    } catch {
      setError("Anmeldung momentan nicht möglich. Bitte Verbindung prüfen.");
    } finally {
      setBusy(false);
    }
  }

  async function handleLogout() {
    if (busy()) return;

    setBusy(true);
    setError("");

    try {
      const result = await signOut();
      if (!result.ok) {
        setError("Abmeldung fehlgeschlagen. Bitte erneut versuchen.");
        return;
      }
      setUser(null);
    } catch {
      setError("Abmeldung fehlgeschlagen. Bitte erneut versuchen.");
    } finally {
      setBusy(false);
    }
  }

  return (
    <main class="page">
      <header class="topbar">
        <div class="brand-mark" aria-hidden="true">W</div>
        <div>
          <div class="brand-name">Wasserwacht</div>
          <div class="brand-subtitle">Inventarverwaltung</div>
        </div>
        {user() && (
          <button class="quiet-button" type="button" onClick={() => void handleLogout()} disabled={busy()}>
            Abmelden
          </button>
        )}
      </header>

      <section class="content">
        {checkingSession() ? (
          <div class="loading" role="status">Anmeldestatus wird geprüft …</div>
        ) : user() ? (
          <div class="welcome-card">
            <div class="eyebrow">ANGEMELDET</div>
            <h1>Willkommen!</h1>
            <p>Du bist als Administrator angemeldet.</p>
            <div class="session-detail">
              <span>Benutzerkonto</span>
              <strong>{user()?.email}</strong>
            </div>
            <div class="session-detail">
              <span>Berechtigung</span>
              <strong>Administrator</strong>
            </div>
            <p class="next-step">Die Inventarbereiche werden im nächsten Umsetzungsschritt ergänzt.</p>
          </div>
        ) : (
          <form class="login-card" onSubmit={handleSubmit}>
            <div class="eyebrow">ADMINISTRATOR-ZUGANG</div>
            <h1>Anmelden</h1>
            <p class="intro">Melde dich mit deiner persönlichen E-Mail-Adresse und deinem Passwort an.</p>

            <label for="email">E-Mail-Adresse</label>
            <input
              id="email"
              name="email"
              type="email"
              autocomplete="username"
              inputmode="email"
              value={email()}
              onInput={(event) => setEmail(event.currentTarget.value)}
              required
              maxlength="320"
              autofocus
            />

            <label for="password">Passwort</label>
            <input
              id="password"
              name="password"
              type="password"
              autocomplete="current-password"
              value={password()}
              onInput={(event) => setPassword(event.currentTarget.value)}
              required
              maxlength="256"
            />

            {error() && <div class="error" role="alert">{error()}</div>}

            <button class="primary-button" type="submit" disabled={busy() || !email().trim() || !password()}>
              {busy() ? "Bitte warten …" : "Sicher anmelden"}
            </button>
            <div class="security-note">
              <span aria-hidden="true">●</span> Geschützte Sitzung · Supabase Auth
            </div>
          </form>
        )}
        {!user() && error() && (
          <p class="connection-hint">Falls der Server nicht erreichbar ist, prüfe bitte die Supabase-Konfiguration.</p>
        )}
      </section>

      <footer>Wasserwacht · Interne Materialverwaltung</footer>
    </main>
  );
}
