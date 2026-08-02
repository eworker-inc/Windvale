# Windvale website

This directory contains the static public home page for <https://windvale.ca/>. The production site intentionally has no application server, cookies, analytics, or runtime dependency. Vite is used only as a convenient local development server.

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

## Publication

The `Deploy homepage` GitHub Actions workflow assembles this directory with the published browser playground under `playground/`, then publishes the combined artifact to the `windvale-ca` Cloudflare Pages project after relevant changes reach `main`. Cloudflare owns the `windvale.ca` zone and supplies HTTPS for the apex site. No application server is involved.

Cloudflare credentials remain outside the repository. Automation receives only the scoped API token and account identifier through GitHub Actions secrets.
