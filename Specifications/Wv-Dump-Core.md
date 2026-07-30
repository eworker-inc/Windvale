# Windvale `wvdump` core

## Status and purpose

`Examples/Foundation/Wv-Dump-Core.wv` is a useful Windvale-written inspector for canonical WVB 1.6 modules. Its pure functions validate immutable bytes, decode all seven payload kinds, walk every function instruction, and return structured failures without relying on C# parsing. Its small hosted shell obtains one filename through explicit capabilities and emits the deterministic line report defined by [Wv-Dump-Report.md](Wv-Dump-Report.md).

The module declares `console.write_line`, `diagnostic.write_line`, `file.read_bytes`, `process.argument`, and `process.argument_count`. With no arguments, `Main` runs embedded deterministic fixtures without reading a file. With one argument, it validates before writing any normal output, reports a valid module to standard output, and reports malformed input to the diagnostic sink with result `2`. Other argument counts print usage and return `64`. Native file failures remain stable host-boundary runtime errors.

## Inspection boundaries

The first pure boundary validates the file header and seven section envelopes:

```text
Inspectˉwvbˉenvelope(Input: bytes) -> Wvbˉinspection
```

The second pure boundary runs only after the envelope is valid and validates every payload:

```text
Inspectˉwvbˉpayloads(Input: bytes) -> Wvbˉpayloadˉinspection
```

`Wvbˉinspection` reports a nominal status, completed section count, and failure offset. `Wvbˉpayloadˉinspection` reports a nominal status, failure offset, declarations reached before failure, and instructions reached before failure. Range checks prove `Offset <= End` before using `Length <= End - Offset`, so hostile unsigned lengths become data diagnostics rather than arithmetic wraparound or runtime bounds traps.

## Status values

| Member | Value | Meaning |
| --- | ---: | --- |
| `Valid` | 0 | The inspected boundary is structurally valid. |
| `Shortˉheader` | 1 | The input is shorter than the 12-byte header. |
| `Badˉmagic` | 2 | The `WVB1` magic is invalid. |
| `Badˉversion` | 3 | The version is not WVB 1.6. |
| `Badˉsectionˉcount` | 4 | The section count is not seven. |
| `Outˉofˉbounds` | 5 | A required range is outside its enclosing input or payload. |
| `Wrongˉsectionˉkind` | 6 | A canonical section position contains another kind. |
| `Unsupportedˉsectionˉheader` | 7 | Section flags or reserved bits are nonzero. |
| `Trailingˉbytes` | 8 | Bytes remain after the seventh section. |
| `Invalidˉpayload` | 9 | A structurally meaningful field violates its payload contract. |
| `Invalidˉutf8` | 10 | A length-delimited string is not strict UTF-8. |
| `Limitˉexceeded` | 11 | A declared count, length, stack, or code field exceeds a Seed bound. |
| `Unknownˉvalueˉtype` | 12 | A value shape tag is invalid in its position. |
| `Unknownˉdataˉtype` | 13 | A data declaration uses an unknown representation tag. |
| `Unknownˉnominalˉtype` | 14 | A Types entry uses an unknown kind tag. |
| `Unknownˉexportˉkind` | 15 | An export is not a function export. |
| `Unknownˉopcode` | 16 | Function code contains an opcode outside WVB 1.6. |
| `Truncatedˉinstruction` | 17 | A known instruction lacks its complete encoded operand. |
| `Invalidˉinstruction` | 18 | A structurally invalid instruction, such as a non-Boolean Boolean constant, was found. |

## Payload validation

The Windvale decoder validates:

- Module profile and strict UTF-8 name.
- Capability count, names, parameter counts, primitive parameter shapes, and return shapes.
- Text, immutable `i32` array, and immutable byte data representations and their independent bounds.
- Function names, parameter/result/local shapes, combined local-slot bounds, maximum stack bounds, contiguous code ranges, and complete Code-section coverage.
- Every instruction opcode, operand width, Boolean operand, per-function code size, and instruction count.
- Export names, function kind, and target range.
- Record and enum tags, names, nonempty field/member counts, field shapes, member names, and signed values.
- Exact consumption of every non-Code payload.

This decoder is deliberately a structural inspector, not an alternate semantic verifier. The mandatory C# verifier still owns canonical declaration ordering, identifier grammar, recognized capability signatures, nominal-reference identity, stack typing, branch boundaries, reachable control flow, exact maximum-stack calculation, and all instruction reference checks. Keeping those responsibilities explicit avoids two subtly different execution gates.

## Qualification fixtures

The no-argument self-test covers a valid minimal WVB 1.6 module plus short header, bad magic, bad version, bad section count, wrong kind, nonzero flags, hostile `0xFFFFFFFF` length, truncation, and trailing bytes. The conformance suite additionally passes malformed payload counts and an unknown opcode through the hosted shell and requires a diagnostic result rather than a runtime escape.

The cross-host contract compiles a real `Sumˉdata` module, passes its exact bytes through the native Windows or Debian file adapter, and compares the complete normalized Windvale-generated report. This proves that declaration decoding, signed and unsigned operands, safe name quoting, and instruction walking agree across hosts.

## Next boundary

With useful `wvdump` complete, deterministic byte construction and the WVO 1.0 object foundation are the next qualified boundary. The following tool milestone is an assembler that turns a small source instruction grammar into canonical sections, symbols, and relocations, followed by a linker that owns layout and fixup application.
