# Windvale hosted model chat command version 1

## Status and purpose

The existing Node-hosted credential, catalog, and bounded-chat command is
implemented. The Windvale-owned interactive application and terminal-input
extension described below are accepted migration candidates pending current
split-compiler construction and native host execution. Until that evidence
passes, the hosted Node command remains the supported user-facing path.

This specification defines the first user-facing command over the supervised
[external-model gateway](Supervised-External-Model-Gateway.md). It lets a person
create and inspect one protected provider credential, list the models visible to
that credential, and hold a bounded multi-turn text conversation with one exact
model. Credential custody and model-catalog administration remain in the hosted
launcher. The interactive chat, commands, exact model request construction,
typed model failures, and bounded history are migrated into
`Applications/Model-Chat/Windvale-Model-Chat.wv`. Windows and Linux launch
scripts select that application only after it has been published by the
separate reproducible construction entry point; otherwise they retain the
implemented hosted chat. No ordinary chat launch hides a cold compiler build.

The command supports the gateway's exact `openai`, `anthropic`, and `google`
bindings over authenticated HTTPS. It is not a provider router, automatic model
selector, streaming client, agent/tool runtime, browser surface, or raw HTTP
command. No URL, authorization header, API key, provider JSON, or provider SDK
object enters the provider-neutral model records.

## Command family

The version-1 commands are:

```text
Windvale-Model-Chat credential create --provider <openai|anthropic|google> --output <file> [--generation <u64>]
Windvale-Model-Chat credential inspect --credential <file>
Windvale-Model-Chat models --credential <file> [--page-size <1..128>] [--timeout-seconds <1..300>]
Windvale-Model-Chat chat --credential <file> --model <id> [--max-output-tokens <1..4096>] [--timeout-seconds <1..300>]
```

Unknown, duplicate, missing, empty, malformed, or secret-bearing options are
rejected. The command has no API-key or passphrase option and reads neither from
the environment. `credential create` requires explicit provider and output
path; the credential generation defaults to 1. `models` defaults to page size
128 and a 120-second operation deadline. `chat` defaults to 512 maximum output
tokens and the same deadline.

`credential inspect` performs structural inspection without unlocking and emits
only the WVSC public provider, origin, port, generation, identity, and credential
byte count. It never emits ciphertext or secret material.

## Protected terminal and file behavior

Credential and passphrase creation each require two matching masked terminal
entries. Model listing and chat require one masked passphrase entry. Protected
input requires a real terminal with raw-mode support; redirected standard input
is rejected. Later visible chat lines use the distinct
[`terminal.line_read_v1`](Terminal-Line-Input-Capability.md) provider and never
return a credential. Command-line arguments, environment variables, ordinary
output, diagnostics, model records, and child launch arguments never carry
either secret. Native child arguments contain only the public provider label,
exact model identifier, and output-token ceiling.

Credential creation uses [WVSC 1](Protected-Provider-Credential.md), writes an
ordinary new file with owner-only mode where the host supports it, flushes the
completed bytes, and never replaces an existing path. Prompt, confirmation,
wrapper, and passphrase buffers are erased at maintained ownership transitions.
The wrapper fixes provider identity, canonical service, port 443, and credential
generation. Unlock starts the supervised gateway with an empty child
environment and verifies that its metadata-only ready record matches the
wrapper before any request is accepted.

The version-1 software wrapper protects a copied credential at rest. It is not
an OS keyring or HSM and cannot protect an unlocked process from a compromised
host. The command keeps the gateway only for the command lifetime and always
requests teardown.

## Catalog and conversation behavior

`models` starts with provider generation zero, follows only canonical bounded
continuations returned by the same gateway, requires one unchanged nonzero
generation across all pages, and prints each admitted model identifier with its
optional display name. Catalog requests are serial and are never automatically
retried.

`chat` selects the exact model named by `--model`; it never substitutes an alias,
fallback, or newer model. After protected unlock, the host binds only
`model.catalog_v1`, `model.inference_v1`, `standard_output.write_v1`, and
`terminal.line_read_v1` to the native Windvale application. Process arguments
remain immutable fixed services. The application receives no credential,
resolver, socket, TLS object, URL, authorization header, provider JSON, or raw
terminal handle.

The Windvale-owned interactive commands are `:help`, `:clear`, and `:quit`.
Empty lines do nothing. Each other line becomes one user message and one serial
generation request. A successful text response is printed before it is
considered for retention.

The caller owns history under the existing `WVMM 1` limits:

- at most 32 messages and 16,384 encoded bytes;
- at most 3,072 strict UTF-8 bytes for each user or retained assistant message;
- complete oldest user/assistant turns are removed when the next exact request
  would exceed a count or byte limit; and
- `:clear` removes all retained turns.

An empty or larger-than-3,072-byte provider response is displayed but not
retained, and the submitted user message is also not committed. A definite
failed response commits neither message. The command shows length-limit and
content-filter completions. It does not silently truncate message content or
claim that output-token selection is a byte bound.

## Failure and retry behavior

Usage failure exits 64. Invalid terminal, input, or credential-file handling
exits 65. Definite provider failures exit 69. Internal or gateway startup
failure exits 70. `Submission_indeterminate` exits 75 and explicitly states
that the request was not retried. User cancellation exits 130.

Provider statuses and bounded diagnostics remain typed internally. A malformed,
oversized, wrong-identity, or invariant-breaking `WVMC 1`/`WVMG 1` response is
rejected before display. Every gateway exit path requests teardown. Version 1
performs no automatic retry, reconnect replay, provider fallback, or uncertain
mutation resubmission.

## Required executable evidence and boundary

The quick `model-chat` owner defines eleven hosted-supervisor cases, while the
separate `Test-Model-Chat-Native-Construction` entry point defines 32 Windvale
cases and is intentionally not part of fast normal feedback. The hosted cases
cover command admission, secret-option rejection, masked credential custody,
metadata-only inspection, catalog administration, native delegation, exact exit
status, readiness mismatch, and signal loss. Nineteen Windvale terminal-core
cases cover valid and malformed WVLR/WVLI records, bounds, generations, line
terminators, and strict UTF-8. Thirteen Windvale chat-core cases cover commands,
prepare/commit, rolling complete turns, and invalid history/messages. The
construction owner must build byte-identical WVB and WVO artifacts, execute the portable cores on
its local host, and construct both Windows and Linux native application images
before the native path is promoted. It makes no public-network call and reads
no real credential.

The executable command itself uses the production supervised gateway and can
make live calls when a user supplies a valid protected credential. A published
native artifact selects the Windvale UI; its absence selects the existing
hosted UI. Live provider
availability is operational evidence, not deterministic acceptance. The Node
process remains the implemented command and a temporary supervised host adapter
for protected unlock, credential custody, provider HTTPS, and private
WVMQ/WVMC/WVMG transport. Once the native path passes its promotion evidence,
Node no longer owns the chat UI or conversation. Installed Package 1 records, OS
keyring/HSM custody, streaming, tool calls, multimodal input, concurrent
requests, and GUI chat remain later increments.
