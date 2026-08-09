# Decision 0460: Native hosted enum directory for variants

- Status: Implemented current-host candidate; enlarged-verifier image construction and dual-host promotion pending
- Date: 2026-08-09
- Advances: [Decision 0213](0213-Stage0-Semantic-Freeze-And-Native-Front-Door.md), [Decision 0459](0459-Native-Wvb-1-11-Verifier-Admission.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [native hosted enum processes](../../Specifications/Windvale-Native-Hosted-Enum-Processes.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)

## Context

The accepted-subset native hosted packager obtains nominal-type evidence through
two Windvale processes. Its request process formerly used the native lowerer's
restricted type reader, which rejected WVB 1.11 variants. Its metadata service
also rejected a valid module when that module declared records or variants but
declared no enums. Together those restrictions stopped the enlarged native
verifier at the hosted enum-request boundary even though packaging needs only a
bounded nominal-type directory.

The ordinary native lowerer still does not implement variant lowering. Widening
that lowerer implicitly would confuse type-directory admission with executable
code-generation support.

## Decision

- Keep the ordinary accepted-subset lowerer reader unchanged. Add a separate
  hosted-only nominal-shape reader that admits the frozen WVB 1.11 record and
  variant declarations needed to construct package metadata.
- Validate variant names, cases, payload presence and payload shapes through the
  hosted reader. Preserve the WVB 1.11 prohibition on nested variant payloads.
- Represent enums as metadata-directory kind 1. Represent records and variants
  as kind 0 entries with no enum members.
- Permit a nonempty nominal-type directory whose total enum-member count is
  zero. The `WVEQ` request and `WVEN` service formats already encode that state
  unambiguously; it is not an empty or malformed response.
- Refresh the retained Windvale-written enum-metadata bridge and native fragment
  from their source-exact products. The corresponding C# adjustment remains
  frozen recovery and differential evidence, not a new normal-path owner.
- Add one focused source fixture containing a record and payload variant, no
  enum declaration, and an exhaustive match. It isolates this contract from
  unrelated verifier and package growth.

## Evidence and consequences

The request project builds as 30,759 bytes with SHA-256
`682c2bf76569ba0ec6c58dfd3ade64d7582a9d22c397c55a22e1785fe8521fb6`.
Its Windows application is 334,336 bytes with SHA-256
`fd2b4d21aca8aa27ed5cca535a9a2cbbabe57036d4646cf8c91c5b531cc16ef1`;
its Linux application is 335,872 bytes with SHA-256
`3b614f15a49b93074d3f2ce54eb193c14f391f3af8ac2f4415dbaf8f6dbca2ba`.

The service project builds as 18,883 bytes with SHA-256
`6e44a4c0f4d61ea9aa3d72442baba60080896c0cf7d3536b353fcd61ff48ec07`.
Its exact WVO is 167,750 bytes and its raw native leaf is 166,682 bytes with
SHA-256 `38ea83b0d417bdc57cd0c5b3bd29f8d9cb37a9575767401486fde6da2ded4cea`.
The Windows application is 184,832 bytes with SHA-256
`7563f37f3c77473ce52b73506dcb54516107fc964c19860d5dcc75d3bdd52cdf`;
the Linux application is 184,320 bytes with SHA-256
`90e99134b750231e011e2c254bac8c12d9d52d8adcbd3a9d5d03d7dc1da26f4b`.

The source-exact retained metadata bridge is 15,292 bytes with SHA-256
`052be4402df26ed542107d666ed894cadb04a46ba6b2428bafc9f1879e38a072`.
Its native fragment is 137,964 bytes with SHA-256
`004db29841eeaf5a448ec67c438a820832ed4af3ede0a8ae1b1d672565ea0999`.
The focused variant-only fixture compiles to a 665-byte WVB with SHA-256
`789094b2b3cfb2f06c0ad1799e8d12a7d0d6e3b618223158437339baeaf188da`.

Two reviewed current-host focused tests pass: the hosted request/service process
test, including exact zero-enum variant output, and the retained enum-name and
metadata source/artifact identity test. This closes the previously measured
enum-request rejection.

The 57-artifact hosted-container candidate inventory is repinned around the six
new request/service WVB and Windows/Linux products. Its 5,426-byte `SHA256SUMS`
has SHA-256
`f674de96634840c42cecd77d3af34de87e2c06458dae3a36577f18da83c5f99d`,
and every listed artifact matches. The existing focused package workflow passes
all three current-host cases in 17.6 seconds: exact Windows construction, exact
cross-target Linux construction, and invalid-WVB rejection with input/output
preservation and no private scratch. This proves candidate-toolset integration
without promoting it.

The enlarged verifier's exact pre-lowered native chunk set and entry offset are
not retained in this checkout, so its separate image-mode container construction
has not run. That construction, independent Linux execution, grouped retirement
gate, and ordinary-path promotion remain explicit next work.

No broad Seed, OS, Standard, Qualification, WebAssembly, QEMU, or complete
retirement gate ran. Those checks remain grouped at the end of the active
retirement goal.

## Reconsideration triggers

Unify the hosted reader with the native lowerer reader only when the lowerer
actually owns executable variant lowering. Do not make package metadata
admission imply code-generation support.
