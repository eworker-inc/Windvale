# Windvale distribution metadata

## Status

This tree owns checked-in metadata that selects reproducible distributable inputs.
It does not contain build caches, SDKs, generated WVB outputs, installed-package
state, or mutable application data.

`Applications/Wvdb-Query/` contains the first Package 1 manifest and Lock 1 file.
They select one application, four exact source dependencies, the workspace,
Project 2 input, native source compiler, capability closure, and expected WVB
identity. The paired native package front doors verify every locked byte identity
before compilation and publish only the exact verifier-admitted output.

The same directory contains one canonical owner approval and separate Windows
and Linux launch records. They select the exact package, bundle, WVB, five
approved semantic capabilities, rights-reduced provider table, object binding,
and target execution identities without placing native paths in portable policy.

The `local-source-1` implementation remains deliberately narrow: it admits the
one checked-in WVDB Query package and lock identity. Decision 0561 separately
admits Bundle 1 and immutable local publication for that exact application.

`Installers/` selects the exact native inputs for both the retained
`0.1.0-dev.1` development artifacts and separate `0.1.0` stable Windows/Linux
artifacts under Decisions 0562 and 0565. Generated archives are caller-owned
release outputs and are not checked into this tree. The owner-signed Release
Envelope 1 and exact-state qualification promoted these bytes into the official
[`v0.1.0` preview](https://github.com/eworker-inc/Windvale/releases/tag/v0.1.0).
A version label by itself remains insufficient evidence of authenticity.

`Releases/` freezes the non-secret input policy for Release Envelope 1. The
creator and independent offline verifier are implemented. The project owner
completed protected key custody and published the authenticated public root,
signatures, manifest, keys, and product artifacts as external `v0.1.0` release
outputs. Those generated outputs and all private keys remain outside this tree.
General resolver, registry, updater, rollback, and package-manager contracts
remain outside Installer 1.

Package metadata does not grant capabilities. A launcher or service manager must
still approve and bind each required rights-limited provider independently.
