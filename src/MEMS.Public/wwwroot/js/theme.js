// Dark mode + print helpers. All localStorage access is wrapped because it can throw (private mode, blocked storage).
(function () {
    const root = document.documentElement;
    let themeBeforePrint = null;

    // Runs for every print path (our button, Ctrl+P, the browser menu): paper is always light, and the
    // print date comes from the voter's own clock. afterprint puts the voter's theme back.
    window.addEventListener('beforeprint', () => {
        themeBeforePrint = root.dataset.bsTheme || 'light';
        root.dataset.bsTheme = 'light';
        const today = new Date().toLocaleDateString('en-CA', { year: 'numeric', month: 'long', day: 'numeric' });
        document.querySelectorAll('[data-print-date]').forEach(e => { e.textContent = today; });
    });
    window.addEventListener('afterprint', () => {
        if (themeBeforePrint) { root.dataset.bsTheme = themeBeforePrint; themeBeforePrint = null; }
    });

    window.memsTheme = {
        current() { return root.dataset.bsTheme || 'light'; },
        set(theme) {
            root.dataset.bsTheme = theme;
            try { localStorage.setItem('theme', theme); } catch { }
        },
        toggle() {
            const next = this.current() === 'dark' ? 'light' : 'dark';
            this.set(next);
            return next;
        },
        print() { window.print(); }
    };
})();
