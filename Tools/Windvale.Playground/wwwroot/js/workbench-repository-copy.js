const MAXIMUM_REPOSITORY_PATH_CHARACTERS = 1_024;
export const MAXIMUM_REPOSITORY_SOURCE_BYTES = 64 * 1_024;
export const MAXIMUM_REPOSITORY_FRAGMENT_BYTES = 512 * 1_024;

export function Selectˉrepositoryˉcopy(Manifest, Requestedˉpath) {
    if (Manifest?.version !== 1 ||
        typeof Manifest.commit !== "string" ||
        !/^[0-9a-f]{40}$/u.test(Manifest.commit) ||
        !Array.isArray(Manifest.files)) {
        throw new Error("The repository snapshot has an unsupported identity.");
    }
    Validateˉrepositoryˉpath(Requestedˉpath);

    const File = Manifest.files.find(Item => Item?.path === Requestedˉpath);
    if (File === undefined) {
        throw new Error("The requested source is not present in this repository snapshot.");
    }
    if (File.kind !== "text" || File.language !== "windvale" ||
        !Number.isSafeInteger(File.size) || File.size < 0 ||
        File.size > MAXIMUM_REPOSITORY_SOURCE_BYTES ||
        typeof File.contentHash !== "string" ||
        !/^[0-9a-f]{64}$/u.test(File.contentHash) ||
        typeof File.publishedUrl !== "string" ||
        !/^\/repository\/code\/[0-9a-f]{64}\.html$/u.test(File.publishedUrl)) {
        throw new Error("The requested source is not eligible for a Workbench copy.");
    }

    return Object.freeze({
        Commit: Manifest.commit,
        Contentˉhash: File.contentHash,
        Fileˉname: Requestedˉpath.split("/").at(-1),
        Path: Requestedˉpath,
        Publishedˉurl: File.publishedUrl,
        Size: File.size,
    });
}

export function Chooseˉworkspaceˉcopyˉname(Fileˉname, Existingˉentries) {
    Validateˉworkspaceˉcopyˉname(Fileˉname);
    if (!Array.isArray(Existingˉentries) || Existingˉentries.length > 64 ||
        Existingˉentries.some(Entry => typeof Entry?.Name !== "string")) {
        throw new Error("The workspace listing is invalid.");
    }
    const Existingˉnames = new Set(Existingˉentries.map(Entry => Entry.Name));
    if (!Existingˉnames.has(Fileˉname)) {
        return Fileˉname;
    }

    const Extension = ".wv";
    const Base = Fileˉname.slice(0, -Extension.length);
    for (let Number = 1; Number <= 65; Number++) {
        const Suffix = Number === 1 ? "-Repository-Copy" : `-Repository-Copy-${Number}`;
        const Maximumˉbaseˉcharacters = 255 - Suffix.length - Extension.length;
        const Candidate = `${Base.slice(0, Maximumˉbaseˉcharacters)}${Suffix}${Extension}`;
        if (!Existingˉnames.has(Candidate)) {
            return Candidate;
        }
    }
    throw new Error("No bounded repository-copy name is available in the workspace.");
}

function Validateˉrepositoryˉpath(Value) {
    if (typeof Value !== "string" || Value.length === 0 ||
        Value.length > MAXIMUM_REPOSITORY_PATH_CHARACTERS ||
        Value.includes("\\") || !Value.endsWith(".wv")) {
        throw new Error("Only a bounded Windvale source path can be copied.");
    }
    const Segments = Value.split("/");
    if (Segments.some(Segment => Segment.length === 0 || Segment.length > 255 ||
        Segment === "." || Segment === ".." || /[\u0000-\u001f\u007f]/u.test(Segment))) {
        throw new Error("The repository source path is malformed.");
    }
}

function Validateˉworkspaceˉcopyˉname(Value) {
    if (typeof Value !== "string" || Value.length === 0 || Value.length > 255 ||
        !/^[A-Za-z0-9][A-Za-z0-9._-]*\.wv$/u.test(Value)) {
        throw new Error("The repository source name cannot enter the browser workspace.");
    }
}
