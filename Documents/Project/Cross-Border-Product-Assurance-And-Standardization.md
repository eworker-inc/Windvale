# Windvale cross-border product assurance and standardization strategy

> Status: Project strategy for review, 2026-08-24. This document refines the
> project vision. It does not establish a normative format, an implemented
> firmware or hardware profile, a certification scheme, an export-control or
> regulatory conclusion, or a national or international standard. Each of those
> outcomes requires its own accepted decision and evidence.

## Purpose

Windvale should investigate a role as a vertically integrated computing and
assurance stack for products that cross organizational and national boundaries.
When a producer supplies a device or system, the recipient should be able to
determine more than who signed the final archive. The recipient should be able
to inspect what source, dependencies, tools, firmware, software, capabilities,
and tests produced the admitted system; reproduce the portions whose contract
permits exact reproduction; and identify every remaining external trust
boundary.

The product proposition is:

> **Build through one coherent stack. Carry exact evidence from source and
> immutable inputs through firmware and software artifacts, execution, and
> externally visible effects. Let each recipient verify the declared claims
> independently and see what remains outside the proof.**

This is a stronger and more useful target than saying that a product is open,
signed, tested, or verified without naming the subject and property of that
claim.

## The cross-border problem

A product assembled in one country may contain:

- silicon, microcode, boot firmware, device firmware, an operating system,
  drivers, runtimes, libraries, applications, models, and mutable policy;
- components from several jurisdictions and suppliers;
- proprietary build services or opaque generated artifacts;
- signatures that prove publisher selection but not source correspondence or
  correctness;
- a software bill of materials that inventories parts without proving the
  behavior or authority of the assembled system; and
- tests whose inputs, tool versions, environments, or negative cases are not
  available to the recipient.

Trust based only on the exporter, vendor, repository host, or signing key is not
independent verification. Windvale should let the producer publish exact
evidence while letting the recipient choose its own trusted verifiers, builders,
policies, and roots.

## One assurance path, not one oversized proof

Windvale should compose evidence across layers without collapsing their
meanings:

| Layer | Candidate evidence | Required non-claim |
| --- | --- | --- |
| Source and decisions | Exact revision, source identities, accepted specifications, review evidence, provenance, licenses | Availability does not prove correctness or complete human review. |
| Dependencies and package | Immutable graph, part identities, platform scope, capability requirements, SBOM mapping | Inventory does not grant authority or prove dependency safety. |
| Build and tools | Builder and tool identities, parameters, deterministic outputs, independent rebuilds | Provenance is a claim about production; it does not prove the builder was uncompromised. |
| Compiler and intermediate forms | Source admission, typed models, verified WVB, exact diagnostics and bounds | Structural and semantic admission does not prove application intent. |
| Native and firmware artifacts | Object and relocation validation, deterministic images, boot manifests, machine-profile conformance | Verified encoding does not prove silicon, microcode, or third-party firmware. |
| Runtime and operating system | Capability closure, resource limits, provider binding, isolation, revocation, teardown, observed effects | Authorized code can still be harmful or logically wrong. |
| AI workload | Exact workload, data and tool authority, model/provider identity, placement, resource and effect evidence | Opaque inference and hidden reasoning normally remain provider-reported. |
| Release and update | Signed selection, sequence policy, qualification reports, recovery and rollback evidence | A trusted signer can select a malicious or defective artifact. |

An assurance envelope may later bind these records, but this document does not
reserve a format name or serialize them. The existing Windvale package, lock,
bundle, approval, launch, provenance, qualification, and release records remain
separate contracts until a real consumer proves that one additional envelope is
needed.

## Claim vocabulary

Every public assurance statement should identify:

- the exact subject and immutable identity;
- the property being claimed;
- the producer, verifier, builder, reviewer, or attester making the claim;
- the specification and profile used;
- the tools, host, hardware, firmware, model, and provider generations that are
  material;
- the tests, hostile inputs, bounds, and comparison oracle;
- the result and retained evidence; and
- trust assumptions, exclusions, residual risks, and expiry or reconsideration
  conditions.

Use these terms narrowly:

