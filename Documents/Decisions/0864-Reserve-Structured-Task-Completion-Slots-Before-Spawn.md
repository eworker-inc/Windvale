# Decision 0864: Reserve structured-task completion slots before spawn

- Date: 2026-08-27
- Status: Implemented candidate; paired-host rerun pending
- Requires: [Decision 0861](0861-Execute-Structured-Tasks-As-Wvb-1.32.md)
- Follows: [Decision 0862](0862-Restore-Cross-Host-Slice-7-Development-Gates.md)

## Context

The frozen Language 1.0 Foundation contract requires spawn to reject before
capture acceptance when the scope cannot retain every eventual child outcome.
The paper workload fixes `Maximumˉcompleted=4`: four live children consume the
four available outcome positions even before any child finishes, so a fifth
spawn must return `Queueˉlimit(Work)`.

The first sequential WVB 1.32 runtime instead limited only the count of already
completed children. It could accept more live children than the completion
bound and then reject a child transition after that child had already run. That
late failure could neither return the accepted closure nor preserve the promise
that every accepted child has one retainable terminal outcome.

## Decision

- Treat `Maximumˉcompleted` as admission capacity. Every accepted live child
  owns one reserved terminal-outcome position, whether runnable or completed.
- Reject spawn with queue-limit reason `2` before changing state when either the
  runnable bound is full or `Live >= Maximumˉcompleted`.
- Hold the reservation across child completion. Consuming the affine handle
  with `await`, or bounded scope teardown, releases it; completion alone does
  not.
- Require every active scope to satisfy `Live <= Maximumˉcompleted` in the
  canonical state validator. Retain the completion-time bound check as a
  defensive malformed-state guard, although valid admitted state makes it
  unreachable.
- Keep WVB at 1.32 and the internal fixed task-state encoding at version 1. No
  source form, opcode, result variant, serialized field, or compatibility path
  changes.

## Evidence

The runtime-core self-test constructs exact limits `(Maximumˉchildren=5,
Maximumˉrunnable=5, Maximumˉcompleted=4)`. It accepts four children, rejects a
fifth without changing state, completes the first child, rejects the fifth
again, awaits the first handle, then accepts the fifth. The remaining four
children all complete before their handles are consumed, proving that the four
reserved outcomes can be retained together and the scope exits normally.

The focused `language-1-memory-budget-split-execution` owner passes all 132
registered cases, including its 37-case runtime-core boundary, after rebuilding
and executing the source-built runner. The current candidate reconstructs as:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| `Wvb-Runner.wvb` | 445,516 | `366f20e2ff2fb12aa418861d2bb8fc0651439b7fb8fd11f73c9081e8a7cd7b4e` |
| `windows-x64-wvrun.exe` | 5,329,920 | `0d87bbcb2265efb58d62ef2b406881aee26d51a9baf80fdf818f052b64acc258` |
| `linux-x64-wvrun.elf` | 5,328,896 | `22720c0eab924d82983abb7c37c6e2aaaf907da24269af2b824bcb2f8833b0ed` |

Segmented staging emits 5,320,819 object bytes in 12 chunks. Linking emits a
5,311,909-byte image in eight chunks at entry offset 105,270, and canonical
transport carries it in two chunks. Direct local execution on Windows and
through Linux WSL preserves the exact ordinary result, four-step report,
unknown-option rejection, and malformed-module rejection identities.
The focused Windows reconstruction owner independently rebuilds all three
candidate artifacts from source, requires byte equality, exercises the same
runtime observations, and passes all three cases in 618.150 seconds. Its
temporary-product cleanup retries for at most two seconds and validates the
exact system-temporary parent and Windvale-owned name before recursive removal.

The runner is also part of both installer channels. The repinned installer
owner passes eight reproducibility, corruption, extraction, installation,
execution, tamper, and uninstall cases across four archives. The selective
installer-repository owner passes twelve deterministic construction,
selection, and malformed-state cases over 15 content-addressed objects. Exact
archive, payload, index, blob, and profile-selection identities are recorded in
the installer specifications rather than silently retaining the prior runner.
Independent GitHub Windows and Linux reconstruction remains required before
this candidate becomes paired-host evidence.

## Consequences

An accepted child can always publish exactly one bounded terminal outcome.
Backpressure is observable at the reversible ownership boundary, where the
caller still receives its exact work and captures. Applications that want more
simultaneous live children must choose a larger completion bound; completing a
child early does not admit replacement work until its handle is consumed.

The change adds one local value and one comparison on spawn plus one validator
comparison. It does not affect child dispatch cost, task outcome layout, source
syntax, or portable bytecode versioning.

## Reconsideration triggers

Reconsider the one-reservation-per-live-child policy only if a future scheduler
can prove an equally bounded outcome owner before capture acceptance. Any
replacement must preserve exact rejected-work recovery, one terminal outcome
per accepted child, affine observation, bounded teardown, and deterministic
source-visible ordering.
