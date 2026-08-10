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
| Metadata producer | 77,570 | `04ace838e229ecc3af22a9acd34badeec1718acf9f7efbc49e0f0be251e5209b` |
| Object instantiation | 22,525 | `efd0f2ef16793030aa18d5db402bec5ee0e45667a0c102741d5c0beb224f439e` |
| Windows imports | 12,392 | `faca11faf0d2675ea95c26f24aea7215a72d9bf2c6a7c6f438662ccbba9534ab` |
| Linux materialization | 18,884 | `aa57f0067177930bc3170c60a0aeb4393c5c12a92d84b2d107fc4a8cefebe25b` |
| Windows materialization | 23,071 | `f1245e3fb401f2236a78be5ce51c041001945404330acf880e497166a3eb8812` |

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
