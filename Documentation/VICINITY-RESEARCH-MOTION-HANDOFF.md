# Vicinity, research phase and motion handover

6 November 2026 — ConstantineX. Integrated on main; private Windows player verified. Public/Desktop release remains Version43. No numbered release or save format change.

## Delivered
- Monastic research produces no progress or secrets during monthly preparations; processed preparation dates cannot be banked for catch-up. Existing allocations and progress persist. Production begins with siege days, at the existing 1,000-point threshold and existing research costs.
- Approved native-script names retained. Noto Serif primary with Georgian/Naskh fallbacks avoids mixed serif/sans Cyrillic. Short/Full retain Crimson. The fixed regional name plaque and selector follow the shared naming preference, including Genoese island names.
- Symbols gallery removed from active map toolbar; existing mechanical symbols retained. Bosnia blue and Serbia red differentiated. Serbia has an Ottoman-vassal presentation suffix while not intervening against the Ottomans; no new political system.
- Large circular imperial C seal on European side opens Vicinity of Constantinople. Geographic map covers both Bosphorus banks, Golden Horn and Marmara approaches, with terrain reserved for a later camp. Palace/Galata/Fleet/Strait-Cutter routes reuse existing screens. Other historic sites are inspection-only, no new mechanics. Studios is the locality/monastery rather than a claim of locating a disputed siege castle.
- Corrected missing lower-right triangle in both window mask fans (JohanX commit6ed9b14); ship completes its existing passage and exits behind the jamb. No new path/timing or campaign logic.
- Eagle original64 poses retained, faster4-second blended cycle and independent18-second upper-right orbit, reduced scale. Reduced-motion mode unchanged.

## Verification
Fresh Unity Windows Development export: Builds/VicinityPresentationPreview/Saving Constantinople.exe. Build succeeded. SAVE_AND_TECH_CHECKS57 passed. Isolated-player results: MonasticPhase52; VicinityMap189 (native glyph/fallback tests included); ChamberShip75; NamelessFortress1436; Court setup1 and compact map473. All passed. Detailed suite evidence: Vicinity presentation verification.json; player captures under the private build Captures folder. These are focused regression checks, not a full campaign balance test.

Visually inspected final1280x720 local map, native Wallachia name/plaque, council ship passage frame and upper-right eagle. Prior candidate visual defects (transparent map backdrop, late generic square border on capital seal, text encoding artefacts) were corrected and regression assertions added. Local map also checked1600x900; eagle suite covers1280/1920. Artwork source/guide: ArtSources/Cartography/ConstantinopleVicinity. No new simulation flags, resource stocks or historical name strings.

## Sources and limits
Natural Earth public-domain coastline guide and generation provenance recorded in ASSET SOURCES.md and local sources.json. Painting is historical-fiction geography, not a surveyed atlas. Marker labels cluster near the city at Fit; zoom permits closer inspection. No Dardanelles/Princes’ Islands extension, new camp simulation or new fortress mechanic. Exact Therapia/Studios dictation interpreted conservatively as existing historic localities.

## Next action
Ready for the next authorised combined publication. Do not silently overwrite public43 or claim Desktop publication from this private preview. JohanX owns isolated drafting of new AGENTS.md/PROJECT_COORDINATION.md/ARCHIVE.md; existing coordination records remain untouched pending lead cutover.