| Term | Meaning |
| --- | --- |
| Inspectable | The stated source, specification, or evidence is available for examination. |
| Reproducible | The stated inputs and process reproduce the claimed artifact or result under the named comparison contract. |
| Conformant | An implementation satisfies a named finite specification profile and conformance suite. |
| Verified | A named verifier established a named property over an exact input; the property must be stated. |
| Qualified | The project or another named authority admitted a result after a defined evidence gate. |
| Attested | A named party or trusted mechanism signed a statement about measured evidence. |
| Human-inspected | A named responsible party inspected the stated scope; no broader review is implied. |
| Independently evaluated | A separately governed party evaluated the stated scope and method. |
| Certified | A recognized scheme or certification body issued a certificate for an exact evaluated configuration. |

Do not use **fully verified** for a whole product when the actual evidence proves
only selected formats, build identities, execution safety properties, or
components.

## Producer and recipient flow

A first cross-boundary flow should remain offline and deterministic:

1. The producer publishes exact source, package and dependency identities,
   license and provenance records, build inputs, tool identities, artifacts,
   conformance results, release signatures, and known non-claims.
2. The recipient obtains its trust roots through separately authenticated
   channels and verifies the complete inventory before extraction or execution.
3. The recipient rebuilds the portable or reproducible portions through one or
   more independently operated environments and compares the required bytes and
   normalized reports.
4. The recipient verifies WVB, objects, executable containers, firmware images,
   package authority, and launch policy through independently selected tools.
5. The recipient records which hardware, microcode, firmware, host, model, or
   service claims remain vendor-reported, attested, independently tested, or
   unknown.
6. Installation and launch bind only the separately approved capabilities and
   resource limits; signature or conformance never grants runtime authority.
7. Observed execution and external effects produce bounded evidence linked back
   to the admitted identities without claiming that observation proves every
   internal computation.

The recipient may be a customer, importer, regulator, procurement authority,
independent laboratory, system integrator, or another product team. Windvale
should define technical evidence and avoid assigning legal meaning that belongs
to a jurisdiction or certification scheme.

## Relationship to established standards

Windvale should interoperate with established assurance ecosystems instead of
creating renamed substitutes:

- [SLSA 1.2](https://slsa.dev/spec/v1.2/) describes supply-chain security tracks,
  build and source provenance, and verification summaries. Windvale-native
  evidence may map into SLSA where the semantics agree.
- [ECMA-424 CycloneDX](https://ecma-international.org/publications-and-standards/standards/ecma-424/)
  represents software, hardware, services, cryptographic artifacts, machine-
  learning models, claims, attestations, and supporting evidence. A Windvale
  product inventory should be exportable without treating CycloneDX as the
  executable identity or authority model.
- [The Update Framework](https://theupdateframework.io/) separates update roles
  and compromise resilience. Windvale's current offline release envelope does
  not claim TUF protection; a network updater should reuse or map to accepted
  TUF roles when its threat model requires them.
- [NIST SP 800-193](https://csrc.nist.gov/pubs/sp/800/193/final) separates
  platform-firmware protection, detection, and recovery. Windvale firmware work
  should state how much of the actual platform is controlled and which firmware
  remains external.
- [Common Criteria](https://www.commoncriteriaportal.org/cc/index.cfm) supplies one
  established language for evaluated configurations, protection profiles, and
  independent IT-security evaluation. Windvale qualification is not Common
  Criteria certification.

Regulations and procurement regimes may consume standards and evidence, but
compliance remains jurisdiction-, product-, role-, and date-specific. Windvale
must not present a technical verifier as universal legal approval.

## Standardization route

Windvale is currently a steward-controlled project with public specifications
and conformance evidence. It is not yet a standard. A credible route is:

1. **Project specifications** — keep exact versioned semantics, formats, limits,
   and conformance claims within the repository.
2. **Independent implementation** — enable a second implementation or verifier
   to consume only the published contract and agree on canonical fixtures.
3. **Interoperability profiles** — select finite package, execution, firmware,
   AI-workload, or assurance profiles with producer and consumer suites.
4. **External review** — involve users, implementers, security researchers,
   hardware and software vendors, procurement specialists, and relevant public
   stakeholders.
5. **Governance and rights** — define change control, compatibility, errata,
   patent disclosure, specification and test licensing, trademark use, and the
   relationship between the standard and E-Worker's reference implementation.
6. **Standards venue** — only after a demonstrated external need, decide whether
   a public project standard, industry consortium, national body, Ecma, ISO/IEC,
   or another venue matches the exact scope.

The full implementation need not become the standard. The most transferable
standard may be a small set of language, bytecode, package, capability,
conformance, and assurance interfaces backed by Windvale as the reference stack.

## Governance and licensing boundary

E-Worker Inc currently controls official Windvale repositories, releases,
qualification, and the Windvale identity. The Windvale Community Source License
also covers repository specifications and reserves some large-organization and
Windvale-as-a-product uses for commercial agreement. That policy can support a
commercial reference implementation, but external standardization may require a
separate, explicit policy for:

- reading, quoting, implementing, and redistributing normative specifications;
- using conformance fixtures and reporting compatibility;
- implementing the standard independently at commercial scale;
- patent grants, disclosures, defensive termination, and contributions;
- certification marks and truthful compatibility statements; and
- neutral participation and durable maintenance if several organizations adopt
  the contracts.

This document does not change the current license. A later decision must resolve
these questions before claiming an open or internationally implementable
standard.

## AI-led development and assurance

AI-led engineering makes the assurance problem more important, not less. Rapid
AI production can exceed the amount of source a human can inspect line by line.
Windvale should respond with narrower components, explicit models, deterministic
verification, hostile-input corpora, reproducible evidence, provenance, risk-
selected human inspection, independent implementations, and honest review
labels.

The evidence defined by
[Decision 0849](../Decisions/0849-Define-AI-Led-Research-And-Review-Evidence.md)
must remain visible across release and assurance claims. AI-produced,
AI-reviewed, machine-verified, human-inspected, independently reproduced, and
externally certified are not synonyms.

## External interoperability and evaluation

Windvale assurance contracts should remain usable across independently operated
technology and evaluation boundaries, including:

- CPU, GPU, accelerator, firmware, and system vendors;
- model providers and inference-infrastructure projects;
- OEMs and system integrators;
- independent compiler, verifier, security, and reproducible-build researchers;
- certification laboratories and standards organizations; and
- users or public institutions with a concrete cross-border assurance problem.

A provider integration does not make the provider's format or policy Windvale
semantics. External participation, evaluation, or implementation does not
substitute organizational reputation for exact technical evidence.

## First practical use cases

The first evidence should come from bounded products rather than a universal
device claim:

1. a signed Windows/Linux Windvale application and library package that a clean
   recipient can verify and reproduce offline;
2. one Windvale OS image whose Windvale-controlled source-to-boot path, package
   identities, capabilities, and QEMU behavior are reproducible while UEFI,
   emulated devices, host, and hardware boundaries remain explicit;
3. one firmware-facing or appliance profile with a finite inventory of external
   microcode and device firmware; and
4. one verified AI workload whose durable agent, code, data, tools, resource
   limits, model placement, provider evidence, and external effects remain
   separately inspectable.

Only after those cases should Windvale freeze a shared assurance-interchange
profile.

## Non-goals

This strategy does not claim:

- mathematical proof of every program or whole product;
- absence of malicious logic in authorized source;
- verification of undocumented silicon, microcode, firmware, or hosted-model
  internals;
- that signatures, SBOMs, provenance, reproducibility, conformance, attestation,
  qualification, and certification are interchangeable;
- automatic compliance with import, export-control, cybersecurity, safety,
  procurement, or sector-specific law;
- an existing certification, standards-body project, or government endorsement;
  or
- that one Windvale implementation should control an external standard forever.

## Decision triggers

A dated decision is required before Windvale accepts:

- a normative assurance-envelope name or serialization;
- a firmware, hardware-root, measured-boot, or remote-attestation profile;
- an external standardization submission or committee commitment;
- a claim of independent implementation, certification, legal compliance, or
  recognized-standard status;
- a separate specification, conformance, patent, or certification-mark license;
- a release requirement that makes cross-border assurance part of Windvale 1.0.
