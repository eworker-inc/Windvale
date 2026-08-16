# Windvale OS FAT32 cluster-chain admission 1

## Status and scope

Cluster-chain admission 1 is the second implemented read-only FAT32 format
boundary. It keeps block-sector location separate from trace validation so an
isolated filesystem service can fetch exact sectors through a rights-limited
block capability while the kernel remains format-blind.

[`Fat32-Cluster-Chain.wv`](../Operating-System/Services/Fat32-Cluster-Chain.wv)
provides two pure operations:

- locate one cluster's four-byte entry in the selected FAT using admitted
  reserved-sector, FAT-count, active-FAT, FAT-size, and cluster-count values;
- admit an exact ordered trace of raw FAT entries for one cluster-chain read.

The trace is evidence supplied in traversal order, not an in-memory copy of the
whole FAT. A later provider loop owns sector reads and appends each raw entry to
the bounded trace before accepting the chain result.

## Entry and chain rules

Every raw entry is masked with `0x0FFFFFFF`; the high four reserved bits do not
affect the read result. Values are classified as follows:

- `0` is a free-cluster failure;
- `1` and `0x0FFFFFF0` through `0x0FFFFFF6` are reserved-value failures;
- `0x0FFFFFF7` is a bad-cluster failure;
- `0x0FFFFFF8` through `0x0FFFFFFF` complete the chain; and
- `2` through the smaller of the admitted last cluster and `0x0FFFFFEF` are
  possible next-cluster links.

The first cluster and every link must stay inside the admitted geometry. The
trace must contain whole four-byte entries, contain no entries after EOC, and
end at EOC. Repeated cluster identifiers fail as a cycle. A link without its
following raw entry is truncated rather than implicitly retried or completed.

Each operation declares a cluster ceiling from 1 through 4,096. A trace larger
than that ceiling, or a ceiling outside the implemented bound, fails before
traversal. Results report an exact status, the number of admitted clusters, and
the last cluster reached.

## Evidence and limits

The chain module builds as a 6,359-byte WVB at SHA-256
`75470d2a1c48c86754e2f91cd5919306fe73d76c567b87f7490fc87cc1eeeb1a`.
The shared 25,600-byte volume-and-chain test WVB at SHA-256
`c978805d2dec9acb9ba08e3fa9466d5f21aab013aff0f6d6c807666ac986bcd9`
lowers to deterministic Windows/Linux images. The 45-case owner covers volume
geometry, first and second FAT location, high-nibble masking, alternate EOC,
free, reserved, bad, out-of-range, cyclic, truncated, trailing, malformed, and
over-budget traces.

This slice does not perform block I/O, compare mirrored FAT copies, preserve a
trace across provider restart, parse directories, or read cluster data. Those
remain service-composition work rather than properties of this pure admission
boundary.
