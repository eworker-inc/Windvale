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

Fragment chunk count is decimal `1` through `8`; every nonfinal fragment chunk
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
| Source-geometry WVB | 17,802 | `22549f1e50084b3cf20113bee6c30c3df9c4f91aad58b0a3ebe247d02a9e4a28` |
| Windows application | 198,656 | `209d77bd3dc10ccaec33bb0ee5351d0f4a569421ba938a26bbfc8e54d9dea996` |
| Linux application | 200,704 | `0457b23abbe871314eba3a91f992b1a479aa6b252f01c82a8be362670dce0f17` |

The focused current-host contract verifies exact geometry, public CLI routing,
native execution without CLR loading, failure preservation, and native-front-
door reproduction. Package wiring remains deletion-bound Stage 0 evidence.
