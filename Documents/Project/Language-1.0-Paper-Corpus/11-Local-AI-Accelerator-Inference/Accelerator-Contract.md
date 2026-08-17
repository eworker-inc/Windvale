# Workload 11 paper accelerator contract

## Status

This is the smallest accelerator contract required to type and review workload
11. It is a paper candidate, not a normative specification, capability-catalog
publication, provider ABI, WIR/WVB selection, or implementation claim. Names and
signatures become frozen only through a later decision after paper review.

The contract refines the four-layer boundary in the
[accelerator compute and AI design](../../Windvale-Accelerator-Compute-And-AI-Design.md):
ordinary host/framework source, portable accelerator operations, one target
kernel part, and replaceable providers.

## Minimum capability split

| Required capability | Responsibility | Explicitly excluded authority |
| --- | --- | --- |
| `accelerator.catalog` version 1 | Select one provider generation satisfying exact formats, operations, modes, attachment policy, and limits. | Allocation, execution, kernel loading, profiling, native extensions, or physical assignment. |
| `accelerator.memory` version 1 | Open one bounded session and atomically admit one rights-limited five-slot residency. | Arbitrary device memory, native mappings, execution, kernels, or passthrough. |
| `accelerator.execute` version 1 | Build and submit the portable upload/linear/readback command set and await one typed terminal result. | Custom kernels, native commands, profiling, or physical assignment. |
| `accelerator.kernel` version 1 | Add only the statically package-bound `windvale.paper.bias_relu.v1` kernel with its exact interface. | Runtime arbitrary code, native vendor kernels, profiling, or broader device access. |

No single capability implies another. The application declares the complete
transitive set, the launcher approves it, and the provider binds four
rights-limited singleton references. This workload does not require native-device
extension, profiling, partition, or passthrough authority.

## Supplied nominal types

`Platformˉaccelerator` supplies these candidate public values:

- `Requirements`, `Sessionˉlimits`, `Residencyˉplan`, `Tensorˉview`,
  `Quantizedˉlinearˉoperation`, and `Kernelˉoperation` are ordinary exact records
  constructed by source;
- `Bufferˉslot`, `Elementˉformat`, `Viewˉrights`, `Numericˉmode`, and
  `Attachmentˉmode` are closed fixed-width enums;
- `Failure` is a Copy record containing exact kind, stage, provider generation,
  requested amount, and applicable limit;
- `Selection` is a Copy, non-authority provider-generation selection witness and
  `Providerˉdescription` is its bounded observable identity/mode/limit record;
- `Session`, `Residency`, `Commandˉbatch`, and `Submission` are opaque move-owned
  resources implementing local release; and
- `Terminal` is a closed variant carrying completed bytes and provider evidence,
  cancellation, deadline, provider loss, or contained fault.

Opaque resource types are supplied platform-library types, not user records with
hidden fields. Source cannot inspect a native handle, address, queue, device
pointer, command encoding, or provider object.

The source-visible record and enum fields used by this bundle are exact:

