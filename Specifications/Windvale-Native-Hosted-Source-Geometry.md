# Windvale native hosted source-geometry producer

## Status and scope

This command constructs canonical `WVSG 1` geometry directly from the real
bounded fragment and hosted-service resources. It removes managed fixture or
script ownership of resource lengths, logical ordering, and publication
placements before native publication-request construction.

## Command contract

```text
wvhostsourcegeometry <chunk-prefix> <fragment-chunks> <sources.wvsg>
```

Fragment chunk count is decimal `1` through `16`; every nonfinal fragment chunk
is exactly 4 MiB. Ten following nonempty chunks contain services `1` through
`8`, then `11` and `12`. All resources are named `<prefix>.chunk-N` and are at
most 4 MiB. The command reads each once, constructs eleven logical regions,
derives canonical 16-byte service placements, self-admits the completed
manifest, and only then writes it. Resource/output aliases are rejected.

Rejection returns 2 and preserves existing output; wrong argument count
returns 64. The six declared capabilities are console and diagnostic output,
file read/write, and process argument/count.

## Exact identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Source-geometry WVB | 17,802 | `47c322f575b73ee9278b9bf111d4c4f2424eb8554de55424842d715dd4b08a3f` |
| Windows application | 198,656 | `b0563aee44931fd6226dc78f2d4a94b5092d38ab08e6b7a4ac54de2ffaae5f61` |
| Linux application | 200,704 | `30af75cf761837146b65a490d51825abcafc9276bd624ef77325c954f265c1b8` |

The focused current-host contract verifies exact geometry, public CLI routing,
native execution without CLR loading, failure preservation, and native-front-
door reproduction. Package wiring remains deletion-bound Stage 0 evidence.
