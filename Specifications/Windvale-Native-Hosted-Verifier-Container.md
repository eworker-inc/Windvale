# Windvale native hosted-verifier container

## Status and scope

This contract joins the admitted format-4 compiler-verifier runtime, target
regions, instantiated startup code, and one complete service-bundle response
into an exact Windows PE or Linux ELF application. It is a portable Windvale
construction boundary. It does not lower WVB, create the four inputs, or define
durable filesystem replacement.

The constructor derives every destination offset and extent from the admitted
4,096-byte `WVHV` runtime. It accepts only the matching target's version-1
platform response and exact startup size. The version-2 bundle response must
contain one complete segment, six services, and the exact declared image size.
The native fragment and every service body must match the SHA-256 evidence in
the runtime metadata; initial zero alignment and inter-service `0x90` padding
are also canonical.

## Placement

For Windows, the constructor places the PE header at file offset zero, startup
at the text-file offset, the bundle at text offset 4,096, imports at the
declared data-file offset, the runtime at data offset 4,096, and the relocation
block at its declared file offset. All other allocated bytes remain zero.

For Linux, it places the ELF header page at zero, startup at file offset 4,096,
the bundle at 8,192, and the runtime at the declared data-file offset. All
alignment bytes remain zero.

## Command and identities

```text
wvhostverifiercompose <runtime.wvhr> <platform.wvhb> <startup.wvsd> <bundle.wvsi> <application>
```

Success writes the exact application and returns zero. Rejection returns 2
without writing the destination. An output path equal to any input path reports
usage, returns 64, and preserves the input.

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Tool WVB | 53,900 | `78973e37b7baa2ab5befd83bfa8df5b6676e40ef58a218ffe7a7c7ce4e53a5fe` |
| Windows application | 822,784 | `8394b3a76ed26401ac3c1b127dc548488d98d1af7295079feadf92fc5059ce1a` |
| Linux application | 823,296 | `e501594c90a2f8c0c2d3c4528aef2bafa1fff437af6f6320a276c5dc3df1e66c` |

One focused current-host test compares both completed verifier applications
byte for byte with the frozen Stage 0 application contracts. It also proves
digest rejection and destination preservation. The existing segmented native
publisher remains the durable publication owner; pipeline wiring and
independent Linux execution remain pending.
