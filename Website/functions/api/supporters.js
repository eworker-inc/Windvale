const SUPPORTERS_KEY = "public-supporters-v1";
const MAX_SUPPORTERS = 500;
const MAX_ANONYMOUS_PER_TIER = 1_000_000;
const TIER_KEYS = Object.freeze([
    "cornerstone",
    "champion",
    "accelerator",
    "builder",
    "spark",
    "any",
]);
const TIER_KEY_SET = new Set(TIER_KEYS);

function Jsonˉresponse(Status, Value, Cacheˉcontrol) {
    return new Response(`${JSON.stringify(Value)}\n`, {
        status: Status,
        headers: {
            "Cache-Control": Cacheˉcontrol,
            "Content-Type": "application/json; charset=utf-8",
            "X-Content-Type-Options": "nosniff",
        },
    });
}

function Emptyˉsupporterˉroll() {
    return {
        version: 1,
        updated: null,
        supporters: [],
        anonymousCounts: Object.fromEntries(TIER_KEYS.map((Tier) => [Tier, 0])),
    };
}

function Normalizeˉdisplayˉname(Value) {
    if (typeof Value !== "string") {
        return null;
    }

    const Displayˉname = Value.trim().replace(/\s+/gu, " ");
    if (Displayˉname.length === 0 || Displayˉname.length > 80 || /[\p{Cc}\p{Cf}]/u.test(Displayˉname)) {
        return null;
    }

    return Displayˉname;
}

function Normalizeˉsupporter(Value) {
    if (Value === null || typeof Value !== "object" || Array.isArray(Value)) {
        return null;
    }

    const Displayˉname = Normalizeˉdisplayˉname(Value.displayName);
    const Tier = typeof Value.tier === "string" && TIER_KEY_SET.has(Value.tier) ? Value.tier : null;
    const Since = typeof Value.since === "string" && /^\d{4}-(0[1-9]|1[0-2])$/u.test(Value.since)
        ? Value.since
        : null;
    if (Displayˉname === null || Tier === null || Since === null) {
        return null;
    }

    return {
        displayName: Displayˉname,
        tier: Tier,
        since: Since,
    };
}

function Normalizeˉanonymousˉcounts(Value) {
    const Counts = {};
    for (const Tier of TIER_KEYS) {
        const Count = Value !== null && typeof Value === "object" && !Array.isArray(Value)
            ? Value[Tier]
            : 0;
        Counts[Tier] = Number.isSafeInteger(Count) && Count >= 0 && Count <= MAX_ANONYMOUS_PER_TIER
            ? Count
            : 0;
    }
    return Counts;
}

export function Normalizeˉsupporterˉroll(Value) {
    if (Value === null || typeof Value !== "object" || Array.isArray(Value) || Value.version !== 1) {
        throw new Error("Unsupported supporter-roll document.");
    }

    const Updated = Value.updated === null
        ? null
        : typeof Value.updated === "string" && /^\d{4}-\d{2}-\d{2}$/u.test(Value.updated)
            ? Value.updated
            : null;
    const Sourceˉsupporters = Array.isArray(Value.supporters) ? Value.supporters.slice(0, MAX_SUPPORTERS) : [];
    const Supporters = Sourceˉsupporters.map(Normalizeˉsupporter).filter((Supporter) => Supporter !== null);

    return {
        version: 1,
        updated: Updated,
        supporters: Supporters,
        anonymousCounts: Normalizeˉanonymousˉcounts(Value.anonymousCounts),
    };
}

export async function onRequestGet(Context) {
    const Supportersˉstore = Context?.env?.WINDVALE_SUPPORTERS;
    if (typeof Supportersˉstore?.get !== "function") {
        return Jsonˉresponse(
            503,
            { error: "Supporter roll is not configured." },
            "no-store",
        );
    }

    try {
        const Stored = await Supportersˉstore.get(SUPPORTERS_KEY, "json");
        const Publicˉroll = Stored === null ? Emptyˉsupporterˉroll() : Normalizeˉsupporterˉroll(Stored);
        return Jsonˉresponse(
            200,
            Publicˉroll,
            "public, max-age=60, s-maxage=300",
        );
    } catch {
        return Jsonˉresponse(
            503,
            { error: "Supporter roll is temporarily unavailable." },
            "no-store",
        );
    }
}

export const Supportersˉcontract = Object.freeze({
    Key: SUPPORTERS_KEY,
    Maxˉsupporters: MAX_SUPPORTERS,
    Tierˉkeys: TIER_KEYS,
});
