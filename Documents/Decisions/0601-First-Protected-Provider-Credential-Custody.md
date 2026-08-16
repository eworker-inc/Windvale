# Decision 0601: First protected provider credential custody

- Date: 2026-08-15
- Status: Implemented candidate with isolated Windows evidence
- Advances: external-model gateway credential boundary
- Contract: [protected provider credential](../../Specifications/Protected-Provider-Credential.md)
- Builds on: [Decision 0600](0600-First-Bounded-Https-Client.md)

## Context

The shared HTTPS client deliberately receives no credential. External-model
providers nevertheless require API keys, and passing those keys through model
messages, portable Windvale arguments, environment variables, loggable strings,
or caller-controlled header maps would create ambient authority and uncontrolled
plaintext copies.

The first gateway needs an executable custody contract now, before provider JSON
and supervision are added. It must work on both development hosts without
pretending that JavaScript memory erasure is an operating-system keystore.

## Decision

- Add a bounded encrypted `WVSC 1` wrapper using scrypt and AES-256-GCM with
  explicit algorithm parameters, sizes, random identity, salt, and nonce.
- Authenticate the exact provider, credential generation, provider-fixed API
  service, implied HTTPS port 443, format geometry, and cryptographic profile
  with the ciphertext.
- Admit only the OpenAI, Anthropic, and Google authorization profiles and exact
  provider services selected by an operator-created binding.
- Expose metadata-only inspection and a private mutable lease. Never expose the
  decrypted credential or provider-owned authorization field.
- Require exact expected generation before constructing a network provider;
  reject stale use, caller authorization fields, origin changes, and delegated
  provider-header authority.
- Erase maintained plaintext buffers through credential creation/unlock,
  authorization construction, HTTPS request assembly, child serialization, and
  provider write admission. Destroying a lease revokes every derived client.
- Use generic unlock failure for wrong passphrase, authenticated tampering, and
  decrypted-plaintext rejection. Keep malformed public structure and operator
  creation failures separately diagnosable without raw host errors.
- Treat this as a hosted bootstrap custody mechanism. The supervised gateway
  process will own the lease; OS keyrings or HSMs may later supply the same
  rights-limited lease without changing model or HTTPS contracts.

## Consequences

The next gateway can receive an encrypted wrapper and operator-supplied unlock
material over a supervised startup channel, keep provider authority out of model
envelopes, and perform fixed-origin HTTPS through the already shared stack.
Credential rotation is an explicit generation change rather than silent value
replacement.

The wrapper does not make a weak passphrase strong, protect an already
compromised host, guarantee erasure inside Node/OpenSSL/the kernel, provide
multi-user ACLs, or define backup and recovery. Production promotion still
requires independent Linux evidence, a protected startup channel and process
lifecycle, log redaction, rotation/revocation policy, and the supervised gateway.

## Reconsideration triggers

Revisit the at-rest profile when platform keyrings, TPM/HSM-backed wrapping,
non-exportable provider credentials, multiple active generations, recovery
escrow, or another provider authentication scheme is qualified. A new profile
or format version is required if cryptographic parameters, origin semantics, or
persisted geometry change.
