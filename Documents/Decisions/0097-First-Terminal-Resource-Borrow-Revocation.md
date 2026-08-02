# Decision 0097: First terminal resource-borrow revocation

- Status: Qualified
- Date: 2026-08-02
- Owners: Windvale compiler/runtime and operating-system boundaries
- Contracts: [Interpreter profile 4](../../Specifications/Windvale-Os-Bytecode-Interpreter.md), [protected process version 7](../../Specifications/Windvale-Protected-Process.md), kernel memory version 5, kernel paging version 3, ABI 16/context 7/service table 5, and firmware probe 28

## Context

Qualified Decision 0096 gives Windvale init ownership of one immutable WVB and lets it authorize one checked borrow into process `2`. The borrower then exits or is contained after a user fault, but version 6 leaves its alias and private resource publication live. That fixed-lifetime shortcut is now observable process-lifecycle pressure.

The smallest coherent next slice is automatic cleanup of that one terminal borrow. It must remove borrower access without pretending to free physical memory, reuse an address space, expose a general revoke syscall, or solve multiprocessor translation invalidation.

## Decision

- Advance firmware to probe 28, protected processes to `WVPROC07`, the resource record to `WVRES002`, and the Windvale process-policy token to `96`. Retain kernel memory version 5, kernel paging version 3, interpreter profile 4, ABI 16, context 7, service table 5, and `WVBR` version 1.
- Extend [`Process-Foundation.wv`](../../Operating-System/Kernel/Process-Foundation.wv) with one mapping count and the lifecycle rule that a terminal borrower returns resource `1` to owned state, clears borrower identity, and reduces mappings from one to zero while preserving one historical grant.
- Accept cleanup only when process `2` and thread `2` are coherently exited/exited or faulted/faulted. Revalidate the complete borrowed record, its fixed owner/borrower/counts, exact service/resource publication, immutable WVB identity, and target leaf before mutation.
- Treat the x86-64 leaf accessed bit as hardware evidence, not corruption. The target leaf may be the exact granted RO/NX entry or that entry plus accessed bit 5. Every other leaf mutation still fails. The deterministic planner normalizes only this bit before comparing all table bytes.
- Before init resumes, clear the sole client WVB PTE, both execution-context resource pointers, the complete 104-byte private service table, and the complete 32-byte `WVBR` table. Change the resource to owned state, borrower zero, grant count one, and mapping count zero. Preserve the init-owned physical page, bytes, digest, service address, and all other record fields.
- Reactivate init's page-table root immediately after cleanup. The CR3 load flushes non-global translations from the retired single-CPU client root. Process `2` is never resumed or reused. This is not a general TLB-shootdown or address-space-reuse contract.
- Apply the same cleanup after the client's ordinary exit and after its contained CPL3 general-protection fault. Init still consumes result `29`, exits normally, and the earlier kernel-fault scenarios retain their terminal contracts.
- Emit `resource-revoked=pass` only after the process machine returns with the cleanup invariants proven. The marker is evidence, not the implementation.
- Keep the init WVB/WVO, interpreter WVB/WVO, linked init/client images, admitted WVB, WVA service leaf, channel, syscall assignments, budgets, and arena byte-identical. Stage 0 C# remains the raw table/record emitter and independent oracle; Windvale owns the lifecycle policy and WVA retains machine-entry and privileged-operation seams.

## Qualification evidence

The focused Windows OS suite passes 25 of 25 tests. It proves deterministic exit, repeated, hardware-accessed-leaf, and fault revocation planning; byte-identical cleanup results; terminal-state rejection; malformed record/table/data rejection; release-replay rejection; exact zeroed publication ranges; and all previous isolation, admission, IPC, exception, and reproducibility contracts.

