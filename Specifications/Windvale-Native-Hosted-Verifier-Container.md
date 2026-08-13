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
| Tool WVB | 91,386 | `a9a208994f3ff91051c61f308d3aa65a0e58cfab93c58be7401c87740fb48546` |
| Windows application | 1,362,432 | `988981a47d02fd7aa5b8dd27e1e4da42a16403e9d876b567c8633986a7902c65` |
| Linux application | 1,363,968 | `0e710d3d816cdc8df32c782fc1015ecd31e3c13cc9af563d4867ad0198085cb1` |

One focused current-host test compares both completed verifier applications
byte for byte with the frozen Stage 0 application contracts. On the current
Windows host it also executes the packaged composer and then the verifier it
produced, requires the canonical successful WVB report, and observes no CLR,
hostfxr, or hostpolicy module. The test additionally proves digest rejection
and destination preservation. The separate
[application-publisher contract](Windvale-Native-Hosted-Verifier-Application-Publisher.md)
now pins both completed outputs and reuses the native durable transaction;
independent Linux execution remains pending.
