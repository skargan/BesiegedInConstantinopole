# Hagia Sophia portals — 30 October 2026

## Implemented

Replaced the two movement buttons inside the Hagia Sophia painting with the existing palace/garden `LocationHotspot` interaction. The left passage returns to the Palace; the right arch opens the garden. Hover reveals a gold contour and destination caption. The hit area follows the visible passage rather than a rectangular button, with positions normalised to the fitted painting so resizing preserves alignment.

The right arch now opens onto a sunlit garden. The standing candle obstruction and hanging lamp inside that doorway were removed using built-in image generation. Other candles, architecture and the foreground reliquary remain. The painting keeps the existing resource path and Unity asset GUID. Character placement and the once-per-turn sanctuary health reward were not changed.

## Scope

- `Assets/Scripts/CouncilLocationsUI.cs` — Hagia navigation and two portal contours.
- `Assets/Resources/UI/MidnightMosaic/Locations/hagia-sophia.png` — replacement background; existing `.meta` retained.
- `Assets/Scripts/HagiaPortalCapture.cs` and `.meta` — opt-in development-player verification with isolated saves.
- `Assets/Scripts/UI/Editor/BuildHagiaPortalPreview.cs` and `.meta` — separate preview builder.
- This note and `Documentation/Hagia portals - image prompt.txt` — handoff and art provenance.

No changes to `LocationHotspot.cs`, published Version 36 binaries, character sprites, game rules or unrelated shared files.

## Verification performed

Unity 2022.3.15f1 exported a development player successfully against the Version 36 source baseline. Running it with `-hagia-portal-capture` completed **45 checks**. Both destinations opened correctly, actual UI raycasts reached the portals, exterior corners were rejected, right-click did not travel, and portal hit areas worked at 1600×900 and 1100×760.

The sanctuary restored health from 50% to 51% on the first visit and did not heal again on return during the same turn. Navigation spent no gold, provisions or campaign time. The capture run used isolated test saves. All four captured screens (idle, left hover, right hover, resized) were visually inspected. The run logged no exceptions or errors.

Preview: `Builds/HagiaPortalPreview/Besieged.exe`. Start normally, enter the Palace, then Hagia Sophia; hover and left-click either passage. The exported player retains the Version 36 header because this is a scoped preview, not a new numbered release.

Reproduce the build with Unity batch mode and `-executeMethod BuildHagiaPortalPreview.Build`. Reproduce checks by launching the preview with `-hagia-portal-capture`; results appear under `Builds/HagiaPortalPreview/Captures/HagiaPortals/`.

## Art provenance

Generated via the built-in `image_gen` tool, using the existing clean Hagia painting as edit target and the user's marked screenshot only as a location guide. The complete final prompt is saved alongside this note. Result: 1672×941 RGB PNG. SHA-256: `366BFCC9F5649B5CC95E73B9188CA83C5B15CD3D772257C6470450D42332EA41`.

## Release handoff

The main developer confirmed the location source, background and Unity build slot were free after publishing Version 36. These changes were integrated into `C:/Users/Dainius/Documents/Codex/Besieged-release18` and tested in the separate preview. Include them in the next normal release; the already-published Version 36 executable does not contain this change.
