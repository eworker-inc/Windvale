# Workload 10 rejected and boundary cases

## Profile, declaration, target, and ABI (14)

| # | Case | Required result |
| ---: | --- | --- |
| 1 | unsafe declaration in Core | compile-time profile rejection |
| 2 | unsafe block in Core or Hosted | compile-time profile rejection |
| 3 | foreign call outside unsafe block | compile-time unsafe-context rejection |
| 4 | safe function implicitly calls unsafe function | exact call-site rejection |
| 5 | Core imports System adapter | profile import-direction rejection |
| 6 | target is Windows, Windvale, non-x86-64 Linux, or another ABI | unsupported target before artifact publication |
| 7 | unknown concrete platform key | target-registry rejection |
| 8 | unknown ABI-contract identity | ABI-registry rejection |
| 9 | ABI contract does not match target descriptor | mismatch before lowering/link |
| 10 | missing, duplicate, or wrong external symbol binding | build/link admission rejection |
| 11 | foreign signature passes bool/enum/record/text/bytes by value | foreign-signature type rejection |
| 12 | variadic, callback, or inferred host-C declaration | unsupported edition-1 foreign form |
| 13 | compiler substitutes its own host calling convention | deterministic ABI conformance failure |
| 14 | unsafe/System is treated as a capability grant | authority conformance failure |

## Scratch, address, pointer, lifetime, and alias (16)

| # | Case | Required result |
| ---: | --- | --- |
| 15 | zero scratch length | `Invalidˉlength` before allocation |
| 16 | scratch length above ABI/address/budget maximum | typed memory failure before allocation |
| 17 | alignment zero, non-power-of-two, or above ABI maximum | `Invalidˉalignment` |
| 18 | physical allocation failure | exact `Allocationˉfailure`; no scratch |
| 19 | unsupported ABI witness at construction | `Unsupportedˉabi` |
| 20 | `Start + Length` u64 overflow | `Addressˉoverflow`; no region/pointer |
| 21 | base plus start/end exceeds 64-bit address width | `Addressˉoverflow` |
| 22 | range exceeds scratch extent | `Outˉofˉrange`; scratch unchanged |
| 23 | start 1 with required alignment 8 | `Misaligned`; no pointer |
| 24 | second mutable region overlaps live region | borrow-check or `Aliasing` rejection |
| 25 | safe slice requested while write region is live | borrow-check rejection |
| 26 | pointer returned/stored beyond region lifetime | lifetime/escape compile rejection |
| 27 | foreign no-retain signature attempts retained capture | ABI/ownership rejection |
| 28 | pointer converted from/to integer or serialized | type rejection |
| 29 | nullable ABI pointer is null | named `Null` validation result; no dereference |
| 30 | non-null foreign pointer lacks region/layout/lifetime proof | dereference rejection |

## Foreign result, generation, unwind, and containment (12)

| # | Case | Required result |
| ---: | --- | --- |
| 31 | return `-1` | `Foreignˉrejected`; scratch unobserved; no retry |
| 32 | return `-2` | `Foreignˉfailed(-2)`; scratch unobserved |
| 33 | return `-3` with observed generation 43 | exact stale expected 42 / observed 43 |
| 34 | stale status without admitted eight-byte prefix | isolated shim contract failure; no safe read |
| 35 | other negative return | `Foreignˉinvalidˉstatus` |
| 36 | returned length 65 for capacity 64 | `Foreignˉlength`; scratch unobserved |
| 37 | negative value implicitly converted to u64 | compile/type rejection; named exact conversion required |
| 38 | automatic retry after any negative/invalid result | behavior conformance failure |
| 39 | recoverable foreign condition tries to unwind | no-unwind ABI containment failure |
| 40 | foreign unwind crosses a safe frame | isolated terminal containment required |
| 41 | callee writes outside range or corrupts ABI state | isolated terminal containment; never ordinary `Result` |
| 42 | callee retains/uses pointer after return | isolated terminal containment; never safe publication |

## Returned record and safe publication (16)

| # | Case | Required result |
| ---: | --- | --- |
| 43 | returned length 0 through 19 | `Truncated` before any incomplete field read |
| 44 | returned length above configured record maximum | `Oversized` |
| 45 | wrong `WVFI` magic | `Invalidˉmagic` |
| 46 | version other than 1 | `Invalidˉversion` |
| 47 | kind 0, 3, or 255 | `Invalidˉkind`; no enum construction |
| 48 | Boolean byte 2 or 255 | `Invalidˉboolean`; no truthiness |
| 49 | reserved byte nonzero | `Invalidˉreserved` |
| 50 | record generation 41 or 43 after success status | `Generationˉmismatch` |
| 51 | declared payload exceeds configured maximum | `Oversized` before payload allocation |
| 52 | header plus declared length exceeds record maximum | `Payloadˉlengthˉoverflow` before addition/allocation |
| 53 | declared payload exceeds returned bytes | `Payloadˉrange` |
| 54 | returned bytes exceed exact header plus declared payload | `Trailingˉbytes` |
| 55 | payload allocation/builder limit failure | exact typed failure; no safe record |
| 56 | unsafe pointer/region/scratch placed in published record | type/profile rejection |
| 57 | report differs by host/ABI/pointer address | deterministic report conformance failure |
| 58 | report output maximum below 62 | exact output limit; no text publication |

These 58 cases are distinct mandatory outcomes. Deliberate memory corruption,
pointer retention, and forbidden unwind execute only in an isolated child
process with a bounded timeout and expected terminal result. They never execute
inside the verification coordinator.
