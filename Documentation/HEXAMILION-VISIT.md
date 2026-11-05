# Hexamilion illustrated visit — 5 November 2026

The geopolitical map opens a wide view of the Hexamilion. A shallow horizontal description panel and Return button cover the foreground, leaving the wall, towers and coastline visible. The visit is presentation only: it grants no resources, changes no diplomacy and advances no time.

The marker at normalised map coordinates `(0.2985, 0.3236)` follows the northeastern Morea / southern Athens boundary in `Assets/Resources/Content/WarTheatre.json`, near the Isthmus of Corinth. It deliberately does not reuse Morea's inland label or its northern army-front point. Like the other landmarks, this is placement on the illustrated map rather than a surveyed geographic coordinate.

Runtime artwork: `Assets/Resources/UI/Maps/Landmarks/hexamilion-wall.png`. The generated painting is an atmospheric interpretation, not an archaeological reconstruction. Its generation provenance remains with the parent artwork handover.

The historical text identifies the six-mile name, the Isthmus defence, Constantine's restoration before his accession and Murad II's breakthrough in 1446. It deliberately avoids a precise restoration year. The closing lesson about soldiers, provisions and allies is original narrative writing.

Source: Rhys Carpenter and Antoine Bon, *The Defenses of Acrocorinth and the Lower Town*, Corinth III.2 (1936), especially the medieval fortifications and historical background: [American School of Classical Studies at Athens, open-access volume](https://www.ascsa.edu.gr/uploads/media/oa_ebooks/oa_corinth/Corinth_III_2.pdf).

Integration: `HexamilionVisitUI.cs` mirrors the Rome visit's overlay canvas, 1600 × 900 scaling and fitted painting. Return closes this visit only. Escape is routed by `ControlSingleton` before the underlying map close handler, rather than competing with it in a second Update method. Runtime validation is owned by the combined build; source inspection alone does not establish a passing playtest.
