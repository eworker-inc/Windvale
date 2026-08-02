import { SUPPORT_CURRENCY, SUPPORT_TIERS } from "../support-data.js";

const TIER_BY_KEY = new Map(SUPPORT_TIERS.map((Tier) => [Tier.key, Tier]));
const RECOGNITION_ORDER = Object.freeze([
    "cornerstone",
    "champion",
    "accelerator",
    "builder",
    "spark",
    "any",
]);

function Makeˉelement(Tagˉname, Classˉname, Text) {
    const Element = document.createElement(Tagˉname);
    if (Classˉname) {
        Element.className = Classˉname;
    }
    if (Text !== undefined) {
        Element.textContent = Text;
    }
    return Element;
}

function Isˉstripeˉpaymentˉlink(Value) {
    try {
        const Url = new URL(Value);
        return Url.protocol === "https:" && Url.hostname === "buy.stripe.com";
    } catch {
        return false;
    }
}

function Buildˉtierˉcard(Tier) {
    const Card = Makeˉelement("article", `support-tier accent-${Tier.accent}`);
    const Visual = Makeˉelement("div", "support-tier-visual");
    const Image = Makeˉelement("img", "support-tier-image");
    Image.src = Tier.image;
    Image.alt = "";
    Image.width = 768;
    Image.height = 512;
    Image.loading = "lazy";
    Image.decoding = "async";
    Visual.append(Image, Makeˉelement("span", "support-tier-note", Tier.note));

    const Amount = Makeˉelement("p", "support-tier-amount", Tier.amount);
    Amount.append(Makeˉelement("small", "", ` ${SUPPORT_CURRENCY}`));

    Card.append(
        Visual,
        Amount,
        Makeˉelement("h3", "", Tier.name),
        Makeˉelement("p", "support-tier-description", Tier.description),
    );

    if (Isˉstripeˉpaymentˉlink(Tier.checkoutUrl)) {
        const Actionˉlabel = Tier.key === "any" ? "Choose an amount" : `Choose ${Tier.name}`;
        const Link = Makeˉelement("a", "support-tier-action", Actionˉlabel);
        Link.href = Tier.checkoutUrl;
        Link.setAttribute(
            "aria-label",
            Tier.key === "any" ? "Choose an amount through Stripe" : `Choose ${Tier.name} support through Stripe`,
        );
        Link.append(Makeˉelement("span", "material-symbol", "arrow_forward"));
        Card.append(Link);
    } else {
        const Pending = Makeˉelement("button", "support-tier-action pending", "Stripe link coming soon");
        Pending.type = "button";
        Pending.disabled = true;
        Card.append(Pending);
    }

    return Card;
}

function Renderˉtiers() {
    const Container = document.querySelector("#support-tiers");
    if (Container) {
        Container.replaceChildren(...SUPPORT_TIERS.map(Buildˉtierˉcard));
    }
}

function Formatˉmonth(Value) {
    const Match = /^(\d{4})-(\d{2})$/u.exec(Value);
    if (!Match) {
        return Value;
    }
    const Dateˉvalue = new Date(Date.UTC(Number(Match[1]), Number(Match[2]) - 1, 1));
    return new Intl.DateTimeFormat(undefined, { month: "short", year: "numeric", timeZone: "UTC" }).format(Dateˉvalue);
}

function Renderˉsupporterˉgroup(Tierˉkey, Supporters) {
    const Tier = TIER_BY_KEY.get(Tierˉkey);
    const Group = Makeˉelement("section", `supporter-group accent-${Tier?.accent ?? "blue"}`);
    const Heading = Makeˉelement("div", "supporter-group-heading");
    const Icon = Makeˉelement("span", "supporter-group-icon material-symbol", Tier?.icon ?? "favorite");
    Icon.setAttribute("aria-hidden", "true");
    Heading.append(
        Icon,
        Makeˉelement("h3", "", Tier?.name ?? "Supporters"),
        Makeˉelement("span", "supporter-count", String(Supporters.length)),
    );

    const Names = Makeˉelement("ul", "supporter-names");
    for (const Supporter of Supporters) {
        const Item = Makeˉelement("li", "");
        Item.append(
            Makeˉelement("strong", "", Supporter.displayName),
            Makeˉelement("small", "", `Since ${Formatˉmonth(Supporter.since)}`),
        );
        Names.append(Item);
    }
    Group.append(Heading, Names);
    return Group;
}

function Renderˉsupporters(Roll) {
    const Container = document.querySelector("#supporter-roll");
    const Summary = document.querySelector("#supporter-summary");
    if (!Container || !Summary) {
        return;
    }

    const Supporters = Array.isArray(Roll.supporters) ? Roll.supporters : [];
    const Anonymousˉcounts = Roll.anonymousCounts && typeof Roll.anonymousCounts === "object"
        ? Roll.anonymousCounts
        : {};
    const Anonymousˉtotal = RECOGNITION_ORDER.reduce((Total, Tier) => Total + (Number(Anonymousˉcounts[Tier]) || 0), 0);
    const Total = Supporters.length + Anonymousˉtotal;
    Summary.textContent = Total === 0
        ? "The public supporter roll begins with the first opt-in supporter."
        : `${Total} supporter${Total === 1 ? "" : "s"} helping Windvale move forward, including ${Anonymousˉtotal} anonymous.`;

    const Groups = RECOGNITION_ORDER
        .map((Tier) => [Tier, Supporters.filter((Supporter) => Supporter.tier === Tier)])
        .filter(([, Members]) => Members.length > 0)
        .map(([Tier, Members]) => Renderˉsupporterˉgroup(Tier, Members));

    if (Groups.length === 0) {
        const Empty = Makeˉelement("div", "supporter-empty");
        const Icon = Makeˉelement("span", "material-symbol", "diversity_1");
        Icon.setAttribute("aria-hidden", "true");
        Empty.append(
            Icon,
            Makeˉelement("strong", "", "Your name could be part of Windvale's story."),
            Makeˉelement("p", "", "Public recognition is always optional. Anonymous support is equally appreciated."),
        );
        Container.replaceChildren(Empty);
        return;
    }

    if (Anonymousˉtotal > 0) {
        const Anonymous = Makeˉelement("div", "anonymous-supporters");
        const Icon = Makeˉelement("span", "material-symbol", "visibility_off");
        Icon.setAttribute("aria-hidden", "true");
        Anonymous.append(
            Icon,
            Makeˉelement("strong", "", `${Anonymousˉtotal} anonymous supporter${Anonymousˉtotal === 1 ? "" : "s"}`),
        );
        Groups.push(Anonymous);
    }
    Container.replaceChildren(...Groups);
}

async function Loadˉsupporters() {
    const Container = document.querySelector("#supporter-roll");
    try {
        const Response = await fetch("/api/supporters", {
            headers: { Accept: "application/json" },
        });
        if (!Response.ok) {
            throw new Error(`Supporter endpoint returned ${Response.status}.`);
        }
        Renderˉsupporters(await Response.json());
    } catch {
        if (Container) {
            Container.replaceChildren(Makeˉelement(
                "p",
                "supporter-unavailable",
                "The supporter roll is temporarily unavailable. Please try again later.",
            ));
        }
    }
}

Renderˉtiers();
Loadˉsupporters();
