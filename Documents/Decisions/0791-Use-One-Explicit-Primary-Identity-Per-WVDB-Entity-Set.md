# Decision 0791: Use one explicit primary identity per WVDB entity set

- Date: 2026-08-20
- Status: Accepted direction; exact key types, generation algorithms, and
  serialized forms remain specification work
- Builds on: [Decision 0790](0790-Define-WVDB-1.0-As-A-Windvale-Owned-Database.md)
- Design register: [WVDB 1.0 upper-layer decisions](../Project/WVDB-1.0-Upper-Layer-Decision-Register.md)

## Context

Every durable WVDB entity needs stable identity for direct access, uniqueness,
relationships, mutation, migration, and diagnostics. The first design question
was whether WVDB should expose one declared identity or keep both a user-visible
primary key and a second opaque entity or row identity.

Two identities can make natural-key changes and some physical layouts easier,
but they also create ambiguity: applications, references, caches, imports, and
diagnostics must continually decide which identity is authoritative. A hidden
row identity can also become accidental public behavior.

One explicit identity is simpler if WVDB permits a database-generated value
when an application has no suitable stable natural key. Physical page, slot,
tree, or segment addresses can remain internal implementation details.

## Decision

Every WVDB entity set declares exactly one **primary identity**. The table
profile presents that identity as its **primary key**.

The primary identity:

- consists of one or more declared, non-null, typed fields;
- has one canonical equality, ordering, and serialized-key contract;
- is immutable for the lifetime of an entity;
- is the default target of references and first-class relationships;
- may use application-supplied values or a WVDB-generated value admitted by
  the eventual type and generation specification; and
- is distinct from physical addresses, which are never logical identities.

WVDB does not expose a second automatic row identifier when a declared primary
identity already exists. A table cannot be keyless in the initial WVDB 1.0
table profile.

A requested primary-identity change is not an in-place field update. It is a
delete of the old entity and insertion of a new entity within an explicit
transaction, subject to reference and relationship rules. WVDB must not
silently retarget relationships.

Natural or business identifiers that are not selected as the primary identity
remain ordinary fields and may receive named unique constraints. This lets an
application choose a generated stable identity while still enforcing a
human-meaningful code, email address, or other candidate key.

The current opaque record-identity bytes remain an implementation candidate.
They become the canonical encoding of the declared primary identity only if
the WVDB 1.0 format specification accepts that mapping.

## Consequences

- Every entity is addressable without relying on a hidden row number.
- References, relationship records, mutations, and diagnostics have one
  authoritative identity.
- Applications without stable natural keys need an exact generated-identity
  facility.
- Wide compound primary identities can enlarge indexes and relationships, so
  the specification must state key-component and encoded-size bounds.
- Immutable identity prevents surprising cascaded key updates but makes an
  intentional identity replacement explicit.
- Physical storage remains free to use clustered, heap, tree, columnar,
  adjacency, or other addresses without exposing them as user semantics.

## Required follow-up

The types-and-values and entities-and-tables specifications must still decide:

- admitted primary-identity field kinds and encoded-size limits;
- the maximum number of primary-key fields;
- the generated-identity kind and allocation algorithm;
- whether callers can reserve or supply generated identities before mutation;
- collision, exhaustion, rollback, import, and uncertain-completion behavior;
  and
- whether a later profile may explicitly reference a named unique candidate
  key instead of the primary identity.

## Reconsideration triggers

Revisit this decision if a qualified workload demonstrates that one explicit
identity cannot support required imports, graph merges, offline creation, or
physical locality without a second durable identity. Any second identity must
have a distinct name and exact authority; it must not be introduced as an
invisible compatibility row identifier.
