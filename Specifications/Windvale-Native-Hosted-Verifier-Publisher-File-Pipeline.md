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
| Metadata producer | 85,942 | `7f9ac017096727387d1265c6408d2acf90a5667ad5050117c226a0f57608e373` |
| Object instantiation | 22,737 | `2ada7d72a645677ba86d919df21a2fa020737cc56c6ff86172df3e233bdba00a` |
| Windows imports | 12,764 | `c71ccad47766356e9da53e8615aabbb404c64e9ebd6c9cd80ad4e302d1609052` |
| Linux materialization | 19,570 | `b69db1d94b837d4d8596630875db1e30204f0ad6fa04b2cc64d882a50c18c623` |
| Windows materialization | 24,296 | `674e2adc989bef1778a65a9880267ed5110e7e2f0d450181349c281119604574` |

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
