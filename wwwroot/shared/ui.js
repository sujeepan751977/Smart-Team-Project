window.SR = window.SR || {};

SR.ui = (function () {
  const esc = (...a) => SR.utils.escape(...a);

  function toast(message, type = "ok") {
    let root = document.getElementById("toast-root");
    if (!root) {
      root = document.createElement("div");
      root.id = "toast-root";
      root.className = "toast-root";
      document.body.appendChild(root);
    }
    const el = document.createElement("div");
    el.className = `toast ${type === "error" ? "error" : "ok"}`;
    el.textContent = message;
    root.appendChild(el);
    setTimeout(() => el.remove(), 3400);
  }

  function badge(text, kind = "") {
    return `<span class="badge ${kind}">${esc(text)}</span>`;
  }

  function empty(text) {
    return `<div class="empty">${esc(text)}</div>`;
  }

  function loader(dark = false) {
    return `<span class="loader ${dark ? "dark" : ""}" aria-hidden="true"></span>`;
  }

  function setLoading(btn, loading, label = "Please wait…") {
    if (!btn) return;
    if (loading) {
      btn.dataset.prev = btn.innerHTML;
      btn.disabled = true;
      btn.innerHTML = `${loader()} ${esc(label)}`;
    } else {
      btn.disabled = false;
      btn.innerHTML = btn.dataset.prev || btn.innerHTML;
    }
  }

  function modal({ title, bodyHtml, confirmText = "Confirm", danger = false, onConfirm }) {
    const backdrop = document.createElement("div");
    backdrop.className = "modal-backdrop";
    backdrop.innerHTML = `
      <div class="modal" role="dialog" aria-modal="true">
        <h2>${esc(title)}</h2>
        <div class="modal-body">${bodyHtml || ""}</div>
        <div class="row-actions">
          <button type="button" class="btn btn-outline" data-cancel>Cancel</button>
          <button type="button" class="btn ${danger ? "btn-danger" : "btn-primary"}" data-ok>${esc(confirmText)}</button>
        </div>
      </div>`;
    document.body.appendChild(backdrop);
    const close = () => backdrop.remove();
    backdrop.querySelector("[data-cancel]").onclick = close;
    backdrop.addEventListener("click", (e) => {
      if (e.target === backdrop) close();
    });
    backdrop.querySelector("[data-ok]").onclick = async () => {
      try {
        if (onConfirm) await onConfirm(backdrop);
        close();
      } catch (err) {
        if (err?.message) SR.ui.toast(err.message, "error");
      }
    };
    return backdrop;
  }

  function confirmAction({ title, message, noteRequired = false, confirmText = "Confirm", danger = true, onConfirm }) {
    const body = `
      <p class="muted" style="margin-bottom:0.85rem">${esc(message)}</p>
      ${
        noteRequired
          ? `<label class="field">Decision note <span class="req">*</span>
              <textarea id="confirm-note" required maxlength="1000"></textarea>
              <span class="field-error" id="confirm-note-err" hidden>Decision note is required.</span>
            </label>`
          : ""
      }`;
    return modal({
      title,
      bodyHtml: body,
      confirmText,
      danger,
      onConfirm: async (backdrop) => {
        let note = "";
        if (noteRequired) {
          note = backdrop.querySelector("#confirm-note").value.trim();
          if (!note) {
            backdrop.querySelector("#confirm-note-err").hidden = false;
            throw new Error("Decision note required");
          }
        }
        await onConfirm(note, backdrop);
      },
    });
  }

  function publicHeader(active = "") {
    return `
      <header class="public-header">
        <a class="logo" href="/index.html"><span class="logo-mark">SR</span> SMART RECRUIT</a>
        <nav class="nav-links desktop-only">
          <a href="/module2-jobseeker/jobs.html" class="${active === "jobs" ? "active" : ""}">Find Jobs</a>
          <a href="/index.html#how" class="${active === "how" ? "active" : ""}">How It Works</a>
          <a href="/index.html#employers" class="${active === "employers" ? "active" : ""}">For Employers</a>
        </nav>
        <div class="nav-actions">
          <button class="menu-btn" type="button" id="public-menu-btn" aria-label="Menu">Menu</button>
          <a class="btn btn-ghost-light btn-sm" href="/login.html">Login</a>
          <a class="btn btn-primary btn-sm" href="/register.html">Create Account</a>
        </div>
      </header>`;
  }

  function mountPublicHeader(active) {
    const slot = document.getElementById("site-header");
    if (!slot) return;
    slot.outerHTML = publicHeader(active);
  }

  function mountPublicShell(active = "jobs") {
    const shell = document.getElementById("app-shell");
    if (!shell) return;
    shell.className = "public-shell";
    shell.innerHTML = `
      ${publicHeader(active)}
      <main class="public-main"><div class="container page" id="page-root"></div></main>`;
  }

  const menus = {
    JobSeeker: [
      { href: "/module2-jobseeker/dashboard.html", label: "Dashboard" },
      { href: "/module2-jobseeker/jobs.html", label: "Find Jobs" },
      { href: "/module2-jobseeker/profile.html", label: "Profile" },
      { href: "/module4-applications/jobseeker-applications.html", label: "My Applications" },
      { href: "/module4-applications/jobseeker-contact-requests.html", label: "Contact Requests" },
      { href: "/module4-applications/jobseeker-interviews.html", label: "Interviews" },
      { href: "/module5-trust/job-reports.html", label: "Job Reports" },
      { href: "/module5-trust/notifications.html", label: "Notifications" },
    ],
    Employer: [
      { href: "/module3-employer/employer-dashboard.html", label: "Dashboard" },
      { href: "/module3-employer/company-profile.html", label: "Company Profile" },
      { href: "/module3-employer/verification.html", label: "Verification" },
      { href: "/module3-employer/vacancies.html", label: "Vacancies" },
      { href: "/module4-applications/employer-applicants.html", label: "Applicants" },
      { href: "/module4-applications/employer-contact-requests.html", label: "Contact Requests" },
      { href: "/module4-applications/employer-interviews.html", label: "Interviews" },
      { href: "/module5-trust/notifications.html", label: "Notifications" },
    ],
    Administrator: [
      { href: "/module1-core/admin-dashboard.html", label: "Dashboard" },
      { href: "/module1-core/users.html", label: "Users" },
      { href: "/module3-employer/admin-employer-verifications.html", label: "Employer Verifications" },
      { href: "/module3-employer/admin-pending-vacancies.html", label: "Pending Vacancies" },
      { href: "/module5-trust/admin-reported-jobs.html", label: "Reported Jobs" },
      { href: "/module5-trust/moderation-audit.html", label: "Moderation Audit" },
      { href: "/module5-trust/notifications.html", label: "Notifications" },
    ],
  };

  function mountAppShell(user) {
    const path = location.pathname;
    const items = menus[user.role] || [];
    const shell = document.getElementById("app-shell");
    if (!shell) return;

    shell.innerHTML = `
      <header class="app-header">
        <div style="display:flex;align-items:center;gap:0.75rem">
          <button class="menu-btn" type="button" id="sidebar-toggle" aria-label="Menu">Menu</button>
          <a class="logo" href="${SR.auth.homeForRole(user.role)}"><span class="logo-mark">SR</span> SMART RECRUIT</a>
        </div>
        <div class="nav-actions">
          <span id="notif-bell-slot"></span>
          <span class="muted" style="color:rgba(255,255,255,.8);font-size:.85rem">${esc(user.fullName || user.email)}</span>
          <button class="btn btn-ghost-light btn-sm" type="button" id="logout-btn">Logout</button>
        </div>
      </header>
      <aside class="sidebar" id="sidebar">
        ${items
          .map((i) => {
            const active = path.endsWith(i.href.split("/").pop()) || path === i.href ? "active" : "";
            return `<a href="${i.href}" class="${active}">${esc(i.label)}</a>`;
          })
          .join("")}
      </aside>
      <main class="main"><div class="page" id="page-root"></div></main>`;

    document.getElementById("logout-btn")?.addEventListener("click", () => SR.auth.logout());
    document.getElementById("sidebar-toggle")?.addEventListener("click", () => {
      shell.classList.toggle("sidebar-open");
    });
  }

  function page(title, subtitle, actionsHtml = "") {
    const root = document.getElementById("page-root");
    if (!root) return null;
    root.innerHTML = `
      <div class="page-head">
        <div>
          <h1>${esc(title)}</h1>
          ${subtitle ? `<p>${esc(subtitle)}</p>` : ""}
        </div>
        <div>${actionsHtml || ""}</div>
      </div>
      <div id="page-body"></div>`;
    return document.getElementById("page-body");
  }

  async function mountNotificationBell() {
    const slot = document.getElementById("notif-bell-slot");
    if (!slot || typeof SR.module5?.unreadCount !== "function") return;
    try {
      const count = await SR.module5.unreadCount();
      const n = typeof count === "number" ? count : count?.count ?? count?.unreadCount ?? 0;
      slot.innerHTML = `<a class="btn btn-ghost-light btn-sm" href="/module5-trust/notifications.html">Alerts${n ? ` (${n})` : ""}</a>`;
    } catch {
      slot.innerHTML = `<a class="btn btn-ghost-light btn-sm" href="/module5-trust/notifications.html">Alerts</a>`;
    }
  }

  return {
    toast,
    badge,
    empty,
    loader,
    setLoading,
    modal,
    confirmAction,
    mountPublicHeader,
    mountPublicShell,
    mountAppShell,
    page,
    mountNotificationBell,
  };
})();