Important qualified artifacts are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Process-policy WVB | 5,152 | `c4aacb9036f825ecd3d038954c1d07c573b43eb4f6ee831d0b81d188f0682679` |
| Process-policy WVO | 45,886 | `d6290156c6d7cf709ba44b3035fac4eea75995ecf3ddfbcb8a0a9b73c0612509` |
| Init/resource-service WVB | 273 | `0fe423c499ce4f573095ddb9ff03355ee8b6ad927941f764ddaf2eaf9537f78b` |
| Init/resource-service WVO | 1,441 | `bccf48af1600cf3be8b93c8f132f227a064a324ac47b23d8ff9cdcf7f21d799a` |
| Interpreter WVB | 12,265 | `25a223346c6357290680476a39a4e67821e5efc9420933a90486f993aef46bf2` |
| Interpreter WVO | 128,340 | `5157b4446422d37597b16b5f29b5aae3f05920fc4718af1a9759efe29f4e73b7` |
| Normal process-machine WVO | 138,751 | `19da37cdc044505a92410449a14e72c35ed8573b409c095b0b9a8a8f9d21f065` |
| Normal process-machine code | 7,885 | `52a5729eca1c36d5ba004dc69fc30d250eea91ad59888d04c4b0c240af9cbfac` |
| Fault process-machine WVO | 138,783 | `bedd2a06969d3df295efe8eefa626dd1c97737731ef1056fbcb6df152f259138` |
| Fault process-machine code | 7,917 | `9df1702699d3f9da8a3926c2406f84cff06e638b6a176401c70b82ddce4e634c` |
| Special kernel WVO | 7,154 | `e22f79d3ed9aad92c9827cc89e711eb78f2e81ed0f207b755f9da9d70be6d66d` |

All four pinned Windows QEMU 11.0/Q35/TCG scenarios pass with exact probe-28 transcripts. Normal is 230,912 bytes with SHA-256 `bc5f04c0e75fb217c9339bcc2a391bbe68f9f79ad97c18a93e35e310dab62d46` and host code `0`; invalid opcode is 230,912 bytes with `2c9c6c60543d3729f7401720c0b03e98dc2c5e3654e45668bfcd4559650bc543` and code `3`; general protection is 230,912 bytes with `bdccbd123dd457d88c4902f33e727320b08c93e6acd62ac0f706912d5f1163ca` and code `3`; contained user fault is 231,424 bytes with `221c710d741565c7113a7b8c2ea94c66358018e144ae59f583a3c1ce10225494` and code `0`.

Exact implementation commit `b2197fa4fb78b26d75e4fd5269cde590cbd98dcf` passes GitHub [Verify run 30741650532](https://github.com/eworker-inc/Windvale/actions/runs/30741650532). Windows and digest-pinned Debian 12 each pass all 67 Seed tests, all 25 OS tests, and the complete non-Fast verifier. Seed elapsed time is 237.598 seconds on Windows and 210.262 seconds on Linux. This promotes probe 28 and Decision 0097 to the latest fully cross-host-qualified OS baseline; live QEMU remains Windows-only evidence.

## Consequences

A terminated interpreter no longer retains a present alias or usable private resource publication. Init still owns the page and the record preserves that one grant occurred. Exit and contained-fault paths now converge on one deterministic cleanup result.

The live QEMU failure that shaped the final contract is useful architecture evidence: x86-64 sets the accessed bit on a used leaf. Validators of live hardware tables must distinguish bounded processor-maintained state from unauthorized mutation. Windvale still rejects writable, executable, dirty, relocated, or otherwise changed resource leaves.

No physical page is returned because the current arena has no release operation and init remains alive through the proof. The client root and data pages are retired but not reclaimed or reused.

## Deliberate non-claims

This decision does not add explicit user-facing revocation, physical-page release, process/address-space reclamation, root reuse, PCID, `INVLPG`, SMP shootdown, resource names, multiple resources or recipients, ownership transfer, capability delegation, arbitrary loading, executable publication, a scheduler, or removal of Stage 0.

## Reconsideration triggers

Reconsider this boundary when:

- a client root can be resumed or reused after cleanup, requiring an explicit invalidation protocol;
- multiple CPUs can execute one address space, requiring coordinated shootdown;
- owner exit or resource replacement requires physical-page reclamation;
- a second real resource or recipient requires typed lookup and independent lifetime; or
- a third runnable creates measured scheduling pressure.
