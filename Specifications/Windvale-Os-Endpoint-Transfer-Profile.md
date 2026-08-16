# Windvale OS endpoint transfer profiles 1

## Status and scope

Endpoint transfer profiles 1 are the implemented architecture-neutral
admission policy for bounded service calls larger than the qualified Probe 40
control-message ceiling. They define the exact message geometry the future
x86-64 syscall adapter must prove before pinning, copying, waiting, replying, or
waking a caller. They do not change the qualified Probe 40 machine or its
4,096-byte maximum.

The kernel remains blind to paths, FAT geometry, and provider payload meaning.
It selects one profile from the bound endpoint and checks identity, generation,
user ranges, mapping rights, overlap, and byte/page limits.

## Profiles

| Profile | Purpose | Request bytes | Caller reply capacity | Provider reply bytes |
| ---: | --- | ---: | ---: | ---: |
| 1 | control | 1..4,096 | 1..4,096 | 1..4,096 |
| 2 | FAT32 block | exactly 48 | exactly 4,144 | 48..4,144 |
| 3 | filesystem | 64..65,600 | exactly 65,600 | 64..65,600 |

Profile 2 carries one exact `WVBR 1` request and reserves the full 48-byte
`WVBP 1` header plus 4,096-byte payload. Profile 3 carries one complete `WVFQ 1`
or `WVFP 1` envelope without silently truncating the shared 65,536-byte data
limit.

Each source or destination window must lie wholly inside a page-aligned user
envelope, use checked `u64` end arithmetic, and span no more than 17 pages. A
request source must be readable. A caller reply destination must be writable
and non-executable. The request and reply windows must not overlap. Maximum
windows that begin too late in a page and would span an eighteenth page are
rejected; callers can always use a page-aligned buffer for the full profile.

## Identity and lifecycle

An admitted call owns the nonzero endpoint reference, distinct caller and
provider references, both process generations, exact source/destination
windows, and selected profile. Reply admission requires the same endpoint,
peers, and generations, rejects a response outside the provider's user range or
without readable mappings, enforces both the profile and caller capacity, and
produces the exact client copy plan.

Provider exit maps only the exact live provider generation to `Peer_lost` and
zero reply bytes. A mismatched endpoint or generation is stale. Invalid calls
cannot be converted into replies. The eventual machine adapter must derive the
readable, writable, and non-executable evidence from current page tables; the
portable boolean inputs are testable policy evidence, not a trusted user claim.

## Evidence and limits

[`Endpoint-Transfer-Profile.wv`](../Operating-System/Kernel/Endpoint-Transfer-Profile.wv)
is a 9,657-byte WVB at SHA-256
`ef7801e909dd24105e6260cb8f88845e1b8d966fb90dc78350f63eeb8d1bf619`.
Its 29-case owner returns 47, checks all three profiles and the 17-page ceiling,
and pins deterministic Windows and Linux native images.

This policy is not yet connected to syscall 6/7, endpoint records, page-table
walks, copy loops, wait state, or Probe boot execution. Until that cutover, live
guest endpoint calls retain the qualified 4,096-byte maximum and applications
cannot execute the composed filesystem read through the guest kernel.
