# Palace visual integration — JohanX, 5 November 2026

Implemented source: redesigned palace background, five library shelves, grouped Fleet ship highlights, private chamber and return portal, five illustrated paginated books with paper sound, cropped-sprite silhouette click mapping, warmer figure tint and broader contact shadow.

Latest approved correction: study doorway at far left, bookshelf immediately right, flat parchment entirely within tabletop. Galata belongs to continuous northern land, not an island. Generated geography is illustrative, not a surveyed map.

## Character staging for future work

- Foreground slot: feet (0.285, 0.07), existing visitor. Keep this floor empty in background.
- Behind-table slot: suggested feet (0.59, 0.34), height 0.43. Requires an independent table foreground occlusion layer before enabling a second visitor.
- Garden-door slot: suggested feet (0.82, 0.34), height 0.36. Avoid covering the garden portal.
- These are reserved art positions, not a change to daily visitor selection. Do not add people or spend RNG merely to fill the room.
- Match camera height, perspective, warm light from right and cool ambient light from windows. Deliver sprites with clean straight alpha, no coloured matte, no baked rectangular shadows. Test at actual in-game size. Use alpha-aware mipmaps to prevent tiny gold trim shimmering.
- Use a soft broad floor shadow plus a denser contact core under feet. Shadows must remain attached to feet at every resolution. Avoid a permanent golden outline: gold is interaction feedback only.
- Foreground furniture must be delivered as a separate aligned transparent layer if characters stand behind it. Never paint characters into reusable backgrounds.

## Collectible objects — proposed, not enabled

The private chamber contains five empty display alcoves, lectern, pedestal, mirror and wall recess. The first proposed event reward is a SMALL geographical globe using contemporary Ptolemaic learning and incomplete geography. Keep original large globe art in the original source/reference image; do not introduce it as an automatic reward. Future objects could be manuscripts, diplomatic gifts, relics or captured standards; each requires an approved acquisition event and journal entry.

## Library

All five supplied texts are preserved in Content/PalaceBooks.json. They are typeset over generated illustrations; text is not baked into pixels. Page navigation and Escape close the reader. Sound follows existing effects volume and mute. Historical fact-checking is a separate editorial pass: supplied dates and the existing Madalena/Theodora naming difference have not been silently rewritten.

Artwork generated with built-in image generation. Prompts: Byzantine palace precise-object edits retaining mosaics, perspective and sunset; exchange doorway/bookshelf; flat correct peninsula map fully on tabletop. Private chamber: empty display alcoves, bronze mirror, sunset and matching palette. Five parchment leaves: Renaissance brown-ink cross-hatching and gold/lapis marginal illustrations for empire, city, decline/diplomacy, emperor and fall/possible restoration, blank reading area on right. Corrected generated minarets out of pre-conquest Hagia Sophia illustrations.

Runtime/publication status must be confirmed by the build owner; standalone compilation alone does not verify rendering or clicks.
