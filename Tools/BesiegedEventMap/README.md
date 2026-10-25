# Besieged Event Dependency Map — current Unity flow

Open **Besieged Event Map.html**, or use the existing local browser at http://127.0.0.1:8765/.

## Review in three steps

1. **Dependency map:** preparation timeline on the left, closed selectable pool beside it. Blue = fixed date; gold dashed = flexible month; green = base option; purple = career unlock. Use the section selector for random events, conditional chains or unscheduled stories. Scroll to zoom and drag to pan; Fit gives a full-section overview.
2. **Preparation calendar:** read month by month, inspect all choices, and rehearse one selection per flexible month. Previous / Next in the inspector lets you read the events sequentially.
3. **Needs attention:** two unscheduled stories and three provisional balance reviews. Standalone random events are legitimate; they are not reported as broken chains.

The current view includes 21 selectable preparations, ten flexible slots, 24 random-event records (including the harvest follow-up), conditional Galata/Giovanni and Shaherezade events, and the fixed calendar. The actual Unity choices are imported, including Intelligence, Scorched Earth and the April reconnaissance routes. A successful reconnaissance adds 30 percentage points to Straitcutter infiltration (45% → 75%).

**Reference archive** retains 84 narrative records and 98 legacy spreadsheet records, plus the full Unity source audit. They are hidden from the working view by default. The old planning framework is preserved in planning/preparation-v4-reference.json.

Map edits and monthly reservations are developer notes; they do not write back to Unity. Export JSON to retain edits. Refreshing the bundled source gives a new browser workspace rather than silently overwriting earlier edits.

## Regenerate from the actual project

Run scripts/import_sources.py with --repo pointing to the desktop Unity root; then scripts/current_flow.py; then scripts/next_campaign_review.py and scripts/apply_murder.py to reapply authored story overlays; then node scripts/package.mjs --repo <Unity root>. The curated schedule/dispatch relationships are explicit in current_flow.py; review that mapping when the game's scheduling code changes. Ordinary Yarn jumps are imported automatically.

Validation: node --test tests/current-flow.test.cjs. The current checks cover graph integrity, calendar/pool separation, persistent achievement gates, chain classification, governance follow-ups, review flags and reference retention. Historical v4 assertions are retained as reference text under tests/reference.

The Unity preparation integration separately passed 52 checks. This map is a snapshot and structural review tool, not a simulation or proof that every narrative branch has been played.


## Brutal Murder investigation

The Names Beneath the Wax adds seven stable murder1–murder7 nodes, four culprit variants and saved evidence/verdicts. The original draft remains an archive reference. Full authored data is in planning/next-campaign/Brutal Murder chain.json; reapply it with scripts/apply_murder.py after rebuilding the base flow. The Unity MurderPreview passed119 exported-player checks, including every dialogue stage and the campaign-ending recollection. Preview verification does not mean the chain is included in a published numbered release.
