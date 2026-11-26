# API-Kontrakt – historischer Entwurf

> Dieser REST-API-Entwurf beschreibt die frühere ASP.NET-Core-Zielarchitektur und ist für die neue Supabase-Architektur nicht mehr maßgeblich.

Die ursprünglichen Pfade unter `/api/v1` bleiben als historische Referenz erhalten. Die neue Plattform verwendet Supabase Data API mit RLS für geeignete Stammdatenzugriffe sowie transaktionale PostgreSQL-Funktionen für Bestandsaktionen.

Die neuen Datenbanktabellen, Funktionen, Rollen und RLS-Regeln werden als versionierte SQL-Migrationen unter `supabase/migrations/` definiert. Die verbindlichen fachlichen Anforderungen bleiben unverändert; insbesondere gibt es weiterhin genau eine zentrale AuditLog-Tabelle und keine StockMovement-Tabelle.

Siehe:
- [Zielarchitektur](ARCHITEKTUR.md)
- [Stack-Migration](STACK-MIGRATION.md)
- [Fachliches Datenmodell](DOMAENENMODELL.md)
- [AuditLog-Konzept](AUDITLOG-KONZEPT.md)
