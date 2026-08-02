let Nextˉrequestˉid = 1;

export function Execute(Bytes, Timeoutˉmilliseconds, Instructionˉlimit = 1_000_000) {
    if (!(Bytes instanceof Uint8Array)) {
        return Promise.resolve(Failure("The browser host did not receive WebAssembly bytes."));
    }
    if (!Number.isInteger(Timeoutˉmilliseconds) || Timeoutˉmilliseconds < 1) {
        return Promise.resolve(Failure("The WebAssembly worker timeout is invalid."));
    }
    if (!Number.isInteger(Instructionˉlimit) ||
        Instructionˉlimit < 1 ||
        Instructionˉlimit > 2_147_483_647) {
        return Promise.resolve(Failure("The Windvale instruction limit is invalid."));
    }

    const Requestˉid = Nextˉrequestˉid++;
    const Transferˉbytes = Bytes.slice();
    const Workerˉurl = new URL("./windvale-wasm-worker.js", import.meta.url);

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
            () => Finish(Failure("The disposable WebAssembly worker exceeded its time limit.")),
            Timeoutˉmilliseconds);

        Workerˉinstance.onmessage = Event => {
            const Message = Event.data;
            if (Message === null ||
                typeof Message !== "object" ||
                Message.RequestId !== Requestˉid) {
                Finish(Failure("The WebAssembly worker returned an invalid response."));
                return;
            }
            Finish(Message);
        };
        Workerˉinstance.onerror = () =>
            Finish(Failure("The disposable WebAssembly worker failed."));
        Workerˉinstance.postMessage(
            {
                RequestId: Requestˉid,
                Bytes: Transferˉbytes.buffer,
                InstructionLimit: Instructionˉlimit,
            },
            [Transferˉbytes.buffer]);
    });
}

function Failure(Error) {
    return {
        Succeeded: false,
        ExecutionAbi: null,
        Status: null,
        Result: null,
        ExecutedInstructions: null,
        Error,
    };
}
