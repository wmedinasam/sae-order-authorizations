// Comportamientos globales sin interop: menú móvil, trampa de foco en modales, contraseña visible, envío en curso.
(() => {
    const app = () => document.querySelector(".wis-app");
    // El panel vuelve arriba al cerrarse: si conserva el scroll, la siguiente vez se ve desde la mitad de la lista.
    const cerrarMenu = () => {
        const a = app();
        if (!a) return;
        a.removeAttribute("data-nav-open");
        document.querySelector(".wis-menu-btn")?.setAttribute("aria-expanded", "false");
        const panel = document.querySelector(".wis-nav");
        if (panel) panel.scrollTop = 0;
    };

    document.addEventListener("click", (e) => {
        const t = e.target;
        if (!(t instanceof Element)) return;

        const menu = t.closest(".wis-menu-btn");
        if (menu) {
            const a = app();
            const abierto = a?.getAttribute("data-nav-open") === "true";
            if (abierto) cerrarMenu();
            else { a?.setAttribute("data-nav-open", "true"); menu.setAttribute("aria-expanded", "true"); }
            return;
        }
        if (t.closest(".wis-scrim") || t.closest(".wis-nav-link")) cerrarMenu();

        const toggle = t.closest("[data-toggle-password]");
        if (toggle) {
            const input = document.getElementById(toggle.getAttribute("data-toggle-password"));
            if (!input) return;
            const mostrar = input.type === "password";
            input.type = mostrar ? "text" : "password";
            toggle.setAttribute("aria-pressed", String(mostrar));
            toggle.setAttribute("aria-label", mostrar ? "Ocultar contraseña" : "Mostrar contraseña");
        }
    });

    document.addEventListener("keydown", (e) => {
        if (e.key === "Escape") cerrarMenu();

        if (e.key !== "Tab") return;
        const modal = document.querySelector(".wis-modal");
        if (!modal) return;
        const foco = [...modal.querySelectorAll("a[href], button:not([disabled]), input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex='-1'])")]
            .filter((el) => el.offsetParent !== null);
        if (foco.length === 0) { e.preventDefault(); return; }
        const primero = foco[0], ultimo = foco[foco.length - 1];
        if (!modal.contains(document.activeElement)) { e.preventDefault(); primero.focus(); }
        else if (e.shiftKey && (document.activeElement === primero || document.activeElement === modal)) { e.preventDefault(); ultimo.focus(); }
        else if (!e.shiftKey && document.activeElement === ultimo) { e.preventDefault(); primero.focus(); }
    });

    // Marca el botón de envío como ocupado sin quitarlo del formulario (SSR estático, p. ej. login).
    document.addEventListener("submit", (e) => {
        const f = e.target;
        if (!(f instanceof HTMLFormElement) || !f.hasAttribute("data-busy-on-submit")) return;
        const b = f.querySelector("button[type=submit]");
        if (b) { b.setAttribute("aria-busy", "true"); b.setAttribute("aria-disabled", "true"); }
    });
})();
