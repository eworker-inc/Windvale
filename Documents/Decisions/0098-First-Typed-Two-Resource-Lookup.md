# Decision 0098: First typed two-resource lookup

- Status: Candidate
- Date: 2026-08-02
- Owners: Windvale compiler/runtime and operating-system boundaries
- Extends: [Decision 0097](0097-First-Terminal-Resource-Borrow-Revocation.md)

## Context

Qualified probe 28 gives init one immutable WVB resource, one borrower, and deterministic terminal cleanup. The resource is still selected by fixed identifier `1`, the private `WVBR` publication has one untyped entry, and the interpreter has no second resource whose identity, type, mapping, or lifetime can expose whether that representation generalizes coherently.

The next measured pressure is a second real input: a four-byte execution budget that the user-space interpreter must read and enforce. It must not be folded into the WVB page, embedded in the interpreter RX image, or treated as an ambient kernel constant. A separate page and record make its ownership and mapping independently visible.

## Decision

- Advance firmware to probe 29, protected processes to `WVPROC08`, and kernel memory to version 6. Advance the resource record to `WVRES003` and the private resource directory to `WVBR` version 2.
- Retain exactly two init-owned immutable resources:
  - identifier `1`, kind `wvb-module`, opaque name `boot:main.wvb`, containing the exact admitted WVB; and
  - identifier `2`, kind `u32-execution-budget`, opaque name `boot:main.budget`, containing the canonical four-byte little-endian value `4`.
- Encode the resource kind in the version-3 record's typed attribute field while retaining immutable, read-only, and no-execute attributes. Keep separate source page, digest, borrower, mapping, and grant counters for each record.
- Let Windvale init select both identifiers as one canonical ordered resource-set token encoding `(1,2)`. Its WVA syscall seam passes that token unchanged; the kernel atomically grants both records or rejects the complete request. An unknown, duplicate, reversed, or partial set is invalid, and process `2` can never observe a half-published directory.
- Publish one 80-byte `WVBR002` directory containing a 16-byte header and two 32-byte entries. Each entry identifies the resource, kind, mapped pointer, exact length, and immutable RO/NX attributes. The WVA-owned `file.read_bytes` leaf matches one of the two exact names, then validates the selected typed entry before returning borrowed bytes.
- Make the interpreter call `file.read_bytes` for both names. It validates the budget resource as exactly four bytes, accepts a bounded nonzero value, and charges one unit per interpreted opcode. The compiler's canonical admitted program contains exactly four opcode steps (`i32.const`, `local.store`, `local.load`, and `return`); budget `3` fails before the return opcode.
- Give the two resources distinct init source pages and distinct client virtual aliases. Increase init's physical allocation from eight to nine pages. The measured budget-enforcing interpreter requires 33 RX pages instead of 32; remove the client's old unused physical placeholder for a borrowed page so its 42-page allocation now contains only tables, code, stack, and data, with the two aliases in the following virtual pages.
- The enlarged Windvale process policy no longer fits safely on the qualified two-page kernel stack. Pinned QEMU proves that three stack pages still fail during process construction and four pages complete all four scenarios. Advance the owned kernel stack to four pages and the arena from 60 to 63 pages: two pages for the measured stack growth and one for init's second resource, while retaining the zero-free-page completion invariant. After the policy returns, the coordinator reconstructs the 2 MiB-aligned arena base from its owned stack and revalidates `WVKMEM06` before publishing process state.
- On ordinary client exit or contained client fault, revalidate both typed records, both directory entries, and both live leaves while permitting only each hardware-set accessed bit. Clear both PTEs, both context pointers, the complete service table, and the complete `WVBR002` directory. Return each record independently to owned/no-borrower/mapping-zero state while preserving one historical grant and both init pages.
- Keep canonical WVB, ABI 16/context 7/service table 5, the admitted program, the WVA process entry/exception seams, and the result channel unchanged. Stage 0 remains the raw page/record/directory emitter and independent oracle; Windvale owns resource selection and budget enforcement, and WVA owns the bounded typed lookup leaf and privileged entry mechanics.
- Increase the exact interpreter process instruction budget from `4,678` to the measured `4,822` instructions required by the second resource call and the new budget validation/enforcement path.

## Required evidence

- Deterministic reference and machine results for the atomic ordered two-resource grant and terminal cleanup after both ordinary exit and contained fault.
- Rejection of unknown, duplicate, reversed, missing, wrong-kind, wrong-length, wrong-digest, wrong-entry, writable/executable, relocated, and otherwise mutated resource state.
- Interpreter coverage for exact budget `4`, exhausted budget `3`, zero, oversized, malformed-length, and missing-name cases.
- Exact proof that the two source pages and two client PTEs are distinct, that no client-owned placeholder page backs either alias, and that the 63-page allocator is exhausted at the new exact final cursor. The four-page stack must pass all pinned scenarios; the measured three-page configuration must not be recorded as sufficient.
- Focused Windows OS tests, all four pinned-QEMU scenarios, deterministic artifact identities, and the repository's Windows/Debian qualification gate before this decision becomes Qualified.

## Consequences

The first resource lookup is now typed and selected from two real independently recorded inputs. The execution budget is policy data owned outside the interpreter image, and a client cannot run with only a partially published directory.

The two resources still share one fixed owner and borrower, are granted in one fixed order, and are cleaned up at the same terminal boundary. This is enough to pressure identity, type, mapping, and publication mechanics without pretending to provide a general namespace or arbitrary lifetime graph.

## Deliberate non-claims

This decision does not add dynamic names, enumeration, arbitrary resource counts, ownership transfer, delegation, explicit revocation, page release, root reuse, replacement, package loading, executable publication, a scheduler, SMP shootdown, Hyper-V evidence, or removal of Stage 0.

## Reconsideration triggers

Reconsider this boundary when:

- a third resource makes fixed directory slots materially repetitive;
- one resource must outlive or be revoked independently of the borrower;
- owner exit or replacement requires page reclamation;
- a package or loader service needs dynamic name resolution; or
- another recipient requires capability transfer rather than one fixed borrow.
