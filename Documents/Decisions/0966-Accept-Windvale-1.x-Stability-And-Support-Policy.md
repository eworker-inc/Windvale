# Decision 0966: accept Windvale 1.x stability and support policy

## Status

Accepted by the maintainer on 2026-09-25: accept the proposed 12-month policy
as written. This accepts the future release policy, not implementation,
qualification, release signing, or publication.

## Context

The Windvale 1.0 product gate needs explicit compatibility, support,
deprecation, and upgrade promises. The
[stability and support policy](../Project/Windvale-1.0-Stability-And-Support-Policy.md)
was proposed on 2026-09-22. Its support window required a maintainer decision
because overlapping minor lines create a continuing maintenance obligation.

## Decision

1. Accept that policy as written. Each official 1.x minor line receives
   correctness and security fixes for 12 months from its first official
   release. A patch does not extend the date. Fixes are supplied through the
   latest immutable patch of each affected supported minor line.
2. Preserve the policy's exact compatibility, deprecation, security-withdrawal,
   upgrade, rollback, data-preservation, and indeterminate-mutation rules.
   The qualified release matrix defines the supported artifact combinations;
   a product version alone does not imply component compatibility.
3. E-Worker Inc, the [project steward](../../GOVERNANCE.md), owns support
   obligations across overlapping minor lines and delegates implementation to
   maintainers. Review the policy before selecting the first 1.0 release
   candidate and no later than 2027-09-25, then at least annually. A review does
   not silently shorten an already published support window.
4. Publish each minor line's exact support-end date in its release notes and
   support page. Only official signed artifacts in the qualified matrix receive
   the promise. The preview, development builds, and unqualified combinations
   retain their existing terms. No response-time service level is introduced.

## Remaining release evidence

The shipped compatibility matrix, exact supported hosts, migration and rollback
limits, clean installation and removal, backup/restore, recovery, and offline
verification still need their release evidence. This decision closes policy
acceptance only. It neither declares Windvale 1.0 released nor authorizes
signing or publication.
