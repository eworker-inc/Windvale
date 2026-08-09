# Windvale native hosted-verifier container

## Status and scope

This contract joins the admitted format-4 compiler-verifier runtime, target
regions, instantiated startup code, and one complete service-bundle response
into an exact Windows PE or Linux ELF application. It is a portable Windvale
construction boundary. It does not lower WVB, create the four inputs, or define
durable filesystem replacement.

The constructor derives every destination offset and extent from the admitted
4,096-byte `WVHV` runtime. It accepts only the matching target's version-1
platform response and exact startup size. Every startup relocation must match
the shared runtime/import/service/native-entry target model, and the template
remaining after those fields are zeroed must match its canonical SHA-256. The
version-2 bundle response must
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
| Tool WVB | 69,165 | `908dd3261d4075ee0f34a5976832e81f6bd16e742caf9469b48bcad43c773872` |
| Windows application | 1,088,000 | `a84e7aac58ce5d1f41ffb82efd0bf4c4fceb6cabdf9515d919a160a39e94a9ff` |
| Linux application | 1,089,536 | `b2d8f2a3fe23f974ee23c313840d14f195f0043a7a237c119d22ff7d2ae3d304` |

One focused current-host test compares both completed verifier applications
byte for byte with the frozen Stage 0 application contracts. On the current
Windows host it also executes the packaged composer and then the verifier it
produced, requires the canonical successful WVB report, and observes no CLR,
hostfxr, or hostpolicy module. The test additionally proves digest rejection
and destination preservation. Format-4 admission must join the existing native
console-application publisher before its durable transaction is reused;
independent Linux execution remains pending.
