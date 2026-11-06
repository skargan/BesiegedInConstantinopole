# Project Coordination

> Canonical operating state — 6 November 2026. Cutover authorised by the Director and ConstantineX. ConstantineX owns future maintenance and integration.

## 0. Current Snapshot

Migration base: `b1e12b1771cf154456729be602f2dfcb8015a3f6`; old state preserved on `archive/pre-three-document-migration`. Development continues on `main-current-november-2026` after cutover; `main` is retained, not deleted. Latest lead-confirmed main revision: `b1e12b1`, committed and pushed on 6 November. Public and Desktop release remains **Version 43**. Private preview verification is not publication.

ConstantineX reports a successful VicinityPresentationPreview Windows export, 57 editor checks and 2,226 focused player assertions: research 52, vicinity/native names 189, ship 75, eagle 1,436, court/map 474. Final selected screenshots inspected. This is not a full campaign balance test. Latest handover: [latest verified handover](Documentation/VICINITY-RESEARCH-MOTION-HANDOFF.md).

Next milestone: resume bounded development under this structure, then consider a separately authorised combined publication. No gameplay edits belong to this migration.

## 1. Project Identity and Vision

**Against Fate: Saving Constantinople**, short name **Saving Constantinople**. Preserve repository names, stable keys and historical release URLs. A historical strategy game about preserving the city through preparation, resource management, diplomacy and consequential decisions. Defeat should remain plausible and invite another campaign; stronger play improves survival without eliminating risk. Preserve the Creative Director’s language, humour and intent.

## 2. Agent Operating Doctrine

The Director owns vision, scope and consequential rules. ConstantineX owns shared integration, builds, publication and this document after cutover. Specialists submit structured handovers and obtain file ownership before shared edits. One accountable owner per modification; one active build owner and one release owner. Sending a message is not acknowledgement. Preserve interrupted work before reassignment.

Prefer bounded, independently verifiable increments; inspect relevant code and evidence only. Reuse valid test evidence until changes invalidate it. After two unproductive approaches, diagnose and change method. Record unrelated discoveries without expanding scope. Do not measure progress by agent activity or code volume.

Meaningful handovers distinguish proposed, approved, staged, integrated, compiled, runtime-tested, visually reviewed, committed, pushed and published. Include affected files, actual checks, limitations, ownership and next action.

## 3. Architecture and Technical Authorities

| Area | Authority and boundaries |
| --- | --- |
| Campaign | VariableSingleton and capture/restore; SaveGames persistence. Preserve stable IDs and serialized references. |
| Orders and interface | CouncilActions applies outcomes; CouncilUI, CampaignHUD and ControlSingleton present and navigate. |
| Ottoman actions | OttomanDirector, strategy content and OttomanSiegeOperations; execute scheduled actions and food consumption once. |
| Assaults | PhasedAssault temporary snapshot; frozen result committed once. Practice state remains isolated. UI breadcrumbs do not add simulation phases. |
| Content | Existing Yarn stories and structured catalogues; do not introduce a parallel content engine. |
| Food and provincial war | SharedProvisions and finite provincial cargo/reserves; debit at departure, credit on arrival. |
| Fleet | FleetOperations/FleetMissionRules; operational reporting separate from narrative scheduling. |
| Personnel | TrainingPool and existing return queues; troops abroad do not consume city provisions. |
| Journal | Existing persistent records and derived strategic status; viewing transcripts never replays effects. |
| Presentation | EventCardArt/EventArtworkView and established manifests; cosmetic motion must not consume gameplay randomness. |

Campaign calendar, battle resolution and presentation clocks are separate. Do not reorder food, fleet, political or event resolution without checking dependencies. Preserve Unity/C#/Yarn, scenes, prefabs and third-party packages; no unrelated replacement or refactor.

Supporting references: [architecture](Documentation/ARCHITECTURE_MAP.txt), [core rules](Documentation/Core%20mechanics%201.3.docx). Older specifications fill gaps; contradictions require reconciliation with approved intent and current implementation.

Known technical debt: legacy instant-assault and phased paths coexist; never commit both. Model major phases are guarded, while some UI orchestration remains. Turn progression calls multiple systems directly; preserve order before any resolver extraction. String-named campaign setters and public legacy encounter fields require narrow edits. Full deterministic campaign replay and in-progress battle persistence are not established. The old CampaignSave v1 constraint was a dated pass, not the current schema authority: inspect SaveGames before persistence changes.

Detailed current authorities: [journal](Documentation/JOURNAL-ADMINISTRATION-AND-REPLAY.md), [strategic status](Documentation/JOURNAL-STRATEGIC-STATUS.md), [fleet](Documentation/FLEET-CORE-ORDERS.md), [asset provenance](Documentation/ASSET%20SOURCES.md). Cosmetic frame changes must not spawn encounters or consume campaign RNG. Verify navigation callbacks as well as visibility.

