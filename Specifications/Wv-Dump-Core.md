# Windvale `wvdump` core

## Status and purpose

This specification describes the implemented envelope core of a Windvale-written `.wvb` inspector and its first structured-result slice. The core proves that portable Windvale source can safely walk its own module envelope and return bounded diagnostic context. It does not yet decode declaration payloads, disassemble instructions, read files, or format a human-readable report.

The implementation is `Examples/Foundation/Wv-Dump-Core.wv`. Its portable inspection boundary is:

```text
record Wvbˉinspection {
    Status: i32;
    Sectionsˉseen: u32;
    Failureˉoffset: u32;
}

Inspectˉwvbˉenvelope(Input: bytes) -> Wvbˉinspection
```

`Status` preserves the original stable numeric distinctions until Seed has an enum contract. `Sectionsˉseen` identifies how many canonical envelopes completed. `Failureˉoffset` identifies the rejected byte offset; on success it is the terminal cursor immediately after the seventh payload.

## Status values

| Value | Meaning |
| --- | --- |
| `0` | The complete WVB 1.2 envelope is canonical. |
| `1` | The input is shorter than the 12-byte header. |
| `2` | The `WVB1` magic is invalid. |
| `3` | The major or minor version is not `1.2`. |
| `4` | The section count is not seven. |
| `5` | A section envelope or payload range is truncated or outside the input. |
| `6` | A section kind is not the expected canonical kind at its position. |
| `7` | Section flags or reserved bits are nonzero. |
| `8` | Bytes remain after the seventh section payload. |

The numeric status field is a deliberate bootstrap limit. The result itself is now structured; a later enum slice can replace the integer names without losing section count or failure position.

## Validation algorithm

The core validates the 12-byte header, then walks exactly seven section envelopes in canonical order. `Readˉsection` returns an immutable `Wvbˉsection` descriptor containing kind, flags, reserved value, envelope offset, payload offset, and payload length. Before every read, the core proves that the requested range fits inside `Bytesˉlength(Input)`. It validates the descriptor before advancing to the next section.

Range validation uses `Length <= Total - Offset` only after proving `Offset <= Total`. An untrusted payload length such as `0xFFFFFFFF` therefore produces status `5` without unsigned wraparound or a runtime bounds trap. After the seventh payload, the cursor must equal the input length exactly.

## Qualification fixtures

The example embeds:

- A valid minimal WVB 1.2 module named `A`, independently accepted by the C# reference decoder and verifier.
- Short-header, bad-magic, bad-version, and bad-section-count inputs.
- Wrong-kind and nonzero-flags inputs.
- A hostile maximum `u32` payload length.
- A truncated final payload and a trailing-byte input.

`Main` requires every fixture to produce its specified status and also checks representative section counts and failure offsets. It returns zero only when all cases pass.

## Next boundary

The next inspector slice should add the smallest enum/status naming and bounded formatting needed to turn these records into a useful report. File input and console presentation remain separate hosted capabilities so the core stays portable across Windows, Linux, and Windvale OS.
