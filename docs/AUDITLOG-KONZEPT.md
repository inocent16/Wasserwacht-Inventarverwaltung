# Auditlog-Konzept (Entwurf)

Status: Architekturvorschlag, noch nicht als endgültiges Schema beschlossen.

## Verbindliche Grundlage

- Es gibt genau ein zentrales Auditlog für die gesamte Anwendung.
- Es ist die einzige Quelle der Änderungshistorie; eine separate Materialbewegungstabelle ist ausgeschlossen.
- Jeder Eintrag ordnet die Änderung dem auslösenden Benutzer zu.
- Vorher- und Nachher-Zustand werden vollständig gespeichert, soweit sie für das Ereignis anwendbar sind.
- Auditlog-Einträge bleiben erhalten, wenn der betroffene Artikel, Standort oder Materialposten später gelöscht wird.
- Fachliche Änderung und Auditlog-Schreiben erfolgen in derselben Datenbanktransaktion.

## Empfohlener Zuschnitt

Empfohlen wird **ein Auditlog-Eintrag je betroffenem Datensatz**. Mehrere Einträge, die durch dieselbe fachliche Aktion entstehen, erhalten dieselbe `OperationId`.

Beispiele:
- **Zugang:** ein Eintrag für den betroffenen Materialposten (vorheriger Bestand und neuer Bestand).
- **Verschiebung:** Eintrag für den Quellposten und Eintrag für den Zielposten; beide mit derselben `OperationId`.
- **Sammelentnahme:** je betroffenem Materialposten ein Eintrag; alle Einträge der Sammelentnahme teilen sich eine `OperationId`.
- **Anlegen:** `BeforeState = null`, `AfterState` enthält den vollständigen Zustand.
- **Löschen:** `BeforeState` enthält den vollständigen Zustand, `AfterState = null`.

Damit lässt sich sowohl die Änderung eines einzelnen Datensatzes gezielt anzeigen als auch eine gesamte fachliche Aktion über ihre `OperationId` gruppieren. Das Auditlog bleibt eine einzige Tabelle und die Bewegungsübersicht wird aus diesen Einträgen abgeleitet.

## Vorgeschlagene Felder

| Feld | Zweck |
| --- | --- |
| `Id` (UUID) | Eindeutige ID des Auditlog-Eintrags |
| `OperationId` (UUID) | Gruppiert alle Einträge derselben fachlichen Aktion |
| `OccurredAtUtc` | Zeitpunkt der Änderung in UTC |
| `ActorUserId` (UUID) | ID des angemeldeten Benutzers |
| `ActorNameSnapshot` | Benutzername zum Zeitpunkt der Änderung, damit die Anzeige auch bei späterer Kontenänderung verständlich bleibt |
| `EventType` | Fachlicher Vorgang, z. B. ArticleUpdated oder StockTransferred |
| `EntityType` | Typ des betroffenen Datensatzes, z. B. Article, Location oder StockItem |
| `EntityId` (UUID) | ID des betroffenen Datensatzes |
| `BeforeState` (JSONB, nullable) | Vollständiger Zustand vor der Änderung |
| `AfterState` (JSONB, nullable) | Vollständiger Zustand nach der Änderung |

`EntityId` soll bewusst keinen Fremdschlüssel auf den aktuellen Datensatz besitzen. Sonst könnten Löschungen oder Kaskaden die Historie gefährden. Für den Benutzerbezug soll ebenfalls verhindert werden, dass ein Konten-Löschvorgang historische Einträge unverständlich macht; die genaue Konten-Löschregel ist separat festzulegen.

Die JSONB-Snapshots sollen nur den vollständigen fachlichen Zustand des betroffenen Datensatzes enthalten, nicht EF-Navigationseigenschaften oder zyklische Objektgraphen. Die genaue JSON-Serialisierung und eine mögliche Snapshot-Schema-Version werden vor Implementierung festgelegt.

## Technische Schutzmaßnahmen

- Auditlog-Einträge werden im normalen Anwendungsbetrieb ausschließlich angelegt, nicht geändert oder gelöscht.
- Die Datenbankindizes sollen mindestens Zeitstempel, `OperationId`, (`EntityType`, `EntityId`) und `ActorUserId` unterstützen.
- Alle Datensätze einer fachlichen Aktion und alle dazugehörigen Auditlog-Einträge werden atomar gespeichert.
- Das Speichern der Snapshots darf nicht von späteren Änderungen am aktuellen Datensatz abhängen.

## Noch zu entscheiden

1. Soll die empfohlene Granularität gelten: ein Eintrag je betroffenem Datensatz und gemeinsame `OperationId` für eine fachliche Aktion?
2. Soll `ActorUserId` auf das Benutzerkonto verweisen, wobei Konten nicht endgültig gelöscht werden dürfen, oder sollen zusätzlich unveränderliche Benutzer-Snapshots die Identität unabhängig vom Kontodatensatz sichern?
3. Welche genauen `EventType`-Werte und welches JSON-Format werden verbindlich verwendet?

Bis diese Punkte geklärt sind, wird weder das Auditlog-Datenbankschema noch eine neue Bewegungsimplementierung festgeschrieben. Die vorhandene `StockMovement`-Entität bleibt als widersprüchlicher Altbestand ausdrücklich zur Ablösung vorgemerkt und darf nicht als Grundlage neuer Funktionalität dienen.
