# Windvale native verification owners

## Status and purpose

This is the current native verification registry. It is not a .NET-retirement
milestone and it is not evidence that every owner ran on every commit.

An owner is one named, independently runnable check for a maintained boundary.
Changed-file development verification selects only the affected owners.
Explicit qualification composes all owners into four cold shards on Windows and
Debian. The same owner commands are reused so development and qualification do
not create parallel test implementations.

The historical retirement claim remains frozen at the immutable `v0.1.0` tag
and in [the retirement archive](Windvale-Native-Retirement-Test-Suite.md).
The cross-host runner choice is recorded by
[Decision 0924](../Documents/Decisions/0924-Use-One-PowerShell-Verification-Owner-Runner.md).
Structured outcomes and duration policy are recorded by
[Decision 0926](../Documents/Decisions/0926-Classify-And-Bound-Verification-Owner-Outcomes.md).
Bounded timing calibration is recorded by
[Decision 0927](../Documents/Decisions/0927-Calibrate-Verification-Durations-From-Bounded-History.md).

## Registry grammar and validation

`Tests/Native/Verification-Owners.txt` is UTF-8, LF-only text with a final
newline. Its first line is exactly:

```text
windvale-native-verification-owners 2
```

Every remaining line has six pipe-separated fields:

```text
owner-name|command-stem|case-count|qualification-shard|duration-profile|expected-summary
```

The runner requires exactly six nonempty fields, unique constrained owner and
command names, a positive bounded case count, a shard from `1` through `4`, a
known duration profile, and at least one owner in every shard. Each command stem resolves under
`Tools/Native` to a non-linked Windows `.cmd` or Linux `.sh` command; Linux
commands must retain executable Git mode. The runner calculates owner, case,
and shard totals from this validated registry instead of comparing them with
duplicated constants. Git history identifies the registry used by a development
run, while qualification evidence records the exact source state.

`Tests/Native/Verification-Duration-Profiles.txt` is the canonical bounded
policy table. Each profile defines conservative expected seconds, enforced
maximum seconds, and zero or one retry for an explicitly retryable
infrastructure failure. Every profile must be used. Expected durations are
planning allowances, not performance claims; structured run results provide the
measurements used to recalibrate them.

The manifest is the canonical detailed inventory. Documentation must not copy
its entire evolving table because duplicated inventories become stale.

## Invocation modes

`Tools/Verify/Invoke-WindvaleTests.ps1` is the cross-host orchestration entry
point. It supports:

- `-Owner <owner-name>` for one exact development owner;
- `-Shard <1-4>` for one explicit qualification shard;
- `-PlanOnly` for registry validation and selection without execution; and
- `-ResultPath <new-json-path>` for a bounded machine-readable result;
- `-AllowLongRun` for an approved plan whose aggregate expected duration exceeds
  the ten-minute local development budget; and
- no arguments for a deliberate complete local qualification run.

Ordinary development must use the changed-file planner or an exact filter. A
commit or push is not a reason to run all owners. Complete and sharded runs are
reserved for release candidates, promotions, security boundaries, or another
named qualification need.

An owner may expose an explicit development mode when its qualification command
constructs evidence that is unnecessarily broad for an ordinary edit. The
changed-file dispatcher must select that mode explicitly, keep its inputs and
oracle in the development dependency registry, and leave the no-argument owner
command as the complete qualification contract. A development-mode pass must
not be reported as the qualification evidence it intentionally omits.

One coherent source state receives one final selected plan. A failure
invalidates that owner and owners whose declared inputs changed; it does not
invalidate unrelated passing owners.

The changed-file front door persists successful development-owner results and
resumes them automatically. Version 1 is deliberately conservative: its state
key covers the complete non-ignored Git source tree rather than trying to infer
a smaller dependency set for each owner. Tracked working-tree changes and
untracked non-ignored files therefore change the key, while a commit or push
that leaves the source tree byte-identical does not. Any source-tree change
invalidates every cached owner result in this first version.

The state key also covers the checkout path, operating-system release,
architecture, host and boot identity, relevant process environment, Node
version, and the paths, sizes, and SHA-256 identities of the fixed host-tool set
used by native owners. Each result key additionally covers the exact suite,
host command, arguments, verification scope, and result-cache format. The
tracked diff and untracked-file content sentinel is measured again after a
passing owner; publication is skipped if it changed while the owner ran. This
keeps the per-owner confirmation proportional to current edits rather than
rebuilding the complete source tree after every pass.

Only `Tools/Verify/Verify-Changed.ps1` development runs reuse these results.
Qualification, direct owner commands, and sharded or complete runner executions
remain fresh. `-NoResultCache` forces a fresh changed-file development run.
`-ResultCacheRoot <path>` or
`WINDVALE_VERIFICATION_RESULT_CACHE_ROOT` selects an outside-repository cache
root for controlled testing; the default is the Windvale local application or
XDG cache area. Cache setup, probing, or publication failure emits a warning
and runs the owner normally, so this optimization cannot become a correctness
dependency.

Pass records are validated ordinary files of at most 16 KiB and are published
atomically from a same-directory temporary file. Corrupt records become misses.
The publisher removes only its exact temporary file, including after a
publication race or failure. Retention is bounded to 16 source states and 512
records per state; source states older than seven days and recognized temporary
files older than one hour are removed during preparation. Unexpected linked or
non-directory paths reject cache use rather than being traversed or removed.

