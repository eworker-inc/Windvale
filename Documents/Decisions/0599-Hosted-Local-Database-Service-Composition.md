# Decision 0599: Hosted local database service composition

- Date: 2026-08-15
- Status: Implemented hosted milestone
- Defines: [hosted local database service](../../Specifications/Windvale-Database-Hosted-Local-Service.md)
- Refines: [portable local service](../../Specifications/Windvale-Database-Local-Service.md)
- Builds on: [Decision 0588](0588-Portable-Local-Database-Service-Contract.md), [Decision 0593](0593-First-Record-Durable-Root-Publication.md)

## Context

Windvale could prepare logical operations, publish a first durable record, and
read durable trees, but application code still had to compose those layers and
understand storage metadata. Combining the complete session, reader, root
writer, and multi-level writer exceeded the current bounded native object.
Cold verification also spent most of its time packaging compiler and
application artifacts rather than executing database operations.

## Decision

- Split the portable service into one shared contract plus focused session,
  put, get, and control modules; do not keep a second monolithic implementation.
- Add hosted open, depth-one put, and get adapters that hide provider metadata
  from logical callers.
- Keep put and get as separate native components under the current 4 MiB object
  contract. Do not increase a safety limit to hide unnecessary code inclusion.
- Map confirmed publication, rejection, uncertainty, and failure explicitly.
  Never replay a mutation after any provider action or uncertain outcome.
- Add a `host-local-service` verifier that performs put, process restart, get,
  missing lookup, and byte-for-byte read-only confirmation on real storage.
- Reuse content-addressed native project and application checkpoints and report
  storage setup separately from local-service work.

## Consequences

A caller can now store and retrieve one logical record through the local
service without knowing pages or superblocks. The read and write components fit
the current native format and keep unused code out of memory. The depth-one
writer still rejects a full root; routing to root split and multi-level update
is the next database milestone.

## Reconsideration triggers

Reconsider the process/component split when segmented native application
construction or measured dead-code elimination provides the same bounded
memory and faster verification. Reconsider the depth-one limit only by adding
an explicit dispatcher to the already verified multi-level writer paths.
