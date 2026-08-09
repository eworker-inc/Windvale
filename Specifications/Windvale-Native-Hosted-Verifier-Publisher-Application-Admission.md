# Windvale native hosted-verifier publisher application admission

## Status and scope

This contract gives Windvale source exact read-only ownership of the two
completed hosted-verifier publisher application identities. It deliberately
lives outside the publisher being measured: embedding either final publisher
digest in that publisher would change the subject and create an endless
self-pin cycle.

This slice provides the portable admission module, its hosted command source,
and a natively built canonical WVB. It does not yet provide a paired hosted
executable or a durable installation transaction.

## Portable admission

`Nativeˉhostedˉverifierˉpublisherˉapplicationˉverification(Input, Target)`
accepts only these exact values:

| Target | Bytes | SHA-256 |
| --- | ---: | --- |
| `1`, Windows x64 | 256,000 | `735320b5ff33419d685925044add6f254bf402c0d49fc575c77f6110fac705f6` |
| `2`, Linux x64 | 254,917 | `de4f06f6d837eb58457a31b4757c3410e389ecc3c11fd79daf229dbdeb23e02a` |

Every other target, length, or digest returns false. SHA-256 is computed over
the complete immutable byte value and compared as eight exact little-endian
words. The admission result is not a structural PE/ELF claim; the pinned bytes
already carry the independently established construction evidence.

## Command source

The hosted source command is:

```text
wvhostverifierpublisheradmit <windows|linux> <publisher.exe|publisher.elf>
```

It declares only console output, diagnostic output, file input, and the two
process-argument capabilities. Success reports the admitted byte count and
returns zero. Invalid arguments return 64; a wrong target, length, or digest
reports one rejection diagnostic and returns 2. It never writes a file.

The canonical command WVB is 30,837 bytes with SHA-256
`f1e7497dc1acba1a08190021d4dac83ec65c3e6b58f80edb3bfcd62eeda55ed3`.
It builds through the native Project 1 front door. The accepted native ABI-22
lowerer produces a 556,273-byte WVO with SHA-256
`ac5972e8de83ad962874217ed6e0fba49586096df4c3b69d61abdf7509e2dff5`.
Both are pinned in version 9 of the publisher-construction candidate.

## Remaining boundary

No existing hosted profile may be relabeled for this command. In particular,
profile 7 is already the hosted-container segmenter. Paired executable
packaging therefore requires a separately reviewed hosted-profile extension;
the WVB-to-WVO boundary itself no longer blocks that work.

Read-only admission also must not be followed by a host-side copy or rename:
that would admit one snapshot and mutate from another. Durable promotion needs
a distinct installer whose Windvale admission and atomic replacement consume
one immutable snapshot. The installer must remain separate from the publisher
it installs, and its own executable identity remains launcher-pinned.
