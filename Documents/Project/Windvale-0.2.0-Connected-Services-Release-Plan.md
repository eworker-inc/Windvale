# Windvale 0.2.0 connected-services release plan

> Status: Selected Product Milestone 5 under
> [Decision 0595](../Decisions/0595-Select-Windvale-0.2.0-Connected-Services-Preview.md).
> No `v0.2.0` tag or release exists yet.

## Release outcome

A person can bootstrap Windvale from the Internet on Windows x64 or Debian x64,
refresh the official signed repository, install separately packaged
applications and services, register and operate the native Windvale Database
Service, and use a local Windvale gateway to contact one external model
provider. Offline installation remains supported and produces the same admitted
objects and logical generation as online installation.

The release remains an experimental `0.y` preview. It does not establish 1.0
compatibility or make every compiler, database, networking, service, package,
model, or OS contract stable.

## Dependency map

```text
shared operation/deadline/cancellation and network authority
    -> Windows/Debian resolver + secure-stream providers
        -> bounded secure HTTP
            -> official online package repository
            -> external-model provider adapter

completed package/store/generation/activation lifecycle
    -> connected installer and package client
        -> portable service declaration
            -> Windows SCM adapter
            -> Debian systemd adapter
                -> native database service
                -> external-model gateway

Windvale OS work -> independent qualified snapshot at release freeze
```

Compiler, runtime, native-backend, library, and diagnostic work enters this
milestone only when one of these named consumers needs it. The release must not
grow merely because an unrelated experiment exists on `main`.

## Track A: WVDB specification and native service

### Product and specification boundary

WVDB is a Windvale-owned database specified by Windvale contracts and
implemented in Windvale Language 1.0. [Decision 0790](../Decisions/0790-Define-WVDB-1.0-As-A-Windvale-Owned-Database.md)
supersedes the earlier external rewrite and parity direction. The active
[WVDB 1.0 specification plan](WVDB-1.0-Specification-Plan.md) owns the design
sequence from user-visible models through storage, durability, service, and
operations.

Current formats and implementation are candidate mechanisms. They must be
reconciled with accepted WVDB specifications rather than becoming public 1.0
semantics by accident. Comparisons with established systems are research only;
they create no API, SQL, file, wire, runtime, or behavioral compatibility
promise.

The connected-services milestone may implement and exercise accepted WVDB
vertical slices, but it must not label a package or protocol `WVDB 1.0` until
the relevant normative specifications and conformance gate are complete.

### Required WVDB capability ledger

The accepted specification set and release profile, rather than an external
product inventory, own the final row count. At minimum the ledger must classify:

| Capability family | Current Windvale standing | `0.2.0` requirement |
| --- | --- | --- |
| Create, identity-gated open, close, reopen | Focused candidate | Complete hosted service behavior |
| Durable publication and crash recovery | Substantial bounded implementation | Complete for every admitted mutation path |
| Entities, entity sets/tables, schemas, and logical records | Portable catalog, strict schema, and typed-row contracts; atomic table lifecycle and migration missing | Exact selected WVDB profile |
| Record read, insert, replace, and delete | Read/write foundations; delete breadth unresolved | Required for the selected WVDB profile |
| Transactions and rollback | Single-writer publication foundations | Exact selected transaction profile |
| Relationships and integrity | No accepted reference/foreign-key or first-class relationship contract | Specify exact selected relationship forms |
| Indexes and queries | Portable ordered-index planning, typed query IR, and SQL lowering; hosted enforcement, planning, and execution missing | Specify the selected access paths; no silent omission |
| Client sessions and server protocol | Portable sequential session only | Versioned bounded service contract |
| Concurrent clients and writer policy | Not ready as a general contract | Exact finite release profile |
| Authentication and authorization | Capability foundations | Explicit client/service authority |
| Configuration, health, status, diagnostics, logs | Not productized | Required operational surface |
| Backup, restore, inspection, repair | Inventory required | Full backup/restore is required before a WVDB 1.0 production claim; any earlier preview must state its exact incomplete standing |
| Format migration and minimum compatible version | Not complete | Required before upgrade claim |

### Database completion gate

