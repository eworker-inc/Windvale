# Decision 0950: bundle compatible database ancestor cases

- Date: 2026-09-04
- Status: Implemented candidate with focused Windows execution evidence;
  Linux and complete paired qualification remain pending
- Extends: [Decision 0945](0945-Separate-Database-Behavior-From-Cross-Target-Packaging.md)
- Extended by: [Decision 0951](0951-Coalesce-Complete-Database-Bundles-In-Development.md)
  and [Decision 0952](0952-Rank-And-Trial-Overlapping-Database-Products.md)
- Preserves: all 57 database cases, distinct case labels, bounded portable
  execution, and fresh mutable behavior

## Context

The database work planner retained three identical dependency-closure groups
after the first publication/recovery and root-growth bundles. The three-case
branch-pages group had already been tried and rejected because its combined
product exceeded the native lowerer's declared output limit. The two remaining
groups each contained two portable cases with the same profile, authority,
source closure, process boundary, and no mutable host state.

Keeping each pair as two projects repeated construction, admission, linking, and
packaging without adding isolation. Removing either case would lose a distinct
behavior. A combined root can call both exported case functions and preserve
which member failed through separate return codes.

## Decision

1. Replace the separate ancestor-groups and depth-four qualification steps with
   one Project 2 product that imports and executes both original modules.
2. Replace the separate ancestor-pages and intermediate qualification steps with
   a second Project 2 product under the same rule.
3. Retain all four original case names in the qualification inventory and in the
   terminal step evidence. A combined process is one construction node, not one
   logical test.
4. Give each bundle project and bundle root its own two-case development
   selector. A change to the combined artifact must not run unrelated ancestor
   or database cases.
5. Do not bundle the branch-pages triple or raise its lowerer bound. Capacity is
   part of the compatibility test, and its failed trial remains evidence against
   that merge.
6. Keep portable reproducibility and cross-target packaging delegated to their
   focused owners under Decisions 0945 and 0946. Complete database qualification
   still runs all 57 behaviors on Windows and Linux.

## Evidence

The database planner now reports 56 execution steps, 57 cases, 60 project
references across 59 unique projects, and 690 root/source references across 144
unique paths. This is two fewer constructions and 21 fewer declared source
visits than the preceding 58-step graph. Only the rejected three-case
branch-pages closure remains as a static candidate.

Focused Windows qualification passed:

- `TransactionAncestorGroupsBundle` in 51,640 ms, reporting
  `TransactionAncestorGroups,TransactionAncestorGroupsDepthFour`;
- `TransactionAncestorPagesBundle` in 61,150 ms, reporting
  `TransactionAncestorPages,TransactionAncestorPagesIntermediate`.

Both nodes compiled the combined Project 2 input, lowered and admitted its WVO,
linked and packaged the current-host image, and executed both retained behavior
functions. No separate cold baseline was measured in this source state, so the
evidence supports the construction-count reduction but not an elapsed-time
percentage for these two pairs.

## Consequences

- The original 60-step database graph is now 56 steps while all 57 logical
  cases remain.
- Adding another behavior to either compatible bundle can reuse its product only
  while source closure, profile, authority, limits, and isolation remain equal.
- Failure remains attributable to the named bundle member, though both members
  share one compiler/linker process boundary.

## Reconsideration triggers

Split a bundle if one member needs a different profile, capability, resource
limit, process lifetime, mutable state, or host boundary, or if combined output
approaches a compiler, lowerer, linker, package, execution, or diagnostic bound.
