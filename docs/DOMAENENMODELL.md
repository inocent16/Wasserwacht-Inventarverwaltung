# Fachliches Datenmodell

Dieses Dokument beschreibt die fachlichen Begriffe. Es ist kein Ersatz für konkrete EF-Konfigurationen oder Migrationen.

## Artikel

Ein Artikel ist der Stammdatensatz eines Materials.

Bestätigte Felder:
- Id (eindeutiger technischer Schlüssel)
- Name (Pflicht)
- Kategorie (Pflicht; zunächst Freitext)
- Beschreibung (optional)
- Hersteller (optional)

Ablaufdaten sind eine Eigenschaft der einzelnen Materialposten, nicht des Artikelstammdatensatzes: Derselbe Artikel kann Posten mit unterschiedlichen Ablaufdaten haben oder ohne Ablaufdatum geführt werden.

Aktiv/Inaktiv ist nicht vorgesehen.

## Standort

Ein Standort ist ein frei durch die Anwender angelegter Aufbewahrungsort, z. B. Lager, Rucksack oder Fahrzeug.

Bestätigte Regeln:
- beliebig viele Standorte
- alle Standorte gleichberechtigt
- kein Standorttyp
- keine automatische Anlage beim ersten Start
- keine implizite Sonderrolle für einen Standort

Beschreibung ist optional. Optionale Standortfunktionen werden unabhängig je Standort aktiviert.

## Materialposten

Ein Materialposten beschreibt eine konkrete Menge eines Artikels an genau einem Standort.

Fachliche Attribute:
- Artikel
- Standort
- Menge (positive ganze Zahl, Einheit Stück)
- Ablaufdatum (optional)
- Chargennummer (optional)
- Hersteller (optional)

Ein Artikel kann an mehreren Standorten sowie je Standort in mehreren Posten mit unterschiedlichen Ablaufdaten oder Chargen vorkommen.

### Zusammenführen

Bei einem Zugang oder einer Verschiebung wird ein passender vorhandener Posten erhöht, statt einen doppelten Posten anzulegen.

Sicher bestätigt:
- Artikel und Standort müssen übereinstimmen.
- Ablaufdatum muss übereinstimmen (einschließlich „kein Ablaufdatum“).
- Unterschiedliche bekannte Chargennummern dürfen nicht zusammengeführt werden.

Noch zu präzisieren: Verhalten bei fehlender Charge auf einer Seite und Bedeutung des Herstellers für die Identität eines Postens. Siehe `OFFENE_FRAGEN.md`.

### Vollständige Entnahme

Wenn die Menge vollständig entnommen wird, wird der Materialposten gelöscht. Die zugehörigen Auditlog-Einträge bleiben erhalten. Historische Einträge dürfen nicht per Fremdschlüssel-Kaskade mit dem Materialposten gelöscht werden.

## Auditlog und Materialbewegungen

Es gibt genau ein zentrales Auditlog für Änderungen in allen Anwendungsbereichen. Eine separate Materialbewegungstabelle ist nicht vorgesehen.

Das Auditlog protokolliert unter anderem die fachlichen Vorgänge:
- Zugang: Material an einem Standort hinzufügen
- Verschiebung: eine Menge von einem Standort zu einem anderen bewegen
- Sammelentnahme: mehrere Positionen aus einem Standort in einem Vorgang entnehmen

Die Bewegungshistorie wird aus den entsprechenden Auditlog-Einträgen dargestellt. Eine Sammelentnahme muss als fachlich zusammengehöriger Vorgang erkennbar sein. Jeder Auditlog-Eintrag speichert den vollständigen Vorher- und Nachher-Zustand der betroffenen Daten, soweit für das Ereignis anwendbar, sowie die Identität des auslösenden Benutzers. Das konkrete Datenformat und die Zuordnung zusammengehöriger Einträge sind vor der Implementierung festzulegen.

## Sollbestand

Optional pro Kombination aus Artikel und Standort. Ohne explizite Zuordnung existiert kein Sollwert und es wird kein Soll-Ist-Vergleich durchgeführt. Ein definierter Sollbestand ist mindestens 1.

## Kategorien

Erste Ausbaustufe: Freitext-Eingabe plus Vorschläge aus bereits verwendeten Kategorien. Später soll auf eine verwaltbare Kategorienliste umgestellt werden. Die spätere Migration muss bestehende Kategorienwerte erhalten.
