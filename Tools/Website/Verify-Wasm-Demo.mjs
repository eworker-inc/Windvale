import { createHash } from "node:crypto";
import { access, readdir, readFile } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

const Scriptˉdirectory = path.dirname(fileURLToPath(import.meta.url));
const Repositoryˉroot = path.resolve(Scriptˉdirectory, "../..");
const Demoˉdirectory = path.join(
    Repositoryˉroot,
    "Tools/Windvale.Playground/wwwroot/wasm-demo",
);
const Previewˉsource = Object.freeze({
    relativePath: "Website/preview.png",
    sha256: "47fecbc4a2e0c3c4f0a853715e9ee30377ad95b7b6d6b9f16877645d25d6c460",
    width: 1731,
    height: 909,
    format: "png",
});
const Previewˉwide = Object.freeze({
    relativePath: "Website/preview-2026-08-06.jpg",
    url: "https://windvale.ca/preview-2026-08-06.jpg",
    sha256: "52e1338fe5095e361c0b7619c4a6ff1854fe688f4c67bc749eef2e513384dacd",
    width: 1200,
    height: 630,
    format: "jpeg",
});
const Previewˉsquare = Object.freeze({
    relativePath: "Website/preview-square-2026-08-06.png",
    url: "https://windvale.ca/preview-square-2026-08-06.png",
    sha256: "60399f6e0349835afe18bcc6060a98ea070f65ef798eab89bac27c52a3bb5374",
    width: 512,
    height: 512,
    format: "png",
});
const Supportˉpreviewˉurl = "https://windvale.ca/support-og.png";

function Readˉpngˉdimensions(Bytes) {
    if (Bytes.length < 24 || Bytes.subarray(1, 4).toString("ascii") !== "PNG") {
        throw new Error("The approved PNG preview has an invalid signature or header.");
    }
    return Object.freeze({
        width: Bytes.readUInt32BE(16),
        height: Bytes.readUInt32BE(20),
    });
}

function Readˉjpegˉdimensions(Bytes) {
    if (Bytes.length < 4 || Bytes[0] !== 0xff || Bytes[1] !== 0xd8) {
        throw new Error("The approved JPEG preview has an invalid signature.");
    }

    const Startˉofˉframeˉmarkers = new Set([
        0xc0, 0xc1, 0xc2, 0xc3,
        0xc5, 0xc6, 0xc7,
        0xc9, 0xca, 0xcb,
        0xcd, 0xce, 0xcf,
    ]);
    let Offset = 2;
    while (Offset < Bytes.length) {
        if (Bytes[Offset] !== 0xff) {
            throw new Error("The approved JPEG preview has an invalid marker boundary.");
        }
        while (Offset < Bytes.length && Bytes[Offset] === 0xff) {
            Offset += 1;
        }
        if (Offset >= Bytes.length) {
            break;
        }

        const Marker = Bytes[Offset];
        Offset += 1;
        if (Marker === 0xd9 || Marker === 0xda) {
            break;
        }
        if (Marker === 0x01 || (Marker >= 0xd0 && Marker <= 0xd8)) {
            continue;
        }
        if (Offset + 2 > Bytes.length) {
            throw new Error("The approved JPEG preview has a truncated segment length.");
        }

        const Segmentˉlength = Bytes.readUInt16BE(Offset);
        if (Segmentˉlength < 2 || Offset + Segmentˉlength > Bytes.length) {
            throw new Error("The approved JPEG preview has an invalid segment range.");
        }
        if (Startˉofˉframeˉmarkers.has(Marker)) {
            if (Segmentˉlength < 7) {
                throw new Error("The approved JPEG preview has a truncated frame header.");
            }
            return Object.freeze({
                width: Bytes.readUInt16BE(Offset + 5),
                height: Bytes.readUInt16BE(Offset + 3),
            });
        }
        Offset += Segmentˉlength;
    }

    throw new Error("The approved JPEG preview does not declare its dimensions.");
}

async function Verifyˉpreviewˉasset(Contract) {
    const Bytes = await readFile(path.join(Repositoryˉroot, Contract.relativePath));
    if (createHash("sha256").update(Bytes).digest("hex") !== Contract.sha256) {
        throw new Error(`${Contract.relativePath} does not match the approved image.`);
    }
    const Dimensions = Contract.format === "jpeg"
        ? Readˉjpegˉdimensions(Bytes)
        : Readˉpngˉdimensions(Bytes);
    if (Dimensions.width !== Contract.width || Dimensions.height !== Contract.height) {
        throw new Error(
            `${Contract.relativePath} dimensions are not ${Contract.width} by ${Contract.height} pixels.`,
        );
    }
}

