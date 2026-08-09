# Windvale native hosted-verifier publisher file pipeline

## Status and scope

This contract exposes the service-free publisher construction stages as small
hosted Windvale file tools. Starting from one already-constructed generic
six-service publisher base application, the tools reproduce the exact Windows
and Linux publisher applications without a target-specific managed writer.

The pipeline deliberately remains split by owned invariant. Metadata identity,
publisher identity and structure, layout, ordered external targets, object
instantiation, Windows imports, and final PE/ELF materialization are separate
immutable records. No source file is split into numbered fragments merely to
avoid size; each module has one named contract.

## Commands

The new ordinary-file boundaries are:

```text
wvhostverifierproducemetadata <target:1|2> <publisher.wvb> <startup.wvo> <metadata.wvvp>
wvhostverifierpublishobjects <request.wvcr> <targets.wvpt> <startup.wvo> <adapter.wvo> <sha256.wvo> <objects.wvio>
wvhostverifierpublishimports <imports.wvim>
wvhostverifierpublishlinux <base.elf> <request.wvcr> <objects.wvio> <metadata.wvvp> <application.elf>
wvhostverifierpublishwindows <base.exe> <request.wvcr> <objects.wvio> <metadata.wvvp> <imports.wvim> <application.exe>
```

The metadata producer hashes and admits the exact publisher WVB and selected
startup WVO. It emits `WVPM 1` with the target-specific instantiated five-byte
startup digest, invokes the existing metadata constructor, and writes only the
admitted 128-byte `WVVP 1` result.

The object tool validates the `WVCR 1` and `WVPT 1` headers plus every field it
consumes, constructs the
exact `WVIX 1` request from their target, addresses, counts, ordered address
payload, and three WVO files, then writes the successful `WVIO 1` response.
Windows import construction has no input file because `WVIR 1` is an exact
fixed request. The materializers construct their bounded `WVLM 1` or `WVWM 1`
envelopes and write only the admitted raw application payload.

Every tool rejects a byte-identical output path argument equal to an input path
argument before mutation. Semantic rejection returns 2 without writing the
destination; usage or exact path-text alias rejection returns 64. These are
private intermediate tools, not durable publishers: alternate spellings or
filesystem aliases are excluded by the future orchestration boundary, and its
private output must pass the separate complete hosted-verifier-application
admission before public replacement. The materializers retain the structural
checks of their underlying focused constructors; they do not independently
hash arbitrary intermediate files or replace the final application admission.

## Candidate identities

All WVBs are built through the digest-bound native source front door.

| Tool WVB | Bytes | SHA-256 |
| --- | ---: | --- |
| Metadata producer | 53,009 | `74de7ca9a0c959c782837d6674c30db1dcccb07ed258d50017c968ad38d503bc` |
| Object instantiation | 21,724 | `410e4f93c24a2f7cac168298e1e3f2bc3d62f9738c36227b69805ad65591b341` |
| Windows imports | 10,464 | `63b87f2618c9fd413238a9a2919bc6cdb1c769e72f4dca2de47c1c7e1c697a29` |
| Linux materialization | 16,600 | `84bec5e36d1ae61f05b28c506b8285526022ec05990153bb0079beb61badeacc` |
| Windows materialization | 18,658 | `2c9092e5781cadf6a675168415c73ed65303737f6134a5d0bb9a59d874a7cbd2` |

## Evidence and remaining work

The reviewed managed differential test builds all five WVBs, runs the file pipeline
for both targets, and reproduces the 256,000-byte Windows and 254,917-byte Linux
publisher applications exactly. Corrupt publisher identity, corrupt ordered
targets, and output aliasing reject while preserving existing destinations.

The [publisher base-construction contract](Windvale-Native-Hosted-Verifier-Publisher-Base-Construction.md)
now supplies the former managed input through exact packaged Windvale processes
and owns the normal candidate lane. This managed test remains only independent
recovery evidence. Independent Linux execution, completed-publisher durable
publication, grouped qualification, promotion, and release integration remain.
