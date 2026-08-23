# Windvale language support

This directory contains the repository-owned editor support for Windvale source files. It provides a TextMate-compatible `source.windvale` grammar and a Visual Studio Code language configuration for `.wv` files.

The grammar covers the lexical surface in `Specifications/Windvale-Language-1.0-Grammar.md` and `Specifications/Windvale-Language-1.0.ebnf`, while retaining the few compatibility words accepted by the current compiler. It recognizes the complete Language 1.0 reserved-word set and grammar terminals, Unicode identifiers with U+02C9 macron-separated segments, documentation comments, source descriptors, declarations and ownership forms, decimal/hexadecimal/binary integers, decimal and hexadecimal floats, rune/text/byte literals, multiline and zero-to-eight-hash raw literals, calls, member access, delimiters, and the complete operator set. Highlighting is presentation support; the browser and native compilers continue to reject syntax outside their documented implemented profiles.

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
