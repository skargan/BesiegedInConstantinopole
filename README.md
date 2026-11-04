# Besieged in Constantinople

A strategy game about preparing Constantinople and defending it through diplomacy, resource decisions, intrigue and siege combat. This fork continues the original project by q-amazed-p.

## Play on Windows

1. Open [Version 39 on GitHub Releases](https://github.com/skargan/BesiegedInConstantinopole/releases/tag/v39).
2. Download **Besieged-Version-39-Windows-x64.zip** and extract the whole folder to a writable location.
3. Run **Besieged.exe**. No Unity installation is needed.

The **Code > Download ZIP** button downloads source, not the playable game. This is a development playtest. [Release notes and verification scope](Documentation/Release39/RELEASE-NOTES.md) describe what is included and what remains unfinished. Campaign saves live in the local Saves folder; no personal saves are bundled. Fresh campaigns are recommended for this release.

## Develop

Use Unity **2022.3.15f1** and open this project root. The enabled player scene is configured in ProjectSettings/EditorBuildSettings.asset. BuildRelease39.Build exports Windows 64-bit to Builds/Version-39 and runs the assault regression suite first. Tools/verify_release39.py runs focused checks in that exported player using isolated test saves.

Read **AGENT COORDINATION.docx** or its searchable text mirror before editing. Editable document sources and original coordination messages are in Documentation. The four current sections cover instructions, architecture, decisions and ownership; release history and the recovered message log follow them.

**main** is the authoritative integration branch. Historical branch tips are retained as archive tags. [FutureDevelopment](FutureDevelopment/README.md) indexes shelved code, original art and DLC material, with provenance and checksums. Do not copy old whole-screen or state files over current systems to revive a prototype.

## Feedback

Include the version, reproduction steps, screen/event and expected result. Attach a preceding save when possible. Focused tests do not establish complete campaign balance or compatibility on every PC.
