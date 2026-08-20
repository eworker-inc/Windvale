# Windvale release inputs

`Windvale-Root-Policy-1.json` freezes the public, non-secret inputs for the first
`0.1.x` release-key delegation. It does not contain a public trust root,
signature, or private key and does not make a release official by itself.

Generated root policies, signatures, release manifests, qualification reports,
source archives, installers, and package assets are caller-owned release
outputs. They are not checked into this directory. The project owner completed
the separate ceremony in
[`Documents/Runbooks/Preview-Release-Ceremony.md`](../../Documents/Runbooks/Preview-Release-Ceremony.md)
and published the authenticated public root and signed product envelope with the
[`v0.1.0` preview](https://github.com/eworker-inc/Windvale/releases/tag/v0.1.0).
Future releases must repeat or deliberately supersede that documented custody
and signing policy; they must not reuse generated ceremony outputs implicitly.

Decision 0750 adds a narrower, deterministic Installer Repository 1 development
candidate. Its index is ready to be one future signed `repository|all` subject,
but the checked-in historical `0.2.0-dev.1` identity is not signed or published,
does not select a `v0.2.0` release line under
[Decision 0800](../../Documents/Decisions/0800-Target-Windvale-1.0-Directly.md),
and does not implement network release discovery. See
[`Specifications/Windvale-Installer-Repository.md`](../../Specifications/Windvale-Installer-Repository.md).

No private root or release key may enter the repository, CI, a release asset, or
an ordinary build cache. Official software-held private keys use the
passphrase-protected `.wvkey` custody wrapper from
[`Decision 0566`](../../Documents/Decisions/0566-Passphrase-Protected-Release-Key-Custody.md);
unencrypted PEM generation is test-only.
