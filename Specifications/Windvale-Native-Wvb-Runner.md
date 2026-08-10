# Windvale native WVB-runner reconstruction

## Status and scope

The profile-5 WVB runner is a current-host-focused native candidate. It admits
the fixed portable `Main() -> i32` execution subset and binds five capabilities
to nine ordered services. The exact candidate reconstructs from the complete
Project 1 source closure through the Windvale-native compiler, lowerer, linker,
hosted-verifier profile, and paired Windows/Linux container materializers.

The project names its root tool plus the SHA-256, scalar-interpreter, envelope,
and formatting dependencies in canonical module order. Project paths are
relative to the manifest; this contract does not require all `.wvproj` files to
live at the repository root. Component-local manifests remain appropriate, and
a future workspace/index contract may improve discovery without changing
Project 1 semantics.

## Exact products

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| WVB runner | 121,593 | `5042a57e3281621ee126a64cadef70834800524de60ed0521cedba043bd271f1` |
| ABI-22 WVO | 1,078,577 | `118cdd634026d7d616f3b7c7dc951176985e725f5852b4d3b045aab4cf5e5ca5` |
| linked fragment | 1,077,675 | `cb9b08b1d88cc67fa26f210832cbdc542df51d2eb8816ab5ef2a7fc296f426ec` |
| Windows application | 1,094,656 | `ab0c2384ecdfd07bc7351562732ae4b1f97e07dcbd2c92e96dc8cb3dee4d3ff7` |
| Linux application | 1,093,632 | `ffc0ad10e0e1dcffc8344bb040885535f5ab67a50cbebb1980c980888c1b5322` |

The WVO contains 1,077,216 text bytes and 459 read-only-data bytes, with 18
symbols and 13 relocations. Linking at base zero selects `Main` at address
14,790.

## Construction and execution

The paired constructors accept one existing output directory:

```text
Tools\Native\Construct-Wvb-Runner-Reconstruction.cmd <existing-output-directory>
./Tools/Native/Construct-Wvb-Runner-Reconstruction.sh <existing-output-directory>
```

They reject the live candidate directory, bind both tool inventories and every
artifact digest, build the WVB from its source project, lower and link once,
assemble both inspector startup objects, then construct profile-5 Windows and
Linux applications. Success reports:

```text
native WVB runner reconstruction status=Complete artifacts=4
```

`Run-Wvb.cmd` and `Run-Wvb.sh` execute the corresponding digest-bound candidate
with either one module argument or the exact optional `--report-steps` flag.
Default output remains `Result: <i32>`. Reporting adds one
`Instructions: <u32>` line; the canonical Sum fixture reports result `29` and
exactly `203` instructions.

The three-case fixed owner proves exact candidate inventory, source-built
paired reconstruction, current-host result and instruction reporting, invalid
option rejection, malformed-module rejection, and input preservation. The
Windows owner passes 3/3 in 50.1 seconds. The paired nine-case native Seed
front-door helper passes on Windows in 3.6 seconds.

## Evidence boundary

Profile 5 intentionally omits enum-name and text-quote. Its startup request is
the only profile allowed to encode those two exact target positions as absent;
all other relocation targets and all other profiles remain nonzero.

The feature-frozen Stage 0 compiler remains a recovery and differential owner,
not the current product oracle. For this source closure it emits a distinct
126,271-byte WVB with SHA-256
`00b87804c047b626b00c167bf99ea9834bc77ab8e88e454d39a738b2787e2bcf`,
which the current native semantic verifier rejects. The native Project front
door emits the compiler-aligned product pinned above. That expected divergence
does not weaken the exact native reconstruction contract.

This is current-Windows-host source-to-WVB and cross-target construction. It is
not independent Linux execution, a clean or previous-release bootstrap,
complete capability-bearing execution, per-function profiling, grouped
qualification, artifact promotion, or recovery deletion.
