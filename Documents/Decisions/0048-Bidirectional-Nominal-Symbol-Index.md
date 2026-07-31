# Decision 0048: Bidirectional nominal symbol index

- Date: 2026-07-31
- Status: Qualified at `e37204ffcdf17b39a486466cc13f35d8ee00b4b4`

The exact qualified candidate carried provisional number 0044. Concurrent OS work reached `main` first with Decisions 0044 through 0047, so integration renumbered this document to 0048 without changing the qualified compiler source or evidence.

## Context

Decision 0042 bounded lexical dispatch and identified symbol-directory decoding, canonical nominal ranking, and ordinal name comparison as the next measured costs in the exact ten-module typed-WVIR workload. The public `WVSD 1` declaration directory intentionally preserves canonical module and source-declaration order, while nominal shapes require a different canonical order: records by ordinal name, followed by enums by ordinal name.

The Windvale compiler repeatedly reconstructed directory-entry records and repeatedly derived the relationship between those two identity spaces. That work was correct but disproportionately expensive in body binding, typed lowering, and independent WVIR validation. Changing WVSD order would have mixed source identity with backend identity and invalidated an already qualified public evidence contract.

## Decision

Keep `WVSD 1.0` byte-for-byte unchanged and extend the private `WVSI` acceleration value to version 1.1. After its existing 256 bucket ranges and complete directory-index payload, WVSI now carries two deterministic tables:

- the reverse table maps each canonical nominal ordinal to its WVSD directory index; and
- the forward table maps each WVSD directory index to its canonical nominal ordinal, using the total nominal count as the sentinel for nonnominal declarations.

The reverse table is constructed in record-then-enum order by repeated exact ordinal UTF-8 minimum selection. Construction remains total before namespace validation: an unresolved reverse entry uses the WVSD entry count as its sentinel. Accepted symbol summaries have unique nominal names, so every nominal mapping is complete. WVSI remains private compiler evidence rather than a supported interchange format.

`Compilerˉsourceˉsymbolˉmatch` now retains the matched WVSD directory index. Type binding, record and enum shape production, typed-WVIR construction, and independent WVIR validation consume the bidirectional tables directly. Hot equality lookups reject unequal byte lengths before ordinal comparison, and directory scans read packed fields directly before constructing a match only for a successful result.

## Consequences

The public namespace, visibility, WVSD, WVLB, WVIR, WVB, diagnostics, and Stage 0 differential contracts do not change. The internal WVSI length is now exactly:

```text
4112 + DirectoryEntries * 8 + NominalTypes * 4
```

The fixed header and bucket ranges occupy 4,112 bytes; the bucket payload and forward table each contain one `u32` per WVSD entry; the reverse table contains one `u32` per record or enum.

On the current Windows development host, the focused typed-WVIR fixture falls from 5,735,695 to 5,715,847 instructions. More importantly, the real nine-module binding closure falls from 2,972,056,275 to 2,600,859,185 instructions, a reduction of 371,197,090 instructions or 12.5%, while the source-derived closure grows to 986 locals, 8,451 reads, 643 assignments, 1,508 calls, and 67,180 WVLB bytes.

The exact ten-module typed-WVIR closure still reaches bounded diagnostic `WVR3011` at the unchanged 4,000,000,000-instruction acceptance ceiling. A separate diagnostic run also reached an experimental 6,000,000,000 ceiling; that experiment does not change the gate. Profiling shows that packed directory-entry construction and nominal-rank derivation are no longer dominant. Repeated lexical and parser traversal is the next structural investigation.

## Verification

Exact commit `e37204ffcdf17b39a486466cc13f35d8ee00b4b4`, tree `8b2fa783637e46872cc783dd22f3f8ee975f4e7f`, passed the focused 24-test compiler area, the complete Standard suite, and full Windows and Debian Qualification. Both hosts completed zero-warning Release builds and all 48 tests, their normalized contracts matched, and all 61 portable artifacts totaling 7,546,823 bytes were byte-identical. The complete evidence is recorded in [Seed verification evidence](../Project/Seed-Verification-Evidence.md#bidirectional-nominal-symbol-index-qualification).
