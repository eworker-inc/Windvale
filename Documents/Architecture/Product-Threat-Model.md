# Windvale 0.1 product threat model

## Status and scope

This is the public threat model for the first Windows/Linux Windvale preview
selected by [Decision 0183](../Decisions/0183-Product-Packaging-Trust-And-Evolution.md).
It covers the product that is actually intended to ship: source and project
inputs, compiler and native tools, WVB/WVO and target containers, Bundle 1 and
WVDB Query, capability approval and provider binding, deterministic installers,
Release Envelope 1, offline verification, and the immutable Stage 0 recovery
reference.

It does not claim that Windvale OS, a registry, network updater, browser,
virtualization stack, device passthrough, database server, or arbitrary future
package is part of 0.1. Their accepted architecture retains separate threat
boundaries and must extend this model before distribution.

This document routes to normative validators and limits. It does not replace
their exact rules.

## Security objectives

The preview protects these assets:

- the exact source revision, source tree, package, tool, installer, approval,
  recovery, provenance, qualification, and release-manifest identities;
- the private root and release signing keys;
- compiler, verifier, installer, package-store, and launch-policy integrity;
- application data that is outside immutable packages and tool generations;
- least-authority capability approval and rights-reduced provider bindings;
- deterministic output and cross-host evidence; and
- a recoverable native lineage independent of the ordinary online repository.

The product aims to detect substitution, corruption, rollback below an explicit
minimum, malformed structured input, path escape, capability widening, and
incomplete or indeterminate publication before those states are treated as
installed, runnable, or official.

Signatures prove selection by a trusted key. They do not prove correctness,
safety, reproducibility, or capability authorization. Reproducibility and
qualification remain independent evidence.

## Attacker and environmental assumptions

An attacker may control downloaded archives, release directories, source text,
project/package/lock/provenance records, WVB, WVO, bundles, object records,
relocations, resource names and bytes, launch arguments, mutable application
data, native paths supplied to host tooling, CI caches, mirrors, and all bytes
presented to public parsers or verifiers. Inputs may be truncated, oversized,
inconsistent, duplicated, reordered, aliased, malicious, or chosen to consume
work and diagnostic capacity.

The preview does not defend execution after the host kernel, administrator
account, verifier process, or trusted signing private key is compromised.
Hardware attacks, malicious firmware, physical memory extraction, speculative
execution, and a sufficiently capable quantum attacker are outside the first
host-product claim. The user must obtain the root public key through an
independent authenticated channel and run the verifier from a trusted checkout
or separately authenticated copy.

## Trust boundaries

1. **Download to offline release verifier.** Every release-directory byte is
   untrusted. The caller-provided root key is the only initial release trust
   anchor.
2. **Extracted installer to installation generation.** Archive extraction is a
   host bootstrap action; the installer revalidates its canonical payload before
   creating the installation root and again before generation publication.
3. **Source/project/package input to compiler output.** Parsing, binding,
   lowering, serialization, and publication are separate boundaries with
   explicit models and no-partial-output behavior.
4. **WVB/WVO/container to execution.** Binary readers verify complete geometry,
   types, capabilities, symbols, relocations, target policy, and bounds before
   publication or execution.
5. **Bundle to content store.** Bundle admission completes before immutable
   object publication. Rejection publishes nothing.
6. **Package requirement to application-owner approval.** Declaring or signing
   a capability is not a grant. Approval and target launch records select the
   exact rights-limited provider set separately.
7. **Windvale semantics to host adapters.** Windows and Linux leaves own native
   handles, syscalls, paths, modes, and error translation; portable code never
   receives those values.
8. **Ordinary development to recovery.** The .NET-free normal path and the
   immutable Stage 0 recovery release are distinct. Recovery is used from a
   separate restored workspace under its documented decision boundary.
9. **Offline root to release signer.** The root private key remains offline and
   delegates a replaceable release public key. CI and ordinary development may
   use the release signer only according to the accepted ceremony; neither key
   grants runtime authority.

## Threats, controls, and residual risk

