import { useEffect, useState } from "react";

type Theme = "light" | "dark";
type Props = { className?: string };

function getSavedTheme(): Theme | undefined {
    try {
        const savedTheme = window.localStorage?.getItem("bingo-company-theme");
        return savedTheme === "light" || savedTheme === "dark" ? savedTheme : undefined;
    } catch {
        return undefined;
    }
}

export function ThemeToggle({ className = "" }: Props) {
    const [theme, setTheme] = useState<Theme>(
        () => getSavedTheme() ?? (window.matchMedia?.("(prefers-color-scheme: dark)").matches ? "dark" : "light")
    );

    useEffect(() => {
        document.documentElement.dataset.theme = theme;
        try {
            window.localStorage?.setItem("bingo-company-theme", theme);
        } catch {
            return;
        }
    }, [theme]);

    const nextTheme = theme === "dark" ? "light" : "dark";
    return (
        <button
            className={`theme-toggle ${className}`.trim()}
            type="button"
            onClick={() => setTheme(nextTheme)}
            aria-label={`Ativar tema ${nextTheme === "dark" ? "escuro" : "claro"}`}
            title={`Ativar tema ${nextTheme === "dark" ? "escuro" : "claro"}`}
        >
            <span aria-hidden="true">{theme === "dark" ? "☀" : "☾"}</span>
        </button>
    );
}
