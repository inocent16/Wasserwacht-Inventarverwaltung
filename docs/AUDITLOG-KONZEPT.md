# Zentrales AuditLog

## Verbindliche Festlegung

- Es gibt genau **eine zentrale AuditLog-Tabelle**: `public.audit_log`.
- Sie ist die einzige Quelle der Änderungshistorie – auch für Materialzugänge, Verschiebungen und Sammelentnahmen.
- Es gibt keine `stock_movements`-Tabelle, keine separate Bewegungshistorie und kein zweites Protokollsystem.
- Jede erfolgreich abgeschlossene fachliche Aktion erzeugt genau einen AuditLog-Eintrag.
- Der Eintrag enthält den vollständigen Vorher- und Nachher-Zustand aller von dieser Aktion betroffenen Daten, soweit für den Ereignistyp anwendbar.
- Der Eintrag ordnet die Aktion dem angemeldeten Auth-Benutzer zu.
- Fachliche Änderung und genau dieser AuditLog-Eintrag werden innerhalb derselben PostgreSQL-Transaktion gespeichert.
- AuditLog-Einträge bleiben erhalten, auch wenn aktuelle Artikel, Standorte oder Materialposten später gelöscht werden.

## Ereignisse

- `article_created`, `article_updated`, `article_deleted`
- `location_created`, `location_updated`, `location_deleted`
- `stock_received`
- `stock_transferred`
- `stock_removal_batch`

Eine Verschiebung wird nicht in zwei Einträge für Quelle und Ziel aufgeteilt. Eine Sammelentnahme wird nicht pro Position einzeln protokolliert.

## Tabellenstruktur

Die erste SQL-Implementierung verwendet:

| Spalte | Typ | Zweck |
| --- | --- | --- |
| `id` | UUID | Identifiziert den Aktionseintrag |
| `occurred_at_utc` | timestamptz | Zeitpunkt der gespeicherten Änderung |
| `actor_user_id` | UUID | Unveränderliche Supabase-Auth-User-ID, ohne FK |
| `actor_name_snapshot` | varchar(200) | E-Mail-Snapshot zum Änderungszeitpunkt |
| `event_type` | varchar(100) | Ereignistyp |
| `before_state` | JSONB | Vollständige Zustände vor der Aktion |
| `after_state` | JSONB | Vollständige Zustände nach der Aktion |
| `metadata` | JSONB | Optionale Zusatzangaben wie eine Notiz, keine zweite Historie |

Bei einer Neuanlage ist `before_state` SQL-NULL; bei einer Löschung ist `after_state` SQL-NULL. Verschiebungs- und Sammelentnahme-Snapshots gruppieren alle betroffenen Positionen zu einer fachlichen Gesamtaktion.

## Schutz und Konsistenz

- RLS erlaubt das Lesen nur Administratoren.
- Direkte INSERT-/UPDATE-/DELETE-Rechte werden den API-Rollen nicht erteilt.
- Die SQL-Funktion `prevent_audit_log_mutation` blockiert UPDATE und DELETE zusätzlich per Trigger.
- Nur kontrollierte SECURITY DEFINER-RPCs können schreiben; sie prüfen zuerst die Datenbankrolle `administrator`.
- Jede RPC führt alle fachlichen Änderungen und das eine AuditLog-INSERT innerhalb desselben Aufrufs und derselben Transaktion aus. Fehler führen zum vollständigen Rollback.
- AuditLog hat absichtlich keine Fremdschlüssel auf Artikel, Standorte, Materialposten oder Auth-Profile. Historische Zustände bleiben so unabhängig von der Löschung aktueller Datensätze und Benutzerkonten erhalten.

## Historienansicht

Die Historienansicht liest ausschließlich `public.audit_log` und filtert die Ereignistypen `stock_received`, `stock_transferred` und `stock_removal_batch`. Es gibt keine zweite Materialbewegungsquelle.
