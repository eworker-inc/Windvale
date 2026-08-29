# Decision 0886: make target and foreign admission a mandatory Language 1.0 phase

## Status

Accepted architecture direction on 2026-08-29. The format layouts remain
conditional on the implementation capacity and malformed-input evidence below;
no serialized format, compiler product identity, implementation, or cross-host
result is published by this decision alone.

## Context

[Decision 0781](0781-Separate-Language-1.0-Profile-Admission-Product.md)
already gives descriptor-bearing Language 1.0 source one independently
packageable `wvadmit` phase before analysis. It validates the locked composite
source profile and publishes one admitted WVSS source set. Descriptorless
Project 2 compiler development may still enter the analyzer directly.

Slice 8 adds two responsibilities that must precede semantic analysis:

- resolve the build's explicit target against source platform scopes without
  inferring the current host; and
- recognize a complete, bounded catalog of syntactic foreign declarations
  before any call, pointer, effect, WIR, import, or native binding is admitted.

Putting both producers into the analyzer would also consume its scarcest
retained compiler resource. A local audit of the declaration-authentication
candidate measured its complete analyzer WVIR at 4,184,180 bytes under the
unchanged 4,194,304-byte value ceiling, leaving 10,124 bytes. Its 91,843
32-byte operation records occupy 2,938,976 bytes, or about 70.2 percent of the
complete value. The existing admission product's corresponding WVIR is
1,201,328 bytes, leaving 2,992,976 bytes under the same bound. These numbers do
not justify raising the general `bytes` limit or moving semantic work into the
admitter; they justify placing target and syntactic declaration admission at
the phase that already owns source-profile admission.

The analyzer does not currently include the 16,634-byte
`Foundation/Sha256.wv` source closure, whose portable implementation contains a
large unrolled compression function. Importing it merely to authenticate phase
evidence could consume the margin this split is intended to preserve. Digest
validation must therefore use the already implemented bounded
`Bytesˉsha256ˉhex` semantic intrinsic and a small fixed 64-hex-to-32-byte
decoder, not the Foundation source implementation. Exact WIR measurement of
that consumer is an acceptance condition below.

This is distinct from native WVO staging geometry. More WVO resource headroom
does not create WVIR headroom, and a smaller analyzer WVIR does not remove the
need for bounded native staging.

The proposed phase needs non-colliding names. `WVFI 1` already names the
runtime-private 136-byte file-input table in
[the native execution-context contract](../../Specifications/Windvale-Native-Execution-Context.md).
`WVFA 1` already names the native x64 per-function artifact in
[the native lowering contract](../../Specifications/Windvale-Native-X64-Lowering.md).
Neither name is available for foreign or admission evidence. Repository-wide
tracked-text and binary-content searches plus an all-reference `-S` history
search on 2026-08-29 found no existing or historical `WVTD`, `WVFC`, or `WVAE`
identity.

## Decision

### Keep one compiler and make admission the Language 1.0 front door

For descriptor-bearing edition-1 source, and for every source set that declares
a platform scope or foreign declaration, `wvadmit` becomes a mandatory phase
before `wvanalyze`. It consumes an exact caller-selected target descriptor in
addition to the existing source-input lock, selected source profile, and
ordered source closure. It publishes four separate immutable values:

1. admitted descriptor-free WVSS;
2. the validated target descriptor, byte-for-byte;
3. a syntactic foreign-declaration catalog; and
4. one small admission-evidence envelope that binds the preceding three values
   to the selected lock and source profile.

The diagnostic one-shot compiler calls the same admission, analysis, and
emission functions in memory. This phase boundary does not create another
grammar, lexer, declaration parser, semantic analyzer, optimizer, emitter, or
ABI implementation.

The descriptorless Project 2 route may remain an internal bootstrap and
compiler-development path while it claims no edition-1 profile, platform, or
foreign admission. It must reject a System profile, platform declaration, or
foreign token rather than bypass the mandatory phase. Release, package, and
Language 1.0 conformance front doors always begin with admission.

### Reserve three internal format identities

Reserve these internal compiler-phase identities:

| Identity | Version | Role |
| --- | --- | --- |
| `WVTD` | 1.0 | one canonical caller-selected target descriptor |
| `WVFC` | 1.0 | one canonical source-ordered syntactic foreign catalog |
| `WVAE` | 1.0 | one fixed admission-evidence envelope |

All multibyte integers are unsigned little-endian. Every value carries its
four-byte magic, major and minor version, exact total length, checked counts
and offsets, and zero reserved fields. An unknown major or minor version,
unknown flag, noncanonical order, duplicate identity, misaligned or overlapping
range, arithmetic overflow, trailing byte, or nonzero reserved field is
invalid. These are internal phase formats, not package or distribution
formats; canonical WVB remains the distribution contract.

`WVAE` is not a container. WVSS, WVTD, and WVFC remain separate bounded values
and are validated independently. A digest in WVAE proves that the values belong
together; it does not make malformed content semantically trustworthy.

### Define `WVTD 1.0` as target evidence, not host state

`WVTD 1.0` begins with this 64-byte header:

| Offset | Bytes | Field |
| ---: | ---: | --- |
| 0 | 4 | magic `WVTD` |
| 4 | 2 | major version `1` |
| 6 | 2 | minor version `0` |
| 8 | 4 | complete byte length |
| 12 | 4 | registered concrete build-target identity |
| 16 | 4 | environment identity |
| 20 | 4 | architecture identity |
| 24 | 4 | ABI identity |
| 28 | 4 | address width in bits |
| 32 | 4 | byte-order identity |
| 36 | 4 | no-unwind scalar/pointer interface major |
| 40 | 4 | extension-identity count |
| 44 | 4 | target-interface-identity count |
| 48 | 4 | extension directory offset, or zero when empty |
| 52 | 4 | target-interface directory offset, or zero when empty |
| 56 | 4 | flags, zero in version 1.0 |
| 60 | 4 | reserved zero |

Each optional directory is a contiguous sequence of four-byte registry
identities in strictly increasing order. The nonempty extension directory
begins at byte 64; the nonempty target-interface directory immediately follows
it, or begins at byte 64 when the extension directory is empty. An empty
directory has offset zero. Each count is at most 32 and the
complete length is exactly `64 + 4 * (extension count + target-interface
count)`, at most 320 bytes. Version 1.0 admits only identities known to the
compiler's exact target registry. The first concrete foreign target is
`linux.x86_64.sysv_amd64_c_v1`: Linux, x86-64, 64-bit addresses, little endian,
the registered SysV AMD64 C ABI, no-unwind scalar/pointer interface major 1,
and empty extension and target-interface directories.

WVTD 1.0 always describes one concrete build target. There is no portable,
host-agnostic, unknown, or wildcard WVTD. The split emitter target name
`portable-wvb-optimized-v1` selects a WVB publication mode; it is not an
environment or a substitute target descriptor. A WVB is portable across a set
of concrete targets only when its admitted source and retained requirements
derive that property. The build still performs target admission independently
for each concrete WVTD, even when two builds later produce byte-identical WVB.

Environment and architecture identities, address width, and byte order must be
known and concrete. ABI identity may be the exact registered `none` identity
only when the target and source require no concrete foreign ABI; its no-unwind
interface major is then zero. Unknown extension or target-interface identities
are always invalid. For every module, `wvadmit` evaluates each declared
platform key as its registry predicate over the complete WVTD and requires at
least one alternative to match. The concrete System ABI key matches only the
Linux/x86-64/SysV/no-unwind-major-1 combination above. A concrete foreign
declaration must resolve to that exact matching predicate.

`wvadmit` copies accepted WVTD bytes unchanged; it never substitutes the host,
completes omitted fields, treats a portable emission mode as a target, or
rewrites an unsupported target.

Before this candidate format is published, the target registry must assign the
stable numeric concrete build-target, environment, architecture, ABI, byte-
order, extension, and target-interface identities used by WVTD. The existing
textual source-scope registry continues to map each source key to one exact
predicate over those fields; a scope key is not field 12. This draft does not
assign the numeric registry values.

### Define `WVFC 1.0` as declaration-only evidence

