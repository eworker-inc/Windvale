# Decision 0895: bind authenticated foreign declarations in a private compiler phase

## Status

Proposed design checkpoint on 2026-08-30. A candidate implementation is in
progress, but no acceptance or paired-host evidence is claimed yet. Acceptance
requires the focused deterministic, malformed-input, retained-snapshot,
resource, and Windows/Linux evidence named below. This checkpoint does not
complete Slice 8: typed lowering, WIR/WVB representation, native ABI lowering,
runtime containment, and final paired qualification remain pending.

## Context

[Decision 0893](0893-Authenticate-Production-Source-Analysis-Ingress.md)
establishes one public Language 1.0 compilation boundary. Its private host
coordinator retains WVAE, WVSS, WVTD, WVFC, the source-input lock, and the
selected profile; invokes the separately bounded `wvauth`; and permits an
authenticated empty catalog to enter the non-authoritative Analyzer route.
The implemented Decision 0893 candidate baseline stops one fully consistent
nonempty catalog before Analyzer launch at exact
`Foreignˉsemanticsˉpending`; the candidate in this decision replaces that stop
only after its binding checks succeed. Decision 0893 remains Proposed.

The current source parser already admits only the frozen System declaration for
`windvale.paper.buffer_source.sysv_amd64_c_v1`, and `wvauth` independently
proves exact catalog order, completeness, source spans, signature digest,
target predicates, `unsafe`, external-symbol spelling, and
`effects(ffi.call)`. The bounded foreign-memory semantic oracle separately
defines the normalized callable and exact ABI rules, but accepts neither WVSS
nor WVFC and deliberately carries no admission authority.

Before this candidate, the ordinary symbol phase was not safe for a Foreign
declaration. The declaration predicate classified Foreign as a symbol, but
WVSD had no Foreign kind value, its entry count excluded Foreign while its
payload included it, and the callable namespace and binding lookup recognized
only ordinary functions, records, capabilities, variant cases, and intrinsics.
Allowing a nonempty catalog to fall through that path would therefore create
inconsistent symbol evidence rather than a semantic binding.

Binding foreign declarations is a compiler semantic responsibility under
[Decision 0886](0886-Make-Target-And-Foreign-Admission-A-Mandatory-Language-1.0-Phase.md).
Moving normalized semantic decisions into `wvauth` would make the authenticator
a second semantic compiler. Publishing WVLB, WVIR, WVCA, or WVB after binding
would instead require the later versioned target/catalog identity carrier,
foreign WIR operation, WVB import, native thunk, and containment decisions.
This checkpoint selects the smaller boundary between those two mistakes.

The first integrated implementation placed the adapter and all five
foreign-only modules in `wvanalyze`. Its measured aggregate was 4,193,688 WVIR
bytes under the 4,194,304-byte immutable-value ceiling, leaving only 616 bytes
of headroom. That result triggered the capacity reconsideration named by this
decision. The portable symbols, bindings, and foreign adapter remain
compiler-owned, but the private hosted orchestration is extracted into the
small `wvbind` product so ordinary analysis does not retain the foreign-only
closure.

## Decision

1. Keep `wvauth` as the independently bounded six-input authenticator. It
   continues to publish no certificate, marker, normalized callable, cache
   value, or successor artifact. Its successful process result is meaningful
   only while the private coordinator retains the same six immutable
   snapshots.
2. Add one coordinator-internal, non-authoritative hosted compiler product,
   `wvbind`. It consumes the retained WVSS, WVTD, and WVFC as untrusted semantic
   inputs and, only after complete semantic-binding success, writes one exact
   bounded canonical evidence line to standard output. That line contains the
   byte length and lowercase SHA-256 digest of each consumed input plus the
   bound Foreign count. It does not write a successor or readback file, accept
   WVAE, lock, or profile input, claim that authentication occurred, or provide
   a public admitted-source route.
   Its exact private invocation is:

   ```text
   wvbind <input.wvss> <input.wvtd> <input.wvfc>
   ```

   The exact success evidence is:

   ```text
   foreign binding status=Published source-bytes=<decimal-u32> source-sha256=<64-lowercase-hex> target-bytes=<decimal-u32> target-sha256=<64-lowercase-hex> catalog-bytes=<decimal-u32> catalog-sha256=<64-lowercase-hex> foreign-count=<decimal-u32>\n
   ```

   It is one newline-terminated UTF-8 line with no prefix, suffix, additional
   output, alternate whitespace, or alternate numeric or hex spelling. Direct
   `wvbind` invocation has no admission authority; the evidence is meaningful
   only to the coordinator that independently computes the expected line from
   the retained authenticated bytes.
