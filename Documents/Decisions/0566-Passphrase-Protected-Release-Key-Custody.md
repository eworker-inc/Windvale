# Decision 0566: Passphrase-protected release-key custody

- Status: Implemented
- Date: 2026-08-15
- Advances: Decisions 0563 and 0565
- Contract: [Windvale release envelope version 1](../../Specifications/Windvale-Release-Envelope.md)

## Context

Decision 0563 required official private keys to remain outside the repository,
CI, releases, logs, and ordinary development storage. Its first ceremony tool
exported unencrypted PKCS #8 PEM and therefore required an encrypted containing
volume. The first owner custody arrangement instead uses three separately
detachable virtual data disks whose host storage and access are owner-controlled
but whose guest volumes are intentionally not encrypted.

Detaching an unencrypted virtual disk does not protect its backing file,
snapshots, or backups from disclosure. Reusing an authentication key, placing a
passphrase on a command line, or silently weakening the first root policy is not
an acceptable substitute.

## Decision

Add Windvale encrypted private-key wrapper version 1 as the official software
custody adapter. It contains an Ed25519 PKCS #8 DER private key encrypted with
AES-256-GCM. Derive the encryption key from a 16..1,024-byte UTF-8 passphrase
using scrypt with `N=131072`, `r=8`, `p=1`, a random 32-byte salt, and a
256 MiB derivation ceiling. Use a random 12-byte nonce and a 16-byte GCM tag.
Authenticate the wrapper version, role, public-key identity, KDF parameters,
salt, cipher, nonce, and plaintext length as associated data.

The creator writes protected private keys as `.wvkey`, binds each wrapper to
the `root` or `release` role and SHA-256 public-key identity, rejects altered
parameters and noncanonical encodings, and reports one generic unlock failure
for an incorrect passphrase or failed authentication. Passphrases are entered
through a masked terminal prompt for official operations, never as command-line
arguments or environment variables, and are cleared from mutable buffers after
use. Piped input exists only for fixed non-secret test credentials.

Retain unencrypted PKCS #8 generation only behind the explicitly named
`generate-test-key` command for ephemeral conformance and dry-run keys. The
official `generate-key` command always requires `--key-passphrase` and emits a
protected `.wvkey` file.

Use separate passphrases for root and operational release/tag custody. Keep the
two protected root copies on independently detached `Keys A` and `Keys B`
disks, detach both after root-policy signing, and move the `Keys B` backing
object to a separate durable failure domain before publication. Keep the
protected release and Git tag-signing keys on the `Release` disk and detach it
between ceremonies. Key-file encryption protects storage snapshots; it does
not protect an unlocked key from a compromised guest, host, terminal, or
operator session.

## Consequences

- The first release can use owner-controlled detachable disks without
  encrypting their complete guest filesystems.
- A copied `.wvkey` remains confidential without its passphrase, while its role,
  public identity, and cryptographic parameters remain inspectable.
- Losing both root copies or their passphrase is an authenticated-trust reset;
  losing the release key requires a new root-signed policy generation.
- The release-envelope owner expands from 13 to 16 cases and covers protected
  round-trip signing, wrong/missing credentials, and wrapper tampering on both
  hosts.
- Existing envelope, root-policy, manifest, signature, and verifier formats do
  not change.

The expanded sixteen-case owner passed on Windows and Linux in Verify run
31888902259 at implementation commit
`41ac658a72d5848d0891af082a9cd03a9b6c8390`.

## Reconsideration triggers

Reconsider for hardware-backed or threshold custody, password-manager or
signing-service integration, a different memory-hard KDF profile, unattended
official signing, root rotation, or a requirement to protect keys while in use
from the VM host.
