# Decision 0090: First in-guest WVB admission

- Date: 2026-08-01
- Status: Accepted and implemented candidate; cross-host qualification pending
- Implements: Step 3 of [Decision 0084](0084-Minimal-Capability-Oriented-Windvale-Os-Architecture.md)
- Extends: Candidate [Decision 0088](0088-First-Kernel-Owned-X64-Page-Tables.md)
- Contract: [Windvale OS WVB admission version 1](../../Specifications/Windvale-Os-Wvb-Admission.md)

## Context

The page-table candidate boots compiler-generated system-profile Windvale and one host-verified portable WVB-derived native object under a kernel-owned W^X root. The guest still does not inspect canonical WVB bytes before executing their native derivative. Decision 0084 requires an AOT Windvale verifier in the boot image to admit one canonical WVB before the first ordinary module executes.

The current native backend intentionally exposes one source-level `Main() -> i32` export per module. It does not yet provide a general guest loader, a multi-export application ABI, or dynamic native-code generation. A first slice must therefore prove the trust order without inventing those missing facilities or moving semantic policy into a C# bridge.

## Decision

- Define WVB admission version 1 as an intentionally fixed profile for the exact 174-byte canonical WVB 1.6 produced from `Embedded-Wvb-Program.wv`.
- Implement the live admission decision in portable Windvale source `Wvb-Admission.wv`. It checks the WVB header, version, seven exact section envelopes, total length, and every byte against the accepted canonical identity. It returns token 73 only after all checks succeed and returns zero for rejection.
- Embed both candidate and accepted bytes in the verifier's immutable read-only data. Stage 0 must prove that the candidate bytes in the checked-in verifier source equal the canonical compiler output before constructing an image.
- AOT-compile the verifier and embedded program independently through the ordinary verified WVB, shared ABI-15 native backend, fragment verifier, and WVO sink. Rename only their external `Main` symbols at the verified object boundary to `Windvale_kernel_wvb_admit` and `Windvale_kernel_embedded_main`; preserve source semantics and object relocations.
- Add one exact bridge export, `Windvale_kernel_x64_wvb_admission`. It constructs a service-free context with instruction budget 8,948 and call-depth budget 2, calls the verifier first, accepts only token 73, then calls the admitted program and accepts only result 29. A rejection, runtime trap, exhausted budget, or wrong result returns failure without reaching the later call.
- Tail-transfer to the retained portable native probe only after both new calls succeed. The prior ABI-15 borrowed-byte evidence and compiler-generated system-profile Main remain unchanged behind this gate.
- Advance the WVA kernel seam to version 7 and the firmware probe to version 21. The WVA Main shim enters the admission bridge. System-profile Windvale emits `wvb-admission=pass` only after the admission bridge, admitted AOT module, and retained native probe return successfully.
- Keep source parsing, canonical WVB production, AOT compilation, object-symbol adaptation, linking, PE32+ packaging, and independent verification in named C# Stage 0 seams. They remain replacement and recovery machinery, not guest admission policy.
- Do not claim a general WVB decoder, complete semantic verifier, runtime loader, interpreter, JIT, process boundary, capability table, or user-mode execution.

## Candidate evidence

Local Windows evidence records:

- all 21 deterministic OS tests passing, including exact artifacts, four changed/truncated candidate families, reference execution, external-symbol adaptation, and bridge order;
- a 174-byte admitted WVB with SHA-256 `7f08efbb20c6cc69c100f07407f759625b38c02a3f05bb4e8dabcc7bdd10c4e2` and exact result 29 in four instructions;
- a 2,786-byte Windvale admission WVB with SHA-256 `231a4001dc316ae965a851aa27eabacaba7ef57d4f72d18ee0e7eaa4d90d2e54`, exact token 73, and 8,944 executed instructions;
- a 504-byte admitted-program WVO with SHA-256 `461361ba8853faa59d7b8f841308fd88b5e7ee837a2654ab3e534771c189a834`;
- a 24,445-byte admission WVO with SHA-256 `5b11e97e5bb9746daa911559ea9a7a204419fe2cded44977163430185e7d150d`;
- a 481-byte admission bridge WVO with SHA-256 `eb229f4fbf104c67e3402280016355da87a3bda51ffcb361c07d709815060f39`;
- a 774-byte WVA version-7 shim WVO with SHA-256 `2ef94f867226059e858e874d1260743e411bd1fd22887a84d35c2e508d410393`;
- a 47,104-byte normal image with SHA-256 `c3a07e1a6c8f162720a3dcd690fdb945bd862b360b26665c53e5be0642a87c38`, complete `wvb-admission=pass` transcript, and pinned-QEMU exit code 0;
- a 47,104-byte invalid-opcode image with SHA-256 `0bbc0b6eedbd21aef853a2233d3fa3dbaa9564eca36f3344067b1c9b240237fc`, normalized `(6, 0)`, and exit code 3; and
- a 47,104-byte general-protection image with SHA-256 `724907ffd0963f015003c91431b79b728109ed861de73f3c1b0bf5e7b58568b6`, normalized `(13, 0)`, and exit code 3.

The complete Windows/Debian qualification gate and independent CI have not been run for this candidate. Decision 0087 at exact commit `12e9e2e` remains the latest cross-host-qualified baseline.

## Consequences

The guest now makes a real Windvale-owned decision over canonical WVB bytes before the native derivative of those exact bytes executes. Canonical WVB remains program identity; the AOT object is a derived boot artifact. The order is executable evidence rather than only a documented intention.

The first profile is deliberately a whitelist for one tiny module. Its duplicate canonical bytes and fixed offsets are appropriate evidence for the bootstrap paradox, not the future general verifier. The next verifier slice should replace fixed identity checks with bounded generic section decoding, semantic instruction verification, and an explicit admitted-module result while retaining these negative and ordering tests.

The admitted program still executes in ring 0 and the boot image remains host-constructed. The next architectural boundary is one protected address space, thread, capability table, resource budget, and IPC channel; moving execution outside the kernel requires that isolation work rather than widening this bridge.

## Reconsider when

- a general verifier result needs a richer identity or diagnostic record than one scalar token;
- a guest loader can retain WVB separately and select AOT code by verified identity without Stage 0 symbol renaming;
- process isolation requires admission and publication to cross a versioned syscall or IPC boundary;
- the shared backend gains multiple external exports or an explicit boot-component entry contract; or
- exact-profile code size no longer fits the bounded boot window.
