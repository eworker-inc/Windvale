# Windvale linker core

## Status and purpose

`Wvˉlinkerˉcore` is the complete Windvale-written implementation of Windvale Linking 1. It validates complete immutable WVO 1.0 values in verified bytecode, exposes deterministic object views, resolves multi-object symbols, computes deterministic placements and addresses, constructs and relocates the flat image, independently reconstructs every result byte, constructs canonical map version 1, and publishes the image once only after all deterministic work succeeds.

The module is compiled from `Examples/Linker/Wv-Linker-Core.wv`. The object-scanner slice was cross-host qualified at `3eb331a`; resolution/layout at `709ccb3`; immutable image construction plus checked relocation at `ec9c980`; independent complete-image reconstruction at `d8008e3`; and canonical map construction plus publish-after-success output at `40ac57d`. That complete pre-Foundation WVB 1.6 SHA-256 is `8d3cb567f6985077b3ad487627bf77a20326b4bc02bcab8d938354f48d339cfd`. The current module imports `Foundationˉmachineˉcontracts` and is cross-host requalified at `d46af86`. Its composed WVB is 109,419 bytes with SHA-256 `ffedbf10782850c3cd693c4d8dc2b88b67b33a19714a0d8a489adb9fb728c96e`; its exact 24-byte image and 1,721-byte map remain unchanged.

## Object boundary

`Inspectˉobject(Input: bytes) -> Wvoˉscan` checks the accepted WVO contract without calling the C# object model or decoding through a host service. Its ordered checks cover:

1. the exact 24-byte minimum header, `WVO1` magic, WVO 1.0 version, x86-64 architecture, and zero reserved flags;
2. section, symbol, relocation, data, and memory limits;
3. bounded machine names, strict UTF-8, and the ASCII machine-name grammar;
4. section kinds, power-of-two alignment through 4,096, materialized versus zero-fill size rules, canonical kind/name order, and global section-name uniqueness;
5. symbol binding and kind values, import sentinel fields, definition ranges, code/data ownership, canonical binding/name order, and global symbol-name uniqueness across binding ranges;
6. relocation kinds, source and target indices, non-overlapping canonical order, four-byte source ranges, and zero placeholders; and
7. exact consumption with no trailing bytes.

A successful `Wvoˉscan` records the counts and the exact section, symbol, and relocation region offsets. `Findˉsection`, `Findˉsymbol`, and `Findˉrelocation` rescan bounded records into immutable views. They do not retain host objects, mutable cursors, native paths, or hidden collections.

## Bootstrap algorithm

The initial acceptance implementation deliberately uses bounded passes over immutable bytes. WVO permits at most 64 sections and 4,096 symbols; canonical ordering lets the scanner compare adjacent records and merge the local/export/import name ranges while checking cross-binding uniqueness. After `Inspectˉlinkˉinputs` has accepted every exact first-read snapshot, later passes derive `Acceptedˉobjectˉview` offsets and counts from those same values instead of rerunning hostile-input validation. These views cannot accept a different value and do not replace the complete WVO boundary.

The runtime's balanced persistent byte representation makes later slice/replace image construction practical without exposing mutable buffers. First-read hosted file snapshots guarantee that repeated reads of one input resource observe one immutable byte value across all future link passes.

## Multi-object resolution and layout

The full analysis request accepts one through 64 objects in explicit argument order. It validates every object and the link-wide limits of 256 sections, 16,384 symbols, and 65,536 relocations before resolution. Duplicate exports are detected by merge-walking pairs of canonical export ranges. Each canonical import range is then resolved against export ranges in input order; missing names produce `WVL1005` and kind disagreement produces `WVL1006`. The requested entry must resolve to an exported function.

Layout walks section kind `code`, read-only data, writable data, and zero-fill, then input index and source section index. Alignment applies to the actual address, not merely the image offset. The implementation derives padding from the low byte for alignments through 256 and the low 16 bits for alignments through 4,096, then uses checked `u32` and image-limit arithmetic. A later pass recomputes placements for every non-import symbol, rejects an address beyond `u32`, and captures the selected entry address.

No host collection, object decoder, resolver, or layout callback participates. Repeated reads use the same immutable resource snapshots. The current analysis returns counts, image length, and entry address but deliberately returns no image bytes.

## Image construction and relocation

`Buildˉunrelocatedˉimage` repeats final placement order and constructs one immutable byte value. Alignment gaps and zero-fill contributions append exact zero bytes; materialized contributions append zero-copy slices of the original object snapshots. The measured image length and constructed byte length must both equal the qualified layout result.

`Applyˉrelocations` walks input and source relocation order. It recomputes the source placement, resolves the local/export/import target to a defined-symbol address, evaluates `absolute-u32` or `relative-i32` using explicit signed magnitudes, rejects overflow as `WVL1009` or `WVL1010`, and replaces exactly four bytes through persistent prefix/value/suffix concatenation. The input objects and unrelocated value remain immutable.

