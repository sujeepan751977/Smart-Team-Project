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
        SR.ui.toast("View My Applications", "info");
        const btn = document.getElementById("apply-btn");
        if (btn) {
          btn.textContent = "Applied";
          btn.disabled = true;
          if (!document.getElementById("view-apps-link")) {
            const link = document.createElement("a");
            link.id = "view-apps-link";
            link.className = "btn btn-outline btn-sm";
            link.href = "/module4-applications/jobseeker-applications.html";
            link.textContent = "View My Applications";
            btn.insertAdjacentElement("afterend", link);
          }
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
      const counts = [
        { key: "Applied", label: SR.status.application("Applied") },
        { key: "UnderReview", label: SR.status.application("UnderReview") },
        { key: "Shortlisted", label: SR.status.application("Shortlisted") },
        { key: "Rejected", label: SR.status.application("Rejected") },
      ].map((c) => ({
        ...c,
        count: list.filter((a) => a.status === c.key).length,
      }));
      ctx.body.innerHTML = `
        <div class="summary-row">
          ${counts
            .map(
              (c) =>
                `<div class="card stat-card"><strong>${c.count}</strong><span>${SR.utils.escape(c.label)}</span></div>`
            )
            .join("")}
        </div>
        <div class="card filter-bar" style="margin-bottom:1rem">
          <div class="filter-toolbar">
            <label class="filter-select">
              <span class="field-label">Status</span>
              <select id="status-filter">
                <option value="">All</option>
                <option value="Applied">${SR.status.application("Applied")}</option>
                <option value="UnderReview">${SR.status.application("UnderReview")}</option>
                <option value="Shortlisted">${SR.status.application("Shortlisted")}</option>
                <option value="Rejected">${SR.status.application("Rejected")}</option>
              </select>
            </label>
          </div>
        </div>
        <div id="apps-list" class="list"></div>`;

      const render = async () => {
        const filter = document.getElementById("status-filter").value;
        const filtered = filter ? list.filter((a) => a.status === filter) : list;
        const box = document.getElementById("apps-list");
        if (!filtered.length) {
          box.innerHTML = list.length
            ? SR.ui.empty("No applications match this filter.")
            : SR.ui.empty("No applications yet.", {
                detail: "Browse open roles and apply when you find a good match.",
                cta: { href: "/module2-jobseeker/jobs.html", label: "Find Jobs" },
              });
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
            <div class="list-item list-item-with-score">
              <div class="list-item-main">
                <h3>${SR.utils.escape(title)}</h3>
                <div class="meta">
                  ${SR.ui.badge(SR.status.application(a.status), SR.status.applicationKind(a.status))}
                  <span>${SR.utils.formatDate(a.appliedAt)}</span>
                </div>
                ${a.coverLetter ? `<p class="muted" style="margin-top:0.5rem">${SR.utils.escape(a.coverLetter)}</p>` : ""}
              </div>
              ${SR.utils.matchSide(a.matchScore ?? a.MatchScore ?? 0)}
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
          : SR.ui.empty("No contact requests.", {
              detail: "When employers request contact after you apply, they will appear here.",
              cta: { href: "/module2-jobseeker/jobs.html", label: "Find Jobs" },
            })
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
          : SR.ui.empty("No interviews scheduled.", {
              detail: "Interview invites from employers will show up here after you apply.",
              cta: {
                href: "/module4-applications/jobseeker-applications.html",
                label: "My Applications",
              },
            })
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
      const vacancies = await SR.api.get("/api/employer/vacancies");
      const items = Array.isArray(vacancies) ? vacancies : [];
      if (!items.length) {
        ctx.body.innerHTML = SR.ui.empty("No vacancies yet.", {
          detail: "Create a vacancy first, then review applicants here.",
          cta: { href: "/module3-employer/vacancies.html", label: "Go to Vacancies" },
        });
        return;
      }

      if (!vacancyId) {
        ctx.body.innerHTML = `
          <div class="list">
            ${items
              .map((v) => {
                const id = v.id ?? v.Id;
                const title = v.title || v.Title || `Vacancy #${id}`;
                const status = v.status ?? v.Status;
                const location = v.workLocation || v.WorkLocation || "";
                return `
              <a class="list-item" href="/module4-applications/employer-applicants.html?vacancyId=${id}" style="text-decoration:none;color:inherit;display:block">
                <h3>${SR.utils.escape(title)}</h3>
                <div class="meta">
                  ${SR.ui.badge(SR.status.vacancy(status), SR.status.vacancyKind(status))}
                  ${location ? `<span>${SR.utils.escape(location)}</span>` : ""}
                  <span>View applicants →</span>
                </div>
              </a>`;
              })
              .join("")}
          </div>`;
        return;
      }

      const selected = items.find((v) => String(v.id ?? v.Id) === String(vacancyId));
      const selectedTitle = selected?.title || selected?.Title || `Vacancy #${vacancyId}`;
      const apps = await SR.api.get(`/api/employer/vacancies/${vacancyId}/applications`);
      const list = (Array.isArray(apps) ? apps : [])
        .slice()
        .sort((a, b) => (b.matchScore ?? b.MatchScore ?? 0) - (a.matchScore ?? a.MatchScore ?? 0));

      const statusVal = (a) => SR.status.application(a.status ?? a.Status);
      const selectedOpt = (a, key) => (statusVal(a) === SR.status.application(key) ? "selected" : "");

      ctx.body.innerHTML = `
        <div class="card" style="margin-bottom:1rem">
          <div class="meta" style="align-items:center;justify-content:space-between;flex-wrap:wrap;gap:0.65rem">
            <strong>${SR.utils.escape(selectedTitle)}</strong>
            <span class="muted" style="font-size:0.88rem">${list.length} applicant${list.length === 1 ? "" : "s"} · sorted by match score</span>
            <a class="btn btn-outline btn-sm" href="/module4-applications/employer-applicants.html">Change vacancy</a>
          </div>
        </div>
        <div class="list">${
        list.length
          ? list
              .map((a) => {
                const id = a.id ?? a.Id;
                const profileId = a.jobSeekerProfileId ?? a.JobSeekerProfileId;
                const name = a.candidateName || a.CandidateName || `Candidate #${profileId}`;
                const match = a.matchScore ?? a.MatchScore ?? 0;
                const appliedAt = a.appliedAt || a.AppliedAt;
                const cover = a.coverLetter || a.CoverLetter || "";
                const st = a.status ?? a.Status;
                const hasCv = a.hasCv ?? a.HasCv;
                const cvName = a.cvFileName || a.CvFileName || "CV";
                return `
          <div class="list-item list-item-with-score" data-id="${id}">
            <div class="list-item-main">
              <h3>${SR.utils.escape(name)}</h3>
              <div class="meta">
                ${SR.ui.badge(SR.status.application(st), SR.status.applicationKind(st))}
                ${SR.utils.matchPill(match)}
                <span>${SR.utils.formatDate(appliedAt)}</span>
                ${hasCv ? `<span>${SR.utils.escape(cvName)}</span>` : `<span class="muted">No CV</span>`}
              </div>
              ${cover ? `<p class="muted" style="margin-top:0.5rem">${SR.utils.escape(cover)}</p>` : ""}
              <div class="row-actions">
                <select data-status>
                  <option value="UnderReview" ${selectedOpt(a, "UnderReview")}>${SR.status.application("UnderReview")}</option>
                  <option value="Shortlisted" ${selectedOpt(a, "Shortlisted")}>${SR.status.application("Shortlisted")}</option>
                  <option value="Rejected" ${selectedOpt(a, "Rejected")}>${SR.status.application("Rejected")}</option>
                </select>
                <button class="btn btn-primary btn-sm" data-save type="button">Update status</button>
                ${
                  hasCv
                    ? `<button class="btn btn-outline btn-sm" data-cv type="button">Download CV</button>`
                    : ""
                }
                <button class="btn btn-outline btn-sm" data-contact type="button">Request contact</button>
                <button class="btn btn-navy btn-sm" data-interview type="button">Schedule interview</button>
                <a class="btn btn-outline btn-sm" href="/module4-applications/employer-application-details.html?id=${id}&vacancyId=${vacancyId}">Details</a>
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
            </div>
            ${SR.utils.matchSide(match)}
          </div>`;
              })
              .join("")
          : SR.ui.empty("No applicants yet.", {
              detail: "Share your open vacancies or check back after candidates apply.",
              cta: { href: "/module3-employer/vacancies.html", label: "Manage Vacancies" },
            })
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
        item.querySelector("[data-cv]")?.addEventListener("click", async () => {
          try {
            await SR.api.download(`/api/employer/applications/${id}/cv`, "applicant-cv.pdf");
            SR.ui.toast("CV downloaded");
          } catch (e) {
            SR.ui.toast(e.message, "error");
          }
        });
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
      ctx.body.innerHTML = SR.ui.errorState(e, {
        cta: { href: "/module3-employer/vacancies.html", label: "Go to Vacancies" },
      });
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
      const name = a.candidateName || a.CandidateName || `Candidate #${a.jobSeekerProfileId}`;
      const hasCv = a.hasCv ?? a.HasCv;
      const cvName = a.cvFileName || a.CvFileName || "CV";
      ctx.body.innerHTML = `
        <div class="card">
          <div class="list-item-with-score" style="padding:0;border:0;box-shadow:none;background:transparent">
            <div class="list-item-main">
              <h2>${SR.utils.escape(name)}</h2>
              <div class="meta" style="margin-top:0.6rem">
                ${SR.ui.badge(SR.status.application(a.status), SR.status.applicationKind(a.status))}
                ${SR.utils.matchPill(a.matchScore ?? a.MatchScore ?? 0)}
                <span>Application #${a.id}</span>
                <span>${SR.utils.formatDate(a.appliedAt)}</span>
                ${hasCv ? `<span>${SR.utils.escape(cvName)}</span>` : `<span class="muted">No CV uploaded</span>`}
              </div>
              ${a.coverLetter ? `<p style="margin-top:0.9rem">${SR.utils.escape(a.coverLetter)}</p>` : ""}
              <div class="row-actions">
                ${
                  hasCv
                    ? `<button class="btn btn-primary" id="download-cv" type="button">Download CV</button>`
                    : ""
                }
                <a class="btn btn-outline" href="/module4-applications/employer-applicants.html${vacancyId ? `?vacancyId=${vacancyId}` : ""}">Back</a>
              </div>
            </div>
            ${SR.utils.matchSide(a.matchScore ?? a.MatchScore ?? 0)}
          </div>
        </div>`;
      document.getElementById("download-cv")?.addEventListener("click", async () => {
        try {
          await SR.api.download(`/api/employer/applications/${id}/cv`, cvName);
          SR.ui.toast("CV downloaded");
        } catch (e) {
          SR.ui.toast(e.message, "error");
        }
      });
    } catch (e) {
      SR.ui.toast(e.message, "error");
      ctx.body.innerHTML = SR.ui.errorState(e, {
        cta: {
          href: "/module4-applications/employer-applicants.html",
          label: "Back to applicants",
        },
      });
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
          : SR.ui.empty("No contact requests.", {
              detail: "Request contact from a candidate on the Applicants page.",
              cta: {
                href: "/module4-applications/employer-applicants.html",
                label: "View Applicants",
              },
            })
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
          : SR.ui.empty("No interviews scheduled.", {
              detail: "Schedule interviews from a candidate on the Applicants page.",
              cta: {
                href: "/module4-applications/employer-applicants.html",
                label: "View Applicants",
              },
            })
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
