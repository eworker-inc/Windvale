# Windvale OS x86-64 generation-2 client-image emission

This contract source-owns fixture offsets 24,989 through 25,064 by reusing the
checked recyclable-client image/context constructor. The region is byte-for-byte
identical to generation 1's offsets 9,607 through 9,682.

The 76-byte constructor copies the exact 449,261-byte admitted interpreter into
private executable pages and initializes native context format 7 with the
189,137-instruction budget, depth 5, and private 1,024-byte text arena. Its
local relocation field 3 maps to process-object symbol 1 with addend -4; the
generation-2 absolute field is 24,992.

The payload SHA-256 remains
`54432a2880a44c20e9c9246eeab45a488a9f9aa7746d2eff9aaef0671faac633`.
The generation-2 self-test WVB is 13,762 bytes at
`8758de24cc2954212d55bedab76d3746cfb584313bd455e11f2a0461fba40b1e`;
Windows is 187,904 bytes at
`a78e65b9424e4a10dc65cbf0f5cf5268a3ec04b39bdac854023b9fe35fb46386`;
Linux is 192,624 bytes at
`fb2d1a9b64f06b8068602e05fe23d710c0e8e45ae1ff06803489c8f31bc6ad4e`.
The focused owner validates bounds rejection, relocation identity, four hashes,
both host images, and result 92. Combined ownership reaches byte 25,064 with
121 external relocation fields.

Endpoint rebinding, resources, remaining context state, ready publication, and
re-entry remain separate transactions.
