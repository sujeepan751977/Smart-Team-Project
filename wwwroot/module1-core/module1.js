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
      body.innerHTML = `
        <div class="grid-3">
          <div class="card stat-card"><strong>${d.totalUsers ?? 0}</strong><span>Total users</span></div>
          <div class="card stat-card"><strong>${d.totalJobSeekers ?? 0}</strong><span>Job seekers</span></div>
          <div class="card stat-card"><strong>${d.totalEmployers ?? 0}</strong><span>Employers</span></div>
          <div class="card stat-card"><strong>${d.activeUsers ?? 0}</strong><span>Active</span></div>
          <div class="card stat-card"><strong>${d.disabledUsers ?? 0}</strong><span>Disabled</span></div>
        </div>
        <div class="admin-shortcuts">
          <a href="/module1-core/users.html">Users Management</a>
          <a href="/module3-employer/admin-employer-verifications.html">Employer Verification</a>
          <a href="/module3-employer/admin-pending-vacancies.html">Pending Vacancies</a>
          <a href="/module5-trust/admin-reported-jobs.html">Reported Jobs</a>
          <a href="/module5-trust/moderation-audit.html">Moderation Audit</a>
        </div>`;
    } catch (e) {
      SR.ui.toast(e.message, "error");
    }
  }

  async function usersPage() {
    const body = await bootAdmin("Users", "Search and manage account status.");
    if (!body) return;
    try {
      const users = await SR.api.get("/api/admin/users");
      const list = Array.isArray(users) ? users : [];
      body.innerHTML = `
        <div class="card form-grid" style="margin-bottom:1rem">
          <div class="form-grid two">
            <label class="field">Search<input id="q" placeholder="Name or email" /></label>
            <label class="field">Role
              <select id="role">
                <option value="">All</option>
                <option>JobSeeker</option>
                <option>Employer</option>
                <option>Administrator</option>
              </select>
            </label>
            <label class="field">Status
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
            <thead><tr><th>ID</th><th>Name</th><th>Email</th><th>Role</th><th>Status</th><th></th></tr></thead>
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
            <td data-label="ID">${u.id}</td>
            <td data-label="Name">${SR.utils.escape(u.fullName)}</td>
            <td data-label="Email">${SR.utils.escape(u.email)}</td>
            <td data-label="Role">${SR.utils.escape(u.role)}</td>
            <td data-label="Status">${u.isActive ? SR.ui.badge("Active", "ok") : SR.ui.badge("Disabled", "danger")}</td>
            <td data-label="Actions">
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
            try {
              await SR.api.patch(`/api/admin/users/${id}/status`, { isActive: enable });
              SR.ui.toast("Status updated");
              location.reload();
            } catch (e) {
              SR.ui.toast(e.message, "error");
            }
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
        const show = input.type === "password";
        input.type = show ? "text" : "password";
        btn.textContent = show ? "Hide" : "Show";
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
          location.href =
            safeNext && user.role === "JobSeeker"
              ? safeNext
              : SR.auth.homeForRole(user.role);
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
      SR.ui.mountPublicHeader();
      let role = "JobSeeker";
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
