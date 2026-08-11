window.SR = window.SR || {};

SR.utils = {
  escape(value) {
    return String(value ?? "")
      .replaceAll("&", "&amp;")
      .replaceAll("<", "&lt;")
      .replaceAll(">", "&gt;")
      .replaceAll('"', "&quot;")
      .replaceAll("'", "&#39;");
  },

  text(el, value) {
    if (el) el.textContent = value ?? "";
  },

  qs(name) {
    return new URLSearchParams(location.search).get(name);
  },

  formToObject(form) {
    const data = {};
    new FormData(form).forEach((v, k) => {
      data[k] = typeof v === "string" ? v.trim() : v;
    });
    return data;
  },

  formatDate(value) {
    if (!value) return "—";
    const d = new Date(value);
    if (Number.isNaN(d.getTime())) return String(value);
    return d.toLocaleString();
  },

  formatDateShort(value) {
    if (!value) return "—";
    const d = new Date(value);
    if (Number.isNaN(d.getTime())) return String(value);
    return d.toLocaleDateString();
  },

  isValidEmail(email) {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(String(email || "").trim());
  },

  fileSize(bytes) {
    const n = Number(bytes) || 0;
    if (n < 1024) return `${n} B`;
    if (n < 1024 * 1024) return `${(n / 1024).toFixed(1)} KB`;
    return `${(n / (1024 * 1024)).toFixed(1)} MB`;
  },

  isHttpUrl(url) {
    try {
      const u = new URL(url);
      return u.protocol === "http:" || u.protocol === "https:";
    } catch {
      return false;
    }
  },

  matchPercent(value) {
    const n = Number(value);
    if (!Number.isFinite(n)) return 0;
    return Math.max(0, Math.min(100, Math.round(n)));
  },

  matchTone(value) {
    const p = SR.utils.matchPercent(value);
    if (p >= 70) return "high";
    if (p >= 40) return "mid";
    return "low";
  },

  matchPill(value) {
    const p = SR.utils.matchPercent(value);
    const tone = SR.utils.matchTone(p);
    return `<span class="match-pill" data-tone="${tone}" title="Match score"><span class="match-pill-value">${p}%</span> Match</span>`;
  },

  matchSide(value) {
    const p = SR.utils.matchPercent(value);
    const tone = SR.utils.matchTone(p);
    return `<div class="job-card-score" data-tone="${tone}" title="Your match score">
      <span class="job-card-score-value">${p}%</span>
      <span class="job-card-score-label">Match</span>
    </div>`;
  },
};

/* Enum / status helpers — never show raw numbers */
SR.status = {
  vacancy(v) {
    const map = {
      1: "Draft",
      2: "Pending Approval",
      3: "Open",
      4: "Rejected",
      5: "Closed",
      6: "Suspended",
      Draft: "Draft",
      PendingApproval: "Pending Approval",
      Open: "Open",
      Rejected: "Rejected",
      Closed: "Closed",
      Suspended: "Suspended",
    };
    return map[v] || String(v ?? "—");
  },
  vacancyKind(v) {
    const label = SR.status.vacancy(v);
    if (label === "Open") return "ok";
    if (label === "Rejected" || label === "Suspended") return "danger";
    if (label === "Pending Approval") return "warn";
    return "neutral";
  },
  verification(v) {
    const map = {
      1: "Unverified",
      2: "Pending Review",
      3: "More Information Required",
      4: "Verified",
      5: "Rejected",
      Unverified: "Unverified",
      PendingReview: "Pending Review",
      MoreInformationRequired: "More Information Required",
      Verified: "Verified",
      Rejected: "Rejected",
    };
    return map[v] || String(v ?? "—");
  },
  verificationKind(v) {
    const label = SR.status.verification(v);
    if (label === "Verified") return "ok";
    if (label === "Rejected") return "danger";
    if (label.includes("Pending") || label.includes("More")) return "warn";
    return "neutral";
  },
  document(v) {
    const map = { 1: "Pending", 2: "Accepted", 3: "Rejected", Pending: "Pending", Accepted: "Accepted", Rejected: "Rejected" };
    return map[v] || String(v ?? "—");
  },
  application(v) {
    const map = {
      1: "Applied",
      2: "Under Review",
      3: "Shortlisted",
      4: "Rejected",
      Applied: "Applied",
      UnderReview: "Under Review",
      Shortlisted: "Shortlisted",
      Rejected: "Rejected",
    };
    return map[v] || String(v || "—");
  },
  applicationKind(v) {
    const s = SR.status.application(v);
    if (s === "Shortlisted") return "ok";
    if (s === "Rejected") return "danger";
    if (s === "Under Review") return "warn";
    return "neutral";
  },
  contact(v) {
    const map = {
      1: "Pending",
      2: "Accepted",
      3: "Rejected",
      4: "Cancelled",
      Pending: "Pending",
      Accepted: "Accepted",
      Rejected: "Rejected",
      Cancelled: "Cancelled",
    };
    return map[v] || String(v || "—");
  },
  contactKind(v) {
    const s = SR.status.contact(v);
    if (s === "Accepted") return "ok";
    if (s === "Rejected" || s === "Cancelled") return "danger";
    if (s === "Pending") return "warn";
    return "neutral";
  },
  interview(v) {
    const map = {
      1: "Scheduled",
      2: "Completed",
      3: "Cancelled",
      4: "Rescheduled",
      5: "No Show",
      Scheduled: "Scheduled",
      Completed: "Completed",
      Cancelled: "Cancelled",
      Rescheduled: "Rescheduled",
      NoShow: "No Show",
    };
    return map[v] || String(v || "—");
  },
  interviewKind(v) {
    const s = SR.status.interview(v);
    if (s === "Completed") return "ok";
    if (s === "Cancelled" || s === "No Show") return "danger";
    if (s === "Rescheduled") return "warn";
    return "neutral";
  },
  notificationType(v) {
    const map = {
      1: "System",
      2: "Application",
      3: "Interview",
      4: "Employer Verification",
      5: "Vacancy Approval",
      6: "Contact Request",
      7: "Job Report",
      8: "Moderation",
    };
    return map[v] || String(v ?? "Notification");
  },
  reportReason(v) {
    const map = {
      1: "Fake Job",
      2: "Scam",
      3: "Spam",
      4: "Misleading Information",
      5: "Inappropriate Content",
      6: "Other",
      FakeJob: "Fake Job",
      Scam: "Scam",
      Spam: "Spam",
      MisleadingInformation: "Misleading Information",
      InappropriateContent: "Inappropriate Content",
      Other: "Other",
    };
    return map[v] || String(v ?? "—");
  },
  reportStatus(v) {
    const map = {
      1: "Pending",
      2: "Under Review",
      3: "Action Taken",
      4: "Rejected",
      Pending: "Pending",
      UnderReview: "Under Review",
      ActionTaken: "Action Taken",
      Resolved: "Action Taken",
      Dismissed: "Rejected",
      Rejected: "Rejected",
    };
    return map[v] || String(v ?? "—");
  },
  reportKind(v) {
    const label = SR.status.reportStatus(v);
    if (label === "Action Taken") return "ok";
    if (label === "Rejected") return "danger";
    if (label === "Under Review" || label === "Pending") return "warn";
    return "neutral";
  },
  moderationAction(v) {
    return String(v || "—");
  },
};
