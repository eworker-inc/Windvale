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
invalid result, and require the recipe's complete output identity. The public
launcher dispatches the three `memory*` roles and `loader` to separately owned
[memory-object](Windvale-Os-Memory-Object-Producer.md) and
[loader-object](Windvale-Os-Loader-Object-Producer.md) producers; the four
compact recipes remain in the original focused package.

## Recipe inventory

| Kind | Output | Bytes | SHA-256 |
| --- | --- | ---: | --- |
| `exceptions` | `09-exceptions.wvo` | 483 | `9caeb7ce353bca33e3bbac729ecca0423d59f8ce6b65ccd6b54fa53c381d617c` |
| `wvb-admission-bridge` | `12-wvb-admission-bridge.wvo` | 484 | `271c378b1f12bb4affa33474d865611cbf14e5b1b8996c703cb3d3cbe22eee7d` |
| `native-bridge-and-support` | `13-native-bridge-and-support.wvo` | 461 | `472a0fbe6497525e634a4785e92aa9ee62c3c7d70fff7510e45acbea644eea0b` |
| `paging` | `10-paging.wvo` | 1,292 | `a6bcad24e4752acc1fbab75d6667e965f2ab4d5613edd2c8e6cda244616fba2d` |
| `memory` | `08-memory.wvo` | 1,529 | `2668e17c3181e168415fb7bdee530873e2ddc8fa2d100af94bcc7b74909df3ed` |
| `memory-invalid-opcode` | `08-memory.wvo` | 1,545 | `09aa0fcfe12c561b79367cb26569dbc6f1f47ca3b98dc892426ca57b4328f868` |
| `memory-general-protection` | `08-memory.wvo` | 1,545 | `23a052f9d47a9416618c9b7a50a382c68c46d3bf7834410cc79f8fef2aa461e0` |
| `loader` | `00-loader.wvo` | 6,336 | `b310bc0e9aebc7b14c0892bb3dd4b833d42539c2194427a8f333b511d6af3804` |

The exception recipe is defined by the focused
[x64 exception-object contract](Windvale-X64-Exception-Object-Producer.md).
The admission bridge contains one 162-byte `.text.admission` section, exports
`Windvale_kernel_x64_wvb_admission`, imports
`Windvale_kernel_wvb_admit`, `Windvale_kernel_x64_native_probe`, and
`Windvale_kernel_x64_process_enter`, and carries relative-i32 relocations at
offsets 106, 125, and 148 with addend `-4`.

The native bridge and support recipe contains a 143-byte `.text.native` section
and a 23-byte `.text.support` section. It exports
`Windvale_kernel_x64_native_probe` and `Windvale_kernel_x64_write_byte`, imports
`Main` and `Windvale_kernel_main`, and carries relative-i32 relocations at
offsets 106 and 129 with addend `-4`.

The paging recipe contains one 899-byte `.text` section. It exports
`Windvale_kernel_x64_paging_install`, imports `Windvale_boot_probe`,
`Windvale_kernel_allocate_pages`, `Windvale_kernel_x64_page_protection_enable`,
and `Windvale_kernel_x64_page_table_activate`, and carries relative-i32
relocations at offsets 254, 306, 715, and 723 with addend `-4`.

The four compact recipes contain only reviewed architecture-specific code bytes and explicit
WVO records. Each complete candidate is admitted through the shared portable
WVO verifier before host publication. A new recipe requires its own exact ABI,
identity, focused cases, and normal-link evidence; the selector is not an open
extension or compatibility registry.

## Retained package identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| `Os-Probe-Object-Producer.wvb` | 42,835 | `ab26d2cd8820887fc15475a4ee29aaf884af9b5a0d8bd3313a847d00cc03e042` |
| Windows x64 application | 461,312 | `fcd22c975ed04534d30733c5ddabb7811a9b9578effd0d27839d171bdac76d0c` |
| Linux x64 application | 462,848 | `c4e22a9f67d5bdb4f186ddfbb63aa93032712ea7bdc260ed28076b12f0217e80` |

The nine-case fixed lane requires all six exact outputs plus independent WVO
admission, existing-destination preservation, unknown-kind rejection, and
invalid-extension rejection. These fixed native expectations remain executable
after the managed recovery generators are archived or removed.
