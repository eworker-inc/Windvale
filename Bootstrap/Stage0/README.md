# Stage 0 recovery archive

The managed Stage 0 implementation is intentionally absent from `main`.
Its authoritative, immutable recovery state is the GitHub release
[`stage0-recovery-e5a1a7473c57`](https://github.com/eworker-inc/Windvale/releases/tag/stage0-recovery-e5a1a7473c57).

| Identity | Value |
| --- | --- |
| Qualified commit | `e5a1a7473c57935c5dfcf09b78b18c3c099e70ef` |
| Qualified tree | `9950150f14cd4864b06c853ab6a716fa6e04495a` |
| Source bundle SHA-256 | `1830bf95b583267b69229125edb83521733a36f27a4d49fe371534734bcc0892` |
| Supplemental checksum file SHA-256 | `de18793e13fa4cf429070739708e2e3bebc4cebbd5eacde5832dca9781928267` |
| Supported recovery hosts | Windows x64 and Linux x64 |

The release contains the complete Git history, exact source and artifact
inventories, dependency and license inventories, recovery runbook, host reports,
cross-host report, and checksums. An independently held E-Worker copy matched all
13 published assets when Decision 0526 was qualified.

## Restore for a recovery investigation

1. Download the release assets into a new, empty recovery workspace. Do not copy
   the managed tree into a normal Windvale checkout.
2. Verify the downloaded checksum manifest itself against the supplemental
   checksum shown above.
3. Verify every selected asset against the published manifest before opening or
   executing it.
4. Follow the runbook contained in that release to reconstruct the exact commit
   on one of the supported hosts.
5. Keep findings and any experimental correction on a separate recovery branch
   or repository. Returning managed source or a direct `dotnet` entry point to
   `main` requires a new decision naming the failed recovery contract.

Decision 0526 owns the original qualification evidence. Decision 0558 owns the
removal of the frozen live copy from `main`. Git history remains useful for
inspection, but only the immutable release and its checksums are the supported
recovery input.
