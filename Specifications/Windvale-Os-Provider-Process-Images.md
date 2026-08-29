# Windvale OS provider process images

## Status and scope

The first filesystem and network provider process payloads are implemented as
deterministic Windvale-native x86-64 images. Each consists of a portable
Windvale service root plus a WVA user-entry shim. The shim validates the
service's readiness token, supplies a page-bounded receive buffer, waits through
the existing syscall operation 5 on its dedicated endpoint, and exits through
operation 3 if the receive returns or readiness fails.

| Provider | WVB | Main WVO | Shim WVO | Linked guest image | Readiness |
| --- | --- | --- | --- | --- | ---: |
| Filesystem | 14,812 bytes / `054dc2c9b5c33e02e6263b644049fd84f1ed2e1219d642ec64c066af5bdc8fcf` | 196,327 / `c0cbc0ce96f14858de9f3973da4cfb5335f6c7087cdd78e6397b480093d59fcc` | 302 / `aae81021f8e5d349570533299bbd1c4196358c3ad857eecc80b5b918c48f301c` | 195,657 / `d40d9cdb16f9aa115a20bac2b27f572fad853eca27cf2539fe61dfd2ecbd7601` | 46 |
| Network | 13,543 bytes / `32c595716af0a3706226d677924a5279ea2d7b97b0a4cbdf7c6c9eed808e1b2a` | 243,124 / `892cfe18b81667c9e4d3e82a1889a9b1f77c45e350d2e75144694db3c2f49ca0` | 296 / `ffc757391199f456850bdb80a2f67b1815b7bc7c1dda9a1bf6b6ed1919df87af` | 242,571 / `68182de6018a6c64d02c4a384355ea14c463a67d1939cb18db0c058223358e42` | 47 |

Filesystem readiness proves generation-one empty provider state and the shared
no-link open profiles. Network readiness proves the four-slot queue with one
reserved control slot plus bounded network resource/direction authority. The
linked entries wait on generation-two endpoint references `131072` and
`131073`, respectively. Those are the checked post-client resource and
post-directory slot identities selected by provider-machine binding.

These are real linkable guest payloads. The current fourteen-section process
object embeds both in separate read-only sections and publishes exact local
symbols. Probe 40 allocates and maps the filesystem image, advances its endpoint
slot, and publishes provider-side ready process/thread state, but does not enter
the payload. Request decoding and reply production are therefore still portable
and hosted evidence rather than live guest behavior. Consumer capability and
durable domain binding, first entry/request, teardown, and the later network
machine must be connected before a live-service claim. The
[provider launch transaction](Windvale-Os-Provider-Launch-Transaction.md)
admits the exact image/page profiles and proves policy-level failure accounting
and teardown; the privileged fixture currently consumes only the construction
and provider-side publication prefix.