The database release gate requires:

- one long-running native service package on Windows and Debian;
- exact create/open/reopen identity and page-size policy;
- versioned bounded requests, responses, sessions, limits, and failure results;
- the selected entity/table/relationship/query/transaction profile;
- one exact writer policy and the selected finite client-concurrency profile;
- recovery after interruption at every durable transition;
- rejection of corrupt, incompatible, stale, oversized, unauthorized, and
  resource-exhausting inputs without guessing or replaying uncertain mutation;
- configuration, health, status, structured diagnostics, and host logs;
- the selected backup/restore/repair/migration obligations from the WVDB
  ledger; and
- paired-host behavior plus exact format/protocol compatibility evidence for
  every compatibility claim actually made.

Existing database specifications remain implemented-candidate inputs until the
WVDB 1.0 program reconciles, supersedes, or incorporates them. The historical
database proposal remains provenance, not active product direction.

## Track B: portable host-service lifecycle

### User commands

The initial command family is:

```text
wv service install <package> --system [--start]
wv service start <service>
wv service stop <service>
wv service restart <service>
wv service status <service>
wv service logs <service>
wv service remove <service>
wv service remove <service> --purge-data
```

`install --system`, host registration, and removal require explicit elevation.
The command must diagnose insufficient authority before partial registration.
Package installation and service registration are distinct; service start is
explicit unless `--start` is supplied.

### Portable service declaration

A signed package service declaration binds:

- canonical service identity and display description;
- exact installed package command and target;
- arguments, working directory, and environment names without secret values;
- separately owned configuration, data, backup, runtime, and log locations;
- startup mode, dependency order, restart policy/limit, and stop timeout;
- required capabilities, account/profile, network grants, and health command;
- upgrade/restart and rollback compatibility policy; and
- uninstall ownership plus explicit purge eligibility.

The declaration is not permission to install, elevate, start, access a secret,
open a listener, or contact a peer. The administrator approves those operations
and the host adapter binds rights-limited instances.

### Host adapters

- Windows uses the Service Control Manager and a real service executable/control
  protocol. It handles stop-pending and marked-for-deletion states explicitly.
- Debian uses a native `systemd` unit, daemon reload, enable/disable, service
  status, journal access, and distribution-appropriate executable/data paths.
- Both hosts map the same portable intent to host-native mechanics and report
  semantic outcomes rather than pretending their configuration stores match.

Removal stops and unregisters the service but preserves configuration,
credentials, databases, backups, and separately owned application data. Purge
is a distinct destructive operation with exact targets and confirmation.

Upgrade activates a new immutable package generation only after admission.
Service restart and health evidence follow activation. Rollback may reactivate a
compatible retained generation; it never rewrites data or silently crosses a
database-format minimum-version boundary.

## Track C: connected installer and official repository

### First repository profile

The first repository is public, read-only, and official-only. It requires no
custom paid object store or always-on registry server:

- small signed repository metadata is published under the Windvale website;
- immutable platform packages are attached as release assets; and
- private signing keys remain offline from both hosts.

The canonical metadata hierarchy is:

```text
Root policy -> Channel -> Release -> package/lock/bundle/approval/launch/service objects
```

Root metadata binds delegated keys and policy. Channel metadata selects one
release serial and freshness window. Release metadata binds every immutable
object location, length, digest, target, capability closure, approval, service
declaration, minimum compatible Windvale version, and revocation state.

HTTPS authenticates transport peers; it does not replace signed metadata.
Redirects, mirrors, website compromise, release-host compromise, stale metadata,
and substituted bytes remain untrusted inputs. No network response may grant
authority or select an undeclared object.

### Client commands

```text
wv repo update
wv search [text]
wv install <package>
wv upgrade [package]
wv rollback <package-or-generation>
wv uninstall <package>
```

### Application commands and standalone hosts

The `0.2.0` client must make installed applications usable without requiring a
person to type the general `wv` front door for every invocation. The release
therefore includes the following bounded command-launch slice:

- package and install Echo's existing standalone Windows PE and Linux ELF hosts;
  those native applications do not load `wv`, .NET, or a WVB interpreter after
  launch;
