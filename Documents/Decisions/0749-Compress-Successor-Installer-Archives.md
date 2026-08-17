# Decision 0749: Compress successor installer archives

## Status

Accepted on 2026-08-17.

## Context

The two target-specific installer payloads each contain about 38.8 MiB of
checked-in native tools. Exact whole-file and fixed-block deduplication finds
little reusable content inside those payloads, but ordinary lossless
compression reduces either target to about 4.6 MiB. The stable `0.1.0`
archives and Release Envelope 1 already have published exact identities and
must not be rewritten.

Future releases will live in an immutable release repository. A bootstrap
installer will select the machine target and later may select named components;
it must not download every host target merely because the repository retains
them.

## Decision

- Preserve both stable `0.1.0` archives byte for byte.
- Advance the unsigned development input to `0.2.0-dev.1`.
- Add `zip-deflate-1` and `tar-gzip-deflate-1` producer profiles. Both use raw
  zlib DEFLATE with level 6, memory level 8, the default strategy, and a 32 KiB
  window. Windvale writes the deterministic ZIP, tar, and gzip containers.
- Pin the exact compressed archive length, SHA-256, payload-manifest SHA-256,
  and target-derived installation generation in distribution metadata.
- Keep one archive per target. The release repository retains both archives;
  the future bootstrap installer selects and acquires only its admitted target.
- Treat component selection, signed network discovery, resumable transfer,
  rollback, and repository garbage collection as later contracts rather than
  silently adding them to Installer 1.

## Consequences

The Windows development archive falls from 38,824,208 to 4,659,946 bytes and
the Linux development archive falls from 38,835,111 to 4,653,399 bytes: an
88.01% reduction across the pair. Extraction still uses ordinary ZIP and gzip
readers, and installation continues to verify every expanded payload byte.

Level 6 was selected after the two target tool sets measured 9,285,585 bytes
when compressed file by file, only 110,177 bytes larger than level 9 while
reducing that compression probe from 6,887.639 ms to 1,359.894 ms on the
development host.

The compressed stream may vary if a future zlib producer changes its encoded
output. Distribution metadata and the deterministic owner fail closed on that
drift; changing the producer requires a newly pinned successor artifact rather
than rewriting an existing release.

## Reconsideration triggers

Reconsider the compression profile when Windvale owns a comparably effective
canonical compressor, when a measured alternative materially improves size or
decompression cost, or when component-level acquisition makes a different
blob boundary preferable.
