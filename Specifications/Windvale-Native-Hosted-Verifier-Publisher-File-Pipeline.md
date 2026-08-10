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
wvhostverifierproducemetadata [role:0|1|2] <target:1|2> <module.wvb> <startup.wvo> <metadata.wvvp>
wvhostverifierpublishobjects <request.wvcr> <targets.wvpt> <startup.wvo> <adapter.wvo> <sha256.wvo> <objects.wvio>
wvhostverifierpublishimports [publisher|promoter|wvb-publisher] <imports.wvim>
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
| Metadata producer | 69,072 | `d21b5c3733f687d0c3e3531521361a54fb25a3f0cf6b76a010a0bf9ff0603170` |
| Object instantiation | 22,313 | `8ddd19ebc0586094ce295c53ad694dea02ae6339abe0ba3bc4415299ae3732a2` |
| Windows imports | 12,052 | `c5bbf97454e12d7844427ce32b435445c3f4db3877c6fca630d97734373c34bd` |
| Linux materialization | 18,198 | `5b3a1ebae5ace39f495631c9183d197870c2cfa00261fa11dd7d3ead521124bc` |
| Windows materialization | 21,846 | `077c2fb6e0d553e97f9ef507c3b3165a08112f492898b9fcf5fbbbb893284b5b` |

## Evidence and remaining work

The 15-case native owner builds publisher, promoter, and WVB-publisher roles
for both targets, reproduces the exact applications, exercises read-only admission, and
uses the current-host promoter and publisher as one durable installation chain.
Corrupt identity, corrupt ordered targets, and output aliasing reject while
preserving existing destinations. The managed differential test remains
independent Stage 0 recovery evidence.

The [publisher base-construction contract](Windvale-Native-Hosted-Verifier-Publisher-Base-Construction.md)
now supplies the former managed input through exact packaged Windvale processes
and owns the normal candidate lane. The separate digest-bound promoter now owns
completed-publisher durable publication. Independent Linux execution, grouped
qualification, promotion, and release integration remain.
