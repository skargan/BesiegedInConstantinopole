# Sultan audience — 30 October 2026

## Result

Messages to the Sultan now fits on one page. Three conversation cards sit left of a larger, centred Mehmed portrait; gifts, demand and taunt sit on the right; the request for surrender terms sits beneath the portrait. Each card is itself clickable. Existing valid or expired offers have a separate review button in the footer. The old scroll area and scrollbar are hidden on this screen only.

All seven choices, descriptions, availability rules, costs, effects, eight-gift catalogue and confirmation flows remain. The introduction now correctly describes the existing shared daily history/poetry/theology allowance rather than claiming one conversation per topic. A local submission guard prevents a second invocation of the same card applying an action twice. No balance or narrative content was changed.

## Integration

- `Assets/Scripts/CouncilUI.cs`: replaces only `Sultan()` with a delegation to `SultanAudience()`.
- `Assets/Scripts/CouncilSultanUI.cs` and `.meta`: isolated layout and original choice routing.
- `Assets/Scripts/SultanAudienceCapture.cs` and `.meta`: opt-in exported-player verification with isolated saves.
- `Assets/Scripts/UI/Editor/BuildSultanAudiencePreview.cs` and `.meta`: separate preview builder.
- This note.

The existing character portrait for ID 202 is reused without alteration. SultanNegotiations, SultanGifts, surrender terms, other council screens and published Version 36 binaries are untouched.

## Validation

Final Unity 2022.3.15f1 export succeeded without compiler errors. The development player completed 947 individual assertions across preparation, fresh siege, used conversations, exhausted history, valid and expired surrender offers, gift browsing/cancellation and major-action submission. These are repeated layout and state assertions, not 947 separate gameplay scenarios.

Verified all seven choices fit without scrolling, portrait centring and size, no clipped or overflowing text, non-overlapping cards, live UI raycasts and a reachable Return button at 1600×900, 1100×760 and 1280×720. Checked free conversations and gift browsing do not spend the major action, funds or time; gift confirmation still rejects insufficient funds; requesting/reviewing terms does not accept them; daily limits retain their original behaviour; a guarded double callback applies demand effects once and enters the normal result/rest phase.

All seven screenshots from the final run were visually inspected. The final run logged no exceptions or errors. Its test saves are isolated under the preview's Captures directory.

## Run and reproduce

Preview: `Builds/SultanAudiencePreview/Besieged.exe`. Open Diplomacy and court, then Messages to the Sultan. The preview retains the Version 36 header; this is not a new numbered release.

Build: Unity batch mode, `-executeMethod BuildSultanAudiencePreview.Build`.

Verify: launch the exported player with `-sultan-audience-capture`. Captures and checks appear in `Builds/SultanAudiencePreview/Captures/SultanAudience/`.

The main developer was notified of the scoped method ownership and separate build, then informed when the Unity build slot was released for cursor work. Include this committed change in the next normal release; the already-published Version 36 executable has not been overwritten.
