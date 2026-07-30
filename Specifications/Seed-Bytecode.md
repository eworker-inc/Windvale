# Windvale Seed bytecode specification

## Status

This document specifies Windvale bytecode module version 1.1 used by Seed. Windvale is in early development; version 1.1 identifies the binary grammar and is not yet a long-term compatibility promise. Version 1.1 adds unsigned byte primitives and does not require a backward reader for version 1.0.

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
u16      minor version: 1
u32      section count: 6
```

Every section has this envelope:

```text
u8       section kind
u8       flags, must be zero
u16      reserved, must be zero
u32      payload byte length
bytes    payload
```

The six mandatory sections occur exactly once in this order:

1. Module
2. Capabilities
3. Data
4. Functions
5. Code
6. Exports

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
  u8[]   parameter types
  u8     return type
  u32    non-parameter local count
  u8[]   local types
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

## Value types

```text
0 void
1 i32
2 bool
3 text
4 u8
5 u32
6 bytes
```

`void` is valid only as a return type. Immutable integer arrays are module data and are not operand-stack values. A `bytes` value is an immutable sequence or slice view and can be stored in locals, passed to functions, and returned.

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
- Every function decodes completely into known instructions.
- Branch targets identify instruction boundaries in the same function.
- Every local, data, function, and capability index is valid and has the required type.
- Every byte-data declaration is bounded and every byte intrinsic receives exactly the required operand types.
- Operand-stack types and depths agree at control-flow merges.
- Calls consume the declared parameter types and push only a non-void result.
- Returns match the function return type.
- Control cannot fall past the end of a function.
- Every instruction is reachable in Seed.
- Computed maximum stack depth equals the declared maximum.
- Capabilities and their signatures are recognized by the Seed capability catalog.

## Implementation limits

- Module bytes: 16 MiB
- Sections: exactly 6
- UTF-8 value: 1 MiB
- Byte-data value: 4 MiB
- Declaration name: 255 UTF-8 bytes
- Capabilities: 32
- Data declarations: 4,096
- Functions: 4,096
- Parameters or locals per function: 4,096
- Code per function: 1 MiB
- Instructions per function: 100,000
- Operand stack: 4,096 values
