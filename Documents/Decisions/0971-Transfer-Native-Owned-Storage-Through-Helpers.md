# Decision 0971: transfer native owned storage through helpers

## Status

Candidate implementation of the accepted Language 1.0 memory direction.
The [native collection contract](../../Specifications/Windvale-Native-Owned-Collections.md)
owns admission, limits and failure behavior. Host verification belongs in the
commit summary; installed promotion and interpreter migration remain open.

## Decision

Extend ABI 24/context 10 to direct owned helper calls and returns. All functions
in an invocation use one physical allocation domain and accounting tree. Emit
the entry/runtime template once at Main and resolve helper operations against
that template. Only Main initializes and tears down the domain; bytecode cannot
call its entry wrapper. Helpers retain the shared instruction and depth limits.

Reuse existing native arguments and return storage for scalar Vector handles,
budgets and canonical allocation Results. Classify Result ownership from its
type, including values received from helpers. Reject ordinary helper returns
with unreleased owners until explicit native cleanup for those paths is added.
This keeps repeated helper calls bounded rather than postponing their releases
until the whole program exits. Terminal failures reclaim the enclosing domain.

## Boundaries and next work

Admit the scalar helper portion of candidate WVB 1.42 under the complete WVB
verifier and stricter native checks. Keep at most 64 functions, 128 locals and
240 native frame cells per function. This does not admit Copy-record Vector
elements, borrowed helper parameters, general aggregate cleanup or sharing.

Next, add the mutable indexed operations and borrow lifetimes needed for
interpreter working buffers, migrate those buffers away from repeated immutable
copies, bind context 10 in the normal host path, and measure the original
interpreter workload's peak and steady-state memory. The helper workload proves
allocation reuse across calls; it does not close those integration gates.
