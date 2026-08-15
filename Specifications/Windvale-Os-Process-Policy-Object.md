# Windvale OS process-policy object build

## Status and purpose

This contract defines the ordinary Windvale-native construction of Probe 40
`04-process-policy.wvo` from its canonical portable source. It composes existing
general tools and does not define a separate compiler or object format.

## Public commands

```bat
Tools\Native\Build-Os-Process-Policy-Object.cmd output.wvo
```

```sh
./Tools/Native/Build-Os-Process-Policy-Object.sh output.wvo
```

The destination must use `.wvo`, must not exist, and must have an existing
parent directory. Private intermediate files remain in a uniquely named sibling
directory and are removed on success or failure.

## Fixed source chain

The builder performs these exact steps:

1. Build `Projects/Operating-System/Windvale-Os-Process-Policy.wvproj`, whose root is
   `Operating-System/Kernel/Process-Foundation.wv` and whose declared source is
   `Operating-System/Kernel/Resource-Domain-Policy.wv`.
2. Require the 33,786-byte WVB identity
   `26a540bc1435114608aa597545c805e0786c9593b6e8ba19e8919b9f7718b0c1`.
3. Lower through the ordinary accepted-subset WVB-to-WVO launcher and require
   the 583,390-byte unrenamed identity
   `dcee27f6384933ef07cf99eefd5f3355e25edbf690c332c7b201a397a0031d95`.
4. Rename exported `Main` to `Windvale_kernel_process_policy` through the
   verified native renamer into a private final candidate.
5. Require the 583,416-byte final identity
   `4d3ffefc6be3c4edb48f1032415d96987bbd62899cdadd1fb4f0dc91ca319428`
   and independently admit the resulting WVO before publishing it through the
   native WVO publisher.

The publisher owns destination publication. A pre-existing destination is
never replaced or deleted, and failed validation cannot expose a partially
validated result at the destination.

## Focused evidence

`Test-Os-Process-Policy-Object` owns exact construction and existing-output
preservation. General source, lowerer, renamer, and WVO malformed-input behavior
remains owned by their existing focused suites rather than being duplicated
here. Cross-host execution belongs to the final grouped retirement gate.
