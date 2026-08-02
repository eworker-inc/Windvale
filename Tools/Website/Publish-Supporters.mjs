import fs from "node:fs/promises";
import path from "node:path";
import process from "node:process";
import { fileURLToPath } from "node:url";
import {
    Normalizeˉsupporterˉroll,
    Supportersˉcontract,
} from "../../functions/api/supporters.js";

const Toolˉdirectory = path.dirname(fileURLToPath(import.meta.url));
const Repositoryˉroot = path.resolve(Toolˉdirectory, "../..");

function Fail(Message) {
    throw new Error(Message);
}

function Requireˉenvironment(Name) {
    const Value = process.env[Name];
    if (typeof Value !== "string" || !Value.trim()) {
        Fail(`Missing required environment variable: ${Name}`);
    }
    return Value.trim();
}

function Isˉinsideˉrepository(Targetˉpath) {
    const Relative = path.relative(Repositoryˉroot, Targetˉpath);
    return Relative === "" || (!Relative.startsWith(`..${path.sep}`) && Relative !== "..");
}

async function Main() {
    const Arguments = process.argv.slice(2);
    const Dryˉrun = Arguments.includes("--dry-run");
    const Paths = Arguments.filter((Argument) => Argument !== "--dry-run");
    if (Paths.length !== 1) {
        Fail("Usage: node Tools/Website/Publish-Supporters.mjs [--dry-run] <supporters.json>");
    }

    const Inputˉpath = path.resolve(Paths[0]);
    if (Isˉinsideˉrepository(Inputˉpath)) {
        Fail("The supporter source file must remain outside the Windvale repository.");
    }

    const Source = JSON.parse(await fs.readFile(Inputˉpath, "utf8"));
    const Publicˉroll = Normalizeˉsupporterˉroll(Source);
    Publicˉroll.updated = new Date().toISOString().slice(0, 10);

    if (Dryˉrun) {
        process.stdout.write(
            `Supporter roll is valid: ${Publicˉroll.supporters.length} public name(s), `
            + `${Object.values(Publicˉroll.anonymousCounts).reduce((Total, Count) => Total + Count, 0)} anonymous.\n`,
        );
        return;
    }

    const Accountˉid = Requireˉenvironment("CLOUDFLARE_ACCOUNT_ID");
    const Namespaceˉid = Requireˉenvironment("WINDVALE_SUPPORTERS_NAMESPACE_ID");
    const Apiˉtoken = Requireˉenvironment("CLOUDFLARE_API_TOKEN");
    const Endpoint = [
        "https://api.cloudflare.com/client/v4/accounts",
        encodeURIComponent(Accountˉid),
        "storage/kv/namespaces",
        encodeURIComponent(Namespaceˉid),
        "values",
        encodeURIComponent(Supportersˉcontract.Key),
    ].join("/");
    const Response = await fetch(Endpoint, {
        method: "PUT",
        headers: {
            Authorization: `Bearer ${Apiˉtoken}`,
            "Content-Type": "application/json; charset=utf-8",
        },
        body: `${JSON.stringify(Publicˉroll)}\n`,
    });
    if (!Response.ok) {
        Fail(`Cloudflare KV update failed with HTTP ${Response.status}.`);
    }

    process.stdout.write(
        `Published ${Publicˉroll.supporters.length} public supporter name(s) and `
        + `${Object.values(Publicˉroll.anonymousCounts).reduce((Total, Count) => Total + Count, 0)} anonymous supporter(s).\n`,
    );
}

Main().catch((Error) => {
    process.stderr.write(`${Error instanceof Error ? Error.message : String(Error)}\n`);
    process.exitCode = 1;
});
