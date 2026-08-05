export function Makeˉelement(Tagˉname, Classˉname, Text) {
    const Element = document.createElement(Tagˉname);
    if (Classˉname) {
        Element.className = Classˉname;
    }
    if (Text !== undefined) {
        Element.textContent = Text;
    }
    return Element;
}

export function Formatˉbytes(Value) {
    if (!Number.isFinite(Value) || Value < 0) {
        return "Size unavailable";
    }
    if (Value < 1024) {
        return `${Value} B`;
    }
    if (Value < 1024 * 1024) {
        return `${(Value / 1024).toFixed(Value < 10 * 1024 ? 1 : 0)} KiB`;
    }
    return `${(Value / (1024 * 1024)).toFixed(1)} MiB`;
}

export function Encodeˉgithubˉpath(Value) {
    return Value.split("/").map(encodeURIComponent).join("/");
}

export async function Loadˉrepositoryˉmanifest() {
    const Response = await fetch("/repository/manifest.json", {
        cache: "no-cache",
        headers: { Accept: "application/json" },
    });
    if (!Response.ok) {
        throw new Error(`Repository manifest returned ${Response.status}.`);
    }
    const Manifest = await Response.json();
    if (
        Manifest?.version !== 1
        || typeof Manifest.commit !== "string"
        || !Array.isArray(Manifest.files)
        || !Array.isArray(Manifest.documents)
    ) {
        throw new Error("Repository manifest has an unsupported format.");
    }
    return Manifest;
}

function Makeˉdirectoryˉnode(Name = "") {
    return {
        name: Name,
        directories: new Map(),
        files: [],
    };
}

function Makeˉrepositoryˉicon(Kind) {
    const Icon = Makeˉelement("span", `repository-tree-icon ${Kind}`);
    Icon.setAttribute("aria-hidden", "true");
    return Icon;
}

function Buildˉdirectoryˉmodel(Items) {
    const Root = Makeˉdirectoryˉnode();
    for (const Item of Items) {
        const Parts = Item.path.split("/");
        let Directory = Root;
        for (const Part of Parts.slice(0, -1)) {
            if (!Directory.directories.has(Part)) {
                Directory.directories.set(Part, Makeˉdirectoryˉnode(Part));
            }
            Directory = Directory.directories.get(Part);
        }
        Directory.files.push(Item);
    }
    return Root;
}

function Containsˉselectedˉpath(Prefix, Selectedˉpath) {
    return Selectedˉpath === Prefix || Selectedˉpath.startsWith(`${Prefix}/`);
}

function Renderˉdirectory(Directory, Prefix, Selectedˉpath, Selectˉitem) {
    const List = Makeˉelement("ul", "repository-tree-list");
    const Directories = [...Directory.directories.values()]
        .sort((Left, Right) => Left.name.localeCompare(Right.name));
    const Files = [...Directory.files]
        .sort((Left, Right) => Left.path.localeCompare(Right.path));

    for (const Child of Directories) {
        const Childˉprefix = Prefix ? `${Prefix}/${Child.name}` : Child.name;
        const Item = Makeˉelement("li", "repository-directory");
        const Details = document.createElement("details");
        Details.open = Containsˉselectedˉpath(Childˉprefix, Selectedˉpath);
        const Summary = Makeˉelement("summary", "repository-directory-name");
        Summary.append(
            Makeˉrepositoryˉicon("folder"),
            Makeˉelement("span", "", Child.name),
        );
        Details.append(
            Summary,
            Renderˉdirectory(Child, Childˉprefix, Selectedˉpath, Selectˉitem),
        );
        Item.append(Details);
        List.append(Item);
    }

    for (const File of Files) {
        const Item = Makeˉelement("li", "repository-file");
        const Button = Makeˉelement("button", File.path === Selectedˉpath ? "selected" : "");
        Button.type = "button";
        Button.title = File.path;
        Button.append(
            Makeˉrepositoryˉicon(File.path.endsWith(".md") ? "document" : "file"),
            Makeˉelement("span", "repository-file-name", File.path.split("/").at(-1)),
        );
        Button.addEventListener("click", () => Selectˉitem(File));
        Item.append(Button);
        List.append(Item);
    }
    return List;
}

