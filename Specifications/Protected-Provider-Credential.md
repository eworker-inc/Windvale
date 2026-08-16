# Windvale protected provider credential version 1

## Status and purpose

The hosted bootstrap custody boundary selected by
[Decision 0604](../Documents/Decisions/0604-First-Protected-Provider-Credential-Custody.md)
protects an external-model API credential at rest, binds it to one provider and
HTTPS origin, and injects it only inside the shared
[bounded HTTPS](Bounded-Https.md) request path. It is not a general secret store,
password manager, operating-system keyring abstraction, or permission for a
portable application to observe authorization headers.

Version 1 supports the `openai`, `anthropic`, and `google` provider identities.
Their exact authorization fields are respectively `authorization: Bearer`,
`x-api-key`, and `x-goog-api-key`. Each provider fixes its canonical production
API service (`api.openai.com`, `api.anthropic.com`, or
`generativelanguage.googleapis.com`) and HTTPS port 443. A caller and wrapper
creator cannot change that origin or supply, replace, or delegate the
provider-owned field.

## WVSC 1 protected wrapper

The persisted form is one bounded binary `WVSC` version 1 wrapper. Its fixed
160-byte little-endian header records total/header sizes, provider code,
nonzero credential generation, service and credential sizes, the exact KDF and
cipher profile, a random public identity, salt, nonce, and authentication tag.
The canonical ASCII service and ciphertext follow the header. Reserved bytes
are zero and the complete wrapper is at most 1,437 bytes.

The fixed cryptographic profile is:

- scrypt with `N=131072`, `r=8`, `p=1`, a 32-byte salt, a 32-byte derived key,
  and a 256 MiB implementation memory ceiling;
- AES-256-GCM with a 12-byte nonce and 16-byte tag; and
- 16-byte nonzero random identity, 32-byte nonzero salt, and 12-byte nonzero
  nonce from the injected secure entropy source, with no weak fallback.

The authenticated associated data is the canonical header with a zero tag plus
the exact service bytes. It binds provider identity, generation, service,
lengths, algorithm choices, identity, salt, and nonce. Changing any binding or
ciphertext makes unlock fail. Provider identity also fixes HTTPS port 443 in
this format version.

Credential plaintext is 16 through 1,024 printable non-space ASCII bytes.
Passphrase input is 16 through 1,024 bytes of strict UTF-8 without NUL. Creation
copies caller input, erases every temporary credential, passphrase, and derived
key buffer on success or failure, and returns only the encrypted wrapper.
It never discovers a credential from process arguments, environment variables,
source files, repository configuration, or ambient provider SDK state.

## Inspection, unlock, and lease

Unauthenticated structural inspection validates every fixed field and checked
length before KDF work. It returns only provider, exact service, implied port
443, generation, public identity, and plaintext byte count. It never returns
ciphertext as text, a passphrase, a credential, an authorization value, or
host-crypto diagnostics.

Unlock authenticates and decrypts into a private mutable lease buffer. Wrong
passphrase, authentication failure, or invalid admitted plaintext returns the
single `unlock_failed` result. Public malformed geometry returns
`invalid_wrapper`; invalid creation inputs, entropy failure, and unavailable
creation crypto remain distinct operator-facing failures.

A lease exposes only public binding metadata and two authority operations:

- `bindHttps` creates a bounded client fixed to the authenticated service,
  port 443, provider generation, trust generation, target set, header allow-list,
  and byte/deadline limits; and
- `destroy` zeroes the private credential buffer and permanently revokes the
  lease and all clients already derived from it.

Every request must present the exact expected credential generation before any
network provider is constructed. The lease constructs the provider-specific
authorization bytes internally, passes them as a mutable buffer into bounded
HTTPS, and erases that buffer afterward. The HTTPS request, serialized child
write frame, and decoded provider write payload are also explicitly erased
after local acceptance. No automatic retry occurs, including after uncertain
partial mutation submission.

Explicit buffer erasure bounds plaintext lifetime in the maintained JavaScript
objects; it cannot prove erasure of copies retained inside the host VM, crypto
library, kernel, or device. Production process isolation therefore requires the
supervised model gateway to own the lease and terminate on revocation, provider
loss, or credential replacement. A later OS-keyring/HSM provider may replace
the at-rest wrapper and unlock mechanism without widening the lease contract.

## Executable evidence

`Test-Protected-Credential` owns 16 isolated cases for all three authorization
profiles, encrypted metadata-only inspection, caller-buffer preservation,
temporary request erasure, wrong-passphrase and authenticated-tamper collapse,
malformed geometry, passphrase/credential/entropy bounds, stale generation,
authorization-field denial, port-443 origin fixation, and idempotent revocation.
It uses fake credentials and an isolated HTTPS supervisor, writes no plaintext
credential file, performs no public-network call, and exports no secret.

The owner has independent Windows and Linux execution evidence for the same
source. OS keyrings, HSMs, interactive prompting, rotation publication,
recovery, backup, and launcher-owned long-term operational custody remain
separate work. The supervised gateway now owns the live lease in its child.
