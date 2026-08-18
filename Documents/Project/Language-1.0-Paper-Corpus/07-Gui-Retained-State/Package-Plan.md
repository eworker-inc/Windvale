# Workload 7 package and resource plan

## Package identity

Canonical paper package identity:
`windvale.paper.language1.retained_gui` version 1.

## Module mapping

| Canonical module | Source | Profile | Authority |
| --- | --- | --- | --- |
| `Retainedˉguiˉpackage` | [`Source/Retained-Gui-Package.wv`](Source/Retained-Gui-Package.wv) | Core | library |
| `Retainedˉguiˉtypes` | [`Source/Retained-Gui-Types.wv`](Source/Retained-Gui-Types.wv) | Core | library |
| `Retainedˉguiˉtheme` | [`Source/Retained-Gui-Theme.wv`](Source/Retained-Gui-Theme.wv) | Core | library |
| `Retainedˉguiˉstate` | [`Source/Retained-Gui-State.wv`](Source/Retained-Gui-State.wv) | Core | library |
| `Retainedˉguiˉlayout` | [`Source/Retained-Gui-Layout.wv`](Source/Retained-Gui-Layout.wv) | Core | library |
| `Retainedˉguiˉrender` | [`Source/Retained-Gui-Render.wv`](Source/Retained-Gui-Render.wv) | Core | library |
| `Retainedˉguiˉhostˉtypes` | [`Source/Retained-Gui-Host-Types.wv`](Source/Retained-Gui-Host-Types.wv) | Hosted | library |
| `Retainedˉguiˉapplication` | [`Source/Retained-Gui-Application.wv`](Source/Retained-Gui-Application.wv) | Hosted | application |

All modules target Windows, Linux, and Windvale. Only the application declares
the three GUI capabilities. The build supplies the candidate Foundation and
paper `Platformˉgui` signatures; those dependencies are not searched source.

## Package-data binding

| Declaration | Resource identity | Type | Declared maximum | Exact length | SHA-256 |
| --- | --- | --- | ---: | ---: | --- |
| `Retainedˉguiˉpackage.Theme` | `windvale.paper.gui.theme.v1` | `bytes` | 64 | 36 | `6b8e997deebb5da7d4afd6654e1c65d29871f9df04bb3a7150fa3a785b9ede40` |

The exact payload is [`Package-Data/Theme.wvtheme`](Package-Data/Theme.wvtheme):

```text
WVTHEME1\n
101820ff\n
f2f4f8ff\n
00a7b5ff\n
```

It contains one eight-byte ASCII magic/version and three lowercase RGBA8 values.
The final LF is part of the 36-byte payload. Length, digest, type, maximum, magic,
separators, and every hex digit validate before state allocation or provider
work. The resource is charged once; parsing produces three Copy colors and does
not retain a second payload.

## Reference application limits

| Limit | Exact reference value | Hard paper ceiling |
| --- | ---: | ---: |
| surface width / height | 64 / 48 | 256 / 256 |
| live arena widgets | 4 initially, 3 finally | 64 |
| stable identity tombstones | 4 | 64 |
| events per batch / batches | 5 / 1 | 64 / 16 |
| frame bytes / render work | 16,384 / 4,096 | 262,144 / 65,536 |
| state arena budget | 16,384 | 65,536 |
| identity-map budget | 8,192 | 32,768 |
| each of two frame budgets | 16,384 | 262,144 |
| task-scope budget | 16,384 | 65,536 |
| task children / runnable / completed | 1 / 1 / 1 | 8 / 8 / 8 |
| task retained bytes / work units | 4,096 / 64 | 16,384 / implementation-admitted |
| task call depth / timers / diagnostics | 16 / 1 / 16 | Foundation-admitted |

The launcher transfers one 73,728-byte root memory budget. Before publication,
the application splits exact 16,384-byte state, 8,192-byte map, two 16,384-byte
frame, and 16,384-byte task children. Combined child authority equals the root;
no event, provider, or host-thread count multiplies it. A failed split publishes
no child and cannot consume a later split.

The provider separately admits one immutable event batch of at most five events,
one retained timer completion, two local frame publications, and their bounded
diagnostics/teardown. Those are provider-domain charges, not hidden use of the
application memory budget.

## Retained identity policy

The arena owns live widgets. The ordered map owns four stable logical identities
to generation-checked handles. Removing status identity 4 vacates its arena slot
and advances the slot generation but deliberately retains the map entry as a
tombstone. Therefore:

- identity 4 cannot be silently reused during the state lifetime;
- lookup through it reports the exact stale generation;
- the live widget count becomes three while the identity count remains four;
- no map-removal operation is required merely to hide stale evidence; and
- a future product that permits identity reuse must choose a separate explicit
  policy and update both structures atomically.

## Construction and publication order

1. Admit and validate the package graph and exact theme object.
2. Validate all application/task limits before memory splitting.
3. Split all five bounded child budgets.
4. Construct the arena with background first, then counter/action/status; build
   the ordered identity map with the same four stable identities.
5. Compute and atomically apply generation-1 layout under one exclusive state
   borrow.
6. Build and publish the initial immutable frame.
7. Construct one task scope from the launcher context and spawn one copied
   layout snapshot.
8. Await one timer tick and one five-event input batch through the scope-derived
   context; apply events only on the owning path.
9. Await the child, reject its generation-1 layout as stale against state
   generation 3, compute/apply one fresh layout, and publish the final frame.
10. Release task, frame, map, arena, and remaining root accounting through
    lexical cleanup.

Any early `try` propagation applies the enclosing task policy and local releases
before returning. No partial frame becomes source-visible; no provider failure
causes automatic retry or endpoint refresh.

## Build and artifact plan

The Core path lowers through ordinary variants, records, checked numerics,
ordered-map lookup, arena handles/mutation, loops, and immutable byte
construction. The Hosted path adds module-bound capability calls, operation
contexts, one explicit closure, one task, and async continuations. No class,
object header, tracing-GC root, GUI opcode, host widget, event-loop global, or
display-specific WIR is required.

Current Seed cannot parse this source. WIR/WVB/native sizes, compile/runtime
time, peak memory, frame-build work, provider queue maxima, cancellation latency,
and teardown bounds remain implementation measurements rather than paper claims.

The reviewed bundle contains 8 source files, 2,004 LF-terminated source lines,
74 top-level declarations (38 functions, 14 records, 7 variants, 4 enums, 10
constants, and 1 package-data declaration), and 65,944 UTF-8 bytes. The largest
module is `Retained-Gui-Application.wv` at 729 lines / 25,524 bytes. These are
reproducible source facts, not compiler or artifact measurements.
