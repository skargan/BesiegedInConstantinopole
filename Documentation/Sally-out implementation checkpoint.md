# Sally-out checkpoint — 6 November 2026
Director authorised implementation and publication. Publication remains pending.

Implemented in working tree: four story23 charge branches, gun-cart rewards (3 cannons, 60 ammunition, 30 materials, 2 engineers), removal of unrelated Orban death marker; military selection number/lethality columns; SallyOut.cs and CouncilSallyUI.cs provisional six-mission resolver and contingent menu; shared food includes 3,000 new light cavalry.

Remaining before release: interception must have its own event presentation; refine combat model so damage and withdrawal consistently distinguish missions; ensure Mehmed is protected by exactly 15,000 actual troops and success/death reflect post-interception state; add light cavalry to enemy inspection and wall-assault integration at 0.15 lethality; initialise/persist/reset through campaign lifecycle; troop/cannon ownership and captured crew consistency; test duplication, day advance, food conservation, boundary probabilities, victory and personal death; visual UI review; combine JohanX vicinity handover; build and publish with version/changelog.

Cut-off convention: <=50 none; otherwise floor(sent/50) percent, capped 100. 300 =>6%. Interceptors 100–500 existing Ottoman light cavalry, capped by available; outside lethality1, wall lethality0.15. One provision per surviving cavalryman, capped by camp stock. All sallies finish day. Horse archers deferred.

Unity export running via BuildVicinityPresentationPreview.Build; Builds/sally-build.log. First launch without elevated licence access failed; second launch succeeded in compilation and entered export. Do not claim runtime validation or publication from source changes.
