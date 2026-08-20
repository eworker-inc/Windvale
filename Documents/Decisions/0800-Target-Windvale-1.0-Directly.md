# Decision 0800: Target Windvale 1.0 directly

- Date: 2026-08-20
- Status: Accepted
- Supersedes: the future `v0.2.0` product target and required product bundle in
  [Decision 0595](0595-Select-Windvale-0.2.0-Connected-Services-Preview.md)
- Product plan: [Windvale 1.0 product plan](../Project/Windvale-1.0-Product-Plan.md)

## Context

Windvale has published the signed `v0.1.0` preview and has since accumulated
qualified foundations for packages, installation, activation, rollback,
networking, services, the compiler, libraries, and WVDB. The roadmap previously
grouped the next work as sequential milestones leading to a `v0.2.0`
connected-services preview.

That structure no longer matches the intended product. Windvale Language 1.0
has an accepted frozen source design, Windvale Libraries 1.0 has a proposed
coherent suite, and WVDB 1.0 is being specified as a Windvale-owned database.
Publishing another broad preview milestone would create an intermediate product
identity without answering which contracts are actually ready for a stable 1.0
promise.

Compiler slices, database vertical slices, and focused verification owners are
still useful ways to bound implementation and evidence. They need not become
public product stages or release numbers.

## Decision

The next intended Windvale product release is **Windvale 1.0**, published as
`v1.0.0` only after its complete release gate passes. No `v0.2.0` product release
is planned.

Organize active work as parallel **product workstreams** and dependency-aware
**completion gates**, not as a sequence of numbered product milestones. An
internal implementation slice describes bounded work and evidence only; it does
not define maturity, compatibility, or a public release promise.

Windvale 1.0 requires a coherent supported host product on Windows and Linux
with these core outcomes:

1. the frozen Windvale Language 1.0 source contract is implemented through the
   normal compiler, verified representation, runtime, and native paths, with
   conformance evidence;
2. the required Windvale Libraries 1.0 Foundation, Data, and Backend contracts
   needed by ordinary data services are specified, implemented, bounded, and
   qualified;
3. WVDB 1.0 supplies its accepted shared core, strict table profile, basic typed
   relationship profile, transactions, queries, indexes, durability, full
   backup/restore, service operation, and production-oriented conformance;
4. the toolchain, package, launcher, installer, service-lifecycle, recovery,
   and signed-release paths make those components usable without development
   checkout conventions; and
5. the complete selected state passes the exact cross-host security,
   determinism, resource, recovery, and release qualification gates.

The exact required module and conformance matrices belong in the linked product,
library, and WVDB plans. A component may freeze a 1.0 contract before the whole
product ships, but incomplete implementation must not be presented as released
Windvale 1.0 behavior.

Windvale OS remains the long-term vertical integration target and continues on
its own accurately reported qualification path. A complete general-purpose
Windvale OS is not a host-product 1.0 release blocker. External-model gateways,
agent systems, browser products, every optional library profile, distributed
WVDB operation, and compatibility with an external language, framework, or
database are not implicit Windvale 1.0 requirements.

The exact checked-in `0.2.0-dev.1` installer and repository candidates retain
their historical development identities and hashes. They are not a selected
`v0.2.0` release line and must not be renamed in place. New development artifact
names should follow the Windvale 1.0 plan through a separate format or release
decision when the next artifact is selected.

Historical decisions and completed milestone records remain valid provenance.
This decision changes the active product target; it does not rewrite completed
evidence, published tags, immutable artifacts, or the original context of those
records.

## Consequences

- Active project documents point to one Windvale 1.0 product plan rather than a
  `v0.2.0` connected-services release plan.
- Decision 0595 remains historical evidence, while its external database parity
  direction is also superseded by Decision 0790.
- Language implementation slices continue against the frozen Language 1.0
  design. They do not reopen source design unless implementation exposes a
  concrete contradiction, unsound rule, or unimplementable required contract.
- Product status reports distinguish frozen specification, implemented
  candidate, qualified component, and released 1.0 behavior.
- Work may proceed concurrently where contracts permit, but the `v1.0.0` tag
  waits for the integrated gate rather than the completion of any one workstream.

## Reconsideration triggers

Revisit this decision if the integrated 1.0 scope cannot be made finite, if a
security or migration need requires an explicitly supported preview before 1.0,
or if Windows and Linux cannot share the promised semantics. Any reconsideration
must name the exact compatibility promise and must not silently turn internal
slices into public versions.
