# Windvale database bootstrap

## Status and scope

`Windvaleˉdatabaseˉbootstrap` is the portable, capability-free planner for the
first durable database image. `Durableˉdatabaseˉbootstrap` binds that plan to
one pre-opened `storage.random_access_v1` object and executes it through the
existing bounded publication executor.

The contract creates only an empty generation-1 database. It does not select a
native path, create a host file, grant storage authority, open an engine
session, create collections, or migrate an existing format.

## Canonical initial image

The database identity is two `u64` fields and must not be all zero. Page size
is exactly one of 4,096, 8,192, 16,384, 32,768, or 65,536 bytes. The target
storage length is checked as `512 + page_size`.

The image has this exact layout:

| Position | Length | Contents |
| ---: | ---: | --- |
| 0 | 256 | canonical first-slot `WVDS 1` superblock |
| 256 | 256 | zero inactive slot |
| 512 | page size | canonical empty root `WVPG 1` page |

The root is page zero, kind `Root`, generation 1, sequence 0, no predecessor,
zero items, and an empty payload. The first superblock names the supplied
database identity, generation 1, sequence 0, root page zero, no commit-log
head, earliest retained sequence zero, one page, the exact target length, the
selected page size, and root depth one.

Plan construction encodes and decodes the durable records before returning a
valid plan. Equal identity and page-size inputs produce byte-identical root and
superblock bytes.

## Publication order

Fresh creation is admitted only when `Storageˉdescribe` reports a nonzero
provider generation and length zero. The existing publication state machine
then performs four ordered actions:

1. write the complete root at position 512;
2. flush content and length;
3. write the 256-byte first superblock at position zero; and
4. flush content.

The supported page sizes fit the 65,536-byte storage transfer bound, so the
root is one write action. A completed result must report the same provider
generation, exact progress, and exact expected length. Rejection is terminal;
partial, indeterminate, stale, changed, or malformed observations require
reopen and are never silently replayed in-process.

## Reopen admission

After restart, the hosted bootstrap reads existing bytes only when the observed
length equals the exact target length. It resumes only these byte-exact states:

- canonical root plus a zero 512-byte header: repeat the content-and-length
  flush, then publish and flush the first superblock; or
- canonical root plus the exact first superblock and zero inactive slot:
  repeat the final content flush.

Repeating either flush is idempotent and closes the only provable interruption
windows. Any other nonempty length, root, or header is `Not_empty` and causes no
mutation. In particular, a short root write or partial superblock write is not
repaired by guessing and is not truncated.

On success the result reports `Created`, the provider generation, target
length, and number of executed actions. A zero action budget may report
`Active`; uncertain provider completion reports `Reopen_required`. Callers
must open or reopen the database engine after creation rather than treating
the bootstrap result as a live engine session.

## Verification

The portable native fixture proves deterministic exact images, decoded root
and superblock fields, minimum and maximum supported page sizes, the complete
four-action publication, both resume points, and rejection of invalid identity,
page size, generation, lengths, root bytes, and header bytes. The hosted durable
storage fixture creates its real backing object exclusively through the hosted
bootstrap before exercising reopen, update, interruption, and recovery.

## Exclusions

This contract does not define ambient file creation, directory durability,
replacement, deletion, collection installation, schema migration, concurrent
creators, server identity policy, authentication, sessions, or networking.
