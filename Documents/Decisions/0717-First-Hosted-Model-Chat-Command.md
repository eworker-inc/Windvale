# Decision 0717: First hosted model chat command

- Status: Implemented
- Date: 2026-08-16
- Advances: Decisions 0573, 0583, 0604, 0605, and 0647
- Contract: [hosted model chat command](../../Specifications/Hosted-Model-Chat-Command.md)

## Context

Windvale has a provider-neutral model protocol, protected credential wrapper,
authenticated HTTPS mappings for OpenAI, Anthropic, and Google, a supervised
gateway, and a native model-only bridge. Those components previously had only
developer tests and binary protocol tools. A person could not safely create a
credential, discover visible models, or type a conversation.

The source runtime also does not yet have a general protected terminal-input
capability. Waiting for that separate capability would leave the complete hosted
gateway unusable by a person, while putting an API key in an option, environment
variable, model record, or reference-oracle invocation would violate the
credential boundary already selected.

## Decision

Add `Windvale-Model-Chat` as a hosted bootstrap application. Give it four exact
operations: create a new WVSC wrapper through confirmed masked input, inspect
public wrapper metadata, list one bound provider's catalog, and chat serially
with one explicitly selected model. Launch the existing supervised gateway with
an empty child environment, verify its ready identity, and keep all provider
origins, headers, credentials, and JSON below that boundary.

Require a real terminal for every secret. Accept no credential or passphrase
argument, environment variable, pipe, or file. Never replace a credential file.
Keep conversation history caller-owned and bounded by WVMM 1. Remove only
complete oldest turns, never truncate retained messages, and never retry or
fallback after any failure. Report uncertain submission distinctly.

Keep the UI in a focused hosted application and the canonical client codec in
the hosted runtime. This creates a usable command now without claiming that Node
is the permanent source-language front end. A later Windvale terminal-input
capability can replace the UI while preserving the credential, gateway, and
model-record contracts.

## Consequences

- A user can safely set up OpenAI, Anthropic, or Google credentials and hold a
  bounded terminal conversation over the production HTTPS path.
- The application selects exactly one provider wrapper and one model. It gains
  no ambient network, arbitrary URL, broad secret, routing, or tool authority.
- The first distribution remains a repository-hosted Node bootstrap with
  Windows and Linux launch scripts rather than an installed Windvale package.
- Very large successful outputs are visible but do not enter subsequent
  conversation history.
- Deterministic acceptance uses a fake gateway and fake credential; live calls
  remain explicit operator actions.

## Reconsideration triggers

Reconsider when protected terminal input and standard text input are available
to ordinary Windvale applications, when OS keyring or HSM custody is introduced,
when Package 1 can bind the hosted gateway, or when streaming, structured tool
calls, multimodal data, concurrent sessions, or provider routing gain separate
contracts.
