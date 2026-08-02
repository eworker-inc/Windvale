import { DEVELOPMENT_PROGRESS, PROGRESS_UPDATED, PROJECT_PROGRESS } from "./project-progress.js";

const THEME_KEY = "windvale-theme";

function Setˉtheme(theme) {
    document.documentElement.dataset.theme = theme;

    const button = document.querySelector("#theme-toggle");
    if (!button) {
        return;
    }

    const isˉdark = theme === "dark";
    button.setAttribute("aria-label", `Switch to ${isˉdark ? "light" : "dark"} theme`);
    button.setAttribute("aria-pressed", String(isˉdark));
    button.querySelector("span").textContent = isˉdark ? "light_mode" : "dark_mode";
}

function Readˉinitialˉtheme() {
    try {
        const savedˉtheme = window.localStorage.getItem(THEME_KEY);
        if (savedˉtheme === "light" || savedˉtheme === "dark") {
            return savedˉtheme;
        }
    } catch {
        // Storage can be unavailable in privacy-focused browser modes.
    }

    return window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
}

function Toggleˉtheme() {
    const nextˉtheme = document.documentElement.dataset.theme === "dark" ? "light" : "dark";
    Setˉtheme(nextˉtheme);

    try {
        window.localStorage.setItem(THEME_KEY, nextˉtheme);
    } catch {
        // The selected theme still applies for the current page.
    }
}

function Makeˉelement(tagˉname, classˉname, text) {
    const element = document.createElement(tagˉname);
    if (classˉname) {
        element.className = classˉname;
    }
    if (text !== undefined) {
        element.textContent = text;
    }
    return element;
}

function Buildˉprogressˉcard(item, index, items) {
    const card = Makeˉelement("button", `progress-card accent-${item.accent}`);
    const tooltipˉid = `progress-tooltip-${item.key}`;
    card.type = "button";
    card.style.setProperty("--progress", `${item.percent}%`);
    card.setAttribute("aria-describedby", tooltipˉid);
    card.setAttribute("aria-expanded", "false");

    const top = Makeˉelement("span", "progress-card-top");
    const icon = Makeˉelement("span", "progress-icon material-symbol", item.icon);
    icon.setAttribute("aria-hidden", "true");
    const percent = Makeˉelement("strong", "progress-percent", `${item.percent}%`);
    top.append(icon, percent);

    const name = Makeˉelement("span", "progress-name", item.name);
    const status = Makeˉelement("span", "progress-status", item.status);
    const track = Makeˉelement("span", "progress-track");
    const fill = Makeˉelement("span", "progress-fill");
    track.setAttribute("aria-hidden", "true");
    track.append(fill);

    const hint = Makeˉelement("span", "progress-hint", "Details");
    const tooltip = Makeˉelement("span", "progress-tooltip");
    tooltip.id = tooltipˉid;
    tooltip.setAttribute("role", "tooltip");
    tooltip.append(
        Makeˉelement("strong", "", item.status),
        Makeˉelement("span", "", item.details),
        Makeˉelement("small", "", `Next: ${item.next}`),
    );

    if (index === 0) {
        tooltip.classList.add("align-left");
    } else if (index === items.length - 1) {
        tooltip.classList.add("align-right");
    }

    card.append(top, name, status, track, hint, tooltip);
    card.addEventListener("click", () => {
        const willˉopen = !card.classList.contains("tooltip-open");
        document.querySelectorAll(".progress-card.tooltip-open").forEach((openˉcard) => {
            openˉcard.classList.remove("tooltip-open");
            openˉcard.setAttribute("aria-expanded", "false");
        });
        card.classList.toggle("tooltip-open", willˉopen);
        card.setAttribute("aria-expanded", String(willˉopen));
    });

    return card;
}

function Renderˉprogress() {
    const container = document.querySelector("#progress-cards");
    const milestoneˉcontainer = document.querySelector("#milestone-cards");
    const updated = document.querySelector("#progress-updated");
    if (!container || !milestoneˉcontainer || !updated) {
        return;
    }

    updated.textContent = PROGRESS_UPDATED;
    container.replaceChildren(...PROJECT_PROGRESS.map(Buildˉprogressˉcard));
    milestoneˉcontainer.replaceChildren(...DEVELOPMENT_PROGRESS.map((item, index, items) => {
        const card = Buildˉprogressˉcard(item, index, items);
        card.classList.add("milestone-card");
        return card;
    }));
}

Setˉtheme(Readˉinitialˉtheme());
Renderˉprogress();

document.querySelector("#theme-toggle")?.addEventListener("click", Toggleˉtheme);
document.addEventListener("click", (event) => {
    if (event.target instanceof Element && event.target.closest(".progress-card")) {
        return;
    }

    document.querySelectorAll(".progress-card.tooltip-open").forEach((card) => {
        card.classList.remove("tooltip-open");
        card.setAttribute("aria-expanded", "false");
    });
});
