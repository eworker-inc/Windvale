# Decision 0144: Modular WebAssembly WVB canonical metadata and references

- Date: 2026-08-03
- Status: Implemented with local Windows and Node.js evidence; cross-host and cross-browser qualification pending
- Extends: [Decision 0139](0139-Descriptor-Bearing-WebAssembly-Call-Graph.md)
- Target: `wasm32-browser-v1-experimental`

## Context

Profile 11 executes one Windvale-written structural WVB consumer, and profile 12 supplies real descriptor-bearing calls so a larger verifier can be separated into bounded phases. The next trustworthy-browser boundary is canonical WVB semantic verification. That boundary includes two materially different problems: canonical declaration/reference validation, which can be proved through bounded rescans, and executable type/control-flow validation, which must track stack and local state at every reachable instruction.

Combining both in the first modular artifact would obscure which properties are actually proved. It would also make failures at profile 12's exact aggregate-code ceiling difficult to diagnose. The first modular consumer therefore needs to close the canonical metadata and reference gap without claiming the remaining executable-flow proof.

## Decision

- Add `Tests/Fixtures/WebAssembly/Wvb-Semantic-Verify-Main.wv` as an eight-function Windvale-written profile-12 consumer over unchanged execution ABI 3.
- Keep one zero-copy `bytes -> bytes` descriptor through every phase. Successful phases return the original descriptor, rejection returns an empty descriptor, and `Main` publishes `[1]` only after every phase succeeds.
- Split the verifier into these decreasing-ordinal call-graph owners:
  - Seed identifier and U+02C9 separator grammar;
  - complete WVB 1.6 structural consumption;
  - module profile and capability catalog identity;
  - canonical data names and strict UTF-8 text payloads;
  - canonical function names plus nominal declaration shapes;
  - instruction operand indices, data kinds, exact branch boundaries, canonical exports, and export-to-function identity; and
  - canonical nominal types, fields, enum members, cross-kind type-name uniqueness, enum backing-value uniqueness, and record-to-enum field identity.
- Retain profile 12's existing limits. The artifact has exactly 65,536 aggregate WVB code bytes, so this milestone does not silently increase the accepted module or generated-Wasm ceilings.
- Treat this as the canonical metadata/reference phase of semantic verification, not the complete mandatory semantic verifier. Typed operand-stack flow, definite local initialization, call argument/result flow, record-field receiver identity, control-flow joins, reachability, and declared maximum-stack agreement remain a separate phase.
- Keep the standalone editable-input page on its smaller profile-8 artifact. This verifier is qualification evidence and a future worker-pipeline component; it is not yet the default playground verifier.

## Consequences

The browser path can now execute a Windvale-written verifier that goes beyond section structure and rejects semantically hostile names, catalogs, declarations, identities, indices, data references, and control targets without JavaScript or .NET participation in the verification operation. The artifact also proves that profile 12's eight-function boundary is sufficient for this phase.

The remaining mandatory verifier work is narrower and explicit: executable type and control-flow proof. Until that phase, a browser worker must not authorize or execute arbitrary uploaded WVB solely because this artifact returns `[1]`. Stage 0 remains the complete oracle and the normal editable playground remains .NET-based.

Exact branch-boundary validation uses bounded allocation-free rescans. This is suitable for ordinary playground-sized inputs but quadratic in branch-heavy code. The 70,016-byte verifier module itself exceeds 500,000,000 dynamic verification instructions, while the retained representative inputs complete below 1.2 million. Self-verification performance is therefore recorded as a measured optimization pressure, not hidden as qualification evidence.

## Local evidence

The Windvale source compiles to a 70,016-byte WVB module with SHA-256:

```text
09a665dcfbf8fe70d9b830be8376b1c1353a5ef09ff10de7b0183e535036fa64
```

Its functions occupy exactly 65,536 aggregate code bytes. The retained profile-12 backend lowers it in 129,151,253 instructions to a deterministic 440,093-byte import-free Wasm module with SHA-256:

```text
a2ef01881a4d381154a0e3feb0cb74cb0cdb3a53631cae1206d2fc03bcabe2fa
```

The reference runtime and Node.js agree on `[1]` for the data/text, nominal-type, and hosted-capability fixtures in exactly 1,122,085, 912,951, and 113,457 instructions. Budget 1,122,084 produces `WVR3011` with no output for the first fixture.

Both engines reject thirteen structurally valid semantic mutations with empty output: invalid module identifier, portable capabilities, changed capability signature, noncanonical data order, invalid text UTF-8, noncanonical function order, wrong nominal function shape, wrong data kind for `text.const`, branch into an instruction operand, export/function-name mismatch, duplicate cross-kind type name, duplicate enum backing value, and a record field redirected from an enum to a record. The C# oracle independently rejects every same mutated byte sequence.

The independent C# Wasm decoder consumes both target types, nine functions including the public wrapper, fixed memory, eleven globals, the exact ten-export ABI, wrapper locals, and every private local layout. The focused Seed WebAssembly test and complete 26-artifact Node.js gate pass locally on Windows. Changed-scope verification on base `dcc694a` produces a zero-warning Release build and passes all 80 selected Seed tests, including the golden compiler contract; this is development feedback rather than cross-host qualification. After rebase onto `c5e72bb`, the focused WebAssembly test passes again against the updated reference runtime. Earlier artifacts retain their exact identities.

## Rejected alternatives

Calling this the complete semantic verifier was rejected because typed executable flow remains unproved and the distinction is security-relevant.

Duplicating parsing and validation in JavaScript was rejected because the milestone is specifically Windvale ownership of the portable verifier; JavaScript remains an independent engine and ABI host.

Raising profile 12's aggregate-code limit was rejected because small semantic simplifications brought the artifact to the existing exact ceiling without removing a check.

Using Stage 0 collections or a new mutable-memory ABI for declaration lookup was rejected for this phase because bounded canonical rescans are sufficient and preserve the existing import-free ABI.

## Reconsider when

- The executable type/control-flow verifier needs additional functions or aggregate code.
- Representative editable programs show branch-boundary rescans dominating browser latency.
- A bounded immutable lookup or boundary-evidence format can reduce rescans without weakening validation.
- Matching Windows/Linux construction and Chromium, Firefox, and WebKit execution evidence is available.
