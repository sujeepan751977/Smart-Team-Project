window.SR = window.SR || {};

SR.module5 = (function () {
  const path = location.pathname;

  async function unreadCount() {
    return SR.api.get("/api/Notifications/unread-count");
  }

  function openReportModal(vacancyId) {
    SR.ui.modal({
      title: "Report this job",
      bodyHtml: `
        <p class="muted" style="margin-bottom:0.8rem">Reports are reviewed by administrators. One report does not automatically disable an employer.</p>
        <label class="field"><span class="field-label">Reason <span class="req" aria-hidden="true">*</span></span>
          <select id="reason" required>
            <option value="1">Fake Job</option>
            <option value="2">Scam</option>
            <option value="3">Spam</option>
            <option value="4">Misleading Information</option>
            <option value="5">Inappropriate Content</option>
            <option value="6">Other</option>
          </select>
        </label>
        <label class="field">Description
          <textarea id="desc" maxlength="1000"></textarea>
        </label>`,
      confirmText: "Submit Report",
      danger: true,
      onConfirm: async (backdrop) => {
        const reason = Number(backdrop.querySelector("#reason").value);
        const description = backdrop.querySelector("#desc").value.trim() || null;
        await SR.api.post(`/api/jobs/${vacancyId}/reports`, { reason, description });
        SR.ui.toast("Report submitted");
        const btn = document.getElementById("report-btn");
        if (btn) {
          btn.disabled = true;
          btn.textContent = "Reported";
        }
      },
    });
  }

  async function boot(roles, title, sub, actions) {
    const user = await SR.guards.requireAuth(roles);
    if (!user) return null;
    SR.ui.mountAppShell(user);
    await SR.ui.mountNotificationBell();
    return { user, body: SR.ui.page(title, sub, actions) };
  }

  async function notifications() {
    const user = SR.auth.getUser();
    const roles =
      user?.role === "Administrator"
        ? ["Administrator"]
        : user?.role === "Employer"
          ? ["Employer"]
          : ["JobSeeker"];
    const ctx = await boot(
      roles,
      "Notifications",
      "Stay up to date on applications and account events.",
      `<button class="btn btn-outline btn-sm" id="read-all" type="button">Mark all read</button>`
    );
    if (!ctx) return;
    try {
      const items = await SR.api.get("/api/Notifications");
      const list = Array.isArray(items) ? items : items.items || [];
      ctx.body.innerHTML = `<div class="list">${
        list.length
          ? list
              .map((n) => {
                const unread = !n.isRead;
                return `
            <div class="list-item ${unread ? "notif-unread" : ""}" data-id="${n.id}">
              <div class="meta">${SR.ui.badge(SR.status.notificationType(n.type || n.notificationType))}
                <span>${SR.utils.formatDate(n.createdAt)}</span>
                ${unread ? SR.ui.badge("Unread", "warn") : SR.ui.badge("Read", "ok")}
              </div>
              <h3 style="margin-top:0.45rem">${SR.utils.escape(n.title || "Notification")}</h3>
              <p class="muted">${SR.utils.escape(n.message || n.body || "")}</p>
              ${
                unread
                  ? `<div class="row-actions"><button class="btn btn-outline btn-sm" data-read type="button">Mark read</button></div>`
                  : ""
              }
            </div>`;
              })
              .join("")
          : SR.ui.empty("You're all caught up.", {
              detail: "New application, verification, and moderation updates will appear here.",
            })
      }</div>`;

      document.getElementById("read-all")?.addEventListener("click", async () => {
        try {
          await SR.api.patch("/api/Notifications/read-all");
          SR.ui.toast("All marked read");
          location.reload();
        } catch (e) {
          SR.ui.toast(e.message, "error");
        }
      });
      ctx.body.querySelectorAll("[data-read]").forEach((btn) => {
        btn.onclick = async () => {
          const id = btn.closest("[data-id]").dataset.id;
          try {
            await SR.api.patch(`/api/Notifications/${id}/read`);
            location.reload();
          } catch (e) {
            SR.ui.toast(e.message, "error");
          }
        };
      });
    } catch (e) {
      SR.ui.toast(e.message, "error");
    }
  }

  async function seekerReports() {
    const ctx = await boot(["JobSeeker"], "Job Reports", "Reports you submitted.");
    if (!ctx) return;
    try {
      const list = await SR.api.get("/api/jobseekers/me/job-reports");
      const items = Array.isArray(list) ? list : [];
      ctx.body.innerHTML = `<div class="list">${
        items.length
          ? items
              .map(
                (r) => `
          <div class="list-item">
            <h3>Report #${r.id} · Vacancy ${r.vacancyId}</h3>
            <div class="meta">
              ${SR.ui.badge(SR.status.reportStatus(r.status), SR.status.reportKind(r.status))}
              <span>${SR.utils.escape(SR.status.reportReason(r.reason))}</span>
              <span>${SR.utils.formatDate(r.createdAt || r.reportedAt)}</span>
              ${r.reviewedAt ? `<span>Reviewed ${SR.utils.formatDate(r.reviewedAt)}</span>` : ""}
            </div>
            ${r.description ? `<p class="muted" style="margin-top:0.5rem">${SR.utils.escape(r.description)}</p>` : ""}
          </div>`
              )
              .join("")
          : SR.ui.empty("You have not reported any jobs.", {
              detail: "If a listing looks suspicious, open the job and use Report Job.",
              cta: { href: "/module2-jobseeker/jobs.html", label: "Find Jobs" },
            })
      }</div>`;
    } catch (e) {
      SR.ui.toast(e.message, "error");
    }
  }

  async function adminReports() {
    const ctx = await boot(["Administrator"], "Reported Jobs", "Review and take action.");
    if (!ctx) return;
    try {
      const list = await SR.api.get("/api/admin/job-reports");
      const items = Array.isArray(list) ? list : [];
      ctx.body.innerHTML = `
        <div class="card filter-bar" style="margin-bottom:1rem">
          <div class="filter-toolbar">
            <label class="filter-select">
              <span class="field-label">Status</span>
              <select id="status-filter">
                <option value="">All</option>
                <option value="1">Pending</option>
                <option value="2">Under Review</option>
                <option value="3">Action Taken</option>
                <option value="4">Rejected</option>
              </select>
            </label>
          </div>
        </div>
        <div id="reports-list" class="list"></div>
        <div class="card moderation-box">
          <h3>Moderation actions</h3>
          <p class="muted" style="margin:0.4rem 0 0.8rem">Decision note is required. One report does not automatically disable an employer.</p>
          <form id="mod-form" class="form-grid two">
            <label class="field">Employer ID<input name="employerId" type="number" /></label>
            <label class="field">Action
              <select name="action">
                <option value="warn">Warn</option>
                <option value="suspend">Suspend</option>
                <option value="disable">Disable</option>
              </select>
            </label>
            <label class="field">Vacancy ID to close<input name="vacancyId" type="number" /></label>
            <label class="field"><span class="field-label">Decision note <span class="req" aria-hidden="true">*</span></span><textarea name="decisionNote" required maxlength="1000"></textarea></label>
            <button class="btn btn-primary" type="submit">Apply employer action</button>
            <button class="btn btn-danger" id="close-vac" type="button">Close vacancy</button>
          </form>
        </div>`;

      const render = async () => {
        const filter = document.getElementById("status-filter");
        const box = document.getElementById("reports-list");
        if (!filter || !box) return;
        const status = filter.value;
        let data = items;
        if (status) {
          data = await SR.api.get(`/api/admin/job-reports/status/${status}`);
          data = Array.isArray(data) ? data : [];
        }
        box.innerHTML = data.length
          ? data
              .map(
                (r) => `
            <div class="list-item" data-id="${r.id}">
              <h3>Report #${r.id} · Vacancy ${r.vacancyId}</h3>
              <div class="meta">
                ${SR.ui.badge(SR.status.reportStatus(r.status), SR.status.reportKind(r.status))}
                <span>${SR.utils.escape(SR.status.reportReason(r.reason))}</span>
                <span>${SR.utils.formatDate(r.createdAt)}</span>
              </div>
              <div class="row-actions">
                <a class="btn btn-outline btn-sm" href="/module5-trust/admin-job-report-details.html?id=${r.id}">Details</a>
                <button class="btn btn-outline btn-sm" data-start type="button">Start review</button>
                <button class="btn btn-primary btn-sm" data-resolve type="button">Resolve</button>
                <button class="btn btn-danger btn-sm" data-dismiss type="button">Dismiss</button>
              </div>
            </div>`
              )
              .join("")
          : SR.ui.empty("No job reports.");

        box.querySelectorAll(".list-item").forEach((item) => {
          const id = item.dataset.id;
          const run = (fn, msg) => async () => {
            try {
              await fn();
              SR.ui.toast(msg);
              location.reload();
            } catch (e) {
              SR.ui.toast(e.message, "error");
            }
          };
          item.querySelector("[data-start]").onclick = run(
            () => SR.api.patch(`/api/admin/job-reports/${id}/start-review`),
            "Review started"
          );
          item.querySelector("[data-resolve]").onclick = run(
            () => SR.api.patch(`/api/admin/job-reports/${id}/resolve`),
            "Resolved"
          );
          item.querySelector("[data-dismiss]").onclick = run(
            () => SR.api.patch(`/api/admin/job-reports/${id}/dismiss`),
            "Dismissed"
          );
        });
      };

      const statusFilter = document.getElementById("status-filter");
      if (statusFilter) statusFilter.onchange = render;
      await render();

      document.getElementById("mod-form")?.addEventListener("submit", (e) => {
        e.preventDefault();
        const d = SR.utils.formToObject(e.target);
        if (!d.decisionNote) {
          SR.ui.toast("Decision note is required", "error");
          return;
        }
        SR.ui.confirmAction({
          title: `Confirm ${d.action}`,
          message: `Apply ${d.action} to employer #${d.employerId}?`,
          noteRequired: false,
          confirmText: "Confirm",
          onConfirm: async () => {
            await SR.api.patch(`/api/admin/employers/${d.employerId}/${d.action}`, {
              decisionNote: d.decisionNote,
            });
            SR.ui.toast("Action applied");
          },
        });
      });

      document.getElementById("close-vac")?.addEventListener("click", () => {
        const d = SR.utils.formToObject(document.getElementById("mod-form"));
        if (!d.decisionNote || !d.vacancyId) {
          SR.ui.toast("Vacancy ID and decision note are required", "error");
          return;
        }
        SR.ui.confirmAction({
          title: "Close vacancy",
          message: `Close vacancy #${d.vacancyId}?`,
          confirmText: "Close",
          onConfirm: async () => {
            await SR.api.patch(`/api/admin/vacancies/${d.vacancyId}/close`, {
              decisionNote: d.decisionNote,
            });
            SR.ui.toast("Vacancy closed");
          },
        });
      });
    } catch (e) {
      SR.ui.toast(e.message, "error");
    }
  }

  async function adminReportDetails() {
    const ctx = await boot(["Administrator"], "Report details", "");
    if (!ctx) return;
    const id = SR.utils.qs("id");
    if (!id) {
      ctx.body.innerHTML = SR.ui.empty("Missing id.");
      return;
    }
    try {
      const r = await SR.api.get(`/api/admin/job-reports/${id}`);
      const reportedBy =
        r.reportedByUserId ??
        r.ReportedByUserId ??
        r.reportedBy ??
        r.ReportedBy ??
        null;
      const details = r.description || r.Description || r.details || "";
      const vacancyId = r.vacancyId ?? r.VacancyId ?? "—";
      ctx.body.innerHTML = `
        <a class="back-link" href="/module5-trust/admin-reported-jobs.html">← Back to reported jobs</a>
        <div class="card" style="margin-top:0.75rem">
          <h2>Report #${SR.utils.escape(r.id ?? id)}</h2>
          <div class="meta" style="margin-top:0.7rem">
            ${SR.ui.badge(SR.status.reportStatus(r.status ?? r.Status), SR.status.reportKind(r.status ?? r.Status))}
            <span>${SR.utils.escape(SR.status.reportReason(r.reason ?? r.Reason))}</span>
            ${r.reportedAt || r.createdAt ? `<span>${SR.utils.formatDate(r.reportedAt || r.createdAt)}</span>` : ""}
            ${r.reviewedAt ? `<span>Reviewed ${SR.utils.formatDate(r.reviewedAt)}</span>` : ""}
          </div>
          <div class="form-grid two" style="margin-top:1rem">
            <div><strong>Report ID</strong><p class="muted">${SR.utils.escape(r.id ?? id)}</p></div>
            <div><strong>Vacancy ID</strong><p class="muted">${SR.utils.escape(vacancyId)}</p></div>
            <div><strong>Reason</strong><p class="muted">${SR.utils.escape(SR.status.reportReason(r.reason ?? r.Reason))}</p></div>
            <div><strong>Status</strong><p class="muted">${SR.utils.escape(SR.status.reportStatus(r.status ?? r.Status))}</p></div>
            ${
              reportedBy != null
                ? `<div><strong>Reported by</strong><p class="muted">User #${SR.utils.escape(reportedBy)}</p></div>`
                : ""
            }
          </div>
          ${
            details
              ? `<h3 style="margin:1rem 0 0.45rem">Details</h3><p>${SR.utils.escape(details)}</p>`
              : `<p class="muted" style="margin-top:1rem">No additional details provided.</p>`
          }
          <div class="row-actions" style="margin-top:1rem">
            <a class="btn btn-outline" href="/module5-trust/admin-reported-jobs.html">Back</a>
          </div>
        </div>`;
    } catch (e) {
      ctx.body.innerHTML = SR.ui.errorState(e, {
        cta: { href: "/module5-trust/admin-reported-jobs.html", label: "Back to list" },
      });
    }
  }

  async function audit() {
    const ctx = await boot(["Administrator"], "Moderation Audit", "Read-only history of admin actions.");
    if (!ctx) return;
    try {
      const list = await SR.api.get("/api/admin/moderation/audit");
      const items = Array.isArray(list) ? list : [];
      ctx.body.innerHTML = `
        <div class="card table-wrap table-as-cards">
          <table class="data">
            <thead>
              <tr>
                <th>Admin</th><th>Action</th><th>Target</th><th>Target ID</th>
                <th>Reason</th><th>Previous</th><th>New</th><th>Date</th>
              </tr>
            </thead>
            <tbody>
              ${
                items.length
                  ? items
                      .map(
                        (a) => `
                <tr>
                  <td class="cell-strong" data-label="Admin">${SR.utils.escape(a.adminUserId ?? a.administratorId ?? "—")}</td>
                  <td data-label="Action">${SR.ui.badge(SR.status.moderationAction(a.actionType || a.action), "neutral")}</td>
                  <td data-label="Target">${SR.utils.escape(a.targetEntity || a.entityType || "—")}</td>
                  <td class="cell-id" data-label="Target ID">${SR.utils.escape(a.targetId ?? a.entityId ?? "—")}</td>
                  <td class="cell-muted" data-label="Reason">${SR.utils.escape(a.decisionNote || a.reason || "—")}</td>
                  <td data-label="Previous">${SR.utils.escape(a.previousValue ?? "—")}</td>
                  <td data-label="New">${SR.utils.escape(a.newValue ?? "—")}</td>
                  <td class="cell-muted" data-label="Date">${SR.utils.formatDate(a.createdAt || a.actionDate)}</td>
                </tr>`
                      )
                      .join("")
                  : `<tr><td colspan="8">${SR.ui.empty("No moderation actions yet.", {
                      detail: "Warn, suspend, disable, and vacancy-close actions will be listed here.",
                    })}</td></tr>`
              }
            </tbody>
          </table>
        </div>`;
    } catch (e) {
      SR.ui.toast(e.message, "error");
    }
  }

  document.addEventListener("DOMContentLoaded", () => {
    if (path.includes("/notifications.html")) return notifications();
    if (path.includes("/job-reports.html") && path.includes("module5")) return seekerReports();
    if (path.includes("admin-reported-jobs")) return adminReports();
    if (path.includes("admin-job-report-details")) return adminReportDetails();
    if (path.includes("moderation-audit")) return audit();
  });

  return { unreadCount, openReportModal };
})();
