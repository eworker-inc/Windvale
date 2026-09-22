# Windvale 1.0 stability and support policy

> Status: Proposed for maintainer acceptance; no 1.0 support promise is active
> Authority: Informative until accepted by a numbered decision
> Last reviewed: 2026-09-22

## Outcome

This proposal gives a 1.0 user a concrete answer to three questions: which
artifact combinations may run, which changes require migration, and where a
supported defect or security report goes. Acceptance is part of the
[Windvale 1.0 product gate](Windvale-1.0-Product-Plan.md#windvale-10-release-gate),
not evidence that implementation or qualification has passed. The signed
`v0.1.0` preview and development artifacts keep their existing, narrower terms.

## Proposed compatibility boundary

An official release identifies a product version and a tested compatibility
matrix. The matrix names the source edition, Foundation API major versions,
WVB reader/writer versions, native target and ABI, package/lock/bundle/release
formats, capability interface majors, service protocols, and WVDB catalog and
durable-storage formats. Product version `1.x` does not rename or imply any of
those independent identities. A supported combination must be listed and
qualified on its claimed Windows and Linux hosts. Unknown required versions,
capabilities, targets, or format features fail explicitly before use.

The following promises are proposed only for combinations in that matrix:

| Surface | 1.x promise | Incompatible change |
| --- | --- | --- |
| Accepted edition-1 source | Same accepted source retains its meaning. A fix may reject code that was invalid under the published contract. | Select a new source edition and provide a migration guide; never reinterpret edition-1 input silently. |
| Public Foundation and capability APIs | Preserve published signatures, ownership, failure meaning, and authority within a major contract version. Additive APIs use explicit new names or versions when needed. | Publish a new major contract identity and show affected callers. A library requirement never grants a capability. |
| WVB, native images, and package artifacts | Readers admit only declared versions and features. Release manifests identify exact tools and target combinations. | Use an independently versioned successor; never infer authority or execute unknown bytes by fallback. |
| WVDB durable data | A supported release can open admitted formats or offers a documented, verified migration path before activation. User data survives failed upgrade and uninstall. | Publish source and destination format identities, preflight, full backup and restore, interruption recovery, and rollback limits before shipping the change. |
| Commands and diagnostics | Keep documented command meaning and stable diagnostic codes; human wording may improve. | Version or replace a command/protocol and publish a transition path. |

Compatible 1.x changes may add explicitly versioned features, increase a
documented limit after qualification, or fix behavior that violated an existing
contract. They must not silently narrow an accepted limit, change ordering or
mutation completion meaning, add ambient authority, or reinterpret stored data.
Security fixes that must restrict previously admitted behavior require a named
advisory, an affected-version statement, and a migration or mitigation path.

## Proposed support and deprecation practice

- Only official, signed release artifacts in the published compatibility matrix
  receive a release support claim. Source checkouts, local builds, candidate
  formats, and unqualified target combinations remain development inputs.
- Each minor line receives correctness and security fixes for 12 months after
  its first official release. A later patch does not extend that date. Publish
  the support-end date with the first release in the line and give notice before
  it expires. Multiple minor lines may be supported at once.
- Fixes are supplied in a new immutable patch release for each affected
  supported minor line. Only its latest patch receives new fixes; a superseded
  patch is supported by upgrading within that line. Publication of a patch does
  not mutate an installed artifact.
- A public API deprecation names the first affected release, the replacement,
  migration steps, and the earliest eligible removal major. Deprecation alone
  does not change valid 1.x behavior. Security withdrawal is disclosed
  separately with its narrower compatibility impact.
- Reports include the release and component identities, host/architecture,
  exact command or operation, bounded diagnostics, and a minimal safe
  reproducer. Public bugs and questions follow [SUPPORT.md](../../SUPPORT.md);
  vulnerabilities follow [SECURITY.md](../../SECURITY.md) privately.
- Release notes identify affected contracts, supported host combinations,
  security changes, format admission, migration steps, known limits, and exact
  offline verification instructions. A release is never described as supported
  solely because its tag exists.

The 12-month window is a **proposal**, not a commitment for `v0.1.0` or an
accepted 1.0 policy. Before acceptance, the maintainer must confirm the window
and ownership of fixes on overlapping minor lines. State each line's actual
support-end date in release notes and [SUPPORT.md](../../SUPPORT.md). Do not
promise a response-time service level without a staffed commitment.

## Proposed upgrade and data rule

An installer or service first verifies the signed immutable release and the
declared artifact matrix. It checks authority, storage format, resource limits,
and migration preconditions before changing active state. Package activation
and database migration are separate transactions: rollback of executable
activation does not imply a database downgrade. The upgrade instructions must
state whether old code can still read the post-migration database. When it
cannot, keep a verified pre-migration full backup and provide an explicit
restore path. Never overwrite a live database in place as a recovery shortcut.

Uninstall removes only installation-owned state. Application and WVDB data have
separate owners and require an explicit data-removal operation. An interrupted
install, activation, migration, backup, restore, or uninstall reports its exact
known or indeterminate state; uncertain mutations are not retried without an
idempotency rule. The 1.0 gate needs clean-install, update, rollback, migration,
restore, offline verification, and removal evidence on both supported hosts.

## Acceptance checklist

1. Accept or revise the proposed 12-month minor-line window and latest-patch
   rule, with an owner and a dated review trigger.
2. List exact shipped component/format/host combinations and implement
   preflight rejection for unsupported combinations.
3. Define every shipped durable-format migration and rollback limit, including
   WVDB backup/restore qualification.
4. Confirm the release, security, and support pages state the same promises;
   accept them in a numbered decision before the `v1.0.0` tag.
