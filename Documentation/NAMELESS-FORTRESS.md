# Nameless Fortress

User-approved atmospheric map landmark, 5 November 2026. Permanent marker near Alamut in the Alborz mountains southwest of the Caspian Sea. Existing map artwork is schematic, not georeferenced: artwork-space anchor (.925, .255) is an approximation to Alamut's real location (36.4447 N, 50.5861 E). Location reference: https://en.wikipedia.org/wiki/Alamut_Castle and https://mapcarta.com/12586476 . No claim that the fictional ruins are an exact historical reconstruction.

MapLandmarks.Add opens NamelessFortressUI. Return/Escape closes the overlay and leaves the underlying map intact. No rewards, costs, journal flags or campaign simulation introduced. Message: “You are far from home, friend. There is nothing for you here. For now…”

New original OpenAI-generated PNG assets: UI/Maps/Landmarks/nameless-fortress and nameless-vulture. Vulture has alpha; a single lightweight UI animation moves and banks it in an elliptical orbit above the ruins, scaled with the painting. AspectRatioFitter preserves the painting at different screen ratios.

Background prompt: Historical PC game realistic detailed 16:9 painting; ruined Alamut-inspired fortress in fifteenth-century Alborz mountains; low broken stone walls on steep narrow summit, layered jagged mountains, empty path, cold blue-grey shadow, amber sunlight, clear sky for animated bird. No people, birds, text, UI, minarets or modern buildings. Original: exec-fc65b462-a143-4690-a081-5e8544a39b39.png.

Bird prompt: isolated realistic soaring vulture viewed slightly from below, broad outstretched brown wings, finger feathers, small pale bare head, fanned tail, transparent background, full bird visible. Original: exec-9a9da757-25a8-4d26-8642-8287e84e8bff.png.

Development verification switch: -nameless-fortress-capture. Checks texture loading, text fit, bird motion and Return at 1280x720 and 1920x1080; screenshots under Captures/NamelessFortress. Build/runtime status to be recorded after verification.

Verified: combined IonianPreview build succeeded. Runtime checks passed at 1280x720 and 1920x1080 for both textures, text fit, visible vulture movement and Return. Screenshot fortress-1280.png visually reviewed: painting fills view, bird alpha clean at game scale, message readable. Map marker uses existing MapLandmarks click/hover implementation; geographic placement remains approximate on the illustrated map. Private preview only, next publication pending.


## Smooth vulture circuit — 6 November 2026

Root cause of the reported sharp turn: the old localScale.x switched instantly between +1 and -1 when cosine changed sign. Replaced by VultureFlight: a deterministic 32-second ellipse above the highest tower, continuous seven-degree banking and perspective scale from 0.78 to 1.0. Transform scale always remains positive. Four painted views change via eased pose blending over approximately 3.2 seconds near each turn; no instantaneous mirror or visible loop reset. No per-frame allocation or campaign randomness. Existing ReducedMotion preference freezes the current pose; closing the visit destroys the animation.

New alpha PNG atlas nameless-vulture-views.png contains four views of the existing brown vulture. A static UV reversal on the left-facing atlas cell corrects the generated pose direction; it is not a runtime sign change. Two RawImages sample the same texture. This keeps feather alpha and avoids a runtime GIF decoder or a full-screen video layer. Original vulture asset retained. Source: OpenAI image generation edit exec-2cd3f5b9-7c9f-4e99-be30-a455cc5c6aa5.png, with original bird as reference; no external artwork. Alpha range verified 0–255.

Windows preview build succeeded. 9,629 checks passed: 1,920 frame samples (five continuity/bounds assertions per sample), exact circuit closure, two-resolution live motion/text/texture/Return checks and positive transform checks. This is numerical presentation verification, not thousands of playthroughs. Captures at eight phases were generated at 1280x720 and 1920x1080; transition and side-flight screenshots visually reviewed. Review GIF Documentation/Previews/vulture-loop.gif encodes 128 Unity-rendered frames at 640x360, four frames/second, one 32-second circuit. The game animates at its normal frame rate; the GIF is a preview only and has an opaque scene background. Production sprites retain true alpha transparency.

Public Version 41 unchanged; ready for next publication.
