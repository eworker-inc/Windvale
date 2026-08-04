# Windvale Seed bytecode specification

## Status

This document specifies the sole current Windvale bytecode format, WVB 1.11. Windvale is in early development and does not preserve obsolete experimental WVB encodings unless a named compatibility case is approved. WVB 1.11 includes 64-bit scalars, independent module metadata, nominal payload variants, bounded sequences and affine builders, division and remainder, fixed-width unsigned bitwise/shift operations, exact text/bytes equality, and exact little-endian `u64` byte codecs. Every canonical writer emits 1.11 and every general WVB reader or verifier rejects any other version.

## Encoding

- All integers are little-endian.
- All text is strict UTF-8 without a byte-order mark.
- Lengths, counts, indices, and code offsets use unsigned 32-bit integers.
- A string is a `u32` byte length followed by that many UTF-8 bytes.
- Decoders use checked arithmetic and reject trailing or missing payload bytes.
- Module, data, function, and export names use the Seed source-identifier grammar. These UTF-8 metadata names are not native ABI symbols.
- Capability names use their separately specified qualified lowercase ASCII grammar.

## File header

```text
4 bytes  magic: 57 56 42 31 (ASCII WVB1)
u16      major version: 1
u16      minor version: 11
u32      section count: 7
```

The version identifies one complete vocabulary. Feature-dependent version selection and lowest-required-version calculation are not part of the current contract.

Every section has this envelope:

```text
u8       section kind
u8       flags, must be zero
u16      reserved, must be zero
u32      payload byte length
bytes    payload
```

The seven mandatory sections occur exactly once in this order:

1. Module
2. Capabilities
3. Data
4. Functions
5. Code
6. Exports
7. Types

## Module section

```text
u8       profile: 1 portable, 2 hosted, 3 system
string   module name
u8       metadata present: 0 or 1
if present:
  <metadata fields below>
metadata fields:
  u8       metadata encoding version: 1
  u8       authority: 1 library, 2 application, 3 service, 4 system
  u32      platform-scope count
  string[] strictly sorted platform scopes
  u32      required-capability count
  repeat:
    string capability identity
    u32    major version
  u32      optional-capability count
  repeat:
    string capability identity
    u32    major version
```

The presence byte is mandatory even when metadata is absent. Platform scopes are unique, strictly sorted lowercase ASCII identities with optional dot-separated segments. At least one scope is required when metadata is present. Required and optional entries are independently unique and sorted. An identity cannot be both required and optional. Seed currently admits catalog identities at major version 1. Required metadata identities exactly equal the executable Capabilities section; optional identities are admission and provider-selection metadata only. System authority and the retained system profile must agree.

## Capabilities section

```text
u32      capability count
repeat:
  string capability name
  u32    parameter count
  u8[]   parameter value types
  u8     return type
```

Entries are strictly sorted by ordinal capability name and cannot be duplicated. Portable modules require zero capabilities.

The current canonical hosted signatures are defined by [Hosted-Resources.md](Hosted-Resources.md). Extending the recognized catalog does not change this encoding or the WVB version when all signatures use existing value types.

## Data section

```text
u32      data count
repeat:
  string data name
  u8     data type: 3 text, 4 immutable i32 array, 5 immutable bytes
  if text:
    string UTF-8 value
  if i32 array:
    u32  element count
    i32[] elements
  if bytes:
    u32  byte count
    bytes value
```

Entries are strictly sorted by ordinal data name and cannot be duplicated.

## Functions section

```text
u32      function count
repeat:
  string function name
  u32    parameter count
  shape[] parameter types
  shape   return type
  u32    non-parameter local count
  shape[] local types
  u32    code offset within the Code section
  u32    code byte length
  u32    declared maximum operand-stack depth
```

Entries are strictly sorted by ordinal function name and cannot be duplicated. Function code ranges must be contiguous, ordered, non-overlapping, and cover the entire Code section.

## Exports section

```text
u32      export count
repeat:
  string export name
  u8     export kind: 1 function
  u32    function index
```

Exports are strictly sorted by ordinal name. An exported name must equal the referenced function's Seed name.

The reference launcher selects exported `Main() -> i32` as the executable source entry point. Future native object formats must define an ASCII-safe external symbol mapping separately.

## Types section

```text
u32      nominal type count
repeat:
  u8     nominal kind: 1 record, 2 enum, 3 variant
  string nominal type name
  if record:
    u32    field count
    repeat:
      string field name
      shape field type
  if enum:
    u32    member count
    repeat:
      string member name
      i32  member value
  if variant:
    u32    case count
    repeat:
      string case name
      u8     payload present: 0 or 1
      if present:
        string payload name
        shape  payload type
```