```text
enum Bufferˉslot: u8 {
    Input = 1;
    Weights = 2;
    Parameters = 3;
    Accumulator = 4;
    Output = 5;
}

enum Numericˉmode: u8 {
    Boundedˉf32ˉv1 = 1;
}

enum Elementˉformat: u8 {
    F16 = 1;
    Signedˉi4 = 2;
    F32 = 3;
}

enum Viewˉrights: u8 {
    Read = 1;
    Readˉwrite = 2;
}

enum Attachmentˉmode: u8 {
    Software = 1;
    Paravirtualˉshared = 2;
    Hardwareˉpartition = 3;
    Exclusiveˉpassthrough = 4;
}

record Tensorˉview {
    Slot: Bufferˉslot;
    Elementˉformat: Elementˉformat;
    Rank: u8;
    Extent0: u32;
    Extent1: u32;
    Stride0ˉelements: u64;
    Stride1ˉelements: u64;
    Byteˉoffset: u64;
    Byteˉlength: u64;
    Rights: Viewˉrights;
}

record Requirements {
    Operationˉset: u32;
    Weightˉformat: u8;
    Activationˉformat: u8;
    Accumulatorˉformat: u8;
    Weightˉlayout: u8;
    Minimumˉdeviceˉbytes: u64;
    Maximumˉqueues: u32;
    Maximumˉcommands: u32;
    Deterministicˉcollection: bool;
    Allowˉsoftware: bool;
    Allowˉparavirtualˉshared: bool;
    Allowˉhardwareˉpartition: bool;
    Allowˉexclusiveˉpassthrough: bool;
}

record Providerˉdescription {
    Providerˉidentity: text;
    Providerˉgeneration: u64;
    Attachmentˉmode: Attachmentˉmode;
    Maximumˉdeviceˉbytes: u64;
    Maximumˉqueues: u32;
    Maximumˉcommands: u32;
    Maximumˉworkˉunits: u64;
    Maximumˉdiagnostics: u32;
    Operationˉset: u32;
    Weightˉformat: u8;
    Activationˉformat: u8;
    Accumulatorˉformat: u8;
    Weightˉlayout: u8;
    Numericˉmode: Numericˉmode;
}

record Sessionˉlimits {
    Maximumˉhostˉstateˉbytes: u64;
    Maximumˉdeviceˉbytes: u64;
    Maximumˉpinnedˉhostˉbytes: u64;
    Maximumˉqueues: u32;
    Maximumˉsubmissions: u32;
    Maximumˉcommands: u32;
    Maximumˉdiagnostics: u32;
    Maximumˉworkˉunits: u64;
}

record Residencyˉplan {
    Inputˉbytes: u64;
    Weightˉbytes: u64;
    Parameterˉbytes: u64;
    Accumulatorˉbytes: u64;
    Outputˉbytes: u64;
    Maximumˉchargedˉbytes: u64;
}

record Quantizedˉlinearˉoperation {
    Inputˉelements: u32;
    Outputˉelements: u32;
    Input: Tensorˉview;
    Weights: Tensorˉview;
    Scales: Tensorˉview;
    Accumulator: Tensorˉview;
    Inputˉformat: u8;
    Weightˉformat: u8;
    Weightˉlayout: u8;
    Accumulatorˉformat: u8;
    Numericˉmode: Numericˉmode;
}

record Kernelˉoperation {
    Identity: text;
    Interfaceˉidentity: text;
    Dispatchˉx: u32;
    Accumulator: Tensorˉview;
    Bias: Tensorˉview;
    Output: Tensorˉview;
}

record Failure {
    Kind: u32;
    Stage: u32;
    Providerˉgeneration: u64;
    Requested: u64;
    Limit: u64;
}

variant Terminal {
    Completed(
        Output: bytes,
        Providerˉidentity: text,
        Providerˉgeneration: u64,
        Attachmentˉmode: Attachmentˉmode,
    );
    Cancelled;
    Deadlineˉreached;
    Providerˉlost(Generation: u64);
    Fault(Error: Failure);
}
```

`Selection` and the four move-owned resources remain opaque supplied nominal
types. A normative child specification must publish these declarations' exact
canonical module identities and format-independent ABI signatures before
implementation; this paper source is the signature-review input.

## Operation signatures

The capability catalog supplies one exact signature set equivalent to:

```text
accelerator.catalog.Select(
    Requirements: Requirements,
    Context: borrow Operationˉcontext,
) -> Result<Selection, Failure>

accelerator.catalog.Describe(
    Selection: borrow Selection,
    Context: borrow Operationˉcontext,
) -> Providerˉdescription

accelerator.memory.Openˉsession(
    Selection: Selection,
    Limits: Sessionˉlimits,
    Context: borrow Operationˉcontext,
) -> Result<Session, Failure>

accelerator.memory.Admitˉresidency(
    Session: borrow mut Session,
    Plan: Residencyˉplan,
    Context: borrow Operationˉcontext,
) -> Result<Residency, Failure>

accelerator.execute.Createˉbatch(
    Session: borrow mut Session,
    Residency: borrow mut Residency,
    Maximumˉcommands: u32,
    Context: borrow Operationˉcontext,
) -> Result<Commandˉbatch, Failure>

accelerator.execute.Addˉupload(
    Batch: borrow mut Commandˉbatch,
    Residency: borrow mut Residency,
    Slot: Bufferˉslot,
    Source: bytes,
    Sourceˉoffset: u64,
    Sourceˉlength: u64,
    Context: borrow Operationˉcontext,
) -> Result<unit, Failure>

accelerator.execute.Addˉquantizedˉlinear(
    Batch: borrow mut Commandˉbatch,
    Residency: borrow mut Residency,
    Operation: Quantizedˉlinearˉoperation,
    Context: borrow Operationˉcontext,
) -> Result<unit, Failure>

accelerator.kernel.Addˉkernel(
    Batch: borrow mut Commandˉbatch,
    Residency: borrow mut Residency,
    Operation: Kernelˉoperation,
    Context: borrow Operationˉcontext,
) -> Result<unit, Failure>

accelerator.execute.Addˉreadback(
    Batch: borrow mut Commandˉbatch,
    Residency: borrow mut Residency,
    Slot: Bufferˉslot,
    Sourceˉoffset: u64,
    Sourceˉlength: u64,
    Context: borrow Operationˉcontext,
) -> Result<unit, Failure>

accelerator.execute.Submit(
    Session: borrow mut Session,
    Residency: borrow mut Residency,
    Batch: borrow mut Commandˉbatch,
    Context: borrow Operationˉcontext,
) -> Result<Submission, Failure>

async accelerator.execute.Wait(
    Submission: borrow mut Submission,
    Context: borrow Operationˉcontext,
) -> Result<Terminal, Failure>
```

