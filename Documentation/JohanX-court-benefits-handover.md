# JohanX — court service benefits — 6 November 2026

## Integrated source; publication remains with ConstantineX

- Kjeld Haraldsson: 50% interception of existing assassin combat, poison and surrender-assassination outcomes, only while serving and not commanding. No plot subsystem.
- Varangians: 5 per calendar month in preparation; 5 per consecutive seven siege-day window. Commander Kjeld: 10, training 15 days instead of 30. Commander changes retain already-used quota; existing orders retain their completion dates.
- Gennadios: daily +1 support, or +3 commanding. Random faction except one at least ten points below every other faction takes precedence. Exile disables sermons. Daily resolution includes intervening preparation days; repeated dates do not award again.
- Commander selection and roster descriptions include both characters.
- Recruitment order panel uses themed quantity slider and plus/minus, preserving price, manpower, completion and confirmation information.
- Public-support icon tinted muted blue-purple.
- Gennadios dialogue portrait installed at Assets/Resources/UI/Identity/Characters/203.png and displayed beside biography.

## Evidence

All non-Editor C# compiled successfully against installed Unity 2022.3.15f1 managed assemblies. Log: ../kjold-next/compile/compile.log.
17 isolated behavioural checks passed against actual CourtServiceBenefits.cs with dependency stubs: base/doubled quota, commander switching, month/week boundaries, training duration, rescue threshold and eligibility, sermon priority, repeat-date guard, commander amount, exile. These are not Unity runtime or save/load tests.
`git diff --check` passed. Yarn changes require Unity import/compilation confirmation. Final visual inspection, campaign save/load, and combined runtime regression remain pending; no publication claimed.

## Art provenance

Generated through imagegen using existing Gennadios full sprite as visual reference. Original generated image: C:/Users/Dainius/.codex/generated_images/01a15e50-9560-7403-9c1d-159fb220af65/exec-1dbfdfbd-4e8f-4be6-9a49-5f12ed4b2d7e.png. Installed copy retains generated square portrait; resource metadata follows existing portrait import settings with unique GUID.

## Next owner

ConstantineX owns shared build/publication. Review recruitment panel and Gennadios dialogue, confirm Yarn import, exercise rescue outcomes and save/load before release. Existing legacy RecruitationSelection path retains its old layout but uses the same authoritative TrainingPool limits. New council recruitment panel is redesigned.