Nominal types are grouped by kind, then strictly sorted by ordinal name, and names are unique across all kinds. Record field order is declaration order and therefore constructor order; field names are unique within the record. Seed requires between 1 and 64 fields. Enums contain 1 through 256 uniquely named members with unique `i32` values. Variants contain 1 through 256 unique ordered cases and at most one payload per case. Field and payload shapes obey the bounded, acyclic source restrictions.

## Value types

```text
0 void
1 i32
2 bool
3 text
4 u8
5 u32
6 bytes
7 record
8 enum
9 i64
10 u64
11 variant followed by u32 nominal-type index
12 sequence followed by element shape and u32 maximum
13 builder followed by element shape and u32 maximum
```

`void` is valid only as a return type. Immutable integer arrays are module data and are not operand-stack values. A `bytes` value is an immutable sequence or slice view and can be stored in locals, passed to functions, and returned.

Function parameter, result, local, record-field, and variant-payload types use a value shape. A primitive shape is its one-byte value type. A nominal shape is byte `7`, `8`, or `11` followed by a `u32` Types-section index. A collection shape is byte `12` or `13`, its recursively encoded non-collection element shape, then its `u32` maximum. Nominal identity and collection kind/element/maximum are exact.

`i64` and `u64` are ordinary scalar shapes. They do not widen counts, indices, lengths, code offsets, enum backing values, or existing binary Foundation operations, which remain explicitly `u32` or `i32`.

## Instruction encoding

```text
01 i32.const       i32 value
02 bool.const      u8 value (0 or 1)
03 text.const      u32 text-data index
04 local.load      u32 local index
05 local.store     u32 local index
06 data.length     u32 i32-array data index
07 data.load.i32   u32 i32-array data index; consumes i32 index
08 u8.const        u8 value
09 u32.const       u32 value
0A bytes.const     u32 byte-data index
0B bytes.length
0C bytes.slice     consumes bytes, u32 offset, u32 length
0D bytes.read_u8   consumes bytes, u32 offset
0E bytes.read_u16_little consumes bytes, u32 offset
0F bytes.read_u32_little consumes bytes, u32 offset

10 i32.add
11 i32.subtract
12 i32.multiply
13 i32.negate
14 u32.add
15 u32.subtract
16 u32.multiply

20 i32.equal
21 i32.not_equal
22 i32.less
23 i32.less_equal
24 i32.greater
25 i32.greater_equal
26 bool.equal
27 bool.not_equal
28 bool.not

60 u32.equal
61 u32.not_equal
62 u32.less
63 u32.less_equal
64 u32.greater
65 u32.greater_equal
66 u8.equal
67 u8.not_equal
68 record.create     u32 record-type index; consumes fields in declaration order
69 record.field      u32 field index; consumes one nominal record value
6A enum.const        u32 enum-type index, u32 member index
6B enum.equal        consumes two values of the same nominal enum
6C enum.not_equal    consumes two values of the same nominal enum
6D enum.name         consumes enum, produces its declared member name as text
6E i32.format        consumes i32, produces invariant decimal text
6F u8.format         consumes u8, produces invariant decimal text
70 u32.format        consumes u32, produces invariant decimal text
71 text.concat       consumes two text values, produces bounded concatenation
72 bytes.read_i32_little consumes bytes and u32 offset, produces signed i32
73 text.utf8_is_valid consumes bytes, produces bool without trapping on invalid UTF-8
74 text.from_utf8     consumes bytes, produces text or traps on invalid UTF-8
75 text.quote         consumes text, produces bounded ASCII JSON-style quoted text
76 u32.from_u8        consumes u8, produces the same value as u32
77 bytes.concat       consumes two bytes values, produces bounded immutable concatenation
78 bytes.from_u8      consumes u8, produces one byte
79 bytes.from_u16_little consumes u32 in the range 0..65535, produces two bytes
7A bytes.from_u32_little consumes u32, produces four bytes
7B bytes.from_i32_little consumes i32, produces four two's-complement bytes
7C text.to_utf8       consumes text, produces its strict UTF-8 bytes
7D bytes.sha256_hex   consumes bytes, produces 64 lowercase ASCII hex characters

80 i64.const          i64 little-endian value
81 u64.const          u64 little-endian value
82 i64.add
83 i64.subtract
84 i64.multiply
85 i64.negate
86 u64.add
87 u64.subtract
88 u64.multiply
89 i64.equal
8A i64.not_equal
8B i64.less
8C i64.less_equal
8D i64.greater
8E i64.greater_equal
8F u64.equal
90 u64.not_equal
91 u64.less
92 u64.less_equal
93 u64.greater
94 u64.greater_equal
95 i64.format          consumes i64, produces invariant decimal text
96 u64.format          consumes u64, produces invariant decimal text

97 variant.create      u32 variant-type index, u32 case index
98 variant.is_case     u32 variant-type index, u32 case index; consumes variant, produces bool
99 variant.payload     u32 variant-type index, u32 case index; consumes variant, produces payload
9A builder.create      u32 element-shape descriptor, u32 maximum
9B builder.push        consumes builder and exact element, produces replacement builder
9C builder.freeze      consumes builder, produces immutable sequence
9D sequence.length     consumes sequence, produces u32
9E sequence.element    consumes sequence and u32 index, produces exact element
9F i32.divide
A0 i32.remainder
A1 u32.divide
A2 u32.remainder
A3 i64.divide
A4 i64.remainder
A5 u64.divide
A6 u64.remainder
A7 u8.bitwise_and
A8 u8.bitwise_or
A9 u8.bitwise_xor
AA u8.bitwise_not
AB u8.shift_left       consumes u8 and u32
AC u8.shift_right      consumes u8 and u32
AD u32.bitwise_and
AE u32.bitwise_or
AF u32.bitwise_xor
B0 u32.bitwise_not
B1 u32.shift_left      consumes u32 value and u32 count
B2 u32.shift_right     consumes u32 value and u32 count
B3 u64.bitwise_and
B4 u64.bitwise_or
B5 u64.bitwise_xor
B6 u64.bitwise_not
B7 u64.shift_left      consumes u64 and u32
B8 u64.shift_right     consumes u64 and u32
B9 text.equal
BA text.not_equal
BB bytes.equal
BC bytes.not_equal
BD bytes.read_u64_little consumes bytes and u32 offset, produces u64
BE bytes.from_u64_little consumes u64, produces eight bytes

30 jump            u32 absolute byte offset in the function
31 branch.false    u32 absolute byte offset; consumes bool

40 call            u32 function index
41 call.capability u32 capability index

50 pop
51 return
```

