# Windvale 1.0 stability and support policy

> Status: Accepted policy for official 1.x releases; no 1.0 release is yet supported
> Authority: Normative under Decision 0966; release qualification remains separate
> Last reviewed: 2026-09-25

## Outcome

This policy gives a 1.0 user a concrete answer to three questions: which
artifact combinations may run, which changes require migration, and where a
supported defect or security report goes. The maintainer accepted it in the
[1.x support decision](../Decisions/0966-Accept-Windvale-1.x-Stability-And-Support-Policy.md)
on 2026-09-25. Acceptance closes the policy portion of the
[Windvale 1.0 product gate](Windvale-1.0-Product-Plan.md#windvale-10-release-gate).
Implementation and qualification remain separate. The signed
`v0.1.0` preview and development artifacts keep their existing, narrower terms.

## Compatibility boundary

An official release identifies a product version and a tested compatibility
matrix. The matrix names the source edition, Foundation API major versions,
WVB reader/writer versions, native target and ABI, package/lock/bundle/release
formats, capability interface majors, service protocols, and WVDB catalog and
durable-storage formats. Product version `1.x` does not rename or imply any of
those independent identities. A supported combination must be listed and
qualified on its claimed Windows and Linux hosts. Unknown required versions,
capabilities, targets, or format features fail explicitly before use.

The following promises apply only to combinations in that matrix:

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

## Support and deprecation practice

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

E-Worker Inc owns fixes on overlapping supported minor lines under the
[governance roles](../../GOVERNANCE.md#roles). Review this policy before the
first 1.0 release candidate and no later than 2027-09-25, then at least
annually; review does not shorten a published support window. State each
line's actual support-end date in release notes and [SUPPORT.md](../../SUPPORT.md).
The window does not apply to `v0.1.0` or current development builds. Do not
promise a response-time service level without a staffed commitment.

## Upgrade and data rule

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

## Release checklist

1. Policy accepted on 2026-09-25: 12-month minor-line window, latest-patch
   rule, E-Worker Inc ownership, and the dated review trigger above.
2. List exact shipped component/format/host combinations and implement
   preflight rejection for unsupported combinations.
3. Define every shipped durable-format migration and rollback limit, including
   WVDB backup/restore qualification.
4. Before the `v1.0.0` tag, confirm the release, security, and support pages
   state these accepted promises and the actual matrix and support-end date.
