# Decision 0490: Indexed compiler-WVB verification

- Status: Accepted current-host candidate
- Date: 2026-08-10
- Scope: compiler-aligned WVB verifier capacity and shared consumers
- Extends: [Decision 0203](0203-Evolved-Compiler-Hosted-Tool-Capacity.md) and [Decision 0459](0459-Native-Wvb-1-11-Verifier-Admission.md)
- Retains: `WVHV 1`, the 16,000,000,000-instruction ceiling, verifier semantics, capability profiles, native ABI 22, and the qualified ordinary front door until promotion

## Context

The current 1.1 MiB compiler build-driver candidate reached the standalone
verifier's 16-billion-instruction ceiling during typed executable and control
verification. A temporary diagnostic package with the ceiling changed to
32 billion accepted the unchanged bytes, proving that instruction work rather
than the dynamic arena or a semantic rejection was the immediate boundary.

The measured work was not inherent to the contract. Typed local operations
rescanned variable-width parameter and local shape lists for every load and
store. Control verification rescanned the complete function for branch targets
and predecessors. The largest current compiler-family functions contain up to
1,408 locals, so those repeated scans amplified ordinary source growth.

Decision 0203 explicitly named approach to the 16-billion ceiling as a
reconsideration trigger. Raising the ceiling again would retain avoidable work
and repin every `WVHV` runtime. A focused indexing layer is the smaller change.

## Decision

- Retain the exact 16,000,000,000-instruction `WVHV` limit.
- Build one fixed-width five-byte kind/nominal record for each parameter and
  local before verifying a function's operations. Local load and store checks
  then use direct checked indexing.
- Build one byte-aligned instruction-boundary directory and one ordered
  source/target branch-pair directory before control validation. Preserve the
  existing rule that a nonzero target begins immediately after an unconditional
  jump, conditional jump, or return, and that unreachable fallthrough is
  accepted only when an earlier branch targets it.
- Keep the directory implementation in the focused portable
  `Compiler-Wvb-Verifier-Typed-Directories.wv` module. The standalone verifier,
  build driver, and WVB publisher all import the same implementation; no fast
  verifier fork or Stage 0-only rule is introduced.
- Update the existing extended verifier/build-driver test so its already-built
  current-host verifier also admits the exact current build-driver WVB. Do not
  add another broad test or reconstruct the verifier twice.

## Current-host evidence

The frozen Stage 0 recovery compiler and the qualified native build front door
produce the same 148,793-byte verifier WVB with SHA-256
`70bd61e78c2ddd6052adb15f24a155f006ded903ce7825d8f54adafa252b76f8`.
Recovery packaging produces:

- Windows: 1,226,240 bytes, SHA-256
  `332488305b0b178dcb713edd81f2df0b8f04455b95e03ee46aa226c69e2ee018`;
- Linux: 1,224,704 bytes, SHA-256
  `e59a0fd2b7c959306b446e8bf387d54118b4719d9099b71f224c1ea4d34802f3`.

The Windows package admits the exact 1,101,328-byte build-driver candidate at
SHA-256
`76947c7eeca769cf912c695887b2f1446ee9344790b654acc6832c8ced163b10`
under the unchanged ceiling in 8.4 seconds, with exact success output and an
empty diagnostic stream. The one directly affected regular WVB-publisher test
passes. The large extended compiler/bootstrap test and broad local
qualification were deliberately not rerun for this slice.

## Consequences

Current compiler-family growth no longer requires weakening the standalone
verifier's resource bound. The additional directories are immutable per-function
evidence derived from the same already-admitted WVB bytes; they grant no host
authority and change no serialized format.

Exact verifier, build-driver, and WVB-publisher descendants change because the
portable verifier source is shared. The role-2 native construction records were
repinned and reproduced the independent Stage 0 publisher bytes exactly:
Windows is 1,340,928 bytes at SHA-256
`9ee91e3044193e2e90461ecf4e7ddefa4b5583f55b041b31911044c6d65b92c7`,
and Linux is 1,340,357 bytes at SHA-256
`2ade91f624609c93a3b80a0802679bef79832c0a63db7996c889794d365f1188`.
Qualified ordinary front-door artifacts remain pinned until their existing
dual-host promotion gates pass.

## Reconsideration

Reconsider this decision if the indexed verifier again approaches the retained
ceiling, if directory construction becomes the dominant bounded cost, if a
general nonempty-stack-join contract replaces the compiler-aligned boundary, or
if Windows and Linux cannot reproduce the same candidate identities and result.
