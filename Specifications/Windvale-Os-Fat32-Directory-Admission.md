# Windvale OS FAT32 directory admission 1

## Status and scope

Directory admission 1 is the first implemented read-only FAT32 directory-data
boundary. It consumes exact 32-byte entries obtained through an already
admitted cluster chain and block grant. It does not perform device I/O or expose
raw FAT names directly as the shared Windvale filesystem path contract.

[`Fat32-Directory-Admission.wv`](../Operating-System/Services/Fat32-Directory-Admission.wv)
looks up one canonical 11-byte short name. The base and extension fields use
uppercase ASCII letters, digits, hyphen, underscore, and trailing space
padding. This deliberately narrow internal form avoids claiming long-file-name
or Unicode normalization semantics before those are specified.

## Entry rules

Input contains one through 4,096 whole directory entries. The scanner stops at
the first zero marker, skips deleted entries and exact long-name records, and
ignores valid volume labels. Reserved attribute bits, directory-plus-volume
labels, nonzero label cluster/size, reserved high bits or reserved/bad cluster
values, nonempty files without a data cluster, and directories without a
cluster or with a nonzero size are rejected.

The result distinguishes file, directory, missing, invalid input or target,
budget exhaustion, invalid entry, duplicate target, and a trace that ends
before an end marker on an incomplete cluster chain. A complete chain may end
without a zero marker. The scanner continues after a match to reject a second
identical short-name entry rather than selecting one ambiguously.

## Evidence and limits

The directory module builds as a 6,340-byte WVB at SHA-256
`14548e1da399a95bb8c25be9c9224b4d524c729457992c2dc26ef153561b7733`.
Its 19-case test WVB is 12,922 bytes at SHA-256
`adfd7e04136874ab60157cf5a718565a8aa73f19cebe61a32d2c219dfb6c0dc7`,
returns 47 on Windows, and pins deterministic Windows/Linux images.

This slice does not assemble VFAT long-file-name slots, normalize Unicode,
interpret timestamps, read file clusters, compare mirrored directory data, or
translate shared Windvale path segments. Those require separate contracts.