3. The compiler-owned adapter hosted by `wvbind` validates the complete WVSS,
   WVTD, and WVFC
   structure, maps every source-ordered WVFC record to exactly one parsed Foreign
   declaration and WVSD entry, rechecks the source spans and registered
   identities needed to construct semantic facts, and rejects omissions,
   extras, reordering, duplicates, mixed modules, mismatched declaration
   ordinals, and inconsistent target or callable facts. It neither invokes the
   canonical catalog producer nor treats a valid catalog as authority.
   After complete source/catalog correspondence and before constructing source
   symbols, it rejects a Foreign count greater than the WVSS module count. The
   current registered declaration has one exact source name and the function
   namespace is unique within a module, so no source exceeding that bound can
   complete symbol binding. This rejection is `Catalogˉdirectory`, reports the
   module-count sentinel as `Failureˉmodule`, zero declaration and directory
   entry, the Foreign count as `Failureˉcatalogˉrecord`, and WVFC offset `12`
   for the record-count field. Symbol and binding statuses remain `Valid`
   because neither phase has run.
4. Define the production coordinator order for a nonempty catalog exactly as:

   ```text
   retain WVAE + WVSS + WVTD + WVFC + lock + profile
       -> wvauth over those exact six snapshots
       -> recheck all six retained snapshots
       -> wvbind over retained WVSS + WVTD + WVFC
       -> compare exact wvbind stdout evidence with the retained lengths,
          SHA-256 digests, and catalog count
       -> recheck all six retained snapshots
       -> report Foreignˉloweringˉpending
       -> publish no WVCA, WVLB, WVIR, or WVB
   ```

   Authentication is this control-flow and retained-byte relationship, not a
   property of `wvbind` or its evidence line. Authentication,
   retained-snapshot, or binder failure takes precedence over the named
   lowering stop.
5. Keep the empty-catalog route exactly as selected by Decision 0893. A
   canonical 48-byte zero-record WVFC does not launch `wvbind`,
   does not change symbol or lookup versions, and proceeds through the existing
   Analyzer and emitter path. For every retained empty-catalog fixture, WVSS,
   WVCA 1.0, WVLB, WVIR, and WVB bytes must remain byte-identical to the
   Decision 0893 baseline. The ordinary `--internal-source-set` route rejects
   any foreign-bearing WVSS with
   `Foreignˉrequiresˉauthenticatedˉbinding`; the descriptorless Project 2
   route continues to reject every Foreign declaration during its raw
   precheck.
6. Extend the internal declaration directory with stable Foreign kind value
   `9`. Select WVSD 1.2 only when at least one Foreign declaration exists;
   otherwise publish the existing WVSD 1.1 bytes. WVSD 1.2 retains the exact
   16-byte header and 24-byte entry layout. It changes only the admitted kind
   set and requires every kind-9 entry to correspond canonically to one parsed
   Foreign declaration. Imports remain excluded.
7. Select private lookup index WVSI 1.3 only with WVSD 1.2; otherwise retain
   WVSI 1.2 bytes. WVSI 1.3 retains the WVSI 1.2 layout, includes every Foreign
   declaration exactly once in its callable buckets, preserves export metadata
   bit 0, requires asynchronous metadata bit 1 to be zero for Foreign, and
   keeps bits 2 through 7 zero. A WVSD/WVSI version mismatch or a Foreign entry
   in WVSD 1.1 or WVSI 1.2 is invalid. Before lookup is used, structural
   validation checks exact version coupling, module and entry counts, bounded
   primary and callable ranges, in-range payload indices, nominal
   forward/reverse bounds, metadata bits, canonical contiguous emitted-name
   offsets and `1..255` lengths, and exact end-of-value consumption. Truncation
   and trailing bytes are invalid.
8. Count Foreign declarations separately from ordinary functions. They
   contribute WVSD entries and the existing total-directory/evidence bounds,
   but do not increment the ordinary function count and do not create a
   function body, local-binding range, WIR-defined function, closure target, or
   generic specialization.
9. Add internal callable kind `Foreign = 7`. Foreign names participate in the
   existing global function namespace and in the callable-constructor
   collision namespace shared with ordinary functions and record
   constructors. Existing duplicate-function, constructor-conflict,
   reserved-name, visibility, module-import, and export failure rules apply in
   their established order; this checkpoint does not renumber retained status
   values. A non-root Foreign declaration uses emitted-machine-name tag `X`
   rather than the ordinary function tag `F`, producing
   `__WvM<module>X<foreign-ordinal>` before the existing collision suffix rule.