| Threat | Required control | Residual risk / non-claim |
| --- | --- | --- |
| Release or mirror substitutes an artifact | Release Envelope 1 pins the independently obtained Ed25519 root, verifies root and release signatures, and checks every declared artifact byte plus the complete file inventory. | A compromised trusted root can authorize malicious bytes; signature validity is not code correctness. |
| Replay of an older valid preview | Root policy bounds release sequences; the offline caller can require a minimum sequence. | There is no automatic time/freshness or persistent anti-rollback state in version 1. |
| Root or release private key disclosure | Private keys are external to the repository, CI, logs, and release assets; root and release roles are separate. | The first format has one key per role and no threshold/HSM guarantee. Root compromise requires an explicit public trust reset unless an authenticated successor policy already exists. |
| Manifest/signature type confusion | Signatures include a Windvale version and exact `root-policy` or `release-manifest` domain. | Cryptographic-library or verifier compromise remains host compromise. |
| Archive path escape or symlink payload | Installer manifests use ordinary relative paths; builders reject traversal; verifiers check resolved containment; Linux payload verification rejects links; install roots reject broad filesystem roots. | The host extraction utility runs before the platform installer. Users must extract into a new directory and then run verification; OS extractor vulnerabilities remain host risk. |
| Installer payload changes after download | Exact transport and payload identities, pre-install verification, private candidate copy, second verification, immutable generation naming, manifest-pinned `wv doctor`, and exact uninstall record. | No OS code-signing, notarization, package-manager registration, repair service, or automatic updater is claimed. |
| Installer removes unrelated state | Per-user defaults, broad-root refusal, exact installation record, target-contained candidate cleanup, and Linux refusal to replace unrelated command links. | A user-selected installation root is product-owned and complete uninstall removes that root; separately owned state must remain outside it. |
| Malicious source or dependency graph escapes declared inputs | Project 2, Package 1, and Lock 1 require explicit paths, bounded graphs, exact identities, transitive capability closure, and deterministic no-partial-output compilation. | The source language and compiler are experimental; logic bugs may remain despite conformance and cross-host evidence. |
| Malformed WVB reaches execution | The [Seed bytecode contract](../../Specifications/Seed-Bytecode.md) and native verifier revalidate sizes, indices, types, functions, control flow, exports, capabilities, and profile before execution. | Verification does not make an intentionally harmful but authorized program benign. Runtime step and memory limits remain required containment. |
| Malformed WVO, symbol, relocation, or target container causes unsafe layout | WVO, linker, PE/ELF, hosted-container, and publisher verifiers use checked geometry, explicit architecture/ABI policy, exact target identities, and no publication on failure. | x86-64 is the only qualified native architecture. Host loader and CPU defects remain outside format admission. |
| Bundle overlap, gap, alias, bomb, or extraction escape | [Bundle 1](../../Specifications/Windvale-Package-Bundle.md) validates the 128-byte header, counts, checked offsets, canonical index, every blob digest, complete contiguous coverage, role reachability, Package/Lock agreement, and target executable before immutable publication. Compression is absent. | The current in-memory implementation accepts at most 4 MiB; larger streaming bundles are not admitted. |
| Content-addressed store overwrite or partial publication | Create-private, verify/reread, atomic publish, and byte-identical idempotent reuse; existing mismatches are corruption. | Garbage collection, multi-generation activation, rollback, and concurrent store service are not part of the first slice. |
| Capability requirement silently becomes authority | Package, lock, approval, release signature, and launch binding are separate evidence. Approval 1 names exactly five capabilities and denies ambient authority. | The first approval format is fixed to WVDB Query and is not a general policy language. |
| Read-only directory becomes ambient filesystem access | The [directory capability](../../Specifications/Read-Only-Directory-Capability.md) binds one fixed object, exposes no path or handle, allows chunks of at most 3,072 bytes, and has separate Windows/Linux read-only leaves. | The native proof uses one fixed current-directory object and is not a configurable isolated directory service. Host compromise can replace bytes before the leaf opens them. |
| Argument, output, or diagnostic provider leaks broader host state | Launch records bind an immutable two-argument shape, standard output, and separate diagnostics; source receives no environment, native handles, network, clock, entropy, or process-launch capability. | Output is visible to the invoking host process and may contain application-selected data. The user controls whether to redirect or publish it. |
| Capability provider exits, is unavailable, or denies access | Bindings prove initial availability only; WVDB Query retains explicit denied/unavailable outcomes and no ambient fallback. | The first hosted application does not implement provider restart, revocation notification, or long-lived supervision. |
| Malicious mutable database input corrupts executable authority | The database file is read-only application data parsed through bounded WVDB readers; it is not executable package metadata and grants nothing. | Application-level data correctness and confidentiality depend on the provider and user-selected data source. No encryption is provided. |
| CI cache or runner is compromised | Release manifests select exact revision/tree and artifact hashes; paired-host qualification and independent offline verification are separate from provenance. Caches are development acceleration, not release identity. | A compromised signer can still select malicious artifacts. Independent rebuilders and key custody are organizational controls, not cryptographic consequences of CI. |
| Provenance is forged or incomplete | Provenance is a manifest-selected immutable artifact and must agree with package/tool identities. Reproducibility compares actual bytes independently. | Provenance describes a claim; it cannot prove that an undeclared compiler or compromised host was absent. |
| Recovery bundle is replaced or normal-path .NET returns | The immutable `stage0-recovery-e5a1a7473c57` identity and recovery instructions are release-manifest evidence; managed recovery occurs in a separate restored workspace and requires a new decision. | Recovery availability depends on durable external retention and documented bootstrap dependencies. Recovery does not automatically repair a compromised signing root. |
| Diagnostics become a denial-of-service or data-exfiltration channel | Parsers and scripts emit bounded phase/item progress and stable summaries; exact contracts bound text/byte values and reject excess work where implemented. Secrets, private keys, and native handles are not diagnostic fields. | Complete global log redaction, crash-dump policy, and structured observability service are later work. Host shells may record invoked paths and arguments. |

