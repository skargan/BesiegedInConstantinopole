# Council layered painting — 5 November2026

Presentation only; no campaign clock, flags, RNG or gameplay changes. Original council-chamber.png preserved as fallback and exact source for ship UVs. New council-chamber-clean.png repairs the ship's old water patch and fills railing openings with shadow. Same1254x1254 registration.

Rendering: clean room painting; full-frame exterior city/water plate behind a two-arch stencil; transparent painted cloud wisps with staggered speeds and sin-squared opacity; right-window-clipped original ship mesh and faint wake; existing room shade/candle effects. The unchanged sunset and distant city are currently one exterior plate, not separate simulated scenes. SetExterior(Texture2D) replaces that plate without moving architecture. Future burning/night profiles are not implemented.

Ship uses polygon/UV sampling from original painting (not a regenerated substitute). Bezier route starts behind central column, moves right and slightly upwards, scales1to0.57, passes beyond right edge, waits20seconds, then starts hidden again.44-second journey/64-second cycle. No reverse jump on open water. Cloud periods42–57seconds staggered; alpha reacheszero at every wrap. UI masks use common full-frame coordinates, original aspect ratio retained. Decorative graphics never intercept clicks. Reduced motion remains respected.

Asset creation: retain master dimensions, camera and framing; no baked UI. Future exterior profiles use same full-frame layout and skyline/arch registration. The small railing apertures stay dark; avoid rendering exterior fragments through them. Do not add minarets. No new dependency or3D scene.

Verification: final build/capture evidence in Builds/TheodoroPreview/Captures/Chamber. Existing publicVersion41 unchanged. Generated alternate ornate ship rejected because it differed from the source vessel; not installed.

Final verification: Unity Windows preview build succeeded;125 chamber checks passed. Inspected source/repaired art and journey screenshots at10/22seconds; resize captures cover1600x900,1280x720,1280x960. Wrap boundary tested at41.999/42.001seconds with opacity below0.00001; quiet interval verified at44.01/63.99. This is focused visual/lifecycle verification, not a controlled performance benchmark.
