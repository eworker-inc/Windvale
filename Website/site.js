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

Setˉtheme(Readˉinitialˉtheme());

document.querySelector("#theme-toggle")?.addEventListener("click", Toggleˉtheme);
