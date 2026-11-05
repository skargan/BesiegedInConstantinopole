# Asset sources register

This is the central index for artwork, cursors, music and sound effects used in Saving Constantinople. Keep original sources and asset-specific manifests; link them here rather than losing their provenance during integration.

For each new asset record: local runtime path, creator/provider, original URL or generated-file identifier, retrieval/creation date, licence and attribution requirements, modifications, and integration status. A free download is not evidence of permission for commercial redistribution. Unverified assets stay out of published builds until their licence is recorded.

## Current discovery work â€” 4 November 2026

| Asset | Source | Rights/provenance | Status |
| --- | --- | --- | --- |
| Black leather/gold pointing and gripping cursor | OpenAI image generation, selected concept 14; generation `exec-121de055-b08d-4254-af1e-d569f8568954.png` | AI-generated for this project; not a third-party stock licence. Reference: `Documentation/CursorStudies/sheet-3.png`. Realistic leather and aged metal refinement. | Imported as `Assets/Resources/UI/MidnightMosaic/cursor-black-gold-pair.png`; shared cursor integration under verification. |
| Technology-room discovery artwork | Existing project technology-room illustration | Preserve generation history in `Documentation/Presentation40/CHECKPOINT.txt` | Integrated revised room: generation exec-caf40288-2da0-4747-90e9-1a7e2d06e745.png, 4 November 2026. Clear chalice/pipe, fossil skull, minaret removal, parchment perspective. |
| Grinding herbs; pouring water; ignition; chime; bubbling; brief choir; imagined creature growl; falling coins | OpenGameArt: eight attributed sources | See DiscoveryAssets/Attribution/SOUND-CREDITS.md and manifest.json | Eight short clips imported under Audio/Discoveries; credits and adapted CC-BY-SA growl shipped in StreamingAssets/Credits. |

Cursor refinement prompt: Produce two matching transparent cursors from concept 14, pointing upper-left and closed gripping hand. Black medieval leather glove, gold knuckle fittings and Byzantine cross, blue/gold cuff. Natural compact proportions, realistic leather grain/seams and restrained aged metal reflections; no cartoon/plastic finish, text or background.

## Existing provenance records

- `Documentation/Artwork/Diplomacy/prompts-and-sources.json`
- `Documentation/Artwork/Diplomacy/aq-source.json`
- `Documentation/Artwork/Diplomacy/asset-audit.json`
- `Documentation/Artwork/LithuaniaPoland/sources.txt`
- `Documentation/Artwork/Galata/art-source.json`
- `Documentation/Artwork/Infirmary/art-source.json`
- `Documentation/Artwork/MarketMerchant/art-source.txt`
- `Documentation/Artwork/Compact military/art-source.txt`
- `Documentation/UI-asset-prompts.txt`
- `Documentation/Art assets and presentation standard.txt`

This index is being consolidated, not a completed licence audit of every legacy asset. Music supplied by the designer, including The Last Ison, needs its author/licence declaration attached here before external distribution.

## Discovery integration

Sources and exact edits: [sound credits](DiscoveryAssets/Attribution/SOUND-CREDITS.md), [machine-readable manifest](DiscoveryAssets/Attribution/manifest.json). Source-page snapshots and original downloads are retained in DiscoveryAssets for review. The generated skull is a playful fossil interpretation, not an authenticated historical object. The room illustration is AI-generated from the project artwork; the no-minarets art rule remains mandatory.

## Map landmarks — 5 November 2026
Gold city medallion and intact/ruined Rumeli scenes: OpenAI image generation, user-supplied icon reference. See MAP-LANDMARKS-HANDOVER.md for source files, prompts and interpretive-art limitations.


## Senate hall — 5 November 2026
- Asset: Assets/Resources/UI/MidnightMosaic/Scenes/senate.png
- Source: OpenAI image generation; original exec-11ac9765-2589-4e8b-810c-15ec062fd449.png.
- Use: The Senate support/delegation screen. Historical-fiction Blachernae-inspired reconstruction; see SENATE-PRESENTATION.md for architectural references. No external licensed image incorporated.


## Province inspection landscapes (5 November 2026)
Eleven OpenAI-generated original landscapes, installed in UI/MidnightMosaic/Scenes/Provinces. Historical inspirations, source URLs, limitations and file provenance: [PROVINCE-LANDSCAPES.md](PROVINCE-LANDSCAPES.md) and province-art-files.json. These are interpretive regional scenes, not exact reconstructions. All original PNGs visually reviewed; no third-party photographs incorporated.

## Nameless Fortress (5 November 2026)
Original OpenAI-generated mountain ruins and transparent vulture, installed in UI/Maps/Landmarks. Full prompts, generated originals and location references: Documentation/NAMELESS-FORTRESS.md. No third-party image copied.

## Mehmed gift illustrations (5 November 2026)
Eight original OpenAI-generated square still-life images in UI/Gifts. Prompts and original file provenance: gift-art-manifest.json. Presentation and validation: GIFT-GALLERY.md. No third-party imagery used.

