# Decision 0889: publish the bounded System/FFI foreign-memory semantic oracle

## Status

Accepted implementation checkpoint on 2026-08-30. Slice 8 remains in progress.
This decision implements the first bounded foreign-memory and call-admission
semantics for the exact registered buffer ABI. It does not complete authenticated
WVFC consumption, native foreign-call lowering, or the Language 1.0 migration.

## Context

Decisions 0764, 0883, 0884, 0886, 0887, and 0888 establish the accepted
System/FFI workload, exact target and foreign front door, bounded retained-data
discipline, independent admission validation, and canonical WVFC production.
The next dependency-safe checkpoint must define what the compiler is allowed to
do with caller-owned foreign memory without treating a source declaration,
normalized record, System profile, unsafe block, or language effect as ambient
authority.

The current implemented compiler slice cannot yet express frozen Language 1.0's
representation-hidden `opaque <class> type` surface and compiler intrinsics
honestly in Foundation source. Publishing ordinary record substitutes would make
the identities forgeable. Parsing WVFC again inside a semantic helper would also
create an unauthenticated bypass around the mandatory front door.

Native ABI lowering is not required to settle the safety rules. A bounded
rule-level oracle can establish the exact type, effect, range, alignment,
generation, alias, lifetime, no-retain, and no-unwind contracts while leaving
authentication and lowering with their existing owners.

## Decision

1. Publish `Compilerˉsourceˉforeignˉsemantics` as a portable bounded semantic
   oracle. It accepts no source or WVFC bytes and owns no parser. Its normalized
   callable facts have meaning only after an Analyzer-owned adapter authenticates
   the admitted WVFC identity against the same immutable WVSS and WVAE evidence.
2. Keep `Foreignˉpointer<T, Abi>`, `Nullableˉforeignˉpointer<T, Abi>`,
   `Foreignˉscratch<Abi>`, and `Foreignˉwriteˉregion<Abi>` compiler-owned and
   representation-hidden. Do not publish fake Foundation record declarations.
3. Limit this registered profile to one positive allocation of at most 64 bytes
   with power-of-two alignment at most 8, one positive exclusive region, one
   pointer, one call, 16 diagnostics, and 524,288 retained evidence bytes. Every
   completed allocation, region, pointer, or call transition reserves its exact
   64-byte successor before observing provider behavior. Valid state carries at
   least the checked minimum implied by those histories while allowing unrelated
   preloaded evidence above it.
4. Require `memory.allocate` for scratch construction, `unsafe.address` plus
   lexical unsafe for region/pointer/non-null/dereference operations, and
   `ffi.call` plus lexical unsafe for foreign invocation. None of these language
   effects or contexts grants a capability or provider.
5. Require live scratch addresses to be nonzero, aligned, and nonwrapping through
   their inclusive last byte. Require live regions to have positive length,
   checked relative and native exclusive ends, bounded extent, compatible
   alignment, exclusivity, and generation equality. Preserve canonical dead
   payloads and monotonic `call <= pointer <= region <= allocation` history.
6. Make `Requireˉnonˉnull` prove only the named transition. Null may be reported
   only on exact empty ownership history; non-null success requires an actual live,
   generation-matched pointer and region. Keep extent, alignment, lifetime, and
   alias admission on a distinct compiler rule taking normalized concrete
   generation/access geometry. Derive every checkable fact from validated state,
   treat the record as simulation facts rather than authority, and reject the
   generic dereference bypass. This adds no Foundation dereference API.
7. Admit only the exact registered 64-byte call through an actually 8-byte-aligned
   destination. Capacity above the region is `Outˉofˉrange`; an in-range capacity
   or region differing from registered 64 is `Invalidˉcallˉcontract`. A normal
   call consumes pointer authority. A retain or unwind attempt is terminal and
   returns a poisoned invalid state with live authority scrubbed.
8. Keep failure ordering deterministic and typed. Structurally invalid or
   contradictory state is `Invalidˉevidence`, not a lifetime result. The exported
   simulation record is not provider/provenance authority; a future compiler-owned
   opaque state and authenticated Analyzer transition own provenance. Provider allocation failure publishes
   no owner, rejected region construction publishes no region or pointer, and
   pre-call admission failure leaves live authority unchanged.
9. Make no WVFC, WIR, WVB, object, or native ABI format-version change. This
   checkpoint neither lowers nor performs a foreign call and does not claim
   end-to-end authentication or native ABI conformance.
10. Model one live immutable scratch slice as the Analyzer's normalized lexical
    borrow. Block region creation and scratch release until an explicit oracle
    release ends its generation-bound lifetime. Slice transitions add no retained
    evidence and expose no new syntax or public Foundation declaration.

## Initial evidence

The focused semantic project binds and lowers 116 functions to a 147,372-byte
compiler-aligned WVB at SHA-256
`7937f9558a97ec5408f79b2871c9611a838eed16e07ae7acf9ccecaedac06055`.
Two preflight builds are byte-identical. A narrow x64 preflight stages the WVB
to a 5,297,182-byte object set in 7 chunks with a 108-byte manifest. The generic
segmented-hosted-WVB cache performs the candidate's sole complete verification
before content-addressed profile-7 packaging.

An initial focused-owner run caught region construction returning generic
`Aliasing` for a live immutable slice. The corrected rule returns
`Sliceˉaliasing` before the live-region/live-pointer `Aliasing` branch. A narrow
corrected profile-7 selector-`y` probe returns 42 with no output. The corrected
focused owner then passes termination, deterministic build/rebuild, exact
profile rejection, sole complete verification, cache hit at key
`e2f5a700bd1fd84f9012780dd1a26f0e60526eb6532cb01742e4561f29ea6408`,
four negative dispatch probes, all 29 isolated rule selectors, and the exact
source-graph rejection.

The prior Windows direct-native identity and its 29-selector result belonged to
the superseded 121,635-byte WVB. The corrected 147,372-byte result is the
current local Windows development evidence.

The thirtieth case is exact source-to-WIR graph evidence: a portable root that
imports the System fixture rejects with `Dependencyˉprofile`, valid source-set,
parse, and body evidence, and no output artifact. Cases 3, 4, 11, and 15–30 are
rule-level evidence; only case 5 is source-graph evidence in this checkpoint.

This is local Windows development evidence, not paired-host qualification.

## Consequences

The compiler gains one small reviewable semantic owner for accepted unsafe
foreign memory without duplicating the authenticated front door or inventing a
host ABI. Bounds, failure order, and consumed authority are explicit enough for
the future Analyzer adapter and native lowering to reuse.

The normalized callable record remains intentionally insufficient as admission
authority. Foundation does not yet expose the frozen opaque/intrinsic surface.
Actual foreign symbol binding, call lowering, result observation, runtime
containment, authenticated WVFC-to-semantic wiring, and paired-host evidence
remain later Slice 8 work.

## Reconsideration triggers

Reconsider this geometry if another accepted ABI requires more than one live
allocation, region, pointer, or call; if retained evidence approaches 524,288
bytes; if opaque/intrinsic Foundation declarations become honestly expressible;
or if authenticated WVFC identities cannot enter semantics without a second
parser or forgeable certificate. Any expansion requires a named contract and
new focused evidence rather than widening this profile implicitly.
