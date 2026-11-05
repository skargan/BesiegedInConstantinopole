# Military order presentation — 5 November2026

Bug: MilitarySelectionDesk invoked the scene's serialized recruitment popup while its new right-hand desk remained in front. The popup appeared obscured behind the desk for allunit types. Corrected by routing newdesk buttons into a dedicated CouncilUI order screen; scene's legacy callbacks remain intact for reference, but are no longer invoked by the newdesk.

MilitarySelectionDesk swaps scene art for each of infantry,archers,cavalry,guards,cannons. Warships selection opens Shipyard directly. Shipyard contains construction above current orders (firstfour visible; complete queue accessible through recruitmentlogistics), matching shipyard background and clear future sprite area. Fleet build/order links route to same screen.

RecruitmentOrder reuses TrainingPool.UnitPrice/MaximumOrder/CanPlace/Place, dates, manpower/material/crew reservations and save checkpoint. Quantities move in1/10/100 steps; visible total and requirements; explicitconfirmation, no payment on preview/cancel. No new recruitment rules, fees, time consumption or military category. Original military selection restored oncancel.

Generated art is historical fiction with plausible fifteenth-century equipment, not a certified reconstruction. Archery corrected toone firingline/separateparallel lanes. No minarets/modern equipment. Asset provenance: military-background-art.json and ASSET SOURCES.md. Existing publicVersion41 unchanged until nextpublication.

Verification: isolated Windows build succeeded;266 militaryorder checks passed at1280x720/1600x900. Allsix selection/order routes, unobscured dialogs, no-spend preview/cancel, affordability, exactoneunit queue/payment, shipyardnavigation and textfit. Archeryorder andshipyard screenshots visuallyreviewed. Existing fullprices/times/resource reservations reused.
