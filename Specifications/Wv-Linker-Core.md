# Windvale linker core

## Status and purpose

`Wvˉlinkerˉcore` is the Windvale-written implementation path for Windvale Linking 1. The current first slice consumes one immutable WVO 1.0 value, validates the complete object structure in verified bytecode, and exposes deterministic section, symbol, and relocation views for later link passes. It is a parser/indexing milestone, not yet a complete linker: it does not resolve symbols, lay out an image, apply relocations, reconstruct the result, construct a map, or write an output image.

The module is compiled from `Examples/Linker/Wv-Linker-Core.wv`. Its current WVB 1.6 SHA-256 is `ac00a5b702f2a4ef185bd5f021ec2611bd8a335d1937804ceeb30f28cc1b8ded`. This object-scanner slice is cross-host qualified at `3eb331a`: the exact committed archive passed the full suite and real CLI verifier on Windows and Debian, the normalized contracts matched, and the directly retrieved modules were byte-for-byte identical.

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

The first implementation deliberately uses repeated bounded passes over immutable bytes. WVO permits at most 64 sections and 4,096 symbols; canonical ordering lets the scanner compare adjacent records and merge the local/export/import name ranges while checking cross-binding uniqueness. Section lookups rescan from the section start. This is sufficient for the current contract and avoids adding a general collection facility before resolution and layout demonstrate its exact requirements.

The runtime's balanced persistent byte representation makes later slice/replace image construction practical without exposing mutable buffers. First-read hosted file snapshots guarantee that repeated reads of one input resource observe one immutable byte value across all future link passes.

## Hosted scan shell

The current shell declares the final linker's explicit hosted capabilities so capability authorization remains visible while the implementation grows. With no arguments, `Main` runs embedded parser and view checks without reading or writing a hosted resource. With one argument, it reads one bounded object resource and emits exactly one report:

```text
object status=<status> sections=<u32> symbols=<u32> relocations=<u32> offset=<u32>
```

Valid input sends the LF-terminated report to standard output and returns `0`. Invalid input sends it to the diagnostic sink and returns `2`. Any other argument count reports `Usage: wvlink-core [object.wvo]` and returns `64`. Native resource failures remain stable runtime diagnostics.

This one-input form is a development shell. The completed linker shell will use the accepted explicit argument contract for base address, entry name, output resource, and one through 64 ordered input objects.

## Qualification boundary

The conformance test compiles and verifies the exact module, runs its no-input self-tests, scans representative WVO values covering the canonical assembler object, provider object, all section representations, symbols, both relocation kinds, and a minimal object, and compares accepted/rejected classification with the independent C# WVO oracle for deterministic one-byte mutations and bounded random byte values.

Both the Windows and Debian verifiers must also compile and inspect the module through the real CLI, prove capability refusal, run its embedded tests, accept the Windvale-written canonical assembler object, and reject a non-WVO input. The normalized host reports include the exact module digest, self-test result, and canonical hosted scan output.

Qualification of this slice proves portable object parsing and view offsets only. Phase 6 remains incomplete until the same Windvale module implements and qualifies full multi-object resolution, deterministic layout, checked relocation, independent image reconstruction, canonical map construction, and publish-after-success output behavior.
