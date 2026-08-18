# Workload 7 GUI capability contract

## Scope

This is the smallest paper-only capability surface required by the retained-GUI
workload. It separates display publication, input delivery, and timer progress;
none implies the other two. The launcher approves and supplies one exact
generation-bound endpoint for each interface.

The application declares only:

```text
requires capability display.surface version 1;
requires capability input.event_batch version 1;
requires capability timer.tick version 1;
```

It receives no ambient desktop, window enumeration, clipboard, filesystem,
network, process, GPU, raw device, wall-clock, font-discovery, accessibility,
global event-loop, or host-thread authority.

## Paper type surface

The imported `Platformˉgui` module supplies types equivalent to:

```text
export opaque Surfaceˉendpoint Copy;
export opaque Inputˉendpoint Copy;
export opaque Timerˉendpoint Copy;

export enum Failureˉreason: u8 {
    Rejected = 1u8;
    Cancelled = 2u8;
    Deadlineˉreached = 3u8;
    Revoked = 4u8;
    Lost = 5u8;
    Restarted = 6u8;
    Invalidˉresponse = 7u8;
}

export record Failure {
    Reason: Failureˉreason;
    Expectedˉgeneration: u64;
    Observedˉgeneration: u64;
}

export record Eventˉbatch {
    Events: Sequence<Retainedˉguiˉtypes.Event>;
    Sequence: u64;
    Providerˉgeneration: u64;
}

export record Tick {
    Sequence: u64;
    Providerˉgeneration: u64;
}

export record Publishˉreceipt {
    Surfaceˉgeneration: u64;
    Frameˉsequence: u64;
    Acceptedˉbytes: u64;
}

export variant Publishˉfailure {
    Rejected(Error: Failure, Frame: Retainedˉguiˉtypes.Pixelˉframe);
    Indeterminate(Error: Failure);
}
```

Each endpoint binds one approved target identity, exact rights/limits, provider
identity, and nonzero generation. Copying an endpoint duplicates neither grant
nor accounting and cannot discover or select another target.

## Operation signatures

```text
async display.surface.Publish(
    Endpoint: Surfaceˉendpoint,
    Context: borrow Operationˉcontext,
    Frame: Retainedˉguiˉtypes.Pixelˉframe,
) -> Result<Publishˉreceipt, Publishˉfailure>
    effects(display.surface, task.suspend)

async input.event_batch.Next(
    Endpoint: Inputˉendpoint,
    Context: borrow Operationˉcontext,
    Maximumˉevents: u64,
    Expectedˉsequence: u64,
) -> Result<Eventˉbatch, Failure>
    effects(input.event_batch, task.suspend)

async timer.tick.Next(
    Endpoint: Timerˉendpoint,
    Context: borrow Operationˉcontext,
    Expectedˉsequence: u64,
) -> Result<Tick, Failure>
    effects(timer.tick, task.suspend)
```

Every call is an explicit suspension and observes the same borrowed operation
context. At the exact deadline, deadline wins. Pre-dispatch cancellation proves
zero publication/event/tick progress. No call constructs a context, requests
cancellation, refreshes an endpoint, or grants another capability.

## Event-batch meaning

`Next` returns exactly the requested next nonzero batch sequence or a typed
failure; it never silently skips, repeats, merges, or reorders a batch. The
immutable sequence has at most `Maximumˉevents` items in provider order and is
fully initialized before success. Returning more is `Invalidˉresponse` and no
event is applied. Empty batches are permitted but still consume one batch
sequence and the application-wide batch maximum.

The provider may coalesce native mouse/window notifications only into the exact
semantic event variants in this contract. Unknown native events are not packed
into integers or reflected records. Events after the first accepted `Close` in
one batch are an invalid provider response for this workload.

## Timer meaning

`Next` returns only `Expectedˉsequence`, with its exact provider generation,
after the endpoint's admitted monotonic tick condition. It exposes no civil
time, locale, timezone, sleep primitive, or independent cancellation flag. A
lost/restarted generation fails explicitly; this workload does not refresh or
replay a tick.

## Immutable frame publication

A frame contains exact width, height, byte stride, immutable RGBA8 bytes, source
event sequence, and layout generation. The provider validates:

- positive admitted dimensions;
- `Stride = Width * 4` with checked arithmetic;
- exact byte length `Stride * Height`;
- the endpoint's surface identity/generation and byte/frame ceilings; and
- monotonically increasing frame sequence.

Success means the local surface provider accepted every byte as one immutable
frame and returns its exact generation, sequence, and accepted byte count. It
does not prove physical scanout, remote viewing, GPU completion, or human
observation.

`Rejected` proves zero frame publication and returns the complete input frame.
`Indeterminate` means dispatch occurred but the provider cannot prove whether the
frame became visible; it does not return a replayable frame. Application source
must not retry either case automatically. Releasing a returned or retained frame
releases only local immutable backing/accounting.

## Restart and teardown

Provider loss/restart reports exact expected/observed generations. Old endpoints,
batches, ticks, and receipts never rebind to a successor. A launcher may start a
new application instance with newly approved endpoints; this workload performs
no in-place discovery or refresh.

Local endpoint/frame/event release is bounded and does not imply graceful
compositor, device, or user-session completion. Reserved provider recovery
capacity owns teardown after revocation or contained failure.
