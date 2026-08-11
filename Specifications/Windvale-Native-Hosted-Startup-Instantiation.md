# Windvale native hosted-startup instantiation

## Status and scope

`WVSI 1` and `WVSD 1` are linker-private contracts for instantiating the
canonical Windows and Linux hosted-tool startup objects. The canonical machine
code remains the existing WVA source and its exact WVO output. Windvale
validates the bounded startup-object profile and applies every `relative-i32`
relocation; it does not contain a second copy of the startup instructions.

This version accepts one x86-64 `.text` section, at most 4,096 code bytes, 128
symbols, 256 relocations, and 65,536 object bytes. The current objects are the
Windows 1,554-byte startup with 40 symbols and 59 relocations and the Linux
809-byte startup with 26 symbols and 32 relocations.

## Request envelope: `WVSI 1`

The request begins with a 40-byte header, followed by one `u32` absolute target
address for every canonical WVO relocation, followed by the complete WVO.

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVSI`, `0x49535657` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | Exact request length |
| 12 | 4 | startup address | Exactly `4,096` in this hosted profile |
| 16 | 4 | code bytes | `4` through `4,096` |
| 20 | 4 | symbol count | `1` through `128` |
| 24 | 4 | relocation count | `1` through `256` |
| 28 | 4 | target count | Equal to relocation count |
| 32 | 4 | WVO bytes | `1` through `65,536`; consumes the request tail |
| 36 | 4 | target policy | `0` requires every target to be nonzero; `1` admits the exact profile-5 optional-service target shape |

Target addresses appear in canonical relocation order. They include targets
for imports and locally defined symbols. The hosted-container planner defined
by Decision 0385 now derives this list from the admitted runtime metadata and
complete target layout. The standalone
[hosted-container startup producer](Windvale-Native-Hosted-Container-Startup.md)
now projects that list and the exact retained WVO into this request without a
managed bridge.

Target policy `1` is bounded to the retained inspector startup shapes: Windows
uses 50 targets with indices 30 and 32 zero, while Linux uses 29 targets with
indices 19 and 21 zero. Every other target remains nonzero. This represents
the two services intentionally omitted by WVB-runner profile 5; all other
profiles retain policy `0`.

## Admitted startup-object profile

Windvale requires canonical WVO 1.0, x86-64, no flags, exact declared counts,
one materialized code section named `.text`, alignment 16, and exact code and
memory sizes. Symbol records must be bounded, canonically ordered, uniquely
named by that ordering, and valid for either the code section or an import.
Every relocation must be a non-overlapping, ordered `relative-i32` record in
section zero, refer to an existing symbol, carry addend `-4`, and identify a
four-byte zero placeholder inside the code. Trailing bytes are rejected.

This is a deliberately narrower consumer of WVO 1.0, not a replacement for
the general object verifier. It owns only the exact hosted-startup shape that
the outer-container constructor needs.

## Response envelope: `WVSD 1`

The response begins with a 32-byte header. Successful responses append exactly
the relocated startup code.

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVSD`, `0x44535657` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | `32` on failure; `32 + code bytes` on success |
| 12 | 4 | status | Status below |
| 16 | 4 | failure offset | Relevant request byte; request length on success |
| 20 | 4 | code bytes | Zero on failure; exact code length on success |
| 24 | 8 | reserved | Zero |

| Value | Meaning |
| ---: | --- |
| 0 | Valid; relocated startup bytes follow |
| 1 | Actual or declared request size is invalid |
| 2 | Request magic differs |
| 3 | Request version differs |
| 4 | Header, target count, target address, or reserved data is invalid |
| 5 | The WVO does not satisfy the hosted-startup profile |
| 6 | A relative relocation result is outside signed 32-bit range |

## Windvale owner and retained artifacts

The reusable portable core exports the bounded constructor, and a focused
bridge exports service-free `Main(bytes) -> bytes`. The normal linker embeds
only the exact WVNF plus the two exact WVO resources; the WVB is retained for
reproducibility and recovery evidence.

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Startup-instantiation WVB | 21,143 | `933864be78b28394b9fc8e495b5ac872311ebca2a624db6e6731cdb8b399d309` |
| Startup-instantiation WVNF | 193,891 | `ad1c049bdf77cb410b95cb638aa401874cca1a21b496e36ecab32ceef1539ffd` |
| Windows hosted startup WVO | 4,398 | `dbf9314d43b47ffc5d3cdeef3c439456b295ac5c3a1cda0b1faaff6227910161` |
| Linux hosted startup WVO | 2,454 | `1b8c08308d3f7320b741ae86022400ced6748352314b7f27954ec1c5a7345946` |

The former C# template patchers remain under `Buildˉstage0` only for
differential and recovery evidence. C# no longer projects target addresses.
The standalone hosted-container startup producer now owns exact WVO identity,
request projection, native invocation, and response admission. The retained
managed relay remains deletion-bound until the complete process pipeline is
composed and promoted.

[Decision 0515](../Documents/Decisions/0515-Native-Hosted-Construction-Build-And-Inspection-Transfer.md)
moves exact startup-instantiation Project 1 construction and public-surface
inspection into the paired native helper. The broad scripts still compare the
native-built WVB with the retained WVB, WVNF, and both target WVO identities.
