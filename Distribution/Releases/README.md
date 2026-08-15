# Windvale release inputs

`Windvale-Root-Policy-1.json` freezes the public, non-secret inputs for the first
`0.1.x` release-key delegation. It does not contain a public trust root,
signature, or private key and does not make a release official.

Generated root policies, signatures, release manifests, qualification reports,
source archives, installers, and package assets are caller-owned release
outputs. They are not checked into this directory. The project owner must
perform the separate ceremony in
[`Documents/Runbooks/Preview-Release-Ceremony.md`](../../Documents/Runbooks/Preview-Release-Ceremony.md)
before adding the authenticated public root or publishing `v0.1.0`.

No private root or release key may enter the repository, CI, a release asset, or
an ordinary build cache. Official software-held private keys use the
passphrase-protected `.wvkey` custody wrapper from
[`Decision 0566`](../../Documents/Decisions/0566-Passphrase-Protected-Release-Key-Custody.md);
unencrypted PEM generation is test-only.
