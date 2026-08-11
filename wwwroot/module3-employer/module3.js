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
      const verification = await SR.api.get("/api/employer-verification").catch(() => null);
      const vStatus = verification?.status ?? verification?.Status;
      const vLabel = vStatus != null ? SR.status.verification(vStatus) : "Unverified";
      const vKind = vStatus != null ? SR.status.verificationKind(vStatus) : "neutral";
      ctx.body.innerHTML = `
        <div class="dash">
          <div class="card dash-hero">
            <h2>Welcome back, ${SR.utils.escape(ctx.user.fullName || "Employer")}</h2>
            <p>Track vacancy health, verification status, and hiring actions from your workspace.</p>
            <div class="row-actions">
              <a class="btn btn-primary" href="/module3-employer/vacancy-form.html">Create vacancy</a>
              <a class="btn btn-outline" href="/module4-applications/employer-applicants.html">View applicants</a>
            </div>
          </div>
          <div class="dash-panel" style="display:flex;align-items:center;justify-content:space-between;gap:1rem;flex-wrap:wrap">
            <div>
              <h3 style="margin:0">Verification status</h3>
              <p class="muted" style="margin-top:0.3rem;font-size:0.88rem">Keep your company verified to build candidate trust.</p>
            </div>
            <div style="display:flex;align-items:center;gap:0.5rem;flex-wrap:wrap">
              ${SR.ui.badge(vLabel, vKind)}
              <a class="btn btn-outline btn-sm" href="/module3-employer/verification.html">Manage</a>
            </div>
          </div>
          <div class="dash-stats">
            ${SR.ui.dashStat({ value: d.totalVacancies ?? 0, label: "Total vacancies", tone: "navy", iconName: "jobs" })}
            ${SR.ui.dashStat({ value: d.openVacancies ?? 0, label: "Open", tone: "ok", iconName: "verify" })}
            ${SR.ui.dashStat({ value: d.pendingVacancies ?? 0, label: "Pending", tone: "warn", iconName: "audit" })}
            ${SR.ui.dashStat({ value: d.closedVacancies ?? 0, label: "Closed", tone: "danger", iconName: "reports" })}
          </div>
          <p class="dash-section-title">Quick actions</p>
          <div class="dash-links">
            ${SR.ui.dashLink({ href: "/module3-employer/vacancy-form.html", label: "Create Vacancy", desc: "Post a new role", iconName: "jobs" })}
            ${SR.ui.dashLink({ href: "/module3-employer/vacancies.html", label: "Manage Vacancies", desc: "Edit and track listings", iconName: "applications" })}
            ${SR.ui.dashLink({ href: "/module3-employer/company-profile.html", label: "Company Profile", desc: "Update company details", iconName: "company" })}
            ${SR.ui.dashLink({ href: "/module3-employer/verification.html", label: "Verification", desc: "Upload documents", iconName: "verify" })}
            ${SR.ui.dashLink({ href: "/module4-applications/employer-applicants.html", label: "Applicants", desc: "Review candidate pipeline", iconName: "applicants" })}
            ${SR.ui.dashLink({ href: "/module4-applications/employer-contact-requests.html", label: "Contact Requests", desc: "Message candidates", iconName: "contact" })}
            ${SR.ui.dashLink({ href: "/module4-applications/employer-interviews.html", label: "Interviews", desc: "Schedule and manage", iconName: "interviews" })}
          </div>
        </div>`;
    } catch (e) {
      if (e.status === 404) {
        ctx.body.innerHTML = `
          <div class="dash">
            <div class="card dash-hero">
              <h2>Set up your company</h2>
              <p>Complete your company profile before creating vacancies or inviting candidates.</p>
              <div class="row-actions">
                <a class="btn btn-primary" href="/module3-employer/company-profile.html">Complete Company Profile</a>
              </div>
            </div>
          </div>`;
        return;
      }
      ctx.body.innerHTML = SR.ui.errorState(e);
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
      <div class="page-stack">
        ${
          !exists
            ? `<div class="card onboard-banner"><strong>Complete onboarding</strong><p class="muted" style="margin-top:0.35rem">Create your company profile to continue hiring.</p></div>`
            : ""
        }
        <form id="company-form" class="card form-card">
          <div class="form-card-body">
            <section class="form-section">
              <h3 class="form-section-title">Company identity</h3>
              <div class="form-grid two">
                <label class="field"><span class="field-label">Company name</span><input name="companyName" value="${v("companyName")}" required /></label>
                <label class="field"><span class="field-label">Registered name</span><input name="registeredCompanyName" value="${v("registeredCompanyName")}" /></label>
                <label class="field"><span class="field-label">Registration number</span><input name="registrationNumber" value="${v("registrationNumber")}" /></label>
                <label class="field"><span class="field-label">Industry</span><input name="industry" value="${v("industry")}" /></label>
              </div>
            </section>
            <section class="form-section">
              <h3 class="form-section-title">Contact & location</h3>
              <div class="form-grid two">
                <label class="field"><span class="field-label">Registered address</span><input name="registeredAddress" value="${v("registeredAddress")}" /></label>
                <label class="field"><span class="field-label">Operating location</span><input name="operatingLocation" value="${v("operatingLocation")}" /></label>
                <label class="field"><span class="field-label">Official email</span><input name="officialCompanyEmail" type="email" value="${v("officialCompanyEmail")}" /></label>
                <label class="field"><span class="field-label">Phone</span><input name="companyPhone" value="${v("companyPhone")}" /></label>
                <label class="field"><span class="field-label">Website</span><input name="website" value="${v("website")}" /></label>
                <label class="field"><span class="field-label">Authorized representative</span><input name="authorizedRepresentative" value="${v("authorizedRepresentative")}" /></label>
              </div>
            </section>
            <section class="form-section">
              <h3 class="form-section-title">About the company</h3>
              <label class="field"><span class="field-label">Company description</span><textarea name="companyDescription" rows="4">${v("companyDescription")}</textarea></label>
            </section>
          </div>
          <div class="form-card-footer">
            <button class="btn btn-primary" type="submit">${exists ? "Save changes" : "Create profile"}</button>
          </div>
        </form>
      </div>`;
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
    } catch (e) {
      ctx.body.innerHTML = SR.ui.errorState(e);
      return;
    }
    const docs = v?.documents || v?.Documents || [];
    const statusRaw = v?.status ?? v?.Status ?? 1;
    const statusLabel = SR.status.verification(statusRaw);
    const canEditDocs =
      statusLabel === "Unverified" ||
      statusLabel === "More Information Required" ||
      statusLabel === "Rejected";
    const showSubmit = statusLabel === "Unverified" && docs.length > 0;
    const showWithdraw = statusLabel === "Pending Review";
    const showResubmit =
      (statusLabel === "Rejected" || statusLabel === "More Information Required") && docs.length > 0;
    const feedback =
      v?.administratorFeedback ||
      v?.AdministratorFeedback ||
      v?.adminFeedback ||
      v?.feedback;
    ctx.body.innerHTML = `
      <div class="grid-2">
        <div class="card form-card">
          <div class="form-card-body">
            <section class="form-section">
              <h3 class="form-section-title">Verification status</h3>
              <div style="display:flex;align-items:center;gap:0.55rem;flex-wrap:wrap">
                ${SR.ui.badge(statusLabel, SR.status.verificationKind(statusRaw))}
              </div>
              ${feedback ? `<p class="muted" style="margin:0">${SR.utils.escape(feedback)}</p>` : ""}
              <p class="muted" style="margin:0;font-size:0.88rem">Upload Business Registration Certificate, Proof of Business Address, or Authorized Representative Document. PDF/JPG/PNG, max 5MB.</p>
              <div class="row-actions" style="margin:0">
                ${showSubmit ? `<button class="btn btn-primary" id="submit-ver" type="button">Submit for review</button>` : ""}
                ${showWithdraw ? `<button class="btn btn-outline" id="withdraw-ver" type="button">Withdraw</button>` : ""}
                ${showResubmit ? `<button class="btn btn-primary" id="resubmit-ver" type="button">Resubmit for review</button>` : ""}
              </div>
              ${
                statusLabel === "Unverified" && !docs.length
                  ? `<p class="muted" style="margin:0;font-size:0.88rem">Upload at least one document, then submit for review.</p>`
                  : ""
              }
              ${
                (statusLabel === "Rejected" || statusLabel === "More Information Required") && !docs.length
                  ? `<p class="muted" style="margin:0;font-size:0.88rem">Upload at least one document, then resubmit for review.</p>`
                  : ""
              }
              ${
                statusLabel === "Pending Review"
                  ? `<p class="muted" style="margin:0;font-size:0.88rem">Your request is under review. You can withdraw it if needed.</p>`
                  : ""
              }
              ${
                statusLabel === "Verified"
                  ? `<p class="muted" style="margin:0;font-size:0.88rem">Your company is verified. Document changes are locked.</p>`
                  : ""
              }
            </section>
            ${
              canEditDocs
                ? `<section class="form-section">
              <h3 class="form-section-title">Upload document</h3>
              <form id="doc-upload" class="form-grid">
                <label class="field"><span class="field-label">File (PDF/JPG/PNG, max 5MB)</span>
                  <input name="file" type="file" accept=".pdf,.jpg,.jpeg,.png" required />
                </label>
                <button class="btn btn-navy" type="submit">Upload</button>
              </form>
            </section>`
                : ""
            }
          </div>
        </div>
        <div class="card docs-card">
          <h3 class="form-section-title" style="border:0;padding:0;margin-bottom:0.65rem">Documents</h3>
          <div class="doc-list">
            ${
              docs.length
                ? docs
                    .map(
                      (d) => `
              <div class="doc-row">
                <div class="doc-row-main">
                  <span class="doc-name" title="${SR.utils.escape(d.fileName || `Document #${d.id}`)}">${SR.utils.escape(d.fileName || `Document #${d.id}`)}</span>
                  ${SR.ui.badge(SR.status.document(d.status), "neutral")}
                </div>
                ${
                  canEditDocs
                    ? `<button class="btn btn-danger btn-sm" data-del="${d.id}" type="button">Remove</button>`
                    : ""
                }
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
    document.getElementById("submit-ver")?.addEventListener("click", () =>
      act(() => SR.api.post("/api/employer-verification/submit"), "Submitted for review")
    );
    document.getElementById("withdraw-ver")?.addEventListener("click", () =>
      act(() => SR.api.patch("/api/employer-verification/withdraw"), "Withdrawn")
    );
    document.getElementById("resubmit-ver")?.addEventListener("click", () =>
      act(() => SR.api.post("/api/employer-verification/resubmit"), "Resubmitted for review")
    );
    document.getElementById("doc-upload")?.addEventListener("submit", async (e) => {
      e.preventDefault();
      const file = e.target.file.files[0];
      if (!file) return;
      if (file.size > 5 * 1024 * 1024) {
        SR.ui.toast("Max 5 MB", "error");
        return;
      }
      const fd = new FormData();
      fd.append("File", file);
      await act(
        () => SR.api.request("POST", "/api/employer-verification/document/upload", null, { formData: fd }),
        "Uploaded"
      );
    });
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
          : SR.ui.empty("No vacancies yet.", {
              detail: "Create a vacancy draft, then submit it for admin approval.",
              cta: { href: "/module3-employer/vacancy-form.html", label: "Create Vacancy" },
            })
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
      <form id="vacancy-form" class="card form-card">
        <div class="form-card-body">
          <section class="form-section">
            <h3 class="form-section-title">Role details</h3>
            <label class="field"><span class="field-label">Title <span class="req" aria-hidden="true">*</span></span><input name="title" required maxlength="200" value="${esc(v.title || "")}" /></label>
            <div class="form-grid two">
              <label class="field"><span class="field-label">Category</span><input name="category" value="${esc(v.category || "")}" /></label>
              <label class="field"><span class="field-label">Employment type</span><input name="employmentType" value="${esc(v.employmentType || "")}" /></label>
              <label class="field"><span class="field-label">Work location</span><input name="workLocation" value="${esc(v.workLocation || "")}" /></label>
              <label class="field"><span class="field-label">Experience level</span><input name="experienceLevel" value="${esc(v.experienceLevel || "")}" /></label>
              <label class="field"><span class="field-label">Salary range</span><input name="salaryRange" value="${esc(v.salaryRange || "")}" /></label>
              <label class="field"><span class="field-label">Expiry <span class="req" aria-hidden="true">*</span></span><input name="expiryDate" type="datetime-local" required value="${esc(expiry)}" /></label>
            </div>
          </section>
          <section class="form-section">
            <h3 class="form-section-title">Description</h3>
            <label class="field"><span class="field-label">Description <span class="req" aria-hidden="true">*</span></span><textarea name="description" required rows="5">${esc(v.description || "")}</textarea></label>
            <label class="field"><span class="field-label">Requirements</span><textarea name="requirements" rows="4">${esc(v.requirements || "")}</textarea></label>
          </section>
          <section class="form-section">
            <h3 class="form-section-title">Required skills</h3>
            <div class="chip-row" id="skill-chips"></div>
            <div class="skill-input-row">
              <input id="skill-input" placeholder="Type a skill and press Enter" />
              <button type="button" class="btn btn-outline" id="add-skill">Add</button>
            </div>
            <p class="muted" style="margin:0;font-size:0.85rem">Skill tags power match scores for job seekers.</p>
          </section>
        </div>
        <div class="form-card-footer">
          <button class="btn btn-primary" type="submit">${id ? "Save changes" : "Create draft"}</button>
        </div>
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
      const headTitle = document.querySelector(".page-head h1");
      const headSub = document.querySelector(".page-head p");
      if (headTitle) headTitle.textContent = v.title || "Vacancy details";
      if (headSub) headSub.textContent = SR.status.vacancy(v.status);
      else if (headTitle && !headTitle.nextElementSibling) {
        const p = document.createElement("p");
        p.textContent = SR.status.vacancy(v.status);
        headTitle.insertAdjacentElement("afterend", p);
      }
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
      ctx.body.innerHTML = SR.ui.errorState(e, {
        cta: { href: "/module3-employer/vacancies.html", label: "Back to vacancies" },
      });
    }
  }

  async function adminVerifications() {
    const body = await bootAdmin("Employer verifications", "Review company verification requests.");
    if (!body) return;
    try {
      const list = await SR.api.get("/api/admin/employer-verifications");
      const items = Array.isArray(list) ? list : [];
      const sorted = [...items].sort((a, b) => {
        const ap = SR.status.verification(a.status ?? a.Status) === "Pending Review" ? 0 : 1;
        const bp = SR.status.verification(b.status ?? b.Status) === "Pending Review" ? 0 : 1;
        if (ap !== bp) return ap - bp;
        return (b.id ?? 0) - (a.id ?? 0);
      });
      body.innerHTML = `<div class="list">${
        sorted.length
          ? sorted
              .map((v) => {
                const status = v.status ?? v.Status;
                const label = SR.status.verification(status);
                const pending = label === "Pending Review";
                const profileId = v.employerProfileId ?? v.EmployerProfileId;
                const companyName = v.companyName || v.CompanyName || "";
                return `
          <div class="list-item" data-id="${v.id}">
            <h3>${companyName ? SR.utils.escape(companyName) : `Verification #${v.id}`}</h3>
            <div class="meta">${SR.ui.badge(label, SR.status.verificationKind(status))}
              ${!companyName && profileId != null ? `<span>Employer profile #${SR.utils.escape(profileId)}</span>` : ""}
              ${companyName ? `<span>Verification #${v.id}</span>` : ""}
              ${v.submittedAt || v.SubmittedAt ? `<span>Submitted ${SR.utils.formatDate(v.submittedAt || v.SubmittedAt)}</span>` : ""}
            </div>
            <div class="row-actions">
              <a class="btn btn-outline btn-sm" href="/module3-employer/admin-employer-verification-details.html?id=${v.id}">Details</a>
              ${
                pending
                  ? `<button class="btn btn-primary btn-sm" data-verify type="button">Verify</button>
                     <button class="btn btn-outline btn-sm" data-info type="button">Request info</button>
                     <button class="btn btn-danger btn-sm" data-reject type="button">Reject</button>`
                  : ""
              }
            </div>
          </div>`;
              })
              .join("")
          : SR.ui.empty("No verification records.")
      }</div>`;

      const feedbackModal = (title, confirmText, danger, pathSuffix) => (item) => {
        const id = item.dataset.id;
        SR.ui.modal({
          title,
          bodyHtml: `<label class="field"><span class="field-label">Feedback <span class="req" aria-hidden="true">*</span></span><textarea id="fb" required maxlength="1000"></textarea></label>`,
          confirmText,
          danger,
          onConfirm: async (backdrop) => {
            const feedback = backdrop.querySelector("#fb").value.trim();
            if (!feedback) throw new Error("Feedback required");
            await SR.api.patch(`/api/admin/employer-verifications/${id}/${pathSuffix}`, { feedback });
            SR.ui.toast(confirmText === "Reject" ? "Rejected" : "Feedback sent");
            location.reload();
          },
        });
      };

      body.querySelectorAll(".list-item").forEach((item) => {
        const id = item.dataset.id;
        item.querySelector("[data-verify]")?.addEventListener("click", async () => {
          try {
            await SR.api.patch(`/api/admin/employer-verifications/${id}/verify`);
            SR.ui.toast("Verified");
            location.reload();
          } catch (e) {
            SR.ui.toast(e.message, "error");
          }
        });
        item.querySelector("[data-info]")?.addEventListener("click", () =>
          feedbackModal("Request information", "Send", false, "request-information")(item)
        );
        item.querySelector("[data-reject]")?.addEventListener("click", () =>
          feedbackModal("Reject verification", "Reject", true, "reject")(item)
        );
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
      const status = v.status ?? v.Status;
      const feedback =
        v.administratorFeedback ||
        v.AdministratorFeedback ||
        v.adminFeedback ||
        v.feedback ||
        "";
      const companyName = v.companyName || v.CompanyName || "";
      const profileId = v.employerProfileId ?? v.EmployerProfileId;
      const docs = v.documents || v.Documents || [];
      const label = SR.status.verification(status);
      const pending = label === "Pending Review";
      body.innerHTML = `
        <a class="back-link" href="/module3-employer/admin-employer-verifications.html">← Back to verifications</a>
        <div class="card" style="margin-top:0.75rem">
          <h2>${companyName ? SR.utils.escape(companyName) : `Verification #${SR.utils.escape(v.id ?? id)}`}</h2>
          <div class="meta" style="margin-top:0.7rem">
            ${SR.ui.badge(label, SR.status.verificationKind(status))}
            ${companyName ? `<span>Verification #${SR.utils.escape(v.id ?? id)}</span>` : ""}
            ${profileId != null ? `<span>Employer profile #${SR.utils.escape(profileId)}</span>` : ""}
            ${v.submittedAt || v.SubmittedAt ? `<span>Submitted ${SR.utils.formatDate(v.submittedAt || v.SubmittedAt)}</span>` : ""}
            ${v.reviewedAt || v.ReviewedAt ? `<span>Reviewed ${SR.utils.formatDate(v.reviewedAt || v.ReviewedAt)}</span>` : ""}
          </div>
          ${
            feedback
              ? `<p style="margin-top:0.9rem"><strong>Feedback</strong></p>
                 <p class="muted">${SR.utils.escape(feedback)}</p>`
              : `<p class="muted" style="margin-top:0.9rem">No administrator feedback yet.</p>`
          }
          <h3 class="docs-heading">Documents</h3>
          <div class="doc-list">
            ${
              docs.length
                ? docs
                    .map((d) => {
                      const fileName = d.fileName || d.FileName || `Document #${d.id ?? ""}`;
                      return `<div class="doc-row">
                        <div class="doc-row-main">
                          <span class="doc-name" title="${SR.utils.escape(fileName)}">${SR.utils.escape(fileName)}</span>
                          ${SR.ui.badge(SR.status.document(d.status ?? d.Status), "neutral")}
                          ${d.uploadedAt || d.UploadedAt ? `<span class="doc-date">${SR.utils.formatDate(d.uploadedAt || d.UploadedAt)}</span>` : ""}
                        </div>
                      </div>`;
                    })
                    .join("")
                : SR.ui.empty("No documents on this request.")
            }
          </div>
          <div class="row-actions" style="margin-top:1rem">
            ${
              pending
                ? `<button class="btn btn-primary" id="detail-verify" type="button">Verify</button>
                   <button class="btn btn-outline" id="detail-info" type="button">Request info</button>
                   <button class="btn btn-danger" id="detail-reject" type="button">Reject</button>`
                : ""
            }
            <a class="btn btn-outline" href="/module3-employer/admin-employer-verifications.html">Back</a>
          </div>
        </div>`;

      document.getElementById("detail-verify")?.addEventListener("click", async () => {
        try {
          await SR.api.patch(`/api/admin/employer-verifications/${id}/verify`);
          SR.ui.toast("Verified");
          location.href = "/module3-employer/admin-employer-verifications.html";
        } catch (e) {
          SR.ui.toast(e.message, "error");
        }
      });
      const openFeedback = (title, confirmText, danger, suffix) => {
        SR.ui.modal({
          title,
          bodyHtml: `<label class="field"><span class="field-label">Feedback <span class="req" aria-hidden="true">*</span></span><textarea id="fb" required maxlength="1000"></textarea></label>`,
          confirmText,
          danger,
          onConfirm: async (backdrop) => {
            const note = backdrop.querySelector("#fb").value.trim();
            if (!note) throw new Error("Feedback required");
            await SR.api.patch(`/api/admin/employer-verifications/${id}/${suffix}`, { feedback: note });
            SR.ui.toast(danger ? "Rejected" : "Feedback sent");
            location.href = "/module3-employer/admin-employer-verifications.html";
          },
        });
      };
      document.getElementById("detail-info")?.addEventListener("click", () =>
        openFeedback("Request information", "Send", false, "request-information")
      );
      document.getElementById("detail-reject")?.addEventListener("click", () =>
        openFeedback("Reject verification", "Reject", true, "reject")
      );
    } catch (e) {
      body.innerHTML = SR.ui.errorState(e, {
        cta: { href: "/module3-employer/admin-employer-verifications.html", label: "Back to list" },
      });
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
