# Decision 0595: Select Windvale 0.2.0 connected-services preview

- Date: 2026-08-15
- Status: Accepted; release gate open
- Selects: Product Milestone 5 and the future `v0.2.0` product release
- Plan: [Windvale 0.2.0 connected-services release plan](../Project/Windvale-0.2.0-Connected-Services-Release-Plan.md)
- Builds on: [Decision 0590](0590-Offline-Package-Lifecycle-And-Generation-Activation-1.md)

## Context

The signed `v0.1.0` preview and the offline package lifecycle are complete.
Windvale can install exact packages, publish immutable generations, activate and
roll back, dispatch verified commands, and uninstall package-owned state while
preserving application data. The next preview needs a user-facing result large
enough to justify a new minor version rather than another internal checkpoint.

Three active product needs now converge:

- the database portion of EWorker Data Service is being rewritten as a native
  Windvale database service;
- Windows and Debian need a general way to register, operate, upgrade, and
  remove packaged services; and
- the installer and external-model adapter both need rights-limited secure
  Internet access.

Windvale OS continues to advance independently. Making every open OS mechanism
a release prerequisite would turn a host-services preview into an unbounded OS
milestone, while excluding already-qualified OS progress from the selected
source state would hide useful work.

## Decision

Select **Windvale 0.2.0: Connected Services Preview** as Product Milestone 5.
Do not create or publish the `v0.2.0` tag until the complete release gate passes.

The release requires all of the following:

1. a Windvale-native database service rewrite selected against one pinned
   EWorker Data Service reference baseline;
2. one portable service declaration and explicit `wv service` lifecycle mapped
   to Windows Service Control Manager and Debian `systemd`;
3. an online installer/package-client path over the shared secure networking
   contracts, backed by a signed official repository while retaining equivalent
   offline installation;
4. one provider-neutral external-model gateway with one real provider adapter;
5. modular signed Windows and Debian packages, safe upgrade/rollback/uninstall,
   current documentation, and one explicit exact-state release qualification;
   and
6. an accurately labelled Windvale OS snapshot or artifact only where its own
   named gate is ready at release freeze. Unfinished OS work does not block the
   host release and receives no broader claim.

The EWorker Data Service C# implementation is a separate reference, provenance
source, differential oracle, and feature-inventory input. Its managed source,
runtime, projects, and direct `dotnet` entry points do not return to Windvale
`main` and are not shipped as the `0.2.0` database service. “Full database
service” means the accepted required parity set against one exact external
baseline, not every past or future feature without inventory.

The first official online repository is static: small signed Root, Channel, and
Release metadata can be served from the Windvale website, while immutable
package artifacts can be release assets. Transport locations do not grant trust;
the installer admits exact signed identities, lengths, digests, targets,
capabilities, approvals, service declarations, freshness, and minimum-version
policy before publication or activation. Channel metadata is published last.

Service installation is explicit and elevated. Package installation alone does
not silently start a privileged background process. Removal unregisters the
service and preserves configuration, credentials, databases, backups, and other
application data by default. Data destruction is a separate explicit purge
operation and is never implied by rollback or ordinary uninstall.

The online repository and external-model gateway share resolver, network
authority, deadline/cancellation, secure-stream, trust, and bounded HTTP work.
They do not introduce separate package-specific or provider-specific ambient
download calls. Public Internet success is optional smoke evidence, never the
deterministic verification oracle.

## Consequences

- `0.2.0` becomes a large integrated release rather than a loose bundle of
  whatever happened to finish.
- The database, service manager, connected installer, and model gateway are all
  release blockers; their internal slices remain independently verifiable.
- The base installer stays small. Database, model gateway, Workbench, and OS
  artifacts remain separately selectable packages or release assets.
- The completed offline package lifecycle is retained and becomes the local
  publication/activation half of the connected installer.
- Arbitrary third-party repositories, silent automatic service start, ambient
  credentials, browser-held provider keys, public-Internet test oracles, and
  automatic retry of indeterminate mutations are excluded from the first
  connected profile.
- SQL, remote database compatibility, concurrent writers, old file/protocol
  compatibility, backup breadth, and other EWorker parity items are neither
  silently required nor silently discarded; the pinned parity inventory must
  classify them before the completion claim.
- Normal commits continue to run affected verification owners only. Complete
  Windows/Debian Qualification runs once for the selected release candidate.

## Reconsideration triggers

Revisit this decision if the external database baseline cannot be pinned or
licensed for reference, if the required parity inventory cannot be bounded, if
Windows and Debian cannot share one semantic service declaration, if secure
repository retrieval cannot preserve offline object identity, if credential
custody cannot keep secrets out of packages/logs/browser state, or if the four
required product lanes cannot be integrated without weakening an accepted
security or recovery contract.
