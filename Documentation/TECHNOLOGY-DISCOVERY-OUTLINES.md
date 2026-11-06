# Technology discovery presentation — 6 November 2026

Owner: ConstantineX. Scope: object outlines, compact alchemy painting and focused verification. No reward, achievement, save-format, audio-preference or campaign-rule changes.

## Implementation

- TechnologyDiscoveryShapes.cs contains hand-traced source-image polygons for all nine objects, shared by hover rendering and pointer hit-testing. Coordinates are authored against the 1672×941 painting and transformed through SceneBackdrop's actual crop.
- Skull follows the head silhouette; treasure uses only the central round catch; pigeon discovery uses the small rolled letter, excluding the bird and feet. The six alchemy shapes follow their individual equipment.
- Scenes/technologies-compact.png places the complete apparatus on the exposed left side of the default right-hand desk. Original technologies.png retained. CouncilTechnologyUI selects the revised sibling artwork.
- Existing sequential unlocking, independent discoveries, 60-second inactivity reset, once-per-campaign rewards, journal entries and draggable desk remain authoritative.
- Fresh-install sound effects and music already default On. Menu.unity serialises Master/Music/SFX volume at 0.74. Existing saved preferences remain respected; no forced preference reset.

## Verification

Private Windows player export succeeded. DiscoveryCapture passed 244 checks: imported sounds, all discovery rewards/duplicate guards, journal and save/load, experiment reset, draggable panel, hit-test exclusion outside the small objects, all six alchemy targets reachable beside the default desk, and text fitting at requested 1600×900, 1280×720, 1920×1200 and 2560×1080.

The fixture uses isolated saves and suppresses career achievement writes. Test audio is muted globally; imported clips and settings were checked, rather than claiming a fresh-install listening test. Screenshots are in Builds/TheodoroPreview/Captures/Discoveries. Final outline captures additionally enable the real gold stroke, since showing only the caption does not exercise hover presentation.

Publication status: private preview only; published desktop release remains Version41. GitHub main backup previously failed with server-side commit_refs_rules error; do not claim these local changes are remotely backed up until push succeeds.
