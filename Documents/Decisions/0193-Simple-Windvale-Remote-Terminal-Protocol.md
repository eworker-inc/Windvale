# Decision 0193: Simple Windvale remote-terminal protocol

- Date: 2026-08-03
- Status: Accepted future architecture; no remote listener, TLS provider, protocol codec, terminal adapter, identity, or remote session is implemented
- Refines: [Decision 0191](0191-Windvale-Console-Shell-And-Cli-Architecture.md) and [Decision 0192](0192-Capability-Oriented-User-Space-Network-Stack.md)
- Architecture: [Remote-terminal protocol](../Architecture/Remote-Terminal-Protocol.md)

## Context

The console architecture separates device or transport adapters, terminal sessions, the shell, and CLI processes. The network architecture delays remote access until reliable application networking, secure transport, identity, authorization, and teardown exist. Windvale still needs a small connection contract that can join those architectures without making raw TCP, terminal escape bytes, SSH's complete feature set, or a new cryptographic construction the native terminal definition.

The first remote connection is for a single developer or administrator reaching one Windvale machine. It does not need multiplexing, detached sessions, port forwarding, file transfer, environment import, remote execution shortcuts, or browser framing. Each additional feature would increase authority, parser surface, lifecycle state, and recovery requirements before the basic terminal path is proven.

## Decision

### Define a small session protocol over a secure stream

Adopt the provisional `WVTS/1` semantic protocol over an authenticated secure ordered byte-stream capability. The first real carrier is TCP protected by TLS 1.3. Windvale owns terminal framing, state, failure, authority, and lifecycle semantics but does not create a transport, cipher, key exchange, certificate format, or authentication primitive.

One secure connection carries one new terminal session and one shell resource domain. The first profile has no multiplexing, detach/resume, roaming, file or clipboard transfer, port or agent forwarding, arbitrary environment import, command-execution shortcut, or graphical terminal surface.

The remote adapter is an ordinary supervised service. It receives exact listen, secure-stream, identity/authorization, terminal-session, launch, and resource-domain capabilities. The kernel and shell do not listen on the network, parse `WVTS/1`, hold remote server keys, or infer authority from a connection.

### Require authenticated encryption for real network use

