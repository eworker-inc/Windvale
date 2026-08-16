# Windvale OS x86-64 thread and timer-state emission

This contract source-owns fixture offsets 12,083 through 12,872. It clears and
constructs three fixed `WVTHR001` thread records for init, directory-provider,
and recyclable-client execution, including generation, state, owner, budget,
kernel/user selectors, flags, saved stack, entry, and page-table values. It also
constructs the first `WVTIME01` timer record with bounded owner/generation state.
Nothing is scheduled, published, or armed by this emission alone.

The exact 790-byte payload SHA-256 is
`387d6b045d79ba4b4312dedba27acf1d642773b44308f3637b98e48d7c7bd286`.
The WVB is 2,526 bytes at
`5341e329f3df812aa7ea81cd8505c95ddc27e3531cbda6a65b6bb3fbf0235d70`;
Windows is 16,384 bytes at
`7327ad985bd44588276c526ba2aac21336df53d5484be3309f48a2deb7d3ddf7`;
Linux is 20,592 bytes at
`bc928927aa085143e3c021941edaa88e4017d801d2ee492e67a9b87d5aab87b3`.
The focused owner validates exact geometry, four bounded hashes, paired images,
and local result 73. Combined ownership reaches byte 12,872 with 86 relocation
fields.