Independent work may run concurrently only under a bounded resource policy.
Shared compiler reconstruction, cache publication, storage, and other
contention-heavy owners remain serialized unless measurement proves a safe
limit. An individual owner may use bounded internal concurrency for independent
products; `language-1-callable-semantics`, for example, packages at most two
distinct fixtures at once while preserving all registered cases. Parallelism
changes scheduling, not the accepted terminal summary or evidence boundary.

The historical `Test-Retirement-Suite.cmd` and `.sh` compatibility aliases and
the later paired verification-owner coordinators were removed from `main`
because they added no coverage and duplicated orchestration. They remain
available from Git history. Current automation and documentation use the
PowerShell runner and verification-owner name.

## Runner contract

Before running a selected owner, the PowerShell runner validates the complete
registry structure and every host command and shard entry. Node discovery binds
exactly the first application in host `PATH` order and validates that one
executable as Node.js 24; multiple installed Node paths must never be combined
into one command string. For each selected owner it must:

1. emit bounded progress before invoking the child;
2. invoke exactly the registered host command;
3. stream child output live while retaining at most 8 MiB separately for each
   output channel;
4. after 30 seconds without complete-line child activity, emit an external
   heartbeat, capped at 120 lines and excluded from the retained child log;
5. terminate the complete owner process tree at the profile's total maximum,
   including any infrastructure retry, and bound the post-termination settle
   interval;
6. require exit code `0` and empty standard error;
7. require the last nonempty output line to equal the registered summary;
8. count cases only from the reviewed registry; and
9. report owner and total elapsed time outside the semantic child summary.

The transitional runner delegates bounded byte streaming to the existing Node
stream helper; individual owner implementations remain paired host scripts.
This keeps behavior unchanged while orchestration converges on PowerShell.
The helper returns a bounded `windvale-verification-owner-process-1` status
record describing normal exit, timeout, or framework failure; the PowerShell
runner validates that record before interpreting owner output.
Owner-log parents are validated component by component with filesystem
metadata. Symbolic links, junctions, and non-directory components reject;
different legitimate spellings of the same Windows directory, including an
NTFS 8.3 ancestor alias, do not constitute link evidence by themselves. The
registered `verification-owner-stream` owner keeps this boundary executable.

The first child failure stops that runner process after its output has already
been exposed live. Outcomes are `passed`, `test-failed`, `timed-out`, or
`framework-error`; process exit codes are respectively `0`, `1`, `124`, and `2`.
Invalid owner, shard, or unapproved over-budget selections return `64`.
The result record format is `windvale-verification-run-result-1`, is capped at
64 KiB, and records the selected plan, per-owner profile, attempts, elapsed time,
outcome, host family, and bounded diagnostic. The destination must be a new file in a
non-linked existing parent.

Only a process-launch, stream-I/O, or process-status publication failure
explicitly marked retryable may use the profile's single retry. A timeout,
output-limit violation, nonzero owner exit, standard-error output, or
terminal-summary mismatch is never retried.

## Timing calibration contract

`Tools/Verify/Update-Verification-Timing-History.ps1` accepts one structured
report or a recursively enumerated report directory. It recognizes only
`windvale-verification-run-result-1` and
`windvale-native-changed-verification-timing-2`. Unknown JSON formats are
counted and skipped; a file claiming either recognized format must validate
completely before any output changes. The input is limited to 256 non-linked
JSON files, 64 KiB each, and 16 MiB total. Recursive discovery stops after 512
directories or 2,048 filesystem entries.
An otherwise valid report with an empty owner or entry array is accepted and
adds no sample; this is the expected result when a development host delegates
all selected shared checks and has no host-specific owner.

The `windvale-verification-timing-history-1` output retains at most 20 samples
for each owner and host. Each sample records the registered owner, normalized
host, observation time, bounded elapsed milliseconds, classified outcome,
source format, and source-file SHA-256 identity. Replaying the same report does
not add a sample. Cached changed-file entries and timings that do not name a
registered owner are not observations. The history is capped at 2 MiB and the
companion `windvale-verification-timing-analysis-1` output at 256 KiB; both use
same-directory atomic replacement in explicitly selected non-linked parents.

The analysis requires at least five passing samples from Windows and five from
Linux before making a profile-change recommendation. A reduction moves only to
the next smaller profile and requires that its expected duration retain 50
percent headroom over the observed 95th percentile while its maximum retains 25
percent over the observed maximum. A timeout suppresses reduction and requests
review. The analyzer may recommend but never edits the duration or owner
registry. Timing history is optimization evidence, not a verification pass or
qualification record.

GitHub runs
all four qualification shards independently with matrix fail-fast disabled,
then an aggregate gate requires both host matrices and the independent
WebAssembly and compiler-convergence jobs. The pinned Debian container installs
the pinned, checksum-verified PowerShell archive before invoking the same runner
used on Windows and ordinary Linux hosts.

## Boundary

An owner result is focused development evidence. A complete paired-host run is
qualification evidence for one exact source state. Neither result changes
language semantics, grants release approval, or revives managed Stage 0 as a
live dependency.
