window.SR = window.SR || {};

SR.ui = (function () {
  const esc = (...a) => SR.utils.escape(...a);

  const NAV_ALIASES = {
    "/module2-jobseeker/job-details.html": "/module2-jobseeker/jobs.html",
    "/module3-employer/vacancy-form.html": "/module3-employer/vacancies.html",
    "/module3-employer/vacancy-details.html": "/module3-employer/vacancies.html",
    "/module1-core/user-details.html": "/module1-core/users.html",
    "/module3-employer/admin-employer-verification-details.html":
      "/module3-employer/admin-employer-verifications.html",
    "/module4-applications/employer-application-details.html":
      "/module4-applications/employer-applicants.html",
    "/module5-trust/admin-job-report-details.html":
      "/module5-trust/admin-reported-jobs.html",
  };

  const ICONS = {
    bell: `<svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M12 22a2.2 2.2 0 0 0 2.2-2.2h-4.4A2.2 2.2 0 0 0 12 22Zm7-5.2V11a7 7 0 1 0-14 0v5.8L3 18.8v1h18v-1l-2-2Z" fill="currentColor"/></svg>`,
    logout: `<svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M10 4H6a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h4" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"/><path d="M15 8l4 4-4 4M9 12h10" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"/></svg>`,
    menu: `<svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M4 7h16M4 12h16M4 17h16" stroke="currentColor" stroke-width="1.9" stroke-linecap="round"/></svg>`,
    dashboard: `<svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M4 4h7v7H4V4Zm9 0h7v5h-7V4ZM4 13h7v7H4v-7Zm9 3h7v4h-7v-4Z" stroke="currentColor" stroke-width="1.7" stroke-linejoin="round"/></svg>`,
    jobs: `<svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M9 6V5a2 2 0 0 1 2-2h2a2 2 0 0 1 2 2v1h4a2 2 0 0 1 2 2v10a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h4Zm2-1h2v1h-2V5Z" stroke="currentColor" stroke-width="1.7" stroke-linejoin="round"/></svg>`,
    profile: `<svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M12 12a4 4 0 1 0-4-4 4 4 0 0 0 4 4Zm0 2c-4 0-7 2-7 4.5V20h14v-1.5C19 16 16 14 12 14Z" stroke="currentColor" stroke-width="1.7" stroke-linejoin="round"/></svg>`,
    applications: `<svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M7 3h7l5 5v13a1 1 0 0 1-1 1H7a1 1 0 0 1-1-1V4a1 1 0 0 1 1-1Z" stroke="currentColor" stroke-width="1.7" stroke-linejoin="round"/><path d="M14 3v5h5M9 13h6M9 17h6" stroke="currentColor" stroke-width="1.7" stroke-linecap="round"/></svg>`,
    contact: `<svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M4 6a2 2 0 0 1 2-2h12a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V6Z" stroke="currentColor" stroke-width="1.7"/><path d="m5 8 7 5 7-5" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round"/></svg>`,
    interviews: `<svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M7 3v2M17 3v2M4 9h16M6 5h12a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V7a2 2 0 0 1 2-2Z" stroke="currentColor" stroke-width="1.7" stroke-linejoin="round"/><path d="M8 13h4M8 17h8" stroke="currentColor" stroke-width="1.7" stroke-linecap="round"/></svg>`,
    reports: `<svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M12 9v4M12 17h.01M10.3 4.3 2.8 17.2A2 2 0 0 0 4.5 20h15a2 2 0 0 0 1.7-2.8L13.7 4.3a2 2 0 0 0-3.4 0Z" stroke="currentColor" stroke-width="1.7" stroke-linejoin="round"/></svg>`,
    notifications: `<svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M12 22a2.2 2.2 0 0 0 2.2-2.2h-4.4A2.2 2.2 0 0 0 12 22Zm7-5.2V11a7 7 0 1 0-14 0v5.8L3 18.8v1h18v-1l-2-2Z" fill="currentColor"/></svg>`,
    company: `<svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M3 21h18M5 21V7l7-3 7 3v14M9 21v-5h6v5M9 10h1M14 10h1M9 14h1M14 14h1" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round"/></svg>`,
    verify: `<svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M9 12l2 2 4-4" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"/><path d="M12 3 4.5 6v6c0 4.5 3.2 7.8 7.5 9 4.3-1.2 7.5-4.5 7.5-9V6L12 3Z" stroke="currentColor" stroke-width="1.7" stroke-linejoin="round"/></svg>`,
    users: `<svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2M9 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8Zm13 10v-2a4 4 0 0 0-3-3.87M16 3.13a4 4 0 0 1 0 7.75" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round"/></svg>`,
    audit: `<svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M9 5H7a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V7a2 2 0 0 0-2-2h-2" stroke="currentColor" stroke-width="1.7" stroke-linejoin="round"/><path d="M9 5a2 2 0 0 1 2-2h2a2 2 0 0 1 2 2v0a2 2 0 0 1-2 2h-2a2 2 0 0 1-2-2v0ZM9 12h6M9 16h4" stroke="currentColor" stroke-width="1.7" stroke-linecap="round"/></svg>`,
    applicants: `<svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2M9 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8Zm14 10v-2a4 4 0 0 0-3-3.87M16 3.13a4 4 0 0 1 0 7.75" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round"/></svg>`,
  };

  function icon(name) {
    return ICONS[name] || "";
  }

  function toast(message, type = "ok") {
    let root = document.getElementById("toast-root");
    if (!root) {
      root = document.createElement("div");
      root.id = "toast-root";
      root.className = "toast-root";
      root.setAttribute("aria-live", "polite");
      document.body.appendChild(root);
    }
    const kind =
      type === "error"
        ? "error"
        : type === "warn" || type === "warning"
          ? "warn"
          : type === "info"
            ? "info"
            : "ok";
    const el = document.createElement("div");
    el.className = `toast ${kind}`;
    el.textContent = message;
    root.appendChild(el);
    while (root.children.length > 4) root.firstChild.remove();
    setTimeout(() => el.remove(), 3400);
  }

  function badge(text, kind = "") {
    return `<span class="badge ${kind}">${esc(text)}</span>`;
  }

  function empty(title, opts = {}) {
    if (typeof opts === "string") opts = { detail: opts };
    const sub = opts.detail || "";
    const cta = opts.cta
      ? `<div class="row-actions"><a class="btn btn-primary" href="${esc(opts.cta.href)}">${esc(opts.cta.label)}</a></div>`
      : "";
    if (!sub && !opts.cta) {
      return `<div class="empty">${esc(title)}</div>`;
    }
    return `<div class="empty"><strong>${esc(title)}</strong>${
      sub ? `<p>${esc(sub)}</p>` : ""
    }${cta}</div>`;
  }

  function skeleton(rows = 3) {
    return `<div class="panel-loading">${Array.from({ length: rows })
      .map(
        (_, i) =>
          `<div class="card"><div class="skeleton" style="width:${90 - i * 10}%"></div><div class="skeleton" style="margin-top:.6rem;width:70%"></div></div>`
      )
      .join("")}</div>`;
  }

  function friendlyError(err) {
    const status = err?.status;
    const msg = err?.message || "";
    if (status === 401) return "Your session has expired. Please sign in again.";
    if (status === 403) return "You don't have permission to perform this action.";
    if (status === 404) return msg || "The requested item could not be found.";
    if (status === 409) return msg || "This action conflicts with the current state.";
    if (status === 400) return msg || "Please check the information and try again.";
    if (status >= 500) return "Something went wrong. Please try again.";
    if (/failed to fetch|networkerror|load failed/i.test(msg)) {
      return "Unable to reach the server. Check your connection and try again.";
    }
    return msg || "Something went wrong. Please try again.";
  }

  function errorState(err, opts = {}) {
    const message = friendlyError(err);
    const retry = opts.retryLabel
      ? `<div class="row-actions"><button type="button" class="btn btn-outline" data-retry>${esc(opts.retryLabel)}</button></div>`
      : opts.cta
        ? `<div class="row-actions"><a class="btn btn-primary" href="${esc(opts.cta.href)}">${esc(opts.cta.label)}</a></div>`
        : "";
    return `<div class="empty"><strong>Unable to load</strong><p>${esc(message)}</p>${retry}</div>`;
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

  function initials(name) {
    const parts = String(name || "?")
      .trim()
      .split(/\s+/)
      .filter(Boolean);
    if (!parts.length) return "?";
    if (parts.length === 1) return parts[0].slice(0, 2).toUpperCase();
    return (parts[0][0] + parts[1][0]).toUpperCase();
  }

  function roleLabel(role) {
    const map = {
      JobSeeker: "Job Seeker",
      Employer: "Employer",
      Administrator: "Administrator",
    };
    return map[role] || role || "Workspace";
  }

  function displayName(name) {
    const raw = String(name || "").trim();
    if (!raw) return "";
    if (raw.includes("@")) return raw;
    return raw
      .split(/\s+/)
      .map((part) => (part ? part[0].toUpperCase() + part.slice(1) : ""))
      .join(" ");
  }

  function modal({ title, bodyHtml, confirmText = "Confirm", danger = false, onConfirm }) {
    const backdrop = document.createElement("div");
    backdrop.className = "modal-backdrop";
    const titleId = `modal-title-${Date.now()}`;
    backdrop.innerHTML = `
      <div class="modal" role="dialog" aria-modal="true" aria-labelledby="${titleId}">
        <h2 id="${titleId}">${esc(title)}</h2>
        <div class="modal-body">${bodyHtml || ""}</div>
        <div class="row-actions">
          <button type="button" class="btn btn-outline" data-cancel>Cancel</button>
          <button type="button" class="btn ${danger ? "btn-danger" : "btn-primary"}" data-ok>${esc(confirmText)}</button>
        </div>
      </div>`;
    document.body.appendChild(backdrop);
    const okBtn = backdrop.querySelector("[data-ok]");
    const close = () => {
      document.removeEventListener("keydown", onKey);
      backdrop.remove();
    };
    const onKey = (e) => {
      if (e.key === "Escape") close();
    };
    document.addEventListener("keydown", onKey);
    backdrop.querySelector("[data-cancel]").onclick = close;
    backdrop.addEventListener("click", (e) => {
      if (e.target === backdrop) close();
    });
    okBtn.onclick = async () => {
      try {
        setLoading(okBtn, true, "Working…");
        if (onConfirm) await onConfirm(backdrop);
        close();
      } catch (err) {
        setLoading(okBtn, false);
        if (err?.message && err.message !== "Decision note required") {
          SR.ui.toast(friendlyError(err), "error");
        }
      }
    };
    setTimeout(() => okBtn.focus(), 0);
    return backdrop;
  }

  function confirmAction({
    title,
    message,
    noteRequired = false,
    confirmText = "Confirm",
    danger = true,
    onConfirm,
  }) {
    const body = `
      <p class="modal-message">${esc(message)}</p>
      ${
        noteRequired
          ? `<label class="field"><span class="field-label">Decision note <span class="req" aria-hidden="true">*</span></span>
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

  function wirePublicMenu(header) {
    const btn = header.querySelector("#public-menu-btn");
    const panel = header.querySelector("#public-mobile-nav");
    if (!btn || !panel) return;
    btn.addEventListener("click", () => {
      const open = header.classList.toggle("nav-open");
      btn.setAttribute("aria-expanded", open ? "true" : "false");
    });
    panel.querySelectorAll("a").forEach((a) => {
      a.addEventListener("click", () => {
        header.classList.remove("nav-open");
        btn.setAttribute("aria-expanded", "false");
      });
    });
  }

  function publicHeader(active = "") {
    return `
      <header class="public-header" id="public-header">
        <a class="logo" href="/index.html">
          <span class="logo-mark">SR</span>
          <span class="logo-text">
            <span class="logo-text-main">SMART RECRUIT</span>
            <span class="logo-text-sub">Hiring platform</span>
          </span>
        </a>
        <nav class="nav-links desktop-only" aria-label="Primary">
          <a href="/module2-jobseeker/jobs.html" class="${active === "jobs" ? "active" : ""}">Find Jobs</a>
          <a href="/index.html#how" class="${active === "how" ? "active" : ""}">How It Works</a>
          <a href="/index.html#employers" class="${active === "employers" ? "active" : ""}">For Employers</a>
        </nav>
        <div class="nav-actions">
          <button class="menu-btn" type="button" id="public-menu-btn" aria-label="Menu" aria-expanded="false" aria-controls="public-mobile-nav">Menu</button>
          <a class="btn btn-ghost-light btn-sm" href="/login.html">Login</a>
          <a class="btn btn-primary btn-sm" href="/register.html">Create Account</a>
        </div>
        <nav class="public-mobile-nav" id="public-mobile-nav" aria-label="Mobile">
          <a href="/module2-jobseeker/jobs.html" class="${active === "jobs" ? "active" : ""}">Find Jobs</a>
          <a href="/index.html#how" class="${active === "how" ? "active" : ""}">How It Works</a>
          <a href="/index.html#employers" class="${active === "employers" ? "active" : ""}">For Employers</a>
          <a href="/login.html">Login</a>
          <a href="/register.html">Create Account</a>
        </nav>
      </header>`;
  }

  function mountPublicHeader(active) {
    const slot = document.getElementById("site-header");
    if (!slot) return;
    slot.outerHTML = publicHeader(active);
    const header = document.getElementById("public-header");
    if (header) wirePublicMenu(header);
  }

  function mountPublicShell(active = "jobs") {
    const shell = document.getElementById("app-shell");
    if (!shell) return;
    document.documentElement.classList.remove("has-app-shell");
    document.body.classList.remove("has-app-shell");
    shell.className = "public-shell";
    shell.innerHTML = `
      ${publicHeader(active)}
      <main class="public-main"><div class="container page" id="page-root"></div></main>`;
    const header = document.getElementById("public-header");
    if (header) wirePublicMenu(header);
  }

  const menus = {
    JobSeeker: [
      { href: "/module2-jobseeker/dashboard.html", label: "Dashboard", icon: "dashboard" },
      { href: "/module2-jobseeker/jobs.html", label: "Find Jobs", icon: "jobs" },
      { href: "/module2-jobseeker/profile.html", label: "Profile", icon: "profile" },
      { href: "/module4-applications/jobseeker-applications.html", label: "My Applications", icon: "applications" },
      { href: "/module4-applications/jobseeker-contact-requests.html", label: "Contact Requests", icon: "contact" },
      { href: "/module4-applications/jobseeker-interviews.html", label: "Interviews", icon: "interviews" },
      { href: "/module5-trust/job-reports.html", label: "Job Reports", icon: "reports" },
      { href: "/module5-trust/notifications.html", label: "Notifications", icon: "notifications" },
    ],
    Employer: [
      { href: "/module3-employer/employer-dashboard.html", label: "Dashboard", icon: "dashboard" },
      { href: "/module3-employer/company-profile.html", label: "Company Profile", icon: "company" },
      { href: "/module3-employer/verification.html", label: "Verification", icon: "verify" },
      { href: "/module3-employer/vacancies.html", label: "Vacancies", icon: "jobs" },
      { href: "/module4-applications/employer-applicants.html", label: "Applicants", icon: "applicants" },
      { href: "/module4-applications/employer-contact-requests.html", label: "Contact Requests", icon: "contact" },
      { href: "/module4-applications/employer-interviews.html", label: "Interviews", icon: "interviews" },
      { href: "/module5-trust/notifications.html", label: "Notifications", icon: "notifications" },
    ],
    Administrator: [
      { href: "/module1-core/admin-dashboard.html", label: "Dashboard", icon: "dashboard" },
      { href: "/module1-core/users.html", label: "Users", icon: "users" },
      { href: "/module3-employer/admin-employer-verifications.html", label: "Employer Verifications", icon: "verify" },
      { href: "/module3-employer/admin-pending-vacancies.html", label: "Pending Vacancies", icon: "jobs" },
      { href: "/module5-trust/admin-reported-jobs.html", label: "Reported Jobs", icon: "reports" },
      { href: "/module5-trust/moderation-audit.html", label: "Moderation Audit", icon: "audit" },
      { href: "/module5-trust/notifications.html", label: "Notifications", icon: "notifications" },
    ],
  };

  function isNavActive(path, href) {
    const file = href.split("/").pop();
    if (path.endsWith(file) || path === href) return true;
    const alias = NAV_ALIASES[path];
    return alias === href;
  }

  function mountAppShell(user) {
    const path = location.pathname;
    const items = menus[user.role] || [];
    const shell = document.getElementById("app-shell");
    if (!shell) return;

    shell.className = "app-shell";
    document.documentElement.classList.add("has-app-shell");
    document.body.classList.add("has-app-shell");
    const name = displayName(user.fullName || user.email || "");
    const role = roleLabel(user.role);
    shell.innerHTML = `
      <header class="app-header">
        <div class="app-header-left">
          <button class="menu-btn header-icon-btn" type="button" id="sidebar-toggle" aria-label="Open menu" aria-expanded="false" aria-controls="sidebar">${icon("menu")}</button>
          <a class="logo" href="${SR.auth.homeForRole(user.role)}">
            <span class="logo-mark">SR</span>
            <span class="logo-text">
              <span class="logo-text-main">Smart Recruit</span>
              <span class="logo-text-sub">${esc(role)}</span>
            </span>
          </a>
        </div>
        <div class="app-header-right nav-actions">
          <span id="notif-bell-slot"></span>
          <div class="header-divider" aria-hidden="true"></div>
          <span class="user-chip" title="${esc(user.email || "")}">
            <span class="user-avatar" aria-hidden="true">${esc(initials(user.fullName || user.email))}</span>
            <span class="user-meta">
              <span class="user-name">${esc(name)}</span>
              <span class="user-role">${esc(role)}</span>
            </span>
          </span>
          <button class="header-icon-btn is-logout" type="button" id="logout-btn" aria-label="Logout" title="Logout">${icon("logout")}</button>
        </div>
      </header>
      <div class="sidebar-backdrop" id="sidebar-backdrop" hidden></div>
      <aside class="sidebar" id="sidebar">
        <div class="sidebar-label">Navigation</div>
        <nav class="sidebar-nav" aria-label="Application">
          ${items
            .map((i) => {
              const active = isNavActive(path, i.href) ? "active" : "";
              return `<a href="${i.href}" class="${active}"><span class="nav-icon">${icon(i.icon)}</span><span>${esc(i.label)}</span></a>`;
            })
            .join("")}
        </nav>
      </aside>
      <main class="main"><div class="page" id="page-root"></div></main>`;

    const toggle = document.getElementById("sidebar-toggle");
    const backdrop = document.getElementById("sidebar-backdrop");
    const closeSidebar = () => {
      shell.classList.remove("sidebar-open");
      if (toggle) toggle.setAttribute("aria-expanded", "false");
      if (backdrop) backdrop.hidden = true;
    };
    const openSidebar = () => {
      shell.classList.add("sidebar-open");
      if (toggle) toggle.setAttribute("aria-expanded", "true");
      if (backdrop) backdrop.hidden = false;
    };

    document.getElementById("logout-btn")?.addEventListener("click", () => SR.auth.logout());
    toggle?.addEventListener("click", () => {
      if (shell.classList.contains("sidebar-open")) closeSidebar();
      else openSidebar();
    });
    backdrop?.addEventListener("click", closeSidebar);
    document.addEventListener("keydown", (e) => {
      if (e.key === "Escape") closeSidebar();
    });
    shell.querySelectorAll(".sidebar a").forEach((a) => {
      a.addEventListener("click", closeSidebar);
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
    if (!slot) return;
    const render = (n) => {
      const count = Number(n) || 0;
      const badge = count
        ? `<span class="header-alert-badge">${count > 99 ? "99+" : count}</span>`
        : "";
      slot.innerHTML = `<a class="header-icon-btn" href="/module5-trust/notifications.html" aria-label="${
        count ? `Alerts, ${count} unread` : "Alerts"
      }" title="Alerts">${icon("bell")}${badge}</a>`;
    };
    if (typeof SR.module5?.unreadCount !== "function") {
      render(0);
      return;
    }
    try {
      const count = await SR.module5.unreadCount();
      const n = typeof count === "number" ? count : count?.count ?? count?.unreadCount ?? 0;
      render(n);
    } catch {
      render(0);
    }
  }

  function dashStat({ value, label, tone = "blue", iconName = "" }) {
    return `<div class="dash-stat tone-${esc(tone)}">
      <div class="dash-stat-top">
        ${iconName ? `<span class="dash-stat-icon">${icon(iconName)}</span>` : `<span></span>`}
        <strong>${esc(String(value ?? 0))}</strong>
      </div>
      <span>${esc(label)}</span>
    </div>`;
  }

  function dashLink({ href, label, desc = "", iconName = "dashboard" }) {
    return `<a class="dash-link" href="${href}">
      <span class="dash-link-icon">${icon(iconName)}</span>
      <span class="dash-link-copy">
        <strong>${esc(label)}</strong>
        ${desc ? `<span>${esc(desc)}</span>` : ""}
      </span>
      <span class="dash-link-arrow" aria-hidden="true">→</span>
    </a>`;
  }

  return {
    toast,
    badge,
    empty,
    skeleton,
    friendlyError,
    errorState,
    loader,
    setLoading,
    modal,
    confirmAction,
    mountPublicHeader,
    mountPublicShell,
    mountAppShell,
    page,
    mountNotificationBell,
    initials,
    icon,
    dashStat,
    dashLink,
  };
})();
