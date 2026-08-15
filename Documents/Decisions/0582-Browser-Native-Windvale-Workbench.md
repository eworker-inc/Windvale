# Decision 0582: Browser-native Windvale Workbench

- Date: 2026-08-15
- Status: Accepted experimental product slice
- Extends: [Decision 0182](0182-Browser-And-WebAssembly-Product-Direction.md)
- Does not accept: WebAssembly as a permanent host or target, browser storage as a Windvale filesystem, or the Workbench as Windvale OS

## Context

The normal playground can edit one source module, compile it in an import-free
WebAssembly compiler, admit the returned canonical WVB as untrusted input, and
execute a bounded scalar application in a disposable worker. That proof is
useful but does not yet feel like a small computing environment: source tabs are
ephemeral, output is not an interactive command session, and users cannot name,
save, reopen, or launch programs from a workspace.

Booting the current x86-64 Windvale OS image in a browser remains a separate
machine-emulation problem. An OS-styled browser host can still provide a useful
and honest product demonstration if it preserves the real compiler, WVB, verifier,
runtime, and capability boundaries and labels the browser-owned seams explicitly.

## Decision

Add an experimental **Windvale Workbench** to the existing static playground.
Its first slice provides an interactive terminal, one flat `/workspace`, bounded
shell commands, editor save/open, and one foreground `run` operation over the
existing direct compiler and scalar interpreter workers.

The browser UI adapter owns the shell and workspace. It may use the Origin
Private File System when `navigator.storage.getDirectory` is available, and it
falls back visibly to session memory when that provider cannot be opened. The
origin-private provider is bounded to 64 regular files, 64 KiB per UTF-8 file,
and 2 MiB aggregate bytes. Names are one ASCII segment of 1 through 255
characters using letters, digits, `.`, `_`, and `-`; `.` and `..` are rejected.
There is no traversal, link, native path, directory, device, or ambient host-file
surface in this slice.

The initial shell supports `help`, `pwd`, `ls`, `cat`, `save`, `open`, `write`,
`rm`, `run`, `status`, and `clear`. Commands are limited to 4 KiB, parsed without
evaluation, and execute one at a time. Pipes, redirection, background jobs, and
scripts are not silently approximated.

`run [file]` reads source through the Workbench provider and then uses the normal
single-module source-to-WVB-to-execution path. Compilation and guest execution
remain in the disposable worker. The running module receives only the existing
explicit `console.write_line` grant when enabled. It receives no workspace,
OPFS, DOM, keyboard, network, clock, or host-file authority. A successful host
workspace write is reported only after the browser writable closes; the shell
does not retry a failed mutation.

The Workbench must visibly say that it is a browser host and not Windvale OS.
Browser workers are not Windvale processes, OPFS is not the Windvale filesystem,
the JavaScript shell is not yet a Windvale application, and browser scheduling
is not the Windvale scheduler.

## Consequences

Users gain a persistent, command-driven path for creating and running small
Windvale applications without a server or extension. The exact compiler,
canonical WVB admission, instruction budgets, output limits, package identities,
and worker containment remain unchanged.

Origin-private data is scoped to the site and may be removed when site data is
cleared. The first slice therefore does not claim backup, synchronization,
durability strength, or access to a user's ordinary files. Import/export and an
optional explicitly selected host-directory mount require later contracts.

Because storage is a host-shell concern rather than guest authority, this slice
does not introduce a new Windvale capability name or weaken the existing browser
module allowlist. Application filesystem access requires a separate versioned,
rights-limited semantic interface and runtime implementation.

## Reconsider when

- applications, rather than the host shell, need workspace access;
- directories, packages, multiple mounts, import/export, or selected host folders are introduced;
- the shell can be implemented as a Windvale application over typed terminal and workspace capabilities;
- multiple foreground or background applications need lifecycle and resource-domain contracts; or
- cross-browser persistence, quota, mutation, and recovery evidence is ready for a support claim.
