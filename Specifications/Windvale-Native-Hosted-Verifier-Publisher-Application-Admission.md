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

The canonical command WVB is 30,325 bytes with SHA-256
`cdcda2e2bcdb7915a769ab9a79f7434e2b26bfbf4e0412a183bd7525769ef954`.
It builds through the native Project 1 front door and is pinned in version 8 of
the publisher-construction candidate.

## Remaining boundary

No existing hosted profile may be relabeled for this command. In particular,
profile 7 is already the hosted-container segmenter. The current accepted
native lowerer reports `Unsupportedˉcode` for this WVB, so paired executable
packaging requires a separately reviewed lowerer/profile extension.

Read-only admission also must not be followed by a host-side copy or rename:
that would admit one snapshot and mutate from another. Durable promotion needs
a distinct installer whose Windvale admission and atomic replacement consume
one immutable snapshot. The installer must remain separate from the publisher
it installs, and its own executable identity remains launcher-pinned.
