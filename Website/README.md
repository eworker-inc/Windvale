# Windvale website

This directory is the complete website project for <https://windvale.ca/>: its pinned npm package, Vite configuration, static pages, generated documentation and source browsers, and Cloudflare Pages Functions. The home, progress-comic, documentation, source, support, and playground surfaces are static browser applications with no dedicated application server. A narrow Function under `functions/` exposes the approved public supporter roll from Workers KV without handling payments.

The site follows the visitor's operating-system light or dark preference through `prefers-color-scheme`. The browser playground lives below the same origin at <https://windvale.ca/playground/>, so navigation and the saved theme remain continuous. The normal playground and its focused compiler proof are static applications over Windvale-authored WebAssembly; neither starts Blazor or a .NET runtime. During local development, Vite proxies `/playground/` to an independent static Vite server at `http://127.0.0.1:5174/` while the browser stays on the website's `http://127.0.0.1:5173/` origin.

## Local preview

From the repository root, install the website dependencies once and start the website plus playground:

```powershell
npm install --prefix Website
npm --prefix Website run dev
```

Open <http://127.0.0.1:5173/> for the website, <http://127.0.0.1:5173/progress/> for progress comics, <http://127.0.0.1:5173/docs/> for rendered repository documents, <http://127.0.0.1:5173/code/> for the highlighted source tree, or <http://127.0.0.1:5173/playground/> for the same-origin playground. The combined command generates one bounded repository snapshot, builds Monaco and verifies/copies the pinned WebAssembly package, starts the static playground on internal port 5174, and starts the website on port 5173. Restart the site command after repository files change so the generated snapshot is refreshed. Do not open `index.html` directly when checking absolute routes.

Use `npm --prefix Website run dev:site` or `npm --prefix Website run dev:playground` only when debugging one half independently.

The playground can keep several example and scratch tabs open during one visit. Choosing an example opens or focuses its tab, the `+` button creates an independent scratch program, and Compile + Run uses only the active tab. Source and the execution budget remain with each open tab until the page is reloaded; the playground does not persist draft source in browser storage.

The normal website workflow does not invoke the retained C# recovery project. Restarting the combined command after an editor or pinned-package change gives the proxy one consistent static publication; mutable package files revalidate before their identities are accepted.

Run the complete targeted website gate with:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Website.ps1
```

It installs the pinned website and playground Node dependencies, rebuilds the browser editor and its exact third-party notices, verifies the native compiler package and supporter contract, generates and validates the repository snapshot, checks the website tooling syntax, and builds the complete Vite publication. Change-aware local and GitHub verification select this gate for website and browser-packaging paths without running unrelated Seed qualification.

## Repository documents and source

`npm run generate` reads tracked and non-ignored working-tree files and creates the ignored `Generated/` publication input. Deployment runs against a clean checkout of one verified commit. The generator:

- renders every tracked Markdown document through a sanitized HTML boundary;
- resolves tracked relative document links and copies local referenced images to content-addressed asset names;
- publishes the complete repository tree without copying unavailable or unsupported binary payloads;
- applies the repository-owned Windvale TextMate grammar and pinned Shiki grammars to bounded text files; and
- records the exact commit and tree in `deployment.json` and the repository manifest.

The `/docs/` page renders the root `README.md` by default and offers the remaining Markdown tree at the left. `/code/` is a separate read-only source browser. Both surfaces link to exact-commit GitHub pages. Content-addressed document, code, and image assets are immutable; the small manifest and route entry points always revalidate.

## Updating the featured progress story

The homepage is an editorial introduction, not a second progress dashboard. [`Documents/Project/Progress.md`](../Documents/Project/Progress.md) remains the authoritative current-state source; the website shows one dated comic and links to that dashboard, the roadmap, and qualification evidence.

Progress stories are saved as ordinary versioned files: the dated or chapter-named full-resolution original lives in [`Documents/Project/Images/`](../Documents/Project/Images/), responsive WebP derivatives live under `assets/progress/`, and the homepage owns the featured date, caption, alternative text, and transcript. Nothing is saved in browser storage, a database, or Cloudflare state. The `/progress/` page presents the illustrated project history in newest-first order without replacing the technical dashboard, which remains directly available from the primary navigation. The [progress comic publishing runbook](../Documents/Runbooks/Progress-Comic-Publishing.md) gives the complete preparation, image-export, accessibility, archive, review, and publication procedure.

The 1731×909 `preview.png` is the default social preview for X and other link unfurls, while `support-og.png` is the support-page preview. Refresh either deliberately when its public visual direction changes, not automatically with every comic, and keep its Open Graph and X alt text synchronized with the page that uses it. The six 768×512 illustrations under `assets/support/` form a character-led mini-story across the support tiers; keep their shared style, safe crop, and tier meanings coherent when replacing them. `assets/favicon.png` and `assets/apple-touch-icon.png` use the Windvale wind-and-butterfly mark on a dark background so the mark remains visible in browser and device chrome.

The page uses a small self-hosted subset of Google Material Symbols Rounded. Its Apache 2.0 license is stored beside the font in `assets/material-symbols-LICENSE.txt`; visitors do not contact Google to load the icons. The playground build copies the exact Monaco Editor, Monaco third-party, DOMPurify, and Marked notices from the pinned packages into `wwwroot/editor/notices/`. The repository-wide [third-party notice index](../THIRD-PARTY-NOTICES.md) records the ownership boundary.

## Analytics

All public pages load Google's standard asynchronous tag for the Google Analytics 4 stream `G-3PB4LZFMRE`, followed by the shared `analytics.js` configuration bootstrap. The bootstrap records ordinary page views across the home, progress, documents, source, support, playground, and not-found pages while disabling Google signals and ad-personalization signals. Google Analytics can use first-party analytics cookies and receives usage data directly from the visitor's browser; Windvale does not run a separate analytics backend. Cloudflare may also inject its versioned Web Analytics beacon and report its measurements through the same-origin `/cdn-cgi/rum` endpoint. The Content Security Policy permits only the Google Tag and GA4 collection origins needed by this configuration plus Cloudflare's dedicated beacon origin, without advertising endpoints.

## Configuring support

The support page lives at `/support/`. Its six one-time tiers, USD currency declaration, and public Stripe Payment Link URLs are the only values maintained in the versioned `support-data-*.js` module. Version that module and its importing support-page module together when changing public tier data or artwork URLs so deployed browsers cannot retain a stale dependency chain. Empty or invalid checkout URLs fail closed and render as unavailable; the page accepts only public `https://buy.stripe.com/` links. Create all six Stripe prices in USD. Stripe can localize fixed tiers through Adaptive Pricing, while the choose-your-own-amount link remains explicitly USD with a $50 starting amount and a $1–$10,000 range.

