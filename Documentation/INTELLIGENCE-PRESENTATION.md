# Intelligence presentation — 5 November 2026

Presentation only: existing camp agents, provincial assignments, operations delay, secrets, tier VIII enemy-spy visibility, camp alert and report rules remain authoritative.

Six matching framed cells appear in three paired rows: agents in camp / agents abroad; enemy operations delay / secrets; enemy spies / camp alert. Duplicate gold is removed; the global HUD retains it. Explanatory notes use the existing CampaignChoiceTooltip, which clamps to the screen, does not intercept clicks and clears on exit or navigation.

Report numerals align at the far left; names are centred in the remaining width. Unknown reports display “?”; agent requirements appear on hover. Enemy spy counts remain unread below eight camp agents. Alert hover explains agent-loss risk, 1–10 percentage-point decay and any permanent alert floor. Report selection and action handlers are unchanged.

Verification: Windows preview build succeeded. IntelligencePresentationCapture passed 274 focused checks at 1280x720 and 1600x900 with 2, 8 and 10 camp agents. Checks cover six framed statistics, absence of duplicate gold and printed notes, hidden/revealed enemy counts, locked-tier hover requirements, tooltip exit/navigation cleanup, alert explanation and text fit. Visually reviewed the 1600 report layout and 1280 alert hover. Captures and checks.txt are under Builds/TheodoroPreview/Captures/IntelligencePresentation.

Files: CouncilIntelligenceUI.cs and IntelligencePresentationCapture.cs. Public Version 41 remains unchanged until the next publication.