## 4. Style and Quality Standards

The Director approved the interface-economy rules on 6 November 2026; see AGENTS.md section 5 for the general policy. Apply them within Midnight Mosaic's established visual language.

| Information or change | Placement and review rule |
| --- | --- |
| Sources, historical context, methodology | Check existing realm/history/help/reference sections first. Extend the appropriate section; link only where useful. Keep decision-critical qualifications next to the choice. Do not add a new explanatory panel merely because more source material exists. |
| Main screens | Essential state and actions; remove redundant control instructions, production caveats and irrelevant negatives such as 'No war commitment' for a court without war options. |
| Orders | Costs and commitments before confirmation; outcomes afterwards. Avoid repeating the same explanation in title, body, button and footer. |
| Additional options | Reuse spacing, typography and shared controls. Consolidate repeated actions where useful. Preserve all functionality; assess readability and art visibility before accepting a layout change. |
| Writing | Preserve approved voice, humour and context. Correct unclear references without replacing the text with generic aphorisms. Reactive dialogue must follow from the actual question, report and known state. |
| Artwork | Reserve character/object space and protect focal landmarks. Match perspective, lighting and scale; inspect at actual display sizes. |

Trace the actual visible silhouette, including clothing, arms and held objects; share the same contour between hover and hit testing. Do not substitute coarse rectangles or ellipses for requested tracing. Inspect against the displayed artwork.

Use immersive second-person present where established; distinguish documented history, interpretation and deliberate alternatives internally. Preserve material/geographic constraints (Golden Horn/Galata/Marmara, distinct wall systems). Do not invent consultation or claim every proposed event is live. No universal two-outcome rewrite: preserve existing event structures and approved exceptions. Profile measured bottlenecks; use deterministic tests for logic and human visual/experiential review for feel. Test unreachable conditions, duplicate rewards, underflow, softlocks and dominant strategies proportionately.

Generated assets need provenance, correct import settings and runtime inspection. No pre-conquest minarets. Prefer 2048×1152 event artwork with safe margins, transparent 512px icons and full figures prepared for floor alignment. Follow [art standards](Documentation/Art%20assets%20and%20presentation%20standard.txt) and [art bible](Documentation/ART_BIBLE.txt).

Source inspection, compilation, automated checks, runtime tests and visual review are distinct evidence. Preserve save compatibility by default under the new bootstrap; earlier permission to discard development saves is a scoped exception, not permanent permission to break persistence.

## 5. Current Project State

Public43 and later private work must remain distinct. Recent private changes include journal presentation, palace contours, Ottoman activity presentation, chain scrapping state, relation tile layout, ship masking and eagle motion. The latest combined preview adds vicinity navigation, native-name presentation and preparation-phase research gating.

Monastic research produces no progress during monthly preparations; existing progress and allocations persist. Siege-day production retains the 1,000-point threshold. Golden Horn chain state and Bosphorus interdiction are separate systems; scrapping the harbour chain must affect its own depiction and protection, not imply removal of the strait fortress.

Longer Isidore theology, Orban philosophy and the Notaras/Giustiniani frenemy arc remain approved writing or development material unless independently evidenced as integrated. Do not infer implementation from this conversation alone.

## 6. Active Workstreams

| Owner | Scope/location | Confirmed state | Next action |
| --- | --- | --- | --- |
| ConstantineX | Main integration/build/release | b1e12b1 pushed; private preview verified; public43 unchanged; freeze released | Review documentation cutover and own next release |
| JohanX | main-current-november-2026; isolated coordination-migration worktree | Documentation cutover authorised; root documents, evidence audit and legacy exporter safeguards only | Hand over canonical maintenance to ConstantineX |
| OrbanX | Historical specialist stream | Inactive; no new assignment confirmed | Preserve historical handovers; do not assume active ownership |

Other historical agent names are not evidence of live assignments. Verify before allocating shared files.

## 7. Binding Decisions

- Latest explicit user decisions establish intent; source establishes implementation; executed checks establish verification. Unresolved conflicts must be visible.
- Isidore returns in October before flexible preparation content. Welcome: +5 Isidore/−5 Gennadios; Orthodox robes: −5/+10; no action: no loyalty effect. Full Union exiles Gennadios; rejection exiles Isidore; symbolic Union and wartime compact retain both. Earlier both-present-at-start proposal is superseded by the October implementation.
- Character loyalty remains; retired general faction/prototype systems must not be revived from old notes. Separate fencing is folded into martial skill.
- Discovery achievements can apply in Normal and Ironman; performance achievements remain Ironman-only. Automated/practice runs do not unlock them.
- Aid transports do not become owned warships unless explicitly granted. Sea hazards apply to sea transport. Operational reports are independent of daily narrative events.
- Fixed dated milestones take priority over ordinary follow-ups. Preserve approved dates; do not silently make historical milestones random.
- One provision feeds 1,000 people/day; medical extra consumption is based on actual treatment. Do not revive superseded rounded consumption rules.
- Preserve the approved native-name presentation preference without renaming stable country IDs.

