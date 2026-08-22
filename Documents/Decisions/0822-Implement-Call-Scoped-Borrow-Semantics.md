# Decision 0822: Implement call-scoped borrow semantics

## Status

Accepted on 2026-08-22 as a prerequisite Language 1.0 compiler checkpoint.
Current evidence is focused Windows development evidence. Complete
Language 1.0 front-door execution, paired-host conformance, and release
qualification remain separate gates.

## Context

The frozen grammar already distinguishes by-value parameters, `borrow T`,
`borrow mut T`, and borrow unary expressions. The parser and typed compiler did
not implement those forms. Foundation collection signatures depend on them,
especially immutable observation and exclusive mutation of owned collections.

Borrow modes are static source facts. Encoding them as WVIR operations, WVB
opcodes, or runtime objects would add work without preserving any semantic fact
needed after successful checking. The compiler also sits at a bootstrap
capacity boundary: its native x64 lowerer admits at most 128 nominal types.

## Decision

The lexer assigns stable appended token kinds to `borrow` and `mut`.
Declaration parsing accepts the frozen parameter and result prefixes, body
parsing accepts immutable and mutable borrow unary expressions, and bare `mut`
is invalid. Source type binding skips the prefix and binds the exact underlying
type.

Typed WIR rereads validated function signatures and keeps by-value, immutable,
and mutable modes as bounded `u32` facts in its transient signature model. Call
arguments require exact mode agreement, except that a borrowed Copy or shared
immutable value may satisfy a by-value formal under the frozen read-through
rule. Mutable formation requires a direct `var` local or an already mutable-
borrowed parameter. A standalone borrow expression is rejected so it cannot be
stored as an ordinary value.

Borrowed results are parsed but rejected until the compiler represents the
frozen one-borrowed-parameter provenance rule. This checkpoint likewise does
not claim complete move invalidation, overlapping-borrow lifetime analysis,
aggregate-derived ownership, resource cleanup, or task/capture interaction.

Borrow modes are erased before WVIR and WVB publication. No serialized format,
operation, opcode, ABI, or runtime representation changes.

Two nominal compiler enums used by the first implementation raised the emitter
from 128 to 130 types and made native staging report `Unsupportedˉmodule`.
Because the facts are bounded internal tags rather than user-visible nominal
types, named `u32` constants replace those enums. The emitter returns to exactly
128 types; the native format bound is not weakened.

## Consequences

One six-function program proves immutable borrow, mutable borrow from `var`,
mutable reborrow, and explicit plus parameter-derived Copy read-through. It
emits an 857-byte WVB at SHA-256
`deef20a9559e7930d37eb62d973e2e95a4e0e328d8dfdb0837321d389985ed69`,
passes compiler-aligned verification, and reports `Result: 42`. Six negative
programs report exactly `Invalidˉborrow` for omitted explicit borrow,
immutable-to-mutable use, mutable borrow from `let`, standalone storage, and
borrowed return, or attempting to move an owned aggregate through a borrow.

The current analyzer is 1,088,695 WVB bytes at SHA-256
`4b5692c0caa9b53126b5461cc1c09fedcd7a716d4ed7f14f28abc9d80248ce58`.
The current emitter is 1,002,147 WVB bytes at SHA-256
`5601ff3d80f8babcc8ef3ecd5615e56729d4905ae7884606b61270d0efc3ecdc`.
Their Windows development applications package successfully at 34,402,816 and
22,080,512 bytes respectively.

Direct borrowed-parameter arguments recover their mode from the declaration
offset already carried by the transient local match. A first implementation
reparsed the entire current signature at each call and failed while analyzing
the large self-hosted emitter source set. The local bounded lookup preserves the
rule and lets the 1,940,645-byte emitter source set publish successfully.

The focused parser owner grows by 12 cases and the Language 1.0 front door grows
by seven semantic cases. The 108-owner registry now declares 5,133 cases under
SHA-256
`37b044d13ba09b34e9cc4d38dbf7e41fb190b84e773579af47352598fa921737`.
Changed-file planning passes 31 general and 194 native routing cases and keeps
the parser fixture separate from the heavier semantic front door.

## Reconsideration triggers

Replace the call-scoped checker with complete ownership dataflow when owned
vectors, maps, arenas, builders, resources, captures, or tasks first require
longer-lived borrow state. Admit borrowed results only with exact one-owner
provenance and mutability validation. Revisit the native 128-type encoding only
through a versioned lowering change with full malformed-input and bootstrap
evidence, not to carry static compiler tags as nominal program types.
