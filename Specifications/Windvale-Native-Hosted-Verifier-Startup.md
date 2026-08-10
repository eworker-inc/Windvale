# Windvale native hosted-verifier startup

## Status and scope

This contract transfers compiler-verifier format-4 layout and startup
instantiation into portable Windvale. It accepts one exact 4,096-byte verifier
runtime header and the canonical startup WVO for the header's target, derives
the target layout and relocation addresses, and emits the existing `WVSD 1`
startup response.

It does not construct service code, the service bundle, runtime metadata,
platform headers, or the final PE/ELF. Those remain distinct admitted inputs or
later construction boundaries.

## Admission and layout

The runtime input must contain valid `WVHV 1` fixed verifier metadata at
offset 480 for target 1 (Windows x64) or 2 (Linux x64). The metadata must retain
container format 4 and ABI 22. Profiles 2 and 8 retain five capabilities and
six ordered services; profiles 6 and 7 retain those capabilities and eleven
ordered services. Every profile retains the canonical 2 MiB record arena and
canonical 128 MiB text arena. Profile 7 reserves two immutable input snapshots:
its snapshot table contains two 32-byte records, its name arena contains two
1 MiB strides, and its data arena contains two 4 MiB strides. Profiles 2, 6,
and 8 retain one snapshot and therefore retain their existing addresses and
emitted bytes.

Profile 7 has this exact runtime geometry, relative to the runtime base:

| Region | Offset or extent |
| --- | ---: |
| argument table | 4,096 |
| argument bytes | 5,168 |
| two-record snapshot table | 70,704 |
| record arena | 73,728 |
| text arena | 2,170,880 |
| name arena | 136,388,608 |
| data arena | 138,485,760 |
| file-input scratch | 146,874,368 |
| Windows runtime virtual bytes | 148,975,616 |
| Linux runtime virtual bytes | 147,927,040 |

Windvale derives all runtime table and arena addresses from that admitted
header. Service addresses are the format-4 bundle base, 8,192, plus each
admitted service placement. Windows layout retains 512-byte file alignment and
4,096-byte virtual alignment; Linux layout retains 4,096-byte file and virtual
alignment. A native entry offset of zero is valid.

The accepted objects are exact native-assembler products:

| Target | WVO bytes | Symbols | Relocations | SHA-256 |
| --- | ---: | ---: | ---: | --- |
| Windows x64 | 3,561 | 33 | 45 | `755ffb99cba6a838dd9eec353ce72d4adfb3af130ec4bce5a2278828dd136616` |
| Linux x64 | 1,925 | 20 | 24 | `08a7afefb69904af8d8c899a86bec76e957dfe255d397dbd9015d9acaa018ae8` |
| Windows x64, profiles 6 and 7 | 3,927 | 38 | 50 | `1bb785d5a06c40b91e45ebdc26b33ae33cb8ee7b244daffaa30ee59b9509edf3` |
| Linux x64, profiles 6 and 7 | 2,291 | 25 | 29 | `5d316c109b5c8964c019c44f96f42370408820c7db1ec278268cef541ba17ebb` |

The request owner maps each relocation ordinal to one semantic runtime,
service, native-entry, or Windows import address and supplies the unchanged
portable startup instantiator with one canonical `WVSI 1` request. It does not
patch machine code itself.

## Command contract

```text
wvhostverifierstartup <runtime.wvhr> <startup.wvo> <response.wvsd>
wvhostverifierstartup wvo-inspector <runtime.wvhr> <startup.wvo> <response.wvsd>
wvhostverifierstartup console-verifier <runtime.wvhr> <startup.wvo> <response.wvsd>
```

Success writes an exact `WVSD 1` response containing 1,275 Windows or 668 Linux
startup bytes for profiles 2 and 8, or 1,350 Windows or 743 Linux bytes for
profiles 6 and 7, reports `Valid`, and returns zero. Invalid
runtime, object, or response evidence reports `Rejected` with the failing
phase, returns 2, and preserves an existing output. Invalid invocation or any
input/output alias reports usage, returns 64, and preserves every input.

The application declares exactly `console.write_line`,
`diagnostic.write_line`, `file.read_bytes`, `file.write_bytes`,
`process.argument`, and `process.argument_count`.

## Exact tool identities and evidence

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Tool WVB | 75,731 | `bcc11d87ca487b1901d6f1d9d08529b8e5c0fe5dbe41989b279375dcb35515f7` |
| Windows application | 902,656 | `10d1b09780e860920ebf13f00205682c4ab06b89e8e01fd5b282682d75a6779d` |
| Linux application | 901,120 | `17579642f3038cee0bca1568ef33cfad7706e33296b7c151455c2111128b517b` |

The WVB builds through the native Project 1 front door. Both applications
reconstruct through the shared native hosted-container packager. The focused
current-host differential test compares the complete Windows and Linux startup
payloads with the frozen C# recovery oracle, rejects a changed WVO without
overwriting output, and rejects output aliasing. Before managed oracle deletion,
the grouped retirement gate must freeze equivalent structural and exact-byte
evidence that does not require C# at execution time.

Outer verifier platform-byte construction, final publication, independent
Linux execution, promotion, and the grouped retirement gate remain pending.