Additional approved decisions, reconciled against later handovers:

| Decision | Status and authority |
| --- | --- |
| Ragusa | Resident envoy required to conclude agreements; recall does not cancel them. Only the monthly spokesman interacts. Return goes to theatre map. Verified before42. |
| Candar | Independent Sinop coastal `candar_realm`; inland Ottoman Paphlagonia retains `candar`. No extra provincial economy. Integrated before42. |
| Fleet | Naval Warfare preparation effects deferred; five texts under development. Golden Horn defence has no routine reports; exact final ranges and independent rare rolls in [Fleet rules](Documentation/FLEET-CORE-ORDERS.md). Verified before42. |
| Copa | 100 provisions OR350 gold weekly, finite2,400 reserve,14-day baseline voyage, existing hazards and closed-strait departure hold. Approved5Nov; existing saved convoy lifecycle. |
| Theodoro | Once-only free+10 goodwill;1,500 gold for multiplicative+5% total archer lethality; Dismounted Champions unlocks80 cavalry for10,800 gold, half advance/half arrival. Verified211 diplomatic+275 mercenary checks before42. Reincorporation/Bank takeover remain future. |
| Food tuning | June–August harvest and13,200 opening provincial provisions are provisional balance, not historical measurements; long-campaign balance pending. |
| Discoveries | Once per campaign: alchemy+1secret; letter+1secret; treasure2,500gold+1secret; fossil journal only. Alchemy resets on leaving/60seconds inactivity. Bronze ring drags horizontally or switches sides on click. Implemented; old 'not yet' label superseded. |
| Research | Compact catalogue and selected detail; stable tech IDs/costs. Initial monks split equally, odd remainder care; restore never reapplies default, recruits join care. Latest siege-only progress rule in section5. |
| Appearance | Warm side-lantern button lighting; realistic leather/aged-gold point/grip cursor; council desk/windows/candles preserve navigation and quotations. Approved and implemented presentation. |
| Senate | Five traced delegation groups, central emperor, Galata envoy/guards/banner; low-support variants deferred. Support/Galata mechanics unchanged. |
| Court religion | Ruling court, not population. Bosnia Catholic under Tomaš; Lithuania/Gazaria/Cyprus/Athens/Ionian Catholic; use structured Definitions.Catholic. Non-Ottoman Powers is the map view name. |
| Names | Most Serene Republic of Venice; intentional Officium Gazarie; approved native scripts/Latin readings. Fixed map plaque, no floating country tooltip. Mercenary historical H icon cancelled; retain descriptions/departure. Naples/Sicily/Sardinia separate lands, one Alfonso court. |
| Navigation/audio | Research and Rediscovery title; shipyard Return to Cannons; audio/music On defaults and saved preferences preserved. |

Dates and original approval wording are recoverable through the [coverage audit](Documentation/COORDINATION-MIGRATION-AUDIT.md). Source decisions about maintaining Word/TXT and the sole-main-branch policy are superseded only for this expressly approved migration.

## 8. Priorities and Release Plan

Finish and polish coherent work before adding scope. ConstantineX approved documentation-only cutover on6November. Preserve originals for evidence; legacy exporters require explicit archival output and cannot overwrite canonical state. New operational updates belong here, not in legacy JSON/Word. Completed work and superseded decisions receive concise archival entries at meaningful checkpoints.

For game releases, retain actual source revision, checks, package identity and publication location. A private successful export does not update Desktop or GitHub release assets. Do not package personal captures, caches or saves.

## 9. Version Changelog

| State | Revision/evidence | Scope |
| --- | --- | --- |
| Public43 | 04ed197a648b7bdb4a0af68d3a2c49978590093c; [publication record](Documentation/Version43%20publication.json) | Last recorded public/Desktop release |
| Private journal | 6e07f60; lead reports 1,130 runtime +57 editor checks | Journal/palace presentation integration |
| Private combined | b1e12b1; latest lead handover | Vicinity, research phase, names, ship and eagle; 2,226 focused player assertions |

[Historical records](ARCHIVE.md) preserve earlier versions. Stale PROJECT_STATUS Version32 is not the current release.

## 10. Current Handover

