# Windvale binary data profile 1.0

## Status and recommendation

- Status: Draft proposal for owner review
- Suite: [Windvale Libraries 1.0](Windvale-Libraries-1.0.md)
- Public module: `Dataˉcbor`
- Schema/tooling module: `Dataˉcddl`
- Selected base standard: [CBOR, RFC 8949](https://www.rfc-editor.org/rfc/rfc8949.html)
- Streaming profile: [CBOR Sequences, RFC 8742](https://www.rfc-editor.org/rfc/rfc8742.html)
- Schema notation: [CDDL, RFC 8610](https://www.rfc-editor.org/rfc/rfc8610.html),
  with the backward-compatible grammar update in
  [RFC 9682](https://www.rfc-editor.org/rfc/rfc9682.html)
- Signed-object integration: [COSE, RFC 9052](https://www.rfc-editor.org/rfc/rfc9052.html),
  with initial algorithms defined by
  [RFC 9053](https://www.rfc-editor.org/rfc/rfc9053.html)

Windvale Libraries 1.0 should adopt standard CBOR as its general binary-document
format instead of designing a new binary equivalent of JSON. CBOR is self-
describing, covers JSON-shaped documents while also distinguishing text from
arbitrary bytes, represents integers directly, supports semantic tags, permits
streaming, and defines a basis for deterministic encoding.

CBOR reduces text parsing and can support validated views over original encoded
bytes. It does not eliminate validation or all decoding work. Untrusted CBOR
still requires bounded structural admission, checked lengths and arithmetic,
UTF-8 validation, duplicate-map handling, nesting/work limits, supported-tag
policy, and application-schema validation before typed use.

This proposal selects an existing interchange standard. It does not rename or
fork CBOR, claim that existing Windvale modules implement it, or make CBOR the
WVDB storage format automatically.

## Why CBOR is the recommended first format

Several established formats solve related but different problems:

| Format | Model and strengths | Important cost for Windvale | Recommendation |
| --- | --- | --- | --- |
| [CBOR](https://www.rfc-editor.org/rfc/rfc8949.html) | IETF Internet Standard; self-describing; compact integers, bytes, text, arrays, maps, tags, preferred and deterministic encodings; CDDL and sequence standards. | Still requires structural validation; the generic data model is broader than JSON; protocols must select tag, numeric, map-key, duplicate, and deterministic policies. | Adopt as `Dataˉcbor`. |
| [MessagePack](https://github.com/msgpack/msgpack/blob/master/spec.md) | Small, established JSON-like binary type system with integers, strings, bytes, arrays, maps, and extension values. | Its own specification describes deterministic map ordering as an application profile idea rather than one broadly standardized core profile; no IETF schema/sequence family comparable to CBOR/CDDL. | Interoperability module only if a real consumer needs it. |
| [BSON](https://bsonspec.org/spec.html) | Traversable JSON-like documents with binary, date, Decimal128, and database-oriented types. | Repeats field names, uses database-oriented types/layout choices, and is closely associated with MongoDB document storage rather than a small general protocol nucleus. | Do not select as the general format. |
| [Amazon Ion](https://amazon-ion.github.io/ion-docs/docs/spec.html) | Rich self-describing data model, interchangeable text/binary forms, decimals, timestamps, annotations, and read-oriented binary access. | Larger data model and symbol-table machinery than the first Windvale need; would introduce a second text notation in addition to JSON and Windvale source/configuration formats. | Reconsider for long-lived rich data only after measured need. |
| [Protocol Buffers](https://protobuf.dev/programming-guides/encoding/) | Compact schema-driven tagged fields, mature evolution rules and tooling. | Not self-describing; field meanings require a schema. Official documentation also states that deterministic serialization is not canonical across schema/build/library changes. | Candidate for a later schema-bound service profile, not binary JSON. |
| [FlatBuffers](https://flatbuffers.dev/white_paper/) | Schema-driven in-place traversal with low-to-zero read overhead and generated accessors. | Requires schema/code generation; buffers are normally not self-describing; alignment/layout and verifier rules are a different contract from a generic document. | Benchmark later for read-heavy typed records. |
| [Cap'n Proto](https://capnproto.org/encoding.html) | Schema-driven in-memory/wire layout, fast traversal, canonical form, and RPC model. | Strongly typed and not self-describing without its schema; word-aligned pointer layout and capability/RPC scope are much broader than a JSON-like document codec. | Benchmark later for typed IPC/RPC, not the general document format. |

The decision is not that CBOR is best for every binary workload. It is the best
fit for the requested **binary structured document next to JSON**. Schema-bound
formats should be evaluated separately against typed DTO, IPC, database-page,
and high-read-multiplier workloads.

## What work CBOR removes and retains

Compared with JSON, admitted CBOR avoids:

- scanning decimal text to discover ordinary integer values;
- quote, escape, and delimiter recognition for every container/value;
- Base64 expansion when the value is naturally bytes;
- repeated discovery of container lengths when definite-length encoding is
  used; and
- reparsing a validated immutable buffer when the library retains a bounded
  structural index and returns typed views.

CBOR still requires at least one admission pass over untrusted input. It also
requires application work to interpret map keys, tags, decimals, timestamps,
and schema meaning. “Binary” never means “safe to cast into memory.”

The first implementation should therefore optimize for **validate once, observe
many times** rather than promise zero parsing. A schema-driven in-place format
may later reduce even that work for selected typed records.

## Profiles

### RFC CBOR profile

The RFC profile admits well-formed and valid CBOR data items under explicit
resource limits and a declared supported-value/tag policy. It preserves the
encoded distinction among integer, floating, byte-string, text-string, array,
map, tag, and simple values.

A generic decoder must distinguish:

1. not well formed — the bytes cannot be decoded as one CBOR item;
2. not valid — the item violates a validity rule selected by the decoder;
3. unsupported — the item is valid CBOR but uses a value/tag/profile the
   implementation did not admit; and
4. unexpected — the item is valid and supported but does not match the
   application schema.

All Windvale profiles reject duplicate map keys under their selected CBOR data-
model equality rules. Generic RFC tools may preserve duplicate-bearing input
only in a separately named diagnostic profile that exposes rank observation and
does not permit keyed lookup. No application profile applies first-key-wins or
last-key-wins behavior.

### Windvale document profile 1

The first Windvale application profile narrows RFC CBOR:

- exactly one top-level data item;
- definite-length byte strings, text strings, arrays, and maps only;
- valid UTF-8 text;
- no duplicate map keys under CBOR data-model equality;
- maximum input bytes, nesting, items, map pairs, string bytes, byte-string
  bytes, tags, index bytes, and validation work;
- Boolean and null as the only untagged simple values in ordinary application
  documents;
- no undefined value, unassigned simple values, or floating-point values in the
  base document profile;
- unsigned and negative integers through the complete CBOR 64-bit argument
  range, without silently narrowing the negative domain to `i64`;
- binary16, binary32, and binary64 admission only in the separately named
  finite-floating document profile, which rejects infinities and all NaNs;
- arbitrary byte strings without text conversion;
- tags preserved generically, with typed interpretation only for a selected
  tag profile; and
- map ordering has no application semantics.

Text-keyed maps are recommended for JSON-equivalent documents. Integer keys are
recommended for compact versioned protocols. Other CBOR key types remain
available only when the application profile specifies their equality and
expected schema.

### Windvale deterministic CBOR profile 1

Persistent hashes, signatures, content addressing, golden fixtures, package
records, and reproducible protocols need a single encoding. The deterministic
profile adds the [core deterministic encoding requirements in RFC
8949](https://www.rfc-editor.org/rfc/rfc8949.html#section-4.2.1):

- preferred serialization and the shortest admitted integer, length, and tag
  arguments;
- no indefinite-length items;
- map keys sorted by bytewise lexical order of their deterministic encodings;
- a protocol-specific single representation for every admitted semantic tag or
  value choice; and
- rejection when two encoded keys are equal under the selected map-key model.

The base deterministic profile rejects every floating-point value. The
separately named deterministic finite-floating profile uses the shortest
binary16/32/64 encoding that preserves the exact finite value, preserves the
sign of zero, preserves finite subnormal values, and rejects infinities and all
NaNs. Implementations derive those choices from specified bit operations, not a
host formatter or an implicit host conversion.

The protocol using deterministic CBOR identifies the exact profile in its own
versioned contract or outer envelope. Windvale does not invent an unregistered
media type or assume that all `application/cbor` payloads use the Windvale
profile.

A signature authenticates exact serialized bytes and therefore does not require
deterministic CBOR to verify. Windvale nevertheless uses deterministic CBOR for
signed reproducible artifacts so independent qualified encoders produce one
content digest and can reuse the same signature, receipt, and cache identity.
Verification never decodes and re-encodes a payload before checking its
signature.

### CBOR Sequence profile

[RFC 8742](https://www.rfc-editor.org/rfc/rfc8742.html) defines a sequence as
zero or more concatenated, self-delimiting CBOR data items. The optional
`Dataˉcborˉsequence` profile supports logs, result streams, import/export, and
incremental service bodies without requiring one unbounded array.

Every sequence consumer still has per-item and whole-sequence limits. End of
transport is not confused with a truncated final item. Sequence items do not
inherit a schema, trust decision, or transaction merely because they share one
stream.

## Generic data model

Candidate public values are:

~~~text
export enum Cborˉkind: u8 {
    Unsigned;
    Negative;
    Bytes;
    Text;
    Array;
    Map;
    Tag;
    Boolean;
    Null;
    Simple;
    Float;
}

export variant Cborˉinteger {
    Unsigned(Value: u64);
    Negativeˉargument(Argument: u64);
}

export enum Cborˉfloatˉwidth: u8 {
    Binary16;
    Binary32;
    Binary64;
}

export record Cborˉfloatˉbits {
    Width: Cborˉfloatˉwidth;
    Bits: u64;
}

export enum Cborˉprofile: u8 {
    Rfc8949;
    Windvaleˉdocumentˉone;
    Windvaleˉdocumentˉfiniteˉfloatˉone;
    Windvaleˉdeterministicˉone;
    Windvaleˉdeterministicˉfiniteˉfloatˉone;
}

export record Cborˉlimits {
    Maximumˉinputˉbytes: u64;
    Maximumˉdepth: u32;
    Maximumˉitems: u64;
    Maximumˉmapˉpairs: u64;
    Maximumˉtextˉbytes: u64;
    Maximumˉbyteˉstringˉbytes: u64;
    Maximumˉtags: u32;
    Maximumˉindexˉbytes: u64;
    Maximumˉwork: u64;
    Maximumˉoutputˉbytes: u64;
}

export opaque resource Cborˉdocument;
export opaque Cborˉvalueˉview Copy;
export opaque Cborˉmapˉentryˉview Copy;
export opaque Cborˉbyteˉstringˉview Copy;
export opaque Cborˉtextˉstringˉview Copy;
~~~

`Negativeˉargument(Argument)` represents the CBOR mathematical value
`-1 - Argument`. This preserves the full major-type-1 domain without an
overflowing conversion to `i64`.

## Admission and observation API

`Admit` consumes immutable owned input, validates it once, and may build a
bounded structural index. Byte-string, text-string, and value views refer to the
admitted document; they do not allocate or outlive it. Segment observation
keeps the generic RFC profile able to represent indefinite-length strings while
the Windvale application profiles require one definite-length segment.

~~~text
export fn Admit(
    Budget: Memoryˉbudget,
    Input: Bytes,
    Profile: Cborˉprofile,
    Limits: Cborˉlimits,
) -> Result<Cborˉdocument, Cborˉfailure>
    effects(memory.allocate);

export fn Validateˉdeterministic(
    Budget: Memoryˉbudget,
    Input: Slice<u8>,
    Profile: Cborˉprofile,
    Limits: Cborˉlimits,
) -> Result<unit, Cborˉfailure> effects(memory.allocate);

export fn Root(
    Document: borrow Cborˉdocument,
) -> Cborˉvalueˉview effects();

export fn Encoded(
    Value: Cborˉvalueˉview,
) -> Slice<u8> effects();

export fn Kind(
    Value: Cborˉvalueˉview,
) -> Cborˉkind effects();

export fn Integer(
    Value: Cborˉvalueˉview,
) -> Option<Cborˉinteger> effects();

export fn Boolean(
    Value: Cborˉvalueˉview,
) -> Option<bool> effects();

export fn Simpleˉvalue(
    Value: Cborˉvalueˉview,
) -> Option<u8> effects();

export fn Floatˉbits(
    Value: Cborˉvalueˉview,
) -> Option<Cborˉfloatˉbits> effects();

export fn Byteˉstring(
    Value: Cborˉvalueˉview,
) -> Option<Cborˉbyteˉstringˉview> effects();

export fn Byteˉstringˉlength(
    Value: Cborˉbyteˉstringˉview,
) -> u64 effects();

export fn Byteˉstringˉsegmentˉcount(
    Value: Cborˉbyteˉstringˉview,
) -> u64 effects();

export fn Byteˉstringˉsegmentˉat(
    Value: Cborˉbyteˉstringˉview,
    Rank: u64,
) -> Option<Slice<u8>> effects();

export fn Textˉstring(
    Value: Cborˉvalueˉview,
) -> Option<Cborˉtextˉstringˉview> effects();

export fn Textˉstringˉbyteˉlength(
    Value: Cborˉtextˉstringˉview,
) -> u64 effects();

export fn Textˉstringˉsegmentˉcount(
    Value: Cborˉtextˉstringˉview,
) -> u64 effects();

export fn Textˉstringˉsegmentˉat(
    Value: Cborˉtextˉstringˉview,
    Rank: u64,
) -> Option<text> effects();

export fn Arrayˉlength(
    Value: Cborˉvalueˉview,
) -> Option<u64> effects();

export fn Arrayˉat(
    Value: Cborˉvalueˉview,
    Index: u64,
) -> Option<Cborˉvalueˉview> effects();

export fn Mapˉlength(
    Value: Cborˉvalueˉview,
) -> Option<u64> effects();

export fn Mapˉentryˉat(
    Value: Cborˉvalueˉview,
    Rank: u64,
) -> Option<Cborˉmapˉentryˉview> effects();

export fn Mapˉkey(
    Entry: Cborˉmapˉentryˉview,
) -> Cborˉvalueˉview effects();

export fn Mapˉvalue(
    Entry: Cborˉmapˉentryˉview,
) -> Cborˉvalueˉview effects();

export fn Mapˉgetˉtext(
    Value: Cborˉvalueˉview,
    Name: borrow text,
) -> Option<Cborˉvalueˉview> effects();

export fn Mapˉgetˉunsigned(
    Value: Cborˉvalueˉview,
    Key: u64,
) -> Option<Cborˉvalueˉview> effects();

export fn Tagˉnumber(
    Value: Cborˉvalueˉview,
) -> Option<u64> effects();

export fn Taggedˉvalue(
    Value: Cborˉvalueˉview,
) -> Option<Cborˉvalueˉview> effects();
~~~

Rank observation follows encoded order for the RFC profile and deterministic
key order for deterministic input. Applications must not assign semantic
meaning to generic map rank.

The first implementation may use a compact offset/length/kind index rather than
materialize a tree. It must reject before retaining index state beyond
`Maximumˉindexˉbytes`. A no-index cursor implementation remains the simple
oracle.

## Writer API

The first writer emits definite-length containers. Container length is declared
before its contents, so the writer can reject too many/few elements and produce
deterministic output without buffering an unbounded subtree.

~~~text
export opaque resource Cborˉwriter;

export fn Writerˉconstruct(
    Budget: Memoryˉbudget,
    Profile: Cborˉprofile,
    Limits: Cborˉlimits,
) -> Result<Cborˉwriter, Cborˉfailure>
    effects(memory.allocate);

export fn Writeˉunsigned(
    Writer: borrow mut Cborˉwriter,
    Value: u64,
) -> Result<unit, Cborˉfailure> effects();

export fn Writeˉnegative(
    Writer: borrow mut Cborˉwriter,
    Argument: u64,
) -> Result<unit, Cborˉfailure> effects();

export fn Writeˉbytes(
    Writer: borrow mut Cborˉwriter,
    Value: Slice<u8>,
) -> Result<unit, Cborˉfailure> effects();

export fn Writeˉtext(
    Writer: borrow mut Cborˉwriter,
    Value: borrow text,
) -> Result<unit, Cborˉfailure> effects();

export fn Writeˉboolean(
    Writer: borrow mut Cborˉwriter,
    Value: bool,
) -> Result<unit, Cborˉfailure> effects();

export fn Writeˉnull(
    Writer: borrow mut Cborˉwriter,
) -> Result<unit, Cborˉfailure> effects();

export fn Writeˉsimple(
    Writer: borrow mut Cborˉwriter,
    Value: u8,
) -> Result<unit, Cborˉfailure> effects();

export fn Writeˉfloatˉbits(
    Writer: borrow mut Cborˉwriter,
    Value: Cborˉfloatˉbits,
) -> Result<unit, Cborˉfailure> effects();

export fn Writeˉtag(
    Writer: borrow mut Cborˉwriter,
    Tag: u64,
) -> Result<unit, Cborˉfailure> effects();

export fn Writeˉarrayˉbegin(
    Writer: borrow mut Cborˉwriter,
    Length: u64,
) -> Result<unit, Cborˉfailure> effects();

export fn Writeˉmapˉbegin(
    Writer: borrow mut Cborˉwriter,
    Pairs: u64,
) -> Result<unit, Cborˉfailure> effects();

export fn Writeˉcontainerˉend(
    Writer: borrow mut Cborˉwriter,
) -> Result<unit, Cborˉfailure> effects();

export fn Finish(
    Writer: Cborˉwriter,
) -> Result<Bytes, Cborˉfailure> effects();

export fn Encodeˉdeterministic(
    Budget: Memoryˉbudget,
    Document: borrow Cborˉdocument,
    Profile: Cborˉprofile,
    Limits: Cborˉlimits,
) -> Result<Bytes, Cborˉfailure>
    effects(memory.allocate);
~~~

`Validateˉdeterministic` and `Encodeˉdeterministic` accept only
`Windvaleˉdeterministicˉone` or
`Windvaleˉdeterministicˉfiniteˉfloatˉone`; passing an admission-only profile is
a profile mismatch. Naming the exact deterministic profile prevents the caller
and verifier from disagreeing about floating-point admission.

For deterministic maps, the low-level writer requires keys in encoded lexical
order and rejects an equal or out-of-order key before accepting its associated
value. A separately named bounded map builder may accept unordered entries,
encode each key once, sort complete admitted key/value spans, and then feed the
writer. Ordinary `Writeˉmapˉbegin` never hides that allocation or buffering.

`Writeˉsimple` is available only in the generic RFC profile. The Windvale
document and deterministic profiles require `Writeˉboolean` or `Writeˉnull` and
reject every other untagged simple value.

## Failure model

~~~text
export variant Cborˉfailure {
    Inputˉlimit(Observed: u64, Maximum: u64);
    Truncated(Offset: u64);
    Invalidˉinitialˉbyte(Offset: u64);
    Invalidˉadditionalˉinformation(Offset: u64);
    Invalidˉbreak(Offset: u64);
    Lengthˉoverflow(Offset: u64);
    Rangeˉoutsideˉinput(Offset: u64, Length: u64);
    Invalidˉutf8(Offset: u64);
    Depthˉlimit(Observed: u32, Maximum: u32);
    Itemˉlimit(Observed: u64, Maximum: u64);
    Mapˉpairˉlimit(Observed: u64, Maximum: u64);
    Duplicateˉmapˉkey(Offset: u64);
    Unsupportedˉsimple(Value: u8, Offset: u64);
    Unsupportedˉtag(Tag: u64, Offset: u64);
    Unsupportedˉfloat(Width: Cborˉfloatˉwidth, Offset: u64);
    Nonfiniteˉfloat(Offset: u64);
    Nonpreferredˉencoding(Offset: u64);
    Indefiniteˉlengthˉforbidden(Offset: u64);
    Mapˉkeyˉorder(Offset: u64);
    Trailingˉinput(Offset: u64);
    Containerˉcount(Expected: u64, Observed: u64);
    Workˉlimit(Observed: u64, Maximum: u64);
    Allocation(Error: Allocationˉfailure);
    Indexˉcapacity(Error: Collectionˉfailure);
}
~~~

Checked `u64` arithmetic is used for every argument, header, offset, length,
container count, tag, work counter, and encoded span. The parser never allocates
from an untrusted length before checking the selected limit and remaining input.

## Standard tag policy

Generic RFC admission may preserve an unknown valid tag as an uninterpreted
tag/value pair. A typed profile recognizes only named standards and checks the
tagged value shape.

The first candidate typed profiles are:

| Purpose | Standard direction |
| --- | --- |
| Positive/negative bignum | CBOR tags 2 and 3 for values outside the ordinary 64-bit argument domain. |
| Decimal128 | CBOR decimal-fraction tag 4 with exact exponent/coefficient policy; a coefficient outside ordinary integers uses the selected bignum representation. |
| UTC instant | RFC 8949 date/time tags only after exact text/epoch precision and leap/time policy is selected. |
| Duration and period | Evaluate the standard tags in [RFC 9581](https://www.rfc-editor.org/rfc/rfc9581.html) against Foundation time types. |
| Embedded CBOR | Tag 24 only under an explicit recursive depth/byte/work budget. |
| URI | Tag 32 only when the tagged text passes `Dataˉuri` under the selected URI profile. |
| Self-described CBOR | Tag 55799 may be admitted for tools but does not replace a protocol version or trust decision. |

No private application tag enters the shared profile merely because one
implementation uses it. Register or version the tag in the owning protocol and
state its deterministic representation.

## JSON conversion

CBOR can represent JSON-shaped data, but the CBOR and JSON numeric models are
not identical and the CBOR generic data model is broader. Conversion therefore
uses explicit policy and can fail.

~~~text
export enum Cborˉjsonˉnumber: u8 {
    Exactˉdecimal;
    Finiteˉbinary64;
}

export record Cborˉjsonˉpolicy {
    Numbers: Cborˉjsonˉnumber;
    Maximumˉoutputˉbytes: u64;
}

export fn Fromˉjson(
    Budget: Memoryˉbudget,
    Document: borrow Jsonˉdocument,
    Profile: Cborˉprofile,
    Policy: Cborˉjsonˉpolicy,
    Limits: Cborˉlimits,
) -> Result<Cborˉdocument, Cborˉconversionˉfailure>
    effects(memory.allocate);

export fn Toˉjson(
    Budget: Memoryˉbudget,
    Document: borrow Cborˉdocument,
    Policy: Cborˉjsonˉpolicy,
    Limits: Jsonˉlimits,
) -> Result<Jsonˉdocument, Cborˉconversionˉfailure>
    effects(memory.allocate);
~~~

Generic conversion accepts only the JSON-shaped subset: null, Boolean, selected
numbers, text strings, arrays, and maps with text keys. It rejects byte strings,
non-text map keys, non-finite floats, and any other CBOR value without one JSON
meaning. It never silently Base64-encodes, stringifies, or drops a value. A typed
schema adapter may deliberately map a known byte field through
`Dataˉencoding.Base64urlˉencode`; that application mapping is not a generic
document conversion rule.

`Exactˉdecimal` maps an integer spelling to the ordinary CBOR integer or bignum
domain and a non-integer decimal spelling to standard decimal-fraction tag 4,
subject to coefficient/exponent and tag limits. `Finiteˉbinary64` performs an
explicit checked binary64 conversion and rejects overflow or a non-finite
result; `Fromˉjson` also requires a finite-floating CBOR profile when that result
is not exactly an integer. In the reverse direction, `Exactˉdecimal` accepts
only ordinary integers, standard bignum tags 2 and 3, and decimal-fraction tag
4, then emits an exact JSON decimal token or rejects it under the selected JSON
limits.
`Finiteˉbinary64` accepts only numbers with one finite binary64 value. All other
CBOR tags reject in both modes.

## CDDL schema/tooling profile

[CDDL](https://www.rfc-editor.org/rfc/rfc8610.html) is an established notation
for describing CBOR and JSON data structures. It is useful for specifications,
protocol fixtures, schema validation, and generated typed adapters.

The first direction is a build/tooling module rather than a mandatory runtime
parser. A package may compile a reviewed CDDL source into one deterministic
bounded schema descriptor; runtime validation consumes the descriptor without
reparsing schema text.

~~~text
export opaque resource Cddlˉschema;
export opaque resource Cddlˉdescriptor;

export fn Compile(
    Budget: Memoryˉbudget,
    Input: borrow text,
    Limits: Cddlˉlimits,
) -> Result<Cddlˉschema, Cddlˉfailure>
    effects(memory.allocate);

export fn Descriptor(
    Budget: Memoryˉbudget,
    Schema: borrow Cddlˉschema,
    Maximumˉbytes: u64,
) -> Result<Cddlˉdescriptor, Cddlˉfailure>
    effects(memory.allocate);

export fn Validateˉcbor(
    Budget: Memoryˉbudget,
    Schema: borrow Cddlˉdescriptor,
    Rootˉrule: borrow Cddlˉruleˉidentity,
    Document: borrow Cborˉdocument,
    Limits: Cddlˉvalidationˉlimits,
) -> Result<unit, Cddlˉvalidationˉfailure> effects(memory.allocate);

export fn Validateˉjson(
    Budget: Memoryˉbudget,
    Schema: borrow Cddlˉdescriptor,
    Rootˉrule: borrow Cddlˉruleˉidentity,
    Document: borrow Jsonˉdocument,
    Limits: Cddlˉvalidationˉlimits,
) -> Result<unit, Cddlˉvalidationˉfailure> effects(memory.allocate);
~~~

CDDL support does not imply every control operator or extension is implemented.
The admitted grammar/update set, controls, generics, recursion, regex behavior,
work accounting, diagnostic order, and descriptor format must be named exactly.
Generated Windvale record adapters are a later tool over an accepted schema;
they do not add reflection to the runtime.

## CBOR Sequence API

~~~text
export opaque resource Cborˉsequenceˉdecoder;
export opaque resource Cborˉsequenceˉwriter;

export fn Sequenceˉdecoderˉopen(
    Budget: Memoryˉbudget,
    Profile: Cborˉprofile,
    Limits: Cborˉsequenceˉlimits,
) -> Result<Cborˉsequenceˉdecoder, Cborˉfailure>
    effects(memory.allocate);

export fn Sequenceˉdecodeˉupdate(
    Decoder: borrow mut Cborˉsequenceˉdecoder,
    Input: Slice<u8>,
) -> Cborˉsequenceˉprogress effects();

export fn Sequenceˉtake(
    Decoder: borrow mut Cborˉsequenceˉdecoder,
) -> Option<Cborˉdocument> effects();

export fn Sequenceˉdecodeˉfinish(
    Decoder: Cborˉsequenceˉdecoder,
) -> Result<unit, Cborˉfailure> effects();

export fn Sequenceˉwriterˉopen(
    Budget: Memoryˉbudget,
    Limits: Cborˉsequenceˉlimits,
) -> Result<Cborˉsequenceˉwriter, Cborˉfailure>
    effects(memory.allocate);

export fn Sequenceˉappend(
    Writer: borrow mut Cborˉsequenceˉwriter,
    Document: borrow Cborˉdocument,
) -> Result<unit, Cborˉfailure> effects();

export fn Sequenceˉfinish(
    Writer: Cborˉsequenceˉwriter,
) -> Result<Bytes, Cborˉfailure> effects();
~~~

A hosted stream adapter reads and writes bounded chunks around this portable
state. The codec itself owns no network, filesystem, blob, or WVDB capability.

## HTTP integration

Windvale HTTP recognizes standard media types without making content type a
trust or schema decision:

- `application/json` uses `Dataˉjson`;
- `application/cbor` uses one CBOR data item under the route's selected profile;
  and
- `application/cbor-seq` uses the optional CBOR Sequence profile.

Candidate convenience operations are:

~~~text
export async fn Readˉcbor(
    Budget: Memoryˉbudget,
    Body: borrow mut Requestˉbody,
    Profile: Cborˉprofile,
    Limits: Cborˉlimits,
    Context: borrow Operationˉcontext,
) -> Result<Cborˉdocument, Httpˉcborˉfailure>
    effects(network.http.service, memory.allocate, task.suspend);

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

export fn Cborˉresponse(
    Budget: Memoryˉbudget,
    Status: u16,
    Headers: borrow Httpˉheaders,
    Document: borrow Cborˉdocument,
    Profile: Cborˉprofile,
    Limits: Cborˉlimits,
) -> Result<Httpˉresponse, Httpˉfailure> effects(memory.allocate);
~~~

A route explicitly lists accepted and produced media profiles. Content
negotiation is bounded and deterministic. Receiving `application/cbor` does not
skip CDDL/application validation or grant permission to use tags outside the
route profile.

The output profile is explicit and must name one of the deterministic variants.
Encoding finishes within its output and memory limits before `Postˉcbor`
dispatches any network mutation or `Cborˉresponse` publishes a response. An
encoding failure therefore has known-zero output progress; an indeterminate
network write is never replayed. `Cborˉresponse` sets the exact media type and
content length and rejects caller headers that conflict with content type,
framing, or another service-owned field.

JSON remains part of Data 1.0 because it is human-readable and has native
support in browsers and many operational tools. CBOR is an opt-in companion for
compact binary transfer, direct byte strings, deterministic artifacts, and
clients that carry a qualified decoder. A browser adapter must bundle or load a
bounded CBOR implementation; it cannot assume a built-in equivalent of
`JSON.parse`.

## WVDB and storage boundary

`Dataˉcbor` may be used for:

- WVDB document fields that explicitly select a CBOR value type;
- client/service request and result envelopes;
- backup manifests, schema catalogs, or change streams whose owning
  specification selects deterministic CBOR;
- immutable blob metadata; and
- application-owned binary documents.

It does not decide:

- WVDB page, log, superblock, index-key, transaction, or backup physical layout;
- whether an arbitrary CBOR map is a table row or schema;
- index semantics for generic map keys or tags;
- automatic JSON/CBOR storage conversion;
- large-object placement; or
- database compatibility and migration.

WVDB owns those contracts. If a WVDB format embeds CBOR, it names the exact CBOR
profile, maximum, schema, deterministic requirement, and failure behavior.

## Authenticated and prevalidated artifacts

Authentication, CBOR admission, schema validation, and authorization are four
different claims:

| Claim | What it proves | What it does not prove |
| --- | --- | --- |
| Authenticated bytes | The exact byte string is covered by an accepted signature or signed package manifest and an authorized publisher identity. | CBOR structure, schema, resource limits, application meaning, or permission to perform an action. |
| Admitted CBOR | The bytes satisfy one exact CBOR profile and observed structural limits. | Publisher identity, application schema, or authorization. |
| Schema-approved data | The admitted root satisfies one exact schema descriptor and root rule. | That the signer may publish it or that the application may act on it. |
| Authorized use | Current policy permits the authenticated identity and validated data to be used for one named purpose. | Permission for another purpose, signer, schema, or trust generation. |

Windvale never exposes a deserializable or caller-settable `Trusted: bool`. A
path, extension, modification time, mutable file handle, successful TLS
connection, or ordinary publisher signature is insufficient. The bytes must be
an immutable snapshot before identity is checked, and the same snapshot must be
used afterward so a path replacement cannot race verification.

The provider rejects a known byte length above the selected hard maximum before
allocation, hashing, signature work, or CBOR admission. A source without a
trusted known length is read through a bounded stream and stops at the maximum;
signed metadata never authorizes an oversized allocation.

### Signed-object profile

The first standard signed-object profile is COSE Sign1 from [RFC
9052](https://www.rfc-editor.org/rfc/rfc9052.html), with EdDSA algorithm `-8`, OKP
key type, and Ed25519 curve `6` from [RFC
9053](https://www.rfc-editor.org/rfc/rfc9053.html). The protected headers carry
the algorithm identity and policy fixes the curve and key usage; input never
negotiates them. Attached and detached payloads are both allowed under a small
exact signature-input maximum.

The selected pure EdDSA profile processes the complete signature input. A large
CBOR file therefore is not copied into a large COSE message merely to sign it.
Instead, a small deterministic manifest or validation receipt carries the
file's SHA-256 digest and exact length and is signed with COSE Sign1. The file is
then streamed once to verify its digest while CBOR admission and index
construction proceed under their own bounds.

Receipt profile 1 fixes SHA-256; an input field cannot negotiate a weaker or
unknown digest. A later digest profile requires a new contract identity and
does not reinterpret existing receipts.

COSE authenticates the exact serialized receipt or small payload. The external
authenticated data includes a fixed application-domain identity such as the
validation-receipt contract, preventing a valid signature for one Windvale
purpose from being replayed as another. The verifier also checks that the
signing identity is authorized for the claimed publisher or validator role, the
algorithm and key usage are allowed, required protected headers are understood,
and the selected trust-generation/freshness policy passes. A COSE parser is
itself bounded untrusted-input processing.

### Validation receipts and indexes

`Dataˉcborˉartifact` is an optional companion module. A qualified validator may
produce a deterministic receipt and optional structural index after ordinary
CBOR admission and, when selected, CDDL/application-schema validation.
The Core module does not read files, select signers, verify COSE, consult a
trust store, or issue trusted evidence; `Platformˉartifactˉtrust` owns that
Hosted composition. This keeps the codec usable without a security provider and
prevents a Data-to-Platform dependency cycle.

The receipt binds at least:

- receipt format and major contract version;
- artifact namespace and permitted use;
- content digest algorithm/value and exact byte length;
- exact CBOR profile and signature-set identity;
- deterministic-encoding result when claimed;
- schema descriptor digest, root-rule identity, and schema-validation contract,
  or an explicit statement that no schema was validated;
- observed input bytes, depth, items, map pairs, text bytes, byte-string bytes,
  tags, index bytes, and validation work;
- validator contract identity; and
- optional structural-index format identity, byte length, and digest.

The receipt does not rely on a file path, timestamp, inode, mutable generation,
or cache location. It is ordinary deterministic data until an authorized
validator attestation authenticates it. Publisher authority and validator
authority are separate roles even when policy permits one key to hold both.

The normal persistent flow is:

1. the validator ordinarily admits the immutable content, applies the selected
   schema, and creates the deterministic receipt and optional index;
2. a validator-authorized COSE Sign1 authenticates the receipt under the
   validation-receipt domain;
3. the publisher creates a deterministic artifact manifest containing the
   content SHA-256/length and optional receipt-payload SHA-256, then signs that
   manifest under the artifact-manifest domain;
4. the first loader ordinarily validates both small signed objects, streams the
   content once for digest and admission, and compares the result with the
   receipt; and
5. later loaders may reuse opaque evidence only while the immutable content,
   receipt, index, schema, validator contract, and trust policy remain exact.

Candidate portable artifact operations are:

~~~text
export record Cborˉobservedˉlimits {
    Inputˉbytes: u64;
    Depth: u32;
    Items: u64;
    Mapˉpairs: u64;
    Textˉbytes: u64;
    Byteˉstringˉbytes: u64;
    Tags: u32;
    Indexˉbytes: u64;
    Validationˉwork: u64;
}

export record Cborˉreceiptˉlimits {
    Maximumˉreceiptˉbytes: u64;
    Maximumˉindexˉbytes: u64;
    Maximumˉwork: u64;
}

export variant Cborˉschemaˉrequirement {
    None;
    Cddl(
        Descriptor: Cddlˉdescriptor,
        Rootˉrule: Cddlˉruleˉidentity,
        Limits: Cddlˉvalidationˉlimits,
    );
}

export opaque resource Cborˉartifactˉcontract;
export opaque resource Cborˉvalidatedˉartifact;
export opaque resource Cborˉvalidationˉreceipt;
export opaque resource Cborˉprevalidatedˉsource;

export variant Cborˉartifactˉfailure {
    Cbor(Error: Cborˉfailure);
    Schema(Error: Cddlˉvalidationˉfailure);
    Receiptˉinvalid(Field: u32, Offset: u64);
    Contentˉidentityˉmismatch;
    Profileˉmismatch;
    Schemaˉmismatch;
    Validatorˉcontractˉmismatch;
    Consumerˉlimit(Field: u32, Observed: u64, Maximum: u64);
    Indexˉmissing;
    Indexˉidentityˉmismatch;
    Indexˉrange(Offset: u64, Length: u64, Contentˉlength: u64);
    Evidenceˉstale;
    Allocation(Error: Allocationˉfailure);
}

export fn Artifactˉcontractˉconstruct(
    Budget: Memoryˉbudget,
    Namespace: Artifactˉnamespace,
    Permittedˉuse: Artifactˉuse,
    Profile: Cborˉprofile,
    Schema: Cborˉschemaˉrequirement,
) -> Result<Cborˉartifactˉcontract, Cborˉartifactˉfailure>
    effects(memory.allocate);

export fn Validateˉartifact(
    Budget: Memoryˉbudget,
    Input: Bytes,
    Contract: borrow Cborˉartifactˉcontract,
    Limits: Cborˉlimits,
) -> Result<Cborˉvalidatedˉartifact, Cborˉartifactˉfailure>
    effects(memory.allocate);

export fn Encodeˉreceipt(
    Budget: Memoryˉbudget,
    Artifact: borrow Cborˉvalidatedˉartifact,
    Validatorˉcontract: Cborˉvalidatorˉcontractˉidentity,
    Maximumˉbytes: u64,
) -> Result<Bytes, Cborˉartifactˉfailure> effects(memory.allocate);

export fn Encodeˉindex(
    Budget: Memoryˉbudget,
    Artifact: borrow Cborˉvalidatedˉartifact,
    Maximumˉbytes: u64,
) -> Result<Option<Bytes>, Cborˉartifactˉfailure> effects(memory.allocate);

export fn Takeˉdocument(
    Artifact: Cborˉvalidatedˉartifact,
) -> Cborˉdocument effects();

export fn Admitˉreceipt(
    Budget: Memoryˉbudget,
    Input: Bytes,
    Limits: Cborˉreceiptˉlimits,
) -> Result<Cborˉvalidationˉreceipt, Cborˉartifactˉfailure>
    effects(memory.allocate);

export fn Openˉprevalidated(
    Budget: Memoryˉbudget,
    Source: Cborˉprevalidatedˉsource,
    Consumerˉlimits: Cborˉlimits,
) -> Result<Cborˉdocument, Cborˉartifactˉfailure>
    effects(memory.allocate);
~~~

A receipt cannot be turned into trusted evidence by application source.
`Platformˉartifactˉtrust` creates `Cborˉprevalidatedˉsource` only after an
immutable content identity, receipt, authorized validator attestation, required
profile/schema, consumer limits, trust generation, and optional index all
match. A consumer may accept the receipt only when every observed value is
within its own limit; a receipt created under a large maximum does not widen a
smaller consumer policy.

The Platform composition creates the source by consuming one authenticated
immutable content handle, the accepted receipt state, and its optional
authenticated index. There is no public constructor from ordinary `Bytes` or a
separate evidence value, so application code cannot pair a valid receipt with a
different buffer. A provider that cannot preserve such an identity must rehash
the bytes before it may create the source.

The small COSE envelope, protected headers, and validation receipt always pass
ordinary bounded parsing and validation. The prevalidated fast path never uses
a receipt or index to validate that same receipt, its signature, or its own
index metadata.

`Openˉprevalidated` still checks local index headers, checked offset arithmetic,
range containment, root identity, and exact content/index association before
returning views. These cheap checks protect against implementation defects and
misbinding without repeating the complete structural walk. If there is no
accepted receipt or index, the caller uses ordinary `Admit` and schema
validation.

An index format declares its version, integer widths, byte order, alignment,
maximum entries, and target scope. A portable index cannot contain native
pointers. A target-specific optimized index is cached only for that exact
runtime/architecture contract and is never mislabeled as portable receipt
evidence.

### Cost model

A signature alone normally slows the first load: the large content digest must
be checked and the small manifest/receipt signature verified, while structural
and schema validation are still needed. Content hashing, CBOR admission, and
index construction should share one bounded sequential read, so the file is not
read repeatedly.

The fast path helps when one or more of these are true:

- a signed package loader already holds immutable content-identity evidence;
- the same content is opened repeatedly under the same profile and schema;
- a signed bounded index avoids rebuilding the structural walk; or
- one admitted `Cborˉdocument` is passed among components in the same process.

If a mutable file must be rehashed on every open, authentication remains useful
for security but may not improve latency. Cache entries are keyed by content
digest, profile, schema, validator contract, trust generation, and index digest,
and are bounded by entries, bytes, lifetime, and verification work. Revocation
or a changed trust/validator contract invalidates the associated evidence.

Authenticated transport is not prevalidation. TLS authenticates a peer and
protects transport bytes, but that peer may still send malformed, oversized,
obsolete, or semantically invalid CBOR. Mutable WVDB pages, logs, and backups
also retain their own structural, checksum/authentication, recovery, and schema
checks; they are not permanently marked trusted by this profile.

## Performance and memory contract

The reference implementation should support four observation strategies:

1. a simple one-pass cursor oracle with no retained index;
2. validate-once immutable document views with a bounded offset index;
3. receipt-backed prevalidated views over authenticated immutable content; and
4. schema-directed generated accessors after a schema profile is accepted.

Qualification records at least:

- bytes per scalar/container/value and total encoded size;
- admission throughput and peak/retained memory;
- repeated field lookup with and without an index;
- skip performance for large unobserved subtrees;
- deterministic map encoding/sorting cost;
- first-load signature, receipt, schema, and index cost versus ordinary
  admission;
- repeated authenticated opens with and without reusable immutable-content
  evidence;
- JSON-to-CBOR and CBOR-to-JSON conversion cost;
- sequence chunk-boundary behavior; and
- adversarial nesting, lengths, duplicate keys, and map ordering.

No zero-copy claim is made when input must be copied for ownership, alignment,
provider lifetime, or isolation. “No materialization” and “zero allocation” are
separate claims and must be measured separately.

## Conformance and malformed input

The first conformance suite includes:

- every RFC 8949 major type and additional-information width;
- shortest, longer valid, and forbidden/reserved encodings;
- definite and indefinite forms under each selected profile;
- valid and invalid UTF-8;
- empty, boundary, nested, truncated, and trailing input;
- maximum `u64` unsigned and negative arguments;
- binary16/32/64 finite, zero, subnormal, infinity, and NaN cases;
- duplicate and equivalent map keys;
- deterministic and out-of-order maps;
- tags with valid, invalid, unsupported, and recursively embedded content;
- exact allocation, item, depth, string, map, index, work, and output limits;
- incremental sequence boundaries at every byte position;
- CDDL valid, invalid, recursive, oversized, and work-exhaustion schemas;
- COSE Sign1 attached/detached bounds, protected-algorithm and Ed25519 key
  matching, external-domain separation, altered signatures, and forbidden
  security decisions in unprotected headers;
- altered content, receipt, signature, schema identity, trust generation,
  validator role, observed limit, and structural index;
- immutable-snapshot replacement races, stale receipts, cache eviction, and
  evidence revocation; and
- differential vectors from the RFC examples plus an independent
  implementation where licensing/provenance permits.

Deterministic encoder tests compare exact bytes. Structural assertions explain
the data model so a golden-byte failure remains diagnosable.

## Deliberately deferred typed layout format

CBOR is not selected as the only future binary representation. After the first
CBOR/JSON/WVDB service is measurable, Windvale should evaluate a schema-bound
read-optimized profile for workloads that repeatedly observe a small subset of
large immutable records.

The evaluation should compare at least Protocol Buffers, FlatBuffers, and Cap'n
Proto concepts against a Windvale-owned typed record descriptor. It should
measure validation cost, generated-code size, random field access, alignment,
endianness, canonical bytes, schema evolution, unknown fields, bounds,
ownership, WebAssembly access, native access, and Windvale OS suitability.

That later decision must not overload `Dataˉcbor` or claim that a schema-driven
buffer is self-describing. Conversely, the CBOR implementation should not grow
an ad hoc in-memory pointer layout merely to imitate a zero-copy format.

## Review questions

Owner review should confirm or revise:

1. CBOR RFC 8949 as the general binary-document standard;
2. definite-length containers in Windvale document profile 1;
3. core deterministic encoding rather than the older length-first map-key
   ordering;
4. rejection of every float in the base document profile and the separately
   named finite-floating document/deterministic profiles;
5. text and unsigned-integer keys as the recommended protocol map keys;
6. selected typed tags for Decimal128, time, URI, and embedded CBOR;
7. CDDL as optional build/tooling support rather than a mandatory runtime
   parser;
8. CBOR Sequences as the streaming format;
9. the validate-once view/index ownership model;
10. bounded COSE Sign1 EdDSA/Ed25519 manifests and receipts plus distinct
    publisher and validator authority;
11. exact JSON/CBOR number conversion, including whether the generic converter
    should remain limited to the JSON-shaped subset;
12. exact default and hard resource ceilings; and
13. the workload and threshold that would justify a second schema-bound binary
    layout format.
