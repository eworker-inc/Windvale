# Historical documents in Git

> Status: Current history retrieval guide
> Authority: Informative
> Last reviewed: 2026-09-26

Old verification diaries, dated dashboards and superseded plans are absent from
the working tree. Their unchanged text is retained at the Git revision below.
Current work starts with [Progress](Project/Progress.md),
[Roadmap](Project/Roadmap.md) and the [specifications](../Specifications/README.md).

Read one old document without restoring the archive:

```text
git show 928f772e9e65c1840f263cf6ac099ab1bae0df27:Documents/Project/Seed-Verification-Evidence.md
```

Substitute another path from the table. To search without loading whole files,
use a bounded Git search, then read only the matching section:

```text
git grep -n -m 5 "linker-prerequisite" 928f772e9e65c1840f263cf6ac099ab1bae0df27 -- Documents/Project/Seed-Verification-Evidence.md
```

| Removed document | Historical copy |
| --- | --- |
| Documents/Documentation-Guide-History-2026-08-31.md | [Read in Git](https://github.com/eworker-inc/Windvale/blob/928f772e9e65c1840f263cf6ac099ab1bae0df27/Documents/Documentation-Guide-History-2026-08-31.md) |
| Documents/Project/Seed-Verification-Evidence.md | [Read in Git](https://github.com/eworker-inc/Windvale/blob/928f772e9e65c1840f263cf6ac099ab1bae0df27/Documents/Project/Seed-Verification-Evidence.md) |
| Documents/Project/Windvale-Language-1.0-Migration-Evidence.md | [Read in Git](https://github.com/eworker-inc/Windvale/blob/928f772e9e65c1840f263cf6ac099ab1bae0df27/Documents/Project/Windvale-Language-1.0-Migration-Evidence.md) |
| Documents/Project/Progress-History-2026-08-31.md | [Read in Git](https://github.com/eworker-inc/Windvale/blob/928f772e9e65c1840f263cf6ac099ab1bae0df27/Documents/Project/Progress-History-2026-08-31.md) |
| Documents/Project/Roadmap-History-2026-08-31.md | [Read in Git](https://github.com/eworker-inc/Windvale/blob/928f772e9e65c1840f263cf6ac099ab1bae0df27/Documents/Project/Roadmap-History-2026-08-31.md) |
| Documents/Project/Library-Development-History-2026-09-26.md | [Read in Git](https://github.com/eworker-inc/Windvale/blob/928f772e9e65c1840f263cf6ac099ab1bae0df27/Documents/Project/Library-Development-History-2026-09-26.md) |
| Documents/Project/Verification-Throughput-History-2026-09-26.md | [Read in Git](https://github.com/eworker-inc/Windvale/blob/928f772e9e65c1840f263cf6ac099ab1bae0df27/Documents/Project/Verification-Throughput-History-2026-09-26.md) |
| Documents/Project/Post-Dotnet-Retirement-Language-And-Libraries.md | [Read in Git](https://github.com/eworker-inc/Windvale/blob/928f772e9e65c1840f263cf6ac099ab1bae0df27/Documents/Project/Post-Dotnet-Retirement-Language-And-Libraries.md) |
| Documents/Project/Windvale-Database-Proposal.md | [Read in Git](https://github.com/eworker-inc/Windvale/blob/928f772e9e65c1840f263cf6ac099ab1bae0df27/Documents/Project/Windvale-Database-Proposal.md) |
| Documents/Project/Windvale-0.2.0-Connected-Services-Release-Plan.md | [Read in Git](https://github.com/eworker-inc/Windvale/blob/928f772e9e65c1840f263cf6ac099ab1bae0df27/Documents/Project/Windvale-0.2.0-Connected-Services-Release-Plan.md) |

These are historical records, not current requirements or new verification.
Retrieve them only for a specific question; do not restore them to normal
development context. Current contracts, recovery manifests and active plans
remain with their existing owners.

## Retired migration notes and diagnostic probes

The following originals are retained at revision bcd9f517. Current architecture
and publication guidance remain in the working tree; completed chronology and
private-publication steps have been removed. The two standalone diagnostic
probes had no maintained callers or registered verification owners. Their
historical measurements remain recoverable; deleting them does not add new
runtime verification evidence.

| Original file | Historical copy |
| --- | --- |
| Documents/Architecture/Native-Execution-And-Dotnet-Retirement.md | [Read in Git](https://github.com/eworker-inc/Windvale/blob/bcd9f5172890d0eac0241b965c154915ca9f73d1/Documents/Architecture/Native-Execution-And-Dotnet-Retirement.md) |
| Documents/Project/GitHub-Publication-Runbook.md | [Read in Git](https://github.com/eworker-inc/Windvale/blob/bcd9f5172890d0eac0241b965c154915ca9f73d1/Documents/Project/GitHub-Publication-Runbook.md) |
| Tools/Verify/Probe-WebAssembly-Guest-Heap.mjs | [Read in Git](https://github.com/eworker-inc/Windvale/blob/bcd9f5172890d0eac0241b965c154915ca9f73d1/Tools/Verify/Probe-WebAssembly-Guest-Heap.mjs) |
| Tools/Verify/Probe-WebAssembly-Scalar-Dispatcher.mjs | [Read in Git](https://github.com/eworker-inc/Windvale/blob/bcd9f5172890d0eac0241b965c154915ca9f73d1/Tools/Verify/Probe-WebAssembly-Scalar-Dispatcher.mjs) |
