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
through eight canonical fragments, with every nonfinal fragment exactly 4 MiB,
plus a validated decimal entry. Both modes support the seven compiler-family
hosted profiles `1` through `7`; they do not alias the separately specified
read-only verifier profiles. Both modes construct the established ten-service
Windows x64 or Linux x64 container. `Package-Segmented-Compiler-Wvb` composes native WVB staging, image
linking, canonical transport, and this image-input mode without asking a host
script to decode a Windvale format.

## Commands

```text
Tools\Native\Package-Hosted-Wvb.cmd <profile> <input.wvb> <output.exe>
./Tools/Native/Package-Hosted-Wvb.sh <profile> <input.wvb> <output.elf>
Tools\Native\Package-Hosted-Wvb.cmd <profile> <input.wvb> <output.exe|output.elf> [windows|linux]
./Tools/Native/Package-Hosted-Wvb.sh <profile> <input.wvb> <output.elf|output.exe> [linux|windows]
Tools\Native\Package-Hosted-Wvb.cmd image <profile> <input.wvb> <chunk-prefix> <fragment-count> <entry> <output.exe>
./Tools/Native/Package-Hosted-Wvb.sh image <profile> <input.wvb> <chunk-prefix> <fragment-count> <entry> <output.elf>
Tools\Native\Package-Segmented-Compiler-Wvb.cmd <profile> <input.wvb> <output.exe>
./Tools/Native/Package-Segmented-Compiler-Wvb.sh <profile> <input.wvb> <output.elf>
```

Omitting the optional target preserves the original current-host behavior.
Supplying `windows` or `linux` selects that target's startup object and fixed
service leaves while the invoking host's native Windvale tools still construct
the result. The output suffix must match the selected target. Wrong argument
count, invalid profile or target, or wrong suffix returns 64. Any failed
digest, native child, status-line admission, or publication returns nonzero and
does not treat a rejected segment request as loop completion.

Before lowering begins, the command verifies the exact `SHA256SUMS` inventory,
all 57 tool artifacts, nine target-specific fixed service leaves, and the
target startup WVO. The inventory covers 19 WVBs and their paired Windows/Linux
applications. Its exact identity is:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| `SHA256SUMS` | 5,426 | `f674de96634840c42cecd77d3af34de87e2c06458dae3a36577f18da83c5f99d` |

The candidate manifest records the source project and target family for every
command. The WVBs reconstruct through the digest-bound native Project 1 front
door. The 57 tool artifacts themselves retain explicit Stage 0 recovery
provenance until this complete toolset is promoted. Their digest-bound native
processes now construct either target container without calling that recovery
path.

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

`Test-Segmented-Compiler-Packaging` additionally exercises image mode with the
current two-fragment WVB-to-WVO lowerer. It requires the exact current host
application identity, the descriptor-returning `Main` differential WVO, and
byte-for-byte reproduction of the retained baseline-JIT bridge WVO.

The Windows candidate composes `wvhostcontrol.wvb` into a 236,032-byte PE with
SHA-256 `d8b10130bc946261526ee0accc9fcbd42dbe2a5d9fd3e4d4f349038550c8c559`,
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
