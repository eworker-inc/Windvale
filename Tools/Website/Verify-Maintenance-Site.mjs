import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath, pathToFileURL } from "node:url";

const Repositoryˉroot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..", "..");
const Maintenanceˉroot = path.join(Repositoryˉroot, "Website", "Maintenance");
const Expectedˉhtml = await readFile(path.join(Maintenanceˉroot, "index.html"), "utf8");
const Workerˉmodule = await import(pathToFileURL(path.join(Maintenanceˉroot, "_worker.js")));

assert.equal(Workerˉmodule.Maintenanceˉhtml, Expectedˉhtml, "The Worker response and fallback page must remain byte-identical.");
assert.ok(Buffer.byteLength(Expectedˉhtml, "utf8") <= 16 * 1024, "The maintenance response must remain at most 16 KiB.");
assert.ok(Expectedˉhtml.includes("Temporarily offline for maintenance."), "The maintenance reason must remain visible.");
assert.ok(!/https?:\/\//u.test(Expectedˉhtml), "The maintenance page must not load or link to external resources.");
assert.ok(!/<script\b/iu.test(Expectedˉhtml), "The maintenance page must remain script-free.");

const Cases = [
    ["GET", "https://windvale.ca/"],
    ["GET", "https://windvale.ca/docs/"],
    ["GET", "https://windvale.ca/code/Compiler/Windvale/Source-Wvb-Core.wv"],
    ["GET", "https://windvale.ca/playground/"],
    ["GET", "https://windvale.ca/api/supporters"],
    ["GET", "https://windvale.ca/deployment.json"],
    ["POST", "https://windvale.ca/arbitrary/mutation"],
    ["HEAD", "https://windvale.ca/"],
];

for (const [Method, Url] of Cases) {
    const Responseˉvalue = await Workerˉmodule.default.fetch(new Request(Url, { method: Method }));
    assert.equal(Responseˉvalue.status, 503, `${Method} ${Url} must return 503.`);
    assert.equal(Responseˉvalue.headers.get("cache-control"), "no-store, max-age=0");
    assert.equal(Responseˉvalue.headers.get("content-type"), "text/html; charset=utf-8");
    assert.equal(Responseˉvalue.headers.get("retry-after"), "3600");
    assert.equal(Responseˉvalue.headers.get("x-content-type-options"), "nosniff");
    assert.equal(Responseˉvalue.headers.get("x-frame-options"), "DENY");
    assert.equal(Responseˉvalue.headers.get("x-robots-tag"), "noindex, nofollow, noarchive");
    assert.match(Responseˉvalue.headers.get("content-security-policy"), /default-src 'none'/u);
    assert.equal(await Responseˉvalue.text(), Method === "HEAD" ? "" : Expectedˉhtml);
}

console.log(`Windvale maintenance site verification passed: ${Cases.length} bounded requests, ${Buffer.byteLength(Expectedˉhtml, "utf8")} response bytes.`);
