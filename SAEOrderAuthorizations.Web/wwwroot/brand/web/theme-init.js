/* Load synchronously in <head>, before brand-tokens.css.
 * Stores only the user's display preference. Safe if localStorage is blocked.
 * Explicit light/dark in HTML takes priority over stored preferences.
 * Use data-theme="auto" (or omit it) to restore the visitor's saved choice.
 */
(function () {
  "use strict";
  var root = document.documentElement;
  var allowed = ["light", "dark", "auto"];
  var initial = root.getAttribute("data-theme");
  var selected = allowed.indexOf(initial) !== -1 ? initial : "auto";
  try {
    var stored = localStorage.getItem("wis-theme");
    if (initial !== "light" && initial !== "dark" && allowed.indexOf(stored) !== -1) selected = stored;
  } catch (_) {
    /* The CSS still supplies the system preference without persistence. */
  }
  root.setAttribute("data-theme", selected);
  var scheme = window.matchMedia("(prefers-color-scheme: dark)");

  function updateMetadata() {
    var preference = root.getAttribute("data-theme");
    var dark = preference === "dark" || (preference === "auto" && scheme.matches);
    var meta = document.querySelector('meta[name="theme-color"][data-wis-theme-color]');
    if (meta) meta.setAttribute("content", dark ? "#101E2A" : "#F6F8FB");
  }

  window.WISBrandTheme = {
    get: function () { return root.getAttribute("data-theme") || "auto"; },
    set: function (value) {
      if (allowed.indexOf(value) === -1) return;
      root.setAttribute("data-theme", value);
      try { localStorage.setItem("wis-theme", value); } catch (_) {}
      updateMetadata();
      window.dispatchEvent(new CustomEvent("wis:themechange", { detail: { theme: value } }));
    }
  };

  if (scheme.addEventListener) scheme.addEventListener("change", updateMetadata);
  else if (scheme.addListener) scheme.addListener(updateMetadata);
  if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", updateMetadata, { once: true });
  else updateMetadata();
})();
