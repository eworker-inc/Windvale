# Decision 0209: One current WVB 1.11 format

- Date: 2026-08-04
- Status: Implemented locally; independent cross-host qualification pending
- Supersedes: feature-dependent WVB 1.6 through 1.12 emission and the WVB 1.12 version allocation in [Decision 0207](0207-U64-Binary-Fields-For-Durable-Storage.md)
- Extends: [Decision 0138](0138-Conditional-Wvb-1-7-64-Bit-Scalars.md), [Decision 0184](0184-Language-Syntax-And-Operator-Evolution.md), and [Decision 0207](0207-U64-Binary-Fields-For-Durable-Storage.md)
- Contracts: [Seed bytecode](../../Specifications/Seed-Bytecode.md), [source-to-WVB backend](../../Specifications/Compiler-Source-Wvb.md), [Seed language](../../Specifications/Seed-Language.md), and [WebAssembly](../../Specifications/Windvale-WebAssembly.md)

## Context

The experimental writer selected the lowest WVB minor needed by each module. That preserved old fixture bytes, but it also made every reader, verifier, compiler backend, diagnostic, golden identity, and narrow target reason about a version ladder. The ladder no longer protected a named shipped consumer: Windvale remains in initial development, current artifacts are rebuilt from source, and the repository policy explicitly avoids compatibility layers for obsolete experimental formats.

The rule was product-negative. A source feature could change the outer format version without changing deployment intent; simple consumers could appear compatible because they accepted an old version rather than because they validated their real semantic subset; and the Windvale-written compiler could lag behind Stage 0 while still emitting a superficially valid older module. Decision 0207 briefly assigned the new `u64` byte codecs to WVB 1.12 while this consolidation was being developed in parallel. Keeping that allocation would immediately recreate the ladder being removed.

## Decision

WVB 1.11 is the sole canonical current bytecode format. Every general writer emits major `1`, minor `11`, all seven canonical sections, and the Module-section metadata-presence byte. Every general reader and verifier rejects any other major/minor pair. The writer does not calculate a lowest required version, and no compatibility shim reads or rewrites WVB 1.6 through 1.10 or the briefly proposed 1.12 encoding.

Opcodes `0xBD` and `0xBE` retain Decision 0207's exact `Bytesˉreadˉu64ˉlittle` and `Bytesˉfromˉu64ˉlittle` semantics as part of the complete WVB 1.11 vocabulary. This changes only their version allocation, not the durable-field rationale, checked range behavior, byte order, or runtime behavior.

Decision 0211 subsequently appends lossless `U64ˉfromˉu32` at opcode `0xBF` to that same sole current WVB 1.11 vocabulary. It does not introduce another minor version.

Narrow consumers use the same WVB 1.11 envelope and state their actual accepted subset explicitly. A consumer that does not implement module metadata requires the presence byte to be zero. Native, WebAssembly, Foundation inspection, and Windvale OS admission code may reject valid WVB 1.11 types, opcodes, metadata, graphs, or profiles outside their named subset; an older header version is never used as a proxy for that subset.

Complete `i64` and `u64` support moves into the Windvale-written compiler at the same boundary. Its lexer, declaration/body models, symbols, bindings, typed WVIR, and WVB lowering cover wide types, literals, constants, parameters, returns, records, locals, inference, checked arithmetic, division/remainder, comparisons, invariant formatting, unsigned bitwise/complement/shift operations, byte codecs, and the admitted compound assignments. Wide constant evaluation uses explicit low/high `u32` limbs and produces byte-identical WVB with Stage 0.

The compiler-capacity WebAssembly verifier retains execution ABI 3 and complete coverage while expanding from four to six import-free phases. The typed walk is partitioned into function indices 0–199, 200–299, and 300+; control/reachability is partitioned into 0–199 and 200+. This keeps every exact current-compiler run below the ABI's `u32` instruction meter rather than weakening verification or changing the execution ABI.

No backward-compatibility work is required through at least 2026-09-04. During that period, an exception requires a named consumer, fixture, migration need, and decision; elapsed time alone does not create a compatibility promise.

## Consequences

- One format version describes the complete current vocabulary and metadata layout.
- Adding source or opcode usage no longer changes a module's version or invalidates caches merely through lowest-version selection.
- All canonical WVB golden identities change because even metadata-free Module payloads gain the mandatory presence byte.
- Historical WVB 1.6 through 1.10 artifacts and the unqualified WVB 1.12 candidate remain historical evidence, not accepted inputs.
- Narrow consumers become easier to audit because their rejections identify real profile, metadata, type, opcode, and graph limits.
- Stage 0 and Windvale compiler byte equality includes the complete wide-scalar surface.
- Compiler-capacity WebAssembly verification uses deterministic function-range partitions with measured headroom below its fixed instruction-meter limit.
- The change requires refreshed deterministic identities and new Windows/Linux qualification before superseded cross-host byte claims are reasserted.

## Reconsideration triggers

- a released Windvale artifact creates a named support or migration obligation;
- an external consumer cannot rebuild and has a documented need for an older format;
- package distribution requires a negotiated format range rather than one repository-current contract;
- a future format change needs simultaneous old/new operation for staged deployment; or
- the 2026-09-04 review explicitly accepts a compatibility policy with owners, duration, tests, and removal rules.
