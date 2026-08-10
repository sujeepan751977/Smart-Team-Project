window.SR = window.SR || {};

SR.module4 = (function () {
  const path = location.pathname;

  async function boot(roles, title, sub, actions) {
    const user = await SR.guards.requireAuth(roles);
    if (!user) return null;
    SR.ui.mountAppShell(user);
    await SR.ui.mountNotificationBell();
    return { user, body: SR.ui.page(title, sub, actions) };
  }

  function openApplyModal(jobId) {
    SR.ui.modal({
      title: "Apply for this job",
      bodyHtml: `<label class="field">Cover letter <span class="hint">optional</span>
        <textarea id="cover" maxlength="1000" placeholder="Optional cover letter"></textarea></label>`,
      confirmText: "Submit application",
      onConfirm: async (backdrop) => {
        const coverLetter = backdrop.querySelector("#cover").value.trim() || null;
        await SR.api.post(`/api/jobs/${jobId}/applications`, { coverLetter });
        SR.ui.toast("Application submitted");
        const btn = document.getElementById("apply-btn");
        if (btn) {
          btn.textContent = "Applied";
          btn.disabled = true;
        }
      },
    });
  }

  async function seekerApplications() {
    const ctx = await boot(["JobSeeker"], "My Applications", "Track every application status.");
    if (!ctx) return;
    try {
      const apps = await SR.api.get("/api/jobseekers/me/applications");
      const list = Array.isArray(apps) ? apps : [];
      const counts = {
        Applied: list.filter((a) => a.status === "Applied").length,
        UnderReview: list.filter((a) => a.status === "UnderReview").length,
        Shortlisted: list.filter((a) => a.status === "Shortlisted").length,
        Rejected: list.filter((a) => a.status === "Rejected").length,
      };
      ctx.body.innerHTML = `
        <div class="summary-row">
          ${Object.entries(counts)
            .map(
              ([k, v]) =>
                `<div class="card stat-card"><strong>${v}</strong><span>${k}</span></div>`
            )
            .join("")}
        </div>
        <div class="card" style="margin-bottom:1rem">
          <label class="field">Filter by status
            <select id="status-filter">
              <option value="">All</option>
              <option>Applied</option>
              <option>UnderReview</option>
              <option>Shortlisted</option>
              <option>Rejected</option>
            </select>
          </label>
        </div>
        <div id="apps-list" class="list"></div>`;

      const render = async () => {
        const filter = document.getElementById("status-filter").value;
        const filtered = filter ? list.filter((a) => a.status === filter) : list;
        const box = document.getElementById("apps-list");
        if (!filtered.length) {
          box.innerHTML = SR.ui.empty("No applications yet.");
          return;
        }
        const cards = [];
        for (const a of filtered) {
          let title = `Vacancy #${a.vacancyId}`;
          try {
            const job = await SR.api.get(`/api/jobs/${a.vacancyId}`);
            title = job.jobTitle || title;
          } catch {
            /* historical vacancy may no longer be Open */
          }
          cards.push(`
            <div class="list-item">
              <h3>${SR.utils.escape(title)}</h3>
              <div class="meta">
                ${SR.ui.badge(a.status, SR.status.applicationKind(a.status))}
                <span>Match ${a.matchScore ?? 0}</span>
                <span>${SR.utils.formatDate(a.appliedAt)}</span>
              </div>
              ${a.coverLetter ? `<p class="muted" style="margin-top:0.5rem">${SR.utils.escape(a.coverLetter)}</p>` : ""}
            </div>`);
        }
        box.innerHTML = cards.join("");
      };
      document.getElementById("status-filter").onchange = render;
      await render();
    } catch (e) {
      SR.ui.toast(e.message, "error");
    }
  }

  async function seekerContacts() {
    const ctx = await boot(["JobSeeker"], "Contact Requests", "Respond when employers reach out.");
    if (!ctx) return;
    try {
      const list = await SR.api.get("/api/jobseekers/me/contact-requests");
      const items = Array.isArray(list) ? list : [];
      ctx.body.innerHTML = `<div class="list">${
        items.length
          ? items
              .map((c) => {
                const pending = String(c.status) === "Pending" || c.status === 1;
                return `
            <div class="list-item" data-id="${c.id}">
              <h3>Request #${c.id}</h3>
              <div class="meta">${SR.ui.badge(SR.status.contact(c.status), SR.status.contactKind(c.status))}
                <span>${SR.utils.formatDate(c.createdAt || c.requestedAt)}</span></div>
              ${c.employerMessage ? `<p style="margin-top:0.5rem">${SR.utils.escape(c.employerMessage)}</p>` : ""}
              ${
                pending
                  ? `<div class="row-actions">
                      <button class="btn btn-primary btn-sm" data-act="Accepted">Accept</button>
                      <button class="btn btn-danger btn-sm" data-act="Rejected">Reject</button>
                    </div>`
                  : ""
              }
            </div>`;
              })
              .join("")
          : SR.ui.empty("No contact requests.")
      }</div>`;

      ctx.body.querySelectorAll("[data-act]").forEach((btn) => {
        btn.onclick = async () => {
          const id = btn.closest("[data-id]").dataset.id;
          try {
            await SR.api.patch(`/api/jobseekers/me/contact-requests/${id}/response`, {
              response: btn.dataset.act,
            });
            SR.ui.toast("Response saved");
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

  async function seekerInterviews() {
    const ctx = await boot(["JobSeeker"], "Interviews", "Your interview schedule.");
    if (!ctx) return;
    try {
      const list = await SR.api.get("/api/jobseekers/me/interviews");
      const items = Array.isArray(list) ? list : [];
      ctx.body.innerHTML = `<div class="list">${
        items.length
          ? items
              .map((i) => {
                const link = i.meetingLink && SR.utils.isHttpUrl(i.meetingLink);
                return `
            <div class="list-item">
              <h3>${SR.utils.escape(i.title)}</h3>
              <div class="meta">
                <span>${SR.utils.formatDate(i.interviewDate)}</span>
                <span>${i.durationMinutes} min</span>
                <span>${SR.utils.escape(i.location || "Remote")}</span>
              </div>
              ${i.instructions ? `<p class="muted" style="margin-top:0.5rem">${SR.utils.escape(i.instructions)}</p>` : ""}
              ${
                link
                  ? `<div class="row-actions"><a class="btn btn-primary btn-sm" href="${SR.utils.escape(
                      i.meetingLink
                    )}" target="_blank" rel="noopener noreferrer">Join Meeting</a></div>`
                  : ""
              }
              <!-- Accept/decline/reschedule not available: InterviewSchedulesController has no such endpoints -->
            </div>`;
              })
              .join("")
          : SR.ui.empty("No interviews scheduled.")
      }</div>`;
    } catch (e) {
      SR.ui.toast(e.message, "error");
    }
  }

  async function employerApplicants() {
    const ctx = await boot(["Employer"], "Applicants", "Review candidates by match score.");
    if (!ctx) return;
    let vacancyId = SR.utils.qs("vacancyId");
    try {
      if (!vacancyId) {
        const vacancies = await SR.api.get("/api/employer/vacancies");
        const items = Array.isArray(vacancies) ? vacancies : [];
        ctx.body.innerHTML = `
          <div class="card form-grid">
            <label class="field">Select vacancy
              <select id="vac-select">
                <option value="">Choose…</option>
                ${items
                  .map((v) => `<option value="${v.id}">${SR.utils.escape(v.title)}</option>`)
                  .join("")}
              </select>
            </label>
          </div>`;
        document.getElementById("vac-select").onchange = (e) => {
          if (e.target.value)
            location.href = `/module4-applications/employer-applicants.html?vacancyId=${e.target.value}`;
        };
        return;
      }

      const apps = await SR.api.get(`/api/employer/vacancies/${vacancyId}/applications`);
      const list = (Array.isArray(apps) ? apps : [])
        .slice()
        .sort((a, b) => (b.matchScore || 0) - (a.matchScore || 0));

      ctx.body.innerHTML = `<div class="list">${
        list.length
          ? list
              .map(
                (a) => `
          <div class="list-item" data-id="${a.id}">
            <h3>Candidate Profile #${a.jobSeekerProfileId}</h3>
            <div class="meta">
              ${SR.ui.badge(a.status, SR.status.applicationKind(a.status))}
              <span>Match ${a.matchScore ?? 0}</span>
              <span>${SR.utils.formatDate(a.appliedAt)}</span>
            </div>
            ${a.coverLetter ? `<p class="muted" style="margin-top:0.5rem">${SR.utils.escape(a.coverLetter)}</p>` : ""}
            <div class="row-actions">
              <select data-status>
                <option value="UnderReview">UnderReview</option>
                <option value="Shortlisted">Shortlisted</option>
                <option value="Rejected">Rejected</option>
              </select>
              <button class="btn btn-primary btn-sm" data-save type="button">Update status</button>
              <button class="btn btn-outline btn-sm" data-contact type="button">Request contact</button>
              <button class="btn btn-navy btn-sm" data-interview type="button">Schedule interview</button>
              <a class="btn btn-outline btn-sm" href="/module4-applications/employer-application-details.html?id=${a.id}&vacancyId=${vacancyId}">Details</a>
            </div>
            <form class="form-grid interview-form" hidden style="margin-top:0.75rem">
              <label class="field">Title<input name="title" required /></label>
              <label class="field">Date<input name="interviewDate" type="datetime-local" required /></label>
              <label class="field">Duration (min)<input name="durationMinutes" type="number" min="15" max="240" value="30" required /></label>
              <label class="field">Location<input name="location" /></label>
              <label class="field">Meeting link<input name="meetingLink" placeholder="https://meet.google.com/..." /></label>
              <label class="field">Instructions<textarea name="instructions"></textarea></label>
              <button class="btn btn-primary btn-sm" type="submit">Save interview</button>
            </form>
          </div>`
              )
              .join("")
          : SR.ui.empty("No applicants yet.")
      }</div>`;

      ctx.body.querySelectorAll(".list-item").forEach((item) => {
        const id = item.dataset.id;
        item.querySelector("[data-save]").onclick = async () => {
          const status = item.querySelector("[data-status]").value;
          try {
            await SR.api.put(`/api/applications/${id}/status`, { status });
            SR.ui.toast("Status updated");
            location.reload();
          } catch (e) {
            SR.ui.toast(e.message, "error");
          }
        };
        item.querySelector("[data-contact]").onclick = () => {
          SR.ui.modal({
            title: "Contact request",
            bodyHtml: `<label class="field">Message<textarea id="msg" maxlength="1000"></textarea></label>`,
            confirmText: "Send",
            onConfirm: async (backdrop) => {
              const employerMessage = backdrop.querySelector("#msg").value.trim() || null;
              await SR.api.post(`/api/employer/applications/${id}/contact-request`, {
                employerMessage,
              });
              SR.ui.toast("Contact request sent");
            },
          });
        };
        item.querySelector("[data-interview]").onclick = () => {
          const form = item.querySelector(".interview-form");
          form.hidden = !form.hidden;
        };
        item.querySelector(".interview-form").onsubmit = async (e) => {
          e.preventDefault();
          const d = SR.utils.formToObject(e.target);
          try {
            await SR.api.post(`/api/employer/applications/${id}/interview`, {
              title: d.title,
              interviewDate: new Date(d.interviewDate).toISOString(),
              durationMinutes: Number(d.durationMinutes),
              location: d.location,
              meetingLink: d.meetingLink,
              instructions: d.instructions,
            });
            SR.ui.toast("Interview scheduled");
            location.href = "/module4-applications/employer-interviews.html";
          } catch (err) {
            SR.ui.toast(err.message, "error");
          }
        };
      });
    } catch (e) {
      SR.ui.toast(e.message, "error");
    }
  }

  async function employerAppDetails() {
    const ctx = await boot(["Employer"], "Application details", "");
    if (!ctx) return;
    const id = SR.utils.qs("id");
    const vacancyId = SR.utils.qs("vacancyId");
    if (!id) {
      ctx.body.innerHTML = SR.ui.empty("Missing id.");
      return;
    }
    try {
      const a = await SR.api.get(`/api/applications/${id}`);
      ctx.body.innerHTML = `
        <div class="card">
          <h2>Application #${a.id}</h2>
          <div class="meta" style="margin-top:0.6rem">
            ${SR.ui.badge(a.status, SR.status.applicationKind(a.status))}
            <span>Candidate Profile #${a.jobSeekerProfileId}</span>
            <span>Match ${a.matchScore ?? 0}</span>
            <span>${SR.utils.formatDate(a.appliedAt)}</span>
          </div>
          ${a.coverLetter ? `<p style="margin-top:0.9rem">${SR.utils.escape(a.coverLetter)}</p>` : ""}
          <p class="muted" style="margin-top:0.8rem">Candidate name/CV are not included in ApplicationDto. Use status and contact request flows.</p>
          <div class="row-actions">
            <a class="btn btn-outline" href="/module4-applications/employer-applicants.html${vacancyId ? `?vacancyId=${vacancyId}` : ""}">Back</a>
          </div>
        </div>`;
    } catch (e) {
      SR.ui.toast(e.message, "error");
    }
  }

  async function employerContacts() {
    const ctx = await boot(["Employer"], "Contact Requests", "Outreach you sent to candidates.");
    if (!ctx) return;
    try {
      const list = await SR.api.get("/api/employer/contact-requests");
      const items = Array.isArray(list) ? list : [];
      ctx.body.innerHTML = `<div class="list">${
        items.length
          ? items
              .map(
                (c) => `
          <div class="list-item">
            <h3>Request #${c.id}</h3>
            <div class="meta">${SR.ui.badge(SR.status.contact(c.status), SR.status.contactKind(c.status))}
              <span>${SR.utils.formatDate(c.createdAt || c.requestedAt)}</span></div>
            ${c.employerMessage ? `<p style="margin-top:0.5rem">${SR.utils.escape(c.employerMessage)}</p>` : ""}
            ${c.jobSeekerResponse ? `<p class="muted">Response: ${SR.utils.escape(c.jobSeekerResponse)}</p>` : ""}
          </div>`
              )
              .join("")
          : SR.ui.empty("No contact requests.")
      }</div>`;
    } catch (e) {
      SR.ui.toast(e.message, "error");
    }
  }

  async function employerInterviews() {
    const ctx = await boot(["Employer"], "Interviews", "Schedules you created.");
    if (!ctx) return;
    try {
      const list = await SR.api.get("/api/employer/interviews");
      const items = Array.isArray(list) ? list : [];
      ctx.body.innerHTML = `<div class="list">${
        items.length
          ? items
              .map((i) => {
                const link = i.meetingLink && SR.utils.isHttpUrl(i.meetingLink);
                return `
            <div class="list-item">
              <h3>${SR.utils.escape(i.title)}</h3>
              <div class="meta">
                <span>${SR.utils.formatDate(i.interviewDate)}</span>
                <span>${i.durationMinutes} min</span>
                <span>${SR.utils.escape(i.location || "Remote")}</span>
              </div>
              ${
                link
                  ? `<div class="row-actions"><a class="btn btn-outline btn-sm" href="${SR.utils.escape(
                      i.meetingLink
                    )}" target="_blank" rel="noopener noreferrer">Open meeting link</a></div>`
                  : ""
              }
              <!-- cancel/complete/reschedule endpoints are not exposed by current InterviewSchedulesController -->
            </div>`;
              })
              .join("")
          : SR.ui.empty("No interviews scheduled.")
      }</div>`;
    } catch (e) {
      SR.ui.toast(e.message, "error");
    }
  }

  document.addEventListener("DOMContentLoaded", () => {
    if (path.includes("jobseeker-applications")) return seekerApplications();
    if (path.includes("jobseeker-contact-requests")) return seekerContacts();
    if (path.includes("jobseeker-interviews")) return seekerInterviews();
    if (path.includes("employer-applicants")) return employerApplicants();
    if (path.includes("employer-application-details")) return employerAppDetails();
    if (path.includes("employer-contact-requests")) return employerContacts();
    if (path.includes("employer-interviews")) return employerInterviews();
  });

  return { openApplyModal };
})();
