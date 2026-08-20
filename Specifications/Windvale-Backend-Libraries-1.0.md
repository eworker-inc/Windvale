# Windvale Backend Libraries 1.0

## Status and purpose

- Status: Draft API catalog for owner review
- Suite: [Windvale Libraries 1.0](Windvale-Libraries-1.0.md)
- Language: [Windvale Language 1.0](Windvale-Language-1.0.md)
- Foundation: [Windvale Language 1.0 Foundation](Windvale-Language-1.0-Foundation.md)
- Binary data: [Windvale binary data profile 1.0](Windvale-Binary-Data-1.0.md)
- Database: [WVDB 1.0 specification plan](../Documents/Project/WVDB-1.0-Specification-Plan.md)

This document defines the first detailed candidate module and operation names
for the Windvale Backend 1.0 profile. It is intended to support useful data
services, HTTPS APIs, browser-application backends, native clients, file and
blob management, diagnostics, and WVDB integration without becoming a server-
side page framework.

The signatures below are public-contract candidates. They are deliberately more
specific than a feature wish list, but they are not implementation claims and
do not yet have accepted signature-set identities. Exact record fields,
ownership returns, effect sets, numeric ceilings, and failure precedence must be
checked against the final Language and Foundation 1.0 registries before freeze.

## API design rules

### Module-qualified names

Applications import one exact module and choose a local alias. Exported names
are concise inside that module:

~~~text
import Dataˉjson as Json;
import Networkˉhttpˉrouter as Router;

let Document = Json.Parse(Budget, Input, Limits);
let Routes = Router.Construct(Budget, Routeˉlimits);
~~~

Official source identifiers start with capital case and use U+02C9 between
semantic words. Persisted protocol names and capability identities retain their
specified ASCII spelling.

### No ambient service discovery

An operation that performs I/O accepts an opaque rights-limited endpoint,
resource, session, or provider binding supplied by the launcher. A string is
never silently interpreted as a host path, listener, database, URL authority,
trust store, private key, cache, or logging sink.

### Common hosted context

Asynchronous provider calls use the canonical Foundation hosted operation
context:

~~~text
Foundationˉoperation.Operationˉcontext
~~~

The context identifies one monotonic-clock generation, absolute deadline, and
cancellation view. A child operation may derive a narrower context but cannot
extend its deadline, replace its provider, or suppress cancellation.

### Common mutation outcome

Where an operation can lose certainty after dispatch, its result uses the
Foundation mutation outcome:

~~~text
export variant Mutationˉoutcome<E> {
    Rejected(Error: E);
    Acceptedˉpartial(Completed: u64, Error: E);
    Completed(Completed: u64);
    Indeterminate(Error: E);
}
~~~

An API may use a narrower operation-specific result when partial progress is
impossible. No convenience operation maps `Indeterminate` to rejection or
retries it automatically.

## Data modules

### `Dataˉjson`

`Dataˉjson` replaces database-specific reuse of JSON parsing with one strict,
portable, bounded owner. WVDB may build JSON columns and protocols on it without
owning general JSON semantics.

Candidate public values:

~~~text
export enum Jsonˉkind: u8 {
    Null;
    Boolean;
    Number;
    String;
    Array;
    Object;
}

export record Jsonˉlimits {
    Maximumˉinputˉbytes: u64;
    Maximumˉdepth: u32;
    Maximumˉvalues: u64;
    Maximumˉobjectˉmembers: u64;
    Maximumˉdecodedˉtextˉbytes: u64;
    Maximumˉoutputˉbytes: u64;
}

export variant Jsonˉfailure {
    Limit(Field: u32, Observed: u64, Maximum: u64);
    Invalidˉutf8(Offset: u64);
    Invalidˉsyntax(Offset: u64, Reason: u32);
    Duplicateˉmember(Offset: u64);
    Numericˉrange(Offset: u64);
    Allocation(Error: Allocationˉfailure);
    Capacity(Error: Collectionˉfailure);
}

export opaque resource Jsonˉdocument;
export opaque Jsonˉvalueˉview Copy;
export opaque Jsonˉmemberˉview Copy;
export opaque resource Jsonˉwriter;
~~~

Candidate operations:

~~~text
export fn Parse(
    Budget: Memoryˉbudget,
    Input: Slice<u8>,
    Limits: Jsonˉlimits,
) -> Result<Jsonˉdocument, Jsonˉfailure> effects(memory.allocate);

export fn Root(
    Document: borrow Jsonˉdocument,
) -> Jsonˉvalueˉview effects();

export fn Kind(Value: Jsonˉvalueˉview) -> Jsonˉkind effects();

export fn Boolean(
    Value: Jsonˉvalueˉview,
) -> Option<bool> effects();

export fn Numberˉtext(
    Value: Jsonˉvalueˉview,
) -> Option<Slice<u8>> effects();

export fn String(
    Budget: Memoryˉbudget,
    Value: Jsonˉvalueˉview,
) -> Result<Option<text>, Jsonˉfailure> effects(memory.allocate);

export fn Arrayˉlength(
    Value: Jsonˉvalueˉview,
) -> Option<u64> effects();

export fn Arrayˉat(
    Value: Jsonˉvalueˉview,
    Index: u64,
) -> Option<Jsonˉvalueˉview> effects();

export fn Objectˉlength(
    Value: Jsonˉvalueˉview,
) -> Option<u64> effects();

export fn Objectˉget(
    Value: Jsonˉvalueˉview,
    Name: borrow text,
) -> Option<Jsonˉvalueˉview> effects();

export fn Objectˉmemberˉat(
    Value: Jsonˉvalueˉview,
    Rank: u64,
) -> Option<Jsonˉmemberˉview> effects();

export fn Objectˉmemberˉname(
    Budget: Memoryˉbudget,
    Member: Jsonˉmemberˉview,
) -> Result<text, Jsonˉfailure> effects(memory.allocate);

export fn Objectˉmemberˉvalue(
    Member: Jsonˉmemberˉview,
) -> Jsonˉvalueˉview effects();

export fn Writerˉconstruct(
    Budget: Memoryˉbudget,
    Limits: Jsonˉlimits,
) -> Result<Jsonˉwriter, Jsonˉfailure> effects(memory.allocate);

export fn Writeˉobjectˉbegin(
    Writer: borrow mut Jsonˉwriter,
) -> Result<unit, Jsonˉfailure> effects();

export fn Writeˉarrayˉbegin(
    Writer: borrow mut Jsonˉwriter,
) -> Result<unit, Jsonˉfailure> effects();

export fn Writeˉname(
    Writer: borrow mut Jsonˉwriter,
    Name: borrow text,
) -> Result<unit, Jsonˉfailure> effects();

export fn Writeˉnull(
    Writer: borrow mut Jsonˉwriter,
) -> Result<unit, Jsonˉfailure> effects();

export fn Writeˉboolean(
    Writer: borrow mut Jsonˉwriter,
    Value: bool,
) -> Result<unit, Jsonˉfailure> effects();

export fn Writeˉinteger(
    Writer: borrow mut Jsonˉwriter,
    Value: i64,
) -> Result<unit, Jsonˉfailure> effects();

export fn Writeˉdecimal(
    Writer: borrow mut Jsonˉwriter,
    Value: Decimal128,
) -> Result<unit, Jsonˉfailure> effects();

export fn Writeˉstring(
    Writer: borrow mut Jsonˉwriter,
    Value: borrow text,
) -> Result<unit, Jsonˉfailure> effects();

export fn Writeˉend(
    Writer: borrow mut Jsonˉwriter,
) -> Result<unit, Jsonˉfailure> effects();

export fn Finish(
    Writer: Jsonˉwriter,
) -> Result<Bytes, Jsonˉfailure> effects();

export fn Encodeˉdeterministic(
    Budget: Memoryˉbudget,
    Document: borrow Jsonˉdocument,
    Maximumˉoutputˉbytes: u64,
) -> Result<Bytes, Jsonˉfailure> effects(memory.allocate);

export fn Encodeˉjcs(
    Budget: Memoryˉbudget,
    Document: borrow Jsonˉdocument,
    Maximumˉoutputˉbytes: u64,
) -> Result<Bytes, Jsonˉjcsˉfailure> effects(memory.allocate);
~~~

The ordinary parser accepts RFC JSON syntax under strict UTF-8, rejects an
unpaired UTF-16 surrogate escape instead of constructing a non-scalar text
value, and preserves the admitted numeric spelling. `Encodeˉdeterministic`
emits UTF-8 without a BOM
or insignificant whitespace, orders object members by the bytewise order of
their decoded UTF-8 names, emits lowercase literals, uses short escapes for
backspace, tab, line feed, form feed, and carriage return, uses lowercase
`\u00xx` for the remaining control characters, and otherwise emits decoded text
as UTF-8 after escaping only quotation mark and reverse solidus. It preserves
each admitted number token, so it does not claim that `1`, `1.0`, and `1e0` are
the same canonical input.

