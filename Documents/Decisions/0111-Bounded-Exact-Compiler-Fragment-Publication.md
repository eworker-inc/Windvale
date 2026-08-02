# Decision 0111: Bounded exact-compiler fragment publication

- Date: 2026-08-02
- Status: Implemented; cross-host qualification pending
- Retains: Native ABI 20 and target `x86-64-wvb-baseline-v20`
- Refines: [Decision 0082](0082-Windvale-Owned-Native-Publication-Layout.md) and [Decision 0109](0109-Native-Two-Byte-Little-Endian-Construction.md)

## Context

Decision 0109 cleared the last observed operation blocker in exact compiler preflight. The baseline selector then produced 4,556,121 bytes against a 1,048,576-byte fragment ceiling. That measurement alone did not distinguish avoidable selector expansion from a legitimate admission limit, so Decision 0109 deliberately retained the smaller bound.

The selector already records exact function ranges. Deterministic attribution now shows that 4,555,263 bytes are function code and only 858 bytes are alignment plus immutable data. The fragment contains 328 functions, 191,632 machine-IR operations, and 48,578 zeroed frame slots. Its five largest functions are 135,731, 110,901, 94,127, 80,123, and 73,853 bytes; no individual function approaches the old whole-fragment ceiling.

Current frame initialization costs 28 emitted bytes per zeroed 16-byte cell plus one two-byte zeroing instruction per function, or 1,360,840 bytes for this compiler. Even eliminating that code entirely would leave 3,195,281 bytes. A local zeroing or instruction-encoding change therefore cannot retain the 1 MiB whole-fragment limit. Function-granular publication would still require one nearby address domain for existing relative calls while adding post-selection layout and relocation machinery; it would not reduce the executable-image extent.

## Decision

- Retain one contiguous, independently decoded baseline fragment and increase its hard code ceiling from 1 MiB to 8 MiB. Eight MiB admits the exact 4,556,121-byte compiler with bounded headroom and remains below the already-qualified 34 MiB executable-publication image ceiling.
- Apply the same exact bound in native selection, independent fragment verification, Stage 0 request construction, and the Windvale-written `WVPQ 1` publication planner. Zero and 8 MiB plus one remain fail-closed boundaries.
- Keep `WVPQ 1` and `WVPL 1`. Their field layout, arithmetic, statuses, canonical placement, and 34 MiB final-image limit do not change; the accepted range of the existing fragment-size field expands compatibly. Regenerate and pin the retained Windvale planner bridge from source.
- Retain native ABI 20, execution context 7, service table 5, all machine encodings, WVB 1.6, and WVO 1.0. The selected compiler bytes remain exactly 4,556,121 with SHA-256 `8e74707df03a535e3ef68cfcfc8da6fa68fda29ccf4344e272fc50c8a5845bab`.
- Require the exact compiler fragment to pass the independent decoder and enter the live W^X execution path. Record the first later runtime boundary without changing another resource contract in this decision.
- Retain the WVO object and flat-linker limits at 4 MiB. This decision admits the exact compiler to the in-memory publication path; it does not claim that the same monolithic fragment can yet serialize as one WVO or link as one flat image.

## Evidence

The over-limit diagnostic now reports function bytes, non-function bytes, total functions, operations, zeroed frame slots, and the five largest functions in deterministic byte/name/index order. The exact compiler passes selection at 4,556,121 bytes and the independent fragment decoder accepts the complete ABI-20 shape. A code extent of 8 MiB plus one is rejected as `WVN3005` before structural decoding.

The portable publication core remains 7,189 bytes and now has SHA-256 `19e111490cba6f3dcae963169be82c8033d267ea505c30850502ae36fb36e13c`. Its regenerated 7,105-byte hosted bridge has SHA-256 `5ad896d92368dcadc61f358d51f5786408d9f1dc977efa5f522f99230f3ed51e`. Stage 0 and Windvale planners accept exactly 8 MiB and reject 8 MiB plus one identically.

Live W^X publication of the exact compiler now completes. Execution begins against a real source input and deterministically reaches `WVR3017`, exhausting the retained 1 MiB immutable-record arena before stdout, diagnostics, or output-file publication. That is the next measured native-compiler boundary.

Windows Development passes a zero-warning Release build, all 67 regular Seed tests, and all 25 OS tests in 67.4 seconds wall time. Windows Standard passes all 68 Seed tests, including the 153.641-second golden compiler-reproduction contract, plus all 25 OS tests in 219.6 seconds wall time; Seed in-process time is 206.326 seconds. Complete Windows/Debian qualification and the native CLI gate remain pending.

## Consequences

The exact Windvale compiler is no longer blocked by unsupported operations, frame admission, selection size, independent decoding, or executable-memory publication. Its next blocker is execution memory for immutable records.

Eight MiB is a security and resource ceiling, not a target size. Baseline compaction and a later optimizing tier remain worthwhile for startup, cache, and AOT footprint, but they are no longer prerequisites for honest in-memory compiler execution.

Native compiler AOT remains separately blocked by the existing 4 MiB WVO/object and flat-linker image limits. Those limits require their own measured decision: a larger bounded object/image contract, multiple objects, or function/data-granular publication.

No source, WVB, machine ABI, calling convention, runtime-service, or platform semantic changes. Older internal planners continue to accept all previously valid fragments and reject newly admitted larger ones explicitly.

## Reconsider when

- Measured exact-compiler growth approaches 8 MiB.
- A compact baseline encoding or register allocation materially reduces complete tool output with independently verified machine shapes.
- Native AOT evidence selects a multi-object/function-granular design instead of revised WVO and linker ceilings.
- A platform executable-memory policy cannot admit the bounded contiguous image.
- Immutable-record allocation evidence supports reclamation, region separation, or a revised arena ceiling instead of another fixed increase.
