window.SR = window.SR || {};

SR.module3 = (function () {
  const path = location.pathname;

  async function bootEmployer(title, sub, actions) {
    const user = await SR.guards.requireAuth(["Employer"]);
    if (!user) return null;
    SR.ui.mountAppShell(user);
    await SR.ui.mountNotificationBell();
    return { user, body: SR.ui.page(title, sub, actions) };
  }

  async function bootAdmin(title, sub, actions) {
    const user = await SR.guards.requireAuth(["Administrator"]);
    if (!user) return null;
    SR.ui.mountAppShell(user);
    await SR.ui.mountNotificationBell();
    return SR.ui.page(title, sub, actions);
  }

  function vacancyPayload(form, skills) {
    const d = SR.utils.formToObject(form);
    return {
      title: d.title,
      category: d.category,
      employmentType: d.employmentType,
      workLocation: d.workLocation,
      experienceLevel: d.experienceLevel,
      salaryRange: d.salaryRange,
      description: d.description,
      requirements: d.requirements,
      requiredSkills: skills || [],
      expiryDate: new Date(d.expiryDate).toISOString(),
    };
  }

  async function employerDashboard() {
    const ctx = await bootEmployer("Employer dashboard", "Hiring overview.");
    if (!ctx) return;
    try {
      const d = await SR.api.get("/api/employers/me/dashboard");
      ctx.body.innerHTML = `
        <div class="grid-4">
          <div class="card stat-card"><strong>${d.totalVacancies ?? 0}</strong><span>Total</span></div>
          <div class="card stat-card"><strong>${d.openVacancies ?? 0}</strong><span>Open</span></div>
          <div class="card stat-card"><strong>${d.pendingVacancies ?? 0}</strong><span>Pending</span></div>
          <div class="card stat-card"><strong>${d.closedVacancies ?? 0}</strong><span>Closed</span></div>
        </div>
        <div class="row-actions" style="margin-top:1rem">
          <a class="btn btn-primary" href="/module3-employer/vacancy-form.html">Create Vacancy</a>
          <a class="btn btn-outline" href="/module3-employer/vacancies.html">Manage Vacancies</a>
          <a class="btn btn-outline" href="/module3-employer/company-profile.html">Company Profile</a>
          <a class="btn btn-outline" href="/module3-employer/verification.html">Verification</a>
          <a class="btn btn-outline" href="/module4-applications/employer-applicants.html">Applicants</a>
          <a class="btn btn-outline" href="/module4-applications/employer-contact-requests.html">Contact Requests</a>
          <a class="btn btn-outline" href="/module4-applications/employer-interviews.html">Interviews</a>
        </div>`;
    } catch (e) {
      SR.ui.toast(e.message, "error");
    }
  }

  async function companyProfile() {
    const ctx = await bootEmployer("Company profile", "Company details used across hiring.");
    if (!ctx) return;
    let p = {};
    let exists = true;
    try {
      p = await SR.api.get("/api/employers/me/profile");
    } catch (e) {
      if (e.status === 404) exists = false;
      else {
        SR.ui.toast(e.message, "error");
        return;
      }
    }
    const v = (k) => SR.utils.escape(p[k] || "");
    ctx.body.innerHTML = `
      ${!exists ? `<div class="card onboard-banner" style="margin-bottom:1rem"><strong>Complete onboarding</strong><p class="muted">Create your company profile to continue.</p></div>` : ""}
      <form id="company-form" class="card form-grid">
        <div class="form-grid two">
          <label class="field">Company name<input name="companyName" value="${v("companyName")}" required /></label>
          <label class="field">Registered name<input name="registeredCompanyName" value="${v("registeredCompanyName")}" /></label>
          <label class="field">Registration number<input name="registrationNumber" value="${v("registrationNumber")}" /></label>
          <label class="field">Industry<input name="industry" value="${v("industry")}" /></label>
          <label class="field">Registered address<input name="registeredAddress" value="${v("registeredAddress")}" /></label>
          <label class="field">Operating location<input name="operatingLocation" value="${v("operatingLocation")}" /></label>
          <label class="field">Official email<input name="officialCompanyEmail" type="email" value="${v("officialCompanyEmail")}" /></label>
          <label class="field">Phone<input name="companyPhone" value="${v("companyPhone")}" /></label>
          <label class="field">Website<input name="website" value="${v("website")}" /></label>
          <label class="field">Authorized representative<input name="authorizedRepresentative" value="${v("authorizedRepresentative")}" /></label>
        </div>
        <label class="field">Company description<textarea name="companyDescription">${v("companyDescription")}</textarea></label>
        <button class="btn btn-primary" type="submit">${exists ? "Update profile" : "Create profile"}</button>
      </form>`;
    document.getElementById("company-form").onsubmit = async (e) => {
      e.preventDefault();
      const body = SR.utils.formToObject(e.target);
      const btn = e.target.querySelector("[type=submit]");
      try {
        SR.ui.setLoading(btn, true, "Saving…");
        if (exists) await SR.api.put("/api/employers/me/profile", body);
        else await SR.api.post("/api/employers/me/profile", body);
        SR.ui.toast("Profile saved");
        location.reload();
      } catch (err) {
        SR.ui.toast(err.message, "error");
      } finally {
        SR.ui.setLoading(btn, false);
      }
    };
  }

  async function verification() {
    const ctx = await bootEmployer("Employer verification", "Upload documents and submit for review.");
    if (!ctx) return;
    let v = null;
    try {
      v = await SR.api.get("/api/employer-verification");
    } catch {}
    const docs = v?.documents || v?.Documents || [];
    ctx.body.innerHTML = `
      <div class="grid-2">
        <div class="card">
          <p>Status: ${SR.ui.badge(SR.status.verification(v?.status ?? v?.Status ?? 1), SR.status.verificationKind(v?.status ?? v?.Status ?? 1))}</p>
          ${v?.adminFeedback || v?.feedback ? `<p class="muted" style="margin-top:0.6rem">${SR.utils.escape(v.adminFeedback || v.feedback)}</p>` : ""}
          <p class="muted" style="margin-top:0.8rem;font-size:0.88rem">Guidance: Business Registration Certificate, Proof of Business Address, Authorized Representative Document. Current upload API accepts a file only (no document type field).</p>
          <div class="row-actions">
            <button class="btn btn-primary" id="submit-ver" type="button">Submit</button>
            <button class="btn btn-outline" id="withdraw-ver" type="button">Withdraw</button>
            <button class="btn btn-outline" id="resubmit-ver" type="button">Resubmit</button>
          </div>
          <form id="doc-upload" class="form-grid" style="margin-top:1rem">
            <label class="field">Upload document (PDF/JPG/PNG, max 5MB)
              <input name="file" type="file" accept=".pdf,.jpg,.jpeg,.png" required />
            </label>
            <button class="btn btn-navy" type="submit">Upload</button>
          </form>
        </div>
        <div class="card">
          <h3>Documents</h3>
          <div class="list" style="margin-top:0.8rem">
            ${
              docs.length
                ? docs
                    .map(
                      (d) => `
              <div class="list-item">
                <h3>${SR.utils.escape(d.fileName || `Document #${d.id}`)}</h3>
                <div class="meta">${SR.ui.badge(SR.status.document(d.status), "neutral")}</div>
                <button class="btn btn-danger btn-sm" data-del="${d.id}" type="button">Remove</button>
              </div>`
                    )
                    .join("")
                : SR.ui.empty("No documents uploaded.")
            }
          </div>
        </div>
      </div>`;

    const act = async (fn, msg) => {
      try {
        await fn();
        SR.ui.toast(msg);
        location.reload();
      } catch (e) {
        SR.ui.toast(e.message, "error");
      }
    };
    document.getElementById("submit-ver").onclick = () =>
      act(() => SR.api.post("/api/employer-verification/submit"), "Submitted");
    document.getElementById("withdraw-ver").onclick = () =>
      act(() => SR.api.patch("/api/employer-verification/withdraw"), "Withdrawn");
    document.getElementById("resubmit-ver").onclick = () =>
      act(() => SR.api.post("/api/employer-verification/resubmit"), "Resubmitted");
    document.getElementById("doc-upload").onsubmit = async (e) => {
      e.preventDefault();
      const file = e.target.file.files[0];
      if (file.size > 5 * 1024 * 1024) {
        SR.ui.toast("Max 5 MB", "error");
        return;
      }
      const fd = new FormData();
      fd.append("file", file);
      await act(
        () => SR.api.request("POST", "/api/employer-verification/document/upload", null, { formData: fd }),
        "Uploaded"
      );
    };
    document.querySelectorAll("[data-del]").forEach((btn) => {
      btn.onclick = () =>
        act(() => SR.api.delete(`/api/employer-verification/document/${btn.dataset.del}`), "Removed");
    });
  }

  async function vacancies() {
    const ctx = await bootEmployer(
      "Vacancies",
      "Create, submit and manage roles.",
      `<a class="btn btn-primary" href="/module3-employer/vacancy-form.html">New vacancy</a>`
    );
    if (!ctx) return;
    try {
      const list = await SR.api.get("/api/employer/vacancies");
      const items = Array.isArray(list) ? list : [];
      ctx.body.innerHTML = `<div class="list">${
        items.length
          ? items
              .map((v) => {
                const label = SR.status.vacancy(v.status);
                const actions = [];
                if (label === "Draft") {
                  actions.push(`<a class="btn btn-outline btn-sm" href="/module3-employer/vacancy-form.html?id=${v.id}">Edit</a>`);
                  actions.push(`<button class="btn btn-primary btn-sm" data-submit="${v.id}" type="button">Submit</button>`);
                } else if (label === "Rejected") {
                  actions.push(`<a class="btn btn-outline btn-sm" href="/module3-employer/vacancy-form.html?id=${v.id}">Edit</a>`);
                  actions.push(`<button class="btn btn-primary btn-sm" data-submit="${v.id}" type="button">Resubmit</button>`);
                } else if (label === "Open") {
                  actions.push(`<button class="btn btn-danger btn-sm" data-close="${v.id}" type="button">Close</button>`);
                  actions.push(`<a class="btn btn-outline btn-sm" href="/module4-applications/employer-applicants.html?vacancyId=${v.id}">View Applicants</a>`);
                }
                actions.push(`<a class="btn btn-outline btn-sm" href="/module3-employer/vacancy-details.html?id=${v.id}">View</a>`);
                return `<div class="list-item">
                  <h3>${SR.utils.escape(v.title)}</h3>
                  <div class="meta">${SR.ui.badge(label, SR.status.vacancyKind(v.status))}<span>${SR.utils.escape(v.workLocation)}</span><span>Expires ${SR.utils.formatDate(v.expiryDate)}</span></div>
                  <div class="vacancy-actions row-actions">${actions.join("")}</div>
                </div>`;
              })
              .join("")
          : SR.ui.empty("No vacancies yet.")
      }</div>`;

      document.querySelectorAll("[data-submit]").forEach((btn) => {
        btn.onclick = async () => {
          try {
            await SR.api.post(`/api/employer/vacancies/${btn.dataset.submit}/submit`);
            SR.ui.toast("Submitted");
            location.reload();
          } catch (e) {
            SR.ui.toast(e.message, "error");
          }
        };
      });
      document.querySelectorAll("[data-close]").forEach((btn) => {
        btn.onclick = () => {
          SR.ui.confirmAction({
            title: "Close vacancy",
            message: "Close this vacancy? Applicants will remain.",
            confirmText: "Close",
            onConfirm: async () => {
              await SR.api.patch(`/api/employer/vacancies/${btn.dataset.close}/close`);
              SR.ui.toast("Closed");
              location.reload();
            },
          });
        };
      });
    } catch (e) {
      SR.ui.toast(e.message, "error");
    }
  }

  async function vacancyForm() {
    const ctx = await bootEmployer("Vacancy form", "Draft a role for publication.");
    if (!ctx) return;
    const id = SR.utils.qs("id");
    let v = {};
    if (id) {
      try {
        v = await SR.api.get(`/api/employer/vacancies/${id}`);
      } catch (e) {
        SR.ui.toast(e.message, "error");
      }
    }
    const expiry = v.expiryDate ? new Date(v.expiryDate).toISOString().slice(0, 16) : "";
    const esc = SR.utils.escape;
    let skills = [...(v.requiredSkills || v.RequiredSkills || [])];

    const renderSkills = () => {
      const row = document.getElementById("skill-chips");
      if (!row) return;
      row.innerHTML = skills
        .map(
          (s, i) =>
            `<span class="chip">${esc(s)} <button type="button" data-i="${i}" aria-label="Remove">&times;</button></span>`
        )
        .join("");
      row.querySelectorAll("button").forEach((btn) => {
        btn.onclick = () => {
          skills.splice(Number(btn.dataset.i), 1);
          renderSkills();
        };
      });
    };

    ctx.body.innerHTML = `
      <form id="vacancy-form" class="card form-grid">
        <label class="field">Title <span class="req">*</span><input name="title" required maxlength="200" value="${esc(v.title || "")}" /></label>
        <div class="form-grid two">
          <label class="field">Category<input name="category" value="${esc(v.category || "")}" /></label>
          <label class="field">Employment type<input name="employmentType" value="${esc(v.employmentType || "")}" /></label>
          <label class="field">Work location<input name="workLocation" value="${esc(v.workLocation || "")}" /></label>
          <label class="field">Experience level<input name="experienceLevel" value="${esc(v.experienceLevel || "")}" /></label>
          <label class="field">Salary range<input name="salaryRange" value="${esc(v.salaryRange || "")}" /></label>
          <label class="field">Expiry <span class="req">*</span><input name="expiryDate" type="datetime-local" required value="${esc(expiry)}" /></label>
        </div>
        <label class="field">Description <span class="req">*</span><textarea name="description" required>${esc(v.description || "")}</textarea></label>
        <label class="field">Requirements<textarea name="requirements">${esc(v.requirements || "")}</textarea></label>
        <div>
          <label class="field">Required skills</label>
          <div class="chip-row" id="skill-chips" style="margin:0.5rem 0"></div>
          <div class="skill-input-row">
            <input id="skill-input" placeholder="Type a skill and press Enter" />
            <button type="button" class="btn btn-outline" id="add-skill">Add</button>
          </div>
          <p class="muted" style="margin-top:0.45rem;font-size:0.85rem">Skill tags power match scores for job seekers.</p>
        </div>
        <button class="btn btn-primary" type="submit">${id ? "Save changes" : "Create draft"}</button>
      </form>`;

    renderSkills();
    const addSkill = () => {
      const input = document.getElementById("skill-input");
      const val = (input.value || "").trim();
      if (!val) return;
      if (!skills.some((s) => s.toLowerCase() === val.toLowerCase())) skills.push(val);
      input.value = "";
      renderSkills();
    };
    document.getElementById("add-skill").onclick = addSkill;
    document.getElementById("skill-input").addEventListener("keydown", (ev) => {
      if (ev.key === "Enter") {
        ev.preventDefault();
        addSkill();
      }
    });

    document.getElementById("vacancy-form").onsubmit = async (e) => {
      e.preventDefault();
      const btn = e.target.querySelector("[type=submit]");
      try {
        SR.ui.setLoading(btn, true, "Saving…");
        const body = vacancyPayload(e.target, skills);
        if (id) {
          await SR.api.put(`/api/employer/vacancies/${id}`, body);
          SR.ui.toast("Saved");
          location.href = `/module3-employer/vacancy-details.html?id=${id}`;
        } else {
          const created = await SR.api.post("/api/employer/vacancies", body);
          const newId = created.vacancyId || created.VacancyId;
          SR.ui.toast("Created");
          location.href = newId
            ? `/module3-employer/vacancy-details.html?id=${newId}`
            : "/module3-employer/vacancies.html";
        }
      } catch (err) {
        SR.ui.toast(err.message, "error");
      } finally {
        SR.ui.setLoading(btn, false);
      }
    };
  }

  async function vacancyDetails() {
    const ctx = await bootEmployer("Vacancy details", "");
    if (!ctx) return;
    const id = SR.utils.qs("id");
    if (!id) {
      ctx.body.innerHTML = SR.ui.empty("Missing id.");
      return;
    }
    try {
      const v = await SR.api.get(`/api/employer/vacancies/${id}`);
      document.querySelector(".page-head h1").textContent = v.title;
      document.querySelector(".page-head p").textContent = SR.status.vacancy(v.status);
      ctx.body.innerHTML = `
        <div class="card">
          <div class="meta">${SR.ui.badge(SR.status.vacancy(v.status), SR.status.vacancyKind(v.status))}
            <span>${SR.utils.escape(v.workLocation)}</span>
            <span>${SR.utils.escape(v.employmentType)}</span>
            <span>Expires ${SR.utils.formatDate(v.expiryDate)}</span>
          </div>
          <p style="margin-top:0.9rem">${SR.utils.escape(v.description)}</p>
          <h3 style="margin:1rem 0 0.45rem">Requirements</h3>
          <p class="muted">${SR.utils.escape(v.requirements || "—")}</p>
          <h3 style="margin:1rem 0 0.45rem">Required skills</h3>
          <div class="chip-row">${(v.requiredSkills || [])
            .map((s) => `<span class="chip">${SR.utils.escape(s)}</span>`)
            .join("") || "—"}</div>
          <div class="row-actions">
            <a class="btn btn-outline" href="/module3-employer/vacancy-form.html?id=${id}">Edit</a>
            <a class="btn btn-outline" href="/module4-applications/employer-applicants.html?vacancyId=${id}">Applicants</a>
            <a class="btn btn-outline" href="/module3-employer/vacancies.html">Back</a>
          </div>
        </div>`;
    } catch (e) {
      SR.ui.toast(e.message, "error");
    }
  }

  async function adminVerifications() {
    const body = await bootAdmin("Employer verifications", "Review company verification requests.");
    if (!body) return;
    try {
      const list = await SR.api.get("/api/admin/employer-verifications");
      const items = Array.isArray(list) ? list : [];
      body.innerHTML = `<div class="list">${
        items.length
          ? items
              .map(
                (v) => `
          <div class="list-item" data-id="${v.id}">
            <h3>Verification #${v.id}</h3>
            <div class="meta">${SR.ui.badge(SR.status.verification(v.status), SR.status.verificationKind(v.status))}
              <span>${SR.utils.escape(v.companyName || "")}</span></div>
            <div class="row-actions">
              <a class="btn btn-outline btn-sm" href="/module3-employer/admin-employer-verification-details.html?id=${v.id}">Details</a>
              <button class="btn btn-primary btn-sm" data-verify type="button">Verify</button>
              <button class="btn btn-outline btn-sm" data-info type="button">Request info</button>
              <button class="btn btn-danger btn-sm" data-reject type="button">Reject</button>
            </div>
          </div>`
              )
              .join("")
          : SR.ui.empty("No verification records.")
      }</div>`;

      body.querySelectorAll(".list-item").forEach((item) => {
        const id = item.dataset.id;
        item.querySelector("[data-verify]").onclick = async () => {
          try {
            await SR.api.patch(`/api/admin/employer-verifications/${id}/verify`);
            SR.ui.toast("Verified");
            location.reload();
          } catch (e) {
            SR.ui.toast(e.message, "error");
          }
        };
        item.querySelector("[data-info]").onclick = () => {
          SR.ui.modal({
            title: "Request information",
            bodyHtml: `<label class="field">Feedback<textarea id="fb" required></textarea></label>`,
            confirmText: "Send",
            onConfirm: async (backdrop) => {
              const feedback = backdrop.querySelector("#fb").value.trim();
              if (!feedback) throw new Error("Feedback required");
              await SR.api.patch(`/api/admin/employer-verifications/${id}/request-information`, { feedback });
              SR.ui.toast("Feedback sent");
              location.reload();
            },
          });
        };
        item.querySelector("[data-reject]").onclick = () => {
          SR.ui.modal({
            title: "Reject verification",
            bodyHtml: `<label class="field">Feedback<textarea id="fb" required></textarea></label>`,
            confirmText: "Reject",
            danger: true,
            onConfirm: async (backdrop) => {
              const feedback = backdrop.querySelector("#fb").value.trim();
              if (!feedback) throw new Error("Feedback required");
              await SR.api.patch(`/api/admin/employer-verifications/${id}/reject`, { feedback });
              SR.ui.toast("Rejected");
              location.reload();
            },
          });
        };
      });
    } catch (e) {
      SR.ui.toast(e.message, "error");
    }
  }

  async function adminVerificationDetails() {
    const body = await bootAdmin("Verification details", "");
    if (!body) return;
    const id = SR.utils.qs("id");
    if (!id) {
      body.innerHTML = SR.ui.empty("Missing id.");
      return;
    }
    try {
      const v = await SR.api.get(`/api/admin/employer-verifications/${id}`);
      // Intentionally ignore any filePath fields — no safe download endpoint.
      const safe = { ...v };
      if (safe.documents) {
        safe.documents = safe.documents.map((d) => {
          const copy = { ...d };
          delete copy.filePath;
          delete copy.FilePath;
          return copy;
        });
      }
      body.innerHTML = `<div class="card"><pre style="white-space:pre-wrap;font:inherit;margin:0">${SR.utils.escape(
        JSON.stringify(safe, null, 2)
      )}</pre>
        <div class="row-actions"><a class="btn btn-outline" href="/module3-employer/admin-employer-verifications.html">Back</a></div></div>`;
    } catch (e) {
      SR.ui.toast(e.message, "error");
    }
  }

  async function adminPendingVacancies() {
    const body = await bootAdmin("Pending vacancies", "Approve or reject employer submissions.");
    if (!body) return;
    try {
      const list = await SR.api.get("/api/admin/vacancies/pending");
      const items = Array.isArray(list) ? list : [];
      body.innerHTML = `<div class="list">${
        items.length
          ? items
              .map(
                (v) => `
          <div class="list-item" data-id="${v.id}">
            <h3>${SR.utils.escape(v.title || v.jobTitle || `Vacancy #${v.id}`)}</h3>
            <p class="muted">${SR.utils.escape((v.description || "").slice(0, 220))}</p>
            <div class="row-actions">
              <button class="btn btn-primary btn-sm" data-approve type="button">Approve</button>
              <button class="btn btn-danger btn-sm" data-reject type="button">Reject</button>
            </div>
          </div>`
              )
              .join("")
          : SR.ui.empty("No pending approvals.")
      }</div>`;

      body.querySelectorAll(".list-item").forEach((item) => {
        const id = item.dataset.id;
        item.querySelector("[data-approve]").onclick = async () => {
          try {
            await SR.api.post(`/api/admin/vacancies/${id}/approve`);
            SR.ui.toast("Approved");
            location.reload();
          } catch (e) {
            SR.ui.toast(e.message, "error");
          }
        };
        item.querySelector("[data-reject]").onclick = () => {
          SR.ui.modal({
            title: "Reject vacancy",
            bodyHtml: `<label class="field">Reason<textarea id="reason" required></textarea></label>`,
            confirmText: "Reject",
            danger: true,
            onConfirm: async (backdrop) => {
              const reason = backdrop.querySelector("#reason").value.trim();
              if (!reason) throw new Error("Reason required");
              await SR.api.post(`/api/admin/vacancies/${id}/reject`, { reason });
              SR.ui.toast("Rejected");
              location.reload();
            },
          });
        };
      });
    } catch (e) {
      SR.ui.toast(e.message, "error");
    }
  }

  document.addEventListener("DOMContentLoaded", () => {
    if (path.includes("employer-dashboard")) return employerDashboard();
    if (path.includes("company-profile")) return companyProfile();
    if (path.includes("/verification.html")) return verification();
    if (path.includes("/vacancies.html")) return vacancies();
    if (path.includes("vacancy-form")) return vacancyForm();
    if (path.includes("vacancy-details")) return vacancyDetails();
    if (path.includes("admin-employer-verifications.html")) return adminVerifications();
    if (path.includes("admin-employer-verification-details")) return adminVerificationDetails();
    if (path.includes("admin-pending-vacancies")) return adminPendingVacancies();
  });

  return {};
})();
