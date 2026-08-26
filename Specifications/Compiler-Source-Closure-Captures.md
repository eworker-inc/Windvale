# Windvale source closure-capture analysis

## Status and boundary

`Compilerˉsourceˉclosureˉcaptures` implements the bounded lexical-capture
analysis for one parsed Language 1.0 closure expression. It consumes validated
WVSS, WVSD, and WVLB evidence plus the owning module and function identity. It
does not infer a capture, retain ambient state, or turn a module capability root
into a lexical value.

This phase validates explicit capture lists, closure parameters, the isolated
closure body, and the declared exact effect clause. WVIR 1.19/1.20 and WVB 1.31
define and execute a representation-private environment for inline scalar and
enum captures. The compiler-private WVCL 1.0 catalog assigns deterministic
source-site ordinals, WVLB 1.4 publishes the matching synthetic binding ranges,
and WVIR emits one `Closureˉcreate` plus one synthetic physical function per
admitted site. No WVB serializes either compiler-private capture or WVCL
evidence.

## Capture modes

The source list admits these exact forms:

| Source form | Mode | Closure-scope binding |
| --- | ---: | --- |
| `copy Name` | 0 | immutable copied value |
| `move Name` | 1 | immutable owned value |
| `borrow Name` | 2 | immutable borrowed value |
| `borrow mut Name` | 3 | mutable exclusive borrowed value |

Every named capture must resolve to a lexical local visible at the closure
expression. Names are unique across the capture list. A closure parameter may
not duplicate a capture or another parameter. Closure parameters remain
ordinary immutable parameters; only `borrow mut` produces a mutable captured
binding.

`copy` is admitted only for a conservatively proven Copy or shared-immutable
shape. The current proof admits primitive value shapes except `never`, all
enums, `text`, and `bytes`. Unproven records, variants, collections, builders,
resources, and private compiler shapes reject instead of being copied
implicitly. `borrow mut` requires a directly mutable outer `var`.

The executable WVB 1.31 subset is intentionally narrower than source-level
capture analysis. It admits `copy`, `move`, and immutable `borrow` only for
inline scalar and enum cells. In particular, `text` and `bytes` remain valid
source copies but do not yet enter a runtime environment, because doing so
requires explicit retain/release and escape rules. Aggregates, collections,
callable values, resource owners, and `borrow mut` remain outside this
checkpoint.

`move` transfers the inline value into the closure environment and invalidates
the outer slot. Persisted WVIR validation follows every reachable control-flow
path, including joins and loops: a later load rejects, a local store explicitly
reinitializes the slot, and disagreeing states at a forward join or backward
edge reject. This proof is independent of the source lowering that produced the
directory.

Immutable borrow is admitted only from an outer parameter or `let`. For this
inline immutable profile, the runtime representation is an observationally
equivalent value snapshot, while persisted WVIR still proves the source borrow
contract: the created callable has exactly one immutable local destination,
that local does not outlive the captured owner, and its only loads feed
`Callˉindirect`. Returning, copying, or otherwise escaping that callable
rejects. A real write-through representation is required before `borrow mut`
can enter the executable profile.

An `async` closure rejects both borrowed capture modes. This is deliberately
stricter than the eventual complete structured-task proof: Slice 7 may admit a
borrow only when one lexical task scope proves that its owner remains live and
immobile across every suspension and join. Until that proof exists, rejection
prevents an unsound escape.

## Isolated binding and capability roots

The analyzer builds a fresh local-binding phase containing only the explicit
captures and parameters, then binds the closure body against that phase. A body
reference to an outer local absent from the list reports `Missingˉcapture`.
Other body-binding failures preserve the underlying WVLB status and source
position.

A required module capability root is resolved by the ordinary module symbol and
binding evidence. It is not inserted into the closure-local phase and therefore
does not require a capture entry. Calls through it remain visible in the
closure's exact capability-effect mask. A rights-reduced provider or capability
instance stored in a local is an ordinary lexical value and must be captured
explicitly.

The declared effect clause is resolved with the same canonical registry used by
top-level functions. The analysis returns separate language-effect and
capability-effect masks. Complete comparison with effects actually used by a
lowered closure body remains part of the WVIR integration checkpoint.

## Evidence and limits

Successful analysis returns one 24-byte entry per capture, in source order:

| Offset | Size | Meaning |
| ---: | ---: | --- |
| 0 | 4 | capture mode |
| 4 | 4 | outer WVLB slot |
| 8 | 4 | exact source shape |
| 12 | 4 | outer declaration offset |
| 16 | 4 | capture-name offset in the owning source |
| 20 | 4 | capture-name byte length |

Capture and parameter lists are each bounded to 64 entries. Capture evidence is
bounded to 1,536 bytes. Every directory status, function range, source span,
count, and retained length is validated before a read or append. Invalid WVSD or
WVLB input rejects as `Invalidˉbindingˉevidence`; it is never interpreted as a
partial directory.

## Focused evidence

The maintained native analyzer self-test covers copied, moved, immutably
borrowed, and mutably borrowed locals; exact outer slot and shape retention;
missing and duplicate captures; conservative Copy rejection; borrowed async
rejection; module capability roots as effects rather than captures;
capture/parameter name conflicts; reconstructed WVLB 1.4 publication; and
forged valid-status/empty-directory evidence. Its ten selectors reuse one
compiler-scale segmented application and return `42`.

The front-door compiler-pipeline helper additionally compiles, verifies, and
executes copied, moved, and immutably borrowed closures, then proves that
use-after-move and mutable borrow publish no WVB. The three accepted modules are
451, 451, and 453 bytes and execute to `42` in 31 guest instructions each. Their
combined five-case development evidence has SHA-256
`592cb889b647a5eccdf2a3a33a8feb148ca2cad4959200200d0f9f16ad5ec139`.
This is current-host evidence, not cross-host qualification.
