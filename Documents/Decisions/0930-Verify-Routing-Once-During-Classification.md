# Decision 0930: verify routing once during classification

- Date: 2026-09-02
- Status: Accepted and implemented
- Extends: [Decision 0929: reuse native planner initialization in routing verification](0929-Reuse-Native-Planner-Initialization-In-Routing-Verification.md)
- Replaces: routing-plan ownership in [Decision 0928: run shared development meta-verification once](0928-Run-Shared-Development-Meta-Verification-Once.md)
- Current contract: [native changed-file verification](../../Specifications/Windvale-Native-Changed-Verification.md)

## Context

The mandatory classification job verifies the complete routing contract before
any scope-specific job starts. A development source state then ran the same
verifier again in the mandatory Linux development job. Both executions used the
same checked-out commit and asserted the same platform-neutral contract.

After initialization reuse, GitHub Actions run `33692866971` measured the
routing verifier at 17.5 seconds inside Linux development. The classification
job also spent part of its 38 seconds on the same verifier. The second run could
not add host or product evidence.

## Decision

- The mandatory classification job owns routing-plan verification for every
  automatic source state.
- A Windows or Linux automatic development job may pass
  `-PlanVerificationInClassification` only because its required classification
  predecessor succeeded. It omits only the routing-plan verifier and still
  computes its changed-path plan, rejects gaps, and runs every selected owner.
- Linux continues to own the conditional GitHub-workflow verifier. A Windows
  development peer may pass `-GitHubVerificationOnLinux` and omit only that
  Linux-owned check.
- Both switches reject local use, non-development scope, and unsupported hosts.
  Explicit qualification never delegates either check.
- The aggregate gate continues requiring successful classification and every
  selected development job. A routing failure therefore remains blocking.

## Consequences

Automatic development runs execute the 264 native and 31 general routing cases
once instead of twice. Timing artifacts from development jobs describe only
checks and owners actually executed by those jobs. Local changed-file
verification remains self-contained and continues running the routing verifier
when selected.

## Reconsideration triggers

Return routing verification to a scope-specific job if classification stops
being a required predecessor, uses a different source identity, or no longer
fails closed before downstream jobs. Revisit the GitHub-verifier placement if
that check becomes unconditional or classification already owns its exact
contract.
