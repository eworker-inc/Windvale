# Windvale OS x86-64 timer activation emission

This contract source-owns fixture offsets 12,873 through 12,997. It selects the
directory-provider thread, validates its page table, clears and binds kernel GS
state, invokes the architecture timer-arm boundary, rolls back GS on rejection,
and transfers to the timer-resume boundary on success. It does not claim that a
hosted test executed privileged instructions or delivered an interrupt.

The 125-byte normalized payload keeps page-table activation (symbol 17), timer
arm (21), timer resume (25), and three internal relative fields explicit. Its
SHA-256 is `3ac2dc5ef8642caba8671c0ee689008be5c4d2626355746406ca86931a83bcf4`.
The WVB is 4,446 bytes at
`0b95cf7586b996922129d2199bec80051253c14e15f2c263c19a65c07547fc09`;
Windows is 47,616 bytes at
`c906ae32935fe03af5670398fb52e61284d13981b67635790ab6073edaaf725d`;
Linux is 53,360 bytes at
`35f3ece863faf0eb9d93c29b8d98dfc19f209637bb0c35f5519506e6c88c6e08`.
The focused owner validates all fields, symbols, targets, hashes, paired images,
and result 74. Combined ownership reaches byte 12,997 with 92 fields.
