# Workload 10 safe record format

Format identity: `windvale.paper.foreign_record.v1`.

The complete record is exact-length little-endian bytes:

| Offset | Width | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVFI` |
| 4 | 1 | version | exactly 1 |
| 5 | 1 | kind | 1 `Snapshot`, 2 `Delta`; every other value invalid |
| 6 | 1 | enabled | exactly 0 false or 1 true |
| 7 | 1 | reserved | exactly zero |
| 8 | 8 | generation | little-endian u64; must equal expected generation |
| 16 | 4 | payload length | little-endian u32, widened explicitly |
| 20 | variable | payload | exact declared bytes; no trailing bytes |

Validation order is written-count minimum/maximum, magic, version, kind,
Boolean, reserved, generation, payload maximum, checked header-plus-length, and
exact remaining range. No record field is read until the complete enclosing
range for that field is known valid.

The valid fixture is 24 bytes:

~~~text
57564649010201002a0000000000000004000000deadbeef
~~~

Its SHA-256 is
`e4b8f0ff1f259afa82eaf1f004dd9eca3be0fce011e9ba4e94f3c946c7ca59a0`.
It means kind `Delta`, enabled true, generation 42, payload `de ad be ef`.

The safe result owns a new four-byte immutable payload. It contains no view into
foreign scratch and no ABI-specific type.
