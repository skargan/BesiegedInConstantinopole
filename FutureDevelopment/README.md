# Future development archive

This is the retrieval index for unfinished, superseded and DLC work preserved during the Version 39 consolidation on 4 November 2026. Nothing in this directory is imported into the playable game. Presence here means **preserved**, not implemented or approved for release.

## Find the work

| Content | Location | Present state and next useful step |
|---|---|---|
| Emperor XP, attributes, court quests and conversations | `Prototypes/emperor-court-dialogues.zip` | Older implementation and artwork. User explicitly chose archival for this release. Reconcile with the retired fencing/leadership systems and current save model before revival. |
| Seven-stage Brutal Murder investigation | `Prototypes/brutal-murder-investigation.zip` | Source, Yarn and design texts preserved. Requires integration into the current event scheduler and save/return flow. Also present in the older court package. |
| Interactive city map and painted wall hotspots | `Prototypes/city-map-navigation.zip` | Earlier navigation prototype. Reconcile with current palace and wall-repair routes before adoption. |
| Combined older court base | `Prototypes/court-integration-base.zip` | Historical combined snapshot, retained for comparing the two prototypes above; not a second authoritative game. |
| Candar secret negotiations | `Prototypes/candar-secret-negotiations-20261103.zip`, `Staged/candar-next/` | Ransom, secrets and paid tactical postponement source. Earlier compilation/pure-roll evidence exists; transaction/save/scheduler and map ownership issues remain unverified. Paphlagonia remains Ottoman in the live game. |
| Ragusa | `Staged/ragusa-next/` | Character art, content and source handovers. Compiler caches and duplicate frozen checkouts excluded. Read its handover before integration. |
| Malta mercenary marketplace DLC | `Staged/diplomatic-expansion/MALTA-MERCENARIES-DLC-DESIGN.txt` | Design only: roster, regional voyages, instalments, non-cash bargains and incidents. Latest decisions: blockade risk stacks, dispute agent is spent, baseline <=75 days but delays can extend it. |
| Gazaria / Caffa and Circassian trading coast | `Staged/diplomatic-expansion/` | Design staging and available Caffa artwork. Separate from Malta; do not silently substitute its rules. |
| Defeat-specific ending artwork | `Staged/ending-art/` | Available ending assets retained. The current release still uses its existing ending presentation; cause-specific art wiring is unfinished. |
| Tavern / unique-event material | `Staged/tavern-event/` | Staged content and handover, not a claim of a working live event chain. |
| Giustiniani artwork | `Staged/giustiniani-art/` | Supporting art retained for the future character/event pass. |
| Original final-art handover | `OriginalArt/` | All135 source PNGs, original manifests and provenance retained. Version39 adds74 missing event mappings using64 originals and preserves58 existing mappings. Other originals remain available for review/replacement. |
| Retired factions, merchant favour, fencing and leadership | `../Documentation/Retired systems/` | Already preserved in the repository; remain retired. Do not restore by wholesale-merging old prototypes. |
| Narrative expansion plans | `../Tools/BesiegedEventMap/planning/next-campaign/` | Court stories and future event designs; source evidence and approval status remain distinct from runtime content. |

## Outstanding work with no completed implementation claimed

- The requested five-choice **Our Turks** rewrite, Orhan loyalty, battalion flag and hidden infiltration consequences still need a dedicated implementation/verification pass. The live event has not been represented as the completed requested rewrite.
- Decision Journal exists; broader epilogue and future-event consumption of its story flags remains expansion work.
- Ionian courtship and naval-base choices remain visibly under development; the implemented refuge/exile agreements are separate.
- A long-campaign balance assessment of provincial food, hunger, reinforcements and mercenary pricing remains future work. Focused regression checks are not that assessment.
- Optional symbolic day/night chamber progression, additional environmental storytelling and broader assault visual expansion remain design ideas, not promised functionality in this release.

## Recovery and provenance

`manifest.json` records SHA-256 hashes and source paths/commits for282 archived files. Each prototype ZIP has `ARCHIVE-PROVENANCE.json` containing its branch tip and per-file hashes. The ZIPs hold branch-changed files, not standalone runnable Unity projects. Historical tips are preserved as `archive/2026-11-04/<branch-suffix>` tags; use those tags for a full checkout. The remote keeps only `main` after release cleanup.

`../Documentation/Release39/branch-audit.json` records the pre-integration comparison of every remote branch. `art-integration.json` lists precisely which artwork mappings were added. Existing worktrees are retained locally; they must be treated as historical/staging copies, not publication sources.

To resume an item: read its handover, identify the current owner, compare against `main`, take only the relevant changes, and validate the affected game flow. Never copy an entire old `CouncilUI.cs` or `VariableSingleton.cs` over the current version.

## Older local working files

`Prototypes/local-preconsolidation-working-tree.zip` additionally preserves 27 uncommitted files from the earlier Besieged-release18 checkout, based on fc5ad71. Much of this is superseded presentation work. Its internal manifest records original paths and SHA-256 hashes. The original checkout is retained, detached at its previous commit, with dirty files unchanged. Do not import these older whole files over current main.
