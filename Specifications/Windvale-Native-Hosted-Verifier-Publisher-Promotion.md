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
or the 254,917-byte Linux publisher at SHA-256
`babe721a573e29f89ec095c35677880077ff465d4e2129063f6742cd47591a97`.

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
| Promoter WVB | 41,268 | `c0c7c88996ef837bc5a2ec3ceb1de61254b025fbd6504e4f3d7dc055c4140672` |
| Promoter WVO | 660,123 | `ba5d9c5afde115fede472369d24c3d1fe466806de523773d2e445e6a9e004667` |
| Linked flat fragment | 658,339 | `e06189a37c038a5237787ffd16fb53466df3d10519efd4129b219bd814f4def2` |

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
| Windows x64 | 681,472 | `598bd2de8247abd19d931efa1edcc8323adef7f56da51da1d41256933667eb23` |
| Linux x64 | 680,901 | `422332fb4f2824ae558bf93adadb6470597399d07810f5428f71aa4d971a4f58` |

The current-host focused native lane constructs both applications, uses the
promoter to install both exact publisher subjects, and then uses the installed
current-host publisher to install an exact hosted verifier. The rejection lane
also proves corrupt-candidate preservation and zero scratch. Independent Linux
execution, grouped qualification, promotion, and release integration remain.
