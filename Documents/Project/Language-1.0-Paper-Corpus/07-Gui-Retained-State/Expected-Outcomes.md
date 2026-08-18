# Workload 7 expected semantic outcomes

## Reference bindings

The launcher supplies:

- surface endpoint generation 7;
- input endpoint generation 11;
- timer endpoint generation 13;
- one valid parent operation context;
- the exact limits and 73,728-byte root budget in
  [the package plan](Package-Plan.md); and
- the exact accepted theme resource.

The surface provider begins at frame sequence 1. The input provider begins at
batch sequence 1. The timer provider begins at tick sequence 1.

## Initial state and frame

State starts at 48 by 40, counter 0, event sequence 0, layout generation 1, and
four live widgets. The initial layout is:

| Identity | Kind | Rectangle `(x,y,width,height)` | Color in frame |
| ---: | --- | --- | --- |
| 1 | background | `(0,0,48,40)` | theme background where no later widget covers |
| 2 | counter | `(4,4,40,8)` | foreground because counter is even |
| 3 | action | `(14,24,20,12)` | accent |
| 4 | status | `(4,14,40,8)` | foreground |

The immutable RGBA8 frame is 48 by 40, stride 192, and 7,680 bytes. It contains
1,040 background pixels, 640 foreground pixels, and 240 accent pixels. Its exact
SHA-256 is
`5e73732a7143581d92b50a21f3c1efcf3c64cfe146f1ca5bb6b9a79e6a793aa7`.

The first publication receipt is surface generation 7, frame sequence 1, and
7,680 accepted bytes.

## Background work and input transcript

The parent copies one generation-1 layout snapshot into the child before reading
events. Scheduler timing does not change that value.

Timer generation 13 returns tick sequence 1. Input generation 11 returns batch
sequence 1 with exactly these events in order:

1. `Pointerˉpressed(X: 16, Y: 26)` — inside the generation-1 action rectangle;
   counter becomes 1 and event sequence becomes 1.
2. `Removeˉstatus` — arena identity 4 is removed, its old handle becomes stale,
   layout generation becomes 2, and event sequence becomes 2.
3. `Resize(Width: 64, Height: 48)` — layout generation becomes 3 and event
   sequence becomes 3.
4. `Requestˉlayout` — requests layout without changing its generation; event
   sequence becomes 4.
5. `Close` — marks state closed and event sequence becomes 5.

No event mutates state before its own validation succeeds. The retained identity
map still contains identity 4 as a tombstone; arena validation reports its exact
stale generation rather than aliasing another widget.

## Stale result and final frame

Await returns the child's valid generation-1 layout. Applying it to state
generation 3 produces
`Staleˉsnapshot(Expected: 1, Observed: 3)` and changes no widget.

The owner then computes from a fresh generation-3 snapshot and applies
`Applied(Newˉgeneration: 3)`. Status remains absent. The final layout is:

| Identity | Kind | Rectangle `(x,y,width,height)` | Color in frame |
| ---: | --- | --- | --- |
| 1 | background | `(0,0,64,48)` | theme background where no later widget covers |
| 2 | counter | `(4,4,56,8)` | accent because counter is odd |
| 3 | action | `(18,32,28,12)` | accent |

The final immutable RGBA8 frame is 64 by 48, stride 256, and 12,288 bytes. It
contains 2,288 background pixels and 784 accent pixels. Its exact SHA-256 is
`cca3674648e1995cb196d126b355bf38a9e4c8b4aa7cb5ebe8dba1630551cfdb`.

The second receipt is surface generation 7, frame sequence 2, and 12,288
accepted bytes. The final application report contains:

```text
Processedˉevents = 5
Counter = 1
Backgroundˉlayout = Staleˉsnapshot(Expected: 1, Observed: 3)
Finalˉlayout = Applied(Newˉgeneration: 3)
Closed = true
Inputˉgeneration = 11
Timerˉgeneration = 13
Timerˉsequence = 1
```

## Scheduler equivalence

Run the child before the timer call, between timer and input, during input
provider suspension, or after all events. Every schedule produces the same two
frame hashes and report because the child owns a copied snapshot and only the
parent applies results/mutates retained state.

## Failure transcripts

### Stale widget handle

After `Removeˉstatus`, `Readˉidentity(Identity: 4)` finds the retained map
tombstone, then arena validation returns
`Staleˉgeneration(Expected: old, Observed: successor)`. It returns no widget and
cannot alias a newly inserted node.

### Oversized event batch

Six events against the reference maximum five return
`Eventˉbatchˉlimit(Observed: 6, Maximum: 5)` before event index zero. No state,
counter, event sequence, or layout generation changes.

### Event after close

If a sixth event follows `Close` in the same admitted batch, the first five
remain applied and the application returns `Eventˉafterˉclose(Index: 5)`. It
does not apply the sixth event or publish the final frame.

### Cancelled background task

`Taskˉoutcome.Cancelled` becomes `Backgroundˉcancelled`. The scope still joins
the child; the application does not compute/publish the final frame on that
path.

### Provider restart

An input, timer, or surface restart reports exact expected/observed generations.
Old endpoints do not rebind. A rejected surface publication returns its frame to
the adapter, which releases it while returning a small normalized failure. An
indeterminate publication is never replayed.

### Publication validation failure

Wrong stride, byte length, frame sequence, state sequence, layout generation, or
surface generation rejects before publication and returns the immutable frame.
No partial frame receipt is produced.
