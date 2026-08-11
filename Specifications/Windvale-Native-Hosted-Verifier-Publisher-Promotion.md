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
`2b165f5029798a4d5467412b65cba0ddffb05dfc449144fd80161d6117784e12`
or the 254,965-byte Linux publisher at SHA-256
`8c9a1dbbb177041c61e4606696ce9ddf9225a98407a7d3af0a4338069a15979e`.

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
| Promoter WVB | 41,268 | `7ea1cda2842c4258f654ee17deb441c1b06a3fcedfc29f7382e9259b2f3800fe` |
| Promoter WVO | 660,123 | `9ee875a6668b1661087dc6a59384c2427e6ef6febb5c83a4ed936e56cd13b44f` |
| Linked flat fragment | 658,339 | `843094cf8ba3de92697568abab6788a276f0ea7bd193e65abfb5c7b56918fb43` |

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
| Windows x64 | 681,472 | `5690fb32c7fec85551e0c5cd58e4f56589a5ad4c09108b5dde86fa9fc7b3fb92` |
| Linux x64 | 680,949 | `3cd1c82807495e34445345b5e61b8c5911434c84d2a6f49a11b21fd2521423f5` |

The current-host focused native lane constructs both applications, uses the
promoter to install both exact publisher subjects, and then uses the installed
current-host publisher to install an exact hosted verifier. The rejection lane
also proves corrupt-candidate preservation and zero scratch. Independent Linux
execution, grouped qualification, promotion, and release integration remain.
