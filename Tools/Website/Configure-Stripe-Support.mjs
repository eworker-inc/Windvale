const STRIPE_API_BASE = "https://api.stripe.com/v1";
const SUPPORT_RETURN_URL = "https://windvale.ca/support/#supporters-title";
const SUPPORT_SCHEMA = "v1";

const Supportˉtiers = Object.freeze([
    {
        key: "spark",
        name: "Windvale Spark Support",
        amount: 2_000,
        description: "One-time support for the source-available Windvale project at the Spark level.",
    },
    {
        key: "builder",
        name: "Windvale Builder Support",
        amount: 5_000,
        description: "One-time support for the source-available Windvale project at the Builder level.",
    },
    {
        key: "accelerator",
        name: "Windvale Accelerator Support",
        amount: 10_000,
        description: "One-time support for the source-available Windvale project at the Accelerator level.",
    },
    {
        key: "champion",
        name: "Windvale Champion Support",
        amount: 50_000,
        description: "One-time support for the source-available Windvale project at the Champion level.",
    },
    {
        key: "cornerstone",
        name: "Windvale Cornerstone Support",
        amount: 100_000,
        description: "One-time support for the source-available Windvale project at the Cornerstone level.",
    },
    {
        key: "any",
        name: "Choose Your Windvale Support",
        amount: null,
        description: "Choose a one-time amount to support the source-available Windvale project.",
    },
]);

const Apiˉkey = process.env.STRIPE_SECRET_KEY?.trim();
const Isˉdryˉrun = process.argv.includes("--dry-run");

if (!Apiˉkey || !/^(?:rk|sk)_(?:test|live)_[A-Za-z0-9_]+$/u.test(Apiˉkey)) {
    throw new Error("Set STRIPE_SECRET_KEY to a Stripe secret or restricted API key before running this tool.");
}

function Makeˉparameters(Entries) {
    const Parameters = new URLSearchParams();
    for (const [Name, Value] of Entries) {
        if (Value !== undefined && Value !== null) {
            Parameters.append(Name, String(Value));
        }
    }
    return Parameters;
}

async function Stripeˉrequest(Method, Path, Parameters, Idempotencyˉkey) {
    const Headers = {
        Authorization: `Bearer ${Apiˉkey}`,
    };
    let Url = `${STRIPE_API_BASE}${Path}`;
    const Options = { method: Method, headers: Headers };

    if (Parameters && Method === "GET") {
        Url = `${Url}?${Parameters}`;
    } else if (Parameters) {
        Headers["Content-Type"] = "application/x-www-form-urlencoded";
        if (Idempotencyˉkey) {
            Headers["Idempotency-Key"] = Idempotencyˉkey;
        }
        Options.body = Parameters;
    }

    const Response = await fetch(Url, Options);
    const Payload = await Response.json();
    if (!Response.ok) {
        const Message = Payload?.error?.message ?? `Stripe returned HTTP ${Response.status}.`;
        const Parameter = Payload?.error?.param ? ` Parameter: ${Payload.error.param}.` : "";
        throw new Error(`${Message}${Parameter}`);
    }
    return Payload;
}

function Selectˉone(Items, Description) {
    if (Items.length > 1) {
        throw new Error(`More than one ${Description} exists. Resolve the duplicate objects in Stripe before continuing.`);
    }
    return Items[0] ?? null;
}

function Productˉparameters(Tier) {
    return Makeˉparameters([
        ["name", Tier.name],
        ["description", `${Tier.description} No equity, charitable tax receipt, or feature entitlement is provided.`],
        ["active", "true"],
        ["metadata[windvale_support_tier]", Tier.key],
        ["metadata[windvale_support_schema]", SUPPORT_SCHEMA],
    ]);
}

async function Ensureˉproduct(Tier, Existingˉproducts) {
    const Matches = Existingˉproducts.filter((Product) =>
        Product.metadata?.windvale_support_tier === Tier.key
        && Product.metadata?.windvale_support_schema === SUPPORT_SCHEMA);
    const Existing = Selectˉone(Matches, `Windvale ${Tier.key} product`);
    if (Isˉdryˉrun) {
        return { id: Existing?.id ?? null, action: Existing ? "update" : "create" };
    }
    if (Existing) {
        const Updated = await Stripeˉrequest(
            "POST",
            `/products/${encodeURIComponent(Existing.id)}`,
            Productˉparameters(Tier),
        );
        return { id: Updated.id, action: "updated" };
    }
    const Created = await Stripeˉrequest(
        "POST",
        "/products",
        Productˉparameters(Tier),
        `windvale-support-${SUPPORT_SCHEMA}-product-${Tier.key}`,
    );
    Existingˉproducts.push(Created);
    return { id: Created.id, action: "created" };
}

