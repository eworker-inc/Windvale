# Windvale `wvdump` core

## Status and purpose

This specification describes the implemented envelope core of a Windvale-written `.wvb` inspector, its structured diagnostic slice, and its first hosted shell. The pure inspection functions safely walk supplied immutable bytes and format one deterministic summary. The shell now receives an explicit filename argument, reads bounded bytes through a declared capability, and separates normal output from diagnostics. It does not yet decode declaration payloads or disassemble instructions.

The implementation is `Examples/Foundation/Wv-Dump-Core.wv`. Its portable inspection boundary is:

```text
enum Wvbˉstatus {
    Valid = 0;
    Shortˉheader = 1;
    Badˉmagic = 2;
    Badˉversion = 3;
    Badˉsectionˉcount = 4;
    Outˉofˉbounds = 5;
    Wrongˉsectionˉkind = 6;
    Unsupportedˉsectionˉheader = 7;
    Trailingˉbytes = 8;
}

record Wvbˉinspection {
    Status: Wvbˉstatus;
    Sectionsˉseen: u32;
    Failureˉoffset: u32;
}

Inspectˉwvbˉenvelope(Input: bytes) -> Wvbˉinspection
```

`Status` is a nominal enum rather than a magic integer. `Sectionsˉseen` identifies how many canonical envelopes completed. `Failureˉoffset` identifies the rejected byte offset; on success it is the terminal cursor immediately after the seventh payload. `Describeˉinspection` formats the enum name, section count, and offset with bounded portable intrinsics.

The module uses the hosted profile and declares `console.write_line`, `diagnostic.write_line`, `file.read_bytes`, `process.argument`, and `process.argument_count`. With no program arguments, `Main` runs the embedded deterministic qualification fixtures. With one argument, it reads that hosted resource and prints a valid summary to normal output or an invalid summary to diagnostics. Other argument counts print usage to diagnostics. The envelope parser itself calls no hosted capability.

## Status values

| Member | Value | Meaning |
| --- | --- | --- |
| `Valid` | `0` | The complete WVB 1.3 envelope is canonical. |
| `Shortˉheader` | `1` | The input is shorter than the 12-byte header. |
| `Badˉmagic` | `2` | The `WVB1` magic is invalid. |
| `Badˉversion` | `3` | The major or minor version is not `1.3`. |
| `Badˉsectionˉcount` | `4` | The section count is not seven. |
| `Outˉofˉbounds` | `5` | A section envelope or payload range is truncated or outside the input. |
| `Wrongˉsectionˉkind` | `6` | A section kind is not the expected canonical kind at its position. |
| `Unsupportedˉsectionˉheader` | `7` | Section flags or reserved bits are nonzero. |
| `Trailingˉbytes` | `8` | Bytes remain after the seventh section payload. |

## Validation algorithm

The core validates the 12-byte header, then walks exactly seven section envelopes in canonical order. `Readˉsection` returns an immutable `Wvbˉsection` descriptor containing kind, flags, reserved value, envelope offset, payload offset, and payload length. Before every read, the core proves that the requested range fits inside `Bytesˉlength(Input)`. It validates the descriptor before advancing to the next section.

Range validation uses `Length <= Total - Offset` only after proving `Offset <= Total`. An untrusted payload length such as `0xFFFFFFFF` therefore produces `Outˉofˉbounds` without unsigned wraparound or a runtime bounds trap. After the seventh payload, the cursor must equal the input length exactly.

## Qualification fixtures

The example embeds:

- A valid minimal WVB 1.3 module named `A`, independently accepted by the C# reference decoder and verifier.
- Short-header, bad-magic, bad-version, and bad-section-count inputs.
- Wrong-kind and nonzero-flags inputs.
- A hostile maximum `u32` payload length.
- A truncated final payload and a trailing-byte input.

`Main` requires every fixture to produce its specified nominal status and also checks representative section counts and failure offsets. It executes the summary formatter and returns zero only when all cases pass.

## Next boundary

The next inspector boundary is useful payload decoding: module identity, capability/data/type/function/export declarations, instruction streams, and deterministic machine-readable reports. Portable decoding should remain independent so the same logic can run on Windows, Linux, and Windvale OS.
