# Decision 0975: Launch owned-storage console applications

## Status

Proposed implementation contract, 3 October 2026. This candidate closes the
ordinary-startup integration slice within the existing ownership-to-storage
priority. Acceptance, independent reconstruction, installed qualification and
the complete memory path remain separate gates.

## Problem and result

Compiled scalar Vector programs require ABI 24 and context 10. Existing console
format one supplies context 7, and hosted format two owns a different service
contract. Manually initialized test harnesses therefore cannot establish that
ordinary packaging and startup support these programs.

Candidate console format three supplies a fresh fixed owned-storage domain on
Windows x64 and Linux x64. Its shared native entry initializes the existing
allocator and accounting leaves, invokes the generated Main, closes any domain
left open on return and checks terminal accounting. The existing portable
planner, container constructor and admission verifier own its canonical bytes.
The existing native publication transaction retains destination mutation.

## Version boundary and scope

The [owned-console specification](../../Specifications/Windvale-Native-Owned-Console-Application.md)
owns format-three placement, finite limits, startup and refusal behavior.
WVCQ, WVCP and WVCC version two select that container using the existing platform
target values. Earlier request/recipe and container bytes retain their exact
contracts. Hosted format two is not repurposed.

The normal `Package-Console --owned` command consumes a Core ABI-24 object with
a native Main returning i32, links the shared runtime and builds the selected container.
Current-source tool construction consumes a separately prepared compiler cache;
it does not reconstruct the compiler implicitly. Structural native-container
admission does not prove arbitrary native code safe or supply missing byte/text
backing, hosted services or broader ownership support.

## Evidence and remaining work

Extend the existing console/native owners for ordinary compiled-program startup,
success and trap teardown, malformed canonical bytes, publication refusal,
determinism and preserved earlier formats. Exact commands, tested revision and
results belong in the commit summary; do not add a competing container writer
or a replay-only verification entry point.

Shared immutable backing, broader borrowed helpers and aggregates, bounded
interpreter working storage and the maintained typed Package-Lock consumer remain
open. This candidate is not a released or fully qualified 1.0 product.
