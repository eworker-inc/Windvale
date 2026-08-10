# Windvale native hosted-verifier publisher promotion

## Status and scope

This contract defines the distinct durable promoter that installs one exact
completed hosted-verifier publisher application. The portable promoter source,
canonical WVB, accepted native WVO, and linked flat fragment exist. Paired
Windows/Linux promoter applications and public installation launchers remain
future work.

The promoter is not the publisher it installs. This separation is required:
embedding a publisher's own completed digest in that publisher would create a
self-digest cycle. The promoter's future application identity belongs only in
its external candidate manifest and digest-bound launcher.

## Portable command

The future executable command is:

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
`735320b5ff33419d685925044add6f254bf402c0d49fc575c77f6110fac705f6`
or the 254,917-byte Linux publisher at SHA-256
`de4f06f6d837eb58457a31b4757c3410e389ecc3c11fd79daf229dbdeb23e02a`.

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
| Promoter WVB | 41,268 | `30eb1e8c93b01266592b322b9c5154b27782ea6c7cd2b6522a10781bf935bec9` |
| Promoter WVO | 660,123 | `6f20c95c4c09958dcc09ee35b8f7a3a0330d67f26446206be5bdd85cd8cb042d` |
| Linked flat fragment | 658,339 | `a7c0ef19de332e00dcae74c9ab8c25b16b1e1ca73169d4485c85575412a28ed8` |

The native linker places `Main` at address 1,178. The transaction apply/begin
entry points remain at 0/789. The WVO has 658,160 code bytes, 179 read-only-data
bytes, 49 symbols, three internal relocations, and no imports. Version 11 of the
publisher-construction candidate pins the WVB and WVO. Its focused inventory
rebuilds, lowers, links, and compares them without a C# process.

## Remaining construction boundary

The existing publisher container construction records hard-pin the original
publisher's smaller WVB/WVO geometry, private entry points, placements, and
completed PE/ELF identities. They must gain an explicit exact promoter role
while retaining the current publisher bytes unchanged. The promoter then needs
paired publisher-specialized applications, digest-bound installation launchers,
successful replacement evidence, corruption and alias preservation, zero
scratch, and independent Linux execution before durable promotion is complete.
