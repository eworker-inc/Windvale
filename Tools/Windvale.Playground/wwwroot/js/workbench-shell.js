const COMMAND_LIMIT = 4 * 1024;

export function Createˉworkbenchˉshell(Options) {
    const Workspace = Options.Workspace;

    return Object.freeze({
        async Execute(Line) {
            if (typeof Line !== "string" || Line.length > COMMAND_LIMIT) {
                throw new Error(`Commands are limited to ${COMMAND_LIMIT.toLocaleString()} characters.`);
            }
            const Arguments = Parseˉcommandˉline(Line);
            if (Arguments.length === 0) {
                return { Lines: [] };
            }

            const Command = Arguments[0].toLowerCase();
            if (Command === "help") {
                Requireˉargumentˉcount(Arguments, 1, 1);
                return { Lines: Helpˉlines() };
            }
            if (Command === "pwd") {
                Requireˉargumentˉcount(Arguments, 1, 1);
                return { Lines: ["/workspace"] };
            }
            if (Command === "ls") {
                Requireˉargumentˉcount(Arguments, 1, 1);
                const Entries = await Workspace.List();
                return {
                    Lines: Entries.length === 0
                        ? ["(empty workspace)"]
                        : Entries.map(Entry => `${Entry.Name.padEnd(32, " ")} ${Formatˉbytes(Entry.Bytes)}`),
                };
            }
            if (Command === "cat") {
                Requireˉargumentˉcount(Arguments, 2, 2);
                return { Lines: [(await Workspace.Readˉtext(Arguments[1])).replace(/\n$/u, "")] };
            }
            if (Command === "save") {
                Requireˉargumentˉcount(Arguments, 1, 2);
                const Active = Options.Readˉactiveˉsource();
                const Name = Arguments[1] ?? Active.Name;
                const Written = await Workspace.Writeˉtext(Name, Active.Source);
                return { Lines: [`saved ${Written.Name} (${Formatˉbytes(Written.Bytes)})`] };
            }
            if (Command === "open") {
                Requireˉargumentˉcount(Arguments, 2, 2);
                const Source = await Workspace.Readˉtext(Arguments[1]);
                Options.Openˉsource(Arguments[1], Source);
                return { Lines: [`opened ${Arguments[1]} in the editor`] };
            }
            if (Command === "write") {
                Requireˉargumentˉcount(Arguments, 3, Number.MAX_SAFE_INTEGER);
                const Value = `${Arguments.slice(2).join(" ")}\n`;
                const Written = await Workspace.Writeˉtext(Arguments[1], Value);
                return { Lines: [`wrote ${Written.Name} (${Formatˉbytes(Written.Bytes)})`] };
            }
            if (Command === "rm") {
                Requireˉargumentˉcount(Arguments, 2, 2);
                await Workspace.Delete(Arguments[1]);
                return { Lines: [`removed ${Arguments[1]} from the browser workspace`] };
            }
            if (Command === "run") {
                Requireˉargumentˉcount(Arguments, 1, 2);
                const Source = Arguments.length === 2
                    ? { Name: Arguments[1], Source: await Workspace.Readˉtext(Arguments[1]) }
                    : Options.Readˉactiveˉsource();
                const Result = await Options.Runˉsource(Source.Name, Source.Source);
                return { Lines: Formatˉrunˉresult(Result) };
            }
            if (Command === "status") {
                Requireˉargumentˉcount(Arguments, 1, 1);
                const Entries = await Workspace.List();
                const Bytes = Entries.reduce((Sum, Entry) => Sum + Entry.Bytes, 0);
                return {
                    Lines: [
                        `workspace  /workspace · ${Workspace.Persistence}`,
                        `files      ${Entries.length} · ${Formatˉbytes(Bytes)}`,
                        "runtime    direct compiler Wasm → verified WVB → scalar interpreter Wasm",
                        "authority  console.write_line only when the editor grant is enabled",
                    ],
                };
            }
            if (Command === "clear") {
                Requireˉargumentˉcount(Arguments, 1, 1);
                return { Clear: true, Lines: [] };
            }

            throw new Error(`Unknown command: ${Arguments[0]}. Type 'help' for available commands.`);
        },
    });
}

export function Parseˉcommandˉline(Line) {
    const Arguments = [];
    let Current = "";
    let Quote = null;
    let Escaped = false;
    let Started = false;

    for (const Character of Line.trim()) {
        if (Escaped) {
            Current += Character;
            Escaped = false;
            Started = true;
            continue;
        }
        if (Character === "\\" && Quote !== "'") {
            Escaped = true;
            Started = true;
            continue;
        }
        if (Quote !== null) {
            if (Character === Quote) {
                Quote = null;
            } else {
                Current += Character;
            }
            Started = true;
            continue;
        }
        if (Character === "'" || Character === '"') {
            Quote = Character;
            Started = true;
            continue;
        }
        if (/\s/u.test(Character)) {
            if (Started) {
                Arguments.push(Current);
                Current = "";
                Started = false;
            }
            continue;
        }
        Current += Character;
        Started = true;
    }

    if (Escaped) {
        throw new Error("A command cannot end with an escape character.");
    }
    if (Quote !== null) {
        throw new Error("A quoted command argument is not closed.");
    }
    if (Started) {
        Arguments.push(Current);
    }
    return Arguments;
}

function Helpˉlines() {
    return [
        "Windvale Workbench commands",
        "  help                 show this command list",
        "  pwd                  show the browser workspace path",
        "  ls                   list persistent workspace files",
        "  cat <file>           print one UTF-8 text file",
        "  save [file]          save the active editor source",
        "  open <file>          open a workspace file in the editor",
        "  write <file> <text>  replace a workspace text file",
        "  rm <file>            remove a workspace file",
        "  run [file]           compile, verify, and run source",
        "  status               show storage, runtime, and authority",
        "  clear                clear this terminal",
        "",
        "One foreground command runs at a time. Pipes, redirection, directories,",
        "and guest filesystem access are intentionally outside this first slice.",
    ];
}

function Formatˉrunˉresult(Result) {
    const Lines = [];
    if (Result.Standardˉoutput.length > 0) {
        Lines.push(Result.Standardˉoutput.replace(/\n$/u, ""));
    }
    if (Result.Executionˉstatus === 0) {
        Lines.push(
            `[exit 0 · Main ${Result.Executionˉresult ?? "—"} · ${Result.Wvbˉbytes.toLocaleString()} WVB bytes · ${Result.Elapsedˉseconds.toFixed(1)}s]`,
        );
    } else {
        Lines.push(`[execution failed · WVR${Result.Executionˉstatus}]`);
    }
    Lines.push(`sha256 ${Result.Wvbˉsha256}`);
    return Lines;
}

function Requireˉargumentˉcount(Arguments, Minimum, Maximum) {
    if (Arguments.length < Minimum || Arguments.length > Maximum) {
        throw new Error(`Invalid arguments for '${Arguments[0]}'. Type 'help' for usage.`);
    }
}

function Formatˉbytes(Bytes) {
    if (Bytes < 1024) {
        return `${Bytes.toLocaleString()} B`;
    }
    return `${(Bytes / 1024).toFixed(1)} KiB`;
}
