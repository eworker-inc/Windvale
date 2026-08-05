# Third-party notices

The [Windvale Community Source License](LICENSE.md) applies only to Windvale-owned
material to which that license is expressly applied. Third-party components
remain under their separately identified licenses, and those terms take
precedence for the third-party material.

## Distributed website and playground components

- **Google Material Symbols Rounded** — Apache License 2.0. The self-hosted
  font subset and a complete license copy are under `Website/assets/`.
- **Monaco Editor 0.56.0** — MIT License, copyright Microsoft Corporation.
  The playground build copies Monaco's `LICENSE.txt` and
  `ThirdPartyNotices.txt` into its published `editor/notices/` directory.
- **DOMPurify 3.4.12** — available under MPL-2.0 or Apache-2.0. Windvale uses
  the Apache-2.0 option. The playground build copies the package's `LICENSE`
  into its published `editor/notices/` directory.
- **Marked 14.0.0** — MIT and BSD-style notices for Marked and Markdown. The
  playground build copies the package's `LICENSE.md` into its published
  `editor/notices/` directory.

The exact installed versions and integrity hashes are recorded in
`Tools/Windvale.Playground/package-lock.json`.

The website snapshot generator uses **Marked 18.0.9** (MIT),
**sanitize-html 2.17.6** (MIT), and **Shiki 4.4.2** (MIT) at build time to
render, sanitize, and highlight repository content. Their exact versions and
integrity hashes are recorded in `Website/package-lock.json`; the generated
publication does not ship their JavaScript runtimes.

## Build, test, and platform dependencies

Windvale also uses third-party SDKs, packages, firmware, emulators, and build
tools. Their presence does not place them under the Windvale license. Package
manifests, lock files, separately carried notices, and upstream distributions
identify their applicable terms. A Windvale distribution must include the
notices required for every third-party component it actually redistributes.
