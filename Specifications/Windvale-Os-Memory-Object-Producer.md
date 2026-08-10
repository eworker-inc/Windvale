# Windvale OS memory-object producer

## Status and scope

This hosted Windvale tool constructs the exact Probe 40 memory objects for the
normal, invalid-opcode, and general-protection scenarios. It is a focused
replacement for those frozen Stage 0 emissions, not a general x64 assembler or
a definition of kernel-memory semantics. The scenario-aware recovery
implementation remains the independent provenance lane until the final
.NET-retirement gate is qualified.

The public entry points remain the unified OS Probe object launchers:

```text
Tools/Native/Produce-Os-Probe-Object.cmd memory <output.wvo>
Tools/Native/Produce-Os-Probe-Object.cmd memory-invalid-opcode <output.wvo>
Tools/Native/Produce-Os-Probe-Object.cmd memory-general-protection <output.wvo>
Tools/Native/Produce-Os-Probe-Object.sh memory <output.wvo>
Tools/Native/Produce-Os-Probe-Object.sh memory-invalid-opcode <output.wvo>
Tools/Native/Produce-Os-Probe-Object.sh memory-general-protection <output.wvo>
```

They require a new `.wvo` path in an existing directory, bind the exact native
package and scenario code-fixture identities, remove a newly created invalid
result, and require the complete output identity.

## Scenario contract

| Scenario | Code bytes | Object bytes | Object SHA-256 |
| --- | ---: | ---: | --- |
| normal | 1,089 | 1,529 | `2668e17c3181e168415fb7bdee530873e2ddc8fa2d100af94bcc7b74909df3ed` |
| invalid-opcode | 1,105 | 1,545 | `09aa0fcfe12c561b79367cb26569dbc6f1f47ca3b98dc892426ca57b4328f868` |
| general-protection | 1,105 | 1,545 | `23a052f9d47a9416618c9b7a50a382c68c46d3bf7834410cc79f8fef2aa461e0` |

Every object contains one `.text` section aligned to 16 bytes. Normal exports
`Windvale_kernel_allocate_pages` at offset 832 and
`Windvale_kernel_memory_enter` at offset 0 with size 826. Invalid-opcode uses
allocator offset 848 and enter size 834; general-protection uses allocator
offset 848 and enter size 844. The allocator size remains 257.

All three import `Windvale_kernel_allocate_memory_object`,
`Windvale_kernel_wva_main`, `Windvale_kernel_x64_exception_install`, and
`Windvale_kernel_x64_paging_install`. Four relative-i32 relocations use addend
`-4`: offset 698 targets the page allocator export; offsets 731, 748, and 766
target exception installation, paging installation, and the WVA entry
respectively. Construction uses the shared verified WVO constructor, and each
complete result is independently admitted before publication.

The reviewed architecture fixtures are:

| Fixture | Bytes | SHA-256 |
| --- | ---: | --- |
| `normal-x64-memory.bin` | 1,089 | `07d2508132456706d8718a0bc9a54cf9b0228afbb61aec8e66ce92d34cf5e803` |
| `invalid-opcode-x64-memory.bin` | 1,105 | `f350059d181b4a640ab03734807243348bcaca723484b1fe093767e4d042ea18` |
| `general-protection-x64-memory.bin` | 1,105 | `69f31f4fc8a08bea9202e4accc6101101103ea83ee213f4b4f8f51202655e049` |

## Retained package identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| `Os-Probe-Memory-Object-Producer.wvb` | 37,517 | `1971e87f8c9931e914e7f7505d4fef213be5b3e6b1d38b0324ffc030be1b7e60` |
| Windows x64 application | 404,992 | `5437c508012d726e8bd6fb79d0942548d615f9ea52348c97b54038ab643d83c4` |
| Linux x64 application | 405,504 | `1ea358f8cc77b36201b22ff820ef6fd000b4bbd48342dfe6eed994e487a15c7b` |

The three fixtures preserve exact reviewed machine-code provenance without
making C# part of the ordinary path. The user-fault and service-fault recovery
scenarios are not claimed by this candidate.