10. A Foreign callable has exact arity three. Positional calls retain source
    order. Named arguments accept only the exact UTF-8 labels `Destination`,
    `Capacity`, and `Expectedˉgeneration`, mapped to indices zero, one, and
    two. Arity mismatch retains `Callˉarity = 15`. With arity three, a
    duplicate, unknown, missing, or otherwise non-exact label set uses appended
    `Invalidˉargument = 16`; no retained status is renumbered. An explicit
    generic call resolving to Foreign uses `Unknownˉcall`.
11. A Foreign declaration is callable-only. Its name cannot be bound as a
    first-class function value, captured by a closure, stored, returned, passed
    indirectly, used as a generic callable argument, or selected by ordinary
    function-reference or indirect-call lowering. Only direct call-target
    lookup may return callable kind Foreign.
12. After source/catalog/target correspondence is established, construct these
    exact normalized callable facts for every current Foreign declaration:

    | Fact | Required value |
    | --- | --- |
    | ABI contract | `COMPILER_SOURCE_FOREIGN_ABI_CONTRACT_BUFFER_SYSV_AMD64_C_V1` (`1`) |
    | Source-name identity | `COMPILER_SOURCE_FOREIGN_SEMANTICS_SOURCE_NAME_BUFFER_READ` (`1`) |
    | External-symbol identity | `COMPILER_SOURCE_FOREIGN_SEMANTICS_EXTERNAL_SYMBOL_BUFFER_READ` (`1`) |
    | Unsafe | `true` |
    | Profile | System (`3`) |
    | Language effects | `ffi.call` only (`256`) |
    | Destination | non-null Foreign pointer to `u8`, ABI contract `1` |
    | Capacity | scalar `u64` |
    | Expected generation | scalar `u64` |
    | Return | scalar `i64` |
    | No retain | `true` |
    | No unwind | `true` |

    `No retain` and `No unwind` derive from the registered ABI contract and
    exact target contract, not from new source tokens. The adapter must pass
    `Compilerˉsourceˉforeignˉcallableˉisˉexact` and independently require
    both Boolean facts. It does not invoke live memory or provider-call
    admission merely to validate a declaration. These normalized values are
    non-authoritative semantic facts. A varied negative-fact fixture proves
    rejection of the mismatch only; neither a matching fact set nor its
    negative test proves authentication, admission, capability grant, or
    permission to execute.
13. The exact supported target remains Linux x86-64, little-endian,
    `sysv_amd64_c_v1`, 64 address bits, with no-unwind C scalar-pointer major
    version `1`. A structure-valid WVTD naming any other target cannot produce
    a Foreign binding.
14. Complete symbol and callable binding for the source set before reporting
    success from `wvbind`. A bound direct Foreign call
    is recognized as Foreign, but typed pointer/region/scratch construction and
    call lowering remain unavailable. The coordinator therefore terminates a
    successfully bound nonempty candidate at exact
    `Foreignˉloweringˉpending`. It launches no ordinary analysis publication or
    emitter child. Once complete source-symbol and body-binding validation has
    succeeded, the adapter checks each already-corresponded Foreign callable
    through its exact WVSD directory entry. It does not resolve the same name a
    second time through the 8-bit callable hash bucket.
15. Do not assign a foreign WIR opcode, reuse function call `62` or capability
    call `63`, place ABI or catalog identity in an existing WIR target/auxiliary
    field, or add data to an existing WVLB, WVIR, WVCA, WVEF, or WVB version.
    The current `ffi.call` language-effect bit `256` and WVEF 1.0 mask remain
    unchanged because no foreign-call operation is published.
16. `wvbind` writes no success evidence until semantic binding completes. On
    success it writes exactly the one canonical line defined above and exits
    successfully. Missing, malformed, mismatched, partial, duplicated, or extra
    standard output is failure. Diagnostics remain separately bounded, and the
    coordinator treats timeout, descendant-process, output-limit, or nonzero
    exit as failure. The evidence line neither mutates retained inputs nor
    creates an aliasable output path.

## Resource geometry

The Decision 0893 outer immutable six-input limits remain unchanged:

- WVAE is exactly 224 bytes;
- WVSS is 37 through 4,194,304 bytes with 1 through 64 modules;
- WVTD is 64 through 320 bytes;
- WVFC input is 48 through 4,194,304 bytes before structural validation, with a
  header count field bounded to at most 43,690 records;
