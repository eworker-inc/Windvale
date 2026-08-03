# Decision 0134: Windvale-native WebAssembly WVB structural verifier

- Date: 2026-08-03
- Status: Implemented with local Windows and Node.js evidence; semantic, cross-host, and cross-browser qualification pending
- Extends: [Decision 0131](0131-Windvale-Native-WebAssembly-Wvb-Envelope-Verifier.md)
- Target: `wasm32-browser-v1-experimental`

## Context

Profile 10 proved that a Windvale-written program could inspect the outer WVB 1.6 envelope as deterministic import-free WebAssembly. Removing the Stage 0 verifier from the normal editable playground requires substantially more: every section payload must be bounded and completely consumed before executable semantics can be trusted.

The first payload verifier is large enough to expose two selector scaling problems. Rechecking every candidate control target by rescanning the complete WVB instruction stream made selection exceed the retained 100,000,000-reference-instruction backend gate. Concatenating every emitted instruction directly into one growing function body also amplified immutable-byte reconstruction cost. These are backend implementation limits, not reasons to weaken the target verifier.

## Decision

- Add experimental profile 11 over execution ABI 3. It retains profile 10's runtime value and control operations while increasing the single runtime function ceiling to 2,047 nonparameter locals, 32,768 code bytes, and 100,000 decoded instructions. Generated Wasm remains independently bounded, now at 524,288 bytes.
- Build one byte of immutable instruction-boundary evidence per input code byte during the existing validation decode. Control-target checks then use constant-time mask lookup after range validation. A target must still be function entry or immediately follow a decoded terminator.
- Accumulate each generated Wasm basic block independently and append it to the function body only when the block closes. This changes selector construction cost without changing the bytes emitted for previously admitted modules.
- Add `Wvb-Structural-Verify-Main.wv`, a portable, capability-free `Main(Input: bytes) -> bytes` verifier. It returns `[1]` only after all seven canonical WVB 1.6 payloads are structurally bounded and exactly consumed; otherwise it returns `[0]` without a host import.
- Validate these payload properties:
  - module profile and bounded nonempty name extent;
  - bounded capability declarations and primitive signature shapes;
  - bounded data entries, kinds, element/byte lengths, and payload extents;
  - bounded function declarations, parameter/local type encodings, contiguous code ranges, and declared stack ceilings;
  - complete known-opcode decoding, exact instruction widths, canonical boolean constants, and a 100,000-instruction aggregate ceiling;
  - function exports with in-range targets; and
  - bounded record/enum payload shapes, field/member counts, names, and field-type extents.
- Keep structural acceptance distinct from canonical semantic verification. This slice does not validate UTF-8 or Windvale name grammar, declaration uniqueness or canonical order, capability catalog identity, nominal target identity, instruction operand indices, branch targets, typed stack flow, definite initialization, control-flow joins or reachability, maximum-stack agreement, export uniqueness, or executable capability authorization.

## Consequences

The target now inspects real nonempty capabilities, data, functions, code, exports, and nominal types entirely inside generated Wasm. It is no longer an envelope-only demonstration, but it is not yet safe to replace the mandatory Stage 0 WVB verifier. A structurally accepted module must still pass the semantic layer before execution or lowering.

The 113,385-byte verifier is larger than the former 65,536-byte selector and playground publication ceiling, so the bounded experimental limit is now 512 KiB. The 4 MiB ABI-3 input/output windows, fixed 129-page memory, two-second disposable-worker lifetime, and 100,000,000-reference-instruction hosted selector gate do not change.

This milestone advances item 4 of the active .NET-removal goal from envelope verification to complete section-payload consumption. Items 1 and 2 are already implemented; item 3 still lacks measured text/record/enum execution support, item 4 still needs semantic verification, and compiler execution, a fully worker-contained static pipeline, and cross-host/cross-browser/recovery qualification remain open.

## Local evidence

The structural verifier compiles to 19,755-byte WVB SHA-256 `72da44ba1292ed3ef4ac62c239dd937862636229a7d60302305a7dd19ac27376`. Its one function has 1,096 parameter-plus-local types, 4,062 WVB instructions, and declared maximum stack two. The selector emits a deterministic 113,385-byte import-free Wasm module with SHA-256:

```text
46fe579fb7082dd4b0dd981e09f6b953127e52c9c6993d7885ca130725762677
```

The reference runtime and Node.js return `[1]` for the verifier's own WVB in exactly 1,446,276 instructions. Budget 1,446,275 returns `WVR3011` with empty output. One mutation in each section rejects as `[0]`: invalid module profile, capability count 33, data count 4,097, function count 4,097, unknown opcode `0xFF`, invalid export target, and a truncated nonempty Types payload.

Three independently compiled nonempty inputs also return `[1]`: the 1,651-byte data/text WVB SHA-256 `5d0779925bee06b8e27afb5ccedd995fc83cbd6aa71954911a644cf078c71704` in 103,696 instructions; the 1,781-byte nominal-types WVB SHA-256 `1366b543a28a1921aca6198bca9eaaf5eeeb97766405d5efcdeff9d27cfca57a` in 94,466; and the 849-byte hosted-capabilities WVB SHA-256 `1df4503a21abf5f2c0b0307ac2dc79402bc8550ec5e4a016df43fdeb8197d528` in 28,803.

The C# oracle independently checks the admitted source shape and completely consumes the generated Wasm local, opcode, meter, dispatch, output-publication, fixed-memory, and export evidence. The local Windows focused Seed case passes, and the Node.js gate rebuilds, validates, and executes 24 generated Wasm artifacts plus the three positive structural inputs. Cross-host and real-browser evidence for profile 11 remain pending.

The composed playground backend WVB is SHA-256 `1b55616aa64af2324f7ec7f7c4cb1afa05a1e63af829e9ca032af8c1952382c8`. The standalone core WVB is SHA-256 `6c3ead31aa5dc50baa441b8f3021678761b7d5b6c2aaa0f549e73f5b028a8b35`.

## Rejected alternatives

Calling the existing C# verifier from JavaScript was rejected because it would preserve .NET at the exact boundary this milestone is intended to transfer.

Treating exact payload consumption as complete semantic verification was rejected because valid extents and known opcodes do not prove type flow, index validity, control-flow safety, canonical identity, or authorization.

Raising the hosted selector instruction gate was rejected as the primary scaling mechanism. The immutable boundary mask removes repeated work while retaining the established gate and stronger target validation evidence.

## Reconsider when

- Semantic verification needs bounded lookup tables or runtime operations beyond the current primitive/bytes profile.
- A measured verifier or compiler workload needs multiple descriptor-bearing functions instead of one large inlined function.
- Direct structured-control reconstruction can replace the dispatcher without weakening malformed-target evidence.
- Profile 11 has cross-host and cross-browser evidence and is ready for an intentional standalone-page advance.
