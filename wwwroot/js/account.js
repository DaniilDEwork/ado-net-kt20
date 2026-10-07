const authForm = document.getElementById("auth-form");
const authErrors = document.getElementById("auth-errors");
const authSubmit = document.getElementById("auth-submit");

function showAuthErrors(messages) {
    authErrors.replaceChildren();
    authErrors.hidden = messages.length === 0;
    if (!messages.length) return;
    const list = document.createElement("ul");
    for (const message of messages) {
        const item = document.createElement("li");
        item.textContent = message;
        list.append(item);
    }
    authErrors.append(list);
}

async function checkExistingLogin() {
    if (!Auth.getSession()) return;
    try {
        const response = await Auth.request("/api/auth/me");
        if (response.ok) location.replace("/Chat");
        else Auth.clear();
    } catch {
        showAuthErrors(["Не удалось проверить вход, проверьте подключение"]);
    }
}

authForm.addEventListener("submit", async event => {
    event.preventDefault();
    if (authSubmit.disabled) return;
    authSubmit.disabled = true;
    showAuthErrors([]);
    const data = Object.fromEntries(new FormData(authForm));
    try {
        const response = await fetch(authForm.dataset.endpoint, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        });
        const result = await response.json();
        if (!response.ok) {
            const errors = result.errors ? Object.values(result.errors).flat() : [result.message || "Не удалось выполнить запрос"];
            showAuthErrors(errors);
            return;
        }
        Auth.save(result);
        sessionStorage.setItem("kt20.notice", authForm.dataset.endpoint.endsWith("register")
            ? "Регистрация завершена, JWT токен получен" : "Вход выполнен, JWT токен получен");
        location.assign(authForm.dataset.returnUrl || "/Chat");
    } catch {
        showAuthErrors(["Не удалось подключиться к серверу, попробуйте ещё раз"]);
    } finally {
        authSubmit.disabled = false;
    }
});
checkExistingLogin();
