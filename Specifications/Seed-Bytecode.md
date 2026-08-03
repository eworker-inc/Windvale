# Windvale Seed bytecode specification

## Status

This document specifies the Windvale bytecode 1.6 base and its 1.7 64-bit scalar extension. Windvale is in early development; these versions identify binary vocabularies and are not yet a long-term compatibility promise. Version 1.6 adds the exact SHA-256 identity operation required by the accepted deterministic linker-map contract. Version 1.7 adds checked `i64` and `u64` stack values, operations, and formatting without changing the section grammar. The canonical writer emits 1.6 unless a module uses the 1.7 vocabulary. Readers do not accept versions 1.0 through 1.5.

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
u16      minor version: 6 or 7, selected by the module vocabulary
u32      section count: 7
```

A 1.6 module must not contain value types `9` or `10` or opcodes `80` through `96`. The verifier rejects such a mismatched header rather than interpreting the extension under the older contract.

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
```

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
  u8     nominal kind: 1 record, 2 enum
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
```

Nominal types are grouped by kind, then strictly sorted by ordinal name, and names are unique across all kinds. Record field order is declaration order and therefore constructor order; field names are unique within the record. Seed requires between 1 and 64 fields. Fields may use `i32`, `i64`, `bool`, `text`, `u8`, `u32`, `u64`, `bytes`, or a nominal enum, but not `void` or another record. Enums contain 1 through 256 uniquely named members with unique `i32` values; member order is declaration order.

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
9 i64 (WVB 1.7)
10 u64 (WVB 1.7)
```

`void` is valid only as a return type. Immutable integer arrays are module data and are not operand-stack values. A `bytes` value is an immutable sequence or slice view and can be stored in locals, passed to functions, and returned.

Function parameter, result, local, and record-field types use a value shape. A primitive shape is its one-byte value type. A nominal shape is byte `7` for a record or byte `8` for an enum followed by a `u32` index into the Types section. Nominal identity is exact: separately declared records or enums remain different operand-stack types even when their contents match.

`i64` and `u64` are ordinary scalar shapes only in WVB 1.7. They do not widen counts, indices, lengths, code offsets, enum backing values, or existing binary Foundation operations, which remain explicitly `u32` or `i32`.

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

80 i64.const          i64 little-endian value (WVB 1.7)
81 u64.const          u64 little-endian value (WVB 1.7)
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
- The minor version matches the vocabulary: WVB 1.6 contains no 64-bit scalar types or extension opcodes, while WVB 1.7 may contain both the base and extension vocabularies.
- Every function decodes completely into known instructions.
- Branch targets identify instruction boundaries in the same function.
- Every local, data, function, and capability index is valid and has the required type.
- Every record or enum declaration, nominal shape, constructor operand, field access, enum constant, and enum comparison has valid nominal identity and exact types.
- Every byte-data declaration is bounded and every byte intrinsic receives exactly the required operand types.
- Strict UTF-8 decoding and encoding, safe quoting, signed little-endian reads, fixed-width byte construction, byte concatenation, SHA-256 identity, and explicit `u8` to `u32` conversion receive and produce their exact declared types.
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
- Data declarations: 4,096
- Functions: 4,096
- Nominal types: 1,024
- Fields per record: 64
- Members per enum: 256
- Parameters plus locals per function: 8,192
- Code per function: 1 MiB
- Instructions per function: 100,000
- Operand stack: 4,096 values
