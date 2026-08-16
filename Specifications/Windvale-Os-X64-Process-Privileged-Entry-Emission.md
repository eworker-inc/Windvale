# Windvale OS x86-64 privileged-entry emission

This contract source-owns fixture offsets 11,442 through 12,082. It constructs
the kernel GDT and TSS, installs four explicit IDT gates, loads the descriptor
tables and task register, requires the x86-64 syscall feature, enables `SCE`,
and programs `STAR`, `LSTAR`, `FMASK`, and `GS_BASE` for the selected kernel and
user segments. Emitting these bytes in a portable test does not execute them or
claim live processor qualification.

The 641-byte normalized payload keeps four object relocation fields and three
internal relative fields zero. The external metadata binds exception handlers
to object symbols 20, 18, 19, and 22. Internal fields route two unsupported-CPU
checks to offset 33,826 and `LSTAR` to offset 34,128. The payload SHA-256 is
`6ac9279ab67e1a6c3fe408cec86730b778b96d0cb8e205bf89917966b635cb32`.

The WVB is 5,205 bytes at
`ea4cd3684fc0a0cc87957bbed1a57d4e8e83848182b48d113d6ebbe230c133a5`;
Windows is 52,736 bytes at
`0361f3a1d4be66ca32455a4fc3b103bbd0453380c8d26713cefc7cc37aadc901`;
Linux is 57,456 bytes at
`4b1a3da08c2a9cd0c21d56ff44f1288dc5e92010191fdd6c53c83536b81bc6ed`.
The focused owner validates all zero fields, external symbol identities,
internal target equations, four bounded hashes, paired images, and local result
72. Combined ownership reaches byte 12,082 with 86 relocation fields.
