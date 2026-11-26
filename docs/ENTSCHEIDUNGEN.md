# Technische und fachliche Entscheidungen

## Mengeneinheit
Alle Artikel werden zunächst ausschließlich in **Stück** geführt. Mengen werden als ganze Zahlen gespeichert.

## Kategorien
- Kategorien können beim Artikel zunächst frei eingegeben werden.
- Bereits verwendete Kategorien sollen als Auswahl bzw. Vorschlag verfügbar sein.
- Die Kategorie wird zunächst als Text am Artikel gespeichert; eine spätere Kategorienverwaltung muss vorhandene Werte migrieren und gleiche Schreibweisen zusammenführen können.

## Datenbank und Anwendungstechnik
- Frontend und Fullstack: SolidJS 1.9 + TanStack Start 1.x.
- Routing: TanStack Router; Server-State: TanStack Query.
- Datenbank und Authentifizierung: Supabase Cloud / PostgreSQL / Supabase Auth.
- Datenbankschema wird ausschließlich durch versionierte SQL-Migrationen geändert.
- Supabase RLS wird auf allen exponierten Anwendungstabellen verpflichtend aktiviert.
- Bestandsaktionen werden als transaktionale PostgreSQL-Funktionen umgesetzt.
- Das C#-Backend bleibt während der Migration erhalten und wird erst nach vollständigem Ersatz entfernt.
- Versionsbegründung: `STACK-MIGRATION.md`.

## Auditlog und Bestandsaktionen
Für die gesamte Anwendung gibt es genau eine zentrale AuditLog-Tabelle als einzige Quelle der Änderungshistorie. Es gibt keine StockMovement-Tabelle, keine separate Bewegungshistorie und kein zweites Protokollsystem.

Jede erfolgreich abgeschlossene fachliche Aktion erzeugt genau einen AuditLog-Eintrag. Jeder Eintrag speichert den vollständigen Vorher- und Nachher-Zustand aller betroffenen Daten sowie die Identität des auslösenden Benutzers. Fachliche Änderungen und der eine dazugehörige AuditLog-Eintrag werden gemeinsam transaktional gespeichert.

## Benutzeranmeldung
Version 1.0 besitzt persönliche Benutzerkonten. Zum Start gibt es ausschließlich die Rolle `Administrator`; eine zusätzliche Standardbenutzerrolle oder feinere Berechtigungsstufen sind zunächst nicht vorgesehen. Öffentliche Selbstregistrierung ist nicht vorgesehen.

Supabase Auth übernimmt Identität und Sitzung. **Anmeldung erfolgt mit E-Mail-Adresse und Passwort.** Öffentliche Selbstregistrierung ist deaktiviert. Vor Implementierung der restlichen Kontoverwaltung sind noch die Passwortänderung/-zurücksetzung und der Admin-Einladungsablauf festzulegen.

Neue Profile erhalten ausdrücklich zunächst keine Anwendungsrolle. Die Rolle `administrator` wird für das erste Konto einmalig durch einen kontrollierten Bootstrap vergeben; weitere Konten erhalten sie erst über einen geschützten Admin-Einladungsprozess. Dadurch führt ein versehentlich aktiviertes öffentliches Signup nicht zu Administratorzugriff.

## Historie / Auditlog
- Das zentrale Auditlog ist verbindlicher Bestandteil des Kernsystems.
- Es gibt keine zusätzliche, separate Materialbewegungstabelle.
- Die Bewegungshistorie wird ausschließlich aus passenden AuditLog-Einträgen dargestellt.
- Audit-Einträge bleiben unabhängig vom Lebenszyklus der betroffenen Datensätze erhalten; keine Kaskadenlöschung historischer Daten.

## Initialer Standort
Es wird beim ersten Start kein Standort automatisch angelegt. Standorte werden durch die Anwender selbst erstellt. Es gibt keinen technisch fest vorgegebenen Standort namens „Lager“.

## Materialposten bei vollständiger Entnahme
Wird die Menge eines Materialpostens vollständig entnommen, wird der Materialposten gelöscht, statt mit Menge 0 bestehen zu bleiben. Die AuditLog-Einträge bleiben dauerhaft erhalten.