- implement `wv run echo -- <arguments>` through the active generation as the
  explicit universal form;
- install one non-conflicting per-application shim, initially `wv-echo`, that
  contains only the canonical `echo` command identity and delegates to the same
  verified native launcher;
- replace the development Node.js dispatcher with the installed native launcher,
  resolve bundle, approval, launch, and host identities from the durable object
  store rather than caller-supplied paths, and preserve exact argument limits,
  capability reduction, private-host publication, exit status, and cleanup;
- prove direct standalone execution separately from protected installed-command
  execution on Windows and Debian; and
- keep exact `echo hello` syntax as a Windvale Shell command-resolution result.
  PowerShell, CMD, Bash, and common Unix shells already reserve or alias `echo`,
  so replacing their builtins is neither a portable installer promise nor a
  `0.2.0` host-release requirement.

The shim removes `wv` from the user's command line, not from the security model.
It cannot point directly at a mutable package directory, carry package-manager
authority, or bypass active-generation, package, approval, executable, target,
capability, argument, and resource admission. Once the admitted native host is
started, the application is independent of the launcher for its execution.

The bootstrap scripts remain the first-install convenience entry points. They
download the bounded verifier and signed release inputs, establish the current
terminal `PATH`, and install the core generation. The installed `wv` client then
owns discovery and package operations. Arbitrary URLs and third-party repository
addition are outside the first profile.

### Download and publication rules

The client must:

1. bind the embedded/pinned official root and caller freshness policy;
2. admit signed Root, Channel, and Release metadata within exact limits;
3. select only the declared host target and capability/service closure;
4. stream no more than the declared object length into a private location while
   computing its digest;
5. reject truncation, excess bytes, substitution, stale/frozen metadata,
   unsupported redirects, cancellation, timeout, and provider loss explicitly;
6. admit the complete package/bundle/approval/launch/service graph offline;
7. publish immutable content, construct a new generation, and activate only
   after complete success; and
8. leave the old activation usable after any pre-publication failure.

Online and offline routes must yield byte-identical admitted objects and the
same logical Generation record. Resume support is optional; unsafe partial reuse
is forbidden. Indeterminate mutations are not retried without a specified
idempotency identity.

### Publication order

```text
build -> verify -> sign immutable objects -> upload private/draft assets
      -> independently fetch and verify hosted bytes
      -> sign Release -> publish Release -> publish Channel last
```

Users must never discover a release whose selected objects are incomplete.
Published identities and version tags are immutable; a correction receives a
new version or channel serial.

## Track D: shared network and external-model gateway

The repository client and model gateway share:

- the implemented bounded operation/deadline/cancellation core;
- the implemented-candidate strict address, endpoint, peer, and
  network-authority values selected under Decision 0594;
- rights-limited Windows and Debian resolver/connect, monotonic-timer,
  secure-entropy, trust, and secure-stream providers;
- bounded HTTP framing, response-body limits, redirect/downgrade policy, and
  exact partial/indeterminate completion; and
- deterministic local peers, virtual time, fixed entropy, malformed inputs,
  loss, truncation, reset, timeout, cancellation, and teardown tests.

The public Internet is an opt-in smoke route. It is never the qualification
oracle and no credential is available to ordinary repository CI.

The model gateway uses the existing provider-neutral protocol and bound-provider
facade. The first live adapter may target OpenAI, but the portable request,
catalog, inference result, usage, failure, and completion contracts remain
provider-neutral. Version 1 requires model discovery and one bounded text
request/result path with explicit model selection, timeout/cancellation, limits,
rate/rejection/provider-loss/malformed/indeterminate results, and no silent
fallback or uncertain retry.

Provider credentials are bound explicitly to the hosted gateway. They are never
committed, packaged, logged, stored in browser state, placed in a database
record, or sent to Workbench. Environment-variable binding is acceptable for
the preview only through an explicit host configuration name; later production
custody may use a separately qualified secret provider.

Streaming, tool calls, images/audio, background inference, multiple simultaneous
providers, automatic fallback, and agent autonomy are outside the minimum
release gate unless separately accepted without delaying the required path.