The real listener accepts only TLS 1.3 using the current specification in [RFC 9846](https://www.rfc-editor.org/info/rfc9846/). It negotiates the exact application protocol through ALPN. The provisional `wvts/1` identifier remains private until publication chooses or registers an identifier. Unsupported negotiation fails before terminal parsing.

The first usable profile pins the server identity at the client and allow-lists the client identity at the machine. TLS requests client authentication. An authorization provider maps the authenticated identity generation to an exact rights-limited session profile; authentication alone never creates a shell or administrative authority. The listener is disabled until its interface/address, port, server identity, trust set, authorization policy, connection limit, and budgets are explicitly bound.

TLS 0-RTT application data is disabled because session creation and terminal input are replay-sensitive. Connection resumption, if added later, never resumes a terminal session implicitly.

Before TLS exists, the codec may run only through in-memory, deterministic simulated, or build-restricted loopback test carriers. There is no packaged production plaintext mode and no silent security downgrade.

### Keep framing and messages bounded

The initial candidate frame uses an eight-byte header: one-byte type, one-byte flags, two reserved zero bytes, and a four-byte network-order payload length. Every profile fixes a maximum payload; 16 KiB is the planning ceiling pending measurement. The decoder validates the complete header and length before allocation or access and rejects nonzero reserved fields, unsupported required flags, truncation, oversize, invalid UTF-8, invalid enums, unknown required messages, and messages illegal in the current state.

The first semantic message set is `Hello`, `Accepted`, `Rejected`, `Textˉinput`, `Keyˉinput`, `Resize`, `Interrupt`, `Endˉinput`, `Output`, `Close`, `Closed`, and `Error`. The exact numeric encodings and payload records remain experimental until specification and qualification.

`Textˉinput` and `Output` carry strict UTF-8. `Keyˉinput`, resize, interrupt, end-input, close, and completion are typed messages rather than native key codes or magic terminal bytes. Normal and diagnostic output remain separate. Terminal cursor, color, mouse, clipboard, and graphical operations require a later negotiated extension and cannot make ANSI escape bytes the semantic API.

TCP/TLS owns ordered reliable transport, so `WVTS/1` adds no retransmission, acknowledgement, channel number, or multiplexing. Each application-data and control queue is bounded. A reserved control budget permits rejection, error, and closure to progress but cannot carry arbitrary output or bypass rate limits.

### Make the connection own the first session lifetime

Authentication, authorization, terminal creation, shell launch, capabilities, and resource-domain admission complete before `Accepted` publishes the session generation. Failure leaves no partially visible shell or grant.

Orderly close requests cancellation and bounded drain, returns structured `Closed` evidence when possible, and then closes TLS. Abrupt network loss, TLS failure, malformed protocol, authorization revocation, adapter failure, or provider loss stops input, requests foreground cancellation, and forces bounded teardown if graceful completion does not finish. Disconnect never leaves a detached shell in the first profile. Reconnection creates a new session generation.

`Endˉinput`, typed interrupt, orderly close, TLS close, network loss, revocation, provider failure, forced termination, and normal application completion remain distinct results. No uncertain application mutation is replayed after reconnect.

### Keep compatibility carriers outside the native contract

SSH may later be an isolated compatibility adapter translating an explicitly bounded subset of session, terminal-size, input, output, interrupt, and close behavior into the terminal service. SSH multiplexing, forwarding, environment, X11, agent, and POSIX process semantics are not implied.

A secure WebSocket or QUIC stream may later carry the same semantic messages for browser or network needs. HTTP, browser origin policy, WebSocket framing, and QUIC do not enter `WVTS/1`; changing carrier does not change authority, terminal, completion, or teardown semantics.

### Qualify the simplest complete slice

Implement only after the local terminal/shell path and the required TCP/TLS/identity path are qualified. Build and fuzz the codec as capability-free cross-host logic first, bind it to test streams, prove a Windows/Linux Windvale client against a hosted reference adapter, then add the Windvale OS service with exact capabilities.

The first end-to-end gate uses one pinned client and server identity on an isolated network, creates one rights-limited shell, exchanges every first-profile event, reports structured completion, closes securely, and leaves no process, endpoint, timer, buffer, listener grant, key reference, or session generation live. It also rejects authentication, authorization, replay, version, malformed, oversized, out-of-state, stalled, backpressured, and abruptly disconnected cases within exact budgets.

## Consequences

- Windvale gains a small native terminal connection without creating new transport or cryptography.
- One connection/one session removes channel routing, resume tokens, orphan ownership, and multi-session fairness from the first implementation.
- Typed key and lifecycle messages preserve the terminal service contract instead of promoting control bytes or ANSI sequences into the network API.
- Mutual provisioned identity and separate authorization avoid password handling and ambient remote-root behavior in the first profile.
- A Windvale client can qualify on Windows and Linux before Windvale OS serves the protocol.
- SSH, WebSocket, QUIC, richer terminal surfaces, and detached sessions remain possible as adapters or explicit revisions.
- Strict limits, reserved closure capacity, generation identity, and connection-owned teardown preserve recovery under hostile or failed peers.

No listener, port, ALPN registration, frame encoding, parser, TLS library, certificate, key store, identity map, authorization policy, terminal client, adapter, remote session, SSH service, WebSocket carrier, or QUIC carrier is implemented by this decision.

## Rejected alternatives

- **Plain TCP or Telnet-like terminal:** lacks authenticated confidentiality and creates a dangerous mode likely to escape a development network.
- **Invent encryption or authentication inside `WVTS/1`:** duplicates security protocols and turns the terminal project into cryptographic protocol design.
- **Adopt the complete SSH connection model as the native API:** imports multiplexing, pseudo-terminal modes, environment, execution, forwarding, and POSIX assumptions before they are needed.
- **Use WebSocket first:** requires HTTP and browser framing for a machine-to-machine path that only needs a secure ordered stream.
- **Multiplex sessions immediately:** adds channel IDs, independent flow windows, fairness, per-channel teardown, and orphan policy without a first consumer.
- **Keep sessions alive after disconnect:** requires secure resume identity, token storage, expiry, ownership transfer, resource policy, and administrative recovery before the basic path is proven.
- **Treat Control-C, EOF, resize, or key input as bytes:** makes one terminal encoding the semantic contract and obscures cancellation and lifecycle outcomes.

## Reconsideration triggers

Reconsider the profile when:

- one connection per session causes measured handshake or resource cost that multiplexing would materially reduce;
- a real administration workflow requires detach/resume, roaming, multiple shells, remote execution, or file transfer;
- certificate-based client authentication is operationally unsuitable and a standard alternative can preserve explicit identity and authorization;
- SSH interoperability is required earlier than a native Windvale client;
- browser access becomes a primary product path rather than an adapter;
- terminal latency or framing overhead misses a recorded target; or
- a richer terminal surface cannot be expressed as a bounded optional extension.

Any revision must retain authenticated encryption for real remote use, separate identity and authorization, exact capability grants, bounded parsing and queues, typed terminal control, observable failure, generation-safe teardown, and no ambient shell authority.