function Priceˉparameters(Tier, Productˉid) {
    const Entries = [
        ["currency", "usd"],
        ["product", Productˉid],
        ["lookup_key", `windvale_support_${Tier.key}_usd_${SUPPORT_SCHEMA}`],
        ["nickname", Tier.amount === null ? "Customer chooses amount in USD" : `$${Tier.amount / 100} USD one-time support`],
        ["metadata[windvale_support_tier]", Tier.key],
        ["metadata[windvale_support_schema]", SUPPORT_SCHEMA],
    ];
    if (Tier.amount === null) {
        Entries.push(
            ["custom_unit_amount[enabled]", "true"],
            ["custom_unit_amount[minimum]", "100"],
            ["custom_unit_amount[maximum]", "1000000"],
            ["custom_unit_amount[preset]", "5000"],
        );
    } else {
        Entries.push(["unit_amount", Tier.amount]);
    }
    return Makeˉparameters(Entries);
}

function Verifyˉprice(Price, Tier, Productˉid) {
    if (Price.currency !== "usd" || Price.product !== Productˉid || Price.recurring !== null) {
        throw new Error(`The existing ${Tier.key} price does not match the required one-time USD contract.`);
    }
    if (Tier.amount === null) {
        if (Price.custom_unit_amount === null) {
            throw new Error("The existing choose-your-support price does not allow a custom amount.");
        }
        if (Price.custom_unit_amount.minimum !== 100
            || Price.custom_unit_amount.maximum !== 1_000_000
            || Price.custom_unit_amount.preset !== 5_000) {
            throw new Error("The existing choose-your-support price has the wrong custom-amount limits.");
        }
    } else if (Price.unit_amount !== Tier.amount) {
        throw new Error(`The existing ${Tier.key} price has the wrong amount.`);
    }
}

async function Ensureˉprice(Tier, Productˉid) {
    const Lookupˉkey = `windvale_support_${Tier.key}_usd_${SUPPORT_SCHEMA}`;
    const Prices = await Stripeˉrequest("GET", "/prices", Makeˉparameters([
        ["active", "true"],
        ["limit", "100"],
        ["lookup_keys[0]", Lookupˉkey],
    ]));
    const Existing = Selectˉone(Prices.data, `Windvale ${Tier.key} price`);
    if (Existing) {
        Verifyˉprice(Existing, Tier, Productˉid);
        return { id: Existing.id, action: "existing" };
    }
    if (Isˉdryˉrun) {
        return { id: null, action: "create" };
    }
    const Created = await Stripeˉrequest(
        "POST",
        "/prices",
        Priceˉparameters(Tier, Productˉid),
        `windvale-support-${SUPPORT_SCHEMA}-price-${Tier.key}`,
    );
    Verifyˉprice(Created, Tier, Productˉid);
    return { id: Created.id, action: "created" };
}

function Paymentˉlinkˉparameters(Tier, Priceˉid) {
    return Makeˉparameters([
        ["line_items[0][price]", Priceˉid],
        ["line_items[0][quantity]", "1"],
        ["payment_method_types[0]", "card"],
        ["after_completion[type]", "redirect"],
        ["after_completion[redirect][url]", SUPPORT_RETURN_URL],
        ["name_collection[individual][enabled]", "true"],
        ["name_collection[individual][optional]", "false"],
        ["name_collection[business][enabled]", "true"],
        ["name_collection[business][optional]", "true"],
        ["custom_fields[0][key]", "public_recognition"],
        ["custom_fields[0][label][type]", "custom"],
        ["custom_fields[0][label][custom]", "Public supporter recognition"],
        ["custom_fields[0][type]", "dropdown"],
        ["custom_fields[0][dropdown][options][0][label]", "List my name and support tier publicly"],
        ["custom_fields[0][dropdown][options][0][value]", "public"],
        ["custom_fields[0][dropdown][options][1][label]", "Keep my support anonymous"],
        ["custom_fields[0][dropdown][options][1][value]", "anonymous"],
        ["custom_fields[0][optional]", "false"],
        ["custom_text[submit][message]", "Thank you for supporting Windvale's community development. Public recognition is always optional."],
        ["inactive_message", "Windvale support is temporarily unavailable. Please return to windvale.ca/support/."],
        ["metadata[windvale_support_tier]", Tier.key],
        ["metadata[windvale_support_schema]", SUPPORT_SCHEMA],
    ]);
}