`WVFC 1.0` has a 48-byte header followed by fixed 96-byte records:

| Header offset | Bytes | Field |
| ---: | ---: | --- |
| 0 | 4 | magic `WVFC` |
| 4 | 2 | major version `1` |
| 6 | 2 | minor version `0` |
| 8 | 4 | complete byte length |
| 12 | 4 | foreign-declaration count |
| 16 | 4 | record size `96` |
| 20 | 4 | record offset `48` |
| 24 | 4 | WVSS module count |
| 28 | 4 | flags, zero in version 1.0 |
| 32 | 16 | reserved zero |

Each record contains, in order:

- WVSS module ordinal and source declaration ordinal;
- flags bit 0 for required `unsafe` and bit 1 for `export`, with all other bits
  zero;
- the registered ABI-contract identity;
- declaration, source name, ABI literal, external-symbol literal, signature,
  and effect-clause offset/length pairs into that module's admitted source; and
- SHA-256 of the exact admitted signature span.

| Record offset | Bytes | Field |
| ---: | ---: | --- |
| 0 | 4 | WVSS module ordinal |
| 4 | 4 | source declaration ordinal |
| 8 | 4 | flags |
| 12 | 4 | registered ABI-contract identity |
| 16 | 4 | declaration offset |
| 20 | 4 | declaration length |
| 24 | 4 | source-name offset |
| 28 | 4 | source-name length |
| 32 | 4 | ABI-literal offset |
| 36 | 4 | ABI-literal length |
| 40 | 4 | external-symbol-literal offset |
| 44 | 4 | external-symbol-literal length |
| 48 | 4 | signature offset |
| 52 | 4 | signature length |
| 56 | 4 | effect-clause offset |
| 60 | 4 | effect-clause length |
| 64 | 32 | SHA-256 of the exact signature span |

Every span must be nonempty where the grammar requires content, contained in
exactly one WVSS module, and contained in the declaration span. Catalog records
are ordered by module ordinal and then declaration ordinal. They neither copy
source text nor carry native addresses, resolved library handles, linker
ordinals, call sites, pointer values, grants, or runtime authority.

The complete WVFC value is at most 4,194,304 bytes. Its count must equal
`(complete length - 48) / 96`, fit the retained source-declaration bound, and
be at most 43,690. It must also equal the complete foreign-declaration scan of
WVSS. Zero declarations produce the canonical 48-byte empty catalog. Duplicate,
omitted, reordered, overlapping, or extra declarations are invalid.

The catalog records syntax only. The registered ABI identity says which exact
contract the spelling requested; it does not prove that the declaration's
types, effects, target, ownership, or later uses are semantically valid.

### Define `WVAE 1.0` as a host-independent envelope

`WVAE 1.0` is exactly 224 bytes:

| Offset | Bytes | Field |
| ---: | ---: | --- |
| 0 | 4 | magic `WVAE` |
| 4 | 2 | major version `1` |
| 6 | 2 | minor version `0` |
| 8 | 4 | structure length `224` |
| 12 | 4 | hash identity `1` for SHA-256 |
| 16 | 4 | flags, zero in version 1.0 |
| 20 | 4 | WVSS byte length |
| 24 | 4 | WVTD byte length |
| 28 | 4 | WVFC byte length |
| 32 | 4 | WVSS module count |
| 36 | 4 | foreign-declaration count |
| 40 | 4 | admitted edition, exactly `1` |
| 44 | 4 | admitted source-profile binding |
| 48 | 16 | reserved zero |
| 64 | 32 | SHA-256 of exact WVSS bytes |
| 96 | 32 | SHA-256 of exact WVTD bytes |
| 128 | 32 | SHA-256 of exact WVFC bytes |
| 160 | 32 | SHA-256 of exact source-input-lock bytes |
| 192 | 32 | SHA-256 of exact selected source-profile bytes |

WVAE deliberately omits the host and executable identity, so independent
Windows and Linux admission products can publish byte-identical evidence.
Development and qualification cache keys bind the exact admitter producer
identity separately. A cache identity must never substitute for format and
semantic validation.

