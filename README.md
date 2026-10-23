# Besieged in Constantinople

A Unity strategy prototype about preparing and defending Constantinople through events, resource decisions and siege combat.

This fork continues the original project by q-amazed-p. Development is focused first on restoring a reliable playtest of the existing game.

## First playtest on Windows

1. Download this branch using GitHub's **Code > Download ZIP** and extract it. If you already have a working checkout, use the branch there instead of keeping multiple editable copies.
2. Open the extracted folder containing **Assets**, **Packages**, and **ProjectSettings**.
3. Double-click **Launch-Unity.cmd**. It looks for Unity **2022.3.15f1** in the default Unity Hub installation folder. If Unity is installed elsewhere, open Unity Hub, add this project folder and choose that editor version.
4. Wait for Unity to import assets and compile scripts. Then choose **Besieged > Open Prototype Scene** from Unity's top menu.
5. Press **Play**, then select the **Game** tab. Press Play again to stop.

The shortcut opens **Assets/Scenes/Continue Game.unity**, the scene enabled in the inherited build configuration. It does not restore a complete campaign despite its name. You can also open that scene manually in the Project panel.

If the Besieged menu does not appear, check **Window > General > Console** for red compilation errors. Record the first full error and the action that triggered it.

These setup changes have been source-reviewed but have not yet been compiled or playtested in Unity.

## What to test first

- Does the opening event appear, and can you select a choice?
- Can you reach the management screens and advance one turn?
- Are the effects of your choice visible and understandable?
- Record the first error, confusing result or blocked button. Avoid collecting dozens of downstream errors caused by the same initial failure.

The prototype's save implementation stores only the turn number. Do not rely on it to preserve a campaign. If a project-root file named **Save** already exists, back it up and rename it before testing a fresh run; do not delete it. Stop Play mode before doing this.

## Browser preview

There is no verified browser build yet. The plan is to export this same Unity game to WebGL and test it in a browser, including the chat's side browser if compatible.

Browser work still requires checking player compilation, packaged initial data, browser-compatible persistence and the scene flow. Opening C# source files or a mockup in a browser is not a playtest of the game.

## Design direction

- Discover ways to save the city against the odds.
- Tie narrative events to consequential resource decisions.
- Expect most runs to end in defeat, with survival duration and achievements rewarding progress.
- Make excellent decisions improve the odds substantially while retaining some possibility of defeat.
- Keep defeat painful enough to matter but understandable enough to invite another attempt.

The original mechanics document is **Documentation/Core mechanics 1.3.docx**. Use it to fill gaps and flag conflicts with existing code before changing established rules.

## Known next work

- Compile and run the prototype in the pinned editor.
- Repair startup, incomplete saves, event command/variable mismatches and missing event scheduling.
- Verify Ottoman action fallbacks, bombardment, siege equipment and casualty calculations.
- Compare implemented behaviour with the mechanics document.
- Build and playtest a browser export only after the local gameplay loop runs reliably.

See **AGENTS.md** for the development agreement. Architecture and balance changes require a design discussion; initial fixes should retain the existing structure.
