# Journal, accounts and presentation — 6 November 2026

Implemented and verified in the private JournalPresentationPreview Windows player. Not yet published as a new numbered release; public Version 43 remains unchanged.

## Journal

The three tabs are Palace Administration, Emperor’s Decisions and Strategic Status. The first groups public accounts and orders by date, with preparation-period headings and siege-day dates. The middle preserves event title, date, actual narrative, offered choices, selected choice and outcome. Linked follow-up nodes form the actual recorded chain. Only the chosen answer is interactive; other answers are disabled. The reader only navigates stored text, never invokes Yarn commands or gameplay callbacks.

EventDecisionHistory captures the existing OptionListButtons/EventUI boundary. Versioned transcripts are encoded in the existing journal Entry.text field under event-history IDs, so the campaign archive schema and checksum representation are unchanged. Loading an outcome checkpoint restores the parent transcript ID before the next node begins. Existing older entries remain plain text: missing historical choices are not invented or reconstructed from current state.

PalaceAdministration observes an explicit whitelist of public city resources, personnel and fractional city provisions. Checkpoints flush changed balances; successful orders flush immediately so their dates precede any turn advancement. Named saves also flush. Baselines reset after new-campaign initialisation and restore. No Ottoman spies, secret flags, enemy estimates or inferred hidden effects are examined. Event resource summaries use the existing public EventOutcome filter.

Key files: DecisionJournal.cs, JournalPageView.cs, EventDecisionHistory.cs, PalaceAdministration.cs, EventUI.cs, Custom OptionView/OptionListButtons.cs, SaveGames.cs, VariableSingleton.cs and the narrow order hooks in TrainingPool.cs/CouncilUI.cs.

## Other requested presentation

- Treasury: single compact quantity-step panel; daily demand/food horizon instead of duplicate gold; finer merchant outline.
- Brokers: whole surface below the HUD; no duplicate treasury header.
- Shipyard Return: Cannons detail in Military and Recruitment.
- Research and Rediscovery: renamed desk/council entry, generated corner pull-ring, finer discovery contours, dedicated monastic rediscovery background and direct research-tab entry.
- Gennadios: full-height alpha sprite and matching portrait.
- Palace locations: separate closely traced door and fleet-ship silhouettes, preserving callbacks and linked ship hover.
- CharacterChatter.Spent is now a read-only presentation query. It no longer changes saved chatter counters just because a market/character highlight becomes visible. Actual speech still initialises its day and records its existing limits.

Artwork provenance and exact collaborator files: RESEARCH-PRESENTATION-HANDOFF.txt and PALACE-CONTOURS-GENNADIOS-HANDOFF.txt. General rule: trace the full visible silhouette, including clothing, limbs and held objects; the same contour drives hit testing and highlight. Exclude nearby unrelated figures and background. Preserve narrow features instead of substituting boxes or coarse ovals.

## Verification

Private Windows export succeeded; editor save/technology validation passed 57 checks. Final exported-player suites: journal history 87, journal/status/map 147, market 44, mercenaries 283, military orders 270, discoveries 253, palace contours 46 — 1,130 checks. Tests use isolated archives.

Actual Yarn Our Turks branches, hidden-spy exclusions, once-only recording, disk save/load, selected-answer navigation, two-stage transcripts and story5→story4 continuation after restoring an outcome were exercised. UI suites use 1280×720 and 1600×900; discoveries additionally retain aspect-ratio checks. Representative final/candidate screenshots were inspected for research, monastic background, merchant, Gennadios, door and recorded-reader presentation. This is not a long-campaign balance test or an all-screen art audit.

Evidence: Builds/JournalPresentationPreview/Captures and Documentation/Journal presentation verification.json.
