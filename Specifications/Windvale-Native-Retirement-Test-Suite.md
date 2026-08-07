# Windvale native retirement test suite

## Status and scope

This contract composes the fixed .NET-free native test commands into one
digest-bound Windows/Linux suite. It owns only cases already transferred to
native orchestration; it does not reproduce the complete managed Seed, OS,
golden, differential, randomized, or bootstrap suites.

The suite has two invocation modes:

- no arguments select the complete ordered plan; and
- `--filter <suite-name>` selects one exact named suite for a focused check.

The complete invocation is a grouped retirement candidate, not an inner-loop
default. A coherent edit should run only its owning filter. The complete
invocation is reserved for the final grouped retirement gate unless a changed
suite boundary specifically requires it.

## Plan identity and grammar

`Tests/Native/Retirement-Suite.txt` is 1,982 LF-only bytes with SHA-256
`b5c16309688400b138a76c72e147533d124e0b26615a9b382ebf4b55dda07aaa`.
The first line is exactly:

```text
windvale-native-retirement-suite 1
```

Every remaining nonempty line has four pipe-separated fields:

```text
suite-name|command-stem|case-count|expected-summary
```

The manifest digest, rather than host discovery or a candidate-generated
inventory, fixes the command order, case count, and accepted terminal summary.
Every command stem resolves beneath `Tools/Native` to the host's `.cmd` or `.sh`
file. The current plan is:

| Suite | Command | Cases | Required terminal summary |
| --- | --- | ---: | --- |
| `seed` | `Test-Seed` | 26 | `Tests: 26, Passed: 26, Failed: 0` |
| `unsafe-wvb` | `Test-Wvb-Unsafe-Rejections` | 10 | `Tests: 10, Passed: 10, Failed: 0` |
| `wvb-containment` | `Test-Wvb-Containment` | 1,000 | `Tests: 1000, Passed: 1000, Failed: 0` |
| `wvo-read-only` | `Test-Wvo-Read-Only-Rejections` | 13 | `Tests: 13, Passed: 13, Failed: 0` |
| `wvo-differential` | `Test-Wvo-Differential` | 256 | `Tests: 256, Passed: 256, Failed: 0` |
| `wvo-containment` | `Test-Wvo-Containment` | 500 | `Tests: 500, Passed: 500, Failed: 0` |
| `wvo-hostile-size` | `Test-Wvo-Hostile-Size` | 4 | `Tests: 4, Passed: 4, Failed: 0` |
| `assembler-rejections` | `Test-Assembler-Rejections` | 11 | `Tests: 11, Passed: 11, Failed: 0` |
| `wva-differential` | `Test-Wva-Differential` | 200 | `Tests: 200, Passed: 200, Failed: 0` |
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
| `publisher-rejections` | `Test-Publisher-Rejections` | 2 | `Tests: 2, Passed: 2, Failed: 0` |
| `aot-chain` | `Test-Aot-Chain` | 1 | `native aot chain status=Passed result=42` |

The version-1 plan therefore contains exactly 23 suites and 3,035 cases.

## Coordinator contract

`Tools/Native/Test-Retirement-Suite.cmd` and `.sh` must first verify the complete
plan digest. For every selected entry, the coordinator must then:

1. invoke exactly the named host command;
2. require process exit `0`;
3. require empty standard error;
4. require the last nonempty standard-output line to equal the manifest summary;
5. count the manifest-declared cases only after selecting the entry; and
6. emit `PASS  suite <suite-name> cases=<case-count>` only after all checks pass.

The first child failure stops the suite, reports the captured child channels on
standard error, and returns `1`. A missing or changed plan also returns `1`.
Invalid arguments or an unknown exact filter return `64`. Temporary capture
files remain private to one newly allocated directory and are removed on exit.

A complete success ends with:

```text
Suites: 23, Passed: 23, Failed: 0, Cases: 3035
```

For example, the `unsafe-wvb` filter succeeds with:

```text
PASS  suite unsafe-wvb cases=10
Suites: 1, Passed: 1, Failed: 0, Cases: 10
```

The coordinator does not build a managed harness, invoke .NET, discover tests,
rewrite fixtures, or calculate its own expected values. Changes to a child
command's case inventory or success summary require a reviewed manifest update,
a new plan digest in both coordinators, and corresponding contract evidence.

## Boundary

This suite proves deterministic composition of the fixed native lanes. It does
not by itself qualify Linux execution, replace all remaining managed
differential or large-native evidence, promote candidate applications,
authorize removal of Stage 0, or complete the Decision 0057 retirement gate.