Successful analysis adds `image sha256=<lowercase-hex>` to the report. This digest equals Stage 0 on both qualified hosts, but the verifier uses complete byte equality rather than treating the digest as its acceptance predicate.

## Independent reconstruction

`Verifyˉlinkˉimage` builds a second complete value through verifier-owned algorithms. Alignment advances an actual address until an independent predicate accepts it. Provider lookup scans all symbols rather than using production export-range lookup. Relocations are applied in reverse input and reverse relocation order with separate signed-magnitude functions. The verifier finally compares the complete candidate and reconstruction byte by byte.

Any placement, provider, address, arithmetic, length, or byte disagreement becomes `WVL1011`, clears the candidate, and returns through diagnostics. The embedded suite injects a one-byte mismatch at the final acceptance boundary and requires `WVL1011` with an empty result. The hosted writer remains unreachable until canonical map construction also succeeds.

## Canonical map and publication

`Buildˉcanonicalˉmap` emits canonical map version 1 directly as immutable ASCII/LF bytes. Input digests use explicit input order; sections use final layout order; definitions, imports, and relocations use input/source order. Names come from the accepted WVO bytes, decimal values use invariant Windvale formatting, and the image identity is calculated from the independently accepted candidate.

`Definitionˉmapˉminimumˉexceedsˉlimit` rejects a definition set whose provable minimum record size already exceeds 1 MiB. `Appendˉmapˉline` then checks the exact cumulative byte length before every append. Both paths return `WVL1012`, discard map bytes, and leave the writer unreachable. The lower-bound optimization cannot reject a map that might fit because it counts only mandatory literal bytes, minimum-width fields, LF, and the exact accepted name lengths.

After the complete map succeeds, `Runˉlinkˉanalysis` invokes `file.write_bytes` exactly once with the accepted image and only then sends the already built map to `console.write`. Deterministic request, object, resolution, layout, relocation, reconstruction, or map failure invokes no writer. A native write failure is reported through the hosted-resource boundary and emits no success map.

## Hosted scan shell

The current shell declares the final linker's explicit hosted capabilities so capability authorization remains visible while the implementation grows. With no arguments, `Main` runs embedded parser and view checks without reading or writing a hosted resource. With one argument, it reads one bounded object resource and emits exactly one report:

```text
object status=<status> sections=<u32> symbols=<u32> relocations=<u32> offset=<u32>
```

Valid input sends the LF-terminated report to standard output and returns `0`. Invalid input sends it to the diagnostic sink and returns `2`. Any other argument count reports `Usage: wvlink-core [object.wvo]` and returns `64`. Native resource failures remain stable runtime diagnostics.

The one-input form remains a focused object-inspection shell. The multi-object linker uses the accepted final argument shape:

```text
wvlink-core <base-address> <entry> <output.bin> <input.wvo>...
```

On success it writes the exact flat image to `<output.bin>`, emits the canonical map to standard output, and returns `0`. A deterministic WVL rejection emits `link status=<status> inputs=<u32> sections=<u32> symbols=<u32> relocations=<u32> image-bytes=<u32> entry-address=<u32> input=<u32>` through diagnostics, returns `2`, and does not create or modify the output.

## Qualification boundary

The conformance test compiles and verifies the exact module, runs its no-input self-tests, scans representative WVO values covering the canonical assembler object, provider object, all section representations, symbols, both relocation kinds, and a minimal object, and compares accepted/rejected classification with the independent C# WVO oracle for deterministic one-byte mutations and bounded random byte values.

Both the Windows and Debian verifiers must also compile and inspect the module through the real CLI, prove capability refusal, run its embedded tests, accept the Windvale-written canonical assembler object, and reject a non-WVO input. The normalized host reports include the exact module digest, self-test result, and canonical hosted scan output.

The qualified suite additionally compares canonical and reversed input order, aligned and unaligned bases, all section representations, aggregate overflow, malformed objects, duplicate exports, missing imports, kind mismatch, missing entry, invalid requests, layout overflow, exact image and map bytes, snapshot read counts, and publish-after-success behavior with the Stage 0 oracle. A 4 MiB code-plus-BSS image exercises the accepted maximum image size, complete byte comparison, map construction, and one hosted write under an explicit 200,000,000-instruction ceiling. Four valid objects containing the aggregate maximum 16,384 short-name definitions prove `WVL1012` with no writer under the same ceiling.

The Windows and Debian verifiers compare real Windvale/Stage 0 image and map files byte for byte, prove rejected links preserve an existing image, and check a missing output parent. The exact `40ac57d` archive passed both complete verifiers and its normalized reports agree, completing roadmap gate 6G and Phase 6. The exact `d46af86` archive then qualified the first Foundation extraction: both hosts produced the same composed linker WVB while preserving every link output and failure contract.
