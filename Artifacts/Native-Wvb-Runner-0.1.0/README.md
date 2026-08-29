# Windvale 0.1.0 WVB runner inputs

This directory retains the exact host-runner inputs selected by the immutable
Windvale 0.1.0 stable installer. They are release reconstruction inputs, not the
active development runner family.

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| `windows-x64-wvrun.exe` | 5,659,136 | `2292555c4dad03d646d7e14d0bf716bd663d95b1d0e224f9f6c11d598b519114` |
| `linux-x64-wvrun.elf` | 5,660,672 | `ccaaa6cbb76c557e65c169ef8bad7ca3396c0a38e3e4b18adf303f94077e83d1` |

The current development runner remains under
`Artifacts/Native-Wvb-Runner-Candidate/`. Keeping the stable installer on this
versioned path prevents a later candidate promotion from changing published
0.1.0 reconstruction inputs. Git reuses the already-existing content-addressed
binary blobs rather than storing a second copy of the bytes in repository
history.
