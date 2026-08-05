import {
    Encodeˉgithubˉpath,
    Formatˉbytes,
    Initializeˉbrowserˉpanel,
    Initializeˉrepositoryˉresizer,
    Loadˉrepositoryˉmanifest,
    Makeˉelement,
    Renderˉrepositoryˉtree,
    Replaceˉselectedˉpath,
    Selectedˉpathˉfromˉurl,
    Setˉbrowserˉpanelˉopen,
    Setˉcommitˉlinks,
} from "../repository-browser.js";

let Manifest;
let Documents = [];
let Selectedˉpath = "";
let Searchˉquery = "";

function Setˉstatus(Message, Failure = false) {
    const Content = document.querySelector("#document-content");
    if (!Content) {
        return;
    }
    const Status = Makeˉelement("p", Failure ? "repository-load-state failure" : "repository-load-state", Message);
    Status.setAttribute("role", Failure ? "alert" : "status");
    Content.replaceChildren(Status);
}

function Updateˉtree() {
    const Container = document.querySelector("#repository-tree");
    if (Container) {
        Renderˉrepositoryˉtree(Container, Documents, Selectedˉpath, (Document) => {
            void Selectˉdocument(Document.path, true);
        }, Searchˉquery);
    }
}

function Updateˉdocumentˉlinks(Document) {
    const Sourceˉlink = document.querySelector("#document-source-link");
    const Githubˉlink = document.querySelector("#document-github-link");
    if (Sourceˉlink) {
        Sourceˉlink.href = `/code/?path=${encodeURIComponent(Document.path)}`;
    }
    if (Githubˉlink) {
        Githubˉlink.href = `${Manifest.repository}/blob/${encodeURIComponent(Manifest.commit)}/${Encodeˉgithubˉpath(Document.path)}`;
    }
}

async function Selectˉdocument(Path, Push) {
    const Document = Documents.find((Item) => Item.path === Path)
        ?? Documents.find((Item) => Item.path === Manifest.defaultDocument)
        ?? Documents[0];
    if (!Document) {
        Setˉstatus("No published Markdown documents are available.", true);
        return;
    }

    Selectedˉpath = Document.path;
    Replaceˉselectedˉpath(Document.path, Push);
    Updateˉtree();
    Setˉbrowserˉpanelˉopen(false);
    document.querySelector("#document-path").textContent = Document.path;
    document.querySelector("#document-meta").textContent = `${Formatˉbytes(Document.size)} · rendered from Markdown`;
    Updateˉdocumentˉlinks(Document);
    Setˉstatus("Loading document…");

    try {
        const Response = await fetch(Document.publishedUrl, { cache: "force-cache" });
        if (!Response.ok) {
            throw new Error(`Document returned ${Response.status}.`);
        }
        const Content = document.querySelector("#document-content");
        Content.innerHTML = await Response.text();
        document.title = `${Document.title} — Windvale documents`;
        const Fragment = window.location.hash.slice(1);
        if (Fragment) {
            requestAnimationFrame(() => document.getElementById(Fragment)?.scrollIntoView());
        } else {
            document.querySelector("#document-viewer")?.scrollTo({ top: 0 });
        }
    } catch {
        Setˉstatus("This document could not be loaded. Please try again later.", true);
    }
}

function Interceptˉdocumentˉlink(Event) {
    const Anchor = Event.target.closest("a");
    if (!Anchor) {
        return;
    }
    const Url = new URL(Anchor.href, window.location.href);
    if (Url.origin !== window.location.origin || Url.pathname !== "/docs/") {
        return;
    }
    const Path = Url.searchParams.get("path");
    if (!Path || !Documents.some((Document) => Document.path === Path)) {
        return;
    }
    Event.preventDefault();
    void Selectˉdocument(Path, true).then(() => {
        if (Url.hash) {
            window.history.replaceState({}, "", `${window.location.pathname}${window.location.search}${Url.hash}`);
            document.getElementById(Url.hash.slice(1))?.scrollIntoView();
        }
    });
}

async function Initialize() {
    Initializeˉbrowserˉpanel();
    Initializeˉrepositoryˉresizer();
    document.querySelector("#document-content")?.addEventListener("click", Interceptˉdocumentˉlink);
    document.querySelector("#repository-search")?.addEventListener("input", (Event) => {
        Searchˉquery = Event.target.value;
        Updateˉtree();
    });

    try {
        Manifest = await Loadˉrepositoryˉmanifest();
        Documents = Manifest.documents;
        Setˉcommitˉlinks(Manifest);
        await Selectˉdocument(Selectedˉpathˉfromˉurl(Manifest.defaultDocument), false);
        window.addEventListener("popstate", () => {
            void Selectˉdocument(Selectedˉpathˉfromˉurl(Manifest.defaultDocument), false);
        });
    } catch {
        Setˉstatus("The repository snapshot is temporarily unavailable.", true);
    }
}

void Initialize();
