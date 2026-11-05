# Map landmarks — ConstantineX — 5 November 2026

BLUF: City medallions and the Rumeli illustrated location viewer are implemented; combined-build verification is in progress.

Ownership: ConstantineX owns MapLandmarks.cs, LandmarkCapture.cs and UI/Maps/Landmarks. JohanX handed back WarTheatreUI after his toolbar/geometry checks. Changes to his map are limited to the Add hook and removal of the superseded Constantinople dot.

Markers: Constantinople 42 reference pixels; Ragusa/Rome 29; Rumeli32. Gold tower, navy enamel interior, transparent exterior. Hover names/gold outline. Constantinople opens Palace; Ragusa and Rome open existing courts. Fortress sits just north of Constantinople with slight separation for clickability on this schematic map.

Authoritative state: existing campaign calendar plus saved iRumeliLandmark. 1=construction announced; 2=ruined; 3=captured; 4=artillery disabled. Construction announcement sets1; September1452 is the existing completed-fortress preparation gate. story16 successful infiltration/sapping set2; successful artillery sabotage4; assault/capture3. No passage/resource/reward mechanics changed. New campaign clears flag. Existing bStraitsOpen remains authoritative for effective passage and is NOT used to invent destruction. Older saves cannot retrospectively distinguish which successful fortress branch was taken; do not fabricate ruins from an open passage.

Art: generated through OpenAI image generation on5November2026. Reference tower icon supplied by user. No external historical photograph copied. Cinematic fortress illustrations are interpretive, not surveyed architectural reconstructions. The ruined version depicts a fictional campaign outcome. Intact illustration also used for construction/capture with state-specific text; ruined illustration for artillery damage as well as destruction.

Assets:
- city.png source exec-06dfc7df-b993-4e4b-87c6-321e75a49db5.png
- rumeli-intact.png source exec-2a1c87e1-ce11-4217-a6a1-2b7534bb0ff7.png
- rumeli-ruined.png source exec-3d858a17-d77f-4053-a690-ebf70214092e.png
All originals: C:/Users/Dainius/.codex/generated_images/01a0a8e2-b837-7953-bbb3-acf52c000d0c/

Prompt brief: single gold crenellated tower medallion, navy opaque centre, transparent exterior, readable at small scale. Fortress: elevated European-shore view north along the Bosphorus, early bronze bombard overlooking Venetian merchantman, warm limestone/gold light/midnight shadows, no minarets or modern elements. Ruin edit: same composition, broken towers and cannon, removed gunners/standards, merchantman sailing north towards open Black Sea.

Next publication: include generated asset attribution and final test evidence; no standalone public release made by this task.

Verification complete: final Unity private build succeeded; LANDMARK_CHECKS72 passed. Text overflow, artwork loading, visibility, outcome state round-trip, four markers, return-to-map checked at1280x720 and1600x900. Map and ruined-view screenshots visually inspected. No public release. Direct court links reuse existing destination handlers; automated suite validates destination IDs and marker availability, not a full diplomatic conversation.

