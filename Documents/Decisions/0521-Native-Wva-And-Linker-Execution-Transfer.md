# Decision 0521: Native WVA and linker execution transfer

## Status

Implemented and independently exercised on Windows and Linux. Grouped repository
Qualification, candidate promotion, and final recovery retirement remain pending.

## Context

Decision 0520 transferred the supported WvDump and WVO read-only execution
subset into the paired native Seed helper. The broad Windows and Linux scripts
still executed ordinary WVA assembler and Wv linker behavior through the
managed reference runtime even though the digest-bound native applications
already owned the same semantic outcomes, publication bytes, and preservation
rules.

The transferable block comprised twelve managed invocations per host script:

- WVA assembler and Wv linker no-argument self-tests;
- canonical WVA assembly plus a duplicated Stage 0 object-verification call;
- linker-scanner acceptance of that object and rejection of non-WVO input;
- WVA semantic rejection for absent and existing destinations;
- construction of the canonical provider object;
- canonical link/image/map publication; and
- undefined-import rejection for absent and existing destinations.

Capability-refusal tests and failed writes to missing output parents are not
the same contract. They exercise the reference runtime's authorization and
typed hosted-file adapter failures (`WVR3010` and `WVR3022`). The pinned native
applications currently terminate at the raw host boundary for those failures,
so those calls remain explicit rather than being counted as transferred.

The adjacent WVO candidate is also intentionally unchanged. Its candidate-2
application exits 1 for the no-argument self-test and for empty or missing file
input, while a current Stage 0 reconstruction passes the self-test. Decision
0520 forbids repinning that historical application from one-host current-writer
evidence. Repairing its native reconstruction and qualification remains a
separate product-identity change.

## Decision

### Give the paired native helper ordinary WVA/linker execution ownership

Extend `Verify-Seed-Native-Front-Door.ps1` and
`Verify-Seed-Native-Front-Door.sh` with one exact WVA/linker execution owner.
Each host implementation:

1. verifies its pinned assembler and linker application byte lengths and
   SHA-256 identities before direct execution;
2. runs both no-argument self-tests and requires exit 0 with no output;
3. feeds the existing 218-byte canonical WVO to the linker scanner, requires
   the exact valid report, and proves the object unchanged;
4. feeds `Sum-Data.wv` to that scanner, requires exit 2 and the exact
   `Badˉmagic` report, and proves the source unchanged;
5. requires WVA semantic rejection to create no new destination and preserve
   the existing canonical object byte-for-byte;
6. assembles `Console-Provider.wva` to the exact 91-byte provider object;
7. links the canonical object and provider into the exact 24-byte image and
   1,721-byte path-free map; and
8. requires undefined-import rejection to create no new image and preserve the
   existing canonical image byte-for-byte.

The host application identities are:

| Product | Host | Bytes | SHA-256 |
| --- | --- | ---: | --- |
| WVA assembler | Windows | 2,895,360 | `e03a1f22317fef36213d14a0a669b262f81143a54cbe334da075901987268ed4` |
| WVA assembler | Linux | 2,895,872 | `ebe18959f2a057db5181f4e2bbf7979fac9359d50542581b63da6dc48c4163a0` |
| Wv linker | Windows | 1,796,608 | `08744f3cacf71280ea757dcdf6509ee3770d5536b08e5b3984a438cb6123fb78` |
| Wv linker | Linux | 1,798,144 | `8a220bfd6c7ef684897583e728419ecd6d383c8e8cf40094edbcfb695e3d6d7a` |

The shared cross-host output identities are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| `Sample.wvo` | 218 | `992c298a4f9b68dec27b7203a2770f2a37ef2016ea45e88d33ee21994060fe85` |
| `Console-Provider.wvo` | 91 | `486134e34bb32abadd233d1c3303acd9c313aa69d3874cafdce0fcb61b6e72ab` |
| `Hello-Linked-Windvale.bin` | 24 | `0e02d447ec379e8bc8be373694d6ca14fdde0125550cbd34ee05b3ecc63ffe9a` |
| `Hello-Linked-Windvale.wvmap` | 1,721 | `31bc6a8e90d5f3049ae3e2eb0735a901923186d6a03ed40f22762b557b2ba5f4` |

The broad scripts alias the former Windvale-assembly path to the helper-owned
`Sample.wvo` and consume the helper-owned provider, image, and map. The retained
Stage 0 assembler/linker lane still reconstructs its separate object and image,
compares exact bytes with those native products, verifies and inspects its
object, and exercises its own malformed and output-preservation contracts.
This preserves differential/recovery evidence without asking managed execution
to own the ordinary Windvale-authored tools.

### Keep the qualification claim bounded

Both helper implementations executed over one identical tracked working state.
The Windows run used the raw PE products on Windows x64. A separate Linux 6.1
x86-64 host received a Git-tracked-only archive, restored executable bits only
inside a disposable directory, and executed the raw ELF products. No private
host identity, address, credential, unrelated source, service, configuration,
or data is part of Windvale evidence or repository state.

This establishes independent dual-host execution for the 184-case helper. It
does not establish the grouped repository Qualification gate, clean or
previous-seed renewal, GitHub's pinned-Debian result, candidate promotion,
atomic installation, or the final Stage 0 recovery release.

## Evidence

- Windows completes one uninterrupted native helper run in 984.1 seconds with
  exact summary `artifacts=105 cases=184`.
- The independent Linux host completes the same helper in 845.9 seconds with
  the identical exact summary.
- The four shared products above have identical byte lengths and SHA-256
  identities on both hosts.
- A focused Windows probe confirms exit 2, no partial output, and existing-byte
  preservation for WVA semantic rejection and linker undefined-import
  rejection.
- PowerShell parsing, Bash syntax, `git diff --check`, the 27-general/54-native
  changed-file routing contract, and the retirement inventory pass.
- The retirement inventory remains twelve direct managed entry files: three
  normal and nine recovery. This slice removes calls inside the broad scripts,
  not a direct entry file.

The paired helper grows from 102 to 105 exact artifacts and from 174 to 184
cases. Removing twelve managed invocations from each broad host script brings
the cumulative normal-path removal across Decisions 0505, 0506, and 0508
through 0521 to 192 managed invocations per script.

## Consequences

Ordinary WVA/linker self-test, canonical assembly, scanner acceptance and
rejection, semantic rejection, provider construction, link publication, and
undefined-import preservation no longer load .NET in either permanent-host
broad script. Native products become the inputs to the retained Stage 0
differential lane, which makes ownership direction explicit.

The broad scripts deliberately retain WVA/linker capability refusal and missing
output-parent behavior. WvDump/WVO capability and resource-error calls, the
stale candidate-2 WVO self-test, Stage 0 object-report oracles, later Stage 0
assembler/linker differential cases, grouped Qualification, artifact promotion,
and the final recovery/archive release remain open.

## Reconsideration triggers

Specify a versioned native hosted-service failure report before replacing typed
reference-runtime authorization or file-provider failures with an unstructured
process exit. Repair the WVO inspector reconstruction through its native
publisher/service chain and obtain independent candidate evidence before
changing its pinned identities. Continue transferring the remaining broad
managed calls only when an exact native owner or an explicitly recovery-only
suite boundary exists.
