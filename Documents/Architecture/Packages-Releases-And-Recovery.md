# Windvale packages, releases, updates, and recovery architecture

## Status

Recommended architecture under proposed [Decision 0198](../Decisions/0198-Next-Integrated-Architecture-Defaults.md). It details the accepted product direction in [Decision 0178](../Decisions/0178-Project-Stewardship-Archives-And-Recovery.md) and [Decision 0183](../Decisions/0183-Product-Packaging-Trust-And-Evolution.md). [Decision 0530](../Decisions/0530-First-Locked-Source-Package-And-Wvdb-Application.md) implements one exact Package 1 / Lock 1 local-source baseline around Project 2; [Decision 0561](../Decisions/0561-First-Admitted-Bundle-Store-And-Rights-Reduced-Wvdb-Query.md) implements its admitted Bundle 1 and bounded immutable publication; Decisions [0562](../Decisions/0562-First-Deterministic-Development-Installers.md) and [0565](../Decisions/0565-First-Stable-Preview-Installers.md) implement deterministic development/stable per-user native-tool installers; and Decisions [0563](../Decisions/0563-First-Release-Envelope-And-Key-Policy.md), [0564](../Decisions/0564-First-Installed-Capability-Approval-And-Launch-Records.md), and [0566](../Decisions/0566-Passphrase-Protected-Release-Key-Custody.md) own the release-signing, offline-verification, approval, launch, and protected-key contracts used by the signed [`v0.1.0` preview](https://github.com/eworker-inc/Windvale/releases/tag/v0.1.0). The 0.1 gate is complete. A general resolver, updater, multi-generation activation/rollback manager, and Windvale OS A/B installation contract do not exist yet.

## Recommendation

Windvale should use immutable content-addressed package objects and installation generations. Mutable state belongs in separately granted application or system storage. A package name, version, source location, Git tag, signature, or installed path is evidence about an object; only its canonical content digest is its identity.

Keep five artifacts separate:

1. a project manifest selects source inputs for one build;
2. a package manifest describes parts, dependencies, resources, platform scope, and capability requirements;
3. a lockfile records one fully resolved deterministic dependency and target graph;
4. a package bundle carries the exact immutable content named by the manifest; and
5. a release envelope signs and attests a collection of packages, tools, source, recovery evidence, and qualification reports.

An update plan selects a new installed generation. It is not another package format.

## Package parts and identity

One package may contain multiple named parts, but every part declares its own:

- canonical module, WVB, AOT, resource, or tool identities;
- environment, architecture, ABI, and platform-extension scope;
- authority level;
- required and optional semantic capability interfaces;
- dependencies on exact package parts;
- immutable resource identities and limits; and
- license and provenance references.

Portability is derived for the selected part graph. A Windows-only driver does not make an unrelated shared library dishonest, and a shared package name does not make every part portable.

The first bundle is deterministic, bounded, independently verifiable, and safe to inspect before extraction. Its logical sections include a versioned header, canonical manifest, ordered part table, ordered content table, immutable blobs, license/provenance references, and integrity evidence. Every offset, size, count, compression method, name, and digest is validated with checked arithmetic. Compression, if admitted later, has explicit expanded-size and work limits and cannot change canonical content identity.

Package resources are addressed by canonical resource identities within a selected package part. They are not native paths and cannot escape into mutable application storage. Executable bytes remain subject to WVB verification or target-specific executable admission after package verification.

## Lockfiles and resolution

Resolution may use local directories or explicitly configured GitHub releases first, but it completes before compilation, installation, or launch. The canonical lockfile records:

- lockfile and resolver versions;
- root package and requested parts;
- every exact package digest, declared version, origin, and origin evidence;
- the complete dependency graph and selected target parts;
- environment, architecture, ABI, and extension selection;
- required and optional capability closure;
- license identifiers and integrity/provenance references; and
- resolver policy inputs that can affect the graph.

Entries are ordered canonically, and identical inputs produce identical lockfile bytes. Network access is never needed after every locked object is present in the local content store. A build using a lockfile does not silently choose a newer dependency, alternate origin, host-native library, or broader capability.

The application owner approves the exact transitive capability closure separately. Signing a package, locking a dependency, or installing a part does not grant any of those capabilities.

## Content store and installation generations

Verified package objects enter a content-addressed store through write-to-temporary, verify, then atomic-publish behavior. Existing objects are immutable. A human-readable package name maps to an installed generation outside the content object.

An installation generation contains exact root packages, lockfile, selected targets, approval records, provider expectations, and launch identities. Creating a new generation does not rewrite the active generation in place. Activation changes one small generation pointer atomically after all objects and policy are admitted.

Garbage collection removes only objects unreachable from active, rollback, recovery, pinned development, or audit generations. It is a separately authorized maintenance operation with a dry-run inventory and deterministic reachability evidence.

## Release trust and provenance

Official releases publish:

- exact source archive and source revision;
- canonical package bundles and lockfiles;
- Windows and Linux tools and their WVB/AOT/container identities;
- dependency and license inventory;
- Stage 0 recovery bundle and reconstruction instructions;
- qualification reports and artifact digests;
- bounded build provenance or attestations; and
- a signed release manifest covering every distributed object.

Release Envelope 1 implements one pinned offline Ed25519 root authorizing one
replaceable release key for a bounded version line and sequence range. The
manifest records key and policy generation, and an offline caller may impose a
minimum sequence. Root rotation, threshold custody, emergency revocation, and
network freshness are deliberately not approximated. A future design may adopt
The Update Framework's distinct roles when network updates become real;
Windvale does not claim TUF protection from this smaller offline profile.

Provenance is evidence, not reproducibility by itself. Windvale can emit a bounded Windvale-native record and optionally translate it to the current [SLSA provenance model](https://slsa.dev/spec/v1.2/provenance) for ecosystem tooling. Exact rebuilds and independent verification remain authoritative project evidence.

GitHub is the operational durable archive accepted by Decision 0178, with the E-Worker-controlled local mirror as operational redundancy. Release manifests name immutable repository revisions and assets; a mutable branch or release page is not the only recovery identity.

## Updates and rollback

Application and tool updates install a new immutable generation, verify it completely, then switch activation. Windvale OS later uses two bootable system slots plus a small boot-selection record:

1. write the inactive slot;
2. verify package, image, boot manifest, and signature identities;
3. mark it as a pending generation with a bounded boot-attempt count;
4. boot it without overwriting the previous confirmed slot;
5. confirm health only after the required services and recovery path are available; and
6. roll back automatically or through local recovery when confirmation does not arrive.

The boot-selection mutation must distinguish rejection, completed switch, exact partial progress where meaningful, and indeterminate completion. Recovery reads both slots and signed generation evidence rather than assuming the last attempted write succeeded.

Rollback for security-sensitive releases is policy, not an unconditional feature. A signed minimum-version or revoked-generation rule may prevent returning to a known-vulnerable version, but it requires trustworthy persistent state and a separately qualified recovery ceremony.

## Achieved Windvale 0.1 gate

Windvale 0.1 is the first inspectable product release, not the first OS release.
The signed `v0.1.0` preview completed this gate on 15 August 2026 with the
following evidence:

1. the normal Windows and Linux build, verify, inspect, and run path is .NET-free under the complete Decision 0057 retirement gate;
2. the archived Stage 0 recovery bundle reconstructs the exact native compiler/toolchain lineage from pinned inputs;
3. one useful application and reusable library build from a package manifest plus canonical lockfile and run from the same canonical WVB identity on Windows and Linux;
4. capability requirements, root approval, rights-reduced provider binding, and denial behavior are inspectable in package and execution evidence;
5. official source, package, tool, provenance, license, and qualification artifacts are reproducible and signed through the first release-key policy;
6. the public threat model covers the shipped parsers, verifiers, packages, providers, and recovery path; and
7. a clean third-party checkout can verify the release offline after obtaining the documented archive.

Windvale OS, a public package registry, automatic network updates, WebAssembly permanence, ARM64, broad hardware, a desktop, and 1.0 compatibility are not 0.1 requirements. If completing package v1 would delay a useful preview excessively, publish a clearly labeled development snapshot rather than weaken the 0.1 meaning.

## Implementation standing and remaining sequence

1. ✅ Keep Workspace 1 and Project 2 as deterministic source-build inputs rather than package formats.
2. 🔵 Retain the exact Package 1 / Lock 1 pilot; generalize it only when a second real package requires resolution beyond the local-source graph.
3. ✅ Implement the bounded independently verified content-addressed store and deterministic bundle required by Milestone 2.
4. 🔵 Bind exact approved capabilities through the 0.1 launch records; a general clean-spawn launcher remains future work.
5. ✅ Publish deterministic Windows/Linux installers plus the signed release envelope and offline verification evidence.
6. ✅ Preserve Decision 0057 retirement and Stage 0 recovery, then qualify and publish the exact `v0.1.0` state.
7. 🔵 Installed generations, activation recovery, application rollback, and recoverable data-preserving uninstall are implemented locally; final paired-host evidence remains before closing Milestone 4.
8. ○ Add Windvale OS A/B system updates only after writable filesystem, durability, boot selection, civil time or signed freshness, key custody, and local recovery evidence exist.

## Deliberately open details

Package 1 and Lock 1 freeze `.wvpack` and `.wvlock`, canonical text records, and the exact `local-source-1` pilot. Bundle 1 and Release Envelope 1 freeze the first bundle and signature profiles without defining a general resolver, threshold policy, content-store garbage collection, release cadence, update transport, or online freshness. The broader architecture fixes immutable content identity, separate manifests/locks/bundles/releases, per-part metadata, offline operation, non-authorizing signatures, generation-based activation, recoverable updates, and the recommended 0.1 product boundary.
