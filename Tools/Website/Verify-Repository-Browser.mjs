import assert from "node:assert/strict";
import { readdir, readFile, stat } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

const Scriptˉdirectory = path.dirname(fileURLToPath(import.meta.url));
const Repositoryˉroot = path.resolve(Scriptˉdirectory, "../..");
const Publicationˉroot = path.join(Repositoryˉroot, "Website/dist");
const Manifestˉpath = path.join(Publicationˉroot, "repository/manifest.json");
const MAX_PAGES_FILES = 20_000;
const MAX_PAGES_FILE_BYTES = 25 * 1024 * 1024;

async function Readˉpublication(Url) {
    assert.match(Url, /^\/repository\/(?:assets|code|docs)\/[a-f0-9]{64}(?:\.[a-z0-9]+)?$/u);
    return readFile(path.join(Publicationˉroot, ...Url.slice(1).split("/")), "utf8");
}

async function Listˉfiles(Directory) {
    const Result = [];
    for (const Entry of await readdir(Directory, { withFileTypes: true })) {
        const Entryˉpath = path.join(Directory, Entry.name);
        if (Entry.isDirectory()) {
            Result.push(...await Listˉfiles(Entryˉpath));
        } else if (Entry.isFile()) {
            Result.push(Entryˉpath);
        }
    }
    return Result;
}

const Manifest = JSON.parse(await readFile(Manifestˉpath, "utf8"));
const Deployment = JSON.parse(await readFile(path.join(Publicationˉroot, "deployment.json"), "utf8"));
assert.equal(Manifest.version, 1);
assert.match(Manifest.commit, /^[a-f0-9]{40,64}$/u);
assert.match(Manifest.tree, /^[a-f0-9]{40,64}$/u);
assert.equal(Deployment.commit, Manifest.commit);
assert.equal(Deployment.tree, Manifest.tree);
assert.equal(Manifest.defaultDocument, "README.md");
assert.equal(Manifest.defaultCode, "Examples/Seed/Hello-Windvale.wv");
assert.ok(Manifest.files.length > 500);
assert.ok(Manifest.documents.length > 100);

const Fileˉpaths = new Set(Manifest.files.map((File) => File.path));
const Documentˉpaths = new Set(Manifest.documents.map((Document) => Document.path));
assert.equal(Fileˉpaths.size, Manifest.files.length);
assert.equal(Documentˉpaths.size, Manifest.documents.length);
assert.ok(Fileˉpaths.has("Compiler/Windvale/Source-Lexer-Core.wv"));
assert.ok(Documentˉpaths.has("Documents/Project/Progress.md"));

const Defaultˉcode = Manifest.files.find((File) => File.path === Manifest.defaultCode);
assert.equal(Defaultˉcode.language, "windvale");
assert.ok(Defaultˉcode.publishedUrl);
const Highlightedˉwindvale = await Readˉpublication(Defaultˉcode.publishedUrl);
assert.match(Highlightedˉwindvale, /class="shiki shiki-themes github-light github-dark"/u);
assert.match(Highlightedˉwindvale, />module</u);
assert.doesNotMatch(Highlightedˉwindvale, /<script|javascript:/iu);

const Windvaleˉassembly = Manifest.files.find(
    (File) => File.path === "Examples/Assembler/Hello-Object.wva",
);
assert.equal(Windvaleˉassembly.language, "windvale-assembly");
assert.ok(Windvaleˉassembly.publishedUrl);
const Highlightedˉwindvaleˉassembly = await Readˉpublication(Windvaleˉassembly.publishedUrl);
assert.match(Highlightedˉwindvaleˉassembly, /class="shiki shiki-themes github-light github-dark"/u);
assert.match(Highlightedˉwindvaleˉassembly, />windvale-assembly</u);
assert.match(Highlightedˉwindvaleˉassembly, />move_i32</u);
assert.doesNotMatch(Highlightedˉwindvaleˉassembly, /shiki-plain|<script|javascript:/iu);

const Readme = Manifest.documents.find((Document) => Document.path === "README.md");
const Renderedˉreadme = await Readˉpublication(Readme.publishedUrl);
assert.match(Renderedˉreadme, /^<h1 id="windvale">Windvale<\/h1>/u);
assert.match(Renderedˉreadme, /src="\/repository\/assets\/[a-f0-9]{64}\.png"/u);
assert.match(Renderedˉreadme, /href="\/docs\/\?path=Documents%2FProject%2FProgress\.md"/u);

for (const Document of Manifest.documents) {
    assert.match(Document.contentHash, /^[a-f0-9]{64}$/u);
    const Fragment = await Readˉpublication(Document.publishedUrl);
    assert.doesNotMatch(Fragment, /<(?:script|iframe|object|embed|form)\b/iu);
    assert.doesNotMatch(Fragment, /\son[a-z]+\s*=|javascript:/iu);
}

const Publicationˉfiles = await Listˉfiles(Publicationˉroot);
assert.ok(Publicationˉfiles.length < MAX_PAGES_FILES);
for (const Fileˉpath of Publicationˉfiles) {
    const Information = await stat(Fileˉpath);
    assert.ok(
        Information.size <= MAX_PAGES_FILE_BYTES,
        `${path.relative(Publicationˉroot, Fileˉpath)} exceeds the Cloudflare Pages asset limit.`,
    );
}

for (const Relativeˉpath of [
    "_headers",
    "404.html",
    "code/index.html",
    "docs/index.html",
    "robots.txt",
    "sitemap.xml",
]) {
    assert.ok((await stat(path.join(Publicationˉroot, ...Relativeˉpath.split("/")))).isFile());
}

for (const Relativeˉpath of ["code/index.html", "docs/index.html"]) {
    const Browserˉpage = await readFile(
        path.join(Publicationˉroot, ...Relativeˉpath.split("/")),
        "utf8",
    );
    assert.match(Browserˉpage, /role="separator"/u);
    assert.match(Browserˉpage, /class="repository-snapshot-note"/u);
    assert.match(
        Browserˉpage,
        /https:\/\/www\.googletagmanager\.com\/gtag\/js\?id=G-3PB4LZFMRE/u,
    );
}

process.stdout.write(
    `Windvale repository browser checks passed: ${Manifest.files.length} files, `
    + `${Manifest.documents.length} rendered documents, ${Publicationˉfiles.length} published assets.\n`,
);
