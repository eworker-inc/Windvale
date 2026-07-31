# Decision 0036: Initial Windvale-written WVB backend

- Date: 2026-07-30
- Status: Accepted; cross-host qualified

## Context

Decision 0035 created an independently validated typed WVIR boundary. The next useful proof must be executable: Windvale code should turn that evidence into a complete WVB module accepted by the mandatory verifier and reference runtime.

Implementing every current source feature in one backend slice would combine data interning, nominal layout, declaration sorting and index remapping, capabilities, imports, code generation, and complete metadata serialization. Introducing another general code-plan format would add a new boundary before proving that WVIR is sufficient.

## Decision

Implement direct deterministic WVIR-to-WVB lowering for a strict initial function-only subset in `Compilerˉsourceˉwvb`.

The accepted input is exactly one portable module whose declarations are strictly ordinal functions and whose values use only `void`, `i32`, `u8`, `u32`, and `bool`. The backend supports constants, locals, calls, arithmetic, comparisons, boolean operations, jumps, branches, and returns. All other declarations, shapes, profiles, and operations are explicit deterministic rejections.

Every WVIR temporary is assigned a WVB local. Operations spill their results so the operand stack is empty at operation and block boundaries. A first pass computes block offsets, code length, and maximum stack depth; a second pass emits immutable bytes without branch patching. The complete seven-section WVB 1.6 envelope is written directly.

The initial ordinal-source restriction deliberately keeps WVSD identities, WVIR call targets, WVB function indices, and export targets equal. Later metadata slices must introduce an explicit validated remapping table before lifting that restriction.

## Consequences

Windvale now has an executable compiler path written in Windvale itself. The first differential fixture is byte-identical to Stage 0, passes the mandatory verifier, and executes in the existing runtime.

This is not yet a self-hosted compiler. It does not compile its own frontend closure and does not support imports, capabilities, static data, nominal types, text/bytes, or Foundation intrinsics. Those omissions are named backend boundaries rather than compatibility promises.

Direct WVB emission remains the preferred route while each added metadata family can be independently verified. A new intermediate representation should be introduced only if a later backend or optimization requirement demonstrates a concrete shared contract that WVIR cannot express.

## Verification gate

The candidate must pass:

- the focused source-to-WVB conformance case;
- exact byte equality with the Stage 0 compiler for the function-only fixture;
- Stage 0 WVB verification and runtime execution of the generated module;
- the complete Standard suite on Windows; and
- exact-commit Windows and Debian qualification with matching normalized reports and byte-identical retrieved artifacts.

Candidate commit `ca5699617d713b4da3689cd69b0b165abc9c090e` passed the full gate from one exact source archive on Windows x64 and Debian GNU/Linux 12 x64. Both hosts completed zero-warning Release builds, all 48 tests, the complete native verifier, matching normalized conformance contracts, and byte-identical comparison of all 53 retrieved portable artifacts. After the shared verifier-maintenance commit was integrated, published implementation commit `d65d286` retained the exact qualified Git tree `bfe879b67d5f6f197b1ace0cdd9b0ad221f94906`. The detailed archive, report, timing, and artifact identities are retained in `Documents/Project/Seed-Verification-Evidence.md`.