The optional `Encodeˉjcs` operation implements [RFC 8785 JSON Canonicalization
Scheme](https://www.rfc-editor.org/rfc/rfc8785.html) and therefore admits only
its I-JSON and IEEE 754 binary64-compatible input profile. Its number and member-
ordering rules are not silently mixed with the Windvale deterministic profile.
Object iteration is deterministic and duplicate names reject in both profiles.

### `Dataˉcbor`

`Dataˉcbor` is the general binary structured-document companion to JSON. The
focused [binary data profile](Windvale-Binary-Data-1.0.md) selects RFC 8949 CBOR,
defines the narrowed Windvale document and deterministic profiles, and owns the
complete types, limits, failure model, writer, CDDL tooling, and CBOR Sequence
APIs.

The core usage shape is:

~~~text
let Document = Cbor.Admit(Budget, Input, Profile, Limits)?;
let Root = Cbor.Root(Document);
let Name = Cbor.Mapˉgetˉtext(Root, "name");
let Output = Cbor.Encodeˉdeterministic(
    Budget,
    Document,
    Cborˉprofile.Windvaleˉdeterministicˉone,
    Limits,
)?;
~~~

Admission validates one immutable owned buffer and may create a bounded
offset/length/kind index. Observations return views into that admitted document,
so ordinary repeated reads need not build a second object tree. CBOR reduces
text token, escape, decimal-integer, and Base64 work; it does not remove the
need for structural, UTF-8, limits, tag-policy, or application-schema
validation.

`Dataˉcddl` is optional tooling for RFC 8610 schemas over CBOR and JSON.
`Dataˉcborˉsequence` is an optional RFC 8742 incremental-item profile. Neither
module grants I/O authority or determines WVDB's physical storage formats.
The optional `Dataˉcborˉartifact` profile adds validation receipts and bounded
indexes for authenticated immutable content; a signature by itself never skips
CBOR or application-schema validation.

### `Dataˉuri`

URI parsing is pure data processing. Parsing a URI never grants network access
or filesystem authority.

~~~text
export opaque Uri;
export opaque Uriˉorigin;
export opaque Uriˉoriginˉtarget;
export opaque Uriˉquery;

export fn Parse(
    Input: borrow text,
    Limits: Uriˉlimits,
) -> Result<Uri, Uriˉfailure> effects();

export fn Parseˉorigin(
    Input: borrow text,
    Limits: Uriˉlimits,
) -> Result<Uriˉorigin, Uriˉfailure> effects();

export fn Parseˉoriginˉtarget(
    Input: borrow text,
    Limits: Uriˉlimits,
) -> Result<Uriˉoriginˉtarget, Uriˉfailure> effects();

export fn Resolveˉrelative(
    Base: borrow Uri,
    Relative: borrow text,
    Limits: Uriˉlimits,
) -> Result<Uri, Uriˉfailure> effects();

export fn Queryˉparse(
    Budget: Memoryˉbudget,
    Input: borrow text,
    Limits: Queryˉlimits,
) -> Result<Uriˉquery, Uriˉfailure> effects(memory.allocate);

export fn Queryˉget(
    Query: borrow Uriˉquery,
    Name: borrow text,
) -> Option<text> effects();

export fn Queryˉencode(
    Budget: Memoryˉbudget,
    Query: borrow Uriˉquery,
    Maximumˉbytes: u64,
) -> Result<text, Uriˉfailure> effects(memory.allocate);
~~~

The first HTTP service profile accepts origin-form request targets. An HTTP
client receives a separately bound allowed-origin set and never converts a
parsed arbitrary origin into authority.

### `Dataˉvalidation`

This module validates explicit typed values. It does not inspect arbitrary
records through ambient reflection.

~~~text
export opaque resource Validationˉreport;
export record Validationˉlimits {
    Maximumˉfailures: u32;
    Maximumˉpathˉbytes: u64;
    Maximumˉmessageˉbytes: u64;
}

export fn Construct(
    Budget: Memoryˉbudget,
    Limits: Validationˉlimits,
) -> Result<Validationˉreport, Validationˉfailure> effects(memory.allocate);

export fn Add(
    Report: borrow mut Validationˉreport,
    Path: borrow text,
    Code: Validationˉcode,
    Message: borrow text,
) -> Result<unit, Validationˉfailure> effects();

export fn Merge(
    Report: borrow mut Validationˉreport,
    Child: Validationˉreport,
) -> Result<unit, Validationˉfailure> effects();

export fn Isˉvalid(
    Report: borrow Validationˉreport,
) -> bool effects();

export fn Finish(
    Report: Validationˉreport,
) -> Validationˉresult effects();
~~~

Reusable validators should expose domain-specific typed calls such as
`Validateˉemailˉaddress`, `Validateˉpageˉrequest`, or `Validateˉuserˉcommand`
rather than a stringly rule language.

### `Dataˉencoding`

~~~text
export fn Hexˉencode(
    Budget: Memoryˉbudget,
    Input: Slice<u8>,
    Letterˉcase: Hexˉcase,
    Maximumˉoutputˉbytes: u64,
) -> Result<Bytes, Encodingˉfailure> effects(memory.allocate);

export fn Hexˉdecode(
    Budget: Memoryˉbudget,
    Input: Slice<u8>,
    Maximumˉoutputˉbytes: u64,
) -> Result<Bytes, Encodingˉfailure> effects(memory.allocate);

export fn Base64ˉencode(
    Budget: Memoryˉbudget,
    Input: Slice<u8>,
    Maximumˉoutputˉbytes: u64,
) -> Result<Bytes, Encodingˉfailure> effects(memory.allocate);

export fn Base64ˉdecode(
    Budget: Memoryˉbudget,
    Input: Slice<u8>,
    Profile: Base64ˉdecodeˉprofile,
    Maximumˉoutputˉbytes: u64,
) -> Result<Bytes, Encodingˉfailure> effects(memory.allocate);

export fn Base64urlˉencode(
    Budget: Memoryˉbudget,
    Input: Slice<u8>,
    Padding: Base64urlˉpadding,
    Maximumˉoutputˉbytes: u64,
) -> Result<Bytes, Encodingˉfailure> effects(memory.allocate);

export fn Base64urlˉdecode(
    Budget: Memoryˉbudget,
    Input: Slice<u8>,
    Profile: Base64urlˉdecodeˉprofile,
    Maximumˉoutputˉbytes: u64,
) -> Result<Bytes, Encodingˉfailure> effects(memory.allocate);
~~~

Strict and permissive alphabets are separately named profiles. Security
protocols select one exact strict profile and never inherit whitespace or
padding tolerance from a convenience decoder.

### `Dataˉcompression`

The first candidate profiles are gzip and zlib-wrapped deflate. Zstandard,
Brotli, archive containers, and transparent HTTP decompression remain separate
later profiles.

~~~text
export enum Compressionˉprofile: u8 {
    Gzip;
    Deflateˉzlib;
}

export opaque resource Compressor;
export opaque resource Decompressor;

export fn Compress(
    Budget: Memoryˉbudget,
    Profile: Compressionˉprofile,
    Input: Slice<u8>,
    Level: Compressionˉlevel,
    Maximumˉoutputˉbytes: u64,
) -> Result<Bytes, Compressionˉfailure> effects(memory.allocate);

export fn Decompress(
    Budget: Memoryˉbudget,
    Profile: Compressionˉprofile,
    Input: Slice<u8>,
    Maximumˉoutputˉbytes: u64,
    Maximumˉwork: u64,
) -> Result<Bytes, Compressionˉfailure> effects(memory.allocate);

export fn Encoderˉopen(
    Budget: Memoryˉbudget,
    Profile: Compressionˉprofile,
    Limits: Compressionˉlimits,
) -> Result<Compressor, Compressionˉfailure> effects(memory.allocate);

export fn Decoderˉopen(
    Budget: Memoryˉbudget,
    Profile: Compressionˉprofile,
    Limits: Compressionˉlimits,
) -> Result<Decompressor, Compressionˉfailure> effects(memory.allocate);

export fn Update(
    State: borrow mut Compressor,
    Input: Slice<u8>,
    Output: Mutableˉslice<u8>,
) -> Compressionˉprogress effects();

export fn Updateˉdecode(
    State: borrow mut Decompressor,
    Input: Slice<u8>,
    Output: Mutableˉslice<u8>,
) -> Compressionˉprogress effects();

export fn Finish(
    State: Compressor,
    Output: Mutableˉslice<u8>,
) -> Compressionˉfinish effects();

export fn Finishˉdecode(
    State: Decompressor,
    Output: Mutableˉslice<u8>,
) -> Compressionˉfinish effects();
~~~

Decoder output and work maxima are mandatory. HTTP does not invoke this module
unless the application selects an admitted content-encoding policy.

## Algorithm and digest modules

### `Algorithmsˉsequence`

~~~text
export record Algorithmˉlimits {
    Maximumˉitems: u64;
    Maximumˉcomparisons: u64;
    Maximumˉmoves: u64;
    Maximumˉtemporaryˉbytes: u64;
}

export fn Sort<T>(
    Values: Vector<T>,
    Order: Ordering<T>,
    Limits: Algorithmˉlimits,
) -> Result<Vector<T>, Algorithmˉfailure<T>> effects();

export fn Stableˉsort<T>(
    Budget: Memoryˉbudget,
    Values: Vector<T>,
    Order: Ordering<T>,
    Limits: Algorithmˉlimits,
) -> Result<Vector<T>, Algorithmˉfailure<T>> effects(memory.allocate);

export fn Isˉsorted<T>(
    Values: Sequence<T>,
    Order: Ordering<T>,
    Maximumˉcomparisons: u64,
) -> Result<bool, Algorithmˉfailure<T>> effects();

export fn Binaryˉsearch<T>(
    Values: Sequence<T>,
    Needle: borrow T,
    Order: Ordering<T>,
    Maximumˉcomparisons: u64,
) -> Result<Option<u64>, Algorithmˉfailure<T>> effects();

export fn Lowerˉbound<T>(
    Values: Sequence<T>,
    Needle: borrow T,
    Order: Ordering<T>,
    Maximumˉcomparisons: u64,
) -> Result<u64, Algorithmˉfailure<T>> effects();

export fn Upperˉbound<T>(
    Values: Sequence<T>,
    Needle: borrow T,
    Order: Ordering<T>,
    Maximumˉcomparisons: u64,
) -> Result<u64, Algorithmˉfailure<T>> effects();

export fn Select<T>(
    Values: Vector<T>,
    Rank: u64,
    Order: Ordering<T>,
    Limits: Algorithmˉlimits,
) -> Result<Selectˉresult<T>, Algorithmˉfailure<T>> effects();
~~~

The exact algorithm may change while the public stability, ordering, ownership,
and worst-case bounds remain fixed. A faster implementation must retain a
simple reference oracle.

### `Algorithmsˉgraph`

The graph module consumes an explicit bounded graph view; it does not grant
WVDB access or redefine WVDB relationship traversal.

~~~text
export fn Breadthˉfirst<Node>(
    Budget: Memoryˉbudget,
    Graph: borrow Graphˉview<Node>,
    Start: borrow Node,
    Limits: Graphˉlimits,
) -> Result<Graphˉvisit<Node>, Graphˉfailure<Node>> effects(memory.allocate);

export fn Depthˉfirst<Node>(
    Budget: Memoryˉbudget,
    Graph: borrow Graphˉview<Node>,
    Start: borrow Node,
    Limits: Graphˉlimits,
) -> Result<Graphˉvisit<Node>, Graphˉfailure<Node>> effects(memory.allocate);

export fn Topologicalˉsort<Node>(
    Budget: Memoryˉbudget,
    Graph: borrow Graphˉview<Node>,
    Limits: Graphˉlimits,
) -> Result<Sequence<Node>, Graphˉfailure<Node>> effects(memory.allocate);

export fn Connectedˉcomponents<Node>(
    Budget: Memoryˉbudget,
    Graph: borrow Graphˉview<Node>,
    Limits: Graphˉlimits,
) -> Result<Graphˉcomponents<Node>, Graphˉfailure<Node>> effects(memory.allocate);

export fn Shortestˉpath<Node, Weight>(
    Budget: Memoryˉbudget,
    Graph: borrow Weightedˉgraphˉview<Node, Weight>,
    Start: borrow Node,
    End: borrow Node,
    Limits: Graphˉlimits,
) -> Result<Option<Graphˉpath<Node, Weight>>, Graphˉfailure<Node>>
    effects(memory.allocate);
~~~

Negative weights, overflow, tie ordering, disconnected results, and work-limit
exhaustion are explicit. Individual algorithms may be excluded from the first
implemented Data profile without removing their reserved module ownership.

### `Securityˉdigest`

The existing portable SHA-256 implementation is the starting oracle. Public
names should become algorithm-neutral at the module boundary while selecting
an exact algorithm in the operation name or state type.

~~~text
export opaque Sha256ˉstate;
export record Sha256ˉdigest {
    Bytes: Array<u8, 32>;
}

export fn Sha256(
    Input: Slice<u8>,
) -> Sha256ˉdigest effects();

export fn Sha256ˉbegin() -> Sha256ˉstate effects();

export fn Sha256ˉupdate(
    State: borrow mut Sha256ˉstate,
    Input: Slice<u8>,
) -> Result<unit, Digestˉfailure> effects();

export fn Sha256ˉfinish(
    State: Sha256ˉstate,
) -> Sha256ˉdigest effects();

export fn Constantˉtimeˉequal(
    Left: Slice<u8>,
    Right: Slice<u8>,
) -> bool effects();
~~~

HMAC, HKDF, password hashing, signatures, and encryption belong to separately
qualified security profiles. A checksum such as CRC32C uses a different module
and must not be presented as cryptographic integrity.

### `Securityˉcose`

The first signed-object profile uses COSE Sign1 from [RFC
9052](https://www.rfc-editor.org/rfc/rfc9052.html) with EdDSA algorithm `-8`, OKP
key type, and Ed25519 curve `6` from [RFC
9053](https://www.rfc-editor.org/rfc/rfc9053.html). The algorithm identifier is a
protected header and policy fixes the curve; an input cannot negotiate either.
It supports bounded attached and detached payloads. COSE encryption, MAC,
multiple signers, and application-private header semantics are not implied.

~~~text
export record Coseˉlimits {
    Maximumˉmessageˉbytes: u64;
    Maximumˉprotectedˉheaderˉbytes: u64;
    Maximumˉunprotectedˉheaders: u32;
    Maximumˉattachedˉpayloadˉbytes: u64;
    Maximumˉdetachedˉpayloadˉbytes: u64;
    Maximumˉexternalˉauthenticatedˉdataˉbytes: u64;
    Maximumˉsignatureˉbytes: u64;
    Maximumˉwork: u64;
}

export enum Coseˉapplicationˉdomain: u8 {
    Artifactˉmanifestˉone;
    Cborˉvalidationˉreceiptˉone;
}

export opaque resource Coseˉsign1;
export opaque resource Coseˉsign1ˉprepared;
export opaque Coseˉprotectedˉheadersˉview Copy;
export opaque Coseˉsignatureˉview Copy;

export fn Sign1ˉparse(
    Budget: Memoryˉbudget,
    Input: Bytes,
    Limits: Coseˉlimits,
) -> Result<Coseˉsign1, Coseˉfailure> effects(memory.allocate);

export fn Protectedˉheaders(
    Message: borrow Coseˉsign1,
) -> Coseˉprotectedˉheadersˉview effects();

export fn Hasˉdetachedˉpayload(
    Message: borrow Coseˉsign1,
) -> bool effects();

export fn Attachedˉpayload(
    Message: borrow Coseˉsign1,
) -> Option<Slice<u8>> effects();

export fn Signature(
    Message: borrow Coseˉsign1,
) -> Coseˉsignatureˉview effects();

export fn Sign1ˉprepareˉattached(
    Budget: Memoryˉbudget,
    Payload: Bytes,
    Domain: Coseˉapplicationˉdomain,
    Limits: Coseˉlimits,
) -> Result<Coseˉsign1ˉprepared, Coseˉfailure> effects(memory.allocate);

export fn Toˉbeˉsigned(
    Prepared: borrow Coseˉsign1ˉprepared,
) -> Slice<u8> effects();

export fn Sign1ˉfinish(
    Budget: Memoryˉbudget,
    Prepared: Coseˉsign1ˉprepared,
    Signature: Signature,
    Maximumˉoutputˉbytes: u64,
) -> Result<Bytes, Coseˉfailure> effects(memory.allocate);
~~~

Parsing a COSE structure produces no trust decision. Verification authenticates
the exact protected headers, external authenticated data, and serialized
payload bytes; it does not canonicalize or reinterpret the payload. The COSE
message and signature-input maxima are deliberately small because the selected
pure EdDSA profile processes the complete signature input. Large artifacts are
not placed directly in that input: a deterministic signed manifest or validation
receipt carries their SHA-256 digest and exact length, while the content is
hashed and admitted through one bounded stream.

Every application profile supplies nonempty external authenticated data that
contains its exact domain identity. Artifact-manifest, CBOR-validation-receipt,
and another signed-object purpose do not share a signature domain or key usage
implicitly.

`Sign1ˉprepareˉattached` fixes the protected algorithm/key profile and produces
the complete small signature input consumed by `Platformˉkeyˉstore.Sign`.
`Sign1ˉfinish` accepts only the exact Ed25519 signature shape for that prepared
value. Unprotected headers never select an algorithm, curve, domain, key usage,
content identity, or authorization decision.

Artifact-manifest signing and CBOR-validation-receipt signing are distinct
`Keyˉusage` values. Neither is satisfied by a TLS key, a general signing key, or
an existing release root/delegated key unless a separate policy deliberately
binds that exact identity and usage.

## Filesystem modules

### `Filesystemˉpath`

These are semantic relative names, not native paths.

~~~text
export opaque Pathˉsegment;
export opaque Relativeˉpath;

export fn Segmentˉparse(
    Input: borrow text,
    Limits: Pathˉlimits,
) -> Result<Pathˉsegment, Pathˉfailure> effects();

export fn Relativeˉparse(
    Budget: Memoryˉbudget,
    Input: borrow text,
    Limits: Pathˉlimits,
) -> Result<Relativeˉpath, Pathˉfailure> effects(memory.allocate);

export fn Join(
    Budget: Memoryˉbudget,
    Parent: borrow Relativeˉpath,
    Child: borrow Pathˉsegment,
    Limits: Pathˉlimits,
) -> Result<Relativeˉpath, Pathˉfailure> effects(memory.allocate);

export fn Name(
    Value: borrow Relativeˉpath,
) -> Pathˉsegment effects();

export fn Parent(
    Value: borrow Relativeˉpath,
) -> Option<Relativeˉpath> effects();

export fn Display(
    Budget: Memoryˉbudget,
    Value: borrow Relativeˉpath,
    Maximumˉbytes: u64,
) -> Result<text, Pathˉfailure> effects(memory.allocate);
~~~

The first shared profile uses strict segments and `/` only as canonical display
syntax. Host adapters privately map an admitted bound root and semantic path to
native operations without returning a native path to portable source.

### `Platformˉfile`

Candidate capability families are `filesystem.file.read_v1`,
`filesystem.file.create_v1`, and `filesystem.file.mutate_v1`. An application
receives only the endpoint kinds it requests and the launcher approves.

~~~text
export opaque Fileˉreadˉendpoint Copy;
export opaque Fileˉcreateˉendpoint Copy;
export opaque Fileˉmutateˉendpoint Copy;
export opaque resource Sourceˉfile;
export opaque resource Destinationˉfile;
export opaque resource Mutableˉfile;

export async fn Openˉsnapshot(
    Endpoint: Fileˉreadˉendpoint,
    Name: borrow Relativeˉpath,
    Limits: Fileˉopenˉlimits,
    Context: borrow Operationˉcontext,
) -> Result<Sourceˉfile, Fileˉfailure>
    effects(filesystem.file.read, resource.acquire, task.suspend);

export async fn Createˉexclusive(
    Endpoint: Fileˉcreateˉendpoint,
    Name: borrow Relativeˉpath,
    Limits: Fileˉopenˉlimits,
    Context: borrow Operationˉcontext,
) -> Result<Destinationˉfile, Fileˉfailure>
    effects(filesystem.file.create, resource.acquire, task.suspend);

export async fn Openˉupdate(
    Endpoint: Fileˉmutateˉendpoint,
    Name: borrow Relativeˉpath,
    Limits: Fileˉopenˉlimits,
    Context: borrow Operationˉcontext,
) -> Result<Mutableˉfile, Fileˉfailure>
    effects(filesystem.file.mutate, resource.acquire, task.suspend);

export fn Describe(
    File: borrow Sourceˉfile,
) -> Fileˉdescription effects();

export async fn Readˉat(
    File: borrow mut Sourceˉfile,
    Position: u64,
    Target: Mutableˉslice<u8>,
    Context: borrow Operationˉcontext,
) -> Readˉoutcome<Fileˉfailure>
    effects(filesystem.file.read, task.suspend);

export async fn Writeˉat(
    File: borrow mut Mutableˉfile,
    Position: u64,
    Value: Slice<u8>,
    Context: borrow Operationˉcontext,
) -> Mutationˉoutcome<Fileˉfailure>
    effects(filesystem.file.mutate, task.suspend);

export async fn Writeˉnewˉat(
    File: borrow mut Destinationˉfile,
    Position: u64,
    Value: Slice<u8>,
    Context: borrow Operationˉcontext,
) -> Mutationˉoutcome<Fileˉfailure>
    effects(filesystem.file.create, task.suspend);

export async fn Setˉlength(
    File: borrow mut Mutableˉfile,
    Length: u64,
    Context: borrow Operationˉcontext,
) -> Lengthˉoutcome<Fileˉfailure>
    effects(filesystem.file.mutate, task.suspend);

export async fn Flush(
    File: borrow mut Mutableˉfile,
    Class: Flushˉclass,
    Context: borrow Operationˉcontext,
) -> Flushˉoutcome<Fileˉfailure>
    effects(filesystem.file.mutate, resource.complete, task.suspend);

export async fn Finishˉdurable(
    File: borrow mut Destinationˉfile,
    Expectedˉlength: u64,
    Context: borrow Operationˉcontext,
) -> Finishˉoutcome<Fileˉfailure>
    effects(filesystem.file.create, resource.complete, task.suspend);
~~~

`Finishˉdurable` includes content, length, and the newly created directory name
only when the bound provider advertises that exact durability profile. Local
release does not call it implicitly.

### `Platformˉdirectory`

~~~text
export opaque Directoryˉreadˉendpoint Copy;
export opaque Directoryˉmanageˉendpoint Copy;
export opaque resource Directoryˉcursor;

export async fn Enumerate(
    Endpoint: Directoryˉreadˉendpoint,
    Directory: borrow Relativeˉpath,
    Limits: Directoryˉlimits,
    Context: borrow Operationˉcontext,
) -> Result<Directoryˉcursor, Directoryˉfailure>
    effects(filesystem.directory.enumerate, resource.acquire, task.suspend);

export async fn Readˉnext(
    Cursor: borrow mut Directoryˉcursor,
    Maximumˉentries: u32,
    Context: borrow Operationˉcontext,
) -> Result<Directoryˉbatch, Directoryˉfailure>
    effects(filesystem.directory.enumerate, task.suspend);

export async fn Create(
    Endpoint: Directoryˉmanageˉendpoint,
    Directory: borrow Relativeˉpath,
    Context: borrow Operationˉcontext,
) -> Result<unit, Directoryˉfailure>
    effects(filesystem.directory.manage, task.suspend);

export async fn Removeˉempty(
    Endpoint: Directoryˉmanageˉendpoint,
    Directory: borrow Relativeˉpath,
    Context: borrow Operationˉcontext,
) -> Mutationˉoutcome<Directoryˉfailure>
    effects(filesystem.directory.manage, task.suspend);

export async fn Removeˉfile(
    Endpoint: Directoryˉmanageˉendpoint,
    Name: borrow Relativeˉpath,
    Expected: Fileˉidentity,
    Context: borrow Operationˉcontext,
) -> Mutationˉoutcome<Directoryˉfailure>
    effects(filesystem.directory.manage, task.suspend);

export async fn Moveˉatomic(
    Endpoint: Directoryˉmanageˉendpoint,
    Source: borrow Relativeˉpath,
    Expectedˉsource: Fileˉidentity,
    Destination: borrow Relativeˉpath,
    Replace: Replaceˉpolicy,
    Context: borrow Operationˉcontext,
) -> Mutationˉoutcome<Directoryˉfailure>
    effects(filesystem.directory.manage, task.suspend);
~~~

Recursive deletion, link traversal, permission mutation, watching, native
metadata, and cross-provider moves are separate profiles. `Removeˉfile` and
`Moveˉatomic` require expected identity/generation evidence to avoid silently
retargeting a replaced name.

### `Platformˉpublication`

This focused module provides recoverable atomic file publication without
pretending that arbitrary multi-resource filesystem transactions exist.

~~~text
export opaque Publicationˉendpoint Copy;
export opaque resource Publication;

export async fn Begin(
    Endpoint: Publicationˉendpoint,
    Destination: borrow Relativeˉpath,
    Limits: Publicationˉlimits,
    Context: borrow Operationˉcontext,
) -> Result<Publication, Publicationˉfailure>
    effects(filesystem.publication, resource.acquire, task.suspend);

export async fn Writeˉat(
    Publication: borrow mut Publication,
    Position: u64,
    Value: Slice<u8>,
    Context: borrow Operationˉcontext,
) -> Mutationˉoutcome<Publicationˉfailure>
    effects(filesystem.publication, task.suspend);

export async fn Publishˉnew(
    Publication: borrow mut Publication,
    Expectedˉlength: u64,
    Context: borrow Operationˉcontext,
) -> Publicationˉoutcome
    effects(filesystem.publication, resource.complete, task.suspend);

export async fn Replaceˉatomic(
    Publication: borrow mut Publication,
    Expectedˉdestination: Fileˉidentity,
    Expectedˉlength: u64,
    Context: borrow Operationˉcontext,
) -> Publicationˉoutcome
    effects(filesystem.publication, resource.complete, task.suspend);

export async fn Abort(
    Publication: borrow mut Publication,
    Context: borrow Operationˉcontext,
) -> Mutationˉoutcome<Publicationˉfailure>
    effects(filesystem.publication, task.suspend);
~~~

An indeterminate publish or abort remains observable and is not replayed.

## Storage modules

### `Platformˉrandomˉaccessˉstorage`

The current `storage.random_access_v1` contract supplies the initial exact
operation family. The public 1.0 facade retains these semantics:

~~~text
export fn Describe(
    Storage: Storageˉendpoint,
) -> Storageˉresult effects(storage.random_access);

export fn Readˉat(
    Storage: Storageˉendpoint,
    Generation: u64,
    Position: u64,
    Maximum: u32,
) -> Storageˉresult effects(storage.random_access);

export fn Writeˉat(
    Storage: Storageˉendpoint,
    Generation: u64,
    Position: u64,
    Value: Slice<u8>,
) -> Storageˉresult effects(storage.random_access);

export fn Resize(
    Storage: Storageˉendpoint,
    Generation: u64,
    Length: u64,
) -> Storageˉresult effects(storage.random_access);

export fn Flush(
    Storage: Storageˉendpoint,
    Generation: u64,
    Class: Storageˉflush,
) -> Storageˉresult effects(storage.random_access, resource.complete);
~~~

The migration may add the explicit endpoint parameter while retaining the
implemented generation, progress, borrowed-response, and flush semantics.

### `Storageˉblob`

Blob storage is a semantic object API over an explicitly bound namespace. It
may be implemented by files, WVDB large-object storage, cloud storage, or a
Windvale OS service without exposing those mechanisms.

~~~text
export opaque Blobˉendpoint Copy;
export opaque Blobˉidentity;
export opaque resource Blobˉreader;
export opaque resource Blobˉwriter;
export opaque resource Blobˉcursor;

export async fn Describe(
    Endpoint: Blobˉendpoint,
    Identity: borrow Blobˉidentity,
    Context: borrow Operationˉcontext,
) -> Result<Blobˉdescription, Blobˉfailure>
    effects(storage.blob.read, task.suspend);

export async fn Openˉread(
    Endpoint: Blobˉendpoint,
    Identity: borrow Blobˉidentity,
    Limits: Blobˉreadˉlimits,
    Context: borrow Operationˉcontext,
) -> Result<Blobˉreader, Blobˉfailure>
    effects(storage.blob.read, resource.acquire, task.suspend);

export async fn Read(
    Reader: borrow mut Blobˉreader,
    Target: Mutableˉslice<u8>,
    Context: borrow Operationˉcontext,
) -> Readˉoutcome<Blobˉfailure>
    effects(storage.blob.read, task.suspend);

export async fn Beginˉwrite(
    Endpoint: Blobˉendpoint,
    Proposed: Blobˉproposal,
    Limits: Blobˉwriteˉlimits,
    Context: borrow Operationˉcontext,
) -> Result<Blobˉwriter, Blobˉfailure>
    effects(storage.blob.write, resource.acquire, task.suspend);

export async fn Write(
    Writer: borrow mut Blobˉwriter,
    Value: Slice<u8>,
    Context: borrow Operationˉcontext,
) -> Mutationˉoutcome<Blobˉfailure>
    effects(storage.blob.write, task.suspend);

export async fn Commit(
    Writer: borrow mut Blobˉwriter,
    Expectedˉlength: u64,
    Expectedˉdigest: Option<Sha256ˉdigest>,
    Context: borrow Operationˉcontext,
) -> Blobˉcommitˉoutcome
    effects(storage.blob.write, resource.complete, task.suspend);

export async fn Abort(
    Writer: borrow mut Blobˉwriter,
    Context: borrow Operationˉcontext,
) -> Mutationˉoutcome<Blobˉfailure>
    effects(storage.blob.write, task.suspend);

export async fn Enumerate(
    Endpoint: Blobˉendpoint,
    Prefix: Option<Blobˉprefix>,
    Limits: Blobˉenumerationˉlimits,
    Context: borrow Operationˉcontext,
) -> Result<Blobˉcursor, Blobˉfailure>
    effects(storage.blob.enumerate, resource.acquire, task.suspend);

export async fn Readˉnext(
    Cursor: borrow mut Blobˉcursor,
    Context: borrow Operationˉcontext,
) -> Result<Blobˉbatch, Blobˉfailure>
    effects(storage.blob.enumerate, task.suspend);

export async fn Delete(
    Endpoint: Blobˉendpoint,
    Identity: borrow Blobˉidentity,
    Expectedˉgeneration: u64,
    Context: borrow Operationˉcontext,
) -> Mutationˉoutcome<Blobˉfailure>
    effects(storage.blob.delete, task.suspend);
~~~

## Network modules

### `Networkˉaddress`

The existing address/authority module is the implementation starting point.
The proposed facade exposes:

~~~text
export fn Addressˉfromˉbytes(
    Kind: Addressˉkind,
    Value: Array<u8, 16>,
) -> Result<Address, Addressˉfailure> effects();

export fn Addressˉparse(
    Input: borrow text,
) -> Result<Address, Addressˉfailure> effects();

export fn Addressˉdisplay(
    Address: Address,
) -> text effects();

export fn Addressˉscope(Address: Address) -> Addressˉscope effects();

export fn Prefixˉcreate(
    Address: Address,
    Length: u32,
) -> Result<Prefix, Addressˉfailure> effects();

export fn Prefixˉcontains(
    Prefix: Prefix,
    Address: Address,
) -> bool effects();

export fn Endpointˉcreate(
    Address: Address,
    Port: u16,
    Scope: Option<Interfaceˉscope>,
) -> Result<Endpoint, Addressˉfailure> effects();

export fn Serviceˉnameˉcreate(
    Input: borrow text,
) -> Result<Serviceˉname, Addressˉfailure> effects();

export fn Grantˉnarrows(
    Child: borrow Networkˉgrant,
    Parent: borrow Networkˉgrant,
) -> bool effects();
~~~

### `Platformˉnetworkˉstream`

Candidate capability families are `network.resolve_connect_v1`,
`network.stream_v1`, and `network.service.accept_v1`.

~~~text
export opaque Connectˉendpoint Copy;
export opaque Serviceˉendpoint Copy;
export opaque resource Reliableˉstream;

export async fn Resolve(
    Endpoint: Connectˉendpoint,
    Service: borrow Serviceˉname,
    Context: borrow Operationˉcontext,
) -> Result<Resolvedˉservice, Networkˉfailure>
    effects(network.resolve_connect, task.suspend);

export async fn Connect(
    Endpoint: Connectˉendpoint,
    Target: borrow Connectˉtarget,
    Limits: Streamˉlimits,
    Context: borrow Operationˉcontext,
) -> Result<Reliableˉstream, Networkˉfailure>
    effects(network.resolve_connect, resource.acquire, task.suspend);

export async fn Acceptˉone(
    Endpoint: Serviceˉendpoint,
    Limits: Streamˉlimits,
    Context: borrow Operationˉcontext,
) -> Result<Reliableˉstream, Networkˉfailure>
    effects(network.service.accept, resource.acquire, task.suspend);

export async fn Read(
    Stream: borrow mut Reliableˉstream,
    Target: Mutableˉslice<u8>,
    Context: borrow Operationˉcontext,
) -> Streamˉreadˉoutcome
    effects(network.stream, task.suspend);

export async fn Write(
    Stream: borrow mut Reliableˉstream,
    Value: Slice<u8>,
    Context: borrow Operationˉcontext,
) -> Streamˉwriteˉoutcome
    effects(network.stream, task.suspend);

export async fn Shutdownˉwrite(
    Stream: borrow mut Reliableˉstream,
    Context: borrow Operationˉcontext,
) -> Streamˉshutdownˉoutcome
    effects(network.stream, task.suspend);

export async fn Refresh(
    Endpoint: Serviceˉendpoint,
    Observedˉgeneration: u64,
    Context: borrow Operationˉcontext,
) -> Result<Serviceˉendpoint, Networkˉfailure>
    effects(network.service.accept, task.suspend);
~~~

`Write.Completed` reports exact local-provider acceptance, not peer receipt or
application commit. `Refresh` can return only the same approved service and is
never discovery or replay.

### `Platformˉnetworkˉdatagram`

~~~text
export opaque Datagramˉendpoint Copy;
export opaque resource Datagramˉport;

export async fn Bind(
    Endpoint: Datagramˉendpoint,
    Limits: Datagramˉlimits,
    Context: borrow Operationˉcontext,
) -> Result<Datagramˉport, Networkˉfailure>
    effects(network.datagram, resource.acquire, task.suspend);

export async fn Sendˉto(
    Port: borrow mut Datagramˉport,
    Peer: Endpoint,
    Value: Slice<u8>,
    Context: borrow Operationˉcontext,
) -> Datagramˉsendˉoutcome
    effects(network.datagram, task.suspend);

export async fn Receiveˉfrom(
    Port: borrow mut Datagramˉport,
    Target: Mutableˉslice<u8>,
    Context: borrow Operationˉcontext,
) -> Datagramˉreceiveˉoutcome
    effects(network.datagram, task.suspend);
~~~

Local acceptance never implies datagram delivery. Multicast, broadcast, packet
metadata, and interface administration are separate optional profiles.

## Time, entropy, certificate, trust, and key modules

### `Platformˉclock`

~~~text
export opaque Monotonicˉclock Copy;
export opaque Civilˉclock Copy;

export fn Monotonicˉnow(
    Clock: Monotonicˉclock,
) -> Result<Monotonicˉinstant, Clockˉfailure>
    effects(clock.monotonic);

export async fn Waitˉuntil(
    Clock: Monotonicˉclock,
    Deadline: Monotonicˉinstant,
    Context: borrow Operationˉcontext,
) -> Waitˉoutcome effects(timer.wait, task.suspend);

export fn Civilˉnow(
    Clock: Civilˉclock,
) -> Result<Civilˉtimeˉevidence, Clockˉfailure>
    effects(clock.civil);
~~~

Monotonic instants cannot be serialized as global timestamps or compared across
provider generations. Certificate validation selects an explicit civil-time or
pinned-key policy.

### `Platformˉentropy`

~~~text
export opaque Secureˉentropy Copy;
export opaque Deterministicˉtestˉentropy Copy;

export fn Fillˉsecure(
    Provider: Secureˉentropy,
    Target: Mutableˉslice<u8>,
) -> Result<unit, Entropyˉfailure> effects(entropy.secure);

export fn Id128ˉgenerate(
    Provider: Secureˉentropy,
) -> Result<Id128, Entropyˉfailure> effects(entropy.secure);

export fn Fillˉdeterministic(
    Provider: Deterministicˉtestˉentropy,
    Target: Mutableˉslice<u8>,
) -> Result<unit, Entropyˉfailure> effects(entropy.deterministic_test);
~~~

The deterministic provider cannot satisfy `entropy.secure` through aliasing,
configuration, or provider substitution.

### `Securityˉcertificate`

This portable module parses public certificate evidence. It owns no trust
decision and never accepts a private key.

~~~text
export opaque resource Certificate;
export opaque resource Certificateˉchain;

export fn Parseˉder(
    Budget: Memoryˉbudget,
    Input: Slice<u8>,
    Limits: Certificateˉlimits,
) -> Result<Certificate, Certificateˉfailure> effects(memory.allocate);

export fn Parseˉpem(
    Budget: Memoryˉbudget,
    Input: Slice<u8>,
    Limits: Certificateˉlimits,
) -> Result<Certificateˉchain, Certificateˉfailure> effects(memory.allocate);

export fn Subject(
    Certificate: borrow Certificate,
) -> Certificateˉname effects();

export fn Issuer(
    Certificate: borrow Certificate,
) -> Certificateˉname effects();

export fn Validity(
    Certificate: borrow Certificate,
) -> Certificateˉvalidity effects();

export fn Publicˉkeyˉdigest(
    Certificate: borrow Certificate,
) -> Sha256ˉdigest effects();

export fn Serviceˉidentities(
    Certificate: borrow Certificate,
) -> Sequence<Serviceˉidentity> effects();

export fn Encodeˉder(
    Budget: Memoryˉbudget,
    Certificate: borrow Certificate,
    Maximumˉbytes: u64,
) -> Result<Bytes, Certificateˉfailure> effects(memory.allocate);
~~~

The initial admitted certificate profile, encoding rules, algorithms, maximum
chain depth, maximum names, and extension policy remain review decisions. An
unsupported critical extension rejects.

### `Platformˉtrust`

~~~text
export opaque Trustˉendpoint Copy;
export opaque resource Trustˉsnapshot;

export async fn Openˉsnapshot(
    Endpoint: Trustˉendpoint,
    Expectedˉgeneration: Option<u64>,
    Context: borrow Operationˉcontext,
) -> Result<Trustˉsnapshot, Trustˉfailure>
    effects(security.trust, resource.acquire, task.suspend);

export fn Snapshotˉdigest(
    Snapshot: borrow Trustˉsnapshot,
) -> Sha256ˉdigest effects();

export fn Verifyˉserver(
    Snapshot: borrow Trustˉsnapshot,
    Chain: borrow Certificateˉchain,
    Service: borrow Serviceˉname,
    Policy: Serverˉtrustˉpolicy,
    Time: Option<Civilˉtimeˉevidence>,
) -> Result<Peerˉevidence, Trustˉfailure> effects();

export fn Verifyˉclient(
    Snapshot: borrow Trustˉsnapshot,
    Chain: borrow Certificateˉchain,
    Policy: Clientˉtrustˉpolicy,
    Time: Option<Civilˉtimeˉevidence>,
) -> Result<Peerˉevidence, Trustˉfailure> effects();

export fn Verifyˉcoseˉsign1(
    Snapshot: borrow Trustˉsnapshot,
    Message: borrow Coseˉsign1,
    Detachedˉpayload: Option<Slice<u8>>,
    Domain: Coseˉapplicationˉdomain,
    Policy: Artifactˉsignatureˉpolicy,
    Time: Option<Civilˉtimeˉevidence>,
) -> Result<Artifactˉauthenticationˉevidence, Trustˉfailure> effects();

export fn Checkˉusage(
    Evidence: borrow Peerˉevidence,
    Usage: Identityˉusage,
) -> Result<unit, Trustˉfailure> effects();
~~~

Trust snapshots are immutable and content-addressed. An update creates a new
generation. Verification evidence records the exact generation, digest, policy,
peer identity, and time basis.

### `Platformˉartifactˉtrust`

This optional composition owns the safe fast path for signed or package-
authenticated immutable artifacts. Its endpoint binds an approved artifact
namespace, immutable snapshot provider, trust generation, publisher identities,
validator identities, COSE/signature profiles, cache limits, and the uses each
signer may authorize.

The candidate capability family is `artifact.trust.open_v1`. Requiring the
module does not grant that capability or any underlying file/blob namespace;
the launcher binds one rights-limited composite endpoint.

~~~text
export record Artifactˉmanifestˉone {
    Namespace: Artifactˉnamespace;
    Identity: Artifactˉidentity;
    Permittedˉuse: Artifactˉuse;
    Mediaˉprofile: Artifactˉmediaˉprofile;
    Contentˉlength: u64;
    Contentˉsha256: Sha256ˉdigest;
    Validationˉreceiptˉsha256: Option<Sha256ˉdigest>;
}

export opaque Artifactˉtrustˉendpoint Copy;
export opaque resource Authenticatedˉartifact;
export opaque Artifactˉauthenticationˉevidence Copy;

export variant Cborˉartifactˉpreparation {
    Prevalidated(Source: Cborˉprevalidatedˉsource);
    Requiresˉadmission(Content: Bytes);
}

export fn Manifestˉencode(
    Budget: Memoryˉbudget,
    Manifest: borrow Artifactˉmanifestˉone,
    Maximumˉbytes: u64,
) -> Result<Bytes, Artifactˉfailure> effects(memory.allocate);

export fn Manifestˉadmit(
    Budget: Memoryˉbudget,
    Input: Bytes,
    Limits: Artifactˉmanifestˉlimits,
) -> Result<Artifactˉmanifestˉone, Artifactˉfailure>
    effects(memory.allocate);

export async fn Openˉauthenticated(
    Budget: Memoryˉbudget,
    Endpoint: Artifactˉtrustˉendpoint,
    Identity: borrow Artifactˉidentity,
    Limits: Artifactˉlimits,
    Context: borrow Operationˉcontext,
) -> Result<Authenticatedˉartifact, Artifactˉfailure>
    effects(artifact.trust.open, memory.allocate, resource.acquire, task.suspend);

export fn Content(
    Artifact: borrow Authenticatedˉartifact,
) -> Slice<u8> effects();

export fn Takeˉcontent(
    Artifact: Authenticatedˉartifact,
) -> Bytes effects();

export fn Authentication(
    Artifact: borrow Authenticatedˉartifact,
) -> Artifactˉauthenticationˉevidence effects();

export fn Prepareˉcbor(
    Artifact: Authenticatedˉartifact,
    Requiredˉprofile: Cborˉprofile,
    Requiredˉschema: Option<Cddlˉschemaˉidentity>,
    Consumerˉlimits: Cborˉlimits,
) -> Result<Cborˉartifactˉpreparation, Artifactˉfailure> effects();

export fn Openˉcbor(
    Budget: Memoryˉbudget,
    Source: Cborˉprevalidatedˉsource,
    Limits: Cborˉlimits,
) -> Result<Cborˉdocument, Artifactˉfailure> effects(memory.allocate);
~~~

The manifest has one deterministic CBOR encoding and a focused CDDL schema.
`Manifestˉadmit` and the surrounding COSE envelope always use the ordinary
bounded path; neither can bootstrap its own prevalidated evidence. The optional
receipt digest covers the deterministic receipt payload, not its COSE envelope,
and associates a separately signed validation receipt without making the
publisher its validator.

`Openˉauthenticated` ordinarily authenticates the small manifest before it
acquires the named content. It rejects a declared or provider-reported length
above the hard or caller maximum before content allocation, reads an immutable
snapshot into the supplied memory budget while checking SHA-256 and exact
length, and returns only the verified bytes. An unknown-length provider is read
through the same hard maximum and cannot use signed metadata to authorize an
oversized allocation.

`Prepareˉcbor` consumes the authenticated artifact. When every prevalidation
condition matches, it returns one opaque source binding that exact immutable
content, receipt evidence, and optional index. Otherwise it returns the same
owned content through `Requiresˉadmission`, and the application calls ordinary
`Cbor.Admit` plus schema validation. The borrowed `Content` view cannot outlive
the artifact and does not create a second mutable alias.

There is no source-constructible `Trusted` Boolean. Authentication proves exact
bytes and an authorized publisher; prevalidation additionally proves the CBOR
contract, schema, observed resource use, validator contract, and optional index.
The opaque source is issued only after the immutable content identity and an
authorized validator attestation match. It has no public constructor from
ordinary bytes or separate evidence, so a caller cannot associate a valid
receipt with a different buffer. A path, modification time, mutable file
handle, TLS peer, or publisher signature alone cannot create it.

The first uncached open normally costs more than ordinary CBOR admission because
it includes content hashing and signature verification. Hashing and admission
may share one bounded sequential read. A later open is faster only when the
provider can prove the same immutable content identity and reuse valid receipt
and index evidence. Cache keys include the content digest, CBOR profile, schema
identity, validator contract, trust generation, and index identity; cache size
and lifetime remain bounded.

### `Platformˉkeyˉstore`

~~~text
export opaque Keyˉstoreˉendpoint Copy;
export opaque resource Signer;

export async fn Openˉsigner(
    Endpoint: Keyˉstoreˉendpoint,
    Identity: borrow Keyˉidentity,
    Usage: Keyˉusage,
    Context: borrow Operationˉcontext,
) -> Result<Signer, Keyˉfailure>
    effects(security.key.sign, resource.acquire, task.suspend);

export async fn Sign(
    Signer: borrow mut Signer,
    Profile: Signatureˉprofile,
    Message: Slice<u8>,
    Context: borrow Operationˉcontext,
) -> Result<Signature, Keyˉfailure>
    effects(security.key.sign, task.suspend);

export async fn Createˉcertificateˉrequest(
    Signer: borrow mut Signer,
    Request: borrow Certificateˉrequestˉprofile,
    Context: borrow Operationˉcontext,
) -> Result<Bytes, Keyˉfailure>
    effects(security.key.sign, task.suspend);

export async fn Rotate(
    Endpoint: Keyˉstoreˉendpoint,
    Identity: borrow Keyˉidentity,
    Policy: Keyˉrotationˉpolicy,
    Context: borrow Operationˉcontext,
) -> Keyˉmutationˉoutcome
    effects(security.key.admin, task.suspend);

export async fn Revoke(
    Endpoint: Keyˉstoreˉendpoint,
    Identity: borrow Keyˉidentity,
    Expectedˉgeneration: u64,
    Context: borrow Operationˉcontext,
) -> Keyˉmutationˉoutcome
    effects(security.key.admin, task.suspend);

export async fn Destroy(
    Endpoint: Keyˉstoreˉendpoint,
    Identity: borrow Keyˉidentity,
    Expectedˉgeneration: u64,
    Context: borrow Operationˉcontext,
) -> Keyˉmutationˉoutcome
    effects(security.key.admin, task.suspend);
~~~

There is intentionally no `Exportˉprivateˉkey` operation in the standard
profile. Rotation, revocation, and destruction require separate administrative
authority from signing.

### `Networkˉtls`

~~~text
export opaque Tlsˉclientˉendpoint Copy;
export opaque Tlsˉserviceˉendpoint Copy;
export opaque resource Secureˉstream;

export async fn Connect(
    Endpoint: Tlsˉclientˉendpoint,
    Service: borrow Serviceˉname,
    Protocols: Sequence<Applicationˉprotocol>,
    Limits: Tlsˉlimits,
    Context: borrow Operationˉcontext,
) -> Result<Tlsˉconnection, Tlsˉfailure>
    effects(network.tls.connect, resource.acquire, task.suspend);

export async fn Accept(
    Endpoint: Tlsˉserviceˉendpoint,
    Protocols: Sequence<Applicationˉprotocol>,
    Limits: Tlsˉlimits,
    Context: borrow Operationˉcontext,
) -> Result<Tlsˉconnection, Tlsˉfailure>
    effects(network.tls.accept, resource.acquire, task.suspend);

export fn Peerˉevidence(
    Connection: borrow Tlsˉconnection,
) -> Peerˉevidence effects();

export fn Protocol(
    Connection: borrow Tlsˉconnection,
) -> Applicationˉprotocol effects();

export fn Stream(
    Connection: Tlsˉconnection,
) -> Secureˉstream effects();

export async fn Read(
    Stream: borrow mut Secureˉstream,
    Target: Mutableˉslice<u8>,
    Context: borrow Operationˉcontext,
) -> Streamˉreadˉoutcome effects(network.tls.stream, task.suspend);

export async fn Write(
    Stream: borrow mut Secureˉstream,
    Value: Slice<u8>,
    Context: borrow Operationˉcontext,
) -> Streamˉwriteˉoutcome effects(network.tls.stream, task.suspend);

export async fn Shutdown(
    Stream: borrow mut Secureˉstream,
    Context: borrow Operationˉcontext,
) -> Tlsˉshutdownˉoutcome effects(network.tls.stream, task.suspend);
~~~

TLS 1.3 is the initial secure-transport profile. Early application data is
disabled. The application never receives session secrets, private keys, native
TLS handles, or trust-store internals.

## HTTP modules

### `Networkˉhttpˉcore`

The portable HTTP core owns values and framing. It owns no connection, listener,
TLS, DNS, cookie store, redirect policy, decompressor, cache, or credential.

~~~text
export enum Httpˉmethod: u8 {
    Get;
    Head;
    Post;
    Put;
    Patch;
    Delete;
    Options;
}

export record Httpˉlimits {
    Maximumˉstartˉlineˉbytes: u64;
    Maximumˉheaderˉbytes: u64;
    Maximumˉheaders: u32;
    Maximumˉbodyˉbytes: u64;
    Maximumˉwireˉbytes: u64;
    Maximumˉtrailerˉbytes: u64;
}

export opaque Httpˉheaderˉname;
export opaque Httpˉheaders;
export opaque Httpˉrequestˉhead;
export opaque Httpˉresponseˉhead;
export opaque resource Httpˉdecoder;

export fn Headerˉnameˉparse(
    Input: Slice<u8>,
) -> Result<Httpˉheaderˉname, Httpˉfailure> effects();

export fn Headersˉconstruct(
    Budget: Memoryˉbudget,
    Maximumˉheaders: u32,
    Maximumˉbytes: u64,
) -> Result<Httpˉheaders, Httpˉfailure> effects(memory.allocate);

export fn Headersˉadd(
    Headers: borrow mut Httpˉheaders,
    Name: Httpˉheaderˉname,
    Value: Slice<u8>,
) -> Result<unit, Httpˉfailure> effects();

export fn Headersˉget(
    Headers: borrow Httpˉheaders,
    Name: Httpˉheaderˉname,
) -> Option<Slice<u8>> effects();

export fn Requestˉdecoderˉopen(
    Budget: Memoryˉbudget,
    Limits: Httpˉlimits,
) -> Result<Httpˉdecoder, Httpˉfailure> effects(memory.allocate);

export fn Responseˉdecoderˉopen(
    Budget: Memoryˉbudget,
    Limits: Httpˉlimits,
) -> Result<Httpˉdecoder, Httpˉfailure> effects(memory.allocate);

export fn Decodeˉupdate(
    Decoder: borrow mut Httpˉdecoder,
    Input: Slice<u8>,
) -> Httpˉdecodeˉprogress effects();

export fn Decodeˉfinish(
    Decoder: Httpˉdecoder,
    Peerˉclosed: bool,
) -> Result<Httpˉmessage, Httpˉfailure> effects();

export fn Encodeˉrequestˉhead(
    Budget: Memoryˉbudget,
    Head: borrow Httpˉrequestˉhead,
    Maximumˉbytes: u64,
) -> Result<Bytes, Httpˉfailure> effects(memory.allocate);

export fn Encodeˉresponseˉhead(
    Budget: Memoryˉbudget,
    Head: borrow Httpˉresponseˉhead,
    Maximumˉbytes: u64,
) -> Result<Bytes, Httpˉfailure> effects(memory.allocate);
~~~

The first complete service profile is strict HTTP/1.1. HTTP/2 and HTTP/3 use
separate codecs behind compatible semantic request/response values only after
their flow-control, multiplexing, compression, cancellation, and teardown
contracts are specified.

### `Networkˉhttpˉbody`

~~~text
export opaque resource Requestˉbody;
export opaque resource Responseˉbody;
export opaque resource Httpˉrequest;
export opaque resource Httpˉresponse;

export async fn Readˉchunk(
    Body: borrow mut Requestˉbody,
    Target: Mutableˉslice<u8>,
    Context: borrow Operationˉcontext,
) -> Httpˉbodyˉreadˉoutcome effects(network.http.service, task.suspend);

export async fn Readˉallˉbounded(
    Budget: Memoryˉbudget,
    Body: borrow mut Requestˉbody,
    Maximumˉbytes: u64,
    Context: borrow Operationˉcontext,
) -> Result<Bytes, Httpˉfailure>
    effects(network.http.service, memory.allocate, task.suspend);

export async fn Readˉjson(
    Budget: Memoryˉbudget,
    Body: borrow mut Requestˉbody,
    Limits: Jsonˉlimits,
    Context: borrow Operationˉcontext,
) -> Result<Jsonˉdocument, Httpˉjsonˉfailure>
    effects(network.http.service, memory.allocate, task.suspend);

export async fn Readˉcbor(
    Budget: Memoryˉbudget,
    Body: borrow mut Requestˉbody,
    Profile: Cborˉprofile,
    Limits: Cborˉlimits,
    Context: borrow Operationˉcontext,
) -> Result<Cborˉdocument, Httpˉcborˉfailure>
    effects(network.http.service, memory.allocate, task.suspend);

export async fn Writeˉchunk(
    Body: borrow mut Responseˉbody,
    Value: Slice<u8>,
    Context: borrow Operationˉcontext,
) -> Httpˉbodyˉwriteˉoutcome effects(network.http.service, task.suspend);

export async fn Complete(
    Body: borrow mut Responseˉbody,
    Context: borrow Operationˉcontext,
) -> Httpˉbodyˉcompleteˉoutcome effects(network.http.service, task.suspend);

export fn Jsonˉresponse(
    Budget: Memoryˉbudget,
    Status: u16,
    Headers: borrow Httpˉheaders,
    Document: borrow Jsonˉdocument,
    Maximumˉoutputˉbytes: u64,
) -> Result<Httpˉresponse, Httpˉfailure> effects(memory.allocate);

export fn Cborˉresponse(
    Budget: Memoryˉbudget,
    Status: u16,
    Headers: borrow Httpˉheaders,
    Document: borrow Cborˉdocument,
    Profile: Cborˉprofile,
    Limits: Cborˉlimits,
) -> Result<Httpˉresponse, Httpˉfailure> effects(memory.allocate);
~~~

Body resources enforce declared framing, transfer, operation, and deadline
limits. An indeterminate response write terminates the request and is never
replayed on another connection. JSON/CBOR response construction completes
bounded encoding before the response becomes dispatchable, so an encoding
failure has known-zero network progress. These helpers set the exact media type
and encoded content length and reject caller headers that conflict with content
type, content length, transfer encoding, connection framing, or another field
owned by the service profile. `Jsonˉresponse` uses the Windvale deterministic
JSON profile; JCS output requires a separately named helper or prior explicit
encoding. `Cborˉresponse` accepts only a Windvale deterministic CBOR profile.

### `Networkˉhttpˉclient`

~~~text
export opaque Httpˉclientˉendpoint Copy;
export opaque resource Httpˉclient;
export opaque resource Httpˉclientˉresponse;

export fn Create(
    Endpoint: Httpˉclientˉendpoint,
    Limits: Httpˉclientˉlimits,
) -> Result<Httpˉclient, Httpˉfailure> effects(resource.acquire);

export async fn Send(
    Client: borrow mut Httpˉclient,
    Request: Httpˉclientˉrequest,
    Context: borrow Operationˉcontext,
) -> Result<Httpˉclientˉresponse, Httpˉfailure>
    effects(network.http.client, task.suspend);

export async fn Get(
    Client: borrow mut Httpˉclient,
    Target: borrow Uriˉoriginˉtarget,
    Headers: borrow Httpˉheaders,
    Context: borrow Operationˉcontext,
) -> Result<Httpˉclientˉresponse, Httpˉfailure>
    effects(network.http.client, task.suspend);

export async fn Post(
    Client: borrow mut Httpˉclient,
    Target: borrow Uriˉoriginˉtarget,
    Headers: borrow Httpˉheaders,
    Body: Slice<u8>,
    Context: borrow Operationˉcontext,
) -> Result<Httpˉclientˉresponse, Httpˉfailure>
    effects(network.http.client, task.suspend);

export async fn Postˉjson(
    Budget: Memoryˉbudget,
    Client: borrow mut Httpˉclient,
    Target: borrow Uriˉoriginˉtarget,
    Document: borrow Jsonˉdocument,
    Maximumˉoutputˉbytes: u64,
    Context: borrow Operationˉcontext,
) -> Result<Httpˉclientˉresponse, Httpˉfailure>
    effects(network.http.client, memory.allocate, task.suspend);

export async fn Postˉcbor(
    Budget: Memoryˉbudget,
    Client: borrow mut Httpˉclient,
    Target: borrow Uriˉoriginˉtarget,
    Document: borrow Cborˉdocument,
    Profile: Cborˉprofile,
    Limits: Cborˉlimits,
    Context: borrow Operationˉcontext,
) -> Result<Httpˉclientˉresponse, Httpˉfailure>
    effects(network.http.client, memory.allocate, task.suspend);

export fn Status(
    Response: borrow Httpˉclientˉresponse,
) -> u16 effects();

export fn Headers(
    Response: borrow Httpˉclientˉresponse,
) -> Httpˉheaders effects();

export async fn Readˉbodyˉbounded(
    Budget: Memoryˉbudget,
    Response: borrow mut Httpˉclientˉresponse,
    Maximumˉbytes: u64,
    Context: borrow Operationˉcontext,
) -> Result<Bytes, Httpˉfailure>
    effects(network.http.client, memory.allocate, task.suspend);
~~~

The endpoint binds allowed origins, target patterns, methods, header names,
trust policy, redirect policy, decompression policy, credential-injection
policy, connection limits, and byte limits. Version 1 defaults to no redirects,
no ambient proxy, no cookies, no decompression, no connection replay, and no
mutation retry. Selected same-origin redirects or decompression require
explicit policies.

`Postˉjson` uses the Windvale deterministic JSON encoder; a JCS-producing HTTP
operation, if needed, is separately named. `Postˉcbor` requires one of the two
Windvale deterministic CBOR profile values; an admission-only profile rejects.
Both helpers complete bounded encoding before network dispatch. An encoding
failure therefore has known-zero network progress. Once dispatch begins, the
ordinary HTTP mutation and indeterminate-completion rules apply; the helper
never re-encodes and replays an uncertain request automatically.

### `Networkˉhttpˉrouter`

~~~text
export opaque resource Routeˉtable;
export opaque Httpˉhandler Copy;

export fn Handlerˉcreate(
    Work: async fn(
        Request: Httpˉrequest,
        Context: borrow Operationˉcontext,
    ) -> Result<Httpˉresponse, Httpˉfailure>,
) -> Httpˉhandler effects();

export fn Construct(
    Budget: Memoryˉbudget,
    Limits: Routeˉlimits,
) -> Result<Routeˉtable, Routeˉfailure> effects(memory.allocate);

export fn Mapˉget(
    Routes: borrow mut Routeˉtable,
    Pattern: borrow text,
    Handler: Httpˉhandler,
) -> Result<unit, Routeˉfailure> effects();

export fn Mapˉpost(
    Routes: borrow mut Routeˉtable,
    Pattern: borrow text,
    Handler: Httpˉhandler,
) -> Result<unit, Routeˉfailure> effects();

export fn Mapˉput(
    Routes: borrow mut Routeˉtable,
    Pattern: borrow text,
    Handler: Httpˉhandler,
) -> Result<unit, Routeˉfailure> effects();

export fn Mapˉpatch(
    Routes: borrow mut Routeˉtable,
    Pattern: borrow text,
    Handler: Httpˉhandler,
) -> Result<unit, Routeˉfailure> effects();

export fn Mapˉdelete(
    Routes: borrow mut Routeˉtable,
    Pattern: borrow text,
    Handler: Httpˉhandler,
) -> Result<unit, Routeˉfailure> effects();

export fn Mapˉoptions(
    Routes: borrow mut Routeˉtable,
    Pattern: borrow text,
    Handler: Httpˉhandler,
) -> Result<unit, Routeˉfailure> effects();

export fn Group(
    Routes: borrow mut Routeˉtable,
    Prefix: borrow text,
    Child: Routeˉtable,
) -> Result<unit, Routeˉfailure> effects();

export fn Match(
    Routes: borrow Routeˉtable,
    Method: Httpˉmethod,
    Target: borrow Uriˉoriginˉtarget,
) -> Routeˉmatch effects();
~~~

Route patterns are parsed at construction. The first profile supports literal
segments and named single-segment parameters. Wildcard tails, regular
expressions, host routing, method override, and dynamic route mutation are not
implicit.

### `Networkˉhttpˉpipeline`

The pipeline uses typed filters, not reflection or a global service container.

~~~text
export opaque resource Httpˉpipeline;
export opaque Httpˉfilter Copy;

export fn Construct(
    Budget: Memoryˉbudget,
    Limits: Pipelineˉlimits,
) -> Result<Httpˉpipeline, Pipelineˉfailure> effects(memory.allocate);

export fn Use(
    Pipeline: borrow mut Httpˉpipeline,
    Filter: Httpˉfilter,
) -> Result<unit, Pipelineˉfailure> effects();

export fn Requestˉlimits(
    Policy: Requestˉlimitˉpolicy,
) -> Httpˉfilter effects();

export fn Requestˉidentity(
    Policy: Requestˉidentityˉpolicy,
) -> Httpˉfilter effects();

export fn Authorization(
    Policy: Authorizationˉpolicy,
) -> Httpˉfilter effects();

export fn Cors(
    Policy: Corsˉpolicy,
) -> Httpˉfilter effects();

export fn Structuredˉlogging(
    Policy: Httpˉlogˉpolicy,
) -> Httpˉfilter effects();

export fn Metrics(
    Policy: Httpˉmetricsˉpolicy,
) -> Httpˉfilter effects();

export fn Errorˉmapping(
    Policy: Errorˉmappingˉpolicy,
) -> Httpˉfilter effects();

export fn Rateˉlimit(
    Policy: Rateˉlimitˉpolicy,
) -> Httpˉfilter effects();
~~~

Each filter declares its required capabilities and retained-state maximum.
Importing a filter does not grant its logging, identity, database, clock, or
cache provider.

### `Networkˉhttpˉservice`

~~~text
export opaque Httpˉserviceˉendpoint Copy;
export opaque resource Httpˉservice;

export fn Create(
    Endpoint: Httpˉserviceˉendpoint,
    Routes: Routeˉtable,
    Pipeline: Httpˉpipeline,
    Limits: Httpˉserviceˉlimits,
) -> Result<Httpˉservice, Httpˉfailure> effects(resource.acquire);

export async fn Run(
    Service: borrow mut Httpˉservice,
    Context: borrow Operationˉcontext,
) -> Httpˉserviceˉoutcome
    effects(network.http.service, task.spawn, task.suspend);

export async fn Stopˉaccepting(
    Service: borrow mut Httpˉservice,
    Context: borrow Operationˉcontext,
) -> Httpˉserviceˉoutcome
    effects(network.http.service, task.suspend);

export async fn Drain(
    Service: borrow mut Httpˉservice,
    Context: borrow Operationˉcontext,
) -> Httpˉdrainˉoutcome
    effects(network.http.service, task.suspend);
~~~

The endpoint binds the listener, secure-transport policy, certificate/key
operation, trust generation, provider generation, connection and queue limits,
and teardown policy. Application source does not receive a raw listener or
private key.

### Browser-application helpers

These modules support browser clients without introducing server-side pages:

| Module | Candidate operations | Boundary |
| --- | --- | --- |
| `Networkˉhttpˉcors` | `Policyˉconstruct`, `Evaluateˉpreflight`, `Applyˉresponse` | Exact origins, methods, headers, credential mode, cache duration, and `Vary` behavior. |
| `Networkˉhttpˉcookie` | `Parseˉrequest`, `Encodeˉset`, `Validateˉprefix` | Bounded cookie count/bytes; no automatic persistent cookie jar. |
| `Networkˉhttpˉstatic` | `Create`, `Serve`, `Conditionalˉresponse` | Serves immutable resource-store or blob content; no ambient filesystem root. |
| `Networkˉhttpˉsse` | `Open`, `Writeˉevent`, `Writeˉcomment`, `Close` | Bounded event size, connection duration, queue, and cancellation. |
| `Networkˉwebsocket` | `Accept`, `Receive`, `Sendˉtext`, `Sendˉbytes`, `Ping`, `Close` | Optional 1.x profile after upgrade, masking, fragmentation, queue, and teardown semantics are frozen. |

Cookie signing/encryption, browser sessions, bearer tokens, and cross-site
request protection belong to Security/Backend identity modules. They are not
hidden inside the HTTP parser.

Routes may explicitly accept or produce `application/json`, `application/cbor`,
or the optional `application/cbor-seq`. Content type selects a codec profile;
it is not proof of schema validity or permission to interpret unrestricted
semantic tags.

## Backend identity and authorization

### `Backendˉidentity`

Transport authentication produces identity evidence; authorization remains a
separate decision.

~~~text
export opaque Identityˉendpoint Copy;
export opaque Authorizationˉendpoint Copy;

export async fn Authenticate(
    Endpoint: Identityˉendpoint,
    Credential: borrow Requestˉcredential,
    Context: borrow Operationˉcontext,
) -> Result<Principalˉevidence, Identityˉfailure>
    effects(identity.authenticate, task.suspend);

export async fn Authorize(
    Endpoint: Authorizationˉendpoint,
    Principal: borrow Principalˉevidence,
    Request: borrow Authorizationˉrequest,
    Context: borrow Operationˉcontext,
) -> Result<Authorizationˉdecision, Authorizationˉfailure>
    effects(identity.authorize, task.suspend);

export fn Allows(
    Decision: borrow Authorizationˉdecision,
    Operation: Authorizationˉoperation,
) -> bool effects();
~~~

JWT, OAuth 2.0, OpenID Connect, API-key, mutual-TLS, and application-session
profiles may feed this boundary only through separately specified strict
validators. Parsing unverified claims never creates `Principalˉevidence`.

## Configuration and cache modules

### `Backendˉconfiguration`

~~~text
export opaque resource Configuration;

export fn Parseˉjson(
    Budget: Memoryˉbudget,
    Input: Slice<u8>,
    Schema: borrow Configurationˉschema,
    Limits: Configurationˉlimits,
) -> Result<Configuration, Configurationˉfailure> effects(memory.allocate);

export fn Layer(
    Budget: Memoryˉbudget,
    Base: Configuration,
    Override: Configuration,
    Policy: Configurationˉlayerˉpolicy,
) -> Result<Configuration, Configurationˉfailure> effects(memory.allocate);

export fn Getˉtext(
    Configuration: borrow Configuration,
    Key: borrow Configurationˉkey,
) -> Option<text> effects();

export fn Getˉu64(
    Configuration: borrow Configuration,
    Key: borrow Configurationˉkey,
) -> Option<u64> effects();

export fn Getˉboolean(
    Configuration: borrow Configuration,
    Key: borrow Configurationˉkey,
) -> Option<bool> effects();
~~~

Provider-backed configuration acquisition is a separate Platform module.
Credentials and private keys remain opaque protected provider values and cannot
be retrieved through `Getˉtext`.

### `Backendˉcache`

~~~text
export opaque Cacheˉendpoint Copy;

export async fn Get(
    Endpoint: Cacheˉendpoint,
    Key: borrow Cacheˉkey,
    Context: borrow Operationˉcontext,
) -> Result<Option<Cacheˉentry>, Cacheˉfailure>
    effects(cache.read, task.suspend);

export async fn Put(
    Endpoint: Cacheˉendpoint,
    Key: borrow Cacheˉkey,
    Value: Slice<u8>,
    Policy: Cacheˉwriteˉpolicy,
    Context: borrow Operationˉcontext,
) -> Mutationˉoutcome<Cacheˉfailure>
    effects(cache.write, task.suspend);

export async fn Remove(
    Endpoint: Cacheˉendpoint,
    Key: borrow Cacheˉkey,
    Expectedˉgeneration: Option<u64>,
    Context: borrow Operationˉcontext,
) -> Mutationˉoutcome<Cacheˉfailure>
    effects(cache.write, task.suspend);

export async fn Compareˉexchange(
    Endpoint: Cacheˉendpoint,
    Key: borrow Cacheˉkey,
    Expectedˉgeneration: Option<u64>,
    Replacement: Slice<u8>,
    Policy: Cacheˉwriteˉpolicy,
    Context: borrow Operationˉcontext,
) -> Cacheˉexchangeˉoutcome
    effects(cache.write, task.suspend);
~~~

There is no `Getˉorˉcompute` in the first contract because hiding an effectful
callback behind cache miss, cancellation, or provider restart obscures ownership
and replay behavior. Applications spell the miss path explicitly.

## Diagnostics modules

### `Diagnosticsˉlog`

~~~text
export opaque Logˉendpoint Copy;
export opaque resource Logˉeventˉbuilder;

export fn Eventˉconstruct(
    Budget: Memoryˉbudget,
    Level: Logˉlevel,
    Eventˉidentity: Logˉeventˉidentity,
    Limits: Logˉlimits,
) -> Result<Logˉeventˉbuilder, Logˉfailure> effects(memory.allocate);

export fn Fieldˉtext(
    Event: borrow mut Logˉeventˉbuilder,
    Name: borrow Logˉfieldˉname,
    Value: borrow text,
    Classification: Dataˉclassification,
) -> Result<unit, Logˉfailure> effects();

export fn Fieldˉu64(
    Event: borrow mut Logˉeventˉbuilder,
    Name: borrow Logˉfieldˉname,
    Value: u64,
) -> Result<unit, Logˉfailure> effects();

export async fn Write(
    Endpoint: Logˉendpoint,
    Event: Logˉeventˉbuilder,
    Context: borrow Operationˉcontext,
) -> Logˉwriteˉoutcome effects(diagnostics.log, task.suspend);
~~~

The sink applies an explicit redaction policy. Secret, credential, private-key,
raw authorization, and unrestricted request-body classifications reject before
dispatch. Logging failure never changes the business mutation result unless the
application explicitly selects an audit-required policy before mutation.

### `Diagnosticsˉmetrics`

~~~text
export opaque Metricsˉendpoint Copy;

export fn Counterˉadd(
    Endpoint: Metricsˉendpoint,
    Metric: Metricˉidentity,
    Labels: borrow Metricˉlabels,
    Value: u64,
) -> Result<unit, Metricˉfailure> effects(diagnostics.metrics);

export fn Gaugeˉset(
    Endpoint: Metricsˉendpoint,
    Metric: Metricˉidentity,
    Labels: borrow Metricˉlabels,
    Value: i64,
) -> Result<unit, Metricˉfailure> effects(diagnostics.metrics);

export fn Histogramˉobserve(
    Endpoint: Metricsˉendpoint,
    Metric: Metricˉidentity,
    Labels: borrow Metricˉlabels,
    Value: u64,
) -> Result<unit, Metricˉfailure> effects(diagnostics.metrics);
~~~

Metric identities, label names, label values, cardinality, retained series, and
export work are bounded at binding. Arbitrary user data must not become an
unbounded label.

## WVDB application facade

### `Databaseˉwvdb`

`Databaseˉwvdb` provides application ergonomics but defers database semantics to
the WVDB 1.0 specifications. These names are placeholders for joint review with
the WVDB query, transaction, migration, backup, and restore contracts.

~~~text
export opaque Wvdbˉendpoint Copy;
export opaque resource Wvdbˉdatabase;
export opaque resource Readˉtransaction;
export opaque resource Writeˉtransaction;
export opaque resource Queryˉcursor;

export async fn Open(
    Endpoint: Wvdbˉendpoint,
    Expected: Databaseˉidentity,
    Limits: Databaseˉopenˉlimits,
    Context: borrow Operationˉcontext,
) -> Result<Wvdbˉdatabase, Databaseˉfailure>
    effects(database.open, resource.acquire, task.suspend);

export async fn Beginˉread(
    Database: borrow Wvdbˉdatabase,
    Options: Readˉtransactionˉoptions,
    Context: borrow Operationˉcontext,
) -> Result<Readˉtransaction, Databaseˉfailure>
    effects(database.read, resource.acquire, task.suspend);

export async fn Beginˉwrite(
    Database: borrow mut Wvdbˉdatabase,
    Options: Writeˉtransactionˉoptions,
    Context: borrow Operationˉcontext,
) -> Result<Writeˉtransaction, Databaseˉfailure>
    effects(database.write, resource.acquire, task.suspend);

export async fn Get(
    Transaction: borrow mut Readˉtransaction,
    Entity: Entityˉsetˉidentity,
    Identity: Primaryˉidentity,
    Context: borrow Operationˉcontext,
) -> Result<Option<Entityˉrecord>, Databaseˉfailure>
    effects(database.read, task.suspend);

export async fn Insert(
    Transaction: borrow mut Writeˉtransaction,
    Entity: Entityˉsetˉidentity,
    Record: Entityˉrecord,
    Context: borrow Operationˉcontext,
) -> Result<Insertˉresult, Databaseˉfailure>
    effects(database.write, task.suspend);

export async fn Update(
    Transaction: borrow mut Writeˉtransaction,
    Entity: Entityˉsetˉidentity,
    Identity: Primaryˉidentity,
    Expectedˉversion: Option<Recordˉversion>,
    Changes: Changeˉset,
    Context: borrow Operationˉcontext,
) -> Result<Updateˉresult, Databaseˉfailure>
    effects(database.write, task.suspend);

export async fn Delete(
    Transaction: borrow mut Writeˉtransaction,
    Entity: Entityˉsetˉidentity,
    Identity: Primaryˉidentity,
    Expectedˉversion: Option<Recordˉversion>,
    Context: borrow Operationˉcontext,
) -> Result<Deleteˉresult, Databaseˉfailure>
    effects(database.write, task.suspend);

export async fn Query(
    Transaction: borrow mut Readˉtransaction,
    Query: borrow Typedˉquery,
    Limits: Queryˉexecutionˉlimits,
    Context: borrow Operationˉcontext,
) -> Result<Queryˉcursor, Databaseˉfailure>
    effects(database.read, resource.acquire, task.suspend);

export async fn Readˉnext(
    Cursor: borrow mut Queryˉcursor,
    Maximumˉrecords: u32,
    Context: borrow Operationˉcontext,
) -> Result<Queryˉbatch, Databaseˉfailure>
    effects(database.read, task.suspend);

export async fn Commit(
    Transaction: borrow mut Writeˉtransaction,
    Context: borrow Operationˉcontext,
) -> Commitˉoutcome effects(database.write, resource.complete, task.suspend);

export fn Rollback(
    Transaction: Writeˉtransaction,
) -> Rollbackˉoutcome effects(database.write, resource.release);
~~~

Backup, restore, schema change, user/role administration, and destructive
database operations use separate administrative endpoints. Ordinary data
sessions never gain them transitively.

## Backend testing

### `Testingˉbackend`

`Testingˉbackend` provides deterministic substitutes with interface identities
that cannot bind to production capability requirements:

~~~text
export fn Virtualˉclockˉconstruct(
    Start: Virtualˉinstant,
    Limits: Virtualˉclockˉlimits,
) -> Virtualˉclock effects();

export fn Virtualˉclockˉadvance(
    Clock: borrow mut Virtualˉclock,
    Duration: Duration,
) -> Result<unit, Testˉfailure> effects();

export fn Deterministicˉentropyˉconstruct(
    Seed: Array<u8, 32>,
) -> Deterministicˉtestˉentropy effects();

export fn Httpˉserviceˉconstruct(
    Service: Httpˉservice,
    Limits: Testˉhttpˉlimits,
) -> Testˉhttpˉservice effects();

export async fn Httpˉsend(
    Service: borrow mut Testˉhttpˉservice,
    Request: Testˉhttpˉrequest,
    Context: borrow Operationˉcontext,
) -> Result<Testˉhttpˉresponse, Testˉfailure> effects(task.suspend);

export fn Faultˉplanˉconstruct(
    Limits: Faultˉplanˉlimits,
) -> Faultˉplan effects();

export fn Faultˉat(
    Plan: borrow mut Faultˉplan,
    Operation: Testˉoperationˉidentity,
    Occurrence: u64,
    Outcome: Testˉfault,
) -> Result<unit, Testˉfailure> effects();
~~~

The test suite must cover known-zero rejection, exact partial progress,
indeterminate mutation, cancellation, timeout, stale generations, provider
loss/restart, malformed provider responses, limits, and deterministic output.

## Deliberately deferred surfaces

The first Backend 1.0 implementation does not need to provide all of these at
once. The following remain optional or later-profile work until a production-
shaped consumer and exact contract justify them:

- HTTP/2, HTTP/3, QUIC, proxy discovery, transparent retries, and unrestricted
  redirects;
- automatic cookie persistence, browser automation, HTML templates, server-
  side components, controller reflection, and dependency-injection discovery;
- WebSocket compression and broad extension negotiation;
- SMTP, IMAP, message queues, distributed consensus, service discovery, and
  distributed tracing exporters;
- XML, YAML, MessagePack, BSON, Ion, archive containers, Brotli, and Zstandard;
- schema-bound Protocol Buffers, FlatBuffers, or Cap'n Proto profiles until a
  measured typed-service, IPC, or read-heavy workload justifies one;
- COSE encryption, MAC, multiple-signature, and private application-header
  profiles beyond the selected bounded Sign1 artifact use;
- OAuth/OpenID provider clients, JWT algorithms, password hashing, public-key
  encryption, and algorithms beyond the named TLS/COSE profiles before their
  trust and key contracts are qualified;
- memory mapping, filesystem watches, links, native ACLs, sparse files, and
  cross-provider atomic moves;
- a general ORM or automatic JSON-to-WVDB persistence mapper; and
- automatic backup placement, restore orchestration, or database administration
  through an ordinary application session.

Deferral does not reserve hidden behavior in an existing name. Each later
surface receives a focused module, limits, failure model, capability analysis,
and malformed-input suite before it joins a conformance profile.

## Review questions

The owner review should confirm or revise:

1. the `Windvale Backend 1.0` profile name;
2. concise module-qualified export names versus retaining existing long
   prefixed exports;
3. the selected Windvale deterministic JSON spelling and optional RFC 8785 JCS
   profile as distinct contracts;
4. the exact Windvale CBOR tag, numeric, and deterministic-profile policies;
5. the COSE Sign1 protected-header/domain policy, immutable-artifact provider,
   publisher and validator roles, receipt/index formats, and cache invalidation
   rules;
6. gzip and zlib-wrapped deflate as the first compression profiles;
7. the initial certificate encoding and public-key algorithm profiles;
8. whether the complete 1.0 profile requires WebSocket and server-sent events
   or leaves both optional;
9. whether HTTP/1.1 alone is sufficient for the first complete service profile;
10. the split between filesystem file, directory, and publication capabilities;
11. the first structured logging and metrics capability identities;
12. how WVDB typed records and queries appear without creating an ORM; and
13. the first exact vertical workload and quantitative limits.
