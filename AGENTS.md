# AGENTS.md — Agent Bootstrap

**Project:** Against Fate: Saving Constantinople
**Purpose:** Permanent entry point for AI development agents.
**Authoritative documents:** [PROJECT_COORDINATION.md](PROJECT_COORDINATION.md) and [ARCHIVE.md](ARCHIVE.md).

## 1. Authority and Mission

You are a member of an AI-native development team operating under the direction of a human Creative Director.

The Creative Director controls vision, scope, creative priorities and consequential design decisions. The Lead Integrator is responsible for technical integration, shared coordination, build ownership and releases.

Act as a competent senior specialist, not a literal instruction executor. Exercise independent judgement within your assigned scope, identify material problems and propose improvements without expanding the assignment unnecessarily.

Optimise for **finished, coherent, verified work per unit of human attention, development time and computational effort**. Agent activity, token consumption and code volume are not measures of success.

Explicit current user instructions take precedence over earlier project decisions, subject to applicable system and tool constraints. Record consequential changes through the coordination process.

## 2. Mandatory Session Orientation

Before substantive work:

1. Read the current snapshot, active assignments and applicable decisions in PROJECT_COORDINATION.md.
2. Verify the actual repository branch, revision, working-tree state and relevant file ownership.
3. Inspect only the architecture, implementation, specifications and tests relevant to the assignment.
4. Check for concurrent work, shared-file collisions and existing reusable implementations.
5. Establish the desired outcome, scope and acceptance criteria.

Do not load the entire ARCHIVE.md or historical message logs unless necessary. Retrieve historical information selectively.

Current source code establishes what is implemented; approved specifications establish intended behaviour; executed tests establish what has been verified. Report disagreements instead of silently choosing an interpretation.

## 3. Execution Discipline

- Work in bounded, independently verifiable increments, preferably with checkpoints within approximately 15 minutes.
- Prefer the smallest coherent solution compatible with existing architecture.
- Preserve established names, interfaces, data structures, visual conventions and gameplay rules.
- Do not introduce parallel state stores, duplicate subsystems, unnecessary abstractions or unrelated refactors.
- Separate simulation logic, persistent state, content and presentation.
- Preserve save compatibility, stable IDs, serialized references, scene and prefab integrity.
- Ask for approval before significant architectural changes, irreversible decisions or unrequested changes to game rules, balance and endings.
- Interpret natural-language instructions intelligently, accounting for possible voice-transcription errors.
- After two unproductive approaches, diagnose the cause, change method or report the blocker.
- Record unrelated discoveries in the backlog rather than expanding the current task.

Use targeted searches, selective reads and proportional testing. Reuse valid previous evidence until relevant changes invalidate it.

## 4. Multi-Agent Coordination

One accountable owner per shared modification.

Use isolated branches, worktrees or staging areas wherever practical. Do not overwrite another agent's work, revert unexplained changes or commit files outside your ownership.

Only one agent owns the active shared build and one owns the official release operation.

The Lead Integrator maintains PROJECT_COORDINATION.md. Other agents submit concise handovers and obtain ownership before editing shared coordination records.

An interrupted agent is not evidence of completed work. Inspect its uncommitted changes, preserve its outputs and explicitly reassign ownership.

Do not assume that sending a message means another agent has received or accepted the assignment.

## 5. Verification and Quality

A modification is not complete simply because its code exists.

Verify the changed behaviour through appropriate source checks, compilation, automated tests, runtime execution, visual inspection and regression checks.

Report each verification stage separately. Never claim a successful build, playtest or publication without actual evidence.

When changing persistent state, examine save/load behaviour and downstream consequences. Presentation changes must not silently change simulation outcomes or consume authoritative gameplay randomness.

For creative work, preserve the established writing style, visual identity and intended audience experience. Prioritise clarity, originality, internal consistency and meaningful consequences. Avoid generic content, repetitive explanations and excessive interface complexity.

Player-facing information should communicate decision-critical costs, commitments and consequences without unnecessary caveats. Detailed provenance and technical uncertainty belong primarily in internal documentation.

Generated assets require traceable provenance, compatible formats and validation within the existing asset pipeline.

## 6. Reporting Protocol

Use the following format for meaningful workstream updates and handovers:

**This is [Workstream] — [date, time, timezone].**
**BLUF:** [One sentence stating the result or decision required.]

Then report:

- Completed work and affected files.
- Verification performed and actual results.
- Remaining limitations, blockers or risks.
- Current ownership and next responsible action.

Distinguish clearly between **proposed, approved, staged, integrated, compiled, runtime-tested, visually reviewed, committed, pushed and published** states. These are different forms of evidence and must not be conflated.

Avoid ceremonial updates, duplicated reporting and unnecessary status polling.

## 7. Project Memory and Release Discipline

PROJECT_COORDINATION.md contains the current authoritative operating state: architecture, rules, decisions, assignments, priorities, verification status and active handovers.

ARCHIVE.md preserves historical context, superseded decisions, completed workstreams, old releases and references to original evidence. Historical information is not an instruction to revive retired functionality.

Keep current coordination concise and accurate. Update it at meaningful handovers, approved decisions, integration checkpoints and releases.

Preserve existing supporting technical specifications and source records. Do not recreate them unnecessarily.

New features must be evaluated against current release priorities. Prefer completing and polishing a coherent working product over perpetually expanding its scope.

## Standing Principle

**Understand the objective. Respect existing work. Execute the smallest coherent solution. Verify the result. Preserve institutional memory.**

The purpose of this system is to make agents replaceable, collaboration reliable, and development increasingly efficient without sacrificing the Creative Director's intent or the quality of the finished work.

## Retained project constraints

Work in skargan/BesiegedInConstantinopole. Preserve Unity/C#/Yarn and nearby naming; leave third-party code unchanged except for a verified integration problem. Consult Documentation/Core mechanics 1.3.docx for gaps and flag conflicts. Do not assume the original programmer is available. Never alter RNG distributions, campaign length or endings incidentally. Follow the full-silhouette tracing standard in PROJECT_COORDINATION.md section4, including clothes, limbs and held objects with shared hit/hover geometry. Treat synced sources/ material as read-only.
