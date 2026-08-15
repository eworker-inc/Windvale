# Decision 0571: Native Independent Module Metadata Admission

- Status: Implemented candidate; cross-host qualification pending
- Date: 2026-08-15
- Advances: Decision 0140 and the post-.NET language migration sequence
- Contracts: [Seed bytecode](../../Specifications/Seed-Bytecode.md), [native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md), and [native WVB-to-WVO](../../Specifications/Windvale-Native-Wvb-To-Wvo.md)

## Context

The source compiler can encode independent platform scope, authority, required
capabilities, and optional capabilities in canonical WVB 1.11. The native x64
lowerer nevertheless required the metadata-presence byte to be zero. Migrating
repository packages first would therefore create source that compiled only
through some paths and could not enter the retained native backend.

Accepting arbitrary trailing Module bytes would be worse: metadata controls
target admission and capability selection, so silently ignoring it would turn
a declared restriction into no restriction.

## Decision

Add one bounded portable metadata reader at the paired native WVB-to-WVO
application boundary. It accepts the retained absent form and independently
validates present encoding version 1, authority, one through 32 canonical
ordered platform identities, ordered catalog capability identities, major
version 1, required/optional disjointness, exact trailing bounds, and
derived-profile agreement.

Require present metadata's required identities to equal the executable
Capabilities section exactly. Optional identities remain admission and provider
selection metadata and do not create callable capability ordinals. Validate the
hosted optional-only encoding in the reader, but retain the shared lowering
core's requirement for at least one executable capability; optional-only
modules therefore remain unsupported by this application generation.

After successful validation, the application adapter reconstructs the bounded
Module envelope in memory with the retained absent marker and passes that
normalized view to the unchanged shared lowering core. Present metadata is not
discarded before its platform, authority, capability, and profile claims are
proved. The shared core and segmented staging projects retain their previous
source identities.

Pin a 16-case portable self-test covering valid present, valid absent,
optional-only, presence, version, authority, platform count/order, capability
order/version/overlap, profile derivation, and trailing bytes. Add one real
source vector with `linux` and `windows` scope, application authority, required
`process.argument_count`, and optional `file.read_bytes`; reconstruct both
target lowerer applications and require the Windows application to emit its
exact WVO while the paired Linux script owns the same execution contract.

## Consequences

- The paired native WVB-to-WVO applications no longer discard or categorically
  reject independent metadata in their admitted executable subset.
- Required capabilities remain executable truth; optional declarations grant
  no call authority.
- The application grows by one focused parser and normalizer rather than
  changing the already large lowering core or cascading new identities through
  the segmented compiler toolset.
- The normal compiler-aligned verifier and WVB inspector still reject present
  metadata, and direct consumers of the shared lowering core remain absent-only.
  Those consumers must migrate together before normal project builds, package
  migration, optional-only lowering, or removal of the legacy source form.
- This is current-host candidate evidence, not cross-host qualification.

## Reconsideration triggers

Reconsider when capability majors beyond 1, new catalog identities, platform
patterns beyond the Seed lowercase qualified grammar, WVO preservation of
target metadata, or a shared verifier/lowerer metadata model is implemented.
