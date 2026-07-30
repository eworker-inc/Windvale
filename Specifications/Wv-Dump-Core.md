# Windvale `wvdump` core

## Status and purpose

This specification describes the implemented first phase of a Windvale-written `.wvb` inspector. The core proves that portable Windvale source can safely walk its own module envelope. It does not yet decode declaration payloads, disassemble instructions, read files, or format a human-readable report.

The implementation is `Examples/Foundation/Wv-Dump-Core.wv`. Its portable inspection boundary is:

```text
Inspectˉwvbˉenvelope(Input: bytes) -> i32
```

## Status values

| Value | Meaning |
| --- | --- |
| `0` | The complete WVB 1.1 envelope is canonical. |
| `1` | The input is shorter than the 12-byte header. |
| `2` | The `WVB1` magic is invalid. |
| `3` | The major or minor version is not `1.1`. |
| `4` | The section count is not six. |
| `5` | A section envelope or payload range is truncated or outside the input. |
| `6` | A section kind is not the expected canonical kind at its position. |
| `7` | Section flags or reserved bits are nonzero. |
| `8` | Bytes remain after the sixth section payload. |

These numeric statuses are a deliberate bootstrap limit. A later language slice should replace them with a structured result while preserving their deterministic distinctions.

## Validation algorithm

The core validates the 12-byte header, then walks exactly six section envelopes in canonical order. Before every read, it proves that the requested range fits inside `Bytesˉlength(Input)`. It validates the kind, flags, reserved field, and payload length before advancing to the next section.

Range validation uses `Length <= Total - Offset` only after proving `Offset <= Total`. An untrusted payload length such as `0xFFFFFFFF` therefore produces status `5` without unsigned wraparound or a runtime bounds trap. After the sixth payload, the cursor must equal the input length exactly.

## Qualification fixtures

The example embeds:

- A valid minimal WVB 1.1 module named `A`, independently accepted by the C# reference decoder and verifier.
- Short-header, bad-magic, bad-version, and bad-section-count inputs.
- Wrong-kind and nonzero-flags inputs.
- A hostile maximum `u32` payload length.
- A truncated final payload and a trailing-byte input.

`Main` requires every fixture to produce its specified status and returns zero only when all cases pass.

## Next boundary

The next inspector slice should introduce the smallest structured section descriptor and result/error types needed to return useful information. File input and console presentation remain separate hosted capabilities so the core stays portable across Windows, Linux, and Windvale OS.
