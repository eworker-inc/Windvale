# Windvale project-WVB checkpoint

## Status

Implemented local-development contract. Project keys use version 2; the stored
WVB record remains version 1.

## Purpose and boundary

This contract owns the shared Project-WVB key and record and the standalone
Windows/Linux cache launcher. The [native tool checkpoint](Windvale-Native-Tool-Checkpoint.md)
defines the shared cache root, source-closure framing and OS batch consumer.
Reuse avoids repeated construction; it does not replace fresh behavior or
required independent reconstruction. The existing compiler-split development
owner verifies hit, miss, corruption, race, cleanup and host-launcher behavior.

## Project-WVB key and record

`Build-Cached-Project-Wvb` and the OS x64 development batch invoke the shared
`Native-Project-Cache-Key-Core.mjs` framing through namespace `project-wvb-v2`.
The standalone key command is a thin command-line adapter over that core.
After the format, namespace, and workspace, the ordered producers are:

1. exact native-front-door `SHA256SUMS` inventory; and
2. exact current-host native build-driver application.

The exact project identity and bytes plus declared root/source closure follow
that common producer prefix.

The build driver writes only into a fresh private checkpoint candidate
directory after its mandatory compiler-aligned verification succeeds. The
cache hashes that candidate and atomically moves the complete directory into
the shared family. This avoids routing a private compiler-scale cache candidate
through the general WVB publisher and read-only front door, whose independent
ordinary-module size envelopes are narrower than the compiler build.

The resulting lowercase 64-hex key names
`project-wvb-v2/<host-family>/<key>`. The WVB checkpoint record contains exactly
four ASCII lines:

```text
windvale-native-project-wvb-checkpoint 1
key <64-lowercase-hex>
wvb-bytes <canonical-positive-decimal>
wvb-sha256 <64-lowercase-hex>
```

The record is at most 1,024 bytes. The WVB is nonempty and at most 67,108,864
bytes. Every hit rejects links, recomputes the size and digest, compares the
complete expected record, and materializes a fresh byte-identical copy. The
keyed build driver and its mandatory verification are the admission boundary;
the cache does not reinterpret or execute the WVB.

The Windows and Linux `Build-Cached-Project-Wvb` launchers share one JavaScript
implementation. Existing host line endings, key framing, namespace and records
remain unchanged. The owner validates the selected entry and its ancestors,
without scanning unrelated cache entries. A miss rechecks the complete input
key after construction and before publication. A concurrent winner is accepted
only when its validated product is byte-identical to the local candidate.
The owner removes its own bounded `.new-` directory after a failed build or
lost race; uncertain process termination preserves that directory and reports
its path. It does not sweep other partial entries or evict completed evidence.
One ten-minute deadline bounds construction, with a cleanup reserve. Project 4
dispatches to the existing current split-project builder and preserves its
prepared-only environment policy and failure code.
