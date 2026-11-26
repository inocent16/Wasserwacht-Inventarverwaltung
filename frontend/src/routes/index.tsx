import { createFileRoute } from "@tanstack/solid-router";

export const Route = createFileRoute("/")({ component: HomePage });

function HomePage() {
  return (
    <main class="page">
      <header class="topbar">
        <div class="brand-mark" aria-hidden="true">W</div>
        <div>
          <div class="brand-name">Wasserwacht</div>
          <div class="brand-subtitle">Inventarverwaltung</div>
        </div>
      </header>
      <section class="content">
        <div class="welcome-card">
          <div class="eyebrow">NEUE PLATTFORMBASIS</div>
          <h1>Die Inventarverwaltung entsteht.</h1>
          <p>
            Das SolidJS- und TanStack-Grundgerüst ist eingerichtet. Als Nächstes
            werden Supabase Auth, das Datenbankschema und die transaktionalen
            Inventarfunktionen angebunden.
          </p>
          <div class="status-list">
            <div class="status-row"><span>SolidJS + TanStack Start</span><strong>eingerichtet</strong></div>
            <div class="status-row"><span>Supabase-Client</span><strong>vorbereitet</strong></div>
            <div class="status-row"><span>Inventarfunktionen</span><strong>ausstehend</strong></div>
          </div>
        </div>
      </section>
      <footer>Wasserwacht · Interne Materialverwaltung</footer>
    </main>
  );
}
