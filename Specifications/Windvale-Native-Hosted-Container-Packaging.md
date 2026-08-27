# Windvale native hosted-container packaging

## Status and scope

This contract composes the existing Windvale-native hosted-container producers
into one digest-bound WVB-to-application command. The platform
script owns only exact tool acquisition, ordered child-process execution,
bounded decimal loop control, private temporary names, and cleanup. Windvale
processes continue to own every binary format, source transformation,
admission decision, digest, segment, and publication transaction.

The ordinary compiler-family candidate accepts a WVB whose lowered native fragment fits in one
nonempty resource of at most 4 MiB. The explicit image-input mode accepts one
through sixteen canonical fragments, with every nonfinal fragment exactly 4 MiB,
plus a validated decimal entry. Both modes support the seven compiler-family
hosted profiles `1` through `7`; they do not alias the separately specified
read-only verifier profiles. Both modes construct the established ten-service
Windows x64 or Linux x64 container. `Package-Segmented-Compiler-Wvb` composes native WVB staging, image
linking, canonical transport, and this image-input mode without asking a host
script to decode a Windvale format.

Profiles `1` through `6` retain the 64,000,000,000-instruction execution
ceiling. Profile `7`, used by the split analyzer and emitter reconstruction,
has a 137,438,953,472-instruction (`2^37`) ceiling and a
234,881,024-byte (224 MiB) dynamic text/byte arena. Unlike profiles `2` and `6`,
profile `7` retains the 1 MiB per-name stride required by the compiler source
closure rather than selecting their compact 8 KiB stride.
The higher bounds are profile-scoped:
the earlier measured 1.73 MiB emitter source closure exhausted 64,000,000,000
during analysis; the 1,931,188-byte compiler closure subsequently left no
usable headroom under 80,000,000,000; and the exact enum-backing compiler
closure exhausted 120,259,084,288 instructions before reaching text-arena
exhaustion under a later 124,554,051,584-instruction probe. Ordinary hosted
tools have no measured reason to inherit either capacity or change their
established application bytes.

## Commands

```text
Tools\Native\Package-Hosted-Wvb.cmd <profile> <input.wvb> <output.exe>
./Tools/Native/Package-Hosted-Wvb.sh <profile> <input.wvb> <output.elf>
Tools\Native\Package-Hosted-Wvb.cmd <profile> <input.wvb> <output.exe|output.elf> [windows|linux]
./Tools/Native/Package-Hosted-Wvb.sh <profile> <input.wvb> <output.elf|output.exe> [linux|windows]
Tools\Native\Package-Hosted-Wvb.cmd image <profile> <input.wvb> <chunk-prefix> <fragment-count> <entry> <output.exe>
./Tools/Native/Package-Hosted-Wvb.sh image <profile> <input.wvb> <chunk-prefix> <fragment-count> <entry> <output.elf>
Tools\Native\Package-Segmented-Compiler-Wvb.cmd <profile> <input.wvb> <output.exe> [--development-cache]
./Tools/Native/Package-Segmented-Compiler-Wvb.sh <profile> <input.wvb> <output.elf> [--development-cache]
Tools\Native\Construct-Segmented-Compiler-Toolset.cmd <existing-separate-output-directory>
./Tools/Native/Construct-Segmented-Compiler-Toolset.sh <existing-separate-output-directory>
pwsh -NoProfile -File Tools/Native/Measure-Segmented-Compiler-Packaging.ps1 -InputWvb <input.wvb> [-Profile <1-through-7>]
```

Omitting the optional target preserves the original current-host behavior.
Supplying `windows` or `linux` selects that target's startup object and fixed
service leaves while the invoking host's native Windvale tools still construct
the result. The output suffix must match the selected target. Wrong argument
count, invalid profile or target, or wrong suffix returns 64. Any failed
digest, native child, status-line admission, or publication returns nonzero and
does not treat a rejected segment request as loop completion.

The segmented command stages, links, and transports the compiler image freshly
by default and when `--development-cache` is present. The optional flag applies
Decision 0554's complete content-addressed checkpoint only to final hosted
application construction. A hit revalidates the complete key, cached manifest,
application bytes, digest, and executable mode before materializing a fresh
destination. Invalid entries fail closed. The default path, reconstruction
owners, GitHub shards, and qualification remain cache-independent.

