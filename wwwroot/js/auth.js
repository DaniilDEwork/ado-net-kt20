window.Auth = (() => {
    const key = "kt20.auth";

    function clear() {
        sessionStorage.removeItem(key);
        document.getElementById("register-link").hidden = false;
        document.getElementById("login-link").hidden = false;
        document.getElementById("logout-button").hidden = true;
        document.getElementById("account-name").hidden = true;
    }

    function getSession() {
        try {
            const session = JSON.parse(sessionStorage.getItem(key));
            if (!session || typeof session.accessToken !== "string" || !session.accessToken
                || !Number.isFinite(Date.parse(session.expiresAt)) || Date.parse(session.expiresAt) <= Date.now()) {
                clear();
                return null;
            }
            return session;
        } catch {
            clear();
            return null;
        }
    }

    function save(session) {
        sessionStorage.setItem(key, JSON.stringify(session));
    }

    function request(url, options = {}) {
        const headers = new Headers(options.headers);
        const session = getSession();
        if (session) headers.set("Authorization", "Bearer " + session.accessToken);
        return fetch(url, { ...options, headers, cache: "no-store" });
    }

    function requireLogin(reason = "") {
        clear();
        const query = new URLSearchParams({ returnUrl: "/Chat" });
        if (reason) query.set("reason", reason);
        location.replace("/Account/Login?" + query);
    }

    const session = getSession();
    document.getElementById("register-link").hidden = !!session;
    document.getElementById("login-link").hidden = !!session;
    const logoutButton = document.getElementById("logout-button");
    logoutButton.hidden = !session;
    const accountName = document.getElementById("account-name");
    accountName.hidden = !session;
    if (session) accountName.textContent = "Пользователь: " + session.userName;

    logoutButton.addEventListener("click", async () => {
        logoutButton.disabled = true;
        document.dispatchEvent(new Event("auth:logout"));
        try {
            await request("/api/auth/logout", { method: "POST" });
        } catch {
        } finally {
            clear();
            location.assign("/?signedOut=1");
        }
    });

    return { getSession, save, clear, request, requireLogin };
})();
