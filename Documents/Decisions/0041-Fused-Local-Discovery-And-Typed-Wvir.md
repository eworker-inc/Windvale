# Decision 0041: Fused local discovery and typed WVIR construction

- Date: 2026-07-31
- Status: Qualified on Windows x64 and Debian Linux x64 at `b1241157310bc597dbdf0d24146f4d81f0128712`

## Context

The qualified compiler validates symbols, performs a locals-only WVLB traversal, and then reparses every function body to construct typed WVIR. That separation kept the first typed-IR slice simple, but the exact ten-module compiler self-lowering input now reaches the unchanged 4,000,000,000-instruction limit after 222.393 seconds on the Windows development host. Its source bytes fit inside WVSS, so enlarging the source envelope would not address this measured failure.

Raising the runtime instruction ceiling would hide repeated compiler work and would make later bootstrap stages pay the same cost again. Publishing a second syntax tree or general body collection would also conflict with the offset-based streaming design.

## Decision

Construct parameter/local WVLB evidence and typed WVIR in one declaration/statement traversal on the successful path.

- Prepare function parameters before entering the body.
- Compile a local initializer before appending that local, preserving initializer-before-declaration visibility.
- Append each local immediately after its initializer and carry the growing function-local packed binding payload through nested blocks.
- Preserve whole-function duplicate-name rejection, absolute scope spans, stable slots, mutability, and branch traversal order.
- Merge each completed function's bindings and WVIR payload once into their global builders.
- Publish the same independently validated WVLB 1 and WVIR 1 directories; no unvalidated transient structure crosses the phase boundary.
- Retain the complete body-binding pass as the diagnostic oracle when typed lowering fails, preserving established binding-error precedence.
- Expose successful-run instruction counts through the opt-in Stage 0 CLI `--report-steps` switch without changing default command output.
- Keep the 4,000,000,000-instruction ceiling unchanged.

The existing standalone `Compilerˉsourceˉbindings` phase remains the qualified complete body-binding API. Fusion changes only the successful preparation path used by typed WVIR; it does not remove independent validators or failure coverage.

## Consequences

Successful typed lowering no longer reparses every statement solely to discover locals. The compiler still has no syntax tree, token collection, host dictionary, or ambient resource lookup. WVLB and WVIR remain explicit deterministic evidence boundaries shared by the WVB and future native backends.

The exact closure remains too expensive: the retained candidate reaches the same bounded `WVR3011` instruction-limit diagnostic after 209.130 seconds on the Windows development host, compared with 222.393 seconds for the qualified separate-pass baseline. Both runs execute the full 4,000,000,000 instructions, so elapsed time is diagnostic host evidence rather than a portable progress metric or a claim that bootstrap closure is complete. The structural result of this slice is removal of one complete successful-path body traversal; the next performance slice must reduce the remaining lookup and typed-lowering work enough to complete below the existing ceiling.

A broader prototype also fused namespace and declaration validation and added cheap name-length screening. It increased source and artifact size and reached the same ceiling after 214.139 seconds in the retained-host comparison. First-byte/hash and name-length directory indexes likewise did not earn their representation and validation cost. Those additions are rejected from this slice; future lookup work needs a measured evidence format or algorithmic boundary rather than another ad hoc index.

The 4 MiB WVSS versus 16 MiB Stage 0 source-envelope gap remains separate. It should be changed only with a representation and memory decision, not bundled into this performance slice.

## Verification gate

The candidate must:

- preserve every existing focused WVLB, WVIR, WVB, diagnostic, corruption, and runtime result;
- prove fused WVLB and WVIR bytes equal the prior separate-pass semantics on focused valid fixtures;
- measure the exact ten-module typed-IR self-lowering input at the unchanged 4,000,000,000-instruction ceiling and record the remaining bounded failure without treating elapsed time as portable semantics;
- keep the real closure outside the normal fast loop until its measured cost is acceptable;
- pass Standard and the complete native verifier on Windows; and
- pass exact-commit Debian qualification with matching normalized reports and byte-identical portable artifacts.

Completing the exact ten-module input below the ceiling remains the entry gate for the later bootstrap-closure milestone, not the acceptance gate for this narrower traversal-fusion improvement.

## Qualification

Exact commit `b1241157310bc597dbdf0d24146f4d81f0128712`, tree `eda80c98a2706ddb22abcc50deb5e60961cd2981`, passed the focused compiler suite, complete Standard verification, and exact-archive Qualification on Windows x64 and Debian GNU/Linux 12 x64. Both hosts completed zero-warning Release builds, all 48 tests, and the complete native verifier. Their normalized conformance contracts matched, and all 61 retrieved portable artifacts were byte-identical.

The retained ten-module experiment still returns bounded runtime diagnostic `WVR3011` at exactly 4,000,000,000 instructions. This qualification therefore accepts the traversal-fusion improvement and its unchanged semantic evidence, not full compiler self-hosting.