Configure every Stripe Payment Link consistently:

- collect individual and business names;
- add a required `Public supporter recognition` dropdown with `List my name and support tier publicly` and `Keep my support anonymous` choices;
- enable Stripe receipts; and
- redirect successful payments to `/support/#supporters-title` until a dedicated thank-you route is needed.

The name and tier are published only after an explicit opt-in and manual review. The website never publishes email addresses, Stripe identifiers, payment details, private notes, or raw checkout fields.

### Public supporter roll

The Cloudflare Pages project uses a KV binding named `WINDVALE_SUPPORTERS`. The Function at `/api/supporters` reads one versioned JSON value under `public-supporters-v1`, validates every field, and returns only the public contract:

```json
{
  "version": 1,
  "updated": "2026-08-02",
  "supporters": [
    {
      "displayName": "Approved public name",
      "tier": "builder",
      "since": "2026-08"
    }
  ],
  "anonymousCounts": {
    "cornerstone": 0,
    "champion": 0,
    "accelerator": 0,
    "builder": 0,
    "spark": 0,
    "any": 0
  }
}
```

Keep the editable JSON source outside this public repository. Validate it with `node Tools/Website/Publish-Supporters.mjs --dry-run <outside-repository-path>` and publish it after review by setting `CLOUDFLARE_ACCOUNT_ID`, `CLOUDFLARE_API_TOKEN`, and `WINDVALE_SUPPORTERS_NAMESPACE_ID`, then running the same command without `--dry-run`. The tool reports counts but never prints supporter names or credentials.

## Publication

The `Deploy verified website snapshot` workflow checks four times per day at 00:17, 06:17, 12:17, and 18:17 UTC. It publishes only when the exact current `main` commit has a successful push-triggered `Verify` run and differs from `deployment.json` on the live site. An unchanged or unverified commit stops before checkout, dependency installation, compilation, or Cloudflare publication. A manual dispatch retains the same verification requirement and can explicitly force a rebuild of an already-live commit.

For a real deployment, the workflow builds `Website/dist`, assembles it with the published browser playground under `playground/`, and runs Wrangler from this directory so the project-root `functions/` folder is bundled. The combined artifact is published to the `windvale-ca` Cloudflare Pages project. Cloudflare owns the `windvale.ca` zone and supplies HTTPS for the apex site. Payments remain on Stripe; the only server-side website behavior is the read-only supporter-roll Function.

`npm --prefix Website run verify:wasm-compiler-package` independently checks the pinned `Artifacts/WebAssembly-Playground` package without .NET. It validates the native and segmented provenance manifests, both browser artifact identities, the import-free fixed-memory ABI-4 compiler and ABI-3 interpreter contracts, and the direct compiler's exact 183-byte WVB result. Normal website publication verifies and copies this package. Reconstructing the segmented generator Wasm remains an explicit Stage 0 recovery operation; normal direct-compiler publication is Windvale-native and .NET-free.

The playground's npm build copies only the two manifest-owned Wasm files and their manifest into `wwwroot/compiler-package/`. The generated directory is ignored locally and becomes part of the static playground publication; no compiler or .NET process runs during this packaging step.

The `/playground/` route uses the static, .NET-free source-to-result worker. It loads both Wasm modules by manifest, compiles canonical single-module `WVSS 1` source directly to `WVCO 1` WVB, resubmits the untrusted WVB for verification and scalar execution, and terminates after success, failure, or a five-minute containment timeout. The separate compiler-proof page has been retired. The pinned sample compiles to the exact 183-byte WVB and returns `42`; direct compiler execution measures about one second on the Windows development host, while initial 18.35 MB download and module compilation vary by browser and cache. `verify:wasm-compiler-demo` checks static containment, and `verify:wasm-compiler-core` executes the exact compile/verify/run pipeline under Node.js's optimizing WebAssembly tier.

Normal deployment copies `Tools/Windvale.Playground/wwwroot` directly after the npm build; it does not install .NET or run `dotnet publish`. Mutable entry points, JavaScript modules, styles, editor bundles, the package manifest, and package artifacts request revalidation, and the Cloudflare zone's Browser Cache TTL remains set to `Respect Existing Headers`. Every fetched compiler artifact is checked against the revalidated manifest's exact byte length and SHA-256 identity before use. A missing or mixed-release asset is a publication or caching fault, not a reason to weaken verification.

Cloudflare credentials remain outside the repository. Automation receives only the scoped API token and account identifier through GitHub Actions secrets.
