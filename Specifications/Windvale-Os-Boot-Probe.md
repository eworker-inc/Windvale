# Windvale OS firmware boot probe

## Status and purpose

Firmware Probe 33 is the current implemented candidate for bounded guest resource request/reply. It composes ABI 21/context 7, WVA seam 9, admission 4/bridge 2, retained bridge 10, memory 11, paging 4, protected processes 12, channel 2, interpreter profile 6, `WVRES004`, and `WVBR002`.

[Decision 0135](../Documents/Decisions/0135-Bounded-Guest-Resource-Request-Reply.md) owns Probe 33. The current candidate passes all 31 bounded OS tests and all four Windows pinned-QEMU scenarios; fresh dual-host qualification remains pending. [Decision 0133](../Documents/Decisions/0133-Frame-Owned-Direct-Native-Records.md) retains the latest cross-host-qualified Probe-32 history.

The firmware ABI follows UEFI 2.11 x64 calling conventions and `GetMemoryMap`/`ExitBootServices`. These host mechanics do not define portable Windvale semantics.

## Artifact construction

The builder compiles system-profile `Hello-World.wv`; portable admission, process-policy, init, exact `Tests/Fixtures/Source-Wvb/Function-Only.wv`, and retained-native modules; hosted `Bytecode-Interpreter.wv`; and the kernel/init/resource/client WVA shims. Portable modules pass through canonical WVB, mandatory verification, ABI-21 native selection and fragment verification, and WVO. Stage 0 rewrites only verified link-facing symbols.

The existing Stage 0 and Windvale-written compilers must emit byte-identical canonical WVB for `Function-Only.wv`, and those bytes must equal the embedded admission identity. The compiler remains at ABI 21.

Stage 0 also creates the loader, memory, exception, paging, admission bridge, process machine, retained bridge, and x64 byte adapter. It independently reconstructs the exact WVA lookup leaf before publication. The linker reconstructs the base-zero image and passes verified code and read-only data to UEFI application writer 3.

The linked kernel payload must fit the fixed 768 KiB supervisor RX window. Init fits one user RX page; the client fits 109 user RX pages. Generated WVB, WVO, EFI, firmware maps, and virtual disks are not committed.

Candidate image identities are:

| Scenario | EFI bytes | SHA-256 | Expected host code |
| --- | ---: | --- | ---: |
| `normal` | 561,664 | `797594f7955277e91ac0de4275bfc06abf9983e32ae0d3603cde89b4f5acf510` | 0 |
| `invalid-opcode` | 561,664 | `4860f646a55941f1b7d13a175cd621d412bdc0ba9cfad793ab73feab82fb835d` | 3 |
| `general-protection` | 561,664 | `f1b456b54476458b26f7e48e921a939bccf2f31f8eef586b39d8f929cfbe7237` | 3 |
| `user-fault` | 562,176 | `b80b3f4d56a60a4b707f7f5cf99fb6bd377b798774cadc3bc44b0e51a8a6019d` | 0 |

All four identities pass the Windows pinned-QEMU gate with complete exact serial markers. Fresh Debian and complete cross-host qualification remain pending; no Debian QEMU execution is claimed.

## Firmware exit and kernel entry

The loader validates UEFI tables, obtains a bounded memory map, and retries `ExitBootServices` at most three times. It uses no firmware service after successful exit and overlays exact `WVKHAND1` before kernel entry.

Memory 11 selects and clears a 2 MiB-aligned 141-page arena below 4 GiB, publishes `WVKMEM11`, copies the handoff, and switches to the four-page kernel stack. Page 5 becomes the IDT; paging 4 consumes pages 6 through 11 and activates the low-1-GiB W^X root with a 768 KiB supervisor RX window.

Admission 4 requires token 73 for the exact 815-byte canonical WVB. Protected-process execution then:

1. runs Windvale process policy and requires token 97;
2. allocates pages 12 through 20 for init and 21 through 140 for the client;
3. creates two roots, two init-owned RO/NX resources, absent client aliases, `WVPROC12`, `WVCHAN02`, and two `WVRES004` records;
4. enters init, which grants ordered set token `131073` and registers a bounded request window;
5. publishes both aliases, service table 5, and `WVBR002` atomically;
6. enters client generation 1, copies and validates the exact `WVRQ 1` / `WVRY 1` configuration exchange, then interprets `Sourceˉwvbˉfixture` for exactly 199 guest instructions and returns `6`;
7. cleans both aliases and publications, reloads init's CR3, releases the exact 120-page tail, and rebuilds the same root as generation 2;
8. repeats grant, request/reply, interpretation, result, and cleanup under generation 2;
9. lets init receive the second `6` and exit;
10. runs the retained native probe and compiler-generated system-profile Main.

The user-fault scenario contains vector 13 from client `CLI` after the result send. CPL0 invalid-opcode and general-protection scenarios remain terminal after the complete successful prefix.

## Exact serial evidence

Normal success requires:

```text
windvale-os-boot 33
entry=pass
system-table=pass
memory-map=pass
boot-services=exited
memory-owned=pass
allocator=pass
kernel-stack=pass
paging=owned
wvb-admission=pass
processes=isolated
resource-grant=pass
typed-resources=pass
resource-revoked=pass
process-reuse=pass
wvb-runtime=interpreted
init-service=pass
ipc=resource-request-reply
Hello from Windvale
cpu-exceptions=armed
native-context=pass
native-wvb=pass
windvale-source=pass
status=pass
shutdown=poweroff
```

The user-fault case inserts `user-fault=contained` before `status=pass`. Kernel panic writes value 1 to QEMU test port `0xF4`, producing host code 3; normal and contained-fault success use the Q35 shutdown leaf and produce code 0. Complete serial evidence is mandatory because host code alone is ambiguous.

## Boot harness

From the repository root:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Os-Boot.ps1 -Scenario normal
pwsh -NoProfile -File Tools/Verify/Verify-Os-Boot.ps1 -Scenario invalid-opcode
pwsh -NoProfile -File Tools/Verify/Verify-Os-Boot.ps1 -Scenario general-protection
pwsh -NoProfile -File Tools/Verify/Verify-Os-Boot.ps1 -Scenario user-fault
```

The harness preflights pinned QEMU/OVMF, creates run-private media and variable state, launches `pc-q35-11.0,accel=tcg` with one CPU and 128 MiB, captures serial, validates scenario output and exit code, and rechecks input identities. Default timeout is 60 seconds. Temporary artifacts are deleted unless `-KeepRunDirectory` is supplied.

Harness diagnostics remain `WVOS3001` build failure, `WVOS3002` start failure, `WVOS3003` timeout, `WVOS3004` exit mismatch, `WVOS3005` serial mismatch, `WVOS3006` changed input, and `WVOS3007` cleanup-boundary failure.

## Deliberate non-claims

Probe 33 does not authenticate firmware CRCs, own general physical memory, release non-tail extents, load arbitrary WVB, provide complete verification, publish executable memory, JIT, schedule generally, transfer capabilities, enumerate resources, provide independent resource lifetimes, run the complete `WVRS 1` store dynamically in the guest, implement filesystem paths/packages/network/devices, handle SMP shootdown, or qualify Hyper-V/physical hardware. Stage 0 machine emitters remain explicit replacement seams.
