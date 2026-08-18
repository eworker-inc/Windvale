# Workload 9 rejected and boundary cases

## Package binding and input admission

| Case | Required result |
| --- | --- |
| missing manifest/lock/notice binding | reject package before source execution |
| duplicate declaration binding | reject package before publication |
| bytes bound to a text declaration or inverse | exact incompatible-type rejection |
| declared maximum below exact content length | reject before mapping/source execution |
| digest or exact length mismatch | reject before mapping/source execution |
| invalid UTF-8 in manifest or lock | reject text binding before source execution |
| notice payload duplicated physically | package nonduplication evidence fails |
| alias points at an undeclared content object | reject reference table |
| content object identity mismatches its digest | reject content table |
| one domain is charged twice for the notice payload | accounting conformance fails |
| a second domain receives uncharged content | accounting/authority conformance fails |
| manifest over 1,024 or lock over 2,048 bytes | `Inputˉlimit` before collection construction |
| malicious declared length exceeds maximum | `Maliciousˉdeclaredˉlength` before allocation/read |
| checked length plus enclosing offset overflows | reject before allocation/read |

## Text grammar

| Case | Required result |
| --- | --- |
| missing or wrong `WVPACK1` / `WVLOCK1` | `Invalidˉmagic` at line 1 |
| unknown manifest field or lock line kind | `Unknownˉfield` with line/value |
| missing package, version, deps marker, or value | exact `Missingˉfield` |
| duplicate manifest package/version | exact `Duplicateˉfield` |
| empty identity | `Invalidˉidentity` |
| uppercase, hyphen, dot, non-ASCII, or leading digit identity | `Invalidˉidentity` at scalar column |
| identity above 64 UTF-8 bytes | `Invalidˉidentity` before collection insertion |
| empty version, sign, prefix, underscore, locale digit, or suffix | exact numeric parse failure |
| u64 version overflow | exact `Overflow` numeric failure |
| trailing word or extra separator | `Unknownˉfield` |
| empty dependency around comma | `Invalidˉidentity` |
| duplicate dependency | collection `Duplicate`; original owner returned |
| CR-only or unadmitted whitespace | unknown/invalid token; no normalization in package data |

## Collections, ordering, graph, and output

| Case | Required result |
| --- | --- |
| zero or above-hard collection maximum | `Invalidˉlimit` before split |
| package count 65 | `Capacityˉexhausted(64)`; map unchanged for item 65 |
| dependency count 33 | `Capacityˉexhausted(32)`; set unchanged for item 33 |
| duplicate package key | `Duplicateˉpackage`; original key/value returned |
| comparison budget exhausted | `Comparisonˉlimit`; collection unchanged |
| two unequal identities compare Equal | protocol-law conformance rejects the implementation; no arbitrary winner |
| comparison is non-total/non-transitive | protocol-law qualification fails |
| unknown dependency | `Unknownˉdependency`; no topology output |
| self dependency | `Dependencyˉcycle` with one identity |
| two-node or longer cycle | `Dependencyˉcycle` with sorted remaining identities |
| cycle diagnostic capacity exhausted | bounded collection failure; no partial diagnostic publication |
| report maximum below 160 | exact `Outputˉlimit`; failing append is atomic |
| host map/insertion order changes bytes | deterministic conformance failure |
| serializer uses reflection or internal nodes | implementation conformance failure |
| map/set rank called at length | proved-precondition trap before access |
| mutation attempted through immutable map/set | compile-time rejection |
| freeze while a rank borrow is live | borrow-check rejection |

## Literal and generic compile-time cases

- wrong raw closing hash count, nine raw hashes, non-ASCII raw bytes, or an
  unterminated ordinary/multiline/raw literal is a bounded lexer diagnostic;
- Unicode escape in a byte literal is rejected;
- interpolation in raw or byte literals is rejected;
- map/set construction without inferable or explicit full type arguments is
  rejected locally;
- result-context-only generic inference is rejected;
- zero or multiple exact `Ordering<Packageˉidentity>` implementations is
  rejected; and
- importing another ordering implementation cannot change an already resolved
  collection call.

The bundle records 51 distinct rejection/boundary rows or bullets. Future tests
may split one row into several fixtures but cannot merge away the named outcome.
