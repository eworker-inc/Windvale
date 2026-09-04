# Windvale 0.1.0 WVB verifier inputs

This directory retains the exact host-verifier inputs selected by the immutable
Windvale 0.1.0 stable installer. They are release reconstruction inputs, not the
active development verifier family.

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| `windows-x64-wvverify.exe` | 1,255,936 | `a1dc701cc8d5ace0a680a15e19435c48b3bccde3cf6197bfdd07ee04a4bf9871` |
| `linux-x64-wvverify.elf` | 1,257,472 | `cb77e47f1d69530a16c661deecd91640764a13994d75c4994780e488e938b1f4` |

The current development verifier remains under
`Artifacts/Native-Front-Door/`. Keeping the stable installer on this versioned
path prevents a later candidate promotion from changing published 0.1.0
reconstruction inputs. Git reuses the already-existing content-addressed binary
blobs rather than storing a second copy of the bytes in repository history.
