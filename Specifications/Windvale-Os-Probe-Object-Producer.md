# Windvale OS Probe object producer

## Status and scope

This hosted Windvale tool constructs the small architecture-specific WVO recipes
that remain outside the compiler and WVA contracts during Probe 40 .NET
retirement. It uses the shared verified WVO constructor and accepts only named,
versioned recipes. It is not a general object editor, machine-code injector,
assembler, or replacement for the native compiler backend.

The retained host entry points are:

```text
Tools/Native/Produce-Os-Probe-Object.cmd <kind> <output.wvo>
Tools/Native/Produce-Os-Probe-Object.sh <kind> <output.wvo>
```

The launchers admit the exact host application, require a new `.wvo` destination
in an existing directory, reject an unknown exact kind, remove a newly created
invalid result, and require the recipe's complete output identity.

## Recipe inventory

| Kind | Output | Bytes | SHA-256 |
| --- | --- | ---: | --- |
| `exceptions` | `09-exceptions.wvo` | 483 | `9caeb7ce353bca33e3bbac729ecca0423d59f8ce6b65ccd6b54fa53c381d617c` |
| `wvb-admission-bridge` | `12-wvb-admission-bridge.wvo` | 484 | `271c378b1f12bb4affa33474d865611cbf14e5b1b8996c703cb3d3cbe22eee7d` |

The exception recipe is defined by the focused
[x64 exception-object contract](Windvale-X64-Exception-Object-Producer.md).
The admission bridge contains one 162-byte `.text.admission` section, exports
`Windvale_kernel_x64_wvb_admission`, imports
`Windvale_kernel_wvb_admit`, `Windvale_kernel_x64_native_probe`, and
`Windvale_kernel_x64_process_enter`, and carries relative-i32 relocations at
offsets 106, 125, and 148 with addend `-4`.

Both recipes contain only reviewed architecture-specific code bytes and explicit
WVO records. Each complete candidate is admitted through the shared portable
WVO verifier before host publication. A new recipe requires its own exact ABI,
identity, focused cases, and normal-link evidence; the selector is not an open
extension or compatibility registry.

## Retained package identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| `Os-Probe-Object-Producer.wvb` | 38,229 | `41696bba17570dda638abf9c0f58938950d8363b1f5044cb6dcf619b25d54cce` |
| Windows x64 application | 413,696 | `895237d4a651b4fb0a8a458a7bfa55f952c0364304d6e2af3f30fdc945ba5889` |
| Linux x64 application | 413,696 | `4c651c82379d3dc7f83781504182f33e3931b1b9e50a2574c23eb08faf3066bf` |

The five-case fixed lane requires both exact outputs plus independent WVO
admission, existing-destination preservation, unknown-kind rejection, and
invalid-extension rejection. These fixed native expectations remain executable
after the managed recovery generators are archived or removed.