`Measure-Segmented-Compiler-Packaging.ps1` creates an isolated empty cache,
requires a `Created` run followed by a `Hit`, compares their complete application
bytes, and reports cold and warm elapsed time as JSON. It is measurement tooling,
not a product-performance threshold or qualification shortcut.

The initial Windows x64 profile-7 measurement used a 950,265-byte source compiler
WVB. Cold construction took 270.088 seconds and warm construction took 101.729
seconds, a 2.66 times speedup. Both paths produced the same 28,313,600-byte
application with SHA-256
`1ee8066b91834bdd1d943a34c5bee9dd8e78aba0abf6015bdc8d478ad3a10c2e`.

Before lowering begins, the command verifies the exact `SHA256SUMS` inventory,
all 72 tool artifacts, nine target-specific fixed service leaves, and the
target startup WVO. The inventory covers 24 WVBs and their paired Windows/Linux
applications. Its exact identity is:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| `SHA256SUMS` | 6,927 | `b15800d907e46c866292302a989584b9825a0594494a529ca96578dab686cb35` |

The candidate manifest records the source project and target family for every
command. The WVBs reconstruct through the digest-bound native Project 1 front
door. The 72 tool artifacts themselves retain explicit Stage 0 recovery
provenance until this complete toolset is promoted. Their digest-bound native
processes now construct either target container without calling that recovery
path.

Decision 0862 consolidates the compiler-scale `wvhostrequest` and
`wvhostsources` applications into this canonical inventory. Their source WVBs
and public behavior are unchanged; the replacement native applications retain
the larger compiler-scale hashing and source-set capacity. Packaging no longer
depends on a parallel ignored overlay directory. The unified toolset reproduces
the current analyzer, emitter, and 445,196-byte WVB runner exactly.

Decision 0833 rebuilds the nine compiler-family WVBs and both target application
families after the profile-7 geometry change. Eight application families use
the ordinary retained native path. `wvhostpublish` is intentionally different:
its Windvale WVB is the read-only admission core, while its platform application
also contains the immutable atomic-publication shell. That application was
reconstructed in an isolated checkout of immutable release
`stage0-recovery-e5a1a7473c57`, after verifying commit
`e5a1a7473c57935c5dfcf09b78b18c3c099e70ef`, tree
`9950150f14cd4864b06c853ab6a716fa6e04495a`, and the release manifests. Only the
resulting Windows/Linux binary products enter this candidate; no managed source,
direct `dotnet` entry point, or recovery workspace returns to `main`.

The separate segmented-toolset constructor builds the staging producer,
compiler-image staging, and canonical transport WVBs through the native source
front door, then feeds each WVB through the retained segmented candidate to
construct its paired Windows and Linux applications. It writes only to an
existing directory distinct from the checked-in candidate and admits all nine
outputs against exact size and SHA-256 identities before reporting completion.
This is current-Windows-host cross-target self-reconstruction: the retained
candidate remains an input seed, so this command is not a non-circular
bootstrap or independent Linux-host qualification.

The reconstruction owner also builds the current compiler build-driver WVB into
private temporary storage through the explicitly unqualified current-host
candidate driver and feeds it to the reconstructed current-host staging
producer. The exact 1,259,719-byte WVB at SHA-256
`3e84e6dc8e646f7cde061e21fdbff7850e83e9faa83114d810b70297a445f949`
stages into 32,003,453 object bytes across 40 chunks plus a 504-byte manifest.
The qualified semantic-freeze front door and retained candidate identities are
not repinned. This case does not execute or promote the private compiler WVB;
general verifier, publisher, final-container, and front-door promotion remain
separate boundaries.

## Ordered path

After the existing native lowerer and linker produce the raw fragment, or after
image mode accepts its canonical fragment inputs, the command executes the
fixed-service and enum-service producers, source geometry,
publication planning, orchestration control, metadata request and construction,
runtime header, container plan, platform bytes, and startup. It then obtains
the admitted service-bundle count, constructs every request/response pair,
builds the final source set, obtains the admitted application-segment count,
constructs every final request/response pair and manifest, and calls the native
atomic publisher.

