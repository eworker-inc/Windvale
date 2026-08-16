# Windvale segmented hosted overlay contract

## Status and purpose

Version 1 defines a narrow hosted-storage composition boundary for a generated
Windvale application whose canonical flat image is larger than the ordinary
4 MiB WVO/linker value limit. It lets database verification attach the existing
native random-access-storage provider without weakening validation of either
the generated application or the provider objects.

This is not a general segmented multi-object linker, a dynamic linker, or a
capability grant. The hosted package still binds exactly one rights-limited
random-access-storage object. The overlay only joins already validated machine
code that uses that provider table.

## Inputs and limits

`Compose-Segmented-Hosted-Overlay` accepts five explicit paths:

```text
<application-chunk-prefix> <application.wvli>
<common-provider.wvo> <platform-provider.wvo> <output-chunk-prefix>
```

The application must be one canonical `WVLI 1.0` image with:

- one through eight contiguous fragments;
- every non-final fragment exactly 4,194,304 bytes;
- no fragment larger than 4,194,304 bytes;
- one `Main` entry strictly inside the declared image; and
- no more than 33,554,432 complete image bytes.

The implementation reopens every declared chunk as one regular, non-reparse
file and checks its index, position, exact length, and full image coverage.
Every possible output fragment must be absent before provider construction.

The common and selected platform provider remain ordinary WVO inputs. The
ordinary Wv linker therefore continues to own their envelope and section
validation, duplicate and unresolved symbol rejection, function-kind checks,
alignment, relocation arithmetic, placeholder checks, and 4 MiB output limit.
The provider link uses a base address aligned to 16 bytes immediately after the
application image and requires exactly one exported `Storage_host_entry`.

## Main trampoline

The provider imports application `Main`, but the large compiler image cannot
be passed back through the ordinary multi-object linker. A separate five-byte
WVA object supplies one exported `Main` placeholder:

```text
E9 FB FF FF FF
```

The ordinary linker resolves every provider import against this trampoline.
Composition then requires exactly one map record for a five-byte exported
`Main`, reopens the provider image, verifies the complete placeholder, and
replaces only its four-byte signed `rel32` displacement. Its target is the
already verified application entry:

```text
application-entry - (trampoline-address + 5)
```

The displacement must fit `i32`. No other application or provider byte is
patched.

## Output and failure behavior

The provider starts after zero through fifteen zero alignment bytes. Its
complete image must fit both the 32 MiB total-image ceiling and unused space in
the application's final 4 MiB fragment. The fragment count does not change.

Composition copies earlier application fragments unchanged, streams the final
application fragment, alignment, and provider into private sibling files, and
checks the final length before publication. Successful publication moves only
those checked files to the requested absent names. An ordinary invocation does
not replace an existing output. A process or filesystem failure during the
final series of same-directory moves may leave only a prefix of output chunks;
the result has no completion manifest and must not be treated as a complete
image.

The completion line reports application, provider, and final image sizes;
fragment count; application and provider entries; trampoline address; and
alignment bytes. Hosted packaging consumes the fragment count and provider
entry directly and independently validates the fragment geometry again.

The PowerShell implementation keeps the small `WVLI`, one streamed source
fragment, and the provider image active rather than joining the full generated
application in memory. The shell implementation uses bounded file copies and
append operations. Neither implementation reconstructs the multi-megabyte WVO
as one in-memory Windvale value.

## Deliberate limits

Version 1 accepts one common/platform storage-provider pair, one trampoline,
and one already linked application. It does not emit a new `WVLI`, add a ninth
fragment, split a provider across fragments, accept an arbitrary provider
family, or prove durable publication. Generalize this boundary only when a
second real hosted-provider family supplies exact composition and verification
requirements.