export function Renderˉrepositoryˉtree(Container, Items, Selectedˉpath, Selectˉitem, Query = "") {
    const Normalizedˉquery = Query.trim().toLocaleLowerCase();
    const Filtered = Normalizedˉquery
        ? Items.filter((Item) => Item.path.toLocaleLowerCase().includes(Normalizedˉquery))
        : Items;
    if (Filtered.length === 0) {
        Container.replaceChildren(Makeˉelement("p", "repository-empty-tree", "No matching files."));
        return;
    }
    Container.replaceChildren(Renderˉdirectory(
        Buildˉdirectoryˉmodel(Filtered),
        "",
        Selectedˉpath,
        Selectˉitem,
    ));
    if (Normalizedˉquery) {
        Container.querySelectorAll("details").forEach((Details) => {
            Details.open = true;
        });
    }
}

export function Setˉbrowserˉpanelˉopen(Open) {
    const Panel = document.querySelector("#repository-panel");
    const Toggle = document.querySelector("#repository-panel-toggle");
    Panel?.classList.toggle("open", Open);
    Toggle?.setAttribute("aria-expanded", String(Open));
}

export function Initializeˉbrowserˉpanel() {
    document.querySelector("#repository-panel-toggle")?.addEventListener("click", () => {
        Setˉbrowserˉpanelˉopen(!document.querySelector("#repository-panel")?.classList.contains("open"));
    });
    document.querySelector("#repository-panel-close")?.addEventListener("click", () => {
        Setˉbrowserˉpanelˉopen(false);
    });
}

const REPOSITORY_PANEL_DEFAULT_WIDTH = 320;
const REPOSITORY_PANEL_MINIMUM_WIDTH = 240;
const REPOSITORY_VIEWER_MINIMUM_WIDTH = 420;
const REPOSITORY_PANEL_MAXIMUM_WIDTH = 640;
const REPOSITORY_PANEL_STORAGE_KEY = "windvale.repository-panel-width";

function Repositoryˉpanelˉbounds(Main) {
    return {
        minimum: REPOSITORY_PANEL_MINIMUM_WIDTH,
        maximum: Math.max(
            REPOSITORY_PANEL_MINIMUM_WIDTH,
            Math.min(
                REPOSITORY_PANEL_MAXIMUM_WIDTH,
                Main.getBoundingClientRect().width - REPOSITORY_VIEWER_MINIMUM_WIDTH,
            ),
        ),
    };
}

function Readˉrepositoryˉpanelˉwidth() {
    try {
        const Stored = Number.parseFloat(localStorage.getItem(REPOSITORY_PANEL_STORAGE_KEY));
        return Number.isFinite(Stored) ? Stored : REPOSITORY_PANEL_DEFAULT_WIDTH;
    } catch {
        return REPOSITORY_PANEL_DEFAULT_WIDTH;
    }
}

function Storeˉrepositoryˉpanelˉwidth(Width) {
    try {
        localStorage.setItem(REPOSITORY_PANEL_STORAGE_KEY, String(Math.round(Width)));
    } catch {
        // The divider remains usable when browser storage is unavailable.
    }
}

