# Windvale native retirement test suite

## Status and scope

This contract composes the fixed .NET-free native test commands into one
digest-bound Windows/Linux suite. It owns only cases already transferred to
native orchestration; it does not reproduce the complete managed Seed, OS,
golden, differential, randomized, or bootstrap suites.

The suite has three invocation modes:

- no arguments select the complete ordered plan; and
- `--filter <suite-name>` selects one exact named suite for a focused check;
  and
- `--shard <1-4>` selects one digest-bound disjoint qualification shard while
  retaining canonical manifest order within that shard.

The complete invocation is a grouped retirement candidate, not an inner-loop
default. A coherent edit should run only its owning filter. The complete
invocation is reserved for the final grouped retirement gate unless a changed
suite boundary specifically requires it.

## Plan identity and grammar

`Tests/Native/Retirement-Suite.txt` is 7,228 LF-only bytes with SHA-256
`bdb25b5fc35ef02a55c33c268ef245b3fd153eae0b7f5a959a4a91b9d2107bd1`.
The first line is exactly:

```text
windvale-native-retirement-suite 2
```

Every remaining nonempty line has five pipe-separated fields:

```text
suite-name|command-stem|case-count|shard|expected-summary
```

The manifest digest, rather than host discovery or a candidate-generated
inventory, fixes the command order, case count, shard, and accepted terminal
summary. Shards are the canonical decimal values `1` through `4`; every suite
belongs to exactly one shard.
Every command stem resolves beneath `Tools/Native` to the host's `.cmd` or `.sh`
file. The current plan is:

| Suite | Command | Cases | Required terminal summary |
| --- | --- | ---: | --- |
| `seed` | `Test-Seed` | 26 | `Tests: 26, Passed: 26, Failed: 0` |
| `seed-native-front-door` | `Test-Seed-Native-Front-Door` | 1 | `Tests: 1, Passed: 1, Failed: 0` |
| `seed-native-front-door-reconstruction` | `Test-Seed-Native-Front-Door-Reconstruction` | 1 | `Tests: 1, Passed: 1, Failed: 0` |
| `seed-native-console-aot` | `Test-Seed-Native-Console-Aot` | 1 | `Tests: 1, Passed: 1, Failed: 0` |
| `compiler-reconstruction` | `Test-Compiler-Reconstruction` | 3 | `Tests: 3, Passed: 3, Failed: 0` |
| `segmented-compiler-toolset-reconstruction` | `Test-Segmented-Compiler-Toolset-Reconstruction` | 3 | `Tests: 3, Passed: 3, Failed: 0` |
| `wvb-to-wvo-reconstruction` | `Test-Wvb-To-Wvo-Reconstruction` | 3 | `Tests: 3, Passed: 3, Failed: 0` |
| `wvb-runner-reconstruction` | `Test-Wvb-Runner-Reconstruction` | 3 | `Tests: 3, Passed: 3, Failed: 0` |
| `wv-linker-reconstruction` | `Test-Wv-Linker-Reconstruction` | 3 | `Tests: 3, Passed: 3, Failed: 0` |
| `wvo-inspector-reconstruction` | `Test-Wvo-Inspector-Reconstruction` | 3 | `Tests: 3, Passed: 3, Failed: 0` |
| `console-verifier-reconstruction` | `Test-Console-Verifier-Reconstruction` | 3 | `Tests: 3, Passed: 3, Failed: 0` |
| `console-publisher-reconstruction` | `Test-Console-Application-Publisher-Reconstruction` | 3 | `Tests: 3, Passed: 3, Failed: 0` |
| `wvo-publisher-reconstruction` | `Test-Wvo-Publisher-Reconstruction` | 2 | `Tests: 2, Passed: 2, Failed: 0` |
| `baseline-jit` | `Test-Baseline-Jit` | 6 | `Tests: 6, Passed: 6, Failed: 0` |
| `unsafe-wvb` | `Test-Wvb-Unsafe-Rejections` | 20 | `Tests: 20, Passed: 20, Failed: 0` |
| `wvb-containment` | `Test-Wvb-Containment` | 1,000 | `Tests: 1000, Passed: 1000, Failed: 0` |
| `wvo-read-only` | `Test-Wvo-Read-Only-Rejections` | 13 | `Tests: 13, Passed: 13, Failed: 0` |
| `wvo-differential` | `Test-Wvo-Differential` | 256 | `Tests: 256, Passed: 256, Failed: 0` |
| `wvo-containment` | `Test-Wvo-Containment` | 500 | `Tests: 500, Passed: 500, Failed: 0` |
| `wvo-hostile-size` | `Test-Wvo-Hostile-Size` | 5 | `Tests: 5, Passed: 5, Failed: 0` |
| `assembler-rejections` | `Test-Assembler-Rejections` | 11 | `Tests: 11, Passed: 11, Failed: 0` |
| `assembler-golden` | `Test-Assembler-Golden` | 4 | `Tests: 4, Passed: 4, Failed: 0` |
| `wva-differential` | `Test-Wva-Differential` | 269 | `Tests: 269, Passed: 269, Failed: 0` |
| `source-containment` | `Test-Source-Containment` | 500 | `Tests: 500, Passed: 500, Failed: 0` |
| `lowerer-rejections` | `Test-Lowerer-Rejections` | 2 | `Tests: 2, Passed: 2, Failed: 0` |
| `linker-rejections` | `Test-Linker-Rejections` | 10 | `Tests: 10, Passed: 10, Failed: 0` |
| `linker-hostile` | `Test-Linker-Hostile-Inputs` | 200 | `Tests: 200, Passed: 200, Failed: 0` |
| `linker-map-limit` | `Test-Linker-Map-Limit` | 1 | `Tests: 1, Passed: 1, Failed: 0` |
| `console-packager-rejections` | `Test-Console-Packager-Rejections` | 3 | `Tests: 3, Passed: 3, Failed: 0` |
| `console-container-hostile` | `Test-Console-Container-Hostile-Inputs` | 256 | `Tests: 256, Passed: 256, Failed: 0` |
| `console-container-mutations` | `Test-Console-Container-Mutations` | 19 | `Tests: 19, Passed: 19, Failed: 0` |
| `hosted-console-container-mutations` | `Test-Hosted-Console-Container-Mutations` | 15 | `Tests: 15, Passed: 15, Failed: 0` |
| `console-segmented-size` | `Test-Console-Application-Segmented-Size-Boundaries` | 2 | `Tests: 2, Passed: 2, Failed: 0` |
| `console-segmented-construction` | `Test-Console-Application-Segmented-Construction` | 2 | `Tests: 2, Passed: 2, Failed: 0` |
| `console-packager-source-reconstruction` | `Test-Console-Packager-Source-Reconstruction` | 2 | `Tests: 2, Passed: 2, Failed: 0` |
| `console-packager-container-reconstruction` | `Test-Console-Packager-Container-Reconstruction` | 4 | `Tests: 4, Passed: 4, Failed: 0` |
| `publisher-rejections` | `Test-Publisher-Rejections` | 4 | `Tests: 4, Passed: 4, Failed: 0` |
| `hosted-verifier-publisher-files` | `Test-Hosted-Verifier-Publisher-File-Pipeline` | 15 | `Tests: 15, Passed: 15, Failed: 0` |
| `uefi-packager` | `Test-Uefi-Packager` | 3 | `Tests: 3, Passed: 3, Failed: 0` |
| `wvo-export-renamer` | `Test-Wvo-Export-Renamer` | 4 | `Tests: 4, Passed: 4, Failed: 0` |
| `os-probe-object` | `Test-Os-Probe-Object-Producer` | 11 | `Tests: 11, Passed: 11, Failed: 0` |
| `os-kernel-target` | `Test-Os-Kernel-Target` | 7 | `Tests: 7, Passed: 7, Failed: 0` |
| `os-process-policy` | `Test-Os-Process-Policy-Object` | 2 | `Tests: 2, Passed: 2, Failed: 0` |
| `os-process-object` | `Test-Os-Process-Object` | 2 | `Tests: 2, Passed: 2, Failed: 0` |
| `os-resource-domain` | `Test-Os-Resource-Domain` | 2 | `native os resource domain status=Passed projects=1 behavior=1 cases=2` |
| `os-services` | `Test-Os-Services` | 10 | `native os services status=Passed projects=8 behavior=2 cases=10` |
| `os-probe` | `Test-Os-Probe` | 4 | `Tests: 4, Passed: 4, Failed: 0` |
| `aot-chain` | `Test-Aot-Chain` | 1 | `native aot chain status=Passed result=42` |
| `native-u64-lowering` | `Test-Native-U64-Lowering` | 1 | `native u64 lowering status=Passed local-result=42 database-page=42 cross-host-images=Verified` |
| `database-superblock` | `Test-Database-Superblock` | 13 | `native database superblock status=Passed cases=13 local-result=42 cross-host-images=Verified` |
| `database-durable-commit` | `Test-Database-Durable-Commit` | 12 | `native database durable commit status=Passed cases=12 local-result=42 cross-host-images=Verified` |
| `database-storage` | `Test-Database-Storage` | 22 | `native database storage status=Passed cases=22 local-results=0 cross-host-images=Verified` |
| `workspace-project2` | `Test-Workspace-Project2` | 8 | `native workspace/project test status=Complete cases=8` |
| `libraries` | `Test-Libraries` | 29 | `native libraries status=Passed projects=19 conformance-builds=8 negative=2 cases=29` |
| `packages` | `Test-Wvdb-Query-Package` | 11 | `native package status=Passed packages=2 builds=4 inspection=2 negative=3 preservation=1 cases=11` |
| `package-format` | `Test-Package-Format` | 82 | `native package format status=Passed result=42 modules=6 builds=7 groups=82 cross-host-images=12` |
| `installation-activation` | `Test-Installation-Activation` | 16 | `native installation activation status=Passed cases=16 records=3 current=5cb7a15cb4f585d3aca474c4acdb35f55e6b6a4660aeee1d568db719f62742d3` |
| `installation-command-resolution` | `Test-Installation-Command-Resolution` | 8 | `native installation command resolution status=Passed cases=8 commands=2 cross-host-generations=Verified` |
| `installation-command-dispatch` | `Test-Installation-Command-Dispatch` | 9 | `native installation command dispatch status=Passed cases=9 commands=2 executions=2 integrity-rejections=4 policy-rejections=3` |
| `installation-generation-publication` | `Test-Installation-Generation-Publication` | 8 | `native installation generation publication status=Passed cases=8 generation=c8b8daac8f5298b8c3cab1ba46312ddbe09d6f1718972599872eb901a482f261 bytes=377` |
| `package-bundle` | `Test-Package-Bundle` | 12 | `native package bundle status=Passed cases=12 applications=2 bundles=2 objects=9 shared=1 idempotent=Verified` |
| `installers` | `Test-Installers` | 8 | `native installer status=Passed cases=8 channels=2 archives=4 reproducible=Verified host-install=Verified` |
| `wvdb-query-capability` | `Test-Wvdb-Query-Native-Capability` | 5 | `native wvdb query capability status=Passed cases=5 capabilities=5 wvb=61f7b9d739a0f4ac9eece1cb79e554e373f49375109cf23d332921395ae37dc2 cross-host-images=Verified` |
| `release-envelope` | `Test-Release-Envelope` | 17 | `native release envelope status=Passed cases=17 signatures=4 artifacts=12 packages=2 protected-private-keys=2` |
| `offline-package-stage` | `Test-Offline-Package-Stage` | 8 | `native offline package stage status=Passed cases=8 packages=2 policy-records=8 generations=2 published=1 artifacts=14 deterministic=Verified tamper=Rejected` |
| `wvdb-approval` | `Test-Wvdb-Approval-Records` | 13 | `native application approval status=Passed cases=13 applications=2 records=6 capabilities=10 targets=4 executions=1` |

