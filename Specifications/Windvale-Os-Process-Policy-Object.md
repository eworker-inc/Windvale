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
   `Operating-System/Kernel/Process-Foundation.wv` and whose declared sources are
   application-launch policy 1, application-machine-construction policy 1,
   and resource-domain policy 1. The root directly pins
   boot-service-composition policy version 1.
2. Require the 42,027-byte WVB identity
   `22e40a95100c635a2bf8980ee6f81f5660e3ac6bf2251a2355e5c9b6106e3d55`.
3. Lower through the ordinary accepted-subset WVB-to-WVO launcher and require
   the 699,368-byte unrenamed identity
   `46844c80221180e039cfb9d45ed2493486d1b026d9712517f64025db202100a9`.
4. Rename exported `Main` to `Windvale_kernel_process_policy` through the
   verified native renamer into a private final candidate.
5. Require the 699,394-byte final identity
   `dea015f8cafac002eddb9383691e2de10cbdcd0c0a589a88d88fbef95241f5b5`
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
