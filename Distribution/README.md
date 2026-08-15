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

`Installers/` selects the exact native inputs for the unsigned
`0.1.0-dev.1` Windows and Linux development installers under Decision 0562. The
generated archives are caller-owned release outputs and are not checked into this
tree. This first installer proves deterministic per-user tool installation without
pretending that a general resolver, registry, updater, rollback manager, or
official `v0.1.0` release exists.

`Releases/` freezes the non-secret input policy for Release Envelope 1. The
creator and independent offline verifier are implemented, but generated public
roots, signatures, manifests, and release artifacts remain external ceremony
outputs. No official root or product envelope exists until the project owner
accepts custody and publishes it.

Package metadata does not grant capabilities. A launcher or service manager must
still approve and bind each required rights-limited provider independently.
