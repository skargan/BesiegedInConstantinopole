# Imperial Council quotations

## Behaviour

The council displays one passage per campaign turn, followed by the author/speaker and short source in the existing style. Editorial status labels are not displayed, as requested. Reopening the council or loading a save retains that turn's passage.

The game preserves Unity's gameplay random state but has no saved numeric whole-campaign seed. The quote deck instead uses the existing saved `SaveGames.CampaignId`, a fresh GUID for each new campaign, including Start at Siege. SHA-256 seeds a privately defined xorshift64* generator and Fisher–Yates shuffle. Neither Unity's gameplay RNG nor PresentationRandom is consumed.

Each campaign has one stable permutation of the full catalogue. The turn selects its position. Every 392 consecutive turns include all 392 entries once, including a campaign beginning at a non-zero siege turn; after that the deck cycles. Adjacent turns cannot repeat. Separate campaign IDs generate different shuffled orders. Save/load needs no extra saved fields or format changes. Legacy archives retain the identity supplied by the existing loader.

## Imported content

Input: `To resolve - quotes.docx`, supplied by the designer from the Besieged game folder. SHA-256: `e481c576d4e811e67659c3581acdb157d1a0656f1e724f201229db1b09a80a32`.

The document's title still says 400; its edited body contains 392 quotations. Import preserves those 392 passages, stable Q IDs, original attributions, 392 entry links and all 50 source-register records. The eight absent IDs remain absent: Q012, Q013, Q192, Q196, Q197, Q200, Q204 and Q224. No introductory prose, headings or source-register rows become quotations.

An independent XML-based comparison verified every passage and full attribution against the supplied document. The simplified display field contains only name and short citation; the durable JSON also retains the original provenance for future editing. This was an import-fidelity audit, not a fresh historical-source audit.

## Files and maintenance

- `Assets/Resources/Data/council-quotes.json`: editable, versioned catalogue and provenance.
- `Assets/Scripts/CouncilQuotations.cs`: pure campaign/turn selection.
- `Assets/Scripts/CouncilQuoteView.cs`: refreshes only when turn or campaign changes, including save loads.
- `Assets/Scripts/CouncilUI.cs`: narrow replacement of the fixed Marcus Aurelius caption and a taller existing quote panel.
- `Tools/ImportCouncilQuotes.py`: regenerate the catalogue from an edited DOCX using Python with python-docx. Takes input DOCX and output JSON paths; it does not modify the source document.
- `CouncilQuoteCapture.cs` and `BuildCouncilQuotePreview.cs`: isolated exported-player checks and preview builder.

Keep IDs stable when editing. Catalogue content/order and `catalogueVersion` affect the derived sequence; changing them intentionally may change the quote shown by an older save. The current version is `selected-2026-10-31-v1`.

## Verification

Exported-player verification passed: 59 checks, plus all 392 quotations measured at 1600 x 900, 1280 x 720 and 1100 x 760 (1,176 layouts). No overflow was found; captions used at most four lines and retained the base font size of 22. Seven captured screens, including the longest passages and maximum-line cases, were visually reviewed.

Checks cover new campaign identities, different campaign orders, stable same-turn selection, no repeats within any 392 consecutive turns (including non-zero starting turns), save/load restoration, actual Start at Siege, navigation and turn refresh, absence of editorial labels from display, unchanged gameplay random state and unchanged campaign resources/date. The player exited successfully with no errors, exceptions or missing-glyph warnings. The independent content audit and staged C# compilation also passed.

Evidence: `Builds/CouncilQuotePreview/Captures/CouncilQuotes/checks.txt`, `caption-layout.csv` and the seven PNG captures. The preview is a separate playable build, not a replacement for the published Version 36.2 executable.

Preview target: `Builds/CouncilQuotePreview/Besieged.exe`. Builder: `BuildCouncilQuotePreview.Build`. Verification flag: `-council-quote-capture`; isolated saves, captures and results are written under `Captures/CouncilQuotes` beside the preview.

## Coordination

Integrated after the main developer completed Version 36.2 at commit `855bdae`. The CouncilUI change preserves the Spies delegation, build label and unrelated work; only the quote label and its panel height changed. The Unity build slot is free after the successful preview and player checks. Publication remains with the main developer.
