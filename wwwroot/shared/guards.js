window.SR = window.SR || {};

SR.guards = (function () {
  async function requireAuth(roles) {
    if (!SR.auth.isLoggedIn()) {
      location.href = "/login.html";
      return null;
    }
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
      localStorage.setItem("smartRecruit_user", JSON.stringify(user));
      if (roles && roles.length && !roles.includes(role)) {
        location.href = "/forbidden.html";
        return null;
      }
      return user;
    } catch (err) {
      if (err.status === 401) return null;
      if (err.status === 403) {
        location.href = "/forbidden.html";
        return null;
      }
      SR.ui.toast(err.message || "Session check failed", "error");
      return null;
    }
  }

  function redirectIfAuthed() {
    if (SR.auth.isLoggedIn()) {
      const user = SR.auth.getUser();
      location.href = SR.auth.homeForRole(user?.role);
      return true;
    }
    return false;
  }

  return { requireAuth, redirectIfAuthed };
})();
