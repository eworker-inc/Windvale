# Decision 0096: First Windvale init-owned boot-resource grant

- Status: Candidate
- Date: 2026-08-02
- Owners: Windvale compiler/runtime and operating-system boundaries
- Contracts: [Interpreter profile 4](../../Specifications/Windvale-Os-Bytecode-Interpreter.md), [protected process version 6](../../Specifications/Windvale-Protected-Process.md), kernel memory version 5, kernel paging version 3, ABI 16/context 7/service table 5, and firmware probe 27

## Context

Qualified Decision 0095 separated the admitted WVB from the interpreter executable and proved a real runtime read, but Stage 0 mapped the page directly into process `2` and published that process's `file.read_bytes` tables before init ran. Init was named a resource service without choosing or granting the resource. That was an honest bootstrap seam, but not yet evidence that Windvale code can own the first policy decision.

The next coherent slice must move one real decision without pretending to implement a package manager, resource namespace, general capability transfer, or page-lifetime system. A single immutable boot resource, one fixed recipient, and a one-shot borrow are enough to test the ownership boundary while preserving every already qualified interpreter and ABI invariant.

## Decision

- Advance firmware to probe 27, protected processes to `WVPROC06`, the interpreter runtime profile to `4`, and kernel memory to `WVKMEM05`. Do not retain compatibility branches for the preceding experimental records.
- Place the exact admitted WVB in init's eighth allocation page. Map it user-readable, read-only, and non-executable at init virtual page `7`; keep process `2`'s target page-table entry absent before the grant.
- Make [`Init-Resource-Service.wv`](../../Operating-System/Kernel/Init-Resource-Service.wv) select fixed resource identifier `1`. Its WVA entry calls that Windvale `Main`, invokes experimental syscall `4` with capability reference `65536`, then uses the unchanged receive and exit calls.
- Extend init's sole fixed capability rights from receive `2` to receive-plus-grant `6`. The handle budget remains one: this is one bounded authority reference, not a second discoverable handle or a transferable capability table entry.
- Define one 128-byte kernel-owned `WVRES001` record after `WVCHAN01`. Before user entry it records state owned `1`, resource `1`, owner process `1`, no borrower, the init page, exact length and SHA-256, immutable/RO/NX flags, the fixed target process/root/data address/virtual address/service address/PTE, and zero grant/mapping counts.
- Admit syscall `4` only from process `1`, only through reference `65536`, and only once. Stage 0 constructs the fixed record and process addresses; the syscall revalidates its header, ownership state, identifier, flags, zero borrower/counts, bounded aligned source, nonzero publication addresses, and absent target PTE before it installs one user RO/NX alias in process `2`. It then publishes that process's ABI-16 service-table and `WVBR` pointers, changes the record to borrowed state `2` with borrower `2`, and sets both counts to one. The coordinator independently checks the exact resulting record fields, alias, and tables before entering the client.
- Preserve init as lifetime owner. The client receives a borrowed alias; physical-page ownership does not move and no revocation or teardown behavior is implied.
- Keep the 199-byte WVA-authored `file.read_bytes` leaf, interpreter WVB/WVO, linked client images, ABI 16, context 7, service table 5, `WVBR` table, admitted WVB identity, result channel, and client instruction/call-depth budgets byte-identical to Decision 0095.
- Increase init's user-page budget to four and system-call budget to three. Keep process `2`'s pre-grant user-page count at 37 and require exactly 38 after the alias is installed. Expand the fixed arena from 59 to 60 pages only because init owns one additional physical page.
- Require the coordinator to observe init blocked after exactly the grant and receive calls, validate the borrowed resource record and client publication before entering process `2`, then preserve the existing result-send, wake, exit, and contained-fault sequences.
- Emit `resource-grant=pass` only after the machine path proves the exact state transition. The marker is evidence, not the implementation.

## Candidate evidence

The focused Windows OS suite passes 25 of 25 tests. It proves deterministic policy, init, interpreter, paging, process-machine, and firmware artifacts; absence of the admitted WVB from both user executables; absent client mapping and zero context pointers before the grant; byte-identical repeated grant planning; the complete `WVRES001` record; exactly one RO/NX alias; exact post-grant service and `WVBR` publication; one-shot rejection; malformed owner, target, leaf, resource, and digest rejection; and all earlier isolation and contained-fault invariants.

