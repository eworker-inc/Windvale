# Decision 0737: Batch OS x64 project-WVB development checkpoints

- Date: 2026-08-16
- Status: Implemented with complete Windows development evidence; independent Linux execution pending
- Extends: [Decision 0736](0736-Reuse-Os-X64-Verification-Trust-Checks.md)
- Preserves: cold no-argument qualification, 56 projects, 336 cases, immutable publication, and exact final bytes

## Context

After session-scoped trust reuse, the complete Windows OS x64 code-emission
owner still took 82,557 ms. It launched a separate native compiler for every
project even when the exact Project 2 closure and compiler bytes had already
produced an admitted deterministic WVB.

The repository already defines `project-wvb-v2`: a host-scoped checkpoint keyed
by the exact workspace, project identity and bytes, ordered root/source closure,
native-front-door inventory, and build-driver bytes. Calling its command wrapper
once per project was not sufficient. A measured 56-hit run took 94,799 ms,
14.8 percent longer than direct compilation, because it added 56 Node startups
and repeated command-shell hashing and copying lifecycles.

## Decision

- Extract project-key derivation into one importable core while retaining the
  existing command-line adapter and exact length-framed key format.
- Add one OS x64 development batch that validates the complete version-2 target
  manifest and derives every selected project key in one Node process.
- Key the exact staged build-driver bytes used by the bounded owner session. On
  a miss, retain one separate native build-driver process and atomically publish
  one ordinary `project-wvb-v2` entry. On a hit, reject links and unexpected
  entries, rehash the WVB, compare the complete checkpoint record, materialize a
  private copy, and rehash that copy.
- Continue publishing every materialized WVB through the staged WVB publisher.
  Lowering, WVO publication, linking, both host containers, current-host
  execution, and all five exact artifact identities remain fresh per row.
- Add `--development-all` for change-aware runs that require multiple or all
  targets. Keep no-argument owner execution unchanged and cache-independent for
  verification-owner coordination and qualification.
- Make the planner map shared key/cache producers to every development owner
  that consumes them, and declare the OS x64 checkpoint dependency closure.
- Remove the exact locally allocated `.new-*` directory in a guarded `finally`
  after any build/publication failure or lost race. Validate its canonical
  parent and non-link directory identity before recursive removal; preserve a
  successfully renamed checkpoint and validate a race winner before accepting
  it.

## Evidence

The refactored key command reproduced existing key
`b4b2c016cf9e9238af3b8e15c67ccd0ad5e9d6dff097a2b8d6b0526ac46c7ba3`
for the `code` project, proving command/core framing compatibility for that
fixture. A 56-hit Windows run passed all 336 cases in 74,729 ms. This saves
7,828 ms, or 9.48 percent, from the 82,557 ms session-only owner and is 1.10
times faster. The focused hit passed its six checks in 4,076 ms instead of the
4,524 ms direct baseline, a 9.90 percent reduction. Its isolated first miss
created and revalidated the checkpoint while passing all six checks in 4,857
ms; cold publication is setup cost rather than a claimed speedup.

Appending one byte to an isolated cached product caused the batch to reject the
checkpoint record and return 1 before publication or execution; it did not
repair the entry. A forced build failure returned 1 and left zero temporary
directories. Four concurrent cold publishers converged to one `Created` and
three validated `Hit` results with four zero exit codes and zero `.new-*`
debris; Windows race losers reported `EPERM`, which is accepted only when the
complete destination checkpoint validates. Native development dependency
closure passes for four owners
and 42 declarations. Changed-file planning passes 24 general and 164 native
cases. Node syntax and Git Bash syntax pass locally. Independent Linux cache
creation, hit, corruption rejection, and owner execution remain host evidence.

## Consequences

Warm development verification avoids unchanged whole-project compilation
without treating cache state as qualification evidence. One process owns key
and checkpoint validation overhead across the selected set; compiler isolation
on misses and independent later-phase evidence remain intact.

The checkpoint stops at WVB. Reusing WVO, linked images, or containers would
remove different evidence and requires its own measured boundary. Parsed-module,
symbol, WIR, or native-object incrementality also remains future compiler work.

## Reconsideration triggers

Reconsider this decision if the shared core produces a key different from the
command adapter, a declared source or producer change does not select a new
entry, a cache hit differs from a clean WVB, development `all` omits a row,
qualification consults cache state, Linux behavior differs, or a versioned
multi-request compiler makes whole-project checkpoints unnecessary.
