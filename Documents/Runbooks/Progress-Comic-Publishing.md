# Progress comic publishing runbook

This runbook records how a Windvale progress comic is stored, prepared for the website, reviewed, and published. The comic is an editorial explanation of a dated project snapshot. [`Documents/Project/Progress.md`](../Project/Progress.md) remains the authoritative source for implemented and qualified state.

## What is saved locally

Progress stories are ordinary versioned repository files. They are not stored in browser local storage, a database, GitHub metadata, or Cloudflare state.

| Item | Local source | Purpose |
| --- | --- | --- |
| Measured project state | [`Documents/Project/Progress.md`](../Project/Progress.md) | Authoritative facts and current transfer point |
| Full-resolution comic | `Documents/Project/Images/Windvale-Project-Progress-YYYY-MM-DD.png` | Dated archival original used by documents and future re-exports |
| Responsive website images | `Website/assets/progress/windvale-progress-YYYY-MM-DD-640.webp` and `-1120.webp` | Smaller browser downloads for narrow and wide screens |
| Featured-story date, caption, alternative text, and transcript | `Website/index.html` | Accessible homepage presentation |
| Optional overview placement | Root `README.md` | Repository introduction when the same story still represents the project well |

Keep every accepted story under its dated name. Existing editorial files such as `Windvale-Project-Portrait-August-2026.png` retain their historical names when restored to the archive. Adding a new story must not overwrite or silently revise an older editorial snapshot.

## Prepare a story

1. Update the Progress dashboard first, with the evidence that supports the new state.
2. Choose one understandable change or relationship for the comic. Do not try to reproduce the complete dashboard in panels.
3. Date the story for the represented snapshot, even if publication occurs later.
4. Export the full-resolution PNG to `Documents/Project/Images/` with the dated filename above.
5. Review technical labels against the Progress dashboard and current specifications. A comic may simplify an explanation, but it must not imply that proposed work is implemented.

## Create website images

Export two WebP copies that preserve the original aspect ratio: one 640 pixels wide and one no more than 1120 pixels wide. Remove image metadata that is unnecessary for publication. A graphics editor is sufficient. If ImageMagick is installed, the equivalent PowerShell commands are:

```powershell
$ProgressDate = "2026-08-04"
$ProgressSource = "Documents/Project/Images/Windvale-Project-Progress-$ProgressDate.png"

magick $ProgressSource -strip -resize "640x>" -quality 82 "Website/assets/progress/windvale-progress-$ProgressDate-640.webp"
magick $ProgressSource -strip -resize "1120x>" -quality 84 "Website/assets/progress/windvale-progress-$ProgressDate-1120.webp"
magick identify $ProgressSource "Website/assets/progress/windvale-progress-$ProgressDate-640.webp" "Website/assets/progress/windvale-progress-$ProgressDate-1120.webp"
```

Use the reported width and height of the 1120-pixel derivative for the homepage image's intrinsic `width` and `height`. This reserves the correct layout space before the image arrives.

## Update the homepage

In `Website/index.html`:

1. Change both responsive image paths and the fallback image path to the new dated files.
2. Set the image's intrinsic dimensions to the wide derivative's exact dimensions.
3. Update the snapshot date and short caption.
4. Write concise alternative text that explains the comic's overall point.
5. Update the collapsed transcript with the meaning or dialogue of every panel.
6. Keep the link to the authoritative Progress dashboard.

Update the root README only when the new comic is also the best repository overview. The social image `Website/og.png` is a separate editorial asset; replace it only when the public link preview should materially change.

## Website presentation

Keep the latest comic in the homepage hero. The static `/progress/` archive presents every accepted story newest-first with responsive images, a date, a short explanation, and an accessible transcript. It links back to the technical Progress dashboard instead of becoming a second state record. All images are local build inputs; the page does not fetch them from GitHub at request time.

If the archive grows enough to make the repeated HTML difficult to maintain, move the story metadata into one small repository-owned static manifest without changing the public layout or storage boundary.

Two later alternatives remain available:

- attach comic thumbnails directly to a milestone timeline when the order of technical gates is more important than the story itself; or
- add a dedicated panel-by-panel reader after enough stories exist to justify next/previous navigation.

The comic collection is a dated, human-readable layer over the Progress page, not a competing state record.

## Review and publish

Before publication:

1. Check names, dates, technical claims, spelling, panel order, alternative text, and transcript.
2. Inspect the homepage at desktop and mobile widths. Confirm that text in the image remains legible and that the responsive source changes do not shift the layout.
3. Run the focused website verifier:

   ```powershell
   pwsh -NoProfile -File Tools/Verify/Verify-Website.ps1
   ```

4. Commit the original, both website derivatives, homepage metadata, transcript, and any deliberate README update together.
5. Use the normal verified website deployment. No separate image upload or Cloudflare cache purge is required because each story uses new dated asset names.
