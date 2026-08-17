# Windvale language support

This directory contains the repository-owned editor support for Windvale source files. It provides a TextMate-compatible `source.windvale` grammar and a Visual Studio Code language configuration for `.wv` files.

The grammar follows the implemented Windvale Seed lexical contract in `Specifications/Seed-Language.md` and `Specifications/Compiler-Source-Lexer.md`. It recognizes declarations, qualified import aliases with `as`, typed `const` definitions, records, enums, payload variants, exhaustive `match`, exact `try` propagation, bounded `sequence`/`builder` types, `push`, `freeze`, `for`/`in`, loop control, named-record literal types and fields, profiles, built-in types, Boolean and numeric literals, strict string escapes, `//` comments, arithmetic, bitwise, shift, short-circuit, equality, and compound-assignment operators, calls, member access, inferred or explicitly typed `let`/`var` definitions, and Windvale's U+02C9 macron-separated identifiers. Multiline trailing commas reuse the existing punctuation scopes and require no editor-only syntax rule.

WVA textual assembly is deliberately outside this package. `.wva` has a separate syntax and should receive a separate grammar rather than being classified as Windvale source.

## Preview in Visual Studio Code

From the repository root, start an Extension Development Host with:

```powershell
code --extensionDevelopmentPath=Tools/Editors/Windvale .
```

Open any `.wv` file in that window. Visual Studio Code should report the language mode as `Windvale` and apply the active theme to the grammar scopes.

## Build and install a local VSIX

Packaging requires Node.js and downloads the Visual Studio Code extension packager. The resulting VSIX belongs under the ignored `artifacts/` directory rather than in Git:

```powershell
Push-Location Tools/Editors/Windvale
npx --yes @vscode/vsce package --out ../../../artifacts/windvale-language-0.1.0.vsix
Pop-Location
code --install-extension artifacts/windvale-language-0.1.0.vsix
```

Run the dependency-free repository contract check after changing the grammar or package metadata:

```powershell
pwsh -NoProfile -File Tools/Editors/Verify-Windvale-Editor.ps1
```

## GitHub status

GitHub does not load grammars from individual repositories. This grammar is maintained here for local editor use and as the future upstream grammar source for GitHub Linguist. Windvale can be submitted to Linguist after its public `.wv` usage satisfies Linguist's admission requirements; until then, `.gitattributes` must not misclassify `.wv` as another language merely to alter repository statistics.
