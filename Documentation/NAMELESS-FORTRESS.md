# Nameless Fortress

User-approved atmospheric map landmark, 5 November 2026. Permanent marker near Alamut in the Alborz mountains southwest of the Caspian Sea. Existing map artwork is schematic, not georeferenced: artwork-space anchor (.925, .255) is an approximation to Alamut's real location (36.4447 N, 50.5861 E). Location reference: https://en.wikipedia.org/wiki/Alamut_Castle and https://mapcarta.com/12586476 . No claim that the fictional ruins are an exact historical reconstruction.

MapLandmarks.Add opens NamelessFortressUI. Return/Escape closes the overlay and leaves the underlying map intact. No rewards, costs, journal flags or campaign simulation introduced. Message: “You are far from home, friend. There is nothing for you here. For now…”

New original OpenAI-generated PNG assets: UI/Maps/Landmarks/nameless-fortress and nameless-vulture. Vulture has alpha; a single lightweight UI animation moves and banks it in an elliptical orbit above the ruins, scaled with the painting. AspectRatioFitter preserves the painting at different screen ratios.

Background prompt: Historical PC game realistic detailed 16:9 painting; ruined Alamut-inspired fortress in fifteenth-century Alborz mountains; low broken stone walls on steep narrow summit, layered jagged mountains, empty path, cold blue-grey shadow, amber sunlight, clear sky for animated bird. No people, birds, text, UI, minarets or modern buildings. Original: exec-fc65b462-a143-4690-a081-5e8544a39b39.png.

Bird prompt: isolated realistic soaring vulture viewed slightly from below, broad outstretched brown wings, finger feathers, small pale bare head, fanned tail, transparent background, full bird visible. Original: exec-9a9da757-25a8-4d26-8642-8287e84e8bff.png.

Development verification switch: -nameless-fortress-capture. Checks texture loading, text fit, bird motion and Return at 1280x720 and 1920x1080; screenshots under Captures/NamelessFortress. Build/runtime status to be recorded after verification.

Verified: combined IonianPreview build succeeded. Runtime checks passed at 1280x720 and 1920x1080 for both textures, text fit, visible vulture movement and Return. Screenshot fortress-1280.png visually reviewed: painting fills view, bird alpha clean at game scale, message readable. Map marker uses existing MapLandmarks click/hover implementation; geographic placement remains approximate on the illustrated map. Private preview only, next publication pending.
