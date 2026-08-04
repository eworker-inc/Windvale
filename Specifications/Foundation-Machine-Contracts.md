# Foundation machine contracts

## Status and scope

`Foundationˉmachineˉcontracts` is the first reusable Windvale Foundation source module. It owns two small WVO-adjacent semantic rules used by both the Windvale assembler and linker. It is portable, capability-free, deterministic, bounded, and cross-host qualified at `d46af86`.

This is an early-development source API without a backward-compatibility promise. It does not change WVB 1.11, WVA 1, WVO 1.0, or Windvale Linking 1.

## Public functions

```text
Foundationˉalignmentˉisˉvalid(Value: u32) -> bool
Foundationˉmachineˉnameˉisˉvalid(Input: bytes) -> bool
```

`Foundationˉalignmentˉisˉvalid` returns `true` exactly for `1`, `2`, `4`, `8`, `16`, `32`, `64`, `128`, `256`, `512`, `1024`, `2048`, and `4096`.

`Foundationˉmachineˉnameˉisˉvalid` returns `false` for zero bytes or more than 255 bytes. At index zero it accepts ASCII `A` through `Z`, `a` through `z`, `_`, `.`, and `$`. At later indices it additionally accepts ASCII `0` through `9`. Every other byte, including non-ASCII UTF-8, is rejected. The function consumes an immutable byte value and does not decode it as Unicode text.

Neither function traps for any valid argument value, invokes a capability, reads ambient state, or allocates a collection proportional to an untrusted declared length beyond the already bounded immutable input.

## Ownership and consumers

The module owns only the Boolean semantic predicates. The assembler owns source token spans and WVA diagnostic selection. The linker owns WVO record offsets and retains a local per-character check where exact invalid-byte reporting is required. Layout padding, address arithmetic, paths, package names, Unicode identifiers, and ABI policy are outside this module.

The two required consumers are:

- `Assembler/Windvale/Wva-Assembler-Core.wv`, for WVA symbol, section, definition, reference, and alignment validation.
- `Linker/Windvale/Wv-Linker-Core.wv`, for WVO section alignment and link entry-name validation.

Both import the module through bounded static source composition. Dependency exports become internal functions in the composed tool WVB; they do not expand either tool's runtime export surface.

## Verification

The standalone module and `Examples/Foundation/Machine-Contracts-Demo.wv` are fixed conformance artifacts. The demo exercises both accepted alignment endpoints, non-power/zero/oversize alignment rejection, accepted machine-name punctuation and trailing digits, empty/leading-digit/disallowed-punctuation rejection, and non-ASCII rejection.

The complete assembler and linker suites remain the consumer tests. Their exact WVO outputs, flat image, canonical map, result reports, no-write failures, and host boundaries must remain equal to the independent Stage 0 oracles on Windows and Debian.

At qualified commit `d46af86`, the standalone module is 2,465 bytes with SHA-256 `9f909a4c47d6f7fb41570b58615a533e79e0219a780c686a64995826b322219a`. The 3,538-byte demo has SHA-256 `b505d3335fa5a4b1dabe2d5e64e4c7a557e0028666cbebe1e2557a0255772f1a` and returns `0`. Windows and Debian produced identical bytes for both artifacts and for the composed assembler and linker modules.
