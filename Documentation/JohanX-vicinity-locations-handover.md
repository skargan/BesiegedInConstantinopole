# JohanX — Vicinity locations and siege camps

6 November 2026. Source changes and generated assets integrated in the working tree; standalone C# compilation passed. No shared build or publication performed.

## Delivered

- Blachernae marker directly opens the existing Palace screen.
- Chrysopolis, Chalcedon and Prinkipo have illustrated visits with compact right-hand descriptions and Return buttons. Escape is handled through the vicinity map's existing Close path, dismissing the visit first.
- Stoudios Monastery remains a separate site. Studius Fortress has a provisional Marmara-approach anchor (0.34, 0.59); this is not a claim of exact location.
- Prinkipo has an atlas anchor (0.675, 0.955), brought inside the lower map edge for access. The painting is not georeferenced: no kilometre distance from its boundary can be established.
- Six transparent, zoom-scaling camp districts appear only when `VariableSingleton.HasSiegeStarted()` is true: siege works, Sultan's camp, followers, Janissaries, mercenaries and Balkan auxiliaries. Alpha-based hit testing and gold silhouette highlighting. These are visual districts, without new troop pools, resources or event rolls.
- Existing MapDragScrollRect and its cursor support retained.

## Files owned

- Assets/Scripts/ConstantinopleVicinityUI.cs
- Assets/Scripts/VicinityLocations.cs and meta
- Assets/Scripts/VicinityCampArt.cs and meta
- Assets/Resources/UI/Maps/VicinityLocations/ and folder meta
- This handover

## Research and development notes

Primary account: [Kritovoulos, Book I, sections 143–146](https://deremilitari.org/2016/08/the-siege-of-constantinople-in-1453-according-to-kritovoulos/).

Studius Castle is named separately from Therapia. Kritovoulos describes artillery destruction and 36 defenders subsequently executed before the city. This establishes a fortress in the narrative, not its exact archaeological site. Do not conflate it with the well-attested monastery inside Constantinople. Modern accounts associate the fort with the Marmara approaches; precise positioning remains unresolved. The Director permits modest geographic compression for gameplay, but no unsupported distance estimate has been supplied.

Prinkipo is Büyükada, distinct from Kınalıada. Kritovoulos describes Baltaoglu bringing part of the fleet against a fortified refuge with about 30 armed defenders and civilians. Bombardment breached it, but did not immediately secure capture; combustible material, pitch and sulphur were used to set it on fire. The account ends with the guards killed and civilians enslaved. Treat numbers and sequence as this author's account. No automatic fall, casualties or date have been added to simulation.

Future event opportunities, PROPOSED ONLY: warning from island boats; evacuate civilians versus reinforce the fort; divert ships to relieve the island; survivors arriving after its loss. Studius can provide a warning about exposed outworks and the treatment of surrendered garrisons. The Director asked for events later, not now.

Camp placement is a readable reconstruction, not a surveyed encampment plan. Kritovoulos sections 117–120 describe broad command sectors: European forces towards the northern land walls, Asian forces towards the south, Sultan in the central sector. The separately labelled mercenary district is a gameplay grouping, not an asserted historical administrative unit.

## Artwork provenance

Generated using the built-in image generation tool, 6 November 2026. Originals preserved under C:/Users/Dainius/.codex/generated_images/01a15e50-9560-7403-9c1d-159fb220af65/.

| Installed asset | Original |
| --- | --- |
| chalcedon.png | exec-bc896116-af4c-4d43-b117-f672b86ea0ff.png |
| chrysopolis.png | exec-39f648ff-34c3-4af3-88fa-faa218b35b5e.png |
| prinkipo.png | exec-ebb01473-5a87-4123-9745-c263397f47eb.png |
| camps.png | exec-f2b9e04c-1553-47c4-b9a9-62b3e9713da6.png |

Direction: cinematic painterly realism, lapis water and warm stone, late medieval architecture, no modern skyline, left-side subjects and quiet right side for controls. Chalcedon: Greek ruins/statues and agent overlooking the city, nearby Ottoman standards. Chrysopolis: shady ferry landing, supply activity, Constantinople and Galata beyond. Prinkipo: pine-covered island, fishing settlement and modest fortified refuge. Camp atlas: transparent 3-column/2-row arrangement of six isolated miniature districts in the listed order. Camps import readable for alpha hit testing, uncompressed with alpha transparency.

## Validation and limits

- All runtime scripts compiled against the installed Unity 2022.3.15f1 references using its Roslyn compiler: exit 0; three existing unused-field warnings in PlayerLossDisplay/OptionButton.
- git diff --check passed (line-ending warning only in unrelated reconciliation notes).
- Generated paintings and camp atlas visually inspected; atlas alpha confirmed.
- In-game layout, clicking, Escape and zoom still require runtime verification in the next shared build. Artwork is an imaginative reconstruction; the Chalcedon statuary/terrace and Prinkipo fort are not archaeological reconstructions.
- No gameplay rules, save schema, build settings or shared release files changed.

## Integration handover

ConstantineX approved JohanX ownership of the vicinity file and new assets. Next responsible action: lead integrates these working-tree changes into the next build, verifies the three visits plus Blachernae navigation and siege-only camp visibility, and reviews Prinkipo's compressed lower-edge placement. New actions/events remain deferred.
