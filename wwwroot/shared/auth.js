window.SR = window.SR || {};

SR.auth = (function () {
  const KEYS = {
    token: "smartRecruit_token",
    user: "smartRecruit_user",
    expiration: "smartRecruit_expiration",
  };

  function getToken() {
    return localStorage.getItem(KEYS.token);
  }

  function getUser() {
    try {
      return JSON.parse(localStorage.getItem(KEYS.user) || "null");
    } catch {
      return null;
    }
  }

  function setSession(result) {
    const token = result.token || result.Token;
    const user = {
      userId: result.userId ?? result.UserId,
      fullName: result.fullName || result.FullName,
      email: result.email || result.Email,
      role: result.role || result.Role,
    };
    localStorage.setItem(KEYS.token, token || "");
    localStorage.setItem(KEYS.user, JSON.stringify(user));
    localStorage.setItem(
      KEYS.expiration,
      String(result.expiration || result.Expiration || "")
    );
    return user;
  }

  function clear() {
    localStorage.removeItem(KEYS.token);
    localStorage.removeItem(KEYS.user);
    localStorage.removeItem(KEYS.expiration);
  }

  function isLoggedIn() {
    return !!getToken();
  }

  function homeForRole(role) {
    switch (role) {
      case "JobSeeker":
        return "/module2-jobseeker/dashboard.html";
      case "Employer":
        return "/module3-employer/employer-dashboard.html";
      case "Administrator":
        return "/module1-core/admin-dashboard.html";
      default:
        return "/index.html";
    }
  }

  async function login(email, password) {
    const result = await SR.api.post(
      "/api/Auth/login",
      { email, password },
      { auth: false }
    );
    if (result && result.success === false) {
      throw new Error(result.message || "Login failed");
    }
    return setSession(result);
  }

  async function registerJobSeeker(payload) {
    return SR.api.post("/api/Auth/register/job-seeker", payload, { auth: false });
  }

  async function registerEmployer(payload) {
    return SR.api.post("/api/Auth/register/employer", payload, { auth: false });
  }

  async function me() {
    return SR.api.get("/api/Auth/me");
  }

  function logout() {
    clear();
    location.href = "/login.html";
  }

  return {
    getToken,
    getUser,
    setSession,
    clear,
    isLoggedIn,
    homeForRole,
    login,
    registerJobSeeker,
    registerEmployer,
    me,
    logout,
  };
})();
