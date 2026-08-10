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
| Metadata producer | 69,072 | `222b92872763d005b8b8dbc11819111b1acfe3506bd0c0e6e011ceee14887568` |
| Object instantiation | 22,313 | `52b988a10c98c35946db93a0b4f2f9323e29defd7785373b5056286a40819ab9` |
| Windows imports | 12,052 | `474388c62ab8dd60eb1d9393bd4ea58460a163933b2f2bccc3e1845b3cdbd4b6` |
| Linux materialization | 18,198 | `fc8a981fad55ad3576053ab0fe54717d3128410ffe0f86d43585249373d78f34` |
| Windows materialization | 21,846 | `c2720d87ce0b16a2e5681c7928ce66e988200f8dc1c5c48318376e03329c4002` |

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
