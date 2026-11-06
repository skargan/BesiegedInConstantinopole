# Development agreement

- Work in skargan/BesiegedInConstantinopole, the designer's fork.
- Preserve the existing Unity, C#, Yarn, scene and prefab structure. Keep changes small and readable for returning human contributors.
- Follow nearby naming and formatting. Do not rename serialized fields or replace systems as incidental cleanup.
- Propose architecture changes, rule changes and broad refactors to the designer before implementing them.
- Use Documentation/Core mechanics 1.3.docx to fill implementation gaps. Flag disagreements between the document and existing behaviour; do not silently choose a new rule.
- The designer is currently collaborating with the AI; do not assume the original programmer is available.
- Priorities: discover ways to save the city against the odds; make events and resource decisions meaningfully interact.
- Most campaigns should end in defeat. Strong play should substantially improve survival while retaining some risk. Defeat should be unpleasant but encourage another attempt.
- Achievements and survival duration are intended sources of progress; exact milestones and rewards remain to be designed.
- Do not change randomness, balance, campaign length or endings without discussing the intended effects.
- Distinguish static checks from successful Unity compilation, playtests and exported-build tests. Never claim the game runs based only on source inspection.
- Keep third-party package code unchanged unless a specific, verified integration problem requires it.

## Object tracing — explicit designer instruction, 6 November 2026

- Trace the visible silhouette closely. For a person or group this includes their clothes, sleeves, arms, hands and objects they hold, rather than a loose outline around bodies or faces.
- Small interruptions are acceptable where required by the artwork, but preserve visible contour changes and avoid cutting across garments, limbs or held objects merely to simplify the shape.
- Use the same traced geometry for the hover outline and hit testing. Do not substitute a broad rectangle, ellipse or coarse enclosing polygon when a silhouette was requested.
- Check the outline against the current artwork at the supported display sizes. Correct routing and approximate placement alone do not establish visual acceptance.
