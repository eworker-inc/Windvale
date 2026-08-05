import assert from "node:assert/strict";
import {
    Normalizeˉsupporterˉroll,
    onRequestGet as Getˉsupporters,
    Supportersˉcontract,
} from "../../Website/functions/api/supporters.js";

const Missingˉbinding = await Getˉsupporters({ env: {} });
assert.equal(Missingˉbinding.status, 503);
assert.equal(Missingˉbinding.headers.get("cache-control"), "no-store");

const Empty = await Getˉsupporters({
    env: { WINDVALE_SUPPORTERS: { get: async () => null } },
});
assert.equal(Empty.status, 200);
assert.deepEqual((await Empty.json()).supporters, []);

const Source = {
    version: 1,
    updated: "2026-08-02",
    supporters: [
        { displayName: "  Ada   Example  ", tier: "builder", since: "2026-08", email: "private@example.test" },
        { displayName: "Invalid Tier", tier: "unknown", since: "2026-08" },
        { displayName: "Invalid Date", tier: "spark", since: "August" },
    ],
    anonymousCounts: { spark: 2, builder: -1, unknown: 99 },
    privateNotes: "must not escape",
};
const Normalized = Normalizeˉsupporterˉroll(Source);
assert.deepEqual(Normalized.supporters, [
    { displayName: "Ada Example", tier: "builder", since: "2026-08" },
]);
assert.equal(Normalized.anonymousCounts.spark, 2);
assert.equal(Normalized.anonymousCounts.builder, 0);
assert.equal(Object.hasOwn(Normalized.anonymousCounts, "unknown"), false);
assert.equal(Object.hasOwn(Normalized, "privateNotes"), false);
assert.equal(JSON.stringify(Normalized).includes("private@example.test"), false);
assert.equal(Supportersˉcontract.Key, "public-supporters-v1");

const Served = await Getˉsupporters({
    env: { WINDVALE_SUPPORTERS: { get: async (Key, Type) => {
        assert.equal(Key, Supportersˉcontract.Key);
        assert.equal(Type, "json");
        return Source;
    } } },
});
assert.equal(Served.status, 200);
assert.deepEqual((await Served.json()).supporters, Normalized.supporters);

const Invalid = await Getˉsupporters({
    env: { WINDVALE_SUPPORTERS: { get: async () => ({ version: 2 }) } },
});
assert.equal(Invalid.status, 503);

process.stdout.write("Windvale supporter data checks passed.\n");