## Size and work budgets

Important preview limits include:

- Release Envelope 1: 4,096 artifacts, 256 MiB per artifact, 512 MiB total,
  8 MiB manifest/input metadata, 1,024-byte/32-segment paths, and 8,192 complete
  inventory entries;
- development installer: exactly two targets and seven native tools per target;
- Bundle 1 implementation: 4 MiB in memory, with the larger format boundary not
  admitted by this implementation;
- WVB/WVO and native tools: the exact bounds owned by their format and
  retirement specifications;
- hosted arguments: 67 arguments, 4 KiB each, 64 KiB total;
- read-only directory reference provider: 4,096 immediate entries and 64 MiB
  total, while the fixed native WVDB provider exposes one object and 3,072-byte
  chunks; and
- browser, VM, device, networking, and OS resource budgets are outside the 0.1
  host-product distribution unless separately named by a release manifest and
  successor threat-model review.

Checked arithmetic, exact counts, no hidden decompression, canonical ordering,
and no-partial-publication behavior are security properties, not performance
optimizations.

## Key and release incident response

- Suspected release-key compromise stops signing immediately. Publish no new
  envelope until the offline root owner approves a new policy generation and
  release key.
- Suspected root compromise stops the release channel. Do not claim ordinary
  rotation; announce a trust reset and distribute a new root through independent
  authenticated channels.
- A vulnerable but uncompromised old release remains cryptographically valid.
  Distributors and users reject it by selecting a trusted minimum sequence or a
  later policy. Version 1 has no online revocation service.
- A malformed or inconsistent release is never repaired in place. Correct it
  with a new sequence/version and immutable assets.
- Preserve affected manifests, signatures, qualification reports, source
  revision, and recovery evidence for audit without preserving private keys or
  secrets in ordinary logs.

## Review triggers

Review and extend this model before shipping a new parser/format, mutable
provider, network client/updater, registry, automatic rollback state, second
application approval grammar, second architecture, Windvale OS image, browser
origin/storage flow, VM/firmware/device input, encrypted secret store, telemetry
export, or any release-key/root transition.
