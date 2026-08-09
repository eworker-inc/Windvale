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

The runtime input must contain valid `WVHV 1` compiler-verifier metadata at
offset 480 for target 1 (Windows x64) or 2 (Linux x64). The metadata must retain
container format 4, ABI 22, profile 2, five capabilities, six ordered services,
the canonical 2 MiB record arena, and the canonical 128 MiB text arena.

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

The request owner maps each relocation ordinal to one semantic runtime,
service, native-entry, or Windows import address and supplies the unchanged
portable startup instantiator with one canonical `WVSI 1` request. It does not
patch machine code itself.

## Command contract

```text
wvhostverifierstartup <runtime.wvhr> <startup.wvo> <response.wvsd>
```

Success writes an exact `WVSD 1` response containing 1,275 Windows startup
bytes or 668 Linux startup bytes, reports `Valid`, and returns zero. Invalid
runtime, object, or response evidence reports `Rejected` with the failing
phase, returns 2, and preserves an existing output. Invalid invocation or any
input/output alias reports usage, returns 64, and preserves every input.

The application declares exactly `console.write_line`,
`diagnostic.write_line`, `file.read_bytes`, `file.write_bytes`,
`process.argument`, and `process.argument_count`.

## Exact tool identities and evidence

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Tool WVB | 64,198 | `36b6b638f14e0ecaff7ad8934ce785e22f9fa3d6c3cd5dcd91cfc98a4fa569d6` |
| Windows application | 701,440 | `4ae0435d62a1576f3b33573dd4402704ee35e4745dc2897f5dca1659899fd268` |
| Linux application | 700,416 | `04eeb622fdcfc0eec5fa030a1529657c6333a7842bb3244065959e40c7fc5c86` |

The WVB builds through the native Project 1 front door. Both applications
reconstruct through the shared native hosted-container packager. The focused
current-host differential test compares the complete Windows and Linux startup
payloads with the frozen C# recovery oracle, rejects a changed WVO without
overwriting output, and rejects output aliasing. Before managed oracle deletion,
the grouped retirement gate must freeze equivalent structural and exact-byte
evidence that does not require C# at execution time.

Outer verifier platform-byte construction, final publication, independent
Linux execution, promotion, and the grouped retirement gate remain pending.
