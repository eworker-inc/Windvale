# Windvale OS x86-64 generation-2 init reply-publication resume emission

This contract source-owns fixture offsets 27,470 through 27,805. It derives the
existing checked 336-byte init reply-publication/resume constructor with two
explicit state changes: the returned init operation is 7 instead of 3 and the
retained client-generation state is 2 instead of 1. Channel-state clearing,
dispatcher entry, external page-table activation at symbol 17, GS/continuation
publication, zero completion, and the client `sysretq` remain unchanged.

The normalized payload keeps symbol field 233 and fourteen internal fields
explicit. Its SHA-256 is
`1bf543cae5f5e9696415ab7cda696fce0945c6c31132c2e531f9d431b8e4deaf`.
The WVB is 4,944 bytes at
`1aa2e1875648d7bbf0e6db9328719682990c59518a27d5fe8d55950d639cee05`;
the WVO is 42,432 bytes at
`87f854a0b1a54f1efde6c70dbe1281fc365cb099ed1791f17a6a02d4d5224c48`;
the linked binary is 41,656 bytes at
`a7cded95095d0abb8a8f35704941fa0bcb7ecb2c2af0198eceff6ced5a089bb0`;
Windows is 43,520 bytes at
`d99e034e63f3cdd9d4571d44684bfe7ef16da45caee09bd2c7a6a1bceca28b3d`;
Linux is 49,264 bytes at
`205324fcae6d30ae3f1f09a4971f452728487a4c37d6090cddb827b62db8f65b`.
The focused native owner validates both state adaptations, every field, four
hashes, both host images, and result 98.

Combined source ownership reaches byte 27,805 with 260 internal or external
relocation fields. The following client handler, later lifecycle, and live guest
evidence remain separate boundaries.
