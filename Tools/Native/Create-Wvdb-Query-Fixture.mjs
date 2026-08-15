import crypto from "node:crypto";
import fs from "node:fs";

const [output] = process.argv.slice(2);
if (!output) {
  process.stderr.write("Usage: node Create-Wvdb-Query-Fixture.mjs <output.bin>\n");
  process.exit(64);
}

const DATABASE_MAGIC = 1_111_774_807;
const PAGE_MAGIC = 1_196_447_319;
const DATABASE_HEADER_BYTES = 32;
const PAGE_BYTES = 256;
const PAGE_HEADER_BYTES = 32;
const value = Buffer.alloc(DATABASE_HEADER_BYTES + PAGE_BYTES);

value.writeUInt32LE(DATABASE_MAGIC, 0);
value.writeUInt32LE(1, 4);
value.writeUInt32LE(value.length, 8);
value.writeUInt32LE(PAGE_BYTES, 12);
value.writeUInt32LE(1, 16);
value.writeUInt32LE(0, 20);
value.writeUInt32LE(16, 24);

const page = DATABASE_HEADER_BYTES;
value.writeUInt32LE(PAGE_MAGIC, page);
value.writeUInt32LE(1, page + 4);
value.writeUInt32LE(0, page + 8);
value.writeUInt32LE(1, page + 12);
value.writeUInt32LE(2, page + 16);
value.writeUInt32LE(7, page + PAGE_HEADER_BYTES);
value.writeInt32LE(42, page + PAGE_HEADER_BYTES + 4);
value.writeUInt32LE(9, page + PAGE_HEADER_BYTES + 8);
value.writeInt32LE(-5, page + PAGE_HEADER_BYTES + 12);

let checksum = 0;
for (let index = 0; index < PAGE_BYTES; index += 1) {
  if (index < 24 || index >= 28) checksum += value[page + index];
}
value.writeUInt32LE(checksum, page + 24);

fs.writeFileSync(output, value, { flag: "wx", mode: 0o600 });
const digest = crypto.createHash("sha256").update(value).digest("hex");
process.stdout.write(
  `wvdb query fixture status=Created bytes=${value.length} checksum=${checksum} sha256=${digest}\n`,
);
