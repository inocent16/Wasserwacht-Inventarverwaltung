import { FormEvent, useCallback, useEffect, useState } from "react";

type User = { id: string; userName: string; displayName: string; role: string };
type CsrfResponse = { requestToken: string };

async function getCsrfToken(): Promise<string> {
  const response = await fetch("/api/v1/auth/csrf", { credentials: "include" });
  if (!response.ok) throw new Error("Sicherheits-Token konnte nicht geladen werden.");
  const body = await response.json() as CsrfResponse;
  if (!body.requestToken) throw new Error("Der Server hat kein Sicherheits-Token geliefert.");
  return body.requestToken;
}

async function loadSession(): Promise<User | null> {
  const response = await fetch("/api/v1/auth/me", { credentials: "include" });
  if (response.status === 401) return null;
  if (!response.ok) throw new Error("Anmeldestatus konnte nicht geprüft werden.");
  return await response.json() as User;
}

export default function App() {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);
  const [userName, setUserName] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");

  const refreshSession = useCallback(async () => {
    try {
      setUser(await loadSession());
      setError("");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Verbindungsfehler.");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { void refreshSession(); }, [refreshSession]);

  async function handleLogin(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setBusy(true);
    setError("");
    try {
      const token = await getCsrfToken();
      const response = await fetch("/api/v1/auth/login", {
        method: "POST",
        credentials: "include",
        headers: { "Content-Type": "application/json", "X-CSRF-TOKEN": token },
        body: JSON.stringify({ userName, password })
      });
      if (response.status === 401) {
        setError("Benutzername oder Passwort ist nicht korrekt.");
        return;
      }
      if (!response.ok) throw new Error(response.status === 400
        ? "Sicherheitsprüfung fehlgeschlagen. Bitte Seite neu laden."
        : "Anmeldung momentan nicht möglich.");
      setPassword("");
      await refreshSession();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Verbindungsfehler.");
    } finally {
      setBusy(false);
    }
  }

  async function handleLogout() {
    setBusy(true);
    setError("");
    try {
      const token = await getCsrfToken();
      const response = await fetch("/api/v1/auth/logout", {
        method: "POST",
        credentials: "include",
        headers: { "X-CSRF-TOKEN": token }
      });
      if (!response.ok && response.status !== 401) throw new Error("Abmeldung fehlgeschlagen.");
      setUser(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Abmeldung fehlgeschlagen.");
    } finally {
      setBusy(false);
    }
  }

  if (loading) return <main className="page"><div className="loading">Inventarverwaltung wird geladen …</div></main>;

  return (
    <main className="page">
      <header className="topbar">
        <div className="brand-mark" aria-hidden="true">W</div>
        <div><div className="brand-name">Wasserwacht</div><div className="brand-subtitle">Inventarverwaltung</div></div>
        {user && <button className="quiet-button" onClick={() => void handleLogout()} disabled={busy}>Abmelden</button>}
      </header>

      <section className="content">
        {user ? (
          <div className="welcome-card">
            <div className="eyebrow">ANGEMELDET</div>
            <h1>Willkommen, {user.displayName}</h1>
            <p>Du bist als Administrator angemeldet.</p>
            <div className="session-detail"><span>Benutzerkonto</span><strong>{user.userName}</strong></div>
            <div className="session-detail"><span>Berechtigung</span><strong>Administrator</strong></div>
            <p className="next-step">Die Inventarbereiche werden im nächsten Umsetzungsschritt ergänzt.</p>
          </div>
        ) : (
          <form className="login-card" onSubmit={handleLogin}>
            <div className="eyebrow">ADMINISTRATOR-ZUGANG</div>
            <h1>Anmelden</h1>
            <p className="intro">Melde dich mit deinem persönlichen Wasserwacht-Konto an.</p>
            <label htmlFor="username">Benutzername</label>
            <input id="username" name="username" autoComplete="username" value={userName}
              onChange={e => setUserName(e.target.value)} required maxLength={100} autoFocus />
            <label htmlFor="password">Passwort</label>
            <input id="password" name="password" type="password" autoComplete="current-password"
              value={password} onChange={e => setPassword(e.target.value)} required />
            {error && <div className="error" role="alert">{error}</div>}
            <button className="primary-button" type="submit" disabled={busy || !userName.trim() || !password}>
              {busy ? "Bitte warten …" : "Sicher anmelden"}
            </button>
            <div className="security-note"><span aria-hidden="true">●</span> Geschützte Verbindung · Persönliche Sitzung</div>
          </form>
        )}
        {!user && error && <p className="connection-hint">Falls der Server nicht erreichbar ist, prüfe bitte die API-Verbindung.</p>}
      </section>
      <footer>Wasserwacht · Interne Materialverwaltung</footer>
    </main>
  );
}