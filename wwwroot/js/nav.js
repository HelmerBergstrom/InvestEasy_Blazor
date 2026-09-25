// Opens and closes the main menu on small screens.
// The header is static server rendered, so this is done with a small script instead of Blazor events.
// Event delegation is used because Blazor's enhanced navigation replaces the header markup,
// which also resets the menu to closed after each navigation.
document.addEventListener("click", (event) => {
    const toggle = event.target.closest("[data-nav-toggle]");
    if (!toggle) {
        return;
    }

    const header = toggle.closest("[data-nav]");
    const isOpen = header.toggleAttribute("data-open");
    toggle.setAttribute("aria-expanded", isOpen ? "true" : "false");
});

// Closes the menu with the Escape key and moves focus back to the button.
document.addEventListener("keydown", (event) => {
    if (event.key !== "Escape") {
        return;
    }

    const header = document.querySelector("[data-nav][data-open]");
    if (!header) {
        return;
    }

    header.removeAttribute("data-open");
    const toggle = header.querySelector("[data-nav-toggle]");
    toggle.setAttribute("aria-expanded", "false");
    toggle.focus();
});
