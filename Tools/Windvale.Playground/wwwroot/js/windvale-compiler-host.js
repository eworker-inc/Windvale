let Nextˉrequestˉid = 1;

export function Compileˉandˉrun(
    Source,
    Timeoutˉmilliseconds = 300_000,
    Executionˉinstructionˉlimit = 1_000_000,
    Authorizeˉconsoleˉwriteˉline = false,
) {
    if (typeof Source !== "string") {
        return Promise.resolve(Failure("The compiler host requires Windvale source text."));
    }
    if (!Number.isInteger(Timeoutˉmilliseconds) ||
        Timeoutˉmilliseconds < 1 ||
        Timeoutˉmilliseconds > 600_000) {
        return Promise.resolve(Failure("The compiler worker timeout is invalid."));
    }
    if (!Number.isInteger(Executionˉinstructionˉlimit) ||
        Executionˉinstructionˉlimit < 1 ||
        Executionˉinstructionˉlimit > 20_000_000) {
        return Promise.resolve(Failure("The execution instruction limit is invalid."));
    }
    if (typeof Authorizeˉconsoleˉwriteˉline !== "boolean") {
        return Promise.resolve(Failure(
            "The console.write_line authorization must be boolean.",
        ));
    }
    const Sourceˉbytes = new TextEncoder().encode(Source);
    if (Sourceˉbytes.byteLength === 0 || Sourceˉbytes.byteLength > 64 * 1024) {
        return Promise.resolve(Failure("The Windvale source is outside the 64 KiB browser limit."));
    }
    try {
        if (new TextDecoder("utf-8", { fatal: true }).decode(Sourceˉbytes) !== Source) {
            return Promise.resolve(Failure("The Windvale source contains invalid Unicode."));
        }
    }
    catch {
        return Promise.resolve(Failure("The Windvale source contains invalid Unicode."));
    }

    const Requestˉid = Nextˉrequestˉid++;
    const Transferˉsource = Sourceˉbytes.slice();
    const Workerˉurl = new URL("./windvale-compiler-worker.js", import.meta.url);
    return new Promise(Resolve => {
        const Workerˉinstance = new Worker(Workerˉurl, { type: "module" });
        let Settled = false;
        const Finish = Result => {
            if (Settled) {
                return;
            }
            Settled = true;
            clearTimeout(Timeout);
            Workerˉinstance.terminate();
            Resolve(Result);
        };
        const Timeout = setTimeout(
            () => Finish(Failure("The disposable compiler worker exceeded its time limit.")),
            Timeoutˉmilliseconds,
        );
        Workerˉinstance.onmessage = Event => {
            const Message = Event.data;
            if (Message === null ||
                typeof Message !== "object" ||
                Message.RequestId !== Requestˉid) {
                Finish(Failure("The compiler worker returned an invalid response."));
                return;
            }
            Finish(Message);
        };
        Workerˉinstance.onerror = () =>
            Finish(Failure("The disposable compiler worker failed."));
        Workerˉinstance.postMessage({
            RequestId: Requestˉid,
            Source: Transferˉsource.buffer,
            ExecutionInstructionLimit: Executionˉinstructionˉlimit,
            AuthorizeConsoleWriteLine: Authorizeˉconsoleˉwriteˉline,
        }, [Transferˉsource.buffer]);
    });
}

function Failure(Error) {
    return {
        Succeeded: false,
        Error,
        Wvb: null,
        WvbSha256: null,
        CompilerInstructions: null,
        ExecutionStatus: null,
        ExecutionResult: null,
        StandardOutput: null,
        ModuleProfile: null,
        ExecutionGuestInstructions: null,
        ExecutionOuterInstructions: null,
    };
}
