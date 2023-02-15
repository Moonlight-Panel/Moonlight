if (document.documentElement) {
    const defaultThemeMode = "dark";
    const name = document.body.getAttribute("data-kt-name");
    let themeMode = localStorage.getItem("kt_" + (name !== null ? name + "_" : "") + "theme_mode_value");

    if (themeMode === null) {
        if (defaultThemeMode === "system") {
            themeMode = window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
        } else {
            themeMode = defaultThemeMode;
        }
    }
    document.documentElement.setAttribute("data-theme", themeMode);
}