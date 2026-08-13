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

The current `local-source-1` implementation is deliberately narrow: it admits the
one checked-in WVDB Query package and lock identity. It proves a deterministic,
offline source-package baseline without pretending that a general resolver,
content-addressed store, package bundle, registry, signature envelope, installer,
or update client exists.

Package metadata does not grant capabilities. A launcher or service manager must
still approve and bind each required rights-limited provider independently.
