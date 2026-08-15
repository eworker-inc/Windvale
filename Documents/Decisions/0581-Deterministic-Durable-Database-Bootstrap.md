# Decision 0581: Deterministic durable database bootstrap

- Date: 2026-08-15
- Status: Implemented candidate with focused Windows execution evidence
- Requires: [Decision 0536](0536-Nested-Records-And-Database-Storage-Recovery.md)
- Defines: [database bootstrap](../../Specifications/Windvale-Database-Bootstrap.md)
- Retains: exact `WVDS 1`/`WVPG 1` validation, bounded publication actions,
  provider-generation fencing, and no uncertain mutation replay

## Context

The hosted durable-storage fixture manually encoded and wrote the initial root
and superblock. That proved the underlying format and provider, but it was not
a reusable database creation contract. A future database server needs one
deterministic way to turn a rights-limited empty storage object into an image
that the existing engine can reopen.

Creation also has a distinct crash-recovery problem. A process may stop after
the root write or after superblock publication. Blindly restarting from byte
zero could overwrite foreign data; blindly retrying an uncertain mutation
would violate the storage contract.

## Decision

- Add a portable bootstrap planner for one canonical generation-1, sequence-0
  image with a first-slot `WVDS 1` superblock and empty root `WVPG 1` page.
- Require a nonzero 128-bit database identity and one of the durable supported
  page sizes from 4 KiB through 64 KiB.
- Reuse the existing four-stage storage publication state machine and hosted
  executor instead of defining a second mutation protocol.
- Admit fresh creation only for an exactly empty storage object.
- On reopen, resume only an exact root with either a zero header or the exact
  canonical first header; repeat the corresponding durability flush.
- Reject every other nonempty object without mutation. Do not truncate a
  partial root or repair a partial/foreign header by inference.
- Return typed invalid-plan, not-empty, storage-failure, active, reopen-required,
  rejected, and created outcomes.
- Replace the hosted fixture's manual creation writes with the production
  bootstrap entry point.

## Evidence

The portable and hosted library projects compile through the current native
front door. Portable coverage decodes the planned records, compares repeated
plans byte-for-byte, advances all four publication actions, admits both exact
resume states, exercises the 64 KiB boundary, and rejects malformed or foreign
storage evidence. The existing provider-backed durable-storage scenario now
creates its real object through `Durableˉdatabaseˉbootstrap` before its repeated
restart and interruption checks.

## Consequences

- A future server has a reusable, rights-limited primitive for initializing an
  empty durable storage object without host-path authority.
- Initial image bytes and interruption behavior no longer live only in a test
  helper.
- Exact already-published bootstrap bytes are safely idempotent; ambiguous
  nonempty storage fails closed and requires operator policy outside this API.
- Database-storage grows from 23 to 24 retirement cases and from fourteen to
  fifteen development targets. With the independently added offline uninstall
  and metadata work, the rebased retirement inventory remains 68 suites and
  grows from 3,544 to 3,545 cases.
- Engine create-or-open orchestration, collection creation, server sessions,
  authentication, networking, concurrent writers, and storage-object lifecycle
  remain later milestones.

## Reconsideration triggers

Add explicit mutation identity before resuming any byte pattern that cannot be
proven canonical. Define a separate storage-object lifecycle capability before
the server creates, replaces, renames, or deletes host objects. Version the
bootstrap contract if the initial logical root begins carrying catalog data or
if a later durable format changes the first committed image.
