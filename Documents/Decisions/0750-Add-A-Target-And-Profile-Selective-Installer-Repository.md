# Decision 0750: Add a target- and profile-selective installer repository

## Status

Accepted on 2026-08-17. The producer, verifier, selector, and focused Windows
evidence are implemented; independent Linux execution remains pending.

## Context

Decision 0749 reduced each successor target archive from about 38.8 MiB to
about 4.6 MiB, but an archive remains an all-tools unit. The intended release
repository must retain both host targets while allowing an installer to acquire
only the machine target and only the tools requested for that installation.

A component repository also introduces a trust question. Signing every blob
would multiply release ceremony records, while signing only a mutable catalog
would not bind the selected bytes.

## Decision

- Define canonical Installer Repository 1 in
  [`Specifications/Windvale-Installer-Repository.md`](../../Specifications/Windvale-Installer-Repository.md).
- Publish compressed files as immutable content-addressed objects and bind both
  compressed and expanded identities in one canonical index.
- Reuse the deterministic DEFLATE/gzip implementation from successor installer
  production rather than maintain two compression definitions.
- Classify every installer file into a named component and define bytewise
  ordered `runtime`, `developer`, `publisher`, and `full` profiles.
- Select exactly one of `windows-x64` or `linux-x64` before acquiring blobs.
- Make the index ready to appear as one `repository|all` artifact in a future
  signed release envelope. The release signature binds the index; the index
  binds every blob.
- Keep all index, object, selection, path, inventory, and decompression work
  explicitly bounded and reject identity or canonicalization drift.

## Consequences

For the current `0.2.0-dev.1` inputs, a Windows runtime selection downloads
550,475 bytes and a Linux runtime selection downloads 548,956 bytes. Developer
selections are about 4.20 MiB, publisher selections about 0.74 MiB, and full
selections about 4.65 MiB. The repository stores 15 unique blobs totaling
9,290,710 bytes plus a 3,546-byte index.

The repository separates available artifacts from selected installation state.
An installer can admit the small index, choose a target/profile, and acquire
only named blobs. Package installation, generation publication, and activation
remain independent transactions.

The checked-in development identity is not signed and no network client is
introduced by this decision. Independent Linux evidence is still required
before paired-host qualification.

## Reconsideration triggers

Reconsider the component boundaries when measured installations commonly need
different tool combinations, and reconsider the blob boundary or compression
profile when shared content materially improves repository size or update
traffic. Reconsider the signed-subject integration when Release Envelope 2 or
network release discovery is implemented.
