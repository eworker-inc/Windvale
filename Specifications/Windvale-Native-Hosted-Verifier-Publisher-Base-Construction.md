# Windvale native hosted-verifier publisher base construction

## Status and scope

This contract closes the final managed input in hosted-verifier publisher image
construction. Two focused hosted Windvale tools derive exact verifier metadata
and runtime headers from canonical six-service or explicit profile-6/profile-7
eleven-service `WVSQ 2` requests.
The ordinary construction command then connects those values to the existing
native bundle, startup, platform, container, publisher-record, object, import,
and materialization processes.

The command is an exact candidate constructor, not yet a durable publisher. It
refuses an existing destination, constructs privately, admits the final length
and digest, and copies a new result only after all stages succeed. Promotion
still requires a completed-publisher admission and durable replacement boundary.

## Focused commands

```text
wvhostverifierpublisherbasemetadata <target:1|2> <native-entry> <request.wvsq> <metadata.wvhv>
wvhostverifierpublisherbasemetadata wvo-inspector <target:1|2> <native-entry> <request.wvsq> <metadata.wvhv>
wvhostverifierpublisherbasemetadata console-verifier <target:1|2> <native-entry> <request.wvsq> <metadata.wvhv>
wvhostverifierpublisherbaseruntime <metadata.wvhv> <runtime.wvhr>
```

The metadata tool admits one exact `WVSQ 2` envelope with its embedded `WVPQ 1`
plan, hashes the linked fragment and six ordered service leaves, constructs the
private `WVVE` evidence, and invokes the existing `WVVE -> WVVR -> WVHV` cores.
It writes only the admitted raw 1,024-byte `WVHV` payload. The runtime tool
requires that exact payload length before any fixed read, forms the existing
1,048-byte runtime request, invokes the shared constructor, and writes only the
raw 4,096-byte `WVHR` result.

The explicit `wvo-inspector` and `console-verifier` forms require the 156-byte
eleven-service plan, hash the fragment and all eleven ordered leaves, and carry
profile 6 or profile 7 through the 572-byte evidence and 624-byte request. The
legacy form and `publisher-admission` selector retain their profile-2/profile-8
bytes.

Both commands reject an exact path-text input/output alias with status 64.
Malformed records return 2 without changing an existing destination. Filesystem
aliases remain excluded by the private orchestration boundary.

The paired ordinary host commands are:

```text
Tools\Native\Construct-Hosted-Verifier-Publisher.cmd <windows|linux> <output.exe|output.elf>
./Tools/Native/Construct-Hosted-Verifier-Publisher.sh <windows|linux> <output.exe|output.elf>
Tools\Native\Construct-Wvb-Publisher.cmd <windows|linux> <output.exe|output.elf>
./Tools/Native/Construct-Wvb-Publisher.sh <windows|linux> <output.exe|output.elf>
```

They verify the complete hosted-container and publisher-construction candidate
inventories, natively lower and link the canonical publisher WVB, require
`Main` at offset 3,001, construct the target base and publisher records, and
require the final exact application identity. They never invoke .NET.
The WVB-publisher wrapper selects role 2 while preserving the original two
roles and their exact bytes.

## Exact identities

