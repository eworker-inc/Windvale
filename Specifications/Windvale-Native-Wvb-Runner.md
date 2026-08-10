# Windvale native WVB-runner reconstruction

## Status and scope

The profile-5 WVB runner is a current-host-focused native candidate. It admits
the fixed portable `Main() -> i32` execution subset and binds five capabilities
to nine ordered services. The exact candidate reconstructs from a retained WVB
through the Windvale-native lowerer, linker, hosted-verifier profile, and paired
Windows/Linux container materializers.

Source-to-WVB reconstruction remains open: the native Project front door
currently rejects this project at the source-binding boundary. The durable
constructor therefore begins from the exact retained 90,009-byte WVB rather
than claiming source reconstruction.

## Exact products

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| WVB runner | 90,009 | `3b881147e5e6c8298cf249e6e02c9f18ed4a677d49ef0a307427465795a1c626` |
| ABI-22 WVO | 761,854 | `e92eed5006a7a98609173c0ed73e66a7aec5e152d8556c9174cab928b946a505` |
| linked fragment | 761,278 | `d602b50d9057f0aad1bb7dca32e624cf78a78244e53ec1a053455caf66a02212` |
| Windows application | 778,240 | `578ddd302da5fbd8d8e14c9410787f5aa05378429a1aca738ee2057e2f9ac1a5` |
| Linux application | 778,240 | `16f39270c239609c6f58b086d0648609fad46860ba9bdd198fa7e6668b628047` |

The WVO contains 761,120 text bytes and 158 read-only-data bytes. Linking at
base zero selects `Main` at address 10,049.

## Construction and execution

The paired constructors accept one existing output directory:

```text
Tools\Native\Construct-Wvb-Runner-Reconstruction.cmd <existing-output-directory>
./Tools/Native/Construct-Wvb-Runner-Reconstruction.sh <existing-output-directory>
```

They reject the live candidate directory, bind both tool inventories and every
artifact digest, lower and link once, assemble both inspector startup objects,
then construct profile-5 Windows and Linux applications. Success reports:

```text
native WVB runner reconstruction status=Complete artifacts=4
```

`Run-Wvb.cmd` and `Run-Wvb.sh` execute the corresponding digest-bound candidate.
The three-case fixed owner proves exact candidate inventory, paired
reconstruction, and current-host result/rejection behavior. The Windows owner
passes 3/3 and returns the exact `Result: 42` report for the canonical fixture.

## Evidence boundary

Profile 5 intentionally omits enum-name and text-quote. Its startup request is
the only profile allowed to encode those two exact target positions as absent;
all other relocation targets and all other profiles remain nonzero.

This is retained-WVB, current-Windows-host cross-target construction. It is not
source-to-WVB closure, independent Linux execution, clean or previous-release
bootstrap, grouped qualification, artifact promotion, or recovery deletion.