The enum-service resource index is `fragment count + 6`, immediately after the
canonical fragments and services 1 through 6. The scripts parse only bounded
decimal status values, including segment counts and the native-linker `Main`
address. They do not decode `WVOP`, `WVLI`, `WVSG`, `WVHS`, `WVPQ`, `WVHM`,
`WVCD`, `WVSI`, `WVSQ`, `WVHT`, or `WVHU`.

## Private lifecycle and failure behavior

Every invocation allocates a fresh unpredictable directory under the host
temporary root. All intermediate names remain below that directory. Cleanup is
guarded to that exact private path and runs after success or failure. The final
publisher preserves an existing destination unless complete segment-set
admission and the durable platform transaction succeed.

## Focused verification

```text
Tools\Native\Test-Hosted-Wvb-Packaging.cmd
./Tools/Native/Test-Hosted-Wvb-Packaging.sh
```

Decision 0492 reconstructs the complete Decision 0491 candidate toolset and
repins both launchers to its 6,927-byte, 72-entry inventory. The focused Windows
owner below is current-source evidence; independent Linux execution and grouped
qualification remain pending.

Decision 0496 uses that retained toolset to reconstruct the three segmented
process WVBs and all six target applications. The nine admitted identities are
construction evidence only; Stage 2, execution of the Linux applications,
promotion, seed independence, and the grouped dual-host gate remain separate.

Each host test packages the pinned orchestration-control WVB and requires exact
equality with the corresponding independent candidate, then cross-constructs
the opposite target and requires exact equality with its candidate. It finally
supplies a fixed invalid `.wvb`, requires rejection, and verifies that both the
input and a pre-existing destination remain byte-identical. The test redirects
the launcher's temporary root into its own private directory and rejects any
remaining package scratch. The verifier-request WVB is also packaged for both
targets and compared with the independent candidates. These five cases are the
focused composition check; they do not replace malformed-format suites or the
final qualification gate.

On Linux, the shared native durable transaction creates the private hosted
application sibling with its exact final `0755` mode before writing, flushing,
rereading, and atomically replacing the destination. Staged WVO publication
uses the same transaction with `0600`. The launchers do not apply a later
`chmod`, so successful replacement cannot expose an intermediate wrong mode.

`Test-Segmented-Compiler-Toolset-Reconstruction` calls the durable segmented
toolset constructor once and verifies three family cases: each exact WVB plus
its paired Windows and Linux applications. `Test-Segmented-Compiler-Packaging`
is a compatibility entry point for that same owner; it no longer couples this
lane to current-lowerer or baseline-JIT differential evidence.

The Windows candidate composes `wvhostcontrol.wvb` into a 236,032-byte PE with
SHA-256 `2483ec3c219f63cf6d16e114fcc8d7ef563296b5b7dea4d9b370c914d8b94362`,
byte-for-byte equal to the independently constructed candidate. The Linux
candidate is a 237,568-byte ELF with SHA-256
`45c8bf1163556c851db8b7fecb2556e899c816d06bd39209d65db942fea3c44a`.
Decision 0492's focused Windows run passes all five current cases. Earlier
GitHub run
[`31286313268`](https://github.com/eworker-inc/Windvale/actions/runs/31286313268)
passed the preceding candidate on genuine Windows and Debian hosts. After
Decision 0422 added the exact atomic Linux executable-mode policy, run
[`31290136463`](https://github.com/eworker-inc/Windvale/actions/runs/31290136463)
passed both ordinary cases and both then-current lowerer segmented cases on
genuine Windows and Debian hosts. Those runs remain historical evidence and do
not qualify the Decision 0492 identities. Artifact promotion, ordinary-path
cutover, and the grouped dual-host retirement gate remain pending.

Decision 0438 adds explicit cross-target selection. Decision 0492's focused
Windows command passes 5/5 and constructs the current Linux orchestration-control
and verifier-request ELFs byte for byte without .NET. Independent execution of
the current cross-target form on Linux remains part of the next paired-host
evidence rather than being inferred from byte equality.
