# Map dragging and twenty campaign review — 6 November 2026

This is ConstantineX. BLUF: the existing fist cursor now accompanies map dragging. Twenty automated scenarios reached campaign endings; no runtime exceptions or UI stalls were recorded. This is exploratory regression evidence, not a representative player win-rate study.

## Delivered

War Beyond the Walls, Vicinity of Constantinople and the retained reference-map viewer share MapDragScrollRect. It reuses the existing grab sprite, preserving ScrollRect movement. Drag completion, disabling the map and loss of focus release cursor ownership. Future draggable maps should use this component. No gameplay balance changed.

Private Windows player compiled; 57 save/technology editor checks passed. MapDragCapture passed 24 runtime assertions across three viewers, including actual content movement and fist UV selection. Regional and vicinity screenshots inspected at 1600x900; cursor shape selection was verified by runtime assertion, not a claim of manual mouse testing.

## Method and limits

Two seeds each for mixed, scarcity, training, naval, espionage, politics, diplomacy, defence, poor-timing and archives. Eight start in preparation; twelve start at the siege with default resources and no preparation rewards. Yarn choices are automated. Combat uses the real assault resolver with prescribed skill inputs. These policies are intentionally narrow: they do not behave like competent human players and do not explore every optional court, mercenary or province interaction. The suite was adapted from the earlier Campaign200Probe, with retired faction action/check removed. Seeded runs are not claimed to be frame-independent deterministic replays.

An initial sweep exposed outdated test assumptions. A single save diagnostic showed only previously absent zero-value fields materialising on load. The final sweep compares semantic state and passed every save/load check, including journal, troop, resource and RNG state. Raw warnings retained in issues.csv concern iWbw37SupplyTick=-1: ProvincialWar deliberately initialises it to CompletedSiegeDays-1 before the first supply tick. Other negative entries printed alongside it are legitimate historical-turn sentinels. No game defect is inferred from these warnings.

## Final sweep

20/20 ended; 968 siege days; 1130 dialogue choices; 100 resolved assaults; 52 save/load checks. Endings: 5 walls lost, 14 public support, 1 emperor died. No victories.

| Run | Approach | Siege days | Ending | Gold left |
|---|---|---:|---|---:|
| 1 | mixed | 63 | Walls lost | 46,294 |
| 2 | mixed | 47 | Walls lost | 91,371 |
| 3 | scarcity | 14 | Public support | 5,400 |
| 4 | scarcity | 16 | Public support | 5,375 |
| 5 | training | 101 | Public support | 742 |
| 6 | training | 83 | Public support | 75 |
| 7 | naval | 47 | Public support | 93,725 |
| 8 | naval | 27 | Walls lost | 94,065 |
| 9 | espionage | 0 | Emperor died | 99,000 |
| 10 | espionage | 40 | Public support | 66,350 |
| 11 | politics | 45 | Public support | 91,150 |
| 12 | politics | 40 | Public support | 91,000 |
| 13 | diplomacy | 84 | Public support | 85,725 |
| 14 | diplomacy | 81 | Walls lost | 30,540 |
| 15 | defence | 36 | Public support | 94,375 |
| 16 | defence | 41 | Public support | 88,503 |
| 17 | poor-timing | 41 | Public support | 98,775 |
| 18 | poor-timing | 40 | Public support | 104,553 |
| 19 | archives | 73 | Walls lost | 66,768 |
| 20 | archives | 49 | Public support | 112,619 |

## What to focus on

1. **Match event prose to consequences.** story23's personally led cavalry charge repeats the successful-return paragraph even in its lethal branch (health minus 1). At 200 cavalry the threshold leaves a 35% fatal branch. The player must not read a triumphant retreat immediately before an unexplained death. Revise outcome text and make the danger legible before commitment; do not silently change its odds.
2. **Make the food strategy understandable early.** Current siege-market cap is 10 provisions/day; roughly 100,000 people need 100/day. Several public-support losses end with substantial cash and no food. A large treasury therefore cannot buy its way out. Review whether existing mission/preparation information adequately conveys this; test a balanced food-aware policy before changing the cap. The defence/scarcity bots attempted oversized purchases which the game correctly refused, making their results particularly unsuitable as balance benchmarks.
3. **Test a genuinely balanced winning route next.** Single-system investment is not a complete siege strategy. Add food-aware, manpower-aware and material-aware choices, retain a reserve for repairs, and exercise deliberate diplomacy. Zero wins here is not a measured human win rate and does not justify weakening the Ottoman AI.
4. **Broaden interaction coverage selectively.** Mercenary incidents, provincial rebellion, court exploration and optional discoveries need targeted walkthroughs; this sweep does not certify them. Prioritise blocked returns, missing consequences and unreadable descriptions over adding more decorative scenes.
5. **Preserve the gains.** Map navigation is now consistent; the campaign loop survived long sequences and repeated reloads without exceptions/stalls. Keep focused regression runs alongside visual review. The current regional/local map frames read coherently; the large empty selection panel on an unselected regional map remains a possible later space-saving improvement.

## Evidence

runs.csv contains each result; trace.csv contains decisions and turn snapshots; issues.csv preserves raw warnings. Executable/captures: Builds/VicinityPresentationPreview. No Desktop/GitHub release was published by this task. JohanX's existing shared changes were left unstaged by this task; compiling the combined player does not certify every one of those changes.
