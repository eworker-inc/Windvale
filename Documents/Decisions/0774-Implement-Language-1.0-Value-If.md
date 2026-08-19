# Decision 0774: Implement Language 1.0 value-producing if

- Status: Accepted
- Date: 2026-08-19

## Context

Decision 0767 freezes statement and value-producing forms of `if` and `match`.
The compiler already represents conditional control flow with explicit blocks,
branches, and a Boolean-only phi used by short-circuit operators. Creating a
second conditional expression pipeline or a WVB selector opcode would duplicate
control-flow semantics and could allow the source compiler, verifier, and
runtime to disagree about which arm executes.

Value blocks also introduce a binding boundary that ordinary statement blocks
did not previously expose: their final expression has no semicolon, may read
earlier arm-local declarations, and yields a value while all of those bindings
remain lexically confined to the arm.

## Decision

1. The body parser appends expression kind `If = 16`. A value `if` requires an
   `else`; the else arm is a braced value block or a recursive value `if`.
2. A value block contains zero or more ordinary statements followed by exactly
   one final expression without a semicolon. A missing value or a semicolon on
   the purported final expression is invalid.
3. The condition is evaluated exactly once and must be `bool`. At runtime only
   the selected arm is reached.
4. Reachable arms must produce the same exact type and ownership class. A
   `never` arm contributes no result and does not force a conversion or invented
   value in the surviving arm.
5. WVIR operation identity `64` is renamed from `Boolˉphi` to `Valueˉphi`.
   It retains its numeric identity and zero-byte backend role, but now accepts
   any exact non-void/non-never shape when both operands and the result agree.
6. Independent WVIR validation requires the phi to be the first operation of
   its join, two distinct unconditional predecessors that select its two
   operands, and no conditional or third predecessor targeting that join.
7. Arm-local bindings are retained in canonical binding evidence with scope
   ending at the arm's closing brace. The enclosing compiler path preserves the
   newest binding snapshot after lowering an initializer or return expression.
8. WVB emission writes the selected predecessor value into the phi-result local
   immediately before its ordinary jump. No new WVB opcode, format version,
   runtime value representation, or compatibility path is introduced.

## Evidence

The 24-assertion value-front-end self-test verifies the new stable expression
identity while the real compiler fixtures exercise complete spans, statement
prefixes, and final-expression boundaries. This avoids recursively interpreting
the parser beyond the bounded scalar runner's fixed guest call depth. The
positive `Value-Control.wv` fixture includes two branch-local declarations,
mutation, name-valued completion, and recursive `else if`. It compiles deterministically
to a 529-byte WVB with SHA-256
`c9b5cecdfb26478844dc8c6e6e97683758693d419fab36b360705eb99ff5d0e8`
and executes through the source-built scalar runner with result `42`.

`Value-If-Lazy.wv` puts an unbounded recursive call in the unselected arm. Its
350-byte WVB has SHA-256
`d18209374d076eea7ff9eb3bde6b2a71e7c01999cb91a01e3d154818e18aa386`
and returns `42`, proving lazy arm execution. Separate source fixtures reject a
missing `else`, a trailing semicolon where a value is required, mismatched arm
types, a non-Boolean condition, and descriptorless Seed use without publishing
WVB output.

The focused Windows language owner rebuilds the compiler once and reuses it for
the deterministic positive builds, executions, and negative cases. Heavy
storage, broad OS, paired-host, and complete Qualification gates are not repeated
for this checkpoint; the final Slice 2 integration gate owns broader evidence.
The owner passed all 128 cases in 365.93 seconds, including every retained
numeric, unit/never, and named-variant regression phase.

## Non-decision

This checkpoint does not implement value-producing `match`, destructuring beyond
the already accepted named-variant path, ownership/borrow analysis not yet
present in the active compiler, localized token execution, a public native ABI,
or complete Language 1.0.

## Consequences

Edition-1 value `if` now uses the same typed control-flow architecture as
statement `if`, Boolean short-circuiting, the WVB backend, and the scalar
runtime. The generalized phi is available to value-producing `match` without a
second bytecode mechanism. The next Slice 2 checkpoint is value-producing
`match` and its exhaustive typed joins.

## Reconsideration triggers

Reconsider the zero-byte phi lowering only if a future backend cannot preserve
exact predecessor-edge value selection with its ordinary branch and local
operations. Reconsider the two-predecessor WVIR form only through a versioned
IR decision with equivalent independent validation and deterministic bytes.
