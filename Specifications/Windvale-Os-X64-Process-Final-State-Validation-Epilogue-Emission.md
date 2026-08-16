# Windvale OS x86-64 final-state validation epilogue emission

This contract source-owns the terminal fixture offsets 31,200 through 33,825.
The 2,626-byte epilogue validates final supervisor state, both endpoint/channel
lifecycle outcomes, retained resource and mapping identities, and the selected
generation-two thread and memory record. Only after all checks pass does it
restore supervisor state and return result 6.

All 164 internal branches resolve to the terminal target at offset 33,826. The
normalized payload has SHA-256
`6d7fa0c376583e5bba075b781b865b874cf123e9fb6021c102f5b8daa0cff591`.
The WVB is 7,636 bytes at
`9c1b7b9123c6d6e65ee1de415e4f62c3b3e21a5e69ee545e5df122514a843f92`;
the WVO is 33,014 bytes at
`edb997020073b124f85e4e8a39da8934f2544fe84dddd574759733912cd3bef9`;
the linked binary is 32,594 bytes at
`9a5d73d31676446966f531fc7888f8e6c376257850fd2d6d57fff93c80531041`;
Windows is 34,304 bytes at
`8e06129e570b04da298ca216599dbc997838b9f257afeee961a1397bbe88f7ac`;
Linux is 36,976 bytes at
`c9552badd8d0e6f603eae6f7e161168144b8a90baaa18416a222032c864eb12a`.
The focused owner validates every branch, state sentinels, four bounded hashes,
both host images, and result 105.

Combined source ownership now covers all 33,826 process-machine bytes with 569
internal or external relocation fields. This is complete source ownership of
the retained machine oracle, not live QEMU application-execution evidence.
