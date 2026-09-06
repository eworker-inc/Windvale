# Decision 0959: Windvale-owned native model chat

- Status: Accepted; migrated implementation pending current-compiler verification
- Date: 2026-09-06
- Advances: Decisions 0647, 0713, and 0717
- Contracts: [hosted model chat command](../../Specifications/Hosted-Model-Chat-Command.md), [terminal line input](../../Specifications/Terminal-Line-Input-Capability.md), and [native external-model gateway bridge](../../Specifications/Native-External-Model-Gateway-Bridge.md)

## Context

Decision 0717 made the supervised HTTPS model path usable, but Node owned the
interactive prompt, commands, model request construction, and conversation
history because Windvale had no bounded terminal-input capability. That was a
temporary bootstrap boundary, not the desired application architecture.

Windvale now has native standard byte output, verified ABI-23 model calls, and
enough source-language/runtime support for the complete bounded conversation.
The remaining authority to add and verify is visible line input that stays
separate from the private model protocol pipes and never pretends to protect
credentials.

## Decision

Define `terminal.line_read_v1(bytes)->bytes` with bounded WVLR requests and
WVLI responses. It admits only visible input, at most 3,072 strict UTF-8 bytes,
and typed end, interruption, rejection, provider loss, stale, and revoked
outcomes. It is not a credential or raw-terminal capability.

Implement the interactive model chat in Windvale source. The application owns
provider/model display, token-limit admission, `:help`, `:clear`, `:quit`, exact
request identities, typed provider failures, and at most 32 WVMM messages. It
receives four provider bindings—catalog, inference, byte output, and line
input—plus immutable process arguments. It receives no credential, network,
resolver, TLS, URL, provider JSON, or native handle authority.

Keep Node only as the temporary protected-credential and HTTPS supervisor. It
performs masked unlock, verifies gateway readiness, launches the native
application with public provider/model/token settings, and transports private
WVMQ/WVMC/WVMG frames over dedicated pipes. It does not read chat lines, parse
chat commands, retain history, or format model answers.

On Linux, bind visible I/O through a separately opened `/dev/tty`. On Windows,
bind `CONIN$` and `CONOUT$`, read UTF-16 with `ReadConsoleW`, and convert to
strict UTF-8. Headless execution leaves terminal authority unavailable and
returns typed loss without weakening the model-only bridge.

## Consequences

- The accepted product-facing chat becomes a genuine `.wv` application on
  Windows and Linux after current-compiler and host execution verification.
- Credentials remain outside Windvale application memory and model records.
- Private model protocol bytes cannot collide with visible terminal text.
- The migrated builders are required to self-host the current compiler/lowerer,
  compare repeated WVB/WVO/provider objects, execute portable core tests
  locally, and construct both host images before promotion.
- The Node launcher, protected wrapper, catalog administration, and HTTPS
  gateway remain bootstrap infrastructure rather than application UI.
- A five-minute supervised native lifetime remains the version-1 bound.

## Reconsideration triggers

Reconsider when Package 1 and the service manager can install and bind the app,
when inherited terminal endpoints replace host terminal discovery, when an OS
keyring/HSM owns unlock, or when streaming, tools, multimodal input, concurrent
sessions, or a GUI obtain separate bounded contracts.
