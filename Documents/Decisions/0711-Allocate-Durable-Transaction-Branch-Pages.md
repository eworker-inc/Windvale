# Decision 0711: Allocate durable transaction branch pages

- Date: 2026-08-16
- Status: Implemented candidate with focused Windows native evidence
- Advances: [Decision 0707](0707-Group-Transaction-Replacements-By-Parent.md)
- Defines: [`WVBD 1`](../../Specifications/Windvale-Database-Transaction-Branch-Pages.md)

## Context

The transaction planner could group changed leaves by their real parents and
produce each parent's complete logical final state. It still had no durable
branch identities or checksummed branch pages, so a transaction could not
carry replacements upward toward a completed root.

## Decision

- Allocate all replacement leaf pages before branch pages, using one
  consecutive transaction allocation range.
- Encode each logical parent output as a durable `WVPG 1` branch page with the
  transaction's next generation and sequence.
- Emit one bounded `WVCR 1` plan so the same parent-group operation can consume
  the new pages at the next ancestor level.
- Complete a depth-two root directly when its final state fits one page; leave
  split-root construction as the explicit next boundary.
- Bind large parent-group and page companions by exact length and SHA-256 in a
  small `WVBD 1` manifest instead of copying them into the manifest.
- Preserve the existing 4 MiB native-code ceiling by separating root,
  validation, and depth-three contracts into focused executables.

## Evidence

The root, validation, and depth-three scale-qualified projects build
deterministically to 263,186, 262,360, and 261,937-byte WVBs. Their SHA-256
values are `5aa144cc46cd367adddab350cf7807788c79537d06ee620e87ed7feeaa6d9ad4`,
`8a1908e53cad440b4726efe5d5092fc0b5f64f14e701600bcc4e4bf69a583c2b`,
and `9fc9bbd6dd29035b802e26a28e5f693110913aad6315acbc11a53240dfdb5949`.
All three verify through the native front door.

They lower deterministically to 4,126,250, 4,137,500, and 4,090,954-byte
WVOs with SHA-256
`6a438e89b06eaf76fd22ddfa57a94426d5c7647a3a0c93952a7317602e718f1c`,
`f13e39705f4c5d5e0ce0f3046a12064f3a25d44589761018a0557ab7174ca331`,
and `dd1b9193b031dc2d0bff811fca3cbc2f21654f4fbc05be17b9ce183d46facd17`.
This preserves the current 4 MiB native code limit while testing the complete
portable implementation.

The packaged Windows applications contain 4,144,640, 4,155,904, and
4,109,312 bytes with SHA-256
`a29d2ca50ec8e9419e8f96e35aadcaf877d2b7c4085f8e9fd15e713c7290003b`,
`2e9ee55c3c67b07701749fb6933bfdd5eff004e74b3c70c2ee9523bbb15a5d2d`,
and `26d89dfe916266a2817c0fe6e24ca6ceb5714ee55d86369e8f02eb295090ebf4`.
All return zero.

Twenty fresh sampled whole-process runs measured medians of 149.524,
118.932, and 112.093 ms. Corresponding means were 149.648, 119.546, and
115.619 ms; sampled peak working sets were 10,792,960, 12,443,648, and
10,686,464 bytes. These are correctness-test costs including native process
startup, not persistent-server throughput.

The post-rebase warm-cache focused Windows development target passes all three
cases in 34.870 seconds, including 1.980 seconds of cached tool setup.
Changed-file planning passes 24 general and 144 native routing cases. A
broader local database gate passed the milestone and 37 of 38 total steps,
then an existing
hosted tree-writer image that does not import this work exceeded the lowerer's
4 MiB output limit. Independent Linux execution and broad qualification
remain pending.

## Consequences

Windvale now creates durable branch pages for one complete tree level and
provides the exact generic replacement input needed by the next level. A
depth-two transaction whose root does not split already has a complete new
root page identity.

The result remains a plan, not a commit. Recursive ancestor processing,
split-root construction, compact-log generation, durable write ordering, and
inactive-superblock publication remain later boundaries.

## Reconsideration triggers

Replace companion hashing or immutable page assembly only when
persistent-server measurements show a material throughput or memory cost.
Any replacement must preserve deterministic bytes, exact bounds, complete
validation before use, and atomic failure.