Important candidate artifacts are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Process-policy WVB | 4,610 | `fad470d9988c997daf4e44f90bbfe665391f5f02dd84ba8e8025580efc11c49f` |
| Process-policy WVO | 40,702 | `364b3ea7b4de30b17af93b5132812b7290c67255028482873a52d7a0c49cb960` |
| Init/resource-service WVB | 273 | `0fe423c499ce4f573095ddb9ff03355ee8b6ad927941f764ddaf2eaf9537f78b` |
| Init/resource-service WVO | 1,441 | `bccf48af1600cf3be8b93c8f132f227a064a324ac47b23d8ff9cdcf7f21d799a` |
| Init WVA shim WVO | 214 | `914327761fee08c69979c0da8a2ef513ac569bd39ab76597590fdf65a5df0511` |
| Linked init image | 1,385 | `ba2a2abe03d420506c79af61cc917f4b0124a2ad7687fa80117e353dde475727` |
| Normal process-machine WVO | 137,807 | `d863e61be67659b30b370da8ba9174b712f0d0bd8f02f31b9cdbb9fd523334c3` |
| Normal process-machine code | 6,941 | `ca0ac1c6110628b3c0cc1b582c905b2610222646b65f43a40e1a729b157828df` |
| Fault process-machine WVO | 137,839 | `c227055913f085d118996e05bde910e37fc5c4af1ef887c2bf91f029a4ca4dc4` |
| Fault process-machine code | 6,973 | `85a966450e3568db149984fb2f290596d8291ab1b572cccaae7cfdcc7edb94c3` |

All four pinned Windows QEMU scenarios pass. Normal is 224,768 bytes with SHA-256 `709ebd7f643f2f9d9c7cf4eb4042977c675a3ff19d7a34da4d7e26e0526a29b7` and host code `0`; invalid opcode is 224,768 bytes with `a89e66da871fcf46637ce4d91463268b2c8cce4309d12953eb7c7b464f57178f` and code `3`; general protection is 224,768 bytes with `ab10c10cc0af01ebe5603033d2f35bce86660b23953c33ff6012ad7cee83a1c5` and code `3`; contained user fault is 225,280 bytes with `b5e726b51f26f48cc9948095bfce4eabaf0b3bc90b89a6c3b3650325adfb05bb` and code `0`. Normal and contained-fault paths complete the real init grant, CPL3 resource read, interpretation to `29`, result transfer, and shutdown. Cross-host qualification is not yet claimed.

## Consequences

The first boot-resource policy decision now executes as Windvale source. Process `2` begins without the resource mapping or usable service pointers, and the kernel publishes both only after init requests the exact grant. The same interpreter and generated ABI leaf then consume the same canonical WVB as before.

This is deliberately a borrow, not ownership transfer. Init remains the recorded owner, process `2` receives one immutable alias for the fixed lifetime, and the kernel still constructs and validates the physical mapping. The distinction avoids inventing revocation or reclamation semantics that the current fixed coordinator cannot yet support.

C# remains the Stage 0 object/image builder, raw page-table and record writer, fixed syscall emitter, firmware packager, and independent checker. Windvale owns selection policy and the complete process/resource contract; WVA owns the entry and syscall mechanics. Those seams remain explicit replacement work rather than hidden product dependencies.

## Deliberate non-claims

This decision does not add a resource namespace, names in init, arbitrary lookup, package parsing, multiple resources or recipients, capability delegation, transferable handles, page-ownership migration, revocation, teardown, reclamation, general shared memory, stable syscalls, process creation, a scheduler, broader interpretation, semantic verification, JIT publication, filesystems, or removal of Stage 0.

## Reconsideration triggers

Reconsider this boundary when:

- a second real resource or recipient requires typed lookup rather than fixed identifier `1`;
- process exit makes alias revocation and physical-page lifetime observable;
- package integrity or dependency selection needs a Windvale-owned catalog;
- a third runnable creates measured scheduling pressure; or
- a broader real WVB requires the next verifier/interpreter semantic slice.
