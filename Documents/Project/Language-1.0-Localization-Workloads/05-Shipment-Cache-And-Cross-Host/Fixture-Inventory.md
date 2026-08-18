# Release 1 localization fixture inventory

## Exact content objects

This inventory combines the admitted `en@1` chain from Workload 1 and the
`zh-Hans@1` chain from Workload 2. SHA-256 covers exact bytes including final LF.

| Selection | Artifact | Bytes | SHA-256 |
| --- | --- | ---: | --- |
| shared | `Unicode-17-Source.wvup` | 2,487 | `2772f22969972eaae52058ce2cad0f2a716181c494a2b422188cbe5215e08ce9` |
| shared | `Language-1-Keyword-Tokens.wvktr` | 1,511 | `cefa459df5dcaf22b53670a81f09c6f696188903c8e136d763da869a5ec6286e` |
| English | `En-Keywords.wvlex` | 2,139 | `25049aab3ecdbdeedcedfdada97247782bf42de6f861c6bdfb0d3fe86a24b89a` |
| English | `En-Vocabulary.wvsvp` | 296 | `2bc0e795b812755056cf08cfdef0b985d9750273dcc70ab470d992ebcfe830f3` |
| English | `En-Foundation-Option.wvcat` | 1,158 | `91f5d7c00167678e5b4bd9bb0a77ca992f18cc8641d35d5eaaf7651e5abacd37` |
| English | `En-Source-Profile.wvsp` | 508 | `e678b1b5daae2c0d87179f2fcd162b1b002cebe8617fc0fb155a5b78a1bdaf27` |
| Chinese | `Zh-Hans-Keywords.wvlex` | 2,183 | `e8cf8c33fa2ce683f4648babd3988a968cb4fc8a18535bb80583c8e5925e258c` |
| Chinese | `Zh-Hans-Vocabulary.wvsvp` | 301 | `efe3e4073b3e639f6ac9820acda569b68af40c90aab8d013962f259168053cb0` |
| Chinese | `Zh-Hans-Foundation-Option.wvcat` | 1,182 | `a3cf34fb3dafddbd85fc75fe748a117d821fe3846debf3483e383a9909de3f6a` |
| Chinese | `Zh-Hans-Source-Profile.wvsp` | 523 | `a58160dea9f70aa0f006cbf6bbe76ffb352e5481d13f8385f58141389f6f0ffe` |

## Exact totals

| Selection | Unique bytes |
| --- | ---: |
| Shared edition inputs | 3,998 |
| English-specific fixture | 4,101 |
| Minimal English fixture (`shared + English`) | 8,099 |
| Incremental Chinese fixture | 4,189 |
| Both Release 1 fixture profiles | 12,288 |

The synthetic `test-Unicode@1` engineering profile and review/build locks are not
Release 1 language selections and are excluded from these totals.

## Honest extrapolation boundary

The fixture contains one complete 16-label Foundation interface catalog. A real
Release 1 pack contains one exact catalog for every shipped source-addressable
Foundation interface. Its semantic source bytes therefore follow:

~~~text
shared edition objects
+ one profile manifest
+ one 66-keyword lexicon
+ one vocabulary-profile artifact
+ sum(complete interface-bound catalogs)
~~~

Diagnostic messages, API prose, tutorials, search indexes, fonts, and IME data
are separate experience/documentation selections and may be much larger. They do
not enter compiler semantic inputs or runtime packages.

The 2,487-byte `.wvup` records Unicode source/provenance inputs; it is not the
compiler's generated lookup-table storage. An implementation carries those
tables once in the compiler/core SDK or one shared core object and accounts for
them there, never once per language pack.

These figures explain an important package-size boundary: language-profile data
is measured in kilobytes for the current fixture. Hundreds of megabytes in a
developer shipment come from native compiler/runtime products, debug/provenance
material, duplicated target products, installers, or other tools—not from these
ten localization objects.
