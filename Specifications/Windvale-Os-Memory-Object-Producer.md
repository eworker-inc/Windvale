# Windvale OS memory-object producer

## Status and scope

This hosted Windvale tool constructs the exact normal-scenario Probe 40
`08-memory.wvo`. It is a focused replacement for the frozen Stage 0 emitter,
not a general x64 assembler or a definition of kernel-memory semantics. The
scenario-aware recovery implementation remains the independent provenance lane
until the final .NET-retirement gate is qualified.

The public entry points remain the unified OS Probe object launchers:

```text
Tools/Native/Produce-Os-Probe-Object.cmd memory <output.wvo>
Tools/Native/Produce-Os-Probe-Object.sh memory <output.wvo>
```

They require a new `.wvo` path in an existing directory, bind the exact native
package identity, remove a newly created invalid result, and require the complete
output identity.

## Object contract

The object is 1,529 bytes at SHA-256
`2668e17c3181e168415fb7bdee530873e2ddc8fa2d100af94bcc7b74909df3ed`.
It contains one 1,089-byte `.text` section aligned to 16 bytes.

It exports `Windvale_kernel_allocate_pages` at offset 832 with size 257 and
`Windvale_kernel_memory_enter` at offset 0 with size 826. It imports
`Windvale_kernel_allocate_memory_object`, `Windvale_kernel_wva_main`,
`Windvale_kernel_x64_exception_install`, and
`Windvale_kernel_x64_paging_install`.

Four relative-i32 relocations use addend `-4`: offset 698 targets the page
allocator export; offsets 731, 748, and 766 target exception installation,
paging installation, and the WVA entry respectively. Construction uses the
shared verified WVO constructor, and the complete result is independently
admitted before publication.

## Retained package identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| `Os-Probe-Memory-Object-Producer.wvb` | 37,769 | `2ae5f3a2f108b74a86150854c78f7f2dc0335cff2cb1e071be7718fce40e17e7` |
| Windows x64 application | 399,872 | `79461480b72cc1865278ea6f06170b8f4e9f4e849898d7b3c06aa3d36ff70032` |
| Linux x64 application | 401,408 | `02280b115ead806f8b6e2f1dd066d7d06a85ae571d790c66d05daecf2acc6554` |

The 158-line source is separate from the 317-line compact-recipe producer so
the public command stays cohesive without creating a growing catch-all source
file. Other recovery scenarios are not claimed by this normal-image candidate.
