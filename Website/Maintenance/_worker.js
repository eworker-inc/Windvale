export const Maintenanceˉhtml = `<!doctype html>
<html lang="en">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <meta name="robots" content="noindex, nofollow, noarchive">
    <meta name="color-scheme" content="dark">
    <title>Windvale · Temporary maintenance</title>
    <style>
        :root {
            color-scheme: dark;
            font-family: Inter, ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
            background: #071112;
            color: #e8f3ee;
        }

        * {
            box-sizing: border-box;
        }

        body {
            min-height: 100vh;
            margin: 0;
            display: grid;
            place-items: center;
            padding: 2rem;
            background:
                radial-gradient(circle at 18% 18%, rgba(74, 178, 149, 0.16), transparent 34rem),
                radial-gradient(circle at 82% 82%, rgba(94, 133, 174, 0.12), transparent 30rem),
                #071112;
        }

        main {
            width: min(42rem, 100%);
            padding: clamp(2rem, 7vw, 4rem);
            border: 1px solid rgba(161, 211, 192, 0.22);
            border-radius: 1.5rem;
            background: rgba(10, 25, 25, 0.84);
            box-shadow: 0 2rem 6rem rgba(0, 0, 0, 0.36);
        }

        .mark {
            width: 3rem;
            height: 3rem;
            display: grid;
            place-items: center;
            margin-bottom: 2rem;
            border: 1px solid rgba(161, 211, 192, 0.35);
            border-radius: 50%;
            color: #9fe1c7;
            font-size: 1.35rem;
            font-weight: 700;
            letter-spacing: -0.06em;
        }

        .eyebrow {
            margin: 0 0 0.75rem;
            color: #9fe1c7;
            font-size: 0.78rem;
            font-weight: 700;
            letter-spacing: 0.15em;
            text-transform: uppercase;
        }

        h1 {
            margin: 0;
            max-width: 12ch;
            font-size: clamp(2.3rem, 8vw, 4.75rem);
            font-weight: 650;
            letter-spacing: -0.055em;
            line-height: 0.98;
        }

        p {
            max-width: 35rem;
            margin: 1.6rem 0 0;
            color: #b8cbc4;
            font-size: clamp(1rem, 2.6vw, 1.15rem);
            line-height: 1.7;
        }

        .status {
            display: inline-flex;
            align-items: center;
            gap: 0.6rem;
            margin-top: 2.25rem;
            color: #d6e8e0;
            font-size: 0.9rem;
        }

        .status::before {
            width: 0.6rem;
            height: 0.6rem;
            border-radius: 50%;
            background: #70cbaa;
            box-shadow: 0 0 1rem rgba(112, 203, 170, 0.75);
            content: "";
        }
    </style>
</head>
<body>
    <main>
        <div class="mark" aria-hidden="true">W</div>
        <p class="eyebrow">Windvale</p>
        <h1>Temporarily offline for maintenance.</h1>
        <p>We are preparing the repository and public website for their next update. Development continues, but the public surfaces are unavailable for a short time.</p>
        <p class="status">No action is needed. Please check back later.</p>
    </main>
</body>
</html>
`;

const RESPONSE_HEADERS = Object.freeze({
    "Cache-Control": "no-store, max-age=0",
    "Content-Security-Policy": "default-src 'none'; style-src 'unsafe-inline'; base-uri 'none'; form-action 'none'; frame-ancestors 'none'",
    "Content-Type": "text/html; charset=utf-8",
    "Cross-Origin-Resource-Policy": "same-origin",
    "Permissions-Policy": "camera=(), geolocation=(), microphone=(), payment=(), usb=()",
    "Referrer-Policy": "no-referrer",
    "Retry-After": "3600",
    "X-Content-Type-Options": "nosniff",
    "X-Frame-Options": "DENY",
    "X-Robots-Tag": "noindex, nofollow, noarchive",
});

export default {
    fetch(Request) {
        const Body = Request.method === "HEAD" ? null : Maintenanceˉhtml;
        return new Response(Body, {
            status: 503,
            statusText: "Service Unavailable",
            headers: RESPONSE_HEADERS,
        });
    },
};
