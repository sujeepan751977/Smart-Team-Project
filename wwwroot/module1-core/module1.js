window.SR = window.SR || {};

SR.module1 = (function () {
  const path = location.pathname;

  async function bootAdmin(title, sub, actions) {
    const user = await SR.guards.requireAuth(["Administrator"]);
    if (!user) return null;
    SR.ui.mountAppShell(user);
    await SR.ui.mountNotificationBell();
    return SR.ui.page(title, sub, actions);
  }

  async function adminDashboard() {
    const body = await bootAdmin("Admin dashboard", "Platform overview and shortcuts.");
    if (!body) return;
    try {
      const d = await SR.api.get("/api/admin/dashboard");
      const user = SR.auth.getUser();
      body.innerHTML = `
        <div class="dash">
          <div class="card dash-hero">
            <h2>Welcome back, ${SR.utils.escape(user?.fullName || "Admin")}</h2>
            <p>Monitor users, verifications, vacancies, and trust signals from one place.</p>
            <div class="row-actions">
              <a class="btn btn-primary" href="/module1-core/users.html">Manage users</a>
              <a class="btn btn-outline" href="/module5-trust/admin-reported-jobs.html">Review reports</a>
            </div>
          </div>
          <div class="dash-stats">
            ${SR.ui.dashStat({ value: d.totalUsers ?? 0, label: "Total users", tone: "navy", iconName: "users" })}
            ${SR.ui.dashStat({ value: d.totalJobSeekers ?? 0, label: "Job seekers", tone: "blue", iconName: "profile" })}
            ${SR.ui.dashStat({ value: d.totalEmployers ?? 0, label: "Employers", tone: "blue", iconName: "company" })}
            ${SR.ui.dashStat({ value: d.activeUsers ?? 0, label: "Active", tone: "ok", iconName: "verify" })}
            ${SR.ui.dashStat({ value: d.disabledUsers ?? 0, label: "Disabled", tone: "danger", iconName: "reports" })}
          </div>
          <p class="dash-section-title">Quick actions</p>
          <div class="dash-links">
            ${SR.ui.dashLink({ href: "/module1-core/users.html", label: "Users Management", desc: "Search, enable, or disable accounts", iconName: "users" })}
            ${SR.ui.dashLink({ href: "/module3-employer/admin-employer-verifications.html", label: "Employer Verification", desc: "Review company documents", iconName: "verify" })}
            ${SR.ui.dashLink({ href: "/module3-employer/admin-pending-vacancies.html", label: "Pending Vacancies", desc: "Approve or reject listings", iconName: "jobs" })}
            ${SR.ui.dashLink({ href: "/module5-trust/admin-reported-jobs.html", label: "Reported Jobs", desc: "Investigate job reports", iconName: "reports" })}
            ${SR.ui.dashLink({ href: "/module5-trust/moderation-audit.html", label: "Moderation Audit", desc: "Track moderation decisions", iconName: "audit" })}
          </div>
        </div>`;
    } catch (e) {
      body.innerHTML = SR.ui.errorState(e);
    }
  }

  async function usersPage() {
    const body = await bootAdmin("Users", "Search and manage account status.");
    if (!body) return;
    try {
      const users = await SR.api.get("/api/admin/users");
      const list = Array.isArray(users) ? users : [];
      body.innerHTML = `
        <div class="card filter-bar" style="margin-bottom:1rem">
          <div class="filter-toolbar">
            <label class="filter-search">
              <span class="field-label">Search</span>
              <input id="q" type="search" placeholder="Name or email" />
            </label>
            <label class="filter-select">
              <span class="field-label">Role</span>
              <select id="role">
                <option value="">All</option>
                <option>JobSeeker</option>
                <option>Employer</option>
                <option>Administrator</option>
              </select>
            </label>
            <label class="filter-select">
              <span class="field-label">Status</span>
              <select id="status">
                <option value="">All</option>
                <option value="active">Active</option>
                <option value="disabled">Disabled</option>
              </select>
            </label>
          </div>
        </div>
        <div class="card table-wrap table-as-cards">
          <table class="data" id="users-table">
            <thead><tr><th>ID</th><th>Name</th><th>Email</th><th>Role</th><th>Status</th><th>Actions</th></tr></thead>
            <tbody></tbody>
          </table>
        </div>`;

      const me = SR.auth.getUser();
      const render = () => {
        const q = document.getElementById("q").value.toLowerCase();
        const role = document.getElementById("role").value;
        const status = document.getElementById("status").value;
        const filtered = list.filter((u) => {
          const hay = `${u.fullName} ${u.email}`.toLowerCase();
          if (q && !hay.includes(q)) return false;
          if (role && u.role !== role) return false;
          if (status === "active" && !u.isActive) return false;
          if (status === "disabled" && u.isActive) return false;
          return true;
        });
        const tb = document.querySelector("#users-table tbody");
        tb.innerHTML = filtered
          .map(
            (u) => `
          <tr data-id="${u.id}">
            <td class="cell-id" data-label="ID">${u.id}</td>
            <td class="cell-strong" data-label="Name">${SR.utils.escape(u.fullName)}</td>
            <td class="cell-muted" data-label="Email">${SR.utils.escape(u.email)}</td>
            <td data-label="Role">${SR.ui.badge(u.role, "neutral")}</td>
            <td data-label="Status">${u.isActive ? SR.ui.badge("Active", "ok") : SR.ui.badge("Disabled", "danger")}</td>
            <td class="cell-actions" data-label="Actions">
              <div class="row-actions" style="margin:0">
                <a class="btn btn-outline btn-sm" href="/module1-core/user-details.html?id=${u.id}">Details</a>
                <button class="btn btn-sm ${u.isActive ? "btn-danger" : "btn-primary"}" data-toggle type="button">${u.isActive ? "Disable" : "Enable"}</button>
              </div>
            </td>
          </tr>`
          )
          .join("");

        tb.querySelectorAll("[data-toggle]").forEach((btn) => {
          btn.onclick = async () => {
            const id = btn.closest("tr").dataset.id;
            const enable = btn.textContent.trim() === "Enable";
            if (String(me?.userId) === String(id) && !enable) {
              SR.ui.toast("You cannot disable your own account.", "error");
              return;
            }
            const run = async () => {
              try {
                await SR.api.patch(`/api/admin/users/${id}/status`, { isActive: enable });
                SR.ui.toast("Status updated");
                location.reload();
              } catch (e) {
                SR.ui.toast(e.message, "error");
              }
            };
            if (!enable) {
              SR.ui.confirmAction({
                title: "Disable user",
                message: "Disable this account? The user will no longer be able to sign in.",
                confirmText: "Disable",
                onConfirm: run,
              });
              return;
            }
            await run();
          };
        });
      };

      ["q", "role", "status"].forEach((id) =>
        document.getElementById(id).addEventListener("input", render)
      );
      document.getElementById("role").addEventListener("change", render);
      document.getElementById("status").addEventListener("change", render);
      render();
    } catch (e) {
      SR.ui.toast(e.message, "error");
    }
  }

  async function userDetails() {
    const body = await bootAdmin("User details", "");
    if (!body) return;
    const id = SR.utils.qs("id");
    if (!id) {
      body.innerHTML = SR.ui.empty("Missing user id.");
      return;
    }
    try {
      const u = await SR.api.get(`/api/admin/users/${id}`);
      body.innerHTML = `
        <div class="card">
          <h2>${SR.utils.escape(u.fullName)}</h2>
          <p class="muted">${SR.utils.escape(u.email)}</p>
          <div class="meta" style="margin-top:0.8rem">
            ${SR.ui.badge(u.role)}
            ${u.isActive ? SR.ui.badge("Active", "ok") : SR.ui.badge("Disabled", "danger")}
            <span>Joined ${SR.utils.formatDate(u.createdAt)}</span>
          </div>
          <div class="row-actions">
            <a class="btn btn-outline" href="/module1-core/users.html">Back</a>
          </div>
        </div>`;
    } catch (e) {
      SR.ui.toast(e.message, "error");
    }
  }

  function initPublic() {
    if (path.endsWith("/index.html") || path === "/" || path.endsWith("/wwwroot/")) {
      SR.ui.mountPublicHeader();
      return;
    }
    if (path.endsWith("/login.html")) {
      if (SR.guards.redirectIfAuthed()) return;
      document.getElementById("toggle-password")?.addEventListener("click", () => {
        const input = document.getElementById("login-password");
        const btn = document.getElementById("toggle-password");
        if (!input || !btn) return;
        const show = input.type === "password";
        input.type = show ? "text" : "password";
        btn.classList.toggle("is-visible", show);
        btn.setAttribute("aria-label", show ? "Hide password" : "Show password");
        btn.title = show ? "Hide password" : "Show password";
      });
      document.getElementById("login-form")?.addEventListener("submit", async (e) => {
        e.preventDefault();
        const alert = document.getElementById("login-alert");
        alert.hidden = true;
        const data = SR.utils.formToObject(e.target);
        const btn = e.target.querySelector("[type=submit]");
        try {
          SR.ui.setLoading(btn, true, "Signing in…");
          const user = await SR.auth.login(data.email, data.password);
          SR.ui.toast("Signed in");
          const next = new URLSearchParams(location.search).get("next");
          const safeNext =
            next && next.startsWith("/") && !next.startsWith("//") ? next : null;
          location.href = safeNext || SR.auth.homeForRole(user.role);
        } catch (err) {
          alert.hidden = false;
          alert.textContent = err.message || "Login failed";
        } finally {
          SR.ui.setLoading(btn, false);
        }
      });
      return;
    }
    if (path.endsWith("/register.html")) {
      if (SR.guards.redirectIfAuthed()) return;
      let role = "JobSeeker";
      const roleParam = new URLSearchParams(location.search).get("role");
      if (roleParam === "Employer") {
        role = "Employer";
        document.querySelectorAll("#role-tabs button").forEach((b) => {
          b.classList.toggle("active", b.dataset.role === "Employer");
        });
      }
      document.querySelectorAll("#role-tabs button").forEach((btn) => {
        btn.addEventListener("click", () => {
          role = btn.dataset.role;
          document.querySelectorAll("#role-tabs button").forEach((b) => b.classList.remove("active"));
          btn.classList.add("active");
        });
      });
      document.getElementById("register-form")?.addEventListener("submit", async (e) => {
        e.preventDefault();
        const alert = document.getElementById("register-alert");
        alert.hidden = true;
        const data = SR.utils.formToObject(e.target);
        if (!data.fullName) {
          alert.hidden = false;
          alert.textContent = "Full name is required.";
          return;
        }
        if (!SR.utils.isValidEmail(data.email)) {
          alert.hidden = false;
          alert.textContent = "Enter a valid email.";
          return;
        }
        if ((data.password || "").length < 8) {
          alert.hidden = false;
          alert.textContent = "Password must be at least 8 characters.";
          return;
        }
        if (data.password !== data.confirmPassword) {
          alert.hidden = false;
          alert.textContent = "Passwords must match.";
          return;
        }
        const btn = e.target.querySelector("[type=submit]");
        const payload = {
          fullName: data.fullName,
          email: data.email,
          password: data.password,
          confirmPassword: data.confirmPassword,
        };
        try {
          SR.ui.setLoading(btn, true, "Creating…");
          if (role === "Employer") await SR.auth.registerEmployer(payload);
          else await SR.auth.registerJobSeeker(payload);
          SR.ui.toast("Account created successfully. Please sign in.");
          location.href = "/login.html";
        } catch (err) {
          alert.hidden = false;
          alert.textContent = err.message || "Registration failed";
        } finally {
          SR.ui.setLoading(btn, false);
        }
      });
    }
  }

  document.addEventListener("DOMContentLoaded", () => {
    if (path.includes("/module1-core/admin-dashboard")) return adminDashboard();
    if (path.includes("/module1-core/users.html")) return usersPage();
    if (path.includes("/module1-core/user-details")) return userDetails();
    initPublic();
  });

  return {};
})();
