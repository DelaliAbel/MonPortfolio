// Comportements légers de la vitrine (rendue côté serveur). Aucun framework, aucun script inline (CSP).
// Les écouteurs sont délégués au document : ils survivent aux mises à jour DOM de la navigation
// améliorée de Blazor ; les observateurs, eux, sont ré-initialisés après chaque « enhancedload ».

const reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)");
let revealObserver;
let spyObserver;

/* ---------- Thème (cookie lu par le serveur pour un rendu sans flash) ---------- */
function setTheme(theme) {
    document.documentElement.dataset.theme = theme;
    document.cookie = `theme=${theme}; path=/; max-age=31536000; samesite=lax`;
}

/* ---------- Menu mobile ---------- */
function toggleNav(force) {
    const header = document.querySelector("[data-site-header]");
    if (!header) return;
    const open = force ?? !header.classList.contains("is-open");
    header.classList.toggle("is-open", open);
    header.querySelector("[data-nav-toggle]")?.setAttribute("aria-expanded", String(open));
}

/* ---------- Filtres de projets ---------- */
function applyFilter(button) {
    const value = button.dataset.filter;
    button.closest("[data-project-filters]")?.querySelectorAll("[data-filter]").forEach((b) => {
        const active = b === button;
        b.classList.toggle("is-active", active);
        b.setAttribute("aria-pressed", String(active));
    });
    document.querySelectorAll(".project-card[data-category]").forEach((card) => {
        card.classList.toggle("is-hidden", value !== "*" && card.dataset.category !== value);
    });
}

document.addEventListener("click", (event) => {
    const target = event.target;
    if (!(target instanceof Element)) return;

    if (target.closest("[data-theme-toggle]")) {
        setTheme(document.documentElement.dataset.theme === "light" ? "dark" : "light");
        return;
    }
    if (target.closest("[data-nav-toggle]")) {
        toggleNav();
        return;
    }
    if (target.closest(".nav__links a")) {
        toggleNav(false);
    }
    const filter = target.closest("[data-filter]");
    if (filter) {
        applyFilter(filter);
    }
});

document.addEventListener("keydown", (event) => {
    if (event.key === "Escape") toggleNav(false);
});

/* ---------- Halo doré qui suit le pointeur sur les cartes ---------- */
document.addEventListener("pointermove", (event) => {
    const card = event.target instanceof Element ? event.target.closest(".glow-card") : null;
    if (!card) return;
    const rect = card.getBoundingClientRect();
    card.style.setProperty("--mx", `${event.clientX - rect.left}px`);
    card.style.setProperty("--my", `${event.clientY - rect.top}px`);
}, { passive: true });

/* ---------- En-tête translucide au défilement ---------- */
function updateHeader() {
    document.querySelector("[data-site-header]")?.classList.toggle("is-scrolled", window.scrollY > 8);
}
window.addEventListener("scroll", updateHeader, { passive: true });

/* ---------- Apparition en fondu au défilement ---------- */
function setupReveal() {
    revealObserver?.disconnect();
    if (reducedMotion.matches || !("IntersectionObserver" in window)) return;

    revealObserver = new IntersectionObserver((entries) => {
        for (const entry of entries) {
            if (!entry.isIntersecting) continue;
            entry.target.classList.add("is-visible");
            revealObserver.unobserve(entry.target);
        }
    }, { rootMargin: "0px 0px -8% 0px", threshold: 0.08 });

    // On n'arme que ce qui est encore sous la ligne de flottaison : rien ne « clignote » à l'écran.
    // Le hero a sa propre animation d'entrée, en CSS pur.
    document.querySelectorAll(".reveal").forEach((element) => {
        if (element.closest(".hero") || element.classList.contains("is-visible")) return;
        if (element.getBoundingClientRect().top < window.innerHeight) return;

        const index = Array.prototype.indexOf.call(element.parentElement?.children ?? [], element);
        element.style.setProperty("--reveal-delay", `${(Math.max(index, 0) % 4) * 90}ms`);
        element.classList.add("reveal-armed");
        revealObserver.observe(element);
    });
}

/* ---------- Lien de navigation actif selon la section visible ---------- */
function setupScrollSpy() {
    spyObserver?.disconnect();
    const links = new Map([...document.querySelectorAll("[data-nav-link]")].map((a) => [a.dataset.navLink, a]));
    if (links.size === 0 || !("IntersectionObserver" in window)) return;

    spyObserver = new IntersectionObserver((entries) => {
        for (const entry of entries) {
            if (!entry.isIntersecting) continue;
            links.forEach((link, id) => link.classList.toggle("is-active", id === entry.target.id));
        }
    }, { rootMargin: "-45% 0px -50% 0px" });

    ["top", ...links.keys()].forEach((id) => {
        const section = document.getElementById(id);
        if (section) spyObserver.observe(section);
    });
}

function init() {
    toggleNav(false);
    updateHeader();
    setupReveal();
    setupScrollSpy();
}

init();
window.Blazor?.addEventListener?.("enhancedload", init);