export function Initializeˉrepositoryˉresizer() {
    const Main = document.querySelector(".repository-main");
    const Resizer = document.querySelector("#repository-resizer");
    if (!Main || !Resizer) {
        return;
    }

    let Resizing = false;
    const Applyˉwidth = (Requestedˉwidth, Persist = false) => {
        const Bounds = Repositoryˉpanelˉbounds(Main);
        const Width = Math.min(Bounds.maximum, Math.max(Bounds.minimum, Requestedˉwidth));
        Main.style.setProperty("--repository-panel-width", `${Math.round(Width)}px`);
        Resizer.setAttribute("aria-valuemin", String(Bounds.minimum));
        Resizer.setAttribute("aria-valuemax", String(Math.round(Bounds.maximum)));
        Resizer.setAttribute("aria-valuenow", String(Math.round(Width)));
        if (Persist) {
            Storeˉrepositoryˉpanelˉwidth(Width);
        }
        return Width;
    };
    const Currentˉwidth = () => Number.parseFloat(
        getComputedStyle(Main).getPropertyValue("--repository-panel-width"),
    ) || REPOSITORY_PANEL_DEFAULT_WIDTH;
    const Finishˉresize = () => {
        if (!Resizing) {
            return;
        }
        Resizing = false;
        document.body.classList.remove("repository-resizing");
        Storeˉrepositoryˉpanelˉwidth(Currentˉwidth());
    };

    Applyˉwidth(Readˉrepositoryˉpanelˉwidth());
    Resizer.addEventListener("pointerdown", (Event) => {
        if (Event.button !== 0) {
            return;
        }
        Event.preventDefault();
        Resizing = true;
        document.body.classList.add("repository-resizing");
        Resizer.setPointerCapture(Event.pointerId);
    });
    Resizer.addEventListener("mousedown", (Event) => {
        if (Event.button !== 0) {
            return;
        }
        Event.preventDefault();
        Resizing = true;
        document.body.classList.add("repository-resizing");
    });
    Resizer.addEventListener("pointermove", (Event) => {
        if (!Resizing) {
            return;
        }
        const Mainˉleft = Main.getBoundingClientRect().left;
        Applyˉwidth(Event.clientX - Mainˉleft);
    });
    Resizer.addEventListener("pointerup", (Event) => {
        if (Resizer.hasPointerCapture(Event.pointerId)) {
            Resizer.releasePointerCapture(Event.pointerId);
        }
        Finishˉresize();
    });
    Resizer.addEventListener("pointercancel", Finishˉresize);
    Resizer.addEventListener("lostpointercapture", Finishˉresize);
    window.addEventListener("mousemove", (Event) => {
        if (!Resizing) {
            return;
        }
        const Mainˉleft = Main.getBoundingClientRect().left;
        Applyˉwidth(Event.clientX - Mainˉleft);
    });
    window.addEventListener("mouseup", Finishˉresize);
    Resizer.addEventListener("dblclick", () => {
        Applyˉwidth(REPOSITORY_PANEL_DEFAULT_WIDTH, true);
    });
    Resizer.addEventListener("keydown", (Event) => {
        const Step = Event.shiftKey ? 48 : 16;
        let Width = Currentˉwidth();
        switch (Event.key) {
            case "ArrowLeft":
                Width -= Step;
                break;
            case "ArrowRight":
                Width += Step;
                break;
            case "Home":
                Width = Repositoryˉpanelˉbounds(Main).minimum;
                break;
            case "End":
                Width = Repositoryˉpanelˉbounds(Main).maximum;
                break;
            default:
                return;
        }
        Event.preventDefault();
        Applyˉwidth(Width, true);
    });
    window.addEventListener("resize", () => {
        Applyˉwidth(Currentˉwidth());
    });
}

export function Selectedˉpathˉfromˉurl(Fallback) {
    return new URLSearchParams(window.location.search).get("path") || Fallback;
}

export function Replaceˉselectedˉpath(Path, Push) {
    const Url = new URL(window.location.href);
    Url.searchParams.set("path", Path);
    if (Push) {
        Url.hash = "";
    }
    window.history[Push ? "pushState" : "replaceState"]({}, "", Url);
}

export function Setˉcommitˉlinks(Manifest) {
    const Shortˉcommit = Manifest.commit.slice(0, 12);
    document.querySelectorAll("[data-repository-commit]").forEach((Element) => {
        Element.textContent = Shortˉcommit;
        if (Element instanceof HTMLAnchorElement) {
            Element.href = `${Manifest.repository}/commit/${encodeURIComponent(Manifest.commit)}`;
        }
    });
}
