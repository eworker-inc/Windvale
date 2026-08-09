# Windvale WVO export renamer

## Status and scope

This is a focused hosted Windvale tool for changing one exported function name
in an admitted WVO 1.0 object. It exists to bridge canonical source exports to
explicit link-facing ABI names during .NET retirement. It is not a general object
editor, linker, symbol reordering tool, or format migration layer.

The host entry points are:

```text
Tools/Native/Rename-Wvo-Export.cmd <input.wvo> <old-name> <new-name> <output.wvo>
Tools/Native/Rename-Wvo-Export.sh <input.wvo> <old-name> <new-name> <output.wvo>
```

The launchers admit the exact host application, refuse an existing output, and
remove a newly created output after failure.

## Transformation contract

The portable implementation must:

1. read and verify the complete input through the shared WVO verifier;
2. require nonempty, valid UTF-8 old and new names within WVO string limits;
3. find exactly one exported function whose complete name equals the old name;
4. preserve every byte before and after that string while rewriting its encoded
   length and UTF-8 bytes;
5. verify the complete candidate WVO again; and
6. write the candidate only after every check succeeds.

The tool fails closed for malformed objects, missing or duplicate matching
exports, invalid names, overflow, or a rewritten object that violates canonical
WVO ordering. It does not rename imports, definitions without an export, data,
relocations, or arbitrary byte ranges.

## Retained package identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| `Wvo-Export-Renamer.wvb` | 37,036 | `7429577711817c534b17bfcb083fd136468a3c33b2fd692e28bf6c3bb1642395` |
| `Wvo-Export-Renamer.exe` | 391,680 | `2cf43335af7782676e21ecdd5cb946cb3c9a7309572e21eadac5c7f5d33d2244` |
| `Wvo-Export-Renamer.elf` | 393,216 | `c27787ee970d551ad0d85026ee7f9c0ac9de72d933e563398ac356d5561ed0ae` |

The four-case fixed contract requires exact positive output, missing-export
rejection without output, invalid-name rejection without output, and preservation
of an existing destination. Linux execution remains pending until the grouped
cross-host retirement gate.
