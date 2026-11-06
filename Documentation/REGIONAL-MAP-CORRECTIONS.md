# Regional map reconciliation — 6 November 2026

Owner: ConstantineX. User clarified that this request concerns War Beyond the Walls, not the Constantinople city map.

## Recovered references

The original user attachment `codex-clipboard-a569f691-4c29-40a8-908e-5c1c2794d30a.png` was recovered from this chat's original image content after its temporary path disappeared. It marks the Adriatic hinterland, western Greece, Chalkidiki, western Anatolian promontories and the Antalya/Karaman coastal boundary in red. The written requests also cover Constantinople's position, Morea's coast, Hungary/Wallachia/Moldavia, splitting Anatolikon and Armeniakon, merging Lydia/Thrakesion, and grouped maritime dependencies.

The screenshot is a review reference, not an authoritative historical survey. Its recovery does not establish that there were no additional maps in other chats. The user has been asked whether later hand-drawn references are also intended; that reconciliation remains pending their answer.

## Current authoritative integration

Checked `Assets/Resources/Content/WarTheatre.json`, loaded by `WarTheatre.Data`, against the recovered reference and inspected the running full map at 1280×720 and 1600×900.

| Requested correction | Current integration |
| --- | --- |
| Adriatic gap | Bosnia, Croatia, Ragusa and Venetian Dalmatian/coastal possessions are present. |
| Western Greece | The western mainland is included in the existing Athens/Macedonian overlays; Ionian islands remain distinct. |
| Chalkidiki | Macedonia's coastal outline includes the peninsulas; bays remain water. No Mount Athos label. |
| Western Anatolia | Opsikion/Thrakesion coastal promontories and associated island outlines are present. |
| Antalya/Karaman | Karaman meets the Cibyrrhaeot district along the coast and interpreted Taurus frontier. |
| Morea/Sicily | Detailed coast-following polygons are loaded, rather than the original coarse outlines. |
| Anatolian districts | Separate Anatolikon and Armeniakon; Lydia/Saruhan is aliased into Thrakesion. Eleven simulated province identities remain. |
| Maritime dependencies | Venice/Crete and Genoa/Corsica use shared hover identities. Later approved Gazaria/Caffa diplomacy remains separate. |
| Eastern power | Existing Aq Qoyunlu court remains linked to the schematic Turkmen Confederations overlay. |
| Candar | Later user instruction supersedes the earlier wholly Ottoman Paphlagonia abstraction: separate Sinop coastal realm is retained. |

These revisions were already in the authoritative map data. No old staging JSON was copied over newer Candar, Rhodes, Italian, Crimean or Lithuanian additions. No territory resources, province IDs, diplomatic identities or ownership rules were changed in this pass. Existing historical grounding and previous geometry work are recorded in `Cartography revision 36.5.txt`, `Cartography revision 36.9.txt` and `Historical map and diplomacy sources 36.13.txt`.

The map remains an interpreted gameplay map. Do not present this audit as proof of exact historical boundaries or a newly completed comprehensive cartographic redraw. If later annotated maps are supplied or located, apply their remaining differences to the current catalogue, preserving subsequent approved additions.
