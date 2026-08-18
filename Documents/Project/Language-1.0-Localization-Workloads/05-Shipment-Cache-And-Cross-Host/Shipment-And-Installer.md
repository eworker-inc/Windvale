# Localization shipment and installer contract

## Logical package selections

The first general package manifests may use these working ASCII-safe package
identities; package identity/version remains separate from source-profile
identity/version:

| Package | Contents | Semantic compiler input? |
| --- | --- | --- |
| `windvale.source.edition1` | Shared Unicode-profile and keyword-token identities plus any one-copy generated table object owned by the SDK. | Yes |
| `windvale.source.en` | Exact `en@1` profile, lexicon, vocabulary profile, and complete interface catalogs. | Yes |
| `windvale.source.zh-hans` | Exact `zh-Hans@1` profile, lexicon, vocabulary profile, and complete interface catalogs. | Yes |
| `windvale.diagnostics.zh-hans` | Bounded localized diagnostic templates. | No |
| `windvale.documentation.zh-hans` | API prose, tutorials, search data, and optional font/IME guidance. | No |

The first three are source-development resources, not executable libraries or
runtime dependencies. A future packaging implementation may refine package names
without changing Language 1.0 source semantics, but released manifests and locks
remain immutable exact identities.

## Installer choices

The installer exposes product intent rather than a list of internal artifacts:

| Selection | Result |
| --- | --- |
| Runtime only | Runtime/application objects; zero source localization objects. |
| Developer tools (English) | Compiler/toolchain plus shared edition objects and `en@1`; this is the minimal developer installation. |
| Add Simplified Chinese source | Adds exact `zh-Hans@1` semantic source objects; requires the shared edition selection already present. |
| Add Chinese diagnostics/docs | Adds non-semantic experience objects independently of source support. |

Host UI locale may suggest a visible selection but cannot silently grant it,
change a `.wv` descriptor, or become compiler input. The final transaction
summary lists exact package/profile identities and incremental bytes.

Release 1 distributes `en@1` and `zh-Hans@1` only after each reaches its required
qualification state. If Chinese remains draft, the official installer must not
label or install it as qualified support merely because candidate artifacts
exist in the repository.

## Immutable objects and deduplication

Every admitted localization artifact becomes an ordinary immutable content
object addressed by SHA-256. The existing Bundle 1 rule stores each blob once in
one bundle, and the host store stores one physical object per digest. Multiple
packages, workspaces, users within the selected installation scope, generations,
or profiles may reference the same object without copying it.

The release/package graph places Unicode/token objects in the shared edition
selection rather than embedding them anew in each language bundle. Even when an
offline archive repeats a blob for independent-bundle recovery, store publication
verifies and reuses the existing digest object rather than retaining duplicate
installed bytes.

Deduplication never uses filename, locale tag, file length, hard-link guess,
compression identity, or unverified hash metadata. A store object is immutable;
language packages cannot expose a writable alias to it.

## Explicit build resolution

Installation makes exact objects available; it does not select them for a build.
The package/build resolver consumes the source file descriptor plus explicit
project lock and produces an exact source-input lock naming profile/component/
catalog identities and hashes. The compiler receives those bytes or immutable
handles as explicit inputs.

The compiler never searches installed profiles, chooses the newest pack, falls
back to English, or downloads a missing catalog. A build requesting an exact
uninstalled object fails with its identity/hash. Installation order does not
affect resolution.

## Offline and connected acquisition

An offline release directory carries the signed release envelope, exact selected
bundles/objects, manifests/locks, licenses, and qualification evidence. It may be
minimal English-only or include optional Chinese/experience selections.

Offline and connected routes must produce:

- byte-identical portable localization objects;
- the same ordered logical package/profile selection for the same request;
- the same content-store object identities; and
- on one host/target, the same immutable generation bytes when all other
  generation inputs are equal.

The generation's official source identity names the release/policy, not whether
transport happened through HTTPS or an offline directory. Local audit records may
record transport separately without changing semantic/package identity.

If an offline directory lacks a requested optional package, installation fails
before generation publication and reports the exact missing objects. It never
contacts a network unless the user starts a separate connected operation.

## Update, rollback, removal, and garbage collection

A source profile version is immutable. A terminology correction creates, for
example, `zh-Hans@2` plus new component/catalog hashes. It does not rewrite
`zh-Hans@1`, existing source descriptors, build locks, store objects, or caches.
Source adopts the new version only through explicit Workload 3 conversion.

An installer update:

1. admits all new portable and host objects privately;
2. constructs and verifies a complete new installation generation;
3. retains the prior generation as rollback according to existing policy; and
4. atomically changes activation only after success.

In-flight compiler/build requests retain the old immutable generation snapshot.
New requests observe one complete old or new generation, never a mixture.

Rollback selects the old admitted generation; it does not decrement a source
descriptor, mutate content, or override a signed security minimum. Removing an
optional language creates a new generation without it. Objects reachable from
active, rollback, recovery, pinned-workspace, or audit generations remain in the
store. Separately authorized garbage collection removes only proven unreachable
objects after a dry-run inventory.
