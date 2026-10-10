// @ts-nocheck
// Ported from purecvisor ui/modules/endpoints.js (Apache-2.0, same author) for the Desktop Node Web Console (ADR-0018,
// pcv-single-edge-frontend-structure-v1). Desktop Node changes: the surface lists the Desktop Node Local API routes of
// the legacy web/src/served/routes.ts registry only (no containers, storage, OVN, VPC, Suricata, DPDK, SR-IOV, push, RPC
// or TOTP), API_BASE comes from /pcv-config.js, actions are validated against the route contract and there is no edition
// switch (the Desktop Node is one edition).
window.PCV = window.PCV || {};
(function (PCV) {
var EP = (function () {
  function configuredApiBase() {
    var cfg = window.PCV_DESKTOP_NODE_CONFIG;
    var origin = cfg && typeof cfg.apiBaseUrl === 'string' ? cfg.apiBaseUrl.replace(/\/$/, '') : '';
    return origin + '/api/v1';
  }
  if (!window.API_BASE) window.API_BASE = configuredApiBase();
  var B = function () { return window.API_BASE || configuredApiBase(); };
  var enc = encodeURIComponent;
  var VM_ACTIONS = ['start', 'shutdown', 'poweroff', 'restart', 'save', 'resume-saved', 'pause', 'resume', 'rename', 'eject',
    'attach', 'delete-status', 'set-memory', 'set-vcpu', 'disk-resize', 'manage', 'clone', 'template-lock'];
  var CHECKPOINT_ACTIONS = ['restore'];
  var JOB_ACTIONS = ['cancel', 'retry', 'reconcile'];
  function action(allowed, a) {
    if (allowed.indexOf(a) === -1) throw new Error('PCV_ROUTE_ACTION_INVALID: ' + a);
    return enc(a);
  }
  function page(path, limit, offset) {
    var l = Number.isInteger(limit) && limit > 0 ? limit : 50;
    var o = Number.isInteger(offset) && offset >= 0 ? offset : 0;
    return B() + path + '?limit=' + enc(l) + '&offset=' + enc(o);
  }
  var DESKTOP_NODE_ENDPOINTS = {
    OPS_SUMMARY:                function()      { return B() + '/ops/summary'; },
    RUNTIME_POLICY:             function()      { return B() + '/runtime/policy'; },
    HOST_STATUS:                function()      { return B() + '/host/status'; },
    NETWORK_INVENTORY:          function()      { return B() + '/network/inventory'; },
    CONSOLE_CAPABILITIES:       function()      { return B() + '/console/capabilities'; },
    VM_LIST:                    function()      { return B() + '/vms'; },
    VM_CREATE:                  function()      { return B() + '/vms'; },
    VM_DETAIL:                  function(n)     { return B() + '/vms/' + enc(n); },
    VM_ACTION:                  function(n, a)  { return B() + '/vms/' + enc(n) + '/' + action(VM_ACTIONS, a); },
    VM_DELETE_STATUS:           function(n)     { return B() + '/vms/' + enc(n) + '/delete-status'; },
    VM_CLONE_PREVIEW:           function(n)     { return B() + '/vms/' + enc(n) + '/clone/preview'; },
    VM_BLKIO:                   function(n)     { return B() + '/vms/' + enc(n) + '/blkio'; },
    VM_BANDWIDTH:               function(n)     { return B() + '/vms/' + enc(n) + '/bandwidth'; },
    VM_MEMORY_STATS:            function(n)     { return B() + '/vms/' + enc(n) + '/memory-stats'; },
    VM_CPU_STATS:               function(n)     { return B() + '/vms/' + enc(n) + '/cpu-stats'; },
    VM_QOS_STORAGE_PREVIEW:     function(n)     { return B() + '/vms/' + enc(n) + '/qos/storage/preview'; },
    VM_QOS_STORAGE:             function(n)     { return B() + '/vms/' + enc(n) + '/qos/storage'; },
    VM_QOS_NETWORK_PREVIEW:     function(n)     { return B() + '/vms/' + enc(n) + '/qos/network/preview'; },
    VM_QOS_NETWORK:             function(n)     { return B() + '/vms/' + enc(n) + '/qos/network'; },
    VM_GUEST_AGENT_STATUS:      function(n)     { return B() + '/vms/' + enc(n) + '/guest-agent/status'; },
    VM_GUEST_AGENT_PING:        function(n)     { return B() + '/vms/' + enc(n) + '/guest-agent/ping'; },
    VM_GUEST_EXEC_PREVIEW:      function(n)     { return B() + '/vms/' + enc(n) + '/guest/exec/preview'; },
    VM_GUEST_EXEC:              function(n)     { return B() + '/vms/' + enc(n) + '/guest/exec'; },
    VM_GUEST_FILE_PREVIEW:      function(n)     { return B() + '/vms/' + enc(n) + '/guest/file/preview'; },
    VM_GUEST_FILE:              function(n)     { return B() + '/vms/' + enc(n) + '/guest/file'; },
    VM_GUEST_CHANNEL_PREVIEW:   function(n)     { return B() + '/vms/' + enc(n) + '/guest/channel/preview'; },
    VM_GUEST_CHANNEL_VERIFY:    function(n)     { return B() + '/vms/' + enc(n) + '/guest/channel/verify'; },
    VM_GUEST_CHANNEL_ENSURE:    function(n)     { return B() + '/vms/' + enc(n) + '/guest/channel'; },
    VM_NETWORK:                 function(n)     { return B() + '/vms/' + enc(n) + '/network'; },
    VM_DEVICES:                 function(n)     { return B() + '/vms/' + enc(n) + '/devices'; },
    VM_EXPORT_PREVIEW:          function(n)     { return B() + '/vms/' + enc(n) + '/export/preview'; },
    VM_EXPORT:                  function(n)     { return B() + '/vms/' + enc(n) + '/export'; },
    VM_IMPORT_PREVIEW:          function()      { return B() + '/vms/import/preview'; },
    VM_IMPORT:                  function()      { return B() + '/vms/import'; },
    VM_CHECKPOINTS:             function(n)     { return B() + '/vms/' + enc(n) + '/checkpoints'; },
    CHECKPOINT:                 function(n, c)  { return B() + '/vms/' + enc(n) + '/checkpoints/' + enc(c); },
    CHECKPOINT_ACTION:          function(n, c, a) { return B() + '/vms/' + enc(n) + '/checkpoints/' + enc(c) + '/' + action(CHECKPOINT_ACTIONS, a); },
    CHECKPOINT_SCHEDULE_PREVIEW:function(n)     { return B() + '/vms/' + enc(n) + '/checkpoints/schedule/preview'; },
    CHECKPOINT_SCHEDULE:        function(n)     { return B() + '/vms/' + enc(n) + '/checkpoints/schedule'; },
    CHECKPOINT_SCHEDULE_CLEAR:  function(n)     { return B() + '/vms/' + enc(n) + '/checkpoints/schedule/clear'; },
    VM_CONSOLE:                 function(n)     { return B() + '/vms/' + enc(n) + '/console'; },
    VM_CONSOLE_FRAME:           function(n, s)  { return B() + '/vms/' + enc(n) + '/console/frame/' + enc(s); },
    VM_CONSOLE_INPUT:           function(n)     { return B() + '/vms/' + enc(n) + '/console/input'; },
    JOB_LIST:                   function()      { return B() + '/jobs'; },
    JOBS_PAGE:                  function(limit, offset) { return page('/jobs', limit, offset); },
    JOB:                        function(id)    { return B() + '/jobs/' + enc(id); },
    JOB_ACTION:                 function(id, a) { return B() + '/jobs/' + enc(id) + '/' + action(JOB_ACTIONS, a); },
    DIAGNOSTIC_BUNDLES:         function()      { return B() + '/diagnostics/bundles'; },
    DIAGNOSTIC_BUNDLES_PAGE:    function(limit, offset) { return page('/diagnostics/bundles', Number.isInteger(limit) ? limit : 10, offset); },
    DIAGNOSTIC_BUNDLE_DOWNLOAD: function(id)    { return B() + '/diagnostics/bundles/' + enc(id) + '/download'; },
    AUTH_LOGIN:                 function()      { return B() + '/auth/login'; },
    AUTH_LOOPBACK_SESSION:      function()      { return B() + '/auth/loopback-session'; },
    AUTH_REFRESH:               function()      { return B() + '/auth/refresh'; },
    AUTH_LOGOUT:                function()      { return B() + '/auth/logout'; },
    AUTH_SESSION:               function()      { return B() + '/auth/session'; },
    AUTH_RBAC:                  function()      { return B() + '/auth/rbac'; },
    ACCOUNTS:                   function()      { return B() + '/accounts'; },
    ACCOUNT_DISABLE:            function(u)     { return B() + '/accounts/' + enc(u) + '/disable'; },
    // Single Edge names kept for the ported modal module: registration is the Desktop Node account create route and
    // there is no password change route on the Desktop Node (null makes the modal report the missing route).
    AUTH_REGISTER:              function()      { return B() + '/accounts'; },
    AUTH_PASSWORD:              function()      { return null; },
    VM_ACTIONS: VM_ACTIONS,
    CHECKPOINT_ACTIONS: CHECKPOINT_ACTIONS,
    JOB_ACTIONS: JOB_ACTIONS
  };
  PCV.getOptionalEndpoint = function (name) {
    var registry = window.EP || DESKTOP_NODE_ENDPOINTS;
    var fn = registry[name];
    if (typeof fn !== 'function') return null;
    return fn.apply(null, Array.prototype.slice.call(arguments, 1));
  };
  PCV.configuredApiBase = configuredApiBase;
  return DESKTOP_NODE_ENDPOINTS;
})();
PCV.endpoints = EP;
window.EP = EP;
})(window.PCV);
