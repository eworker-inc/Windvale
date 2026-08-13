# Windvale applications

## Status

This tree owns useful deployable Windvale entry points. Applications are distinct
from reusable code in `Foundation/` and `Libraries/`, developer commands in
`Tools/`, and illustrative or conformance programs in `Examples/` and `Tests/`.

The first application is `Database/Wvdb-Query.wv`. It reads one bounded immutable
WVDB snapshot through `filesystem.directory_read_v1` and reports an integer value
for a supplied `u32` key. It composes the portable decimal parser, the portable
experimental WVDB reader, and the hosted read-only snapshot facade.

## Wvdb Query

The command contract is:

```text
wvdb-query <name.wvdb> <u32-key>
```

Its outcomes are:

| Exit | Meaning |
| ---: | --- |
| `0` | The key was found and its value was printed. |
| `2` | The snapshot was valid but the key was absent. |
| `3` | The rights-limited directory operation failed. |
| `4` | The bytes were read but the WVDB snapshot was invalid. |
| `64` | The arguments were invalid. |

The application declares the exact transitive capability closure at its root:
console output, diagnostic output, one read-only directory instance, and bounded
process argument access. Those declarations are requirements, not runtime grants.

The checked-in Project 2 manifest lives under `Projects/Applications/`; package
and lock metadata live under `Distribution/Applications/`. The current native
package front door deterministically builds and inspects the canonical WVB on
Windows and Linux. Native execution remains a separate successor slice because the
current native runner does not yet bind `filesystem.directory_read_v1`.
