# Decision 0501: Native Wv-Linker reconstruction

- Status: Implemented current-host candidate; grouped Windows/Linux qualification pending
- Date: 2026-08-10
- Scope: current-Windows-host native cross-target reconstruction of the standard profile-4 Wv-Linker candidate
- Extends: Decisions 0221, 0302, 0441, 0492, 0496, and 0497

## Context

The standard `WVHL 1` linker applications already execute the complete
Windvale-written flat linker behind digest-bound Windows and Linux launchers.
Their checked-in application identities still carried Stage 0 construction
provenance. Rebuilding the linker's WVO and then invoking that same standard
linker to produce its native fragment would also make the application under
construction part of its own construction path.

Decision 0496 provides a distinct segmented staging, image-linking, and
canonical-transport path. That path can construct the standard linker's raw
image without invoking either target `Wv-Linker` application. The retained raw
lowerer can separately produce one complete WVO oracle for the same exact WVB.

## Decision

Windvale adopts one bounded reconstruction route for the exact standard
Wv-Linker candidate:

1. build `Projects/Linker/Windvale-Wv-Linker.wvproj` once through the retained native project
   front door and require the complete WVB identity;
2. invoke the retained raw accepted-subset lowerer once and require the
   complete WVO oracle identity;
3. independently stage that exact WVB through the segmented staging producer,
   link its ordered object chunks through the segmented image linker, and
   transport the result into canonical image chunks;
4. require one canonical chunk, exported entry `Main` at offset 884,630, and
   the complete raw-fragment identity;
5. construct the Windows and Linux profile-4 applications from that admitted
   WVB and canonical fragment through the retained native hosted-container
   toolset; and
6. require all five product identities in a separate caller-owned output
   directory.

The exact reconstructed products are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Wv-Linker WVB | 135,740 | `02f727a8ce2d6826c8414cada0933c7d5a54893ea061621d08147984c3d6f874` |
| raw-lowerer WVO oracle | 1,786,271 | `0141219773241e8780e2520f30ab8377914bf89a72f57da091871ac40d68a287` |
| canonical linked fragment | 1,777,781 | `d30e0c4dce7159bf98c546a0200e8b541797612ab67d6f21e3d8ee876af27480` |
| Windows application | 1,796,608 | `08744f3cacf71280ea757dcdf6509ee3770d5536b08e5b3984a438cb6123fb78` |
| Linux application | 1,798,144 | `8a220bfd6c7ef684897583e728419ecd6d383c8e8cf40094edbcfb695e3d6d7a` |

## Evidence boundary

The raw-lowerer WVO contains two sections, 168 symbols, and 150 relocations.
The independently transported fragment contains 1,776,560 text bytes followed
by 1,221 read-only-data bytes. Its complete identity agrees with the image
expected for the exact WVO oracle; the standard linker applications do not
participate in producing it.

This is current-Windows-host native cross-target construction. It consumes the
retained same-release compiler, raw lowerer, segmented staging/link/transport,
hosted-container, startup, and service-leaf candidates. Avoiding target
self-linking does not make the route a clean bootstrap or previous-release seed
renewal.

## Consequences

- The managed application writer is no longer the sole constructor for this
  exact candidate.
- Neither target Wv-Linker application participates in constructing its own or
  its sibling's fragment or container.
- The raw WVO oracle and independently staged fragment remain separate exact
  evidence; this decision does not replace WVO admission or linker behavioral
  tests.
- The Linux application is a cross-target product until independently
  reconstructed and executed on Linux.
- The constructor writes a separate reconstruction directory. It is not an
  atomic installer or durable promotion transaction.
- Grouped qualification, ordinary-path promotion, clean bootstrap evidence,
  and removal or archival of Stage 0 remain separate gates.

## Reconsider when

Reconsider this decision if the Wv-Linker source/project closure, raw lowerer
oracle, segmented stage/link/transport identities, native entry offset,
profile-4 hosted-container inputs, or either target application identity
changes.
