const WORKSPACE_DIRECTORY = "Windvale-Workbench-v1";
const MAXIMUM_FILES = 64;
const MAXIMUM_FILE_BYTES = 64 * 1024;
const MAXIMUM_WORKSPACE_BYTES = 2 * 1024 * 1024;
const Textˉencoder = new TextEncoder();
const Textˉdecoder = new TextDecoder("utf-8", { fatal: true });

export async function Openˉworkbenchˉworkspace() {
    if (navigator.storage?.getDirectory === undefined) {
        return new Memoryˉworkspace();
    }

    try {
        const Root = await navigator.storage.getDirectory();
        const Directory = await Root.getDirectoryHandle(WORKSPACE_DIRECTORY, { create: true });
        return new Originˉprivateˉworkspace(Directory);
    }
    catch {
        return new Memoryˉworkspace();
    }
}

export function Validateˉworkspaceˉname(Name) {
    if (typeof Name !== "string" || Name.length === 0 || Name.length > 255 ||
        Name === "." || Name === ".." ||
        !/^[A-Za-z0-9][A-Za-z0-9._-]*$/u.test(Name)) {
        throw new Error(
            "Workspace names must be one ASCII segment of 1-255 letters, digits, '.', '_', or '-'.",
        );
    }
    return Name;
}

class Originˉprivateˉworkspace {
    constructor(Directory) {
        this.Directory = Directory;
        this.Persistence = "origin-private";
    }

    async List() {
        const Entries = [];
        let Totalˉbytes = 0;
        for await (const [Name, Handle] of this.Directory.entries()) {
            if (Handle.kind !== "file") {
                continue;
            }
            Validateˉworkspaceˉname(Name);
            const File = await Handle.getFile();
            Requireˉfileˉsize(File.size);
            Totalˉbytes += File.size;
            Requireˉworkspaceˉsize(Totalˉbytes);
            Entries.push({ Name, Bytes: File.size });
            if (Entries.length > MAXIMUM_FILES) {
                throw new Error(`The workspace contains more than ${MAXIMUM_FILES} files.`);
            }
        }
        Entries.sort((Left, Right) => Left.Name.localeCompare(Right.Name, "en", {
            sensitivity: "variant",
        }));
        return Entries;
    }

    async Readˉtext(Name) {
        Validateˉworkspaceˉname(Name);
        try {
            const Handle = await this.Directory.getFileHandle(Name);
            const File = await Handle.getFile();
            if (File.size > MAXIMUM_FILE_BYTES) {
                throw new Error(`The file exceeds the ${MAXIMUM_FILE_BYTES.toLocaleString()}-byte workspace limit.`);
            }
            return Textˉdecoder.decode(await File.arrayBuffer());
        }
        catch (Failure) {
            if (Failure instanceof DOMException && Failure.name === "NotFoundError") {
                throw new Error(`Workspace file not found: ${Name}`);
            }
            throw Failure;
        }
    }

    async Writeˉtext(Name, Value) {
        Validateˉworkspaceˉname(Name);
        const Bytes = Textˉencoder.encode(Value).byteLength;
        Requireˉfileˉsize(Bytes);
        const Entries = await this.List();
        const Existing = Entries.find(Entry => Entry.Name === Name);
        if (Existing === undefined && Entries.length >= MAXIMUM_FILES) {
            throw new Error(`The workspace is limited to ${MAXIMUM_FILES} files.`);
        }
        const Total = Entries.reduce((Sum, Entry) => Sum + Entry.Bytes, 0) -
            (Existing?.Bytes ?? 0) + Bytes;
        Requireˉworkspaceˉsize(Total);

        const Handle = await this.Directory.getFileHandle(Name, { create: true });
        const Writable = await Handle.createWritable();
        try {
            await Writable.write(Value);
            await Writable.close();
        }
        catch (Failure) {
            await Writable.abort().catch(() => {});
            throw Failure;
        }
        return { Name, Bytes };
    }

    async Delete(Name) {
        Validateˉworkspaceˉname(Name);
        try {
            await this.Directory.removeEntry(Name);
        }
        catch (Failure) {
            if (Failure instanceof DOMException && Failure.name === "NotFoundError") {
                throw new Error(`Workspace file not found: ${Name}`);
            }
            throw Failure;
        }
    }
}

class Memoryˉworkspace {
    constructor() {
        this.Files = new Map();
        this.Persistence = "session-memory";
    }

    async List() {
        return Array.from(this.Files, ([Name, Value]) => ({
            Name,
            Bytes: Textˉencoder.encode(Value).byteLength,
        })).sort((Left, Right) => Left.Name.localeCompare(Right.Name, "en", {
            sensitivity: "variant",
        }));
    }

    async Readˉtext(Name) {
        Validateˉworkspaceˉname(Name);
        if (!this.Files.has(Name)) {
            throw new Error(`Workspace file not found: ${Name}`);
        }
        return this.Files.get(Name);
    }

    async Writeˉtext(Name, Value) {
        Validateˉworkspaceˉname(Name);
        const Bytes = Textˉencoder.encode(Value).byteLength;
        Requireˉfileˉsize(Bytes);
        if (!this.Files.has(Name) && this.Files.size >= MAXIMUM_FILES) {
            throw new Error(`The workspace is limited to ${MAXIMUM_FILES} files.`);
        }
        const Entries = await this.List();
        const Existing = Entries.find(Entry => Entry.Name === Name);
        const Total = Entries.reduce((Sum, Entry) => Sum + Entry.Bytes, 0) -
            (Existing?.Bytes ?? 0) + Bytes;
        Requireˉworkspaceˉsize(Total);
        this.Files.set(Name, Value);
        return { Name, Bytes };
    }

    async Delete(Name) {
        Validateˉworkspaceˉname(Name);
        if (!this.Files.delete(Name)) {
            throw new Error(`Workspace file not found: ${Name}`);
        }
    }
}

function Requireˉfileˉsize(Bytes) {
    if (Bytes > MAXIMUM_FILE_BYTES) {
        throw new Error(`Workspace files are limited to ${MAXIMUM_FILE_BYTES.toLocaleString()} UTF-8 bytes.`);
    }
}

function Requireˉworkspaceˉsize(Bytes) {
    if (Bytes > MAXIMUM_WORKSPACE_BYTES) {
        throw new Error(`The workspace is limited to ${MAXIMUM_WORKSPACE_BYTES.toLocaleString()} bytes.`);
    }
}
