(() => {
    const STORAGE_KEY = "windvale-theme";
    let Savedˉtheme;
    try {
        Savedˉtheme = localStorage.getItem(STORAGE_KEY);
    } catch {
        // Storage can be unavailable in privacy-focused browser modes.
    }
    const Theme = Savedˉtheme === "light" || Savedˉtheme === "dark"
        ? Savedˉtheme
        : matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";

    document.documentElement.dataset.theme = Theme;
    document.documentElement.style.colorScheme = Theme;
})();
