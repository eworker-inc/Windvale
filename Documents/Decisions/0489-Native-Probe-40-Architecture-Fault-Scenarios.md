# Decision 0489: Native Probe 40 architecture-fault scenarios

- Status: Implemented current-host native-build candidate; Linux and grouped qualification pending
- Date: 2026-08-10
- Advances: [Decision 0452](0452-Native-Probe-40-Memory-Object-Producer.md), [Decision 0435](0435-Digest-Bound-Os-Boot-Execution.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contracts: [OS memory-object producer](../../Specifications/Windvale-Os-Memory-Object-Producer.md), [OS Probe object producer](../../Specifications/Windvale-Os-Probe-Object-Producer.md), and [native retirement suite](../../Specifications/Windvale-Native-Retirement-Test-Suite.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)

## Context

The ordinary Probe 40 builder reproduced only the normal image. The supplied
image boot verifier already owned five scenario contracts, but invalid-opcode
and general-protection still required the Stage 0 object constructor even
though both differed from normal only in `08-memory.wvo`. User-fault and
service-fault also vary the protected-process composition and remain a larger
independent slice.

The Stage 0 recovery oracle shows that every non-memory WVO is byte-identical
across the normal and two architecture-fault scenarios. It produces exact
memory objects of 1,529, 1,545, and 1,545 bytes respectively. Keeping that
variation behind one focused producer avoids parallel object models and does
not require new source-language semantics or C# in the ordinary path.

## Decision

- Retain three digest-bound x86-64 memory-code fixtures: normal,
  invalid-opcode, and general-protection.
- Make the existing hosted Windvale memory-object producer accept the closed
  `memory`, `memory-invalid-opcode`, and `memory-general-protection` roles.
  It constructs all three through the shared verified WVO constructor.
- Keep the one-argument native Probe 40 build command as normal. Accept one
  optional exact scenario argument for invalid-opcode or general-protection.
  Select only the memory role; keep all other source, object, link, and package
  inputs unchanged.
- Expand the object-producer owner from 9 to 11 cases and the image owner from
  2 to 4 cases. The latter constructs all three exact images and retains the
  existing-output preservation case.
- Keep user-fault and service-fault out of this slice because they require
  distinct process-object construction rather than only the architecture
  memory entry.

## Evidence and consequences

The native producer matches the independent Stage 0 oracle byte for byte:

| Scenario | Memory WVO bytes | Memory WVO SHA-256 | EFI bytes | EFI SHA-256 |
| --- | ---: | --- | ---: | --- |
| normal | 1,529 | `2668e17c3181e168415fb7bdee530873e2ddc8fa2d100af94bcc7b74909df3ed` | 683,008 | `080b4d669e9a11fdc802bf7197ae5a044978b6ba39741b2b1c832296987f74d9` |
| invalid-opcode | 1,545 | `09aa0fcfe12c561b79367cb26569dbc6f1f47ca3b98dc892426ca57b4328f868` | 683,008 | `8af8a705da7a63e895e39a94a1ff60dae52bfa1ad0b9c0984adeafe538bae734` |
| general-protection | 1,545 | `23a052f9d47a9416618c9b7a50a382c68c46d3bf7834410cc79f8fef2aa461e0` | 683,008 | `47f5ae37b48edb0212c6d439237e43ee2ca8064061786010f9644acf70f7ad4b` |

The refreshed producer WVB is 37,517 bytes at SHA-256
`1971e87f8c9931e914e7f7505d4fef213be5b3e6b1d38b0324ffc030be1b7e60`.
Its Windows application is 404,992 bytes at
`5437c508012d726e8bd6fb79d0942548d615f9ea52348c97b54038ab643d83c4`;
its Linux application is 405,504 bytes at
`1ea358f8cc77b36201b22ff820ef6fd000b4bbd48342dfe6eed994e487a15c7b`.

Current-host native execution passes the 11-case object owner and four-case
image owner. Pinned QEMU 11.0/Q35/TCG then admits and boots both native images:
invalid-opcode reports vector 6/error code 0 and general-protection reports
vector 13/error code 0; both reach the exact panic transcript and exit 3. The
retirement coordinator remains 32 suites and grows from 3,164 to 3,168 fixed
cases. Independent Linux execution, the two process-fault images, durable UEFI
publication, grouped qualification, and promotion remain open. The Stage 0
scenario builder stays retained for recovery and differential evidence.

## Reconsideration triggers

Replace the fixtures with WVA or shared native-backend generation when those
exact instruction sequences have an independent general consumer. Do not
generalize the selector into arbitrary machine-code injection or merge the two
remaining process-fault scenarios into this object-only boundary.