## Track E: packaging and release artifacts

Keep downloads modular. The release may publish:

- small Windows and Debian core installers;
- native Windvale Database Service packages per host;
- external-model gateway packages per host;
- Workbench as an experimental package when its local gateway path is ready;
- optional experimental Windvale OS images/artifacts that passed their own
  named gates at release freeze;
- source, provenance, licenses, SBOM/inventory, release metadata, public keys,
  offline verifier, qualification reports, and the signed release envelope.

The database and model gateway do not become implicit base-install payloads.
Users select them from the official repository.

## Windvale OS inclusion policy

OS work continues throughout the milestone on its independent roadmap. The
`v0.2.0` source tag naturally identifies the accepted OS source present at
freeze. A downloadable OS image or additional product claim is included only
when its exact build/boot/safety gate is green for that state.

Unfinished shell, networking, driver, launch, supervision, storage, or service
work does not block the host release and is not described as implemented.
Ready OS work is documented accurately rather than excluded merely because it
was not a release dependency.

## Release stages and tags

Ordinary implementation commits do not receive product tags. Internal artifacts
may use `0.2.0-dev.<n>` labels. Public integration checkpoints may use immutable
annotated prerelease tags only when they publish inspectable artifacts:

- `v0.2.0-alpha.<n>` for incomplete connected-service demonstrations;
- `v0.2.0-beta.<n>` after all required product lanes work end to end;
- `v0.2.0-rc.<n>` for an exact frozen qualification candidate; and
- `v0.2.0` only after the final candidate passes and its release envelope is
  signed.

A failed candidate is never retagged or moved. Fixes produce a new prerelease
number and one new exact source state.

## Completion gate

Milestone 5 and `v0.2.0` complete only when one exact source and artifact set
proves all of the following:

1. the accepted WVDB specification baseline and release capability ledger are
   pinned;
2. the required native database service profile passes on Windows and Debian;
3. `wv service` installs, starts, stops, restarts, reports, upgrades, rolls back,
   unregisters, and data-preservingly removes real database and model services;
4. both host adapters reject unsafe identities, paths, authority, stale state,
   incompatible rollback, partial registration, and ambiguous removal;
5. the official signed repository publishes immutable core, database, and model
   packages with Root/Channel/Release freshness and revocation policy;
6. a clean online bootstrap and installed client discover, download, admit,
   install, activate, upgrade, roll back, and uninstall without ambient trust;
7. the equivalent offline route yields identical admitted objects and logical
   generations;
8. Echo runs directly as a standalone native host, through `wv run echo --`, and
   through its installed protected shim; the latter two routes resolve the same
   active command and enforce the same approval, launch, capability, argument,
   identity, exit-status, and cleanup contracts without a production Node.js
   dispatcher;
9. one real external-model adapter lists visible models and completes one
   bounded text request through the local gateway without exposing its key;
10. deterministic isolated tests cover hostile network, repository, service,
   database, model, interruption, cancellation, and teardown cases;
11. installer and service removal preserve application data by default and an
    independently checked purge removes only explicitly owned targets;
12. release documentation names every experimental compatibility boundary and
    the exact ready Windvale OS standing; and
13. the frozen source state passes the deliberate complete Windows/Debian
    Qualification gate before signing and publishing `v0.2.0`.

## Explicit non-goals

The first connected release does not require a paid object store, custom dynamic
registry server, arbitrary third-party repositories, automatic background
updates, silent service start, universal Linux distribution support, ARM64,
multi-provider routing, browser-held API keys, a complete Windvale OS, or 1.0
compatibility. Any of those may advance independently but cannot weaken or make
ambiguous the selected gate.

## Verification discipline

Each implementation slice owns deterministic fixtures and one focused native
verification owner. Comparative database research does not become a build,
runtime, or conformance dependency for normal Windvale verification.
Repository and model tests use isolated peers and fake credentials. Service
tests use disposable exact registrations and verify cleanup/data preservation.

Ordinary commits run affected owners only. A passing owner result is reused
while its inputs remain unchanged. The complete cold cross-host matrix is run
once per deliberate release candidate rather than after every integration
commit.
