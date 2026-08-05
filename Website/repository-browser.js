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
            Makeˉelement("span", "material-symbol", "folder"),
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
            Makeˉelement("span", "material-symbol", File.path.endsWith(".md") ? "description" : "draft"),
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
