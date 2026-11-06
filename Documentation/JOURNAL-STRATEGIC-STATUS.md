# Decision Journal tabs — 6 November 2026

Three independent tabs: Daily Notes, Decisions, Strategic Status. The previous mixed Decisions-and-affairs page is replaced; chronology and persistent decision records retain their existing save format.

`JournalStrategicStatus.Read()` is a read-only projection:

- Bosphorus Passage → existing `bStraitsOpen`.
- Golden Horn Chain → existing `bGoldenHornChain`.
- Galata Alignment → formal `PreparationCampaign.GalataState`, never goodwill.
- Union of Florence → `iReligion`; completion of existing story1 records its ruling in the existing journal. Existing siege/late-preparation campaigns with Orthodox policy map to Rejected; new campaigns remain Undecided before the ruling.
- Sea Dominance → existing `NavalState.Dominance`, including the strict 50% superiority boundary and zero/zero Contested.
- Turkish Mercenary Battalion → existing creation flag. The creation journal entry preserves revelation if its flag later becomes inactive. No additional battalion simulation or inferred casualties.
- Existing Ionian shelter/refuge agreements move out of Decisions' prefixed status text into this dashboard when relevant.

The status tab refreshes automatically while open. No new effects, random draws, independent strategic variables or save schema. Later changes to conditions automatically override initial statuses. Other proposed statuses (Varna, crusades, marriage, heir) remain deferred.

Private Windows export succeeded and JournalStatusCapture passed 139 checks at exact 1280×720 and 1600×900, verifying the mapping, one-time Union journal entry, save/load, progressive battalion visibility, live passage refresh, actual tab/Return navigation and text fitting. Captures are under Builds/TheodoroPreview/Captures/JournalStatus. Public Version41 is unchanged until publication.