## Version 41 diplomatic expansion — 5 November 2026
- Candar open/closed scenes: original OpenAI-generated artwork, files exec-73750897-a8d3-41df-bbff-4aadef1af557.png and exec-479b1dff-d3b0-4c42-9225-2e07ccc53776.png. Installed as Scenes/candar-open.png and candar-closed.png. Interpretive Sinop court/doorway; no exact reconstruction claimed. Hover silhouette traced against this artwork.
- Copa port/compound: original OpenAI-generated artwork, corrected originals exec-625a8ae6-8966-415c-a26d-2a143cc1677c.png and exec-160f906a-dfc0-4003-96b8-9e5b37c8e92a.png. Installed as Scenes/copa-port.png and copa-compound.png. Earlier cartridge-tube anachronism rejected; final costumes corrected. Fictional meeting in an existing trading port.
- Ragusa representative scenes: previously generated staged assets from ragusa-next/art/GazulusForeground.png and CotrugliForeground.png, now installed in UI/Diplomacy/Ragusa. Historical framing and voices preserved from ragusa-next/writing/history-and-voices.txt. Dialogue is fictional.
- Caffa harbour: staged diplomatic-expansion/caffa-harbour.png, installed as Scenes/caffa-harbour.png. Generated artistic reconstruction; historical cast and limitations in diplomatic-expansion/historical-cast-and-rules.md.
- Theodoro: staged theodoro/mangup.png, installed as Scenes/theodoro-mangup.png. Original generated artistic reconstruction, supplied by JohanX; THEODORO-NARRATIVE-HANDOFF.txt records scope.
- No new third-party audio or photographs were added in this expansion. Existing mercenary company art reused for the Copa contract.

## Layered council painting (5 November2026)
- council-chamber-clean.png: OpenAI edit of existing original council-chamber.png; generated exec-8afd373d-14ac-4e1e-94d9-d26334a43c4e.png. Water repaired after removing the foreground ship; railing apertures shaded.1254-square alignment preserved.
- council-cloud-wisp.png: original OpenAI-generated transparent sunset cloud overlay, generated exec-5fb53ca7-9920-40e6-afad-7b749c1eedf9.png (original retained in this chat generated_images folder). No third-party imagery.
- Animated ship: exact original-painting pixels rendered through an outlined UV mesh in ChamberShipGraphic, not the generated alternate boat. Original asset provenance remains applicable.
- The ornate generated ship alternative was rejected and is not installed. Layer design and limitations: COUNCIL-LAYERED-PAINTING.md.

## Military training and shipyard backgrounds — 5 November2026
Original OpenAI-generated painterly reconstructions, installed in Scenes. Original filenames:
- training-infantry.png: exec-d043d46c-51f6-433c-81dd-566ec227653e.png
- training-archers.png: exec-8a610ac8-690d-464e-825e-7a95f70e10de.png
- training-cavalry.png: exec-b3a6c348-4e04-4f81-b803-ff11eb8a8548.png
- training-varangians.png: exec-039e0fac-abf4-4b19-88d9-3d1ed9c4b005.png
- training-cannons.png: exec-36d5302f-aba7-41aa-b6cb-531d53160ec0.png
- shipyard.png: exec-575d7199-f2de-4853-b36a-cc491b97e319.png
Archery revision replaces exec-c1a72cfa-98ff-44df-a48b-9b1926b43d8c.png: archers share a firing line with separate parallel lanes. No third-party pictures incorporated. Equipment references: https://www.metmuseum.org/art/collection/search/27686 (brigandine/Chalcis hoard); https://www.metmuseum.org/art/collection/search/23229 (Italian sallet); https://art.thewalters.org/object/51.580.1/ (barbute circa1450). These inform equipment, not exact reconstruction of Byzantine uniforms. Varangian appearance in1453 is interpretive. Shipyard reserves clear foreground for a future visitor; no character added.

## Fleet orders command room — 5 November2026
Original OpenAI-generated fleet-orders.png, source exec-54f6bfea-3230-4258-8dbe-383727d05565.png. Captain and lieutenant studying charts, with astrolabe and compass; no looking glass or modern navigation instruments. Installed Scenes/fleet-orders.png. Historical-fiction illustration, no external photograph copied.

## Senate landmarks — 5 November 2026
OpenAI image generation edit of the existing original Senate painting. Output exec-3c663de1-fe7c-4926-991a-63fb3320ccff.png installed as Scenes/senate-landmarks.png; original senate.png retained. Enlarged fictional research tower with astronomical instrument and Byzantine galley under construction at the quay. Foreground delegations, emperor and Hagia Sophia composition preserved. No third-party imagery incorporated. This is historical-fiction scenery, not a claim about the actual 1453 Senate chamber or these landmarks' sightlines. Hand-traced interaction coordinates are in SenateHotspots.cs.
