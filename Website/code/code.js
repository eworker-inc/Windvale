import {
    Encodeˉgithubˉpath,
    Formatˉbytes,
    Initializeˉbrowserˉpanel,
    Loadˉrepositoryˉmanifest,
    Makeˉelement,
    Renderˉrepositoryˉtree,
    Replaceˉselectedˉpath,
    Selectedˉpathˉfromˉurl,
    Setˉbrowserˉpanelˉopen,
    Setˉcommitˉlinks,
} from "../repository-browser.js";

let Manifest;
let Files = [];
let Selectedˉpath = "";
let Searchˉquery = "";

function Setˉstatus(Message, Failure = false) {
    const Content = document.querySelector("#code-content");
    if (!Content) {
        return;
    }
    const Status = Makeˉelement("div", Failure ? "repository-load-state failure" : "repository-load-state");
    Status.setAttribute("role", Failure ? "alert" : "status");
    Status.append(
        Makeˉelement("span", "material-symbol", Failure ? "error" : "hourglass_top"),
        Makeˉelement("p", "", Message),
    );
    Content.replaceChildren(Status);
}

function Updateˉtree() {
    const Container = document.querySelector("#repository-tree");
    if (Container) {
        Renderˉrepositoryˉtree(Container, Files, Selectedˉpath, (File) => {
            void Selectˉfile(File.path, true);
        }, Searchˉquery);
    }
}

function Addˉlineˉanchors() {
    document.querySelectorAll("#code-content .line").forEach((Line, Index) => {
        const Lineˉnumber = Index + 1;
        Line.id = `L${Lineˉnumber}`;
        Line.dataset.line = String(Lineˉnumber);
    });
    const Fragment = window.location.hash.slice(1);
    if (/^L\d+$/u.test(Fragment)) {
        requestAnimationFrame(() => document.getElementById(Fragment)?.scrollIntoView({ block: "center" }));
    }
}

function Fileˉunavailableˉmessage(File) {
    switch (File.unavailableReason) {
        case "binary":
            return "This is a binary file. Open it on GitHub to inspect or download it.";
        case "too-large":
            return "This text file is intentionally not expanded in the browser because of its size.";
        case "not-present-in-checkout":
            return "This tracked artifact is not present in the publication checkout.";
        default:
            return "This file type is listed in the tree but is not rendered by the source browser.";
    }
}

async function Selectˉfile(Path, Push) {
    const File = Files.find((Item) => Item.path === Path)
        ?? Files.find((Item) => Item.path === Manifest.defaultCode)
        ?? Files.find((Item) => Item.publishedUrl)
        ?? Files[0];
    if (!File) {
        Setˉstatus("No repository files are available.", true);
        return;
    }

    Selectedˉpath = File.path;
    Replaceˉselectedˉpath(File.path, Push);
    Updateˉtree();
    Setˉbrowserˉpanelˉopen(false);
    document.querySelector("#code-path").textContent = File.path;
    document.querySelector("#code-meta").textContent = [
        File.language || File.kind,
        Formatˉbytes(File.size),
    ].join(" · ");
    const Githubˉlink = document.querySelector("#code-github-link");
    Githubˉlink.href = `${Manifest.repository}/blob/${encodeURIComponent(Manifest.commit)}/${Encodeˉgithubˉpath(File.path)}`;
    const Documentˉlink = document.querySelector("#code-document-link");
    const Hasˉdocument = Manifest.documents.some((Document) => Document.path === File.path);
    Documentˉlink.hidden = !Hasˉdocument;
    Documentˉlink.href = `/docs/?path=${encodeURIComponent(File.path)}`;
    document.querySelector("#copy-code").disabled = !File.publishedUrl;
    document.title = `${File.path.split("/").at(-1)} — Windvale source`;

    if (!File.publishedUrl) {
        Setˉstatus(Fileˉunavailableˉmessage(File));
        return;
    }

    Setˉstatus("Loading source…");
    try {
        const Response = await fetch(File.publishedUrl, { cache: "force-cache" });
        if (!Response.ok) {
            throw new Error(`Source fragment returned ${Response.status}.`);
        }
        document.querySelector("#code-content").innerHTML = await Response.text();
        Addˉlineˉanchors();
        if (!window.location.hash) {
            document.querySelector("#code-viewer")?.scrollTo({ top: 0, left: 0 });
        }
    } catch {
        Setˉstatus("This source file could not be loaded. Please try again later.", true);
    }
}

async function Copyˉcode() {
    const Code = document.querySelector("#code-content code")?.textContent;
    if (!Code) {
        return;
    }
    const Button = document.querySelector("#copy-code");
    try {
        await navigator.clipboard.writeText(Code);
        Button.textContent = "Copied";
    } catch {
        Button.textContent = "Copy unavailable";
    }
    window.setTimeout(() => {
        Button.textContent = "Copy";
    }, 1600);
}

async function Initialize() {
    Initializeˉbrowserˉpanel();
    document.querySelector("#repository-search")?.addEventListener("input", (Event) => {
        Searchˉquery = Event.target.value;
        Updateˉtree();
    });
    document.querySelector("#copy-code")?.addEventListener("click", () => {
        void Copyˉcode();
    });

    try {
        Manifest = await Loadˉrepositoryˉmanifest();
        Files = Manifest.files;
        Setˉcommitˉlinks(Manifest);
        await Selectˉfile(Selectedˉpathˉfromˉurl(Manifest.defaultCode), false);
        window.addEventListener("popstate", () => {
            void Selectˉfile(Selectedˉpathˉfromˉurl(Manifest.defaultCode), false);
        });
    } catch {
        Setˉstatus("The repository snapshot is temporarily unavailable.", true);
    }
}

void Initialize();
