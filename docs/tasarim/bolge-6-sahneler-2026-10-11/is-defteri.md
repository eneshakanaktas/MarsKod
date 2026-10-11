# SDD ledger — plan: docs/superpowers/plans/2026-10-11-bolge-6-sahneler-3b.md

Spec: docs/superpowers/specs/2026-10-08-bolge-6-design.md §3b. Branch: bolge6-sahneler-3b (from main 12dbb07+bd82528), docs base commit 3b3fd4d.

## Pre-flight scan
| Pair / task | Shared | Finding |
|---|---|---|
| T1↔T2 | Oyun.cs LoadLevel/ResetLevel | T1 adds shot.Release(); T2 replaces with scenes.Restore() (which releases). Consistent. |
| T1↔T2 | CanyonRift.Trail/BendIndex/ViewPoint/ViewTarget, CanyonTraces.Rift | Produced T1, consumed T2 with same names. OK. |
| T1↔T2..T4 | Oyun.SahneDenetimi.cs SceneShots | T1 creates RiftShots; T2/T3/T4 append Descent/Flight/LandingShots. OK. |
| T2↔T3↔T4 | CanyonScenes ctor | Final ctor (robot, shot, hud, cellPos, cols, rows, startYaw) defined in T2. OK. |
| T2↔T3 | Sparrow ResetHome/SetState | T3 changes eyes inside SetState; ResetHome calls SetState. OK. |
| T3↔T4 | Hud (ScreenBlink / SkipCatcher), Sound (Rotor / Rocket) | Additive, no overlap. OK. |
| T2↔T4 | GridPath, Walk, CellsTo | T4 reuses. OK. |
| T1 self | tests vs code | No dotnet tests (visual); verification = shots + Enes gate. OK. |
| T2 self | GridPathTests uses `Bolumler.Yukle` placeholder name | Implementer must use existing BolumTests level-loading helper. |
| T3 self | — | OK. |
| T4 self | Step 1 asks Enes mid-task | See ruling 3. |
| T5 self | — | OK. |
| All | Global "no commit per task" vs SDD per-task diffs | See ruling 2. |
| All | Plan/user gate (Enes approval after T1-T4) vs SDD continuous execution | See ruling 1. |

## Rulings
- Ruling 1: Stop after each of T1-T4 for Enes's visual approval (user CLAUDE.md + spec 3b.5 outrank skill's continuous-execution) — costs wall-clock only.
- Ruling 2: Implementers commit per task on branch bolge6-sahneler-3b; at session closing the branch is squash-merged into main as ONE commit with ILERLEME.md (team rule) — if wrong, extra commits would appear on main; squash prevents that.
- Ruling 3: T4 Step 1 (rocket girth) — implementer measures, applies girth if Kıvılcım doesn't fit, continues; Enes judges girth together with s51-* at T4 gate (subagent cannot ask Enes) — costs a fix round if Enes rejects.
- Ruling 4: Implementers/reviewers on Sonnet (project rule: code-writing helpers = Sonnet); final whole-branch review on Opus.

## Progress
Task 1: dispatched (base 3b3fd4d, implementer sonnet)
Task 1: implemented f6de833 (DONE_WITH_CONCERNS); review: Needs fixes (visual 3b.2 gaps; CameraShot keyboard-scale jump; Release not smooth). Enes rejected look-down ("çok mantıksız") and chose new framing: along the rift toward the canyon mouth.
Ruling 5: Fix round 1 goes to a FRESH implementer on Opus instead of resuming the Sonnet one — the fix is a design-level visual rework under Enes's new direction (project rule: hard visual problems → Opus) — costs more tokens; Sonnet keeps later mechanical tasks.
Task 1: minor (deferred→folded into fix1): riftClip constants duplicated w/o cross-reference; LookRotation zero guard; SahneDenetimi logs TAMAM without visual checks (accepted: screenshot harness).
