# Windvale native hosted-console container mutation tests

## Status and scope

This fixed contract transfers ordinary format-2 hosted PE and ELF admission
from the live Stage 0 verifier to portable Windvale and the native console
publisher. It covers two complete valid `Helloˉhosted` applications and the
exact thirteen managed mutation operations around metadata, output services,
imports, runtime tables, truncation, and trailing bytes.

The contract is separate from the format-1
[console-container mutation tests](Windvale-Native-Console-Container-Mutation-Tests.md)
and the arbitrary
[hostile-input corpus](Windvale-Native-Console-Container-Hostile-Input-Tests.md).
It does not replace segmented larger-than-4-MiB admission, construction of a
large hosted application, execution of the published target on both hosts, or
the final grouped retirement gate.

## Portable admission boundary

`Linker/Windvale/Console-Application-Admission-Core.wv` is the single portable
dispatcher used by the verification bridge and native publisher. Canonical
format-2 markers select the focused Windows or Linux hosted verifier; all other
inputs retain the existing format-1 recipe verifier. A format-2 candidate must
fit in the first immutable value and have an empty second chunk.

The hosted verifier is split by ownership:

- `Hosted-Console-Application-Verification-Common.wv` verifies the execution
  context, service/output tables, `WVHC 1` metadata, SHA-256 fields, canonical
  output leaf, normalized WVA startup, native entry, and native recovery;
- `Hosted-Console-Application-Verification-Windows.wv` owns exact PE headers,
  sections, directories, imports, relocation records, padding, and bounds; and
- `Hosted-Console-Application-Verification-Linux.wv` owns exact ELF headers,
  program headers, Windvale note, segment padding, and bounds.

The implementation imports portable `Foundation/Sha256.wv`; admission does not
depend on a host hashing capability. Format-1 layout, construction, and
core verification modules remain byte-identical.

## Corpus and oracle provenance

The two immutable bases are current Stage 0 format-2 applications:

| Target | Bytes | SHA-256 |
| --- | ---: | --- |
| `windows-x64-console-v2` | 3,584 | `0f59222c33828d65a086de9f2b3eb22f00fc3b8c69cf7262a19b9e8df8b4f4e0` |
| `linux-x64-console-v2` | 9,216 | `7ad022f26e24949ddb7a4b1cb7681e7edc24c573e501c64d92a8f0c9b4bca1fd` |

Both contain the same 691-byte native image at SHA-256
`ea44bb6a529d7b4b15c90a1a4d5acee696235409d56217862e53ddfb83408f98`.
The Windows and Linux canonical output-leaf digests are respectively
`10f3a500aca7f0236cdf9f6c20658591df88bc612e677264cdaa0bcef59a0a48`
and
`c5ea073a24c46dd634b1a67a7e7041d476dbce856d058aa8adc2c4e680d3d226`.

One reviewed, temporary Stage 0 harness compared the portable Windvale bridge
against the existing managed result for both bases and every mutation. It
passed 15/15, including one exact 691-byte write for each valid base and no
write for each rejection. The temporary program and repository build output
were then removed; permanent tests neither build nor invoke it.

## Fixed manifest

`Manifest.txt` starts with
`windvale-hosted-console-container-mutations 1`. Every LF-terminated case row
contains:

```text
name|platform|expectation|stage0|operation|bytes|sha256
```

The ordered inventory contains eight Windows and seven Linux cases: two valid
bases, nine `xor1` values, two of those with the output-leaf digest recomputed,
two 500-byte truncations, and two trailing-zero values. Windows rejections
retain `WVW2100` provenance; Linux rejections retain `WVL2100` provenance.

The 2,024-byte manifest has SHA-256
`208a309624bef868b657cc87e2e95d6c085da1528bc5bc471226dc4b22c764f9`.
It and all fifteen candidates are stored in one 3,534-byte gzip tar archive at
SHA-256
`a8027a9d4238767ae9b7ab18e3d0114da4e4fdf3edcbbc044d4358f2ce1fd055`.
The repository representation is 4,774 LF-only base64 bytes at SHA-256
`a0d08db4598b8767e1680dbeeb9c5006ca9ad9d081f50f451123ed4ff4c361a4`.
Independent pre-run review reconstructed every mutated candidate byte-for-byte
from its valid base and recorded operation.

## Native command contract

`Tools/Native/Test-Hosted-Console-Container-Mutations.cmd` and `.sh` verify the
archive, manifest, platform/suffix, operation counts, size, and digest of every
input before invoking the digest-bound current-host publisher. No input is
mutated or generated during the run.

For each valid base, publication must return `0`, write no diagnostic, emit the
exact platform success-report identity, and leave a destination byte-identical
to the candidate. For every rejection, publication must return `1`, write no
standard output, emit exactly
`publication status=Rejected phase=console-application` plus LF, preserve the
fixed destination sentinel, and leave no `.wvpublish-*` scratch. Every input
must remain unchanged.

The rejection-report SHA-256 is
`39db034713225109f62c272db447d75cfe93ff0c259c8d9e5211f0df5c007e1f`.
The Windows and Linux valid-report SHA-256 values are respectively
`6eb507dd88b808f1a0b8fdc811da18bcfa2e6c5d18d56f8b1fb7a5cca33bff2d`
and
`0e3fc5697dd9f6b882d0d4b7cc8c1d771a65789278a35f28ec7f3e729952f142`.

Success ends with:

```text
Tests: 15, Passed: 15, Failed: 0
```

The command starts no .NET process. The retained managed verifier remains an
explicit recovery and final independent-evidence owner until the complete
Decision 0057 gate permits its archive or deletion.
