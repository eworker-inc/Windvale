# Windvale WVO object construction

## Status and scope

`Object-Model/Windvale/Wvo-Object-Construction.wv` is the portable, verified
construction boundary for small canonical WVO 1.0 objects whose sections,
symbols, and relocations are known by a focused producer. It complements the
assembler and compiler object writers; it is not a parser, linker, arbitrary
object editor, or replacement for either front end.

Decision 0802 makes the Object-Model area the durable owner of shared canonical
WVO construction. The current module remains the implemented small-object
profile. Replacing overlapping assembler/compiler record encoding, or adding a
planned/segmented compiler-scale profile, is pending and must not force a large
object into one ordinary `bytes` value merely to claim reuse.

The module exports four primitive operations:

| Operation | Result |
| --- | --- |
| `Encodeˉsection` | One canonical section record and its contents |
| `Encodeˉsymbol` | One canonical symbol record |
| `Encodeˉrelocation` | One canonical relocation record |
| `Constructˉobject` | One admitted WVO 1.0 envelope over the supplied records |

The primitive numeric tags are the WVO 1.0 wire values. Section kinds must be
1 through 4, symbol bindings 1 through 3, symbol kinds 1 through 2, and
relocation kinds 1 through 2. Invalid primitive tags return an empty byte value.

## Construction boundary

The caller owns record ordering, counts, indices, offsets, sizes, alignment,
names, relocation placeholders, and architecture-specific code bytes. The
constructor writes only canonical little-endian WVO 1.0 fields. It then admits
the complete candidate through the shared portable WVO verifier. A count,
extent, name, index, ordering, alignment, or relocation inconsistency therefore
returns an empty byte value rather than a partially trusted object.

A retained hosted entry point must additionally refuse an existing destination,
require a nonempty verified result of its exact expected identity, and publish
only after all recipe-specific checks succeed. New producers should reuse this
module only when a real object recipe exists; it is not justification for copying
a large managed object generator or widening WVA with raw-byte escape syntax.