const Publicˉsources = await Promise.all([
    "README.md",
    "Website/_headers",
    "Website/sitemap.xml",
    "Tools/Windvale.Playground/README.md",
    "Tools/Windvale.Playground/wwwroot/index.html",
].map(Relativeˉpath => readFile(path.join(Repositoryˉroot, Relativeˉpath), "utf8")));

if (Publicˉsources.some(Source => Source.toLowerCase().includes("wasm-demo"))) {
    throw new Error("A public website source still links to the retired direct Wasm demo.");
}

let Demoˉentries;
try {
    Demoˉentries = await readdir(Demoˉdirectory);
} catch (Failure) {
    if (Failure?.code !== "ENOENT") {
        throw Failure;
    }
    Demoˉentries = [];
}
if (Demoˉentries.length !== 0) {
    throw new Error("The retired direct Wasm demo still contains public files.");
}

await Promise.all([
    Verifyˉpreviewˉasset(Previewˉsource),
    Verifyˉpreviewˉasset(Previewˉwide),
    Verifyˉpreviewˉasset(Previewˉsquare),
]);

for (const Relativeˉpath of [
    "Website/index.html",
    "Website/progress/index.html",
    "Website/docs/index.html",
    "Website/code/index.html",
    "Tools/Windvale.Playground/wwwroot/index.html",
]) {
    const Source = await readFile(path.join(Repositoryˉroot, Relativeˉpath), "utf8");
    for (const Required of [
        '<html lang="en" itemscope itemtype="https://schema.org/WebPage">',
        'itemprop="inLanguage" content="en-CA"',
        'property="og:locale" content="en_CA"',
        `property="og:image" content="${Previewˉwide.url}"`,
        `property="og:image:secure_url" content="${Previewˉwide.url}"`,
        'property="og:image:type" content="image/jpeg"',
        'property="og:image:width" content="1200"',
        'property="og:image:height" content="630"',
        `property="og:image" content="${Previewˉsquare.url}"`,
        `property="og:image:secure_url" content="${Previewˉsquare.url}"`,
        'property="og:image:type" content="image/png"',
        'property="og:image:width" content="512"',
        'property="og:image:height" content="512"',
        'name="twitter:card" content="summary_large_image"',
        `name="twitter:image" content="${Previewˉwide.url}"`,
        `rel="image_src" href="${Previewˉwide.url}"`,
        `itemprop="image primaryImageOfPage" href="${Previewˉwide.url}"`,
        `itemprop="thumbnailUrl" href="${Previewˉsquare.url}"`,
    ]) {
        if (!Source.includes(Required)) {
            throw new Error(`${Relativeˉpath} does not publish the approved social preview contract.`);
        }
    }
}

const Supportˉsource = await readFile(
    path.join(Repositoryˉroot, "Website/support/index.html"),
    "utf8",
);
for (const Required of [
    '<html lang="en" itemscope itemtype="https://schema.org/WebPage">',
    'itemprop="inLanguage" content="en-CA"',
    'property="og:locale" content="en_CA"',
    `property="og:image" content="${Supportˉpreviewˉurl}"`,
    `property="og:image:secure_url" content="${Supportˉpreviewˉurl}"`,
    'property="og:image:type" content="image/png"',
    `property="og:image" content="${Previewˉsquare.url}"`,
    `property="og:image:secure_url" content="${Previewˉsquare.url}"`,
    'name="twitter:card" content="summary_large_image"',
    `name="twitter:image" content="${Supportˉpreviewˉurl}"`,
    `rel="image_src" href="${Supportˉpreviewˉurl}"`,
    `itemprop="image primaryImageOfPage" href="${Supportˉpreviewˉurl}"`,
    `itemprop="thumbnailUrl" href="${Previewˉsquare.url}"`,
]) {
    if (!Supportˉsource.includes(Required)) {
        throw new Error("Website/support/index.html does not publish the approved social preview contract.");
    }
}

try {
    await access(path.join(Repositoryˉroot, "Website/og.png"));
    throw new Error("The superseded homepage social preview still exists.");
} catch (Failure) {
    if (Failure?.code !== "ENOENT") {
        throw Failure;
    }
}

console.log("Retired direct Wasm demo and social preview verification passed.");