- the source-input lock is 1 through 1,048,576 bytes;
- the selected profile is 1 through 65,536 bytes; and
- all six retained inputs total at most 9,503,264 bytes.

Because WVFC is exactly `48 + RecordCount * 96`, the largest structurally valid
catalog is 4,194,288 bytes at 43,690 records. A 4,194,304-byte WVFC cannot match
that geometry and is necessarily trailing-invalid even though the outer
admission boundary accepts its length for structural diagnosis. The count
ceiling is a catalog-format bound; it is not a claim that 43,690 corresponding
Foreign declarations can fit in one bounded WVSS.

`wvbind` therefore retains at most 8,388,912 bytes of
direct structurally valid WVSS, WVTD, and WVFC input. WVSS and WVFC structural
and correspondence scans are linear in their input bytes, and the adapter's
source-ordered catalog-to-directory merge is linear in the directory and
catalog entry counts. Each record uses constant-time validated WVSD facts after
complete symbol and binding validation; it does not repeat hashed name
resolution. Existing WVSI construction and validation remain bounded but are
not generally linear: emitted-name construction, namespace buckets, and
nominal forward/reverse construction have bounded-superlinear worst cases,
including `O(D^2 + N * D)` work for `D` directory entries and `N` nominal
entries. The adapter must not materialize a second complete WVFC or retain
source text per declaration. Per-record normalized values and failure evidence are bounded;
the existing 4 MiB immutable-value ceiling, 64-module limit, 4,096 source
declarations per module, WVFC record-count field bound, WVSD total-entry bound,
and 16-diagnostic semantic-oracle bound remain effective. The actual maximum
authenticated Foreign count is the smaller value permitted simultaneously by
WVSS syntax/size, per-module declaration bounds, WVSD bounds, and WVFC
correspondence.

Successful standard output is bounded to the single canonical evidence line:
three decimal lengths, three 64-character lowercase SHA-256 digests, one
decimal Foreign count, fixed labels and separators, and one final newline. The
current input and catalog bounds make that line at most 351 UTF-8 bytes. The
coordinator's existing aggregate child-output ceiling remains an outer bound,
not permission for additional success output.

The coordinator retains Decision 0893's 300-second child limit, 65,536-byte
aggregate diagnostic limit, at-most-30-second heartbeat interval, descendant
process-tree termination, five-second settle, and exact cleanup rules. The
integrated Analyzer candidate measured 4,193,688 WVIR bytes and only 616 bytes
of remaining immutable-value headroom. That capacity trigger requires the
separate binder product. Both resulting closures must remain below the 4 MiB
compiler-value ceiling with reported input size, elapsed time, and sampled peak
working set; an older 378,600-byte Analyzer margin is historical context, not
evidence for the changed products.

## Required evidence

Retain the dedicated 24-case, two-fixture
`language-1-authenticated-foreign-binding` verification owner for the portable
adapter's focused semantic boundary. It reconstructs the current compiler pair
once and builds both fixtures with that same identity. Its exact selectors cover
canonical binding; accepted declaration layouts; foreign-free and
foreign-bearing WVSD/WVSI version selection; mixed versions, invalid kinds,
reserved fields, count/length/range/trailing-data faults, and lookup-directory
tampering; Foreign/Foreign, function, and constructor collisions; imported
visibility; exact positional and named calls; all arities zero through four;
first-class value, indirect, and generic-call rejection; catalog order,
correspondence, and normalized-fact tampering; unsupported targets; and the
early Foreign-count semantic bound. The callable-semantics owner additionally
proves the explicit-closure-capture rejection in the closure phase that owns
that rule. These cases do not execute the ordinary Analyzer ingress guard or
claim every capacity and production case listed below. Those cases are
necessary candidate evidence, not the complete acceptance gate. Before
acceptance, that owner together with the overlapping production and compiler
owners must provide:

- deterministic double builds and pinned WVB/application identities for the
  changed Analyzer, `wvbind`, and private coordinator products;
- paired Windows and Linux reports over the same exact fixtures, product
  identities, selected target, statuses, offsets, and output bytes;
- all three accepted frozen declaration layouts, exported and non-exported
  visibility, and exact WVFC-to-WVSD record/directory mapping;
- WVSD 1.1/WVSI 1.2 empty-catalog byte identity and conditional WVSD 1.2/WVSI
  1.3 construction, validation, mixed-version rejection, truncation, trailing
  data, invalid kind, missing entry, extra entry, and reordered entry cases;
- same-name Foreign/Foreign, Foreign/function, and Foreign/record-constructor
  collisions, reserved names, inaccessible imports, and exported imports;
