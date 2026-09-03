# Decision 0933: pair authenticated Foreign calls before WVB emission

## Status

Accepted and implemented locally on Windows on 2026-09-03. The authenticated
production path reaches typed WVIR, pairs every Foreign declaration and call
with its retained WVFB facts, and then stops before WVB emission. WVB encoding,
runtime/provider containment, native ABI invocation, Linux reproduction, and
Slice 8 qualification remain pending.

## Context

[Decision 0925](0925-Publish-And-Retain-Authenticated-Foreign-Lowering-Carrier.md)
made the coordinator retain an independently checked WVFB carrier, and
[Decision 0932](0932-Represent-Typed-Foreign-Calls-In-Wvir-1.31.md) made the
Analyzer emit typed WVIR operation `190`. Neither value alone proves that the
typed call belongs to the same authenticated source/catalog relationship. The
production path therefore still stopped before analysis.

The next boundary must establish that correlation without turning WVIR into an
authentication certificate, passing the whole target/catalog closure into the
compiler-scale Analyzer, adding a parallel compiler, or letting the emitter
consume an unpaired Foreign call.

## Decision

1. Add a private Analyzer form,
   `--internal-foreign-source-set`, used only by the authenticated coordinator.
   It consumes the retained WVSS and constructs the ordinary WVSS, WVCA, WVLB,
   and WVIR analysis products. The public internal-source-set and raw Project 2
   routes continue to reject Foreign source.
2. Reuse the existing `wvbind` executable for
   `--internal-pair-analysis <wvss> <wvfb> <wvir>` instead of creating a sixth
   compiler product. The mode writes no file and returns only bounded counts in
   one exact success line.
3. Reconstruct source symbols from WVSS, require the complete conditional WVSD
   1.2 shape, independently validate WVFB, and match every kind-9 symbol to one
   carrier record by module, WVSD directory index, and fixed arity three.
4. Accept only WVIR 1.31 or 1.32 at this checkpoint. Bound its function, block,
   and operation geometry before scanning, require every operation `190` target
   to identify a retained carrier record, and reject more than 4,096 Foreign
   calls.
5. After pairing succeeds, require the coordinator to recheck the original six
   authenticated snapshots and retained WVFB again. It then returns exact
   `source emission status=Foreignˉwvbˉpending`, launches no emitter, and
   publishes no final WVB.
6. Keep generic type catalog and serialized WVLB construction Analyzer-owned.
   The binding phase validates complete names, callables, and bodies, then
   returns its phase product directly; it must not run the legacy WVLB 1.1
   finisher over an exact generic Foreign-pointer parameter that requires the
   newer generic WVLB form.
7. Exercise pairing in a small isolated hosted fixture. Keeping it outside the
   already maximal typed-analysis fixture bounds native compiler retained state
   and avoids turning a focused correlation check into another compiler-scale
   rebuild.

## Verification

The `language-1-authenticated-foreign-binding` owner now builds four WVB
products, packages three applications, and executes 27 isolated selectors over
five fixtures. Its pairing selector accepts the exact carrier/symbol/WVIR
relationship and rejects altered carrier directory, carrier module, reserved
carrier data, WVSD version, WVIR version, WVIR target, and record count. The
owner passed in 101.909 seconds on the final Windows development state after
reusing unchanged compiler products.

The `language-1-production-admission-ingress` owner then exercised 21 cases in
455.506 seconds. Its real nonempty-catalog case admits a two-module System
source containing the exact generic Foreign pointer and typed call, runs all
five products through pairing, receives only `Foreignˉwvbˉpending`, and observes
no public WVB. It also retains the existing path, mutation, output, route, and
determinism sentinels.

## Consequences

The authenticated production path no longer stops between binding and typed
analysis. It proves that its retained normalized Foreign facts and every typed
Foreign call describe the same source symbols, while preserving the distinction
between authentication, typed analysis, and execution authority.

The next compiler checkpoint can assign a WVB encoding and verifier containment
rule to the already paired call. This decision does not resolve a library,
create a host address, execute Foreign code, grant a capability, migrate a real
consumer, or complete Language 1.0.

## Reconsideration triggers

Revisit the private pairing boundary if a later ABI needs facts not present in
WVFB, if a new WVIR minor changes the table geometry consumed here, if more than
one Foreign declaration per module becomes valid, or if measured production
work shows that a separately cached pairing artifact is necessary and can be
authenticated without weakening snapshot ownership.
