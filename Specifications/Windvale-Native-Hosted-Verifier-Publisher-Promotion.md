# Windvale native hosted-verifier publisher promotion

## Status and scope

This contract defines the distinct durable promoter that installs one exact
completed hosted-verifier publisher application. The portable promoter source,
canonical WVB, accepted native WVO, linked flat fragment, paired Windows/Linux
promoter applications, and digest-bound installation launchers now exist.

The promoter is not the publisher it installs. This separation is required:
embedding a publisher's own completed digest in that publisher would create a
self-digest cycle. The promoter's application identity belongs only in
its external candidate manifest and digest-bound launcher.

## Portable command

The executable command is:

```text
wvhostverifierpublisherinstall <candidate.exe|candidate.elf> <destination.exe|destination.elf>
```

The source requires the exact five capabilities already used by the durable
hosted-verifier application publisher: console output, diagnostic output, file
input, process argument, and process argument count. It accepts only matching
`.exe` or `.elf` candidate/destination pairs and rejects byte-identical path
arguments before admission.

The complete candidate is read once as a Windvale byte value and admitted by
`Nativeˉhostedˉverifierˉpublisherˉapplicationˉverification`. That function
accepts only the 256,000-byte Windows publisher at SHA-256
`17cb5c4228e8448693b17f1b73695fd0ecfd03d7ada922794a5bf3bd7594fc96`
or the 254,965-byte Linux publisher at SHA-256
`510f5ce5d2a494eacf0adc7a613581bc2371c4ad0f5f985f501381edc1632fac`.

After successful admission, the private
`Applicationˉpublicationˉpublisherˉbegin/apply` ABI exposes the existing
publication state machine to the native transaction adapter. The final
application must preserve one immutable candidate snapshot through sibling
creation, flush, exact reread, file-identity alias rejection, anchored atomic
replacement, and directory durability. A read-only admit process followed by a
host copy or rename is not conforming.

## Native source identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Promoter WVB | 41,268 | `086bd4d93d93d51b0f9140a0adf9f54a7f205dc902d9cb5d732dc7a887e10edc` |
| Promoter WVO | 660,123 | `ee5274c86d680640d3ab75754faf63585a639a44fc9626ea5b9f9bcce9779e8e` |
| Linked flat fragment | 658,339 | `d50dc45866818c36a0332af71e914dc9a05052d97f43c0f60add4a75101bbec6` |

The native linker places `Main` at address 1,178. The transaction apply/begin
entry points remain at 0/789. The WVO has 658,160 code bytes, 179 read-only-data
bytes, 49 symbols, three internal relocations, and no imports. Version 16 of the
publisher-construction candidate pins the WVB and WVO. Its focused inventory
rebuilds, lowers, links, and compares them without a C# process.

## Constructed application identities

The construction records use explicit role 0 for the original publisher and
role 1 for the promoter. The role is carried in reserved `WVPM`, `WVVP`,
`WVPS`, and `WVCR` fields. Exact identity admission infers the role from the
WVB/WVO pair; callers cannot relabel an arbitrary module. Role 0 retains its
record geometry and semantics; its candidate identities advance only when a
shared admitted input advances.

| Target | Bytes | SHA-256 |
| --- | ---: | --- |
| Windows x64 | 681,472 | `86c72f5485bd6eeba1bdb65841102d7f388a8714b8e07ca3d519250de2886d8b` |
| Linux x64 | 680,949 | `700f3df624611abad03cbd70811bad2ab015136ecdacc6dff9cdd97f5fc81395` |

The current-host focused native lane constructs both applications, uses the
promoter to install both exact publisher subjects, and then uses the installed
current-host publisher to install an exact hosted verifier. The rejection lane
also proves corrupt-candidate preservation and zero scratch. Independent Linux
execution, grouped qualification, promotion, and release integration remain.
