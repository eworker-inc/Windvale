# Windvale applications

## Status

This tree owns useful deployable Windvale entry points. Applications are distinct
from reusable code in `Foundation/` and `Libraries/`, developer commands in
`Tools/`, and illustrative or conformance programs in `Examples/` and `Tests/`.

The first applications are `Database/Wvdb-Query.wv` and `Shell/Echo.wv`.
`Wvdb-Query` reads one bounded immutable WVDB snapshot through
`filesystem.directory_read_v1`; `Echo` is the first ordinary application in the
accepted Windvale Shell 1 catalog.

## Echo

The command writes its immutable arguments separated by one ASCII space and one
final LF. Zero arguments write one LF. Empty arguments and strict Unicode text
are preserved rather than reparsed as a host command line.

`Echo` declares exactly standard line output plus bounded process argument and
argument-count access. It has no filesystem, diagnostic, environment,
native-process, or ambient path authority. The focused `echo-application` owner
builds deterministic Windows and Linux hosted applications and executes nine
success and boundary cases independently on both hosts. Exact Package 1, Lock 1,
Bundle 1, Approval 1, Launch Record 3, and Generation 1 records now bind `echo`
through the Windvale-written resolver and the guarded Windows/Linux dispatcher.
The separate ten-case `echo-command-launch` owner proves execution, rejection,
and private-host cleanup. An interactive shell remains a later integration.

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
Windows and Linux. The focused capability owner also binds the rights-reduced
read-only directory provider and executes the application on both hosts.
