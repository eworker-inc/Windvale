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

1. Build `Projects/Operating-System/Windvale-Os-Process-Policy.wvproj`, whose sole root is
   `Operating-System/Kernel/Process-Foundation.wv`.
2. Require the 18,764-byte WVB identity
   `c46c6b3780cad8d292607ed687a7e511e2e3c47fbc6fc21526ecc0ffeb937895`.
3. Lower through the ordinary accepted-subset WVB-to-WVO launcher and require
   the 129,284-byte unrenamed identity
   `11e1796c176dcdeb2f643108b646363751347707ca4b16b0e914b8c0b384987e`.
4. Rename exported `Main` to `Windvale_kernel_process_policy` through the
   verified native renamer into a private final candidate.
5. Require the 129,310-byte final identity
   `35d751147a7285fb926ba68e77da4ef554bcf68a58963520153f23ea3e8c4678`
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
