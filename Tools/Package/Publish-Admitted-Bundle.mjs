import fs from "node:fs";
import path from "node:path";
import crypto from "node:crypto";

const [mode, bundlePath, expectedDigest, outputRoot] = process.argv.slice(2);
if (mode !== "extract" || !/^[0-9a-f]{64}$/.test(expectedDigest ?? "")) {
  process.stderr.write("Usage: node Publish-Admitted-Bundle.mjs extract <bundle> <sha256> <output-root>\n");
  process.exit(64);
}
const bundle = fs.readFileSync(bundlePath);
const sha256 = value => crypto.createHash("sha256").update(value).digest("hex");
if (bundle.length < 128 || bundle.length > 4_194_304 || sha256(bundle) !== expectedDigest) {
  throw new Error("The admitted bundle identity or policy size changed before extraction.");
}
if (bundle.subarray(0, 4).toString("ascii") !== "WVPB") throw new Error("Invalid bundle magic.");
const indexBytes = Number(bundle.readBigUInt64LE(32));
const contentOffset = Number(bundle.readBigUInt64LE(40));
const contentBytes = Number(bundle.readBigUInt64LE(48));
const blobCount = bundle.readUInt32LE(56);
if (indexBytes < 1 || indexBytes > 1_048_576 || contentOffset !== 128 + indexBytes ||
    contentBytes !== bundle.length - contentOffset) throw new Error("Invalid admitted geometry.");
const index = bundle.subarray(128, 128 + indexBytes).toString("utf8");
const lines = index.split("\n").filter(Boolean).filter(line => line.startsWith("blob "));
if (lines.length !== blobCount) throw new Error("Invalid admitted blob count.");
const inventory = [];
for (let ordinal = 0; ordinal < lines.length; ordinal++) {
  const match = /^blob ([0-9a-f]{64}) ([0-9]+) ([0-9]+)$/.exec(lines[ordinal]);
  if (!match) throw new Error("Invalid admitted blob record.");
  const [, digest, bytesText, offsetText] = match;
  const bytes = Number(bytesText);
  const offset = Number(offsetText);
  if (!Number.isSafeInteger(bytes) || !Number.isSafeInteger(offset) || bytes < 0 ||
      offset < 0 || offset + bytes > contentBytes) throw new Error("Invalid admitted blob range.");
  const value = bundle.subarray(contentOffset + offset, contentOffset + offset + bytes);
  if (sha256(value) !== digest) throw new Error(`Changed admitted blob ${digest}.`);
  const leaf = `Blob-${String(ordinal).padStart(4, "0")}.bin`;
  fs.writeFileSync(path.join(outputRoot, leaf), value, { flag: "wx", mode: 0o600 });
  inventory.push(`blob ${digest} ${bytes} ${leaf}`);
}
process.stdout.write(`${inventory.join("\n")}\n`);