- exact positional and named-argument binding, all three valid labels, wrong
  case, unknown/duplicate/missing labels, arity zero through four, and rejection
  of every first-class, closure, indirect, and generic-callable use;
- exact normalized ABI, source name, external symbol, profile, effect, four
  types, no-retain, and no-unwind facts, with one independently varied negative
  case for each fact and an assertion that neither positive nor negative fact
  evidence confers admission authority;
- missing, extra, reordered, duplicate, cross-module, span-tampered,
  digest-tampered, source-name-tampered, symbol-tampered, effect-tampered,
  wrong-target, and unsupported-ABI catalogs;
- structurally valid zero-, one-, and 43,690-record WVFC values, one-past count
  rejection, exact 4,194,288-byte valid geometry, necessarily trailing-invalid
  4,194,304-byte geometry, feasible end-to-end WVSS/WVFC correspondence bounds,
  post-correspondence rejection when Foreign count exceeds module count with
  exact `Catalogˉdirectory` status and WVFC offset `12`,
  exact WVSS byte ceilings, 64/65 modules, 4,096/4,097 declarations in one module,
  bounded diagnostic output, timeout, descendant termination, and cleanup;
- sentinels proving `wvauth` precedes `wvbind`, all six retained
  snapshots are rechecked both before and after it, the exact canonical
  `wvbind` evidence line is compared with coordinator-computed lengths, digests,
  and Foreign count, and no ordinary Analyzer publication or emitter is
  launched after `Foreignˉloweringˉpending`;
- tamper attempts before authentication, between authentication and binding,
  during `wvbind`, and after binding; missing, malformed, mismatched, partial,
  duplicated, extra, and oversized success output; each with destination
  preservation and no transferable success marker; and
- byte-for-byte comparison of the retained empty-catalog WVSS, WVCA, WVLB,
  WVIR, and WVB corpus against the Decision 0893 baseline.

Changed-file planning must route the new decision, `wvbind` adapter, symbol,
binding, driver, and coordinator paths to this owner and to the existing
system-FFI front-end, foreign-memory semantics, callable semantics, Language
1.0 front-door, compiler-split, and production-admission owners where their
contracts overlap. A broader qualification run is not a substitute for the
focused malformed-input and ordering evidence.

## Consequences

The production path can prove that every authenticated foreign declaration has
one exact semantic symbol and normalized direct-call identity without
pretending that a native call exists. Direct invocation of the internal
`wvbind` product still produces only untrusted digest evidence; it cannot cause
the public coordinator to publish WVB without independent authentication and
unchanged retained snapshots.

Ordinary and empty-catalog source retains its current exact symbol, lookup,
analysis, and bytecode bytes. Foreign-bearing source selects explicit internal
symbol and lookup versions, so old validators reject it rather than
misinterpreting kind zero or silently omitting a declaration.

The next lowering decision begins from a distinct Foreign callable instead of
overloading an ordinary function or capability. It must define the paired
WVLB/WVIR/WVCA identities, emitter comparison against retained WVTD/WVFC, a
typed foreign-call operation, WVB imports, native ABI lowering, and containment
before removing `Foreignˉloweringˉpending`.

## Nonclaims

This checkpoint does not publish foreign semantic authority, change WVFC or
WVTD, add a serialized authentication format, lower foreign pointer, nullable
pointer, scratch, region, generation, borrow, alias, address, or recovery
semantics, infer or propagate a published foreign-call operation, change WVLB,
WVIR, WVCA, WVEF, or WVB, emit a foreign import or native thunk, resolve a
dynamic library or symbol, authorize execution, grant a capability, perform a
native ABI call, contain a provider, complete Slice 8, accept Decision 0893, or
qualify the whole Language 1.0 compiler.

## Reconsideration triggers

The integrated-Analyzer capacity trigger has occurred: 4,193,688 WVIR bytes
left only 616 bytes beneath the immutable-value ceiling, so the hosted adapter
is now a separate product. Reconsider this split if `wvbind` cannot stay
bounded without duplicating semantic ownership; if conditional WVSD/WVSI
versions cause ordinary-source byte drift; if
exact record-to-symbol mapping requires a second full catalog or superlinear
work; if the private host cannot prove which WVSS/WVTD/WVFC bytes `wvbind`
consumed; if Foreign must become a first-class callable to implement the frozen
paper workload; if the registered ABI changes its no-retain or no-unwind
contract; or if Windows and Linux differ in namespace order, normalized facts,
failure precedence, offsets, canonical digest evidence, resource bounds, or
cleanup.
