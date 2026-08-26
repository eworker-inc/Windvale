# Decision 0860: lower frame-owned callables through the native x64 ABI

## Status

Accepted on 2026-08-26.

## Context

Decisions 0852 through 0859 established exact function identities, effects,
capture validation, deterministic synthetic targets, WVLB 1.4 bindings, WVIR
callable operations, WVB 1.30/1.31 verification, and portable scalar execution.
The main compiler still did not construct a synthetic closure body from those
products, enforce move and borrow state through persisted WVIR, or lower a
callable through the shared native x86-64 backend.

A general heap closure or dynamic dispatch table would widen ownership,
allocation, lifetime, calling convention, and corruption boundaries at once.
The first native slice instead needs one complete useful profile whose limits
and rejection behavior remain visible.

## Decision

1. The source compiler appends each synthetic closure function after ordinary
   functions and final generic instances, using its validated WVCL ordinal and
   WVLB 1.4 range. The physical parameter prefix is captures in declaration
   order, followed by public parameters.
2. The parent emits one `Closureˉcreate` with those exact capture operands. The
   synthetic body resolves capture and parameter bindings through their exact
   physical slots and emits an ordinary typed return.
3. `copy`, `move`, and immutable `borrow` are executable only for the admitted
   inline scalar and enum environment. Move invalidates the outer slot along
   every reachable WVIR path. Immutable borrow is allowed only from a parameter
   or `let`, and its callable must remain a single local consumed by
   `Callˉindirect` before the owner lifetime ends. Mutable borrow, escape,
   retained descriptors, aggregates, collections, resources, and nested
   callables reject before WVB publication.
4. Canonical WVB 1.30 and 1.31 callable descriptors have the containing
   module's exact profile. The portable verifier rejects a different profile,
   not merely an out-of-range value.
5. The native lowerer accepts WVB 1.11, 1.30, and 1.31 under exact version
   feature rules. It admits at most 128 total Types entries and 128 callables
   within that total, 64 public parameters, 64 captures per closure, 65,536
   reference/call operations, and 65,536 closure creations per module.
6. One native callable occupies one 16-byte frame cell: physical target `u32`,
   callable Types identity `u32`, and environment address `u64`. `D3` carries a
   zero environment. `D5` copies captures into creating-frame-owned 16-byte
   cells. There is no heap allocation, serialized host pointer, ambient grant,
   hidden retention, or alternate call ABI.
7. `D4` revalidates target, type, and environment before entry, copies the
   immutable capture prefix into call scratch, appends public arguments, and
   uses the existing direct-call ABI, instruction meter, and depth meter.
8. The first profile requires one unique physical target per callable
   descriptor, confines callable operations to a one-block function, reserves
   at most 1,024 environment cells per function, and keeps the complete frame
   within 2,048 cells. Callable parameters/results, aggregate-contained
   callables, wider control flow, general same-signature dispatch, and escaping
   environments remain separately versioned work.
9. The hosted enum-request reader accepts callable descriptors as zero-member
   type entries for WVB 1.30/1.31 so native packaging preserves stable nominal
   indices without treating the metadata reader as a semantic verifier.
10. The segmented WVO symbol validator admits the already specified aggregate
    maximum of 1,280 symbols: at most 1,024 functions plus at most 256 data
    declarations. The former 768-symbol private ceiling rejected the enlarged
    lowerer even though both component inventories remained within contract; it
    is removed rather than raising either function or data limit.

## Consequences

The real edition-1 compiler can now compile, verify, execute, lower, link, and
package a captured closure through one coherent WVB/native path. Move and borrow
requirements remain static proofs; the native representation cannot outlive the
frame whose bounds make its environment address meaningful.

The unique-target and single-block restrictions are intentionally visible.
They keep corruption checks and native code generation simple while avoiding a
premature heap owner or dispatch table. Unsupported programs fail closed rather
than receiving a subtly different calling or lifetime model.

Nonempty-effect or `async`/`unsafe` function values, write-through mutable
captures, retained text/bytes or aggregate captures, general dispatch, and
escaping environments are not silently implemented by this decision. They
require their own representation, ownership proof, target evidence, and format
version where serialization changes.

## Evidence

The split compiler produces three deterministic WVB 1.31 programs:

| Capture | WVB bytes | SHA-256 |
| --- | ---: | --- |
| `copy` | 451 | `8000144daaab85c10698e6205729f7de6798f866f69ed32861cf1e2c8daafc03` |
| `move` | 451 | `b95e5bd8e20584f73f55f34ff9de0e5a9fe03ab9118bf48adba70b1078a17cca` |
| immutable `borrow` | 453 | `d8c6632dc52a8337af4fac4711a09c8fd4089174351f278fe8debfc51304f7dd` |

Each executes to `42` in 31 guest instructions. A use after move and a mutable
borrow publish no WVB. The copy program lowers to a 2,537-byte WVO at SHA-256
`13b3031377dc0dd81a94cc9dfbacff954cf125b2a49522c0c16420990c87e0cf`
and its current-host packaged application exits `42`. The retained 400-byte
noncapturing WVB lowers to a 1,972-byte WVO at SHA-256
`7c89115a1bb3b23e215e5a2780b07a0ddaaa6bbb15f3cfe068dc9d545aa0a6ea`
and its linked application also exits `42`.

The focused callable owner reports:

```text
native language 1 callable semantics status=Passed cases=60 result=42 modules=11 evidence-bytes=4270904 native-aot-cases=8 evidence-sha256=287b0fd511d00a0f98356bc2fbf9d75e0c67a2f973a4468d48d965ab030c5613
```

Its native portion contains two successful AOT executions and six rejection
cases covering version downgrade, target signature, capture shape, and profile
mismatch. The portable verifier separately rejects the malformed profile.

The rebuilt WVB-to-WVO candidate is 567,615 bytes at SHA-256
`d6831ce5145cb3bbe5b607293762f220829d77586ad96fedcec9f8c7b57719a3`;
its Windows and Linux applications are 8,160,256 and 8,159,232 bytes at
SHA-256 `6a33f19d38f689e35776a7d3d88f09c2f06046312d8eeb629e669245e3333102`
and `5cb17d2e6fd8a02721bd2249623bff65891f4ac6149cc44e60a5849c51774029`.
These are candidate identities, not paired-host qualification or promotion.

The repinned compiler-image staging component remains 75,666 WVB bytes at
SHA-256 `ac01daa598f67d34ae5ed9dbc83a168dc288c05f7369b0773713947f0d5a85cd`.
Its Windows and Linux staging applications remain 854,016 and 856,064 bytes at
SHA-256 `c46534cd0fbbd294d2aa242a3ed26ca3ef663d6b1e054290befe0f4edc426da4`
and `360f05b19181f001439a8309f571d0979eef260285f4995d7dcae1f06679a445`.
Only the aggregate symbol admission and resulting identities change; the
64-MiB image, 1,024-function, 256-data, and 16-fragment limits do not widen.

The native verification registry contains 113 owners and 5,507 cases. Its
18,052 LF-only bytes have SHA-256
`b1ca2a737b8174b1d2959e9375275bc6dd6dc225bc1c233f883ea0b8434c08fb`.

## Reconsideration triggers

Reconsider this ABI when a real Language 1.0 workload needs two runtime targets
with one structural signature, a callable parameter or result, a callable that
crosses control-flow blocks, retained captures, or an environment that escapes
its creating frame. Any successor must keep exact target/type validation,
explicit authority, bounded ownership and cleanup, deterministic output, the
ordinary meters, and a simple comparison oracle.
