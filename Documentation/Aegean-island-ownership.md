# Aegean island coastlines — 6 November 2026

The map now traces 65 painted island components at the native 2172 × 724 artwork resolution. The work replaces the coarse Chios, Lesbos, Negroponte, Naxos, Rhodes, Kythera and Crete outlines and adds the remaining southern Aegean island polygons. Small offshore rocks follow their neighbouring island jurisdiction. No new economic provinces or diplomatic simulations are added.

## Ownership and diplomatic grouping

- Genoese sphere: Chios and Oinousses (Giustiniani/Maona), Psara, Samos, Ikaria (Arangio); Lesbos retains its Gattilusi identity.
- Hospitallers: Rhodes and its neighbouring island possessions, including Kos, Kalymnos/Leros, Nisyros, Tilos, Symi and the northern island approaches.
- Venetian sphere: Crete and its satellites, Kythera/Antikythera, Negroponte/Skyros, Tinos/Mykonos and the Latin Cycladic lordships. Karpathos/Kasos are Cornaro possessions; Astypalaia is Querini; Andros is Sommaripa; Naxos is Crispo.
- Salamis follows the Athenian mainland; Hydra/Spetses and nearby Moreote islets follow Morea.

Venice is the existing diplomatic destination for the Latin island lordships, **not a claim that all were directly administered by Venice**. Their short descriptions distinguish the ruling houses where relevant. Genoa similarly groups several distinct maritime lordships. This preserves the game's established dependency-selection model without inventing dozens of courts. The geography painting simplifies some neighbouring islands into joined silhouettes; the tracing follows the actual painting rather than drawing water-dividing borders through them.

## References

- Foundation of the Hellenic World, Latin rule in the Aegean: https://www.ime.gr/chronos/projects/fragokratia/en/webpages/aegean_gen.html
- William Miller, Greece and the Aegean under Frank and Venetian domination: https://cristoraul.org/english/readinghall/THIRDMILLENNIUMLIBRARY/THE-EASTERN-ROMAN-EMPIRE/15-GREECE-AND-THE-AEGEAN-UNDER-FRANK-AND-VENETIAN-DOMINATION_1204-1571.html
- Municipality of Karpathos, Cornaro rule: https://karpathos.gr/en/discover/istoria/
- Municipality of Kalymnos, Hospitaller administration: https://kalymnos.gr/wp-content/uploads/2025/05/history-of-kalymnos.pdf
- SearchCulture / historical heraldic record of the Querini: https://www.searchculture.gr/aggregator/edm/pandektis_eraldika/000081-10442_45199?language=el
- Municipality of Astypalaia, Querini castle: https://www.astypalaia.gr/%CF%84%CE%B9-%CE%B8%CE%B1-%CE%B5%CE%BE%CE%B5%CF%81%CE%B5%CF%85%CE%BD%CE%AE%CF%83%CE%B5%CF%84%CE%B5/%CE%B1%CE%BE%CE%B9%CE%BF%CE%B8%CE%AD%CE%B1%CF%84%CE%B1/%CF%84%CE%BF-%CE%B2%CE%B5%CE%BD%CE%B5%CF%84%CF%83%CE%B9%CE%AC%CE%BD%CE%B9%CE%BA%CE%BF-%CE%BA%CE%AC%CF%83%CF%84%CF%81%CE%BF-%CF%84%CE%B7%CF%82-%CF%87%CF%8E%CF%81%CE%B1%CF%82/

## Reproduction

`Tools/trace_aegean.py` inventories connected warm-coloured land components. `--apply` writes their coast polygons to the catalogue, preserving unrelated entries. It removes only collinear raster vertices, avoiding shortcuts across bays. `Builds/aegean-components.png` labels the authoring components; `Builds/aegean-traced.png` previews the exact catalogue geometry. Neither preview is a replacement background image.

`-aegean-capture` in a development player verifies the instantiated shapes, generated meshes, grouped hover, hover exit and selection for every changed island, and captures the southern Aegean.