The Director and ConstantineX authorised cutover on6November; no source/build work was active. Original Word/text/JSON evidence remains unchanged; archival branch preserves the full pre-cutover tree. Preserve unrelated `.vscode/settings.json` and `Tools/__pycache__/` in consolidation. ConstantineX resumes current-state maintenance and development on the new branch after handover.

This is ConstantineX — 6 November 2026 — BLUF: the approved combined Biševo composition is retained and its encounter is implemented in a private preview. No numbered publication.

Biševo: island-only entry, shore/sanctuary/Blue Cave/monastery scenes, separate pirate sprite. A warship is required to enter the monastery. First entry reserves a free envoy if available; pay 5,000 gold or lose that envoy. Without a free envoy, pay or lose one warship. Never substitute a ship for an envoy. Pending hostage state persists; resolved visits are empty. Sanctuary discovery awards one secret once per campaign and records it in the journal. Existing campaign integer/save infrastructure is authoritative.

Verification checkpoint: final private Windows preview compiled successfully and passed 87 focused runtime assertions, including the resume-screen hook, with screenshots at 1280×720 and 1600×900. Landing, pirate, sanctuary and Blue Cave presentation were visually inspected; all five source images were inspected. Hungary/Croatia polygons have been merged with equal-area and valid-geometry checks to remove their internal seam; visual map verification remains pending.

Remaining queue: trace remaining Venetian Adriatic islands and visually verify the merged Hungarian border; reconcile outstanding work against actual source before expanding scope. Public/Desktop43 is unchanged. Asset provenance and historical-fiction boundary are recorded at the end of ARCHIVE.md.


This is ConstantineX — 6 November 2026 — BLUF: cursor proportions corrected and connected to the active point/grip resource. Shorter index finger, existing black leather/aged gold/blue cuff retained; fingertip click pivot realigned. Private Windows build and 12 cursor assertions passed, actual-size/enlarged captures inspected. Source artwork provenance in ARCHIVE.md. Desktop/public43 unchanged. This narrow cursor request does not complete the remaining Adriatic map queue.


This is ConstantineX — 6 November 2026 — BLUF: Version 44 combined Windows candidate built and checked; preparing publication package, not publishing.

Completed 24 traced Venetian island components, preserved discovery island and Ragusan fringe, and visually confirmed removal of the Hungarian internal seam. Live map raycast opens Biševo correctly. Combined player: Biševo/map 91, vicinity/native names 189, journal history 87 checks passed; editor save/technology 57. Initial restricted preference write failed; normal-access rerun passed. Geometry: all polygons valid, unique IDs, eleven economic provinces unchanged. Release notes: Documentation/Version44 release notes.md. Public/Desktop43 unchanged. No further gameplay development is required for this candidate; long-campaign balancing and explicitly deferred content remain outside scope.

Version 44 publication readiness: READY. Candidate source cc36032; 287 player files packaged, ZIP CRC verified, SHA-256 2d63c52159d7e10fb91b9405aa3eb0fc6db3a368ba3219704bbcf71c24c80d3e. Package: Builds/Saving-Constantinople-Version-44-Windows.zip (1,341,405,886 bytes). No saves, captures or DoNotShip folders. Candidate manifest: Documentation/Version44 candidate.json. Next action is Desktop/GitHub publication of this exact candidate; no publication performed in this checkpoint.


This is ConstantineX — 6 November 2026 — BLUF: Version 44 published to GitHub and installed on Desktop. Release: https://github.com/skargan/BesiegedInConstantinopole/releases/tag/v44 . Desktop: C:/Users/Dainius/Desktop/Saving Constantinople - Version 44. Source 96b3ad6; public download returns HTTP 200 and 1,341,405,886 bytes; GitHub digest matches tested ZIP. Desktop launch verified. Publication record: Documentation/Version44 publication.json. Version43 retained.


### ConstantineX — 6 November 2026 — Map and encounter presentation corrections
Completed: map header menu/date and left navigation; Romanian realm names; Kuban bottom desk; Candar/Ionian map returns; independent Venice/Genoa city coordinates; Venetian coastal fill and Po fragment colour; Biševo silhouette refinement. Gameplay rules unchanged. Private Windows preview compiled successfully; BISEVO_CHECKS 97 passed, including navigation and bottom-panel assertions. Runtime captures of the Adriatic map, Kuban panel, landing silhouettes and flowers were visually inspected. Seven tiny disconnected raster coast patches remain outside the mainland union; the marked connected coastal gap is filled. Published Version 44 is unchanged.

Clickable artwork must use the same carefully traced silhouette for hit testing and highlighting, including visible recesses and projections. Check outlines against the source painting and at runtime scale; do not substitute broad ovals or boxes. Coastline polygons follow the painted shore, with shared diplomatic ownership for dependencies.