The analyzer computes every 32-byte digest in this format, and every WVFC
signature digest it authenticates, through the existing bounded
`Bytesˉsha256ˉhex` intrinsic. A fixed decoder requires exactly 64 lowercase
ASCII hexadecimal bytes and converts them to the 32 raw bytes stored above.
The consumer must not import `Foundation/Sha256.wv`, call a host hashing
service, or accept a producer-supplied digest without recomputation. Before the
format is implemented, the focused capacity case must measure the intrinsic
calls, decoder, and comparisons in the complete Analyzer WVIR and prove the
result remains below 4,194,304 bytes. If it does not, this format is not
published; Slice 8 must choose a separately bounded independent validator or a
new evidence version rather than widening the Analyzer or falling back to the
Foundation source closure.

### Authenticate the immutable handoff at both sides

Admission performs these steps before publishing anything:

1. snapshot and validate the source-input lock, selected profile, ordered source
   closure, and exact WVTD;
2. construct descriptor-free WVSS and validate every module's edition and
   profile agreement;
3. scan all module headers and platform scopes, reject unknown or duplicate
   scope evidence, and require the selected WVTD to satisfy every scope;
4. scan every foreign declaration, require System profile, `unsafe`, a known
   ABI contract, an exact target predicate, canonical source spans, and bounded
   declaration evidence;
5. publish canonical WVFC, then construct WVAE over the immutable snapshots;
   and
6. publish the four values as one all-or-nothing private phase result.

The coordinator writes into one private temporary directory, syncs complete
values before publication, and removes only its exact private candidate after
success, failure, or a lost publication race. No WVCA, WVLB, WVIR, WVB, object,
or native image is published after admission failure. The coordinator retains
the exact lock and source-profile snapshots in that private phase directory and
passes those snapshots, not reread ambient paths, to analysis for WVAE
verification.

The analyzer treats all four supplied values as untrusted. Before semantic
analysis it:

1. validates the complete WVAE structure and recomputes all five digests;
2. validates WVSS, WVTD, and WVFC independently with checked arithmetic;
3. authenticates every WVFC source span against the exact WVSS module;
4. performs one bounded canonical foreign-token scan and proves catalog
   completeness; and
5. revalidates each claimed foreign declaration through a small independent
   declaration-evidence verifier before using any catalog field.

The analyzer must not accept a digest as proof of syntax, trust a private path
as proof of provenance, or retain a complete second catalog-producing parser.
The producer owns normalization and catalog construction; the consumer owns
bounded structural and source-span authentication. This is the same distrust
principle used at the WVCA/WVLB/WVIR boundary, with a smaller verifier than the
producer.

The resulting pipeline is:

```text
ordered source + lock + profile + exact WVTD
    -> wvadmit
    -> retained lock/profile snapshots + WVSS + WVTD + WVFC + WVAE
    -> wvanalyze
    -> WVCA + WVLB + WVIR
    -> wvemit
    -> canonical WVB
```

Target and foreign semantic identities retained in WVLB/WVIR must compare with
the original WVTD/WVFC before target-aware emission. The exact paired WVLB,
WVIR, and WVB version changes belong to later Slice 8 decisions; this decision
does not silently add fields to an existing version.

### Keep semantic foreign work in the analyzer

The analyzer continues to own all work whose answer depends on program meaning:

- the distinct foreign declaration symbol kind and namespace rules;
- parameter and return type resolution against the registered ABI contract;
- declaration-to-call binding and exact external-symbol identity;
- `unsafe` call-site admission and exact `ffi.call` effect propagation;
- nullable and non-null foreign pointer distinctions;
- scratch and region ownership, alignment, checked address arithmetic, lexical
  borrow lifetime, exclusivity, aliasing, generation, and escape rejection;
- recoverable returned-data validation versus terminal ABI-contract failure;
- target-specific typed WIR construction; and
- the semantic evidence later consumed by WVB foreign imports, native thunks,
  linker imports, and containment boundaries.

The admitter does not infer effects, select overloads, resolve call sites,
prove pointer safety, create WIR, bind a dynamic library, look up an external
symbol, grant a capability, or authorize execution. A valid WVTD/WVFC/WVAE set
means only that the build target and complete foreign declaration surface are
syntactically known and mutually bound to the admitted source.

