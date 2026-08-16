# Windvale OS x86-64 init-return and program-validation emission

This contract source-owns fixture offsets 13,448 through 13,786. It validates
the returning init thread and process state, reacquires the generation-one
client program resource, and rejects any mismatch in resource identity, kind,
size, generation, rights, publication state, process generation, or private
page-table linkage.

The 339-byte normalized payload carries twenty-one internal conditional-branch
fields. Every rejection reaches the common fail-closed process-machine boundary
at byte 33,826. Its SHA-256 is
`18cab9dafda9e6619822969c036d304b5cfc025aeab91be77b86379071ee1d74`.
The WVB is 3,198 bytes at
`6c2bf662aa5156f525b21a011753174816c63526db82894032cec825cca0155f`;
Windows is 23,552 bytes at
`e72ef5b51f5b2adc6e3b26cd1983953ad96f28a4eeb9bc2490fe540106e8dff9`;
Linux is 28,784 bytes at
`d044ea33bb2a76f23ff8e621bd4ce5004afd53970d3b9cc07b96008afb566a93`.
The focused owner validates every field and target, both host images, and result
77. Combined ownership reaches byte 13,786 with 100 external relocation fields.

This is fail-closed init-return and one-resource validation evidence. It does
not yet validate the budget, store, or directory resources, transfer the client
user context, implement general handlers, or prove live application execution.
