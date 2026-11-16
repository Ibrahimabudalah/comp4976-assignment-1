// Dark mode + print helpers. All localStorage access is wrapped because it can throw (private mode, blocked storage).
window.memsTheme = {
    current() { return document.documentElement.dataset.bsTheme || 'light'; },
    set(theme) {
        document.documentElement.dataset.bsTheme = theme;
        try { localStorage.setItem('theme', theme); } catch { }
    },
    toggle() {
        const next = this.current() === 'dark' ? 'light' : 'dark';
        this.set(next);
        return next;
    },
    // Paper is always light: switch, print, then restore the voter's theme.
    print() {
        const previous = this.current();
        document.documentElement.dataset.bsTheme = 'light';
        window.addEventListener('afterprint', () => { document.documentElement.dataset.bsTheme = previous; }, { once: true });
        window.print();
    }
};
