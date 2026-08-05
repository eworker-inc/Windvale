# Decision 0229: Native malformed-WVB test fixtures

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md), [Decision 0218](0218-First-Native-Test-Orchestration.md), and [Decision 0227](0227-Native-Result-And-Failure-Test-Plan.md)
- Contract: [Windvale native test plan](../../Specifications/Windvale-Native-Test-Plan.md)

## Context

`WVNT 2` builds valid projects and checks successful or failed execution. The managed conformance suite still exclusively owned malformed WVB envelope fixtures even though the qualified native verifier already rejects those inputs. Recreating corruptions independently in Windows batch and Bash would introduce duplicated host-specific mutation policy, while adding a general mutator would create another tool and packaging surface solely for tests.

## Decision

### Add one fixed external-fixture input kind

Supersede the unqualified version-2 plan with fixed `WVNT 3`. Every row has six fields: name, input kind, input path, decoded/input WVB SHA-256, expectation kind, and expectation value.

- `project` retains the existing native Project 1 build path.
- `fixture-base64` decodes one checked-in base64 text fixture into a caller-private `.wvb`, then requires its exact decoded SHA-256 before use.

The input kind is closed; arbitrary generators, commands, shell fragments, mutation offsets, and host-specific paths are forbidden. Windows uses inbox `certutil`; Linux uses the ordinary `base64` utility. The decoded WVB, not its textual encoding alone, is the semantic test identity.

### Pin five envelope corruptions

Derive five fixtures once from the existing 174-byte return-42 WVB (`7933c4ba0cb854477a95750966f9532c2b9eb5888e55ec9ae64ebdf552a08f31`):

| Fixture | Exact change | Decoded bytes | Decoded SHA-256 |
| --- | --- | ---: | --- |
| `Bad-Magic` | XOR byte 0 with `0xFF` | 174 | `20618498d9df059d52fc0d660bf52f32df291c88b94d4b5ded224078f936108e` |
| `Bad-Version` | change byte 4 from format version 1 to 2 | 174 | `4f0cc323d4eb6713405a3e92f7c885b358aa1efc5fa08e2ecca7be7e17287614` |
| `Bad-Utf8` | change the first module-name byte at offset 25 to `0xFF` | 174 | `d7e26806542fc5a924193f7d42a229b8e86aaab78c7ba9f845f0b51dcc655c55` |
| `Truncated` | remove the final byte | 173 | `c795b6a811dac439e60775dfc1c48b23b6a594e203639bc660f974d41dc4073f` |
| `Trailing` | append one zero byte | 175 | `7121dfd48f36433738e81c9328e54b86ac3e26aa5a61c89f5bef6469df0481c1` |

Add `verify-failure|semantic`: the qualified native `wvverify` must return 1, write nothing to standard output, and write exactly `wvb status=Invalid phase=semantic` plus LF to standard error. A generic nonzero result or a different verifier phase is insufficient.

### Keep verification proportional

Review both host launchers, every fixed digest and expected channel, and the managed wrapper's exact report before execution. Run the direct Windows native plan once. Do not rebuild or rerun the managed wrapper, Standard, Qualification, OS, or GitHub gates. Linux execution remains part of the final grouped dual-host gate.

## Consequences

- The .NET-free plan now owns five representative malformed WVB envelope oracles in addition to five results and three runtime failures.
- The active path uses the already-qualified Windvale verifier and does not learn expected diagnostics from the candidate run.
- Version 2 was not qualified or distributed, so version 3 replaces it without a compatibility lane.
- Section-count/limit corruptions, typed-execution failures, control-reachability failures, randomized malformed modules, unsafe bytecode, WVO, linker, OS, bootstrap, and golden coverage remain in the managed evidence lane until separately transferred.

## Reconsideration triggers

Create a bounded Windvale-owned plan parser or fixture container when another input/expectation family would otherwise add duplicated branching to both host adapters. Preserve exact decoded identities and never make arbitrary host commands part of the plan format.
