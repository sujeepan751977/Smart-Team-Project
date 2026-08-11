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
        const user = {
          userId: me.id ?? me.Id ?? SR.auth.getUser()?.userId,
          fullName: me.fullName || me.FullName,
          email: me.email || me.Email,
          role,
          isActive: me.isActive ?? me.IsActive,
        };
        SR.auth.setUser(user);
        if (role === "JobSeeker") {
          SR.ui.mountAppShell(user);
          await SR.ui.mountNotificationBell();
          return { user, body: SR.ui.page(title, sub, actions), guest: false };
        }
        // Other roles keep their shell while browsing public jobs
        SR.ui.mountAppShell(user);
        await SR.ui.mountNotificationBell();
        return { user, body: SR.ui.page(title, sub, actions), guest: true };
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
      const [dash, profile, cv] = await Promise.all([
        SR.api.get("/api/jobseekers/me/dashboard"),
        SR.api.get("/api/jobseekers/me/profile").catch(() => null),
        SR.api.get("/api/jobseekers/me/cv").catch(() => null),
      ]);
      const pct = Number(dash.profileCompletion ?? profile?.profileCompletion ?? 0) || 0;
      const hasCv = !!cv;
      const needsChecklist = pct < 100 || !hasCv;
      const checklistItems = [];
      if (pct < 100) {
        checklistItems.push({
          done: false,
          text: `Complete your profile (${pct}%)`,
          href: "/module2-jobseeker/profile.html",
          label: "Edit profile",
        });
      }
      if (!hasCv) {
        checklistItems.push({
          done: false,
          text: "Upload your CV",
          href: "/module2-jobseeker/profile.html#cv",
          label: "Upload CV",
        });
      }
      let nextHref = "/module2-jobseeker/jobs.html";
      let nextLabel = "Find Jobs";
      let nextHint = "Browse open roles and see how you match.";
      if (pct < 100) {
        nextHref = "/module2-jobseeker/profile.html";
        nextLabel = "Complete your profile";
        nextHint = "A stronger profile improves your match scores.";
      } else if (!hasCv) {
        nextHref = "/module2-jobseeker/profile.html#cv";
        nextLabel = "Upload your CV";
        nextHint = "Employers expect a CV before you apply.";
      }
      const title =
        dash.professionalTitle || profile?.professionalTitle || "";
      ctx.body.innerHTML = `
        <div class="dash">
          <div class="card dash-hero">
            <h2>Welcome back, ${SR.utils.escape(dash.fullName || ctx.user.fullName)}</h2>
            <p>${
              title
                ? SR.utils.escape(title)
                : "Set your professional title and skills so employers can find you."
            }</p>
            <p style="margin-top:0.55rem;color:rgba(255,255,255,0.9)">${SR.utils.escape(nextHint)}</p>
            <div class="row-actions">
              <a class="btn btn-primary" href="${nextHref}">${SR.utils.escape(nextLabel)}</a>
              ${
                nextHref !== "/module2-jobseeker/jobs.html"
                  ? `<a class="btn btn-outline" href="/module2-jobseeker/jobs.html">Browse jobs</a>`
                  : `<a class="btn btn-outline" href="/module4-applications/jobseeker-applications.html">My Applications</a>`
              }
            </div>
          </div>
          <div class="dash-split">
            <div class="dash">
              <div class="dash-stats">
                <div class="dash-stat tone-blue">
                  <div class="dash-stat-top">
                    <span class="dash-stat-icon">${SR.ui.icon("profile")}</span>
                    <div class="ring" style="--p:${pct}" data-label="${pct}%"></div>
                  </div>
                  <span>Profile completion</span>
                </div>
                ${SR.ui.dashStat({ value: dash.totalSkills ?? 0, label: "Total skills", tone: "navy", iconName: "applications" })}
                ${SR.ui.dashStat({ value: title || "—", label: "Professional title", tone: "ok", iconName: "jobs" })}
              </div>
              ${
                needsChecklist
                  ? `<div class="dash-panel">
                      <h3>Getting started</h3>
                      <ul class="dash-check">
                        ${checklistItems
                          .map(
                            (item) =>
                              `<li><span>${SR.utils.escape(item.text)}</span><a href="${item.href}">${SR.utils.escape(item.label)}</a></li>`
                          )
                          .join("")}
                      </ul>
                    </div>`
                  : ""
              }
            </div>
            <div>
              <p class="dash-section-title">Shortcuts</p>
              <div class="dash-links" style="margin-top:0.55rem">
                ${SR.ui.dashLink({ href: "/module2-jobseeker/jobs.html", label: "Find Jobs", desc: "Browse open vacancies", iconName: "jobs" })}
                ${SR.ui.dashLink({ href: "/module2-jobseeker/profile.html", label: "Complete Profile", desc: "Skills and experience", iconName: "profile" })}
                ${SR.ui.dashLink({ href: "/module2-jobseeker/profile.html#cv", label: "Upload CV", desc: "Share your resume", iconName: "applications" })}
                ${SR.ui.dashLink({ href: "/module4-applications/jobseeker-applications.html", label: "My Applications", desc: "Track application status", iconName: "applications" })}
                ${SR.ui.dashLink({ href: "/module4-applications/jobseeker-interviews.html", label: "Interviews", desc: "Upcoming interview slots", iconName: "interviews" })}
                ${SR.ui.dashLink({ href: "/module5-trust/notifications.html", label: "Notifications", desc: "Latest account alerts", iconName: "notifications" })}
              </div>
            </div>
          </div>
        </div>`;
    } catch (e) {
      ctx.body.innerHTML = SR.ui.errorState(e);
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
        <form id="profile-form" class="card form-card">
          <div class="form-card-body">
            <section class="form-section">
              <h3 class="form-section-title">Profile strength</h3>
              <div class="progress-label"><span>Completion</span><span>${profile.profileCompletion ?? 0}%</span></div>
              <div class="progress"><span style="width:${profile.profileCompletion ?? 0}%"></span></div>
            </section>
            <section class="form-section">
              <h3 class="form-section-title">Professional details</h3>
              <label class="field"><span class="field-label">Professional title</span><input name="professionalTitle" value="${esc(profile.professionalTitle)}" /></label>
              <div class="form-grid two">
                <label class="field"><span class="field-label">Location</span><input name="location" value="${esc(profile.location)}" /></label>
                <label class="field"><span class="field-label">Experience (years)</span><input name="experience" type="number" min="0" value="${esc(profile.experience)}" /></label>
              </div>
              <label class="field"><span class="field-label">Education</span><input name="education" value="${esc(profile.education)}" /></label>
              <label class="field"><span class="field-label">About</span><textarea name="about" rows="4">${esc(profile.about)}</textarea></label>
            </section>
            <section class="form-section">
              <h3 class="form-section-title">Skills</h3>
              <div class="chip-row" id="skill-chips"></div>
              <div class="skill-input-row">
                <input id="skill-input" placeholder="Type a skill and press Enter" />
                <button type="button" class="btn btn-outline" id="add-skill">Add</button>
              </div>
            </section>
          </div>
          <div class="form-card-footer">
            <button class="btn btn-primary" type="submit">Save profile</button>
          </div>
        </form>
        <div class="card form-card" id="cv">
          <div class="form-card-body">
            <section class="form-section">
              <h3 class="form-section-title">Curriculum vitae</h3>
              <p class="muted" style="margin:0">PDF, DOC, DOCX · max 5 MB</p>
              ${
                cv
                  ? `<div class="doc-row" style="margin-top:0.35rem">
                       <div class="doc-row-main">
                         <span class="doc-name">${esc(cv.fileName || cv.originalFileName || "Uploaded")}</span>
                         <span class="doc-date">${esc(cv.fileType || "")}</span>
                         <span class="doc-date">${SR.utils.fileSize(cv.fileSize)}</span>
                         <span class="doc-date">${SR.utils.formatDate(cv.uploadedAt)}</span>
                       </div>
                     </div>`
                  : `<p class="muted" style="margin:0.35rem 0 0">No CV uploaded yet.</p>`
              }
              <form id="cv-form" class="form-grid" style="margin-top:0.35rem">
                <label class="field"><span class="field-label">Choose file</span>
                  <input name="file" type="file" accept=".pdf,.doc,.docx,application/pdf" required />
                </label>
              </form>
            </section>
          </div>
          <div class="form-card-footer">
            <button class="btn btn-navy" type="submit" form="cv-form">${cv ? "Replace CV" : "Upload CV"}</button>
          </div>
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
    const ctx = await bootJobs(
      "Find Jobs",
      "Browse open vacancies — no account needed to explore."
    );
    if (!ctx) return;
    if (!ctx.guest) {
      const sub = document.querySelector(".page-head p");
      if (sub) sub.textContent = "Browse open vacancies and see how well you match.";
    }
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
        <div class="jobs-results-pane">
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
      const useAuth = SR.auth.isLoggedIn();
      const items = await SR.api.get(`/api/jobs?${q}`, { auth: useAuth });
      let list = Array.isArray(items) ? items : items.items || [];

      // If signed in as job seeker but API did not personalize scores, retry once with auth.
      const personalized = list.some(
        (j) => j.hasMatchScore === true || j.HasMatchScore === true
      );
      if (!ctx.guest && useAuth && list.length && !personalized) {
        list = await SR.api.get(`/api/jobs?${q}&_ts=${Date.now()}`, { auth: true });
        list = Array.isArray(list) ? list : list.items || [];
      }

      if (!ctx.guest) {
        list = list
          .slice()
          .sort(
            (a, b) =>
              Number(b.matchScore ?? b.MatchScore ?? 0) -
              Number(a.matchScore ?? a.MatchScore ?? 0)
          );
      }
      document.getElementById("next-page").disabled = list.length < pageSize;
      document.getElementById("prev-page").disabled = pageNumber <= 1;
      document.getElementById("next-page").onclick = () => {
        if (list.length >= pageSize) go(pageNumber + 1);
      };

      if (!list.length) {
        results.innerHTML = SR.ui.empty("No jobs found. Try adjusting filters.");
        return;
      }

      results.innerHTML = list
        .map((j) => {
          const desc = ((j.description || j.Description || "") + "").slice(0, 160);
          const score = Number(j.matchScore ?? j.MatchScore ?? 0);
          const showScore =
            !ctx.guest &&
            (j.hasMatchScore === true ||
              j.HasMatchScore === true ||
              score > 0);
          return `
          <a class="list-item ${showScore ? "list-item-with-score" : ""}" href="/module2-jobseeker/job-details.html?id=${j.id ?? j.Id}">
            <div class="list-item-main">
              <h3>${SR.utils.escape(j.jobTitle || j.JobTitle || "Job")}</h3>
              <div class="meta">
                <span>${SR.utils.escape(j.companyName || j.CompanyName || "")}</span>
                <span>${SR.utils.escape(j.location || j.Location || "")}</span>
                <span>${j.requiredExperience ?? j.RequiredExperience ?? 0} yrs exp</span>
                ${showScore ? SR.utils.matchPill(score) : ""}
              </div>
              <p class="muted" style="margin-top:0.55rem">${SR.utils.escape(desc)}${((j.description || j.Description || "") + "").length > 160 ? "…" : ""}</p>
              <div class="chip-row" style="margin-top:0.65rem">${(j.requiredSkills || j.RequiredSkills || [])
                .slice(0, 6)
                .map((s) => `<span class="chip">${SR.utils.escape(s)}</span>`)
                .join("")}</div>
              <div class="row-actions"><span class="btn btn-outline btn-sm">View Details</span></div>
            </div>
            ${showScore ? SR.utils.matchSide(score) : ""}
          </a>`;
        })
        .join("");

      if (!ctx.guest && list.length && !list.some((j) => j.hasMatchScore === true || j.HasMatchScore === true || Number(j.matchScore ?? j.MatchScore ?? 0) > 0)) {
        SR.ui.toast("Match scores need a refresh — sign out and sign in again.", "error");
      }
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
      const cannotApplyReason =
        job.cannotApplyReason ||
        job.canApplyReason ||
        job.applyBlockedReason ||
        "You cannot apply to this vacancy right now. Complete your profile or check if you already applied.";
      const actions = ctx.guest
        ? `<a class="btn btn-primary" id="guest-apply-btn" href="${loginApplyHref}">Apply Now</a>
           <a class="btn btn-outline" href="/register.html">Create Account</a>`
        : job.canApply
          ? `<button class="btn btn-primary" id="apply-btn" type="button">Apply Now</button>
             <button class="btn btn-danger btn-sm" id="report-btn" type="button">Report Job</button>`
          : `<button class="btn btn-outline" type="button" disabled>Cannot apply</button>
             <button class="btn btn-danger btn-sm" id="report-btn" type="button">Report Job</button>
             <p class="muted" style="margin-top:0.65rem;font-size:0.88rem">${SR.utils.escape(cannotApplyReason)}</p>`;

      const matchPanel = ctx.guest
        ? `<div class="card match-panel" style="margin-bottom:1rem">
              <p class="muted">Sign in as a Job Seeker to see your personal match score.</p>
              <div class="row-actions" style="margin-top:1rem">${actions}</div>
            </div>`
        : `<div class="card match-panel" style="margin-bottom:1rem">
              <div class="score">${SR.utils.matchPercent(job.matchScore ?? job.MatchScore)}%</div>
              <p class="muted">Overall match score</p>
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
        <a class="back-link" href="/module2-jobseeker/jobs.html">← Back to jobs</a>
        <div class="grid-2" style="margin-top:0.75rem">
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
