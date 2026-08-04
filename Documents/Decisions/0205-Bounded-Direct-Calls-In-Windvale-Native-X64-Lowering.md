# Decision 0205: Bounded direct calls in Windvale-native x86-64 lowering

- Date: 2026-08-04
- Status: Implemented locally; independent cross-host qualification pending
- Advances: Phase 10 native host tools and the [Decision 0057 native-retirement gate](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Extends: [Decision 0204](0204-Metered-Loops-In-Windvale-Native-X64-Lowering.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

Decision 0204 completed bounded scalar control flow inside one function. The next ownership break was not another scalar operation but the boundary between functions: WVB declaration traversal, entry selection, independent frame layout, shared execution budgets, packed status propagation, direct-call patching, and multiple WVO symbols all remained implicit in the C# selector.

ABI 22 already defines those semantics. `R11` carries one instruction budget through the complete call graph, `R10` carries one call-depth budget, each function entry decrements depth before allocating its frame, every exit restores depth, and a caller must propagate a nonzero packed status without executing a later WVB instruction. A Windvale-specific calling convention would split the backend instead of transferring it.

## Decision

Admit a deliberately bounded two-function shape alongside the existing one-function shape. The canonical function order is one non-`Main` helper at index zero followed by exported `Main` at index one. Both functions have no parameters, return `i32`, retain the existing bounded `i32`/`bool` locals and metered control-flow subset, and occupy adjacent verified ranges in the shared code section.

Permit `call 0` only in `Main`; reject calls in the helper and every other target. This admits multiple direct call sites, including sites inside already verified control flow, while proving an acyclic graph without introducing recursion or a general graph directory. The call consumes no operands and produces one `i32` value.

Emit the helper first with a non-entry ABI-22 prologue and tails. It reuses the caller's `R10`, `R11`, and `R15` execution context, charges call depth at entry, restores it on every exit, and does not restore the host context. Emit `Main` second with the existing host-entry prologue. Patch each `call rel32` directly to helper offset zero, inspect the high status word, branch to the caller's propagation tail on failure, and store successful `EAX` in the selected value cell.

Emit one local `$function_0000` symbol for the helper followed by exported `Main`, with exact offsets and sizes. Internal calls remain fully resolved inside `.text`, so this slice adds no WVO relocation.

Extend the existing shared-backend differential case rather than adding a top-level test. Require exact Stage 0 WVO bytes through the Windvale memory adapter, hosted tool, and the same tool compiled to native x86-64. Retain Stage 0 execution at the exact shared instruction and depth budgets, reject a changed call target, and keep publish-only-after-success behavior.

## Consequences

- The Windvale-written selector now owns the first real multi-function ABI-22 path and one bounded direct-call edge.
- The canonical call case is exactly 795 code bytes and a 902-byte WVO. Their SHA-256 identities are `5687bce4c0a13535256d4d8c238153ecb8a48c27e77248a307b203ca33303424` and `790d2436ef6f45a6379494038dbbc4ba8987d597ee32e711eb3ef2ab3aeda133`.
- The core, memory-adapter, and hosted-tool WVB identities are respectively `26cde3077eca627ca50763113178f68206c52b3df833ec3fd0b70ca261c6af89`, `d9556132e930dd226e77b50ab963b3783e3368bb1375fc61326a9aa6e6ef6ffc`, and `654d893551b923d707a46ba1d41a99672cdeceafbd14cce65a75461022d2c0b4`.
- The hosted tool currently lowers through Stage 0 to 1,055,451 code bytes and a 1,057,785-byte WVO. These are current implementation measurements, not optimization promises.
- No ABI, WVB, WVO, runtime-status, linker, or application-container format changes.
- Parameters, Boolean returns, deeper acyclic graphs, recursion, and general call-graph metadata remain outside this slice. Scalar parameters are the next narrow backend-ownership boundary before static data and descriptors.

## Reconsideration triggers

- scalar parameters require a different register/stack convention from ABI 22;
- more than one helper requires stored function directories or a general bounded call-graph proof;
- recursion is admitted and requires cyclic depth evidence beyond the existing runtime counter;
- internal calls move across object sections or objects and require typed relocations; or
- independent Windows/Linux evidence changes any accepted byte, status, or rejection result.
