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
| Filesystem | 14,812 bytes / `054dc2c9b5c33e02e6263b644049fd84f1ed2e1219d642ec64c066af5bdc8fcf` | 196,327 / `5ee235d5dca7bfdab8a5a1b7c54874b6545725e69da754e22e06f72f578ebdb3` | 302 / `dc212ce43b59102a05521531e6df4674291851c72a1be8990eff049ea46879dd` | 195,657 / `453cef870da3f375400d1c58cc8ebd385f761c2eafbdf3b3fb70603db8520dab` | 46 |
| Network | 13,543 bytes / `32c595716af0a3706226d677924a5279ea2d7b97b0a4cbdf7c6c9eed808e1b2a` | 243,124 / `892cfe18b81667c9e4d3e82a1889a9b1f77c45e350d2e75144694db3c2f49ca0` | 296 / `628852893fcbc32e610261517a79c3acd56714ce0c197beab1c0a3917dedf726` | 242,571 / `57067da10da68fc1d35b41784e147d8f60ed1e05441cb68bc803ad5a9682f6d1` | 47 |

Filesystem readiness proves generation-one empty provider state and the shared
no-link open profiles. Network readiness proves the four-slot queue with one
reserved control slot plus bounded network resource/direction authority. The
linked entries wait on endpoint reference `65538` and `65539`, respectively.

These are real linkable guest payloads. The current ten-section process object
embeds both in separate read-only sections and publishes exact local symbols,
but Probe 40 does not yet launch them. Request decoding, reply production,
process/domain allocation, RX mapping, endpoint creation, publication, and
teardown must be connected before live-service behavior is claimed. The
[provider launch transaction](Windvale-Os-Provider-Launch-Transaction.md) now
admits the exact image/page profiles and proves failure-atomic accounting and
teardown, but the current privileged Probe 40 machine does not consume it.
