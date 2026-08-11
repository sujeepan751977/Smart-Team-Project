window.SR = window.SR || {};

SR.api = (function () {
  function extractError(data, status) {
    if (!data) {
      if (status === 401) return "Your session has expired. Please sign in again.";
      if (status === 403) return "You don't have permission to perform this action.";
      if (status === 404) return "The requested item could not be found.";
      if (status === 409) return "This action conflicts with the current state.";
      if (status >= 500) return "Something went wrong. Please try again.";
      return `Request failed (${status})`;
    }
    if (typeof data === "string") {
      if (data.trim().startsWith("<!")) {
        return status === 404
          ? "The requested item could not be found."
          : "Unexpected server response. Please try again.";
      }
      return data;
    }
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

  let redirecting401 = false;

  async function request(method, path, body, options = {}) {
    const headers = {};
    const auth = options.auth !== false;
    const token = SR.auth.getToken();
    if (auth && token) headers.Authorization = `Bearer ${token}`;

    const init = { method: method.toUpperCase(), headers, cache: "no-store" };

    if (options.formData instanceof FormData) {
      init.body = options.formData;
    } else if (body !== undefined && body !== null) {
      headers["Content-Type"] = "application/json";
      init.body = JSON.stringify(body);
    }

    let res;
    try {
      res = await fetch(path, init);
    } catch {
      const err = new Error(
        "Unable to reach the server. Check your connection and try again."
      );
      err.status = 0;
      throw err;
    }

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
        if (!redirecting401 && !location.pathname.endsWith("/login.html")) {
          redirecting401 = true;
          const next = encodeURIComponent(location.pathname + location.search);
          location.href = `/login.html?next=${next}`;
        }
      }
      if (res.status === 403 && options.redirectForbidden === true) {
        location.href = "/forbidden.html";
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
    async download(path, fallbackName = "download") {
      const token = SR.auth.getToken();
      const headers = {};
      if (token) headers.Authorization = `Bearer ${token}`;
      let res;
      try {
        res = await fetch(path, { headers });
      } catch {
        throw new Error("Unable to reach the server. Check your connection and try again.");
      }
      if (!res.ok) {
        const text = await res.text();
        let data = null;
        try {
          data = text ? JSON.parse(text) : null;
        } catch {
          data = text;
        }
        throw new Error(extractError(data, res.status));
      }
      const blob = await res.blob();
      const disposition = res.headers.get("Content-Disposition") || "";
      const match = /filename\*?=(?:UTF-8''|")?([^\";]+)/i.exec(disposition);
      const fileName = match
        ? decodeURIComponent(match[1].replace(/"/g, "").trim())
        : fallbackName;
      const url = URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = fileName;
      document.body.appendChild(a);
      a.click();
      a.remove();
      URL.revokeObjectURL(url);
    },
  };
})();
