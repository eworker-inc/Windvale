# Windvale website

This directory contains the static public home page for <https://windvale.ca/>. It intentionally has no application server, build framework, package manager, cookies, analytics, or runtime dependency.

The site follows the visitor's operating-system light or dark preference through `prefers-color-scheme`. The browser playground remains an independently deployed application at <https://play.windvale.ca/>.

## Local preview

Serve this directory through any static HTTP server. Do not open `index.html` directly when checking security headers or absolute routes.

## Publication

The `Deploy homepage` GitHub Actions workflow publishes this directory to the `windvale-ca` Cloudflare Pages project after relevant changes reach `main`. Cloudflare owns the `windvale.ca` zone and supplies HTTPS for the apex site.

Cloudflare credentials remain outside the repository. Automation receives only the scoped API token and account identifier through GitHub Actions secrets.
