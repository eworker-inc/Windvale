import { execFile } from "node:child_process";
import { createHash } from "node:crypto";
import {
    copyFile,
    mkdir,
    readFile,
    rm,
    stat,
    writeFile,
} from "node:fs/promises";
import path from "node:path";
import process from "node:process";
import { promisify } from "node:util";
import { fileURLToPath } from "node:url";
import { Marked } from "marked";
import Sanitizeˉhtml from "sanitize-html";
import { createHighlighter } from "shiki";

const Executeˉfile = promisify(execFile);
const Scriptˉdirectory = path.dirname(fileURLToPath(import.meta.url));
const Repositoryˉroot = path.resolve(Scriptˉdirectory, "../..");
const Websiteˉroot = path.join(Repositoryˉroot, "Website");
const Repositoryˉurl = "https://github.com/eworker-inc/Windvale";
const MAX_HIGHLIGHTED_BYTES = 1_048_576;
const IMAGE_EXTENSIONS = new Set([
    ".gif",
    ".ico",
    ".jpeg",
    ".jpg",
    ".png",
    ".svg",
    ".webp",
]);
const TEXT_FILENAMES = new Set([
    ".editorconfig",
    ".gitattributes",
    ".gitignore",
    "LICENSE",
    "SHA256SUMS",
]);
const LANGUAGE_BY_EXTENSION = Object.freeze({
    ".asm": "asm",
    ".c": "c",
    ".cmd": "batch",
    ".cs": "csharp",
    ".csproj": "xml",
    ".css": "css",
    ".diff": "diff",
    ".h": "c",
    ".html": "html",
    ".ini": "ini",
    ".js": "javascript",
    ".json": "json",
    ".md": "markdown",
    ".mjs": "javascript",
    ".props": "xml",
    ".ps1": "powershell",
    ".s": "asm",
    ".sh": "shellscript",
    ".slnx": "xml",
    ".toml": "toml",
    ".txt": "plain",
    ".wv": "windvale",
    ".wva": "windvale-assembly",
    ".wvmap": "plain",
    ".wvproj": "xml",
    ".xml": "xml",
    ".yaml": "yaml",
    ".yml": "yaml",
});
const SHIKI_LANGUAGES = Object.freeze([
    "asm",
    "batch",
    "c",
    "csharp",
    "css",
    "diff",
    "html",
    "ini",
    "javascript",
    "json",
    "markdown",
    "powershell",
    "shellscript",
    "toml",
    "xml",
    "yaml",
]);

function Fail(Message) {
    throw new Error(Message);
}