## Verification

Verification is required before execution and rejects a module unless:

- The header, sections, strings, counts, types, and code ranges are structurally valid and within implementation limits.
- The version is exactly WVB 1.11 and the Module metadata presence byte is encoded exactly as specified above.
- Platform scopes, authority, required capabilities, optional capabilities, and capability major versions satisfy the independent module-metadata rules.
- Every function decodes completely into known instructions.
- Branch targets identify instruction boundaries in the same function.
- Every local, data, function, and capability index is valid and has the required type.
- Every record, enum, or variant declaration, nominal shape, constructor operand, field/payload access, case test, constant, and enum comparison has valid nominal identity and exact types.
- Every collection shape has an admitted non-collection element and maximum; builder transitions and sequence operations have exact types and cannot cross forbidden boundaries.
- Division/remainder, bitwise/shift, and content equality operations have exact operand types; shifts use a `u32` count and content equality is limited to text and bytes.
- Every byte-data declaration is bounded and every byte intrinsic receives exactly the required operand types.
- Strict UTF-8 decoding and encoding, safe quoting, signed and `u64` little-endian reads, fixed-width byte construction, byte concatenation, SHA-256 identity, and explicit `u8` to `u32` conversion receive and produce their exact declared types.
- Operand-stack types and depths agree at control-flow merges.
- Calls consume the declared parameter types and push only a non-void result.
- Returns match the function return type.
- Control cannot fall past the end of a function.
- Every instruction is reachable in Seed.
- Computed maximum stack depth equals the declared maximum.
- Capabilities and their signatures are recognized by the Seed capability catalog.

## Implementation limits

- Module bytes: 16 MiB
- Sections: exactly 7
- UTF-8 value: 1 MiB
- Byte-data value: 4 MiB
- Declaration name: 255 UTF-8 bytes
- Capabilities: 32
- Platform scopes: 32
- Required capability requirements: 32
- Optional capability requirements: 32
- Data declarations: 4,096
- Functions: 4,096
- Nominal types: 1,024
- Fields per record: 64
- Members per enum: 256
- Parameters plus locals per function: 8,192
- Code per function: 1 MiB
- Instructions per function: 100,000
- Operand stack: 4,096 values
