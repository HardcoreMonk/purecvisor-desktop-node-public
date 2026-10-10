// @ts-nocheck
// Ported from purecvisor ui/app.js (Apache-2.0, same author): bootstrap position and window.PCV conventions.
// Desktop Node bootstrap for web/app.bundle.js (ADR-0018). The campaign single-edge-frontend-structure-20261010
// fills this file in Tasks 6-14; until Task 16 the legacy web/app.js stays the served console and this bundle
// is built and checked only.
window.PCV = window.PCV || {};
window.PCV.config = Object.assign({}, window.PCV.config || {}, {
  EDITION: 'desktop-node',
  API_BASE: '/api/v1',
  BUNDLE: 'app.bundle.js'
});
