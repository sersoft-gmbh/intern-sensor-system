(() => {
    'use strict'

    const setTheme = (theme) => document.documentElement.setAttribute('data-bs-theme', theme);
    const getPreferredTheme = () => window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', () => setTheme(getPreferredTheme()));
    setTheme(getPreferredTheme());
})();
