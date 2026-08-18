# Language 1.0 paper workload 7: retained GUI state

## Status

Draft reviewed after the project owner accepted all seven findings on
2026-08-17 under
[Decision 0761](../../../Decisions/0761-Resolve-Language-1.0-Retained-Gui-Findings.md).
This is paper Language 1.0 source. Current Seed tools do not accept it, and it
does not implement a GUI, freeze edition 1, or claim a native display provider.

## Result

Eight modules express one bounded retained application that:

1. validates one exact 36-byte package theme;
2. constructs four widgets in a typed arena and four stable identities in an
   ordered map;
3. computes/applies generation-1 layout and publishes an immutable 7,680-byte
   RGBA8 frame;
4. copies one layout snapshot into a lexical child task;
5. awaits one timer tick and one bounded five-event input batch through the
   scope-derived operation context;
6. applies all retained-state mutation only on the owning parent path;
7. removes status with generation-safe tombstone evidence, increments a counter,
   resizes, requests layout, and closes;
8. rejects the child result as stale, computes/applies a fresh generation-3
   layout, and publishes an immutable 12,288-byte final frame; and
9. reports exact surface/input/timer generations, sequences, state, and layout
   outcomes.

The initial/final frame SHA-256 identities are respectively
`5e73732a7143581d92b50a21f3c1efcf3c64cfe146f1ca5bb6b9a79e6a793aa7`
and
`cca3674648e1995cb196d126b355bf38a9e4c8b4aa7cb5ebe8dba1630551cfdb`.

## Source modules

| Module | Responsibility |
| --- | --- |
| `Retainedˉguiˉpackage` | Exact package-data declaration. |
| `Retainedˉguiˉtypes` | Core theme/state/event/layout/frame/limit values. |
| `Retainedˉguiˉtheme` | Exact allocation-free theme decoder. |
| `Retainedˉguiˉstate` | Arena/map construction, validated reads, atomic layout application, and event mutation. |
| `Retainedˉguiˉlayout` | Pure deterministic widget geometry from a Copy snapshot. |
| `Retainedˉguiˉrender` | Bounded deterministic RGBA8 frame construction/freeze. |
| `Retainedˉguiˉhostˉtypes` | Hosted provider/task failures and application report. |
| `Retainedˉguiˉapplication` | Budgets, endpoints, task/context, event loop, stale-result handling, and publication. |

The first six modules are Core. Only the final two are Hosted; only the
application has capability requirements.

## Evidence index

- [GUI capability contract](Gui-Contract.md)
- [package and resource plan](Package-Plan.md)
- [semantic review](Semantic-Review.md)
- [rejected and boundary cases](Rejected-Cases.md)
- [expected outcomes](Expected-Outcomes.md)
- [implementation responsibilities](Implementation-Responsibilities.md)
- [review findings](Review-Findings.md)

## Acceptance answer

Language 1.0 can express this retained-state workload without classes,
inheritance, tracing-GC cycles, observable properties, implicit UI-thread state,
ambient event loops, locks, exceptions, detached tasks, source-level threads,
reflection serialization, or GUI-specific compiler syntax.

The accepted general completion is two typed-arena mutations:
`Arenaˉreplace` and `Arenaˉremove`, with exact ownership and generation behavior.
Closed event variants, one owned state aggregate, copied background snapshots,
parent-only result application, immutable frame bytes, and three narrow
capabilities cover the remaining pressure through existing language rules.

## Nonclaims

This is not a widget toolkit, window manager, compositor, terminal, GPU renderer,
font/text stack, accessibility tree, animation system, clipboard, IME, desktop
session, or cross-platform native API abstraction. It proves the language and
capability boundaries those later libraries can build on.
