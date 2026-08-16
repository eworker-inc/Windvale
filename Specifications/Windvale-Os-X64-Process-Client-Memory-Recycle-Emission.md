# Windvale OS x86-64 client-memory recycle emission

This contract source-owns fixture offsets 19,526 through 19,741. It validates
the selected generation-1 client record, releases its exact memory object,
checks the restored allocator cursor and free-page count, allocates the
generation-2 object, and requires the same physical root before reconstruction.

The release call uses memory-object import symbol 15 with client reference
`0x00010002`. Its returned root must equal the exiting client's retained root,
and the memory state must restore cursor 13 with 122 free pages. The allocation
call uses import symbol 13 with reference `0x00020002` and exactly 122 pages.
The result must be nonzero, 4 KiB aligned, remain within the 1 GiB identity
window, occupy one 2 MiB identity window, and equal the released root.

The 216-byte normalized payload has ten explicit fail-closed branches to the
shared terminal target and two external call relocations. Its SHA-256 is
`831ec3d8cf08158457764eea0980ab9e0a431b27c6f1fd46c863ec86c3bbf51d`.
The WVB is 4,205 bytes at
`6d43607fde70e4debb388d504d5197f5810377958917ca49c10d31bf3988907d`;
Windows is 36,352 bytes at
`bd784204bcb993dd642d1122038af4add3efaf0b68dd695c57cf5be5b7bc402c`;
Linux is 41,072 bytes at
`5215b6db9ae314e336946db9af0be94a1b83be919a6adde2404e064053cdb315`.
The focused owner validates every branch and relocation field, both host images,
and result 89. Combined ownership reaches byte 19,741 with 120 external
relocation fields.

This proves checked release, zeroing through the imported memory-object
contract, and same-root generation-safe allocation. It does not yet reconstruct
or publish generation-2 process, paging, resource, endpoint, or context state.