Each signature has the capability identity as an effect. Resource construction
also has `resource.acquire`; provider-visible local release has the applicable
capability and `resource.release`; `Wait` has `task.suspend`. The source wrappers
retain these effects rather than hiding them through a generic adapter. Every
operation receives the one scope-derived `Operationˉcontext` and observes its
deadline/cancellation generation before publishing a mutation. All operations
except `Wait` are bounded, non-suspending admission or batch-construction calls;
an implementation that may suspend one must expose an async signature and
`task.suspend` instead of hiding the continuation.

## Admission and mutation rules

Selection validates operation set 1, signed-I4/f16/f32/row-major formats,
`Boundedˉf32ˉv1`, one queue, eight command slots, deterministic result collection,
at least 320 charged device bytes, and one explicitly allowed attachment mode.
This workload permits software and paravirtual shared providers and forbids
hardware partitions and exclusive passthrough because their independent
assignment capabilities are absent. Unsupported requirements reject without a
session or allocation.

`Describe` is total for the immutable selection witness and returns provider
identity (maximum 128 UTF-8 bytes), generation, attachment mode, device-byte,
queue, command, work, diagnostic, operation-set, format, and numeric-mode limits
before session construction. It grants no new authority. Session construction
revalidates that witness against current provider generation. Terminal provider
identity/generation/mode must equal the description or the application rejects
provider evidence.

Opening a session reserves its complete host, pinned, queue, command, work,
diagnostic, and device ceilings against one exact provider generation. Residency
admission atomically reserves all five slots. A rejected plan publishes no
partial slot and leaves the session unchanged.

For this workload the session record admits at most 4,096 host-state bytes, 64
pinned-host bytes, 320 charged device bytes, one queue, one submission, eight
commands, 64 work units, and 16 diagnostic records. The provider identity later
returned by completion has a 128-byte UTF-8 maximum inside the same host-state
ceiling.

Every `Add` operation is all-or-nothing. It validates complete source ranges,
slot length, command count, provider generation, operation/format identity,
shape products with checked arithmetic, alias rules, and dependencies before
mutating the batch. A successful upload command retains one shared immutable
source value; it does not expose or require a native mapping. A failed add leaves
the batch and residency unchanged.

A `Tensorˉview` is Copy geometry, not a pointer, resource, or stored source
borrow. Its use always accompanies a live borrowed `Residency`. The `Add`
operation validates slot generation, rank, extents, element format, logical
strides, packing, byte range, rights, and alias rules and then retains the exact
residency generation in the batch. Rank one canonically uses `Extent1 = 1` and
`Stride1ˉelements = 0`. Signed-I4 strides count logical elements; its named
format/layout contract converts the checked logical range to packed bytes.

`Submit` seals the command batch and either rejects before device acceptance or
publishes one submission. It retains exact resource generations until terminal
completion and prevents mutation or release that could invalidate admitted
commands. The batch remains locally owned for diagnostic inspection but cannot
be changed after acceptance.

## Six-command graph

Commands execute in this exact dependency order:

