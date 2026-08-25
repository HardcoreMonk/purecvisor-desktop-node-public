// @ts-nocheck
function isAuthError(error) {
  return ['PCV_AUTH_REQUIRED', 'PCV_AUTH_FORBIDDEN', 'PCV_LOGIN_FAILED', 'PCV_JWT_INVALID', 'PCV_JWT_EXPIRED', 'PCV_RBAC_FORBIDDEN'].includes(error?.code);
}

function tokenRequiredRouteStatus(error = state.error) {
  if (isAuthError(error)) {
    return 'token-required route rejected the browser token';
  }
  if (state.authAccessToken.trim()) {
    return `account JWT session active as ${getAccountRoleLabel()}`;
  }
  if (!state.apiToken.trim()) {
    return 'token-required routes may show Auth required';
  }
  return 'browser token present for token-required routes';
}

function getAccountRoleLabel() {
  return state.authSession?.role || state.authSession?.account?.role || 'unauthenticated';
}

function getAccountPermissions() {
  return asArray(state.authSession?.permissions || state.authSession?.account?.permissions)
    .map((permission) => String(permission || '').trim())
    .filter(Boolean);
}

function accountRbacModeEnabled() {
  const mode = String(readNested(state.runtimePolicy || {}, ['auth', 'mode']) || '').toLowerCase();
  const rbac = readNested(state.runtimePolicy || {}, ['auth', 'rbac']);
  return Boolean(state.authAccessToken || rbac === true || mode.includes('account'));
}

function rbacAllows(permission) {
  if (!accountRbacModeEnabled()) return true;
  const permissions = getAccountPermissions();
  return permissions.includes('*') || permissions.includes(permission);
}

function requireRbac(permission, actionLabel = 'this action') {
  if (rbacAllows(permission)) return;
  throw normalizeError({
    code: 'PCV_RBAC_FORBIDDEN',
    message: `The current account role cannot use ${actionLabel}.`,
    detail: `Required permission: ${permission}. Current role: ${getAccountRoleLabel()}.`
  });
}

function applyAccountSessionPayload(payload) {
  state.authAccessToken = String(payload?.access_token || state.authAccessToken || '');
  state.authRefreshToken = String(payload?.refresh_token || state.authRefreshToken || '');
  state.authSession = payload?.session || state.authSession || null;
  state.authError = null;
  saveAccountSessionToStorage();
}

