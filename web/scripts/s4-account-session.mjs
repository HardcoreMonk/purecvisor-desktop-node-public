// Account session for run-s4-lan-scenario.mjs (BL-0035). The access token lives 15 minutes, while S3 can wait about
// 62 minutes for the first scheduled checkpoint. The session refreshes the access token before it expires with
// POST /api/v1/auth/refresh (the route the Web Console uses) and, when refresh fails, logs in again with the password
// it holds in memory. Tokens and the password stay in memory; onSecret lets the caller redact every issued token.
export const AUTH_LOGIN_ROUTE = "/api/v1/auth/login";
export const AUTH_REFRESH_ROUTE = "/api/v1/auth/refresh";
const REFRESH_MARGIN_MAX_MS = 60000;

// Refresh one minute before expiry, or after four fifths of a short lifetime.
export function refreshDelayMs(expiresInSeconds) {
  const lifetimeMs = expiresInSeconds * 1000;
  return Math.max(0, lifetimeMs - Math.min(REFRESH_MARGIN_MAX_MS, lifetimeMs / 5));
}

// request(method, path, body) sends an unauthenticated call and resolves to { status, payload }.
export function createAccountSession({ request, username, password, now = () => Date.now(), onSecret = () => {} }) {
  let accessToken = "";
  let refreshToken = "";
  let refreshDueAt = Number.POSITIVE_INFINITY;
  const counts = { refreshes: 0, relogins: 0, failures: 0 };

  function accept(payload) {
    const data = payload?.data;
    const access = typeof data?.access_token === "string" ? data.access_token : "";
    if (!access) return false;
    accessToken = access;
    refreshToken = typeof data?.refresh_token === "string" ? data.refresh_token : "";
    onSecret(accessToken);
    if (refreshToken) onSecret(refreshToken);
    const expiresIn = Number(data?.expires_in);
    refreshDueAt = Number.isFinite(expiresIn) && expiresIn > 0 ? now() + refreshDelayMs(expiresIn) : Number.POSITIVE_INFINITY;
    return true;
  }

  async function login() {
    const result = await request("POST", AUTH_LOGIN_ROUTE, { username, password });
    const ok = result.status === 200 && accept(result.payload);
    return { ...result, ok };
  }

  async function refresh() {
    if (!refreshToken) return false;
    try {
      const result = await request("POST", AUTH_REFRESH_ROUTE, { refresh_token: refreshToken });
      return result.status === 200 && accept(result.payload);
    } catch {
      return false;
    }
  }

  // Returns "refresh", "login", or "" when neither refresh nor re-login produced a token.
  async function reauthenticate() {
    if (await refresh()) {
      counts.refreshes += 1;
      return "refresh";
    }
    try {
      if ((await login()).ok) {
        counts.relogins += 1;
        return "login";
      }
    } catch {
      // The failure is counted below; the caller keeps the old token and sees the next 401.
    }
    counts.failures += 1;
    return "";
  }

  // Returns "current" while the access token is not yet due for refresh, "" without a session.
  async function ensureFresh() {
    if (!accessToken) return "";
    if (now() < refreshDueAt) return "current";
    return reauthenticate();
  }

  return {
    login,
    ensureFresh,
    reauthenticate,
    get token() {
      return accessToken;
    },
    get counts() {
      return { ...counts };
    }
  };
}