1. upload eight f16 input bytes from tokenizer offset 16;
2. upload four packed-I4 weight bytes;
3. upload 16 f32 scale/bias bytes from model offset 24;
4. run the 2-by-4 quantized-linear operation into the f32 accumulator slot;
5. dispatch two lanes of `windvale.paper.bias_relu.v1` into the output slot; and
6. read back exactly eight output bytes.

The provider may batch transfers or fuse implementation work only if failure,
completion, numeric, diagnostic, and result-order behavior remains identical.
The portable contract never exposes whether a tensor core or vendor library was
used.

The quantized-linear command reads the two row scales from parameter bytes zero
through seven and does not read the two biases at bytes eight through 15. The
custom kernel receives those bias bytes as its second f32 view.

## Numeric mode

`Boundedˉf32ˉv1` admits only the exact workload formats and shape. It requires:

- signed-I4 two's-complement unpacking, low nibble first and row-major;
- finite f16 input values converted without changing their mathematical value;
- f32 products, f32 accumulation, and f32 scale application;
- no hidden accumulator narrower than f32;
- scale after accumulation and bias only in the custom kernel;
- finite two-element f32 output; and
- the comparison limit in [Reference-Oracle.md](Reference-Oracle.md).

It may use a different fixed reduction tree within the comparison limit. It may
not enable ambient fast math, flush an admitted nonzero value, change packing,
substitute stochastic rounding, reorder result elements, or return NaN/infinity.

## Custom-kernel interface

The kernel source is the ordinary pure scalar function
`Biasˉreluˉlane(Accumulated: f32, Bias: f32) -> f32`. The package-bound target
interface maps two provider-validated input-view lanes to two independent calls
and stores returned f32 values in lane order. The kernel ABI guarantees:

- dispatch X is exactly two;
- accumulator and bias views each contain two initialized f32 values;
- the output view contains two writable f32 positions and aliases neither input;
- bias begins at byte 8 of the 16-byte parameter slot;
- all view range products are checked before dispatch;
- each scalar call receives Copy values and cannot observe another lane or an
  address;
- result collection writes each returned value to its matching lane without
  creating overlapping mutable source borrows; and
- no recursion, allocation, task, capability call, barrier, atomic, subgroup,
  native extension, or unbounded loop is admitted.

The same Language 1.0 front end parses and types the function. A target-specific
verified kernel representation and backend may be required after ordinary WIR;
that representation does not define source semantics.

## Terminal outcomes

`Terminal` has exactly:

- `Completed(Output: bytes, Providerˉidentity: text,
  Providerˉgeneration: u64, Attachmentˉmode: Attachmentˉmode)`;
- `Cancelled`;
- `Deadlineˉreached`;
- `Providerˉlost(Generation: u64)`; and
- `Fault(Error: Failure)`.

Completed proves all six commands reached their declared completion boundary,
output bytes are locally available, and no later device write can change them.
Provider identity contains at most 128 UTF-8 bytes. Attachment mode is one of
software, paravirtual shared, hardware partition, or exclusive passthrough and
does not by itself prove a performance or isolation class. `Cancelled` is
reported only after no
later private output can publish and teardown can proceed. If cancellation races
with completion, the provider returns the one outcome it can prove; it does not
report rollback. Provider loss and fault remain distinct. This workload has no
external mutation, so uncertain private output is discarded and never retried
or published.

## Release and loss

Local release consumes and invalidates each resource even after provider loss.
Normal reverse order is submission, batch, residency, then session. Residency
release waits for or fails retained commands, invalidates buffer generations,
revokes mappings/DMA, releases all five slots, and credits 320 device bytes.
Session release returns queue, pinned, host-state, diagnostic, and work charges.

Provider restart creates a new generation. Selection, session, residency, batch,
submission, and kernel admission from the prior generation are stale and can
never bind silently to the new provider.

## Software oracle and physical providers

The software provider implements the same capability signatures, command graph,
kernel interface, ownership, failure, and numeric contract and must produce the
exact reference bytes. It qualifies semantics only.

A Windows, Linux, Windvale OS, SPIR-V, or vendor adapter may be added later. Each
reports provider identity, generation, attachment mode, supported formats and
modes, limits, and fault/reset behavior. No adapter may weaken this contract or
turn its implementation API into Windvale semantics.