The base-metadata and base-runtime rows are the retained packaged identities
from the construction-toolset refresh that consumes the profile-7-capable
sources.

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Base-metadata WVB | 78,245 | `a96d2bcae53793f95102a45278a229bf4615109e3df670aa44db02d8945e0703` |
| Base-runtime WVB | 22,465 | `39fc7a256118c41b360a1e0bcf1903798ecc8f1f4155cc787d59e6b92dd5e084` |
| Windows base-metadata application | 1,003,008 | `b23f04e029730edde01a8c3942d6b8771cfe65a7d8c56f6eac7f29c8371be1df` |
| Linux base-metadata application | 1,003,520 | `9ade312396293afa20ad5e409e10366c54b9c4616615ccb9cbcf0a4a4432101e` |
| Windows base-runtime application | 236,032 | `65e94ce354feae8121d8868a27308ba3ae1a993baf3313b8d847d1fb732102e2` |
| Linux base-runtime application | 237,568 | `db5fc8da164bb72da1c379b582c6bbffa64e8ecd1b6f6f4dd807a7e95f726055` |
| Publisher-application admission WVB | 30,778 | `73c6bfb23c277b6e0384a79bb00a9631709f3d4e9c727e7c27eb9e5dcbbd97f9` |
| Publisher-application admission WVO | 555,690 | `e348c41dcd96dbacedcc1820d42013e3c19795d89f7183ac7bc64311612dd927` |
| Publisher-promoter WVB | 41,268 | `7ea1cda2842c4258f654ee17deb441c1b06a3fcedfc29f7382e9259b2f3800fe` |
| Publisher-promoter WVO | 660,123 | `9ee875a6668b1661087dc6a59384c2427e6ef6febb5c83a4ed936e56cd13b44f` |
| Linked publisher-promoter fragment | 658,339 | `843094cf8ba3de92697568abab6788a276f0ea7bd193e65abfb5c7b56918fb43` |
| WVB publisher WVB | 163,300 | `9ebfe92eef070dfdcf18c4d176b5f32f64ad3f80751340b8a59ab2f1d567ec2a` |
| WVB publisher WVO | 1,349,361 | `43a594776b4e280575ac14e2866b4708961dd1290d643b41779a4933a8ba5991` |
| Linked WVB publisher fragment | 1,347,597 | `3d419d28b606408e7b2430cceacf4c0b7b109bcd511df4e98ca0d41b871f1c2d` |
| Linked publisher fragment | 232,736 | `260e9f4f23c99dab13145ceb98724a4c74157fc579c5685194b7312c1a5cb115` |
| Windows base | 248,832 | `579ff68d6645797a08c71a3ead03be6a56c2b4fd7eda8a3db548038eb9ccc007` |
| Linux base | 249,856 | `577bda8af2b1d8fca6f37e894c6b7f920e547f3e2b0bd1a28d2af518743a6629` |
| Windows publisher | 256,000 | `2b165f5029798a4d5467412b65cba0ddffb05dfc449144fd80161d6117784e12` |
| Linux publisher | 254,965 | `8c9a1dbbb177041c61e4606696ce9ddf9225a98407a7d3af0a4338069a15979e` |
| Windows promoter base | 674,816 | `818b1dcb4ad7145f2beee18c5e9afbb2e5aeab3bb56df905a5f07ae8eb3082ec` |
| Linux promoter base | 675,840 | `848ee9ed30ffc5094f77b4f79b72e3b4a426b4f9e0fc8e26631ed6619596f782` |
| Windows promoter | 681,472 | `5690fb32c7fec85551e0c5cd58e4f56589a5ad4c09108b5dde86fa9fc7b3fb92` |
| Linux promoter | 680,949 | `3cd1c82807495e34445345b5e61b8c5911434c84d2a6f49a11b21fd2521423f5` |
| Windows WVB-publisher base | 1,363,968 | `243b763d8b49b34108585c56f46c90190eac085a80c59873c8a2cb3e88d16102` |
| Linux WVB-publisher base | 1,363,968 | `2fc0332887c96ad0fa34d1987091d60ddbbe61f019739d41734cd491b8ca4b64` |
| Windows WVB publisher | 1,371,136 | `b9fd1b11bc1e4a726e4a43b16830a9351fe573b30e547ba8d8f6660f688ed421` |
| Linux WVB publisher | 1,369,077 | `b8efb90f7d7c4eae99de01df6c0a3c24a7396d9b9e717ff69d005282ed3d63af` |
| Windows WVO-publisher base | 422,912 | `22534a8a0ae42e977cd79daa3ff8b6fde5ef39d719edda07726410f95df6683d` |
| Linux WVO-publisher base | 421,888 | `af61a601f4cd8e7fb81704353160a518d2e4f199084fde4b29518d27c89774f7` |
| Windows WVO publisher | 430,080 | `76f632ffa7998a6cce0386456fee98f02cbb5ec424d0d914a7e1f06ff3853910` |
| Linux WVO publisher | 426,997 | `2889237d7fdb20b1d420c05834f19183d18b02112e3f4eea0ed7ff43414814f2` |

Version 20 of the construction candidate contains 27 canonical WVB/WVO
artifacts and 22 paired host applications. Its 49-entry `SHA256SUMS` is 5,064
LF-only bytes with SHA-256
`d9a41516b7d5f768afe377fd957e897bcb1cd3552fdf4c9510af3fc6969a7edc`.

## Native owner and remaining gate

The `hosted-verifier-publisher-files` retirement lane owns fifteen fixed cases:
the complete inventory, exact Windows and Linux publisher construction, exact
Windows and Linux profile-8 admitter construction, a current-host read-only
admission matrix, malformed base-record preservation, exact alias preservation,
and execution of the constructed current-host publisher to install the exact
verifier candidate. Its inventory case also rebuilds and natively lowers the
publisher-application admission WVB and requires byte equality with both pinned
artifacts. It also rebuilds and lowers the distinct publisher promoter, links
`Main` at 1,178, requires the exact 658,339-byte fragment, constructs both exact
promoter applications, installs both publisher subjects through the promoter,
and runs the installed current-host publisher through one verifier installation.
It additionally rebuilds and lowers the WVB publisher, constructs both exact
role-2 applications, and executes the current-host candidate on a canonical
portable WVB without loading .NET.

The separate `wvo-publisher-reconstruction` lane owns the role-3 candidate
inventory and exact native WVB plus paired-application reconstruction. It uses
the raw lowerer directly so the WVO publisher does not participate in
publishing its own construction object.

The immutable recovery release retains historical differential evidence outside the
normal path. Independent Linux execution, grouped qualification, candidate
promotion, and release integration remain open.
