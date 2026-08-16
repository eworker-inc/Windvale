# Windvale OS x86-64 generation-2 endpoint-rebind emission

This contract source-owns fixture offsets 25,065 through 25,512. It validates
both closed generation-1 endpoint records, their provider and channel identities,
generation fields, rights, close evidence, and zero transient state before
changing either client reference to generation 2.

The 448-byte normalized payload has 28 fail-closed branches to the shared
terminal target and no external call relocation. Its SHA-256 is
`0bfe36edc975de32420bf9a13e985f0f218138d523a8df615047562d116880bc`.
The WVB is 3,388 bytes at
`66c40838688bf09ec245b38b98196d13f62073119afee66b295e526aabc18d52`;
Windows is 23,552 bytes at
`edae8ea5a71adfa34652e80f3daa68c0e39ee0e5aff825b329239e58b2077374`;
Linux is 28,784 bytes at
`522929410889169de460c55c4f936027c1f2c508c9541cd4f845d439bba3a22d`.
The focused owner validates every branch field and target, four hashes, both
host images, and result 93. Combined ownership reaches byte 25,512 with 149
relocation fields.

This proves checked endpoint identity rebinding only. Memory/resource validation,
alias mapping, readiness publication, context completion, and re-entry remain.
