# Decision 0444: Native Probe 40 inner process-image WVA handoff

- Status: Implemented current-host normal-scenario cutover; Linux execution pending
- Date: 2026-08-09
- Advances: [Decision 0443](0443-Native-Probe-40-Top-Level-Wva-Assembly.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)
- Runbook: [Native tests](../Runbooks/Native-Tests.md#windvale-os-boot-execution)

## Context

Decision 0443 moved the three top-level Probe 40 WVA objects to the qualified
native assembler, but Stage 0 still assembled four WVA objects while composing
the process image: the init-service shim, directory-service shim, boot-resource
service stencil, and one scenario-selected client shim. All six possible inner
sources already produce exact canonical WVOs through the native assembler.

The managed process-image builder still owns compilation, native lowering,
structural checks, object adaptation, and three internal links. Replacing all
of those responsibilities together would obscure which boundary failed.

## Decision

- Define one focused immutable input record for the four process-image WVA
  objects used by a selected scenario.
- Preserve the default embedded-source C# assembler path unchanged as frozen
  recovery and differential evidence.
- Let the native-WVA inventory CLI accept an explicit process-WVA directory.
  Require exactly the three common WVO names plus the one name selected by the
  requested scenario; reject missing, extra, or differently named entries.
- Make the recovery command assemble the three common sources and selected
  client source through the digest-bound native assembler before Stage 0 runs.
  Admit every product by its exact SHA-256.
- Feed those immutable WVO bytes into the existing process-image checks and
  managed links. Do not weaken `Verifyˉshim`, boot-resource publication checks,
  image limits, entry lookup, or downstream object identities.
- Keep this handoff in a focused `Kernel-Process-Image-Wva.cs` owner rather than
  expanding the already broad process-image source.

## Evidence and consequences

The native assembler produces these exact inner objects:

| Source object | Bytes | SHA-256 |
| --- | ---: | --- |
| `Init-Resource-Service-Shim.wvo` | 2,118 | `52098aac184961fda7c3a23c8577851df6c18736555cb169b340d7b0c7249359` |
| `Directory-Process-Service-Shim.wvo` | 1,549 | `c0a7524130b8733ed17a3ce52fc04986cb449394c9ee509280120b86a3ed8c88` |
| `Boot-Resource-Service.wvo` | 462 | `fde44aad9549731d53c5ccf3a57733b3619df94369b61ef27a693e1059784bc9` |
| `Process-User-Shim.wvo` | 1,510 | `69ea7402a3a752e5c4b45689aeeb902b7e2ff1ce87a34bc9bad81417a3992fe6` |
| `Process-User-Fault-Shim.wvo` | 1,479 | `19c6b672873d86187e7588aadc0a485ec1f0ece9406529ad0fe045db9463b090` |
| `Process-Service-Fault-Shim.wvo` | 1,294 | `72f87e1b283cdb0d5dfc86149d749ec3e011f3a6e5e3da7397dce54d325bd27e` |

The Release solution build succeeds with zero warnings. The normal recovery
command completes in 23.1 seconds after natively assembling four inner and
three top-level objects. It reproduces the exact 683,008-byte EFI at SHA-256
`080b4d669e9a11fdc802bf7197ae5a044978b6ba39741b2b1c832296987f74d9`.
Its private WVA, object, linked-image, and EFI-candidate paths are removed.
An empty process-WVA directory is rejected with `WVOS2001`, exit code 1, and
zero destination entries.

No broad Seed, OS, QEMU, Qualification, Linux recovery, or non-normal recovery
scenario ran for this local cutover. Managed WVA assembly is no longer executed
by the normal recovery command. Managed source compilation, native lowering,
boot-resource object adaptation, three inner links, and remaining machine-code
and object production stay explicit for later slices.

## Reconsideration triggers

Reconsider the four-object handoff when inner linking moves to the native
linker, when process-image construction becomes a Windvale-owned tool, or when
a scenario changes one of the exact source identities. Any input name, source,
or digest change requires review before the recovery command accepts it.
