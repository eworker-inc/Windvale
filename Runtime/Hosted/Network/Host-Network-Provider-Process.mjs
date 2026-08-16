import process from "node:process";
import {
    Decodeˉhostˉnetworkˉrequest,
    HOST_NETWORK_MAX_REQUEST_BYTES,
    HOST_NETWORK_REQUEST_HEADER_BYTES,
    Readˉframedˉrecordˉlength,
} from "./Host-Network-Protocol.mjs";

export function Runˉhostˉnetworkˉprovider(Provider) {
    let Pending = Buffer.alloc(0);
    let Publishing = Promise.resolve();
    const Active = new Set();

    const Publish = Bytes => {
        Publishing = Publishing.then(() => new Promise((Resolve, Reject) => {
            if (process.stdout.write(Bytes)) Resolve();
            else {
                process.stdout.once("drain", Resolve);
                process.stdout.once("error", Reject);
            }
        }));
        Publishing.catch(() => {
            Provider.teardown();
            process.exitCode = 74;
        });
    };

    const Dispatch = Bytes => {
        let Request;
        try { Request = Decodeˉhostˉnetworkˉrequest(Bytes); } catch {
            Provider.teardown();
            process.exitCode = 65;
            process.stdin.destroy();
            return;
        }
        const Key = Request.requestId.toString();
        if (Active.has(Key)) {
            Provider.teardown();
            process.exitCode = 65;
            process.stdin.destroy();
            return;
        }
        Active.add(Key);
        Provider.handle(Bytes).then(Publish, () => {
            Provider.teardown();
            process.exitCode = 70;
        }).finally(() => Active.delete(Key));
    };

    process.stdin.on("data", Chunk => {
        if (process.exitCode) return;
        Pending = Buffer.concat([Pending, Chunk]);
        if (Pending.length > HOST_NETWORK_MAX_REQUEST_BYTES * 2) {
            Provider.teardown();
            process.exitCode = 65;
            process.stdin.destroy();
            return;
        }
        while (Pending.length >= 12) {
            let Total;
            try {
                Total = Readˉframedˉrecordˉlength(
                    Pending, "WVNR", HOST_NETWORK_REQUEST_HEADER_BYTES,
                    HOST_NETWORK_MAX_REQUEST_BYTES,
                );
            } catch {
                Provider.teardown();
                process.exitCode = 65;
                process.stdin.destroy();
                return;
            }
            if (Pending.length < Total) break;
            const Request = Pending.subarray(0, Total);
            Pending = Pending.subarray(Total);
            Dispatch(Request);
        }
    });

    process.stdin.on("end", async () => {
        if (Pending.length !== 0 && !process.exitCode) process.exitCode = 65;
        while (Active.size !== 0) await new Promise(Resolve => setTimeout(Resolve, 1));
        await Publishing.catch(() => {});
        Provider.teardown();
    });

    process.stdin.on("error", () => {
        Provider.teardown();
        process.exitCode = 74;
    });

    for (const Signal of ["SIGINT", "SIGTERM"]) {
        process.on(Signal, () => {
            Provider.teardown();
            process.exit(0);
        });
    }
}