The version-2 plan therefore contains exactly 66 suites and 3,502 cases. Its
balanced shard inventory is:

| Shard | Suites | Cases | Measured slower-host seconds |
| ---: | ---: | ---: | ---: |
| 1 | 1 | 22 | 651.2 |
| 2 | 16 | 1,282 | 592.2 |
| 3 | 25 | 1,284 | 592.1 |
| 4 | 24 | 914 | 591.8 plus the newly added dispatch owner |

The timing column is scheduling evidence from GitHub run `31806725202`, not a
semantic limit. The shard-1 timing predates the three new tree-growth cases and
the shard-3 timing predates the `package-format`, `installation-activation`,
`installation-command-resolution`, `installation-command-dispatch`,
`installation-generation-publication`, `package-bundle`,
`installers`, `wvdb-query-capability`, `release-envelope`, and
`wvdb-approval` owners; both will be
refreshed after the expanded plan completes qualification. Allocation uses
the slower observed Windows/Linux interval for each measured owner and keeps
the largest owner alone because it sets the current lower bound. Future
rebalancing changes the digest-bound plan and requires new
dual-host evidence.

## Coordinator contract

`Tools/Native/Test-Retirement-Suite.cmd` and `.sh` must first verify the complete
plan digest. For every selected entry, the coordinator must then:

1. invoke exactly the named host command;
2. require process exit `0`;
3. require empty standard error;
4. require the last nonempty standard-output line to equal the manifest summary;
5. count the manifest-declared cases only after selecting the entry; and
6. measure the child wall time without placing timing in its semantic summary;
   and
7. emit `PASS  suite <suite-name> cases=<case-count> elapsed-ms=<time>` only
   after all checks pass.

The first child failure stops the suite, reports the captured child channels on
standard error, and returns `1`. A missing or changed plan also returns `1`.
Invalid arguments or an unknown exact filter return `64`. Temporary capture
files remain private to one newly allocated directory and are removed on exit.

A complete success ends with:

```text
Timing: elapsed-ms=<time>
Suites: 66, Passed: 66, Failed: 0, Cases: 3502
```

For example, the `unsafe-wvb` filter succeeds with:

```text
PASS  suite unsafe-wvb cases=20 elapsed-ms=<time>
Timing: elapsed-ms=<time>
Suites: 1, Passed: 1, Failed: 0, Cases: 20
```

Each shard ends with its selected suite and case totals. GitHub runs all four
shards independently on each host with matrix fail-fast disabled, so one shard
failure does not discard evidence from the other shards. The unchanged final
`Verification gate` requires the aggregate matrix result from both hosts.

The coordinator does not build a managed harness, invoke .NET, discover tests,
rewrite fixtures, or calculate its own expected values. Changes to a child
command's case inventory or success summary require a reviewed manifest update,
a new plan digest in both coordinators, and corresponding contract evidence.

## Boundary

This suite proves deterministic composition of the fixed native lanes. It does
not by itself qualify Linux execution, replace all remaining managed
differential or large-native evidence, promote candidate applications,
authorize removal of Stage 0, or complete the Decision 0057 retirement gate.
