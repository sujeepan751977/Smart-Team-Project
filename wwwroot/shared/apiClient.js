window.SR = window.SR || {};

SR.api = (function () {
  function extractError(data, status) {
    if (!data) return `Request failed (${status})`;
    if (typeof data === "string") return data;
    const direct = data.message || data.Message || data.title || data.Title;
    const errors = data.errors || data.Errors;
    if (errors && typeof errors === "object") {
      const parts = [];
      Object.keys(errors).forEach((k) => {
        const v = errors[k];
        if (Array.isArray(v)) parts.push(...v);
        else if (v) parts.push(String(v));
      });
      if (parts.length) return parts.join(" ");
    }
    return direct || `Request failed (${status})`;
  }

  async function request(method, path, body, options = {}) {
    const headers = {};
    const auth = options.auth !== false;
    const token = SR.auth.getToken();
    if (auth && token) headers.Authorization = `Bearer ${token}`;

    const init = { method: method.toUpperCase(), headers };

    if (options.formData instanceof FormData) {
      init.body = options.formData;
    } else if (body !== undefined && body !== null) {
      headers["Content-Type"] = "application/json";
      init.body = JSON.stringify(body);
    }

    const res = await fetch(path, init);
    if (res.status === 204) return null;

    const text = await res.text();
    let data = null;
    if (text) {
      try {
        data = JSON.parse(text);
      } catch {
        data = text;
      }
    }

    if (!res.ok) {
      if (res.status === 401) {
        SR.auth.clear();
        if (!location.pathname.endsWith("/login.html")) {
          location.href = "/login.html";
        }
      }
      if (res.status === 403 && options.redirectForbidden !== false) {
        /* caller may handle; default redirect for page boots */
      }
      const err = new Error(extractError(data, res.status));
      err.status = res.status;
      err.data = data;
      throw err;
    }
    return data;
  }

  return {
    request,
    get: (p, o) => request("GET", p, null, o),
    post: (p, b, o) => request("POST", p, b, o),
    put: (p, b, o) => request("PUT", p, b, o),
    patch: (p, b, o) => request("PATCH", p, b, o),
    delete: (p, o) => request("DELETE", p, null, o),
  };
})();
