window.SR = window.SR || {};

SR.module2 = (function () {
  const path = location.pathname;

  async function boot(title, sub, actions) {
    const user = await SR.guards.requireAuth(["JobSeeker"]);
    if (!user) return null;
    SR.ui.mountAppShell(user);
    await SR.ui.mountNotificationBell();
    return { user, body: SR.ui.page(title, sub, actions), guest: false };
  }

  /** Jobs browse: guests allowed; JobSeekers get the app shell. */
  async function bootJobs(title, sub, actions) {
    if (SR.auth.isLoggedIn()) {
      try {
        const me = await SR.auth.me();
        const role = me.role || me.Role;
        if (role === "JobSeeker") {
          const user = {
            userId: me.id ?? me.Id ?? SR.auth.getUser()?.userId,
            fullName: me.fullName || me.FullName,
            email: me.email || me.Email,
            role,
            isActive: me.isActive ?? me.IsActive,
          };
          localStorage.setItem("smartRecruit_user", JSON.stringify(user));
          SR.ui.mountAppShell(user);
          await SR.ui.mountNotificationBell();
          return { user, body: SR.ui.page(title, sub, actions), guest: false };
        }
      } catch (err) {
        if (err.status === 401) SR.auth.clear();
      }
    }
    SR.ui.mountPublicShell("jobs");
    return { user: null, body: SR.ui.page(title, sub, actions), guest: true };
  }

  async function dashboard() {
    const ctx = await boot("Dashboard", "Your career hub on Smart Recruit.");
    if (!ctx) return;
    try {
      const [dash, profile] = await Promise.all([
        SR.api.get("/api/jobseekers/me/dashboard"),
        SR.api.get("/api/jobseekers/me/profile").catch(() => null),
      ]);
      const pct = dash.profileCompletion ?? profile?.profileCompletion ?? 0;
      ctx.body.innerHTML = `
        <div class="card welcome-banner" style="margin-bottom:1rem">
          <h1>Welcome back, ${SR.utils.escape(dash.fullName || ctx.user.fullName)}</h1>
          <p class="muted" style="margin-top:0.4rem">${SR.utils.escape(dash.professionalTitle || "Complete your profile to improve matches.")}</p>
        </div>
        <div class="grid-3">
          <div class="card">
            <div class="ring-wrap">
              <div class="ring" style="--p:${Number(pct) || 0}" data-label="${Number(pct) || 0}%"></div>
              <div><strong>Profile completion</strong><p class="muted">Keep skills and experience up to date.</p></div>
            </div>
          </div>
          <div class="card stat-card"><strong>${dash.totalSkills ?? 0}</strong><span>Total skills</span></div>
          <div class="card stat-card"><strong>${SR.utils.escape(dash.professionalTitle || "—")}</strong><span>Professional title</span></div>
        </div>
        <div class="admin-shortcuts" style="margin-top:1rem;display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:.75rem">
          <a class="btn btn-primary" href="/module2-jobseeker/jobs.html">Find Jobs</a>
          <a class="btn btn-outline" href="/module2-jobseeker/profile.html">Complete Profile</a>
          <a class="btn btn-outline" href="/module2-jobseeker/profile.html#cv">Upload CV</a>
          <a class="btn btn-outline" href="/module4-applications/jobseeker-applications.html">My Applications</a>
          <a class="btn btn-outline" href="/module4-applications/jobseeker-contact-requests.html">Contact Requests</a>
          <a class="btn btn-outline" href="/module4-applications/jobseeker-interviews.html">Interviews</a>
          <a class="btn btn-outline" href="/module5-trust/notifications.html">Notifications</a>
        </div>`;
    } catch (e) {
      SR.ui.toast(e.message, "error");
    }
  }

  async function profilePage() {
    const ctx = await boot("Profile", "Skills and experience drive match scores.");
    if (!ctx) return;
    let profile = {
      professionalTitle: "",
      location: "",
      experience: 0,
      education: "",
      about: "",
      skills: [],
      profileCompletion: 0,
    };
    let cv = null;
    try {
      profile = await SR.api.get("/api/jobseekers/me/profile");
    } catch {}
    try {
      cv = await SR.api.get("/api/jobseekers/me/cv");
    } catch {}
    let skills = [...(profile.skills || [])];
    const esc = SR.utils.escape;

    const renderSkills = () => {
      document.getElementById("skill-chips").innerHTML = skills
        .map(
          (s, i) =>
            `<span class="chip">${esc(s)} <button type="button" data-i="${i}" aria-label="Remove">&times;</button></span>`
        )
        .join("");
      document.querySelectorAll("#skill-chips button").forEach((btn) => {
        btn.onclick = () => {
          skills.splice(Number(btn.dataset.i), 1);
          renderSkills();
        };
      });
    };

    ctx.body.innerHTML = `
      <div class="grid-2">
        <form id="profile-form" class="card form-grid">
          <div class="progress-label"><span>Profile completion</span><span>${profile.profileCompletion ?? 0}%</span></div>
          <div class="progress"><span style="width:${profile.profileCompletion ?? 0}%"></span></div>
          <label class="field">Professional title<input name="professionalTitle" value="${esc(profile.professionalTitle)}" /></label>
          <div class="form-grid two">
            <label class="field">Location<input name="location" value="${esc(profile.location)}" /></label>
            <label class="field">Experience (years)<input name="experience" type="number" min="0" value="${esc(profile.experience)}" /></label>
          </div>
          <label class="field">Education<input name="education" value="${esc(profile.education)}" /></label>
          <label class="field">About<textarea name="about">${esc(profile.about)}</textarea></label>
          <div>
            <label class="field">Skills</label>
            <div class="chip-row" id="skill-chips" style="margin:0.5rem 0"></div>
            <div class="skill-input-row">
              <input id="skill-input" placeholder="Type a skill and press Enter" />
              <button type="button" class="btn btn-outline" id="add-skill">Add</button>
            </div>
          </div>
          <button class="btn btn-primary" type="submit">Save profile</button>
        </form>
        <div class="card" id="cv">
          <h3>CV</h3>
          <p class="muted" style="margin:0.5rem 0 1rem">PDF, DOC, DOCX · max 5 MB</p>
          ${
            cv
              ? `<p><strong>${esc(cv.fileName || cv.originalFileName || "Uploaded")}</strong></p>
                 <div class="meta">
                   <span>${esc(cv.fileType || "")}</span>
                   <span>${SR.utils.fileSize(cv.fileSize)}</span>
                   <span>${SR.utils.formatDate(cv.uploadedAt)}</span>
                 </div>`
              : `<p class="muted">No CV uploaded yet.</p>`
          }
          <form id="cv-form" class="form-grid" style="margin-top:1rem">
            <input name="file" type="file" accept=".pdf,.doc,.docx,application/pdf" required />
            <button class="btn btn-navy" type="submit">${cv ? "Replace CV" : "Upload CV"}</button>
          </form>
        </div>
      </div>`;

    renderSkills();
    const addSkill = () => {
      const input = document.getElementById("skill-input");
      const val = input.value.trim();
      if (!val) return;
      const exists = skills.some((s) => s.toLowerCase() === val.toLowerCase());
      if (!exists) skills.push(val);
      input.value = "";
      renderSkills();
    };
    document.getElementById("add-skill").onclick = addSkill;
    document.getElementById("skill-input").addEventListener("keydown", (e) => {
      if (e.key === "Enter") {
        e.preventDefault();
        addSkill();
      }
    });

    document.getElementById("profile-form").onsubmit = async (e) => {
      e.preventDefault();
      const d = SR.utils.formToObject(e.target);
      const btn = e.target.querySelector("[type=submit]");
      try {
        SR.ui.setLoading(btn, true, "Saving…");
        await SR.api.put("/api/jobseekers/me/profile", {
          professionalTitle: d.professionalTitle,
          location: d.location,
          experience: Number(d.experience) || 0,
          education: d.education,
          about: d.about,
          skills,
        });
        SR.ui.toast("Profile saved");
        location.reload();
      } catch (err) {
        SR.ui.toast(err.message, "error");
      } finally {
        SR.ui.setLoading(btn, false);
      }
    };

    document.getElementById("cv-form").onsubmit = async (e) => {
      e.preventDefault();
      const file = e.target.file.files[0];
      if (!file) return;
      const allowed = [".pdf", ".doc", ".docx"];
      const lower = file.name.toLowerCase();
      if (!allowed.some((x) => lower.endsWith(x))) {
        SR.ui.toast("Allowed formats: PDF, DOC, DOCX", "error");
        return;
      }
      if (file.size > 5 * 1024 * 1024) {
        SR.ui.toast("Maximum file size is 5 MB", "error");
        return;
      }
      const fd = new FormData();
      fd.append("file", file);
      const btn = e.target.querySelector("[type=submit]");
      try {
        SR.ui.setLoading(btn, true, "Uploading…");
        if (cv) await SR.api.request("PUT", "/api/jobseekers/me/cv", null, { formData: fd });
        else await SR.api.request("POST", "/api/jobseekers/me/cv", null, { formData: fd });
        SR.ui.toast("CV saved");
        location.reload();
      } catch (err) {
        SR.ui.toast(err.message, "error");
      } finally {
        SR.ui.setLoading(btn, false);
      }
    };
  }

  async function jobsPage() {
    const ctx = await bootJobs("Find Jobs", "Browse open vacancies — no account needed to explore.");
    if (!ctx) return;
    const params = new URLSearchParams(location.search);
    let pageNumber = Number(params.get("pageNumber") || 1);
    const pageSize = Number(params.get("pageSize") || 10);

    ctx.body.innerHTML = `
      ${
        ctx.guest
          ? `<div class="card" style="margin-bottom:1rem"><strong>Browsing as guest.</strong> <span class="muted">Sign in as a Job Seeker to apply and see your match score.</span> <a class="btn btn-primary btn-sm" href="/login.html?next=${encodeURIComponent(location.pathname + location.search)}">Login</a> <a class="btn btn-outline btn-sm" href="/register.html">Create Account</a></div>`
          : ""
      }
      <div class="jobs-layout">
        <aside class="card filters-panel form-grid" id="filters">
          <h3>Filters</h3>
          <label class="field">Search<input name="search" value="${SR.utils.escape(params.get("search") || "")}" /></label>
          <label class="field">Company<input name="company" value="${SR.utils.escape(params.get("company") || "")}" /></label>
          <label class="field">Skill<input name="skill" value="${SR.utils.escape(params.get("skill") || "")}" /></label>
          <label class="field">Location<input name="location" value="${SR.utils.escape(params.get("location") || "")}" /></label>
          <label class="field">Minimum experience<input name="minimumExperience" type="number" min="0" value="${SR.utils.escape(params.get("minimumExperience") || "")}" /></label>
          <!-- minimumMatch omitted: repository does not apply it -->
          <button class="btn btn-primary" type="button" id="apply-filters">Apply filters</button>
        </aside>
        <div>
          <div id="job-results" class="list"></div>
          <div class="pagination">
            <button class="btn btn-outline btn-sm" id="prev-page" type="button">Previous</button>
            <span class="muted">Page ${pageNumber}</span>
            <button class="btn btn-outline btn-sm" id="next-page" type="button">Next</button>
          </div>
        </div>
      </div>`;

    const readFilters = () => {
      const box = document.getElementById("filters");
      return {
        search: box.querySelector("[name=search]").value.trim(),
        company: box.querySelector("[name=company]").value.trim(),
        skill: box.querySelector("[name=skill]").value.trim(),
        location: box.querySelector("[name=location]").value.trim(),
        minimumExperience: box.querySelector("[name=minimumExperience]").value,
      };
    };

    const go = (page) => {
      const f = readFilters();
      const q = new URLSearchParams();
      Object.entries(f).forEach(([k, v]) => {
        if (v !== "" && v != null) q.set(k, v);
      });
      q.set("pageNumber", String(page));
      q.set("pageSize", String(pageSize));
      location.href = `/module2-jobseeker/jobs.html?${q}`;
    };

    document.getElementById("apply-filters").onclick = () => go(1);
    document.getElementById("prev-page").onclick = () => {
      if (pageNumber > 1) go(pageNumber - 1);
    };

    const results = document.getElementById("job-results");
    results.innerHTML = `<div class="card"><div class="skeleton"></div><div class="skeleton" style="margin-top:.6rem;width:70%"></div></div>`;

    try {
      const q = new URLSearchParams({
        pageNumber: String(pageNumber),
        pageSize: String(pageSize),
      });
      ["search", "company", "skill", "location", "minimumExperience"].forEach((k) => {
        const v = params.get(k);
        if (v) q.set(k, v);
      });
      const items = await SR.api.get(`/api/jobs?${q}`, { auth: !ctx.guest });
      const list = Array.isArray(items) ? items : items.items || [];
      document.getElementById("next-page").disabled = list.length < pageSize;
      document.getElementById("prev-page").disabled = pageNumber <= 1;
      document.getElementById("next-page").onclick = () => {
        if (list.length >= pageSize) go(pageNumber + 1);
      };

      if (!list.length) {
        results.innerHTML = SR.ui.empty("No jobs found. Try adjusting filters.");
        return;
      }

      // Do not show match % on list cards — list DTO score is not used as a primary signal here.
      results.innerHTML = list
        .map((j) => {
          const desc = (j.description || "").slice(0, 160);
          return `
          <a class="list-item" href="/module2-jobseeker/job-details.html?id=${j.id}">
            <h3>${SR.utils.escape(j.jobTitle)}</h3>
            <div class="meta">
              <span>${SR.utils.escape(j.companyName)}</span>
              <span>${SR.utils.escape(j.location)}</span>
              <span>${j.requiredExperience ?? 0} yrs exp</span>
            </div>
            <p class="muted" style="margin-top:0.55rem">${SR.utils.escape(desc)}${(j.description || "").length > 160 ? "…" : ""}</p>
            <div class="chip-row" style="margin-top:0.65rem">${(j.requiredSkills || [])
              .slice(0, 6)
              .map((s) => `<span class="chip">${SR.utils.escape(s)}</span>`)
              .join("")}</div>
            <div class="row-actions"><span class="btn btn-outline btn-sm">View Details</span></div>
          </a>`;
        })
        .join("");
    } catch (e) {
      results.innerHTML = SR.ui.empty(e.message || "Failed to load jobs.");
    }
  }

  async function jobDetails() {
    const ctx = await bootJobs("Job details", "Loading…");
    if (!ctx) return;
    const id = SR.utils.qs("id");
    if (!id) {
      ctx.body.innerHTML = SR.ui.empty("Missing job id.");
      return;
    }
    try {
      const job = await SR.api.get(`/api/jobs/${id}`, { auth: !ctx.guest });
      const b = job.matchBreakdown || {};
      const skillsScore = b.skillsScore ?? b.SkillsScore ?? 0;
      const expScore = b.experienceScore ?? b.ExperienceScore ?? 0;
      const eduScore = b.educationScore ?? b.EducationScore ?? 0;
      const locScore = b.locationScore ?? b.LocationScore ?? 0;

      const headTitle = document.querySelector(".page-head h1");
      const headSub = document.querySelector(".page-head p");
      if (headTitle) headTitle.textContent = job.jobTitle || "Job details";
      if (headSub) headSub.textContent = `${job.companyName || ""} · ${job.location || ""}`;

      const next = encodeURIComponent(`/module2-jobseeker/job-details.html?id=${id}`);
      const loginApplyHref = `/login.html?next=${next}`;
      const actions = ctx.guest
        ? `<a class="btn btn-primary" id="guest-apply-btn" href="${loginApplyHref}">Apply Now</a>
           <a class="btn btn-outline" href="/register.html">Create Account</a>`
        : job.canApply
          ? `<button class="btn btn-primary" id="apply-btn" type="button">Apply Now</button>
             <button class="btn btn-danger btn-sm" id="report-btn" type="button">Report Job</button>`
          : `<button class="btn btn-outline" type="button" disabled>Cannot apply</button>
             <button class="btn btn-danger btn-sm" id="report-btn" type="button">Report Job</button>`;

      const matchPanel = ctx.guest
        ? `<div class="card match-panel" style="margin-bottom:1rem">
              <p class="muted">Sign in as a Job Seeker to see your personal match score.</p>
              <div class="row-actions" style="margin-top:1rem">${actions}</div>
            </div>`
        : `<div class="card match-panel" style="margin-bottom:1rem">
              <div class="score">${Math.round(job.matchScore || 0)}%</div>
              <p class="muted">Overall match (from backend)</p>
              <div style="margin-top:1rem">
                <div class="progress-label"><span>Skills (max 60)</span><span>${Math.round(skillsScore)}</span></div>
                <div class="progress"><span style="width:${Math.min(100, (skillsScore / 60) * 100)}%"></span></div>
                <div class="progress-label" style="margin-top:.7rem"><span>Experience (max 20)</span><span>${Math.round(expScore)}</span></div>
                <div class="progress"><span style="width:${Math.min(100, (expScore / 20) * 100)}%"></span></div>
                <div class="progress-label" style="margin-top:.7rem"><span>Education (max 10)</span><span>${Math.round(eduScore)}</span></div>
                <div class="progress"><span style="width:${Math.min(100, (eduScore / 10) * 100)}%"></span></div>
                <div class="progress-label" style="margin-top:.7rem"><span>Location (max 10)</span><span>${Math.round(locScore)}</span></div>
                <div class="progress"><span style="width:${Math.min(100, (locScore / 10) * 100)}%"></span></div>
              </div>
              <div class="row-actions">${actions}</div>
            </div>`;

      ctx.body.innerHTML = `
        <div class="grid-2">
          <div class="card">
            <div class="meta" style="margin-bottom:0.8rem">
              ${SR.ui.badge(job.trustLabel || "Standard Review")}
              <span>Experience: ${job.requiredExperience ?? 0} yrs</span>
              <span>Requirements: ${SR.utils.escape(job.educationRequirement || "—")}</span>
            </div>
            <p>${SR.utils.escape(job.description || "")}</p>
            <h3 style="margin:1rem 0 0.45rem">Required skills</h3>
            <div class="chip-row">${(job.requiredSkills || []).map((s) => `<span class="chip">${SR.utils.escape(s)}</span>`).join("") || "—"}</div>
            ${
              ctx.guest
                ? ""
                : `<h3 style="margin:1rem 0 0.45rem">Matched skills</h3>
            <div class="chip-row">${(job.matchedSkills || []).map((s) => `<span class="chip matched">${SR.utils.escape(s)}</span>`).join("") || "—"}</div>
            <h3 style="margin:1rem 0 0.45rem">Missing skills</h3>
            <div class="chip-row">${(job.missingSkills || []).map((s) => `<span class="chip missing">${SR.utils.escape(s)}</span>`).join("") || "—"}</div>`
            }
          </div>
          <div>
            ${matchPanel}
            <div class="card trust-card">
              <h3>Job Trust</h3>
              <p style="margin:0.45rem 0">${SR.ui.badge(job.trustLabel || "Standard Review")}</p>
              <p class="muted" style="font-size:0.88rem">Trust information helps you evaluate the listing but is not a guarantee.</p>
            </div>
          </div>
        </div>`;

      document.getElementById("apply-btn")?.addEventListener("click", () => {
        if (SR.module4?.openApplyModal) SR.module4.openApplyModal(id);
      });
      document.getElementById("report-btn")?.addEventListener("click", () => {
        if (SR.module5?.openReportModal) SR.module5.openReportModal(id);
      });
    } catch (e) {
      SR.ui.toast(e.message, "error");
      ctx.body.innerHTML = SR.ui.empty(e.message || "Job not found.");
    }
  }

  document.addEventListener("DOMContentLoaded", () => {
    if (path.includes("/module2-jobseeker/dashboard")) return dashboard();
    if (path.includes("/module2-jobseeker/profile")) return profilePage();
    if (path.includes("/module2-jobseeker/jobs.html")) return jobsPage();
    if (path.includes("/module2-jobseeker/job-details")) return jobDetails();
  });

  return {};
})();
