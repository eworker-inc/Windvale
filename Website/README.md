# Windvale website

This directory contains the public project home for <https://windvale.ca/>. The home, support page, and playground are static browser applications with no dedicated application server. A narrow Cloudflare Pages Function exposes the approved public supporter roll from Workers KV without handling payments. Vite is used only as a convenient local development server.

The site follows the visitor's operating-system light or dark preference through `prefers-color-scheme`. The browser playground lives below the same origin at <https://windvale.ca/playground/>, so navigation and the saved theme remain continuous. During local development, Vite proxies `/playground/` to the independent Blazor development server at `http://127.0.0.1:5174/` while the browser stays on the website's `http://127.0.0.1:5173/` origin.

## Local preview

From the repository root, install the local development dependencies once and start the website plus playground:

```powershell
npm install
npm run dev
```

Open <http://127.0.0.1:5173/> for the website or <http://127.0.0.1:5173/playground/> for the same-origin playground. `npm run dev` builds the Monaco bundle, starts Blazor on the internal port 5174, and starts Vite on port 5173. Vite refreshes the website as files change. Do not open `index.html` directly when checking absolute routes.

Use `npm run dev:site` or `npm run dev:playground` only when debugging one half independently.

## Updating the progress cards

All public component and development-milestone progress lives in `project-progress.js`. To update the dashboard, change an item's `percent`, `status`, `details`, and `next` values plus `PROGRESS_UPDATED`. The page creates both sets of cards, progress bars, and accessible tooltips automatically; no HTML or CSS change is needed.

The page uses a small self-hosted subset of Google Material Symbols Rounded. Its Apache 2.0 license is stored beside the font in `assets/material-symbols-LICENSE.txt`; visitors do not contact Google to load the icons.

## Analytics

All public pages load the shared `analytics.js` bootstrap for the Google Analytics 4 stream `G-3PB4LZFMRE`. The bootstrap records ordinary page views across the home, support, playground, and not-found pages while disabling Google signals and ad-personalization signals. Google Analytics can use first-party analytics cookies and receives usage data directly from the visitor's browser; Windvale does not run a separate analytics backend. The Content Security Policy permits only the Google Tag and GA4 collection origins needed by this configuration, without advertising endpoints.

## Configuring support

The support page lives at `/support/`. Its six one-time tiers, USD currency declaration, and public Stripe Payment Link URLs are the only values maintained in `support-data.js`. Empty or invalid checkout URLs fail closed and render as unavailable; the page accepts only public `https://buy.stripe.com/` links. Create all six Stripe prices in USD. Stripe can localize fixed tiers through Adaptive Pricing, while the choose-your-own-amount link remains explicitly USD with a $50 starting amount and a $1–$10,000 range.

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

The `Deploy homepage` GitHub Actions workflow assembles this directory with the published browser playground under `playground/`, bundles the repository-root Pages Functions, then publishes the combined artifact to the `windvale-ca` Cloudflare Pages project after relevant changes reach `main`. Cloudflare owns the `windvale.ca` zone and supplies HTTPS for the apex site. Payments remain on Stripe; the only server-side website behavior is the read-only supporter-roll Function.

Cloudflare credentials remain outside the repository. Automation receives only the scoped API token and account identifier through GitHub Actions secrets.