function Normalizeˉrepositoryˉpath(Value) {
    return Value.replaceAll("\\", "/").replace(/^\.\//u, "");
}

function Isˉinside(Parent, Candidate) {
    const Relative = path.relative(Parent, Candidate);
    return Relative !== "" && Relative !== ".." && !Relative.startsWith(`..${path.sep}`);
}

function Escapeˉhtml(Value) {
    return Value
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#39;");
}

function Hashˉbytes(Bytes) {
    return createHash("sha256").update(Bytes).digest("hex");
}

function Jsonˉtext(Value) {
    return `${JSON.stringify(Value, null, 2)}\n`;
}

function Compareˉordinal(Left, Right) {
    return Left < Right ? -1 : Left > Right ? 1 : 0;
}

async function Runˉgit(Arguments) {
    const Result = await Executeˉfile("git", Arguments, {
        cwd: Repositoryˉroot,
        encoding: "utf8",
        maxBuffer: 64 * 1024 * 1024,
        windowsHide: true,
    });
    return Result.stdout.trim();
}

async function Listˉrepositoryˉpaths() {
    const Result = await Executeˉfile(
        "git",
        ["ls-files", "--cached", "--others", "--exclude-standard", "-z"],
        {
            cwd: Repositoryˉroot,
            encoding: "buffer",
            maxBuffer: 64 * 1024 * 1024,
            windowsHide: true,
        },
    );
    return Result.stdout
        .toString("utf8")
        .split("\0")
        .filter(Boolean)
        .map(Normalizeˉrepositoryˉpath)
        .filter((Value) => !Value.startsWith("Website/Generated/"))
        .sort(Compareˉordinal);
}

function Readˉarguments() {
    const Arguments = process.argv.slice(2);
    const Outputˉindex = Arguments.indexOf("--output");
    if (Outputˉindex < 0 || Outputˉindex + 1 >= Arguments.length || Arguments.length !== 2) {
        Fail("Usage: node Website/Scripts/Generate-Repository-Browser.mjs --output <Website-relative-directory>");
    }

    const Outputˉdirectory = path.resolve(Websiteˉroot, Arguments[Outputˉindex + 1]);
    if (!Isˉinside(Websiteˉroot, Outputˉdirectory)) {
        Fail("The repository-browser output must be a child of the Website directory.");
    }
    return Outputˉdirectory;
}

function Detectˉlanguage(Repositoryˉpath) {
    const Name = path.posix.basename(Repositoryˉpath);
    if (TEXT_FILENAMES.has(Name)) {
        return Name === "LICENSE" ? "plain" : "plain";
    }
    return LANGUAGE_BY_EXTENSION[path.posix.extname(Repositoryˉpath).toLowerCase()] ?? null;
}

function Isˉprobablyˉbinary(Bytes) {
    const Limit = Math.min(Bytes.byteLength, 8192);
    for (let Index = 0; Index < Limit; Index += 1) {
        if (Bytes[Index] === 0) {
            return true;
        }
    }
    return false;
}

function Decodeˉutf8(Bytes) {
    try {
        return new TextDecoder("utf-8", { fatal: true }).decode(Bytes);
    } catch {
        return null;
    }
}

function Plainˉcodeˉhtml(Source) {
    const Lines = Source.split("\n");
    return [
        '<pre class="shiki shiki-plain"><code>',
        ...Lines.map((Line) => `<span class="line">${Escapeˉhtml(Line)}</span>\n`),
        "</code></pre>\n",
    ].join("");
}

function Cleanˉmarkdownˉtitle(Value) {
    return Value
        .replace(/`([^`]*)`/gu, "$1")
        .replace(/\[([^\]]+)\]\([^)]*\)/gu, "$1")
        .replace(/[*_~]/gu, "")
        .trim();
}

function Documentˉtitle(Source, Repositoryˉpath) {
    const Match = /^#\s+(.+)$/mu.exec(Source);
    if (Match) {
        return Cleanˉmarkdownˉtitle(Match[1]);
    }
    const Name = path.posix.basename(Repositoryˉpath, ".md");
    return Name === "README" ? path.posix.basename(path.posix.dirname(Repositoryˉpath)) || "Windvale" : Name;
}

function Makeˉslugger() {
    const Counts = new Map();
    return (Value) => {
        const Base = Value
            .normalize("NFKD")
            .toLowerCase()
            .replace(/[^\p{Letter}\p{Number}\s-]/gu, "")
            .trim()
            .replace(/[\s-]+/gu, "-") || "section";
        const Count = Counts.get(Base) ?? 0;
        Counts.set(Base, Count + 1);
        return Count === 0 ? Base : `${Base}-${Count}`;
    };
}

function Splitˉtarget(Value) {
    const Hashˉindex = Value.indexOf("#");
    return Hashˉindex < 0
        ? { path: Value, fragment: "" }
        : { path: Value.slice(0, Hashˉindex), fragment: Value.slice(Hashˉindex) };
}

function Decodeˉurlˉpath(Value) {
    try {
        return decodeURIComponent(Value);
    } catch {
        return Value;
    }
}

function Resolveˉrepositoryˉtarget(Sourceˉpath, Value) {
    const Target = Splitˉtarget(Value);
    if (!Target.path) {
        return { path: Sourceˉpath, fragment: Target.fragment };
    }
    if (/^[a-z][a-z0-9+.-]*:/iu.test(Target.path) || Target.path.startsWith("//")) {
        return null;
    }

    const Decoded = Decodeˉurlˉpath(Target.path);
    const Candidate = Decoded.startsWith("/")
        ? path.posix.normalize(Decoded.slice(1))
        : path.posix.normalize(path.posix.join(path.posix.dirname(Sourceˉpath), Decoded));
    if (!Candidate || Candidate === ".." || Candidate.startsWith("../")) {
        return null;
    }
    return { path: Candidate, fragment: Target.fragment };
}

function Encodeˉqueryˉpath(Value) {
    return encodeURIComponent(Value).replaceAll("%2F", "%2F");
}

function Rewriteˉlink(Sourceˉpath, Value, Documentˉpaths, Fileˉpaths, Imageˉurls) {
    const Resolved = Resolveˉrepositoryˉtarget(Sourceˉpath, Value);
    if (!Resolved) {
        return Value;
    }

    let Targetˉpath = Resolved.path;
    if (Documentˉpaths.has(Targetˉpath)) {
        return `/docs/?path=${Encodeˉqueryˉpath(Targetˉpath)}${Resolved.fragment}`;
    }
    if (Documentˉpaths.has(`${Targetˉpath}/README.md`)) {
        Targetˉpath = `${Targetˉpath}/README.md`;
        return `/docs/?path=${Encodeˉqueryˉpath(Targetˉpath)}${Resolved.fragment}`;
    }
    if (Imageˉurls.has(Targetˉpath)) {
        return `${Imageˉurls.get(Targetˉpath)}${Resolved.fragment}`;
    }
    if (Fileˉpaths.has(Targetˉpath)) {
        return `/code/?path=${Encodeˉqueryˉpath(Targetˉpath)}${Resolved.fragment}`;
    }
    return Value;
}

function Rewriteˉimage(Sourceˉpath, Value, Imageˉurls) {
    const Resolved = Resolveˉrepositoryˉtarget(Sourceˉpath, Value);
    if (!Resolved || !Imageˉurls.has(Resolved.path)) {
        return null;
    }
    return Imageˉurls.get(Resolved.path);
}

function Renderˉmarkdown(Source, Repositoryˉpath, Documentˉpaths, Fileˉpaths, Imageˉurls) {
    const Slug = Makeˉslugger();
    const Renderer = {
        heading(Token) {
            const Text = this.parser.parseInline(Token.tokens);
            return `<h${Token.depth} id="${Escapeˉhtml(Slug(Token.text))}">${Text}</h${Token.depth}>\n`;
        },
    };
    const Parser = new Marked({
        async: false,
        gfm: true,
        renderer: Renderer,
    });
    const Rendered = Parser.parse(Source);
    return `${Sanitizeˉhtml(Rendered, {
        allowedTags: [
            "a", "abbr", "blockquote", "br", "code", "dd", "del", "details", "div", "dl", "dt",
            "em", "h1", "h2", "h3", "h4", "h5", "h6", "hr", "img", "input", "kbd", "li", "ol",
            "p", "pre", "s", "samp", "small", "span", "strong", "sub", "summary", "sup", "table",
            "tbody", "td", "tfoot", "th", "thead", "tr", "ul", "var",
        ],
        allowedAttributes: {
            a: ["href", "title", "rel"],
            code: ["class"],
            div: ["class"],
            h1: ["id"],
            h2: ["id"],
            h3: ["id"],
            h4: ["id"],
            h5: ["id"],
            h6: ["id"],
            img: ["alt", "decoding", "height", "loading", "src", "title", "width"],
            input: ["checked", "disabled", "type"],
            ol: ["start"],
            span: ["class"],
            td: ["align"],
            th: ["align"],
        },
        allowedSchemes: ["http", "https", "mailto"],
        allowProtocolRelative: false,
        enforceHtmlBoundary: true,
        transformTags: {
            a(Tagˉname, Attributes) {
                const Href = Attributes.href;
                if (!Href) {
                    return { tagName: Tagˉname, attribs: Attributes };
                }
                const Rewritten = Rewriteˉlink(
                    Repositoryˉpath,
                    Href,
                    Documentˉpaths,
                    Fileˉpaths,
                    Imageˉurls,
                );
                const External = /^https?:/iu.test(Rewritten);
                return {
                    tagName: Tagˉname,
                    attribs: {
                        ...Attributes,
                        href: Rewritten,
                        ...(External ? { rel: "noreferrer" } : {}),
                    },
                };
            },
            img(Tagˉname, Attributes) {
                const Sourceˉurl = Attributes.src
                    ? Rewriteˉimage(Repositoryˉpath, Attributes.src, Imageˉurls)
                    : null;
                return {
                    tagName: Tagˉname,
                    attribs: {
                        ...Attributes,
                        ...(Sourceˉurl ? { src: Sourceˉurl } : {}),
                        decoding: "async",
                        loading: "lazy",
                    },
                };
            },
        },
        exclusiveFilter(Frame) {
            return Frame.tag === "img" && !Frame.attribs.src;
        },
    })}\n`;
}

async function Readˉavailableˉfile(Repositoryˉpath) {
    const Absoluteˉpath = path.join(Repositoryˉroot, ...Repositoryˉpath.split("/"));
    try {
        const Information = await stat(Absoluteˉpath);
        if (!Information.isFile()) {
            return null;
        }
        return await readFile(Absoluteˉpath);
    } catch (Error) {
        if (Error && typeof Error === "object" && Error.code === "ENOENT") {
            return null;
        }
        throw Error;
    }
}

async function Main() {
    const Outputˉdirectory = Readˉarguments();
    const [Commit, Tree, Commitˉdate, Repositoryˉpaths, Windvaleˉgrammarˉsource] = await Promise.all([
        process.env.GITHUB_SHA || Runˉgit(["rev-parse", "HEAD"]),
        Runˉgit(["rev-parse", "HEAD^{tree}"]),
        Runˉgit(["show", "-s", "--format=%cI", "HEAD"]),
        Listˉrepositoryˉpaths(),
        readFile(path.join(
            Repositoryˉroot,
            "Tools/Editors/Windvale/syntaxes/Windvale.tmLanguage.json"), "utf8"),
    ]);

    await rm(Outputˉdirectory, { recursive: true, force: true });
    const Repositoryˉdirectory = path.join(Outputˉdirectory, "repository");
    const Codeˉdirectory = path.join(Repositoryˉdirectory, "code");
    const Documentˉdirectory = path.join(Repositoryˉdirectory, "docs");
    const Assetˉdirectory = path.join(Repositoryˉdirectory, "assets");
    await Promise.all([
        mkdir(Codeˉdirectory, { recursive: true }),
        mkdir(Documentˉdirectory, { recursive: true }),
        mkdir(Assetˉdirectory, { recursive: true }),
    ]);

    const Fileˉpaths = new Set(Repositoryˉpaths);
    const Documentˉpaths = new Set(Repositoryˉpaths.filter((Value) => Value.endsWith(".md")));
    const Imageˉurls = new Map();
    const Fileˉbytes = new Map();
    for (const Repositoryˉpath of Repositoryˉpaths) {
        const Bytes = await Readˉavailableˉfile(Repositoryˉpath);
        if (Bytes) {
            Fileˉbytes.set(Repositoryˉpath, Bytes);
        }
        if (!Bytes || !IMAGE_EXTENSIONS.has(path.posix.extname(Repositoryˉpath).toLowerCase())) {
            continue;
        }
        const Extension = path.posix.extname(Repositoryˉpath).toLowerCase();
        const Hash = Hashˉbytes(Bytes);
        const Publishedˉname = `${Hash}${Extension}`;
        if (!Imageˉurls.has(Repositoryˉpath)) {
            await copyFile(
                path.join(Repositoryˉroot, ...Repositoryˉpath.split("/")),
                path.join(Assetˉdirectory, Publishedˉname),
            );
            Imageˉurls.set(Repositoryˉpath, `/repository/assets/${Publishedˉname}`);
        }
    }

    const Windvaleˉgrammar = JSON.parse(Windvaleˉgrammarˉsource);
    Windvaleˉgrammar.name = "windvale";
    Windvaleˉgrammar.aliases = ["wv"];
    const Highlighter = await createHighlighter({
        themes: ["github-light", "github-dark"],
        langs: [...SHIKI_LANGUAGES, Windvaleˉgrammar],
    });

    const Files = [];
    const Codeˉfragmentˉpaths = new Map();
    for (const Repositoryˉpath of Repositoryˉpaths) {
        const Bytes = Fileˉbytes.get(Repositoryˉpath) ?? null;
        const Language = Detectˉlanguage(Repositoryˉpath);
        const Binary = !Bytes || Isˉprobablyˉbinary(Bytes);
        const Source = !Binary && Bytes ? Decodeˉutf8(Bytes) : null;
        let Publishedˉurl = null;
        let Reason = null;

        if (!Bytes) {
            Reason = "not-present-in-checkout";
        } else if (Binary || Source === null) {
            Reason = "binary";
        } else if (!Language) {
            Reason = "unsupported-text";
        } else if (Bytes.byteLength > MAX_HIGHLIGHTED_BYTES) {
            Reason = "too-large";
        } else {
            const Fragment = Language === "plain" || Language === "windvale-assembly"
                ? Plainˉcodeˉhtml(Source)
                : Highlighter.codeToHtml(Source, {
                    lang: Language,
                    themes: {
                        light: "github-light",
                        dark: "github-dark",
                    },
                    defaultColor: false,
                });
            const Fragmentˉhash = Hashˉbytes(Buffer.from(Fragment, "utf8"));
            const Publishedˉname = `${Fragmentˉhash}.html`;
            if (!Codeˉfragmentˉpaths.has(Fragmentˉhash)) {
                await writeFile(path.join(Codeˉdirectory, Publishedˉname), Fragment, "utf8");
                Codeˉfragmentˉpaths.set(Fragmentˉhash, Publishedˉname);
            }
            Publishedˉurl = `/repository/code/${Publishedˉname}`;
        }

        Files.push({
            path: Repositoryˉpath,
            size: Bytes?.byteLength ?? null,
            contentHash: Bytes ? Hashˉbytes(Bytes) : null,
            language: Language,
            kind: Binary ? "binary" : "text",
            publishedUrl: Publishedˉurl,
            unavailableReason: Reason,
        });
    }

    const Documents = [];
    const Documentˉfragmentˉpaths = new Map();
    for (const Repositoryˉpath of [...Documentˉpaths].sort(Compareˉordinal)) {
        const Bytes = Fileˉbytes.get(Repositoryˉpath);
        const Source = Bytes ? Decodeˉutf8(Bytes) : null;
        if (Source === null) {
            continue;
        }
        const Fragment = Renderˉmarkdown(
            Source,
            Repositoryˉpath,
            Documentˉpaths,
            Fileˉpaths,
            Imageˉurls,
        );
        const Fragmentˉhash = Hashˉbytes(Buffer.from(Fragment, "utf8"));
        const Publishedˉname = `${Fragmentˉhash}.html`;
        if (!Documentˉfragmentˉpaths.has(Fragmentˉhash)) {
            await writeFile(path.join(Documentˉdirectory, Publishedˉname), Fragment, "utf8");
            Documentˉfragmentˉpaths.set(Fragmentˉhash, Publishedˉname);
        }
        Documents.push({
            path: Repositoryˉpath,
            title: Documentˉtitle(Source, Repositoryˉpath),
            size: Bytes.byteLength,
            contentHash: Hashˉbytes(Bytes),
            publishedUrl: `/repository/docs/${Publishedˉname}`,
        });
    }

    Highlighter.dispose();
    const Manifest = {
        version: 1,
        repository: Repositoryˉurl,
        branch: "main",
        commit: Commit,
        tree: Tree,
        commitDate: Commitˉdate,
        defaultDocument: Documentˉpaths.has("README.md") ? "README.md" : Documents[0]?.path ?? null,
        defaultCode: Fileˉpaths.has("Examples/Seed/Hello-Windvale.wv")
            ? "Examples/Seed/Hello-Windvale.wv"
            : Files.find((File) => File.publishedUrl && File.path.endsWith(".wv"))?.path ?? null,
        documents: Documents,
        files: Files,
    };
    await Promise.all([
        writeFile(path.join(Repositoryˉdirectory, "manifest.json"), Jsonˉtext(Manifest), "utf8"),
        writeFile(path.join(Outputˉdirectory, "deployment.json"), Jsonˉtext({
            version: 1,
            commit: Commit,
            tree: Tree,
            commitDate: Commitˉdate,
        }), "utf8"),
    ]);

    process.stdout.write(
        `Generated repository browser for ${Commit.slice(0, 12)}: `
        + `${Files.length} files, ${Documents.length} documents, `
        + `${Codeˉfragmentˉpaths.size} code fragments, ${Imageˉurls.size} image assets.\n`,
    );
}

Main().catch((Error) => {
    process.stderr.write(`${Error instanceof Error ? Error.stack ?? Error.message : String(Error)}\n`);
    process.exitCode = 1;
});
