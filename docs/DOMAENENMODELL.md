# Fachliches Datenmodell

Dieses Dokument beschreibt die fachlichen Begriffe und Regeln der Anwendung. Die SQL-Implementierung liegt unter `supabase/migrations/`.

## Artikel

Ein Artikel ist der Stammdatensatz eines Materials.

- Id (UUID)
- Name (Pflicht, max. 200 Zeichen)
- Kategorie (Pflicht, zunächst Freitext, max. 100 Zeichen)
- Beschreibung (optional, max. 2000 Zeichen)
- Hersteller (optional, max. 200 Zeichen)

Ablaufdaten sind eine Eigenschaft der einzelnen Materialposten, nicht des Artikelstammdatensatzes. Aktiv/Inaktiv ist nicht vorgesehen.

## Standort

Ein Standort ist ein frei durch die Anwender angelegter Aufbewahrungsort.

- beliebig viele Standorte
- alle Standorte gleichberechtigt
- kein Standorttyp
- keine automatische Anlage beim ersten Start
- keine implizite Sonderrolle oder Standardstandort-Logik

## Materialposten

Ein Materialposten beschreibt eine konkrete Menge eines Artikels an genau einem Standort.

- Artikel-ID
- Standort-ID
- Menge (positive ganze Zahl, Einheit Stück)
- Ablaufdatum (optional)
- Chargennummer (optional)
- Hersteller (optional)

Ein Artikel kann an mehreren Standorten sowie je Standort in mehreren Posten mit unterschiedlichen Ablaufdaten oder Chargen vorkommen.

### Zusammenführen – implementierter sicherer Ausgangsstand

Die SQL-Funktionen führen Materialposten nur dann zusammen, wenn alle Identitätsmerkmale exakt übereinstimmen: Artikel, Zielstandort, Ablaufdatum, Chargennummer und Hersteller. Dabei gilt:
- NULL und ein bekannter Wert sind verschieden.
- Unterschiedliche bekannte Chargen bleiben getrennt.
- Ein abweichender oder fehlender Hersteller wird nicht stillschweigend überschrieben.
- Texte werden am Rand getrimmt; Groß-/Kleinschreibung bleibt relevant.

Die ursprüngliche fachliche Frage zur Bedeutung fehlender Chargen und des Herstellers bleibt als Produktentscheidung in `OFFENE_FRAGEN.md` dokumentiert. Bis dahin verhindert diese konservative Regel unerwartete Vermischungen.

### Vollständige Entnahme

Wenn die Menge vollständig entnommen wird, wird der Materialposten gelöscht. Die AuditLog-Einträge bleiben erhalten. Eine Sammelentnahme referenziert jeden konkreten Materialposten über seine UUID; es gibt keine automatische FEFO-/FIFO-Verteilung.

## Benutzerkonten

Supabase Auth verwaltet persönliche Konten mit E-Mail-Adresse und Passwort. Zum Start existiert ausschließlich die Rolle `administrator`. Öffentliche Selbstregistrierung ist deaktiviert.

Neue Auth-Konten erhalten per Trigger ein Profil mit `role = NULL` und haben dadurch keinen Zugriff auf die Inventarverwaltung. Das erste Administratorkonto wird kontrolliert per Bootstrap freigeschaltet; weitere Konten werden später über einen geschützten Administrator-Einladungsprozess provisioniert.

Jede fachliche Änderung wird im zentralen AuditLog der angemeldeten Auth-User-ID und einem E-Mail-Snapshot zugeordnet.

## AuditLog und Materialbewegungen

Es gibt genau eine zentrale Tabelle `public.audit_log`. Sie ist die einzige Quelle der Änderungshistorie. Es gibt keine `stock_movements`-Tabelle und kein zweites Protokollsystem.

Jede erfolgreich abgeschlossene fachliche Aktion erzeugt genau einen AuditLog-Eintrag:
- Artikel und Standort anlegen, ändern oder löschen
- Materialzugang
- Verschiebung
- Sammelentnahme

Der Eintrag enthält den vollständigen Vorher- und Nachher-Zustand aller von der Aktion betroffenen Datensätze. Bei Verschiebungen umfasst er Quelle und Ziel; bei Sammelentnahmen alle Positionen gemeinsam. Bestandsänderungen und AuditLog-Eintrag werden innerhalb derselben PostgreSQL-Transaktion gespeichert.

## Technische Tabellen

- `profiles`: Auth-Profil und Anwendungsrolle
- `articles`: Artikelstammdaten
- `locations`: Standorte
- `stock_items`: aktuelle Materialposten
- `audit_log`: einzige Historienquelle

Die drei Bestandsfunktionen heißen `receive_stock`, `transfer_stock` und `remove_stock_batch`. Artikel- und Standortänderungen erfolgen über auditiertes CRUD-RPC. Details stehen in `API-KONTRAKT.md`.
