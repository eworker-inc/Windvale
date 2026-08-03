# Decision 0172: First kernel-owned service endpoint

- Date: 2026-08-03
- Status: Implemented and locally verified on Windows; cross-host qualification pending
- Advances: [Decision 0084](0084-Minimal-Capability-Oriented-Windvale-Os-Architecture.md), [Decision 0165](0165-Contained-Windvale-Service-Failure.md)
- Retains: ABI 22, context 7, `WVCHAN04`, `WVKMEM14`, the five boot scenarios, and the existing WVA syscall wire values

## Context

Probe 36 checked capability reference `65536` and directional rights before each syscall, but each `WVPROC15` still held the raw `WVCHAN04` address. The reference therefore selected only one fixed process-record entry; it did not resolve a separate kernel object, bind the provider and current client generations, or become invalid when the provider terminated.

The next useful pressure is smaller than a name registry, public endpoint creation, arbitrary capability transfer, multi-client IPC, restart, or supervision. The existing service needs one explicit kernel-owned identity between a process capability entry and its retained channel.

## Decision

Firmware Probe 37 advances the process record to `WVPROC16` and introduces one 64-byte `WVENDP01` service-endpoint record. `WVCHAN04` remains at state offset `0x410`; `WVENDP01` occupies `0x480`; four `WVRES006` records move to `0x4C0`, `0x540`, `0x5C0`, and `0x640`. The memory arena, address spaces, syscall numbers, capability reference, rights, message limits, and channel format do not change.

`WVPROC16` offset `0xC0` names the endpoint object rather than the channel. `WVENDP01` records:

- magic, version, and exact 64-byte length;
- open or closed state, service kind, reference `65536`, and capacity one;
- provider process reference `65537` and current client reference `65538` or `131074`;
- the kernel-only `WVCHAN04` address;
- bounded resolution and close counts; and
- provider status open, exited, or faulted, with a zero reserved field.

Every capability-bearing syscall now resolves through the endpoint before channel or resource mutation. The machine path requires the exact process-local reference and right, the exact state-page endpoint address, complete `WVENDP01` header and invariants, open provider state, the retained channel binding, and a provider/client participant reference derived from the calling process ID and generation. A successful resolution increments the endpoint count. User code never receives the endpoint or channel address.

The normal and contained-client-fault paths resolve five init operations plus three client operations in generation 1, require count eight, then rebind the endpoint from client reference `65538` to `131074` when the same physical root is rebuilt. Generation 2 contributes the same eight resolutions. Init exit closes the endpoint exactly once at count sixteen with provider status exited.

The service-fault path resolves four init operations and two client calls. The existing exact init fault closes the channel and wakes the client; the endpoint also closes exactly once at count six with provider status faulted. A closed endpoint cannot be resolved again by the independent lifecycle model or emitted syscall path.

Portable `Process-Foundation.wv` owns the endpoint identity, generation-rebind, normal-close, and service-fault-close policy. Canonical WVA callers continue to present the same reference and syscall arguments. Stage 0 temporarily owns raw `WVENDP01` serialization, x86-64 resolution, state transitions, and independent lifecycle checks as an explicit replacement seam.

## Local evidence

The Release solution builds with zero warnings. All 38 focused OS tests pass, including valid, truncated, stale, replayed-close, generation-rebind, exited-provider, and faulted-provider endpoint cases plus exact emitted resolver instruction checks.

All five pinned Windows QEMU 11.0/Q35/TCG scenarios pass with exact Probe-37 serial evidence:

| Scenario | EFI bytes | SHA-256 | Host code |
| --- | ---: | --- | ---: |
| `normal` | 615,936 | `aeb2e4b781fb01f59dae6a9f588e84cfa14e7bb84acddfc1b593e9854faa2818` | 0 |
| `invalid-opcode` | 615,936 | `6113a667b3156bdf71650ee5147b27e843d65258849d1deca16adffb2075cfac` | 3 |
| `general-protection` | 615,936 | `96d0cca11543b92eebba7640cf1ca278c1693d2511e3b1f84503bbc4276bc364` | 3 |
| `user-fault` | 616,448 | `f14b75ef03c2c76a6c6456184baca4087e551a9f5b53be921952c431c0a9e742` | 0 |
| `service-fault` | 604,160 | `0b8e0e8f07f3465f28db713b711f81e014b979c761d9fb8d3361726cc38f3588` | 0 |

Deterministic candidate identities include process-policy WVB 12,398 bytes at `42676fb558683a5a1a1b30d7f74c15fc0396f0e384bed86db0a3d1f3fb4c0bda`, process-policy WVO 84,836 bytes at `6d0e4e88d862438702da5b034fcaae1a9fcb9e7aac6044d6ebbd254c2f2c10f8`, normal process-machine WVO 493,286 bytes at `5c3f291e8180e448cdd6a0e65fb187c6c1941477a43ab4cb986596a05504c4ed`, user-fault WVO 493,334 bytes at `9a08dfe59cff6a2bb88200ebf5a2bd48242806d97d34d3e88266615ef294feff`, and service-fault WVO 481,598 bytes at `609b46dccc32f44341f59c04f6362d7ee0111083ba5bdb83a01d7eaf79ca58cb`.

No cross-host qualification is claimed until the complete repository verifier reports this exact candidate on Windows and digest-pinned Debian.

## Consequences

- Capability reference `65536` now resolves a real kernel object rather than serving as a fixed guard in front of a raw process-record channel pointer.
- The client binding is generation-safe across same-root reuse, while the endpoint identity and provider persist.
- Provider exit or fault makes availability explicit at the endpoint boundary as well as the channel boundary.
- The endpoint object remains kernel-only, internal, and replaceable; it is not a public stable ABI.
- This slice provides the object required by later discovery or replacement work without committing to either policy.

## Deliberate limits and reconsideration triggers

Probe 37 does not add names, lookup, a registry, public endpoint creation, handle-table allocation, transfer, delegation, multiple clients, concurrent calls, cancellation, timeout, restart, replacement, supervision, a general scheduler, or VFS behavior.

Reconsider this exact record when a second endpoint or concurrent client exists, when init must publish endpoints dynamically, when service replacement changes provider identity, when rights reduction needs separate capability-table entries, or when Windvale/WVA can own the complete machine transition without weakening the independent checks.
