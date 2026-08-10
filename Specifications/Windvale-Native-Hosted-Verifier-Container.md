# Windvale native hosted-verifier container

## Status and scope

This contract joins the admitted format-4 fixed-verifier runtime, target
regions, instantiated startup code, and one complete service-bundle response
into an exact Windows PE or Linux ELF application. It is a portable Windvale
construction boundary. It does not lower WVB, create the four inputs, or define
durable filesystem replacement.

The constructor derives every destination offset and extent from the admitted
4,096-byte `WVHV` runtime. It accepts only the matching target's version-1
platform response and exact startup size. Every startup relocation must match
the shared runtime/import/service/native-entry target model, and the template
remaining after those fields are zeroed must match its canonical SHA-256. The
version-2 bundle response must contain one complete segment, the selected
profile's exact six or eleven services, and the exact declared image size.
The native fragment and every service body must match the SHA-256 evidence in
the runtime metadata; initial zero alignment and inter-service `0x90` padding
are also canonical.

Profile 7 uses the same eleven-service bundle and canonical inspector startup
objects as profile 6, but its admitted runtime header reserves two immutable
file snapshots. The composer does not infer that distinction from file size:
it requires profile 7 in the metadata and uses the profile-aware runtime
virtual extent and startup target map. Profiles 2, 6, and 8 retain their
existing placement and emitted bytes.

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
wvhostverifiercompose wvo-inspector <runtime.wvhr> <platform.wvhb> <startup.wvsd> <bundle.wvsi> <application>
wvhostverifiercompose console-verifier <runtime.wvhr> <platform.wvhb> <startup.wvsd> <bundle.wvsi> <application>
```

Success writes the exact application and returns zero. Rejection returns 2
without writing the destination. An output path equal to any input path reports
usage, returns 64, and preserves the input.

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Tool WVB | 91,072 | `679e7c0238b5e71a29dee2e52880e3390d68d33c29d7e9b631e8d90119894854` |
| Windows application | 1,359,872 | `c30ae33769bb7e4f7b901bd7294e2e0eefdd5c20bd2510d05e54d3f5dfa64374` |
| Linux application | 1,359,872 | `3cfcc9220c4e5b71bbe48613e33cddd8a7bbb06ab8719291bcfdecab5a10bb99` |

One focused current-host test compares both completed verifier applications
byte for byte with the frozen Stage 0 application contracts. On the current
Windows host it also executes the packaged composer and then the verifier it
produced, requires the canonical successful WVB report, and observes no CLR,
hostfxr, or hostpolicy module. The test additionally proves digest rejection
and destination preservation. The separate
[application-publisher contract](Windvale-Native-Hosted-Verifier-Application-Publisher.md)
now pins both completed outputs and reuses the native durable transaction;
independent Linux execution remains pending.