### Version and failure policy

The command-line admission product advances to a new exact producer identity
and accepts an input WVTD path plus output paths for WVSS, the byte-identical
WVTD snapshot, WVFC, and WVAE. The analyzer gains one explicit admitted
Language 1.0 mode that requires the exact lock and profile snapshots plus all
four values. Missing, extra, or mixed-version values reject; there is no
implicit empty catalog and no host-default target.

Failures distinguish at least invalid target structure, unsupported target,
source/target mismatch, foreign declaration outside System, unknown ABI,
foreign/target mismatch, invalid catalog structure, source/catalog mismatch,
incomplete or duplicate catalog, invalid envelope, digest mismatch, and
resource limit. Diagnostics name the admission or analysis-validation phase
and a bounded source span where one is authenticated. Failure publishes no
partial successor artifact.

Existing version-1 admission producer identities and profile-aware cache
families become cold. They are not consulted for a target-aware request. The
early project has no compatibility requirement that justifies retaining a
dual decoder or legacy System path. Historical artifacts remain historical
evidence, while normal development advances atomically to the new producer and
cache namespace.

## Consequences

- Target selection becomes an explicit, host-independent compiler input before
  analysis; a Linux foreign declaration cannot compile merely because the
  compiler happens to run on Linux.
- Complete syntactic foreign inventory and target rejection move into the
  smaller product with measured compiler-scale capacity, while the analyzer
  keeps the semantic work that cannot be decided earlier.
- The analyzer still independently authenticates untrusted phase artifacts,
  but it need not retain the complete target/profile/catalog producer closure.
- WVTD, WVFC, and WVAE remain separate values under explicit limits. No
  concatenation widens the ordinary 4 MiB Windvale value contract.
- The phase adds one process and several small values to a cold build. Exact
  cache keys or a retained compiler service may later remove that startup cost
  without merging phase ownership.
- `WVFI` and `WVFA` retain their existing native runtime and lowering meanings.
- No foreign provider authority, runtime symbol lookup, native import, thunk,
  containment mechanism, or real FFI execution is implemented by this
  architecture decision.

## Migration and verification requirements

Implementation must proceed as one coherent producer/consumer migration:

1. publish the target registry and exact WVTD/WVFC/WVAE format specifications;
2. add bounded valid, empty, boundary, truncated, oversized, reordered,
   duplicate, unknown-version, reserved-field, overflow, digest-mismatch,
   forged-span, omitted-entry, extra-entry, and target-mismatch fixtures;
3. build the catalog in the admission product and authenticate it in the
   analyzer through independent code paths;
4. remove the old System/foreign bypass and advance producer, cache, planner,
   and convergence identities together;
5. require byte-identical admitted evidence and successor analyzer products on
   Windows and Linux; and
6. measure both products' WIR, WVB, native package, elapsed time, and sampled
   working set before and after the migration without presenting a contended
   timing run as a speedup.

The focused target/foreign owner may test producer and consumer corruption
cases independently, but final Slice 8 integration must also prove exact split
fixed points, current-source verifier admission, self-hosting, WVB/linker
foreign evidence, real and hostile containment workloads, and paired-host
qualification at their named later boundaries.

## Reconsideration triggers

Reconsider the product geometry if the admission product approaches its own
retained WIR or native-package limit, if independent catalog authentication
duplicates enough parsing to erase the measured capacity benefit, or if a
retained compiler service can preserve the same immutable validation boundary
with materially lower process cost.

Version WVTD when a new target cannot be represented by the fixed registry and
directory shape. Version WVFC when another implemented ABI requires declaration
evidence that the fixed record cannot express. Version WVAE when the set-binding
contract needs another independently provable value; do not repurpose reserved
bytes or weaken exact rejection.

Reconsider the 4 MiB WVFC maximum only with a representative source workload,
streaming or segmented validation design, explicit memory evidence, and paired-
host qualification. Do not reuse `WVFI` or `WVFA` even if their current owners
later change, because published magic identities retain historical meaning.
