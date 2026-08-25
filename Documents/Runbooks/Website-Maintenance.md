# Website maintenance

This runbook temporarily replaces every public `windvale.ca` route with one
bounded maintenance response while the normal website and repository continue
to evolve privately. The cover does not delete or rewrite the ordinary website.

## Contract

- `Website/Maintenance/_worker.js` owns every request while the cover is live.
- Every method and path returns `503 Service Unavailable` with `Retry-After`,
  `Cache-Control: no-store`, and `X-Robots-Tag: noindex, nofollow, noarchive`.
- The response is self-contained, script-free, at most 16 KiB, and exposes no
  repository snapshot, supporter data, playground, download, or external link.
- The `WINDVALE_WEBSITE_MODE=maintenance` repository variable is the persistent
  deployment lock. The ordinary scheduled publisher skips its entire job while
  that exact value is present.
- The maintenance and ordinary workflows share the `windvale-homepage`
  concurrency group, so their Cloudflare publications cannot overlap.

Previously downloaded files, browser caches, search-engine copies, external
archives, and local Git clones cannot be recalled by a maintenance deployment.
Cloudflare may retain old cached assets for up to one week, but the maintenance
Worker intercepts new requests to their URLs.

## Enter maintenance

1. Confirm the maintenance source passes the website verifier and that the
   maintenance workflow is present on `main`.
2. Set repository variable `WINDVALE_WEBSITE_MODE` to the exact value
   `maintenance` before dispatching any deployment.
3. Dispatch `Deploy website maintenance cover` and require a successful run.
4. Probe `/`, `/docs/`, `/code/`, `/playground/`, `/api/supporters`,
   `/deployment.json`, and one formerly published immutable source-asset URL.
   Require status 503 and the maintenance cache, retry, and robots headers.
5. Only after those probes pass, change repository visibility or begin work
   that must remain outside the public website snapshot.

Do not rely on the maintenance workflow alone as a lock. It publishes one
deployment; the repository variable prevents later scheduled ordinary
deployments from replacing it.

## Work under maintenance

Normal website source remains under `Website/`, the playground remains under
`Tools/Windvale.Playground/`, and independently deployable applications remain
under `Applications/Web/`. Make and verify changes in those ordinary owners.
Do not edit the maintenance response to preview the next website.

GitHub-hosted workflow use becomes metered while the repository is private.
Preserve passing evidence, avoid redundant runs, and keep the website lock set
until one exact replacement commit has passed its required verification.

## Restore the website

1. Push the complete replacement website and normally require its exact
   `Verification gate` to pass. During the bounded compiler 0.1-to-1.0
   migration only, an operator may instead use the manual
   `allow_unverified_compiler_migration=true` input when the known failing
   evidence belongs to that migration. This exception does not qualify the
   compiler and does not bypass the website, playground, or WebAssembly
   publication checks.
2. Set `WINDVALE_WEBSITE_MODE` to `live` immediately before publication. The
   ordinary workflow treats an absent value or any value other than
   `maintenance` as live for backward compatibility.
3. Dispatch `Deploy verified website snapshot` with `force=true`. Set
   `allow_unverified_compiler_migration=true` only for the migration exception
   described above; scheduled runs never use it.
4. Require `/deployment.json` to name the intended commit. Probe the home,
   documents, source, support, playground, not-found, and supporter API routes.
5. If publication or probing fails, restore the exact `maintenance` value and
   dispatch the maintenance workflow again before diagnosing the normal site.
6. After the replacement is confirmed, make the repository public when that is
   the intended project state. Recheck branch protection, required status
   checks, security features, release downloads, raw installer URLs, and public
   Actions evidence after the visibility change.