function Verifyˉpaymentˉlink(Link, Tier, Priceˉid) {
    const Lineˉitem = Link.line_items?.data?.[0];
    const Recognitionˉfield = Link.custom_fields?.find((Field) => Field.key === "public_recognition");
    const Recognitionˉvalues = Recognitionˉfield?.dropdown?.options?.map((Option) => Option.value) ?? [];
    if (!Link.active || !Link.livemode || Link.currency !== "usd") {
        throw new Error(`The existing ${Tier.key} Payment Link is not an active live USD link.`);
    }
    if (Lineˉitem?.price?.id !== Priceˉid || Lineˉitem.quantity !== 1) {
        throw new Error(`The existing ${Tier.key} Payment Link has the wrong price or quantity.`);
    }
    if (!Link.payment_method_types?.includes("card")) {
        throw new Error(`The existing ${Tier.key} Payment Link does not support cards.`);
    }
    if (!Link.name_collection?.individual?.enabled || Link.name_collection.individual.optional
        || !Link.name_collection?.business?.enabled || !Link.name_collection.business.optional) {
        throw new Error(`The existing ${Tier.key} Payment Link has the wrong name-collection contract.`);
    }
    if (!Recognitionˉfield || Recognitionˉfield.type !== "dropdown" || Recognitionˉfield.optional
        || Recognitionˉvalues.length !== 2
        || !Recognitionˉvalues.includes("public") || !Recognitionˉvalues.includes("anonymous")) {
        throw new Error(`The existing ${Tier.key} Payment Link has the wrong public-recognition contract.`);
    }
    if (Link.after_completion?.type !== "redirect" || Link.after_completion.redirect?.url !== SUPPORT_RETURN_URL) {
        throw new Error(`The existing ${Tier.key} Payment Link has the wrong completion redirect.`);
    }
}

async function Ensureˉpaymentˉlink(Tier, Priceˉid, Existingˉlinks) {
    const Matches = Existingˉlinks.filter((Link) =>
        Link.metadata?.windvale_support_tier === Tier.key
        && Link.metadata?.windvale_support_schema === SUPPORT_SCHEMA);
    const Existing = Selectˉone(Matches, `Windvale ${Tier.key} Payment Link`);
    if (Isˉdryˉrun) {
        return { id: Existing?.id ?? null, url: Existing?.url ?? null, action: Existing ? "verify" : "create" };
    }
    if (Existing) {
        const Detail = await Stripeˉrequest(
            "GET",
            `/payment_links/${encodeURIComponent(Existing.id)}`,
            Makeˉparameters([["expand[0]", "line_items.data.price"]]),
        );
        Verifyˉpaymentˉlink(Detail, Tier, Priceˉid);
        return { id: Detail.id, url: Detail.url, action: "verified" };
    }
    const Created = await Stripeˉrequest(
        "POST",
        "/payment_links",
        Paymentˉlinkˉparameters(Tier, Priceˉid),
        `windvale-support-${SUPPORT_SCHEMA}-card-link-${Tier.key}`,
    );
    Existingˉlinks.push(Created);
    return { id: Created.id, url: Created.url, action: "created" };
}

const Account = await Stripeˉrequest("GET", "/account");
const Products = await Stripeˉrequest("GET", "/products", Makeˉparameters([
    ["active", "true"],
    ["limit", "100"],
]));
const Paymentˉlinks = await Stripeˉrequest("GET", "/payment_links", Makeˉparameters([
    ["active", "true"],
    ["limit", "100"],
]));

const Results = {};
for (const Tier of Supportˉtiers) {
    const Product = await Ensureˉproduct(Tier, Products.data);
    const Price = Product.id === null
        ? { id: null, action: "create" }
        : await Ensureˉprice(Tier, Product.id);
    Results[Tier.key] = {
        product: Product,
        price: Price,
        paymentLink: { id: null, url: null, action: "pending" },
    };
}

for (const Tier of Supportˉtiers) {
    const Priceˉid = Results[Tier.key].price.id;
    if (Priceˉid === null) {
        Results[Tier.key].paymentLink = { id: null, url: null, action: "create" };
        continue;
    }
    try {
        Results[Tier.key].paymentLink = await Ensureˉpaymentˉlink(Tier, Priceˉid, Paymentˉlinks.data);
    } catch (Errorˉvalue) {
        Results[Tier.key].paymentLink = {
            id: null,
            url: null,
            action: "blocked",
            reason: Errorˉvalue instanceof Error ? Errorˉvalue.message : "Unknown Stripe error.",
        };
    }
}

process.stdout.write(`${JSON.stringify({
    mode: Apiˉkey.includes("_live_") ? "live" : "test",
    dryRun: Isˉdryˉrun,
    account: {
        businessName: Account.business_profile?.name ?? Account.company?.name ?? null,
        chargesEnabled: Account.charges_enabled,
        payoutsEnabled: Account.payouts_enabled,
    },
    tiers: Results,
}, null, 2)}\n`);
