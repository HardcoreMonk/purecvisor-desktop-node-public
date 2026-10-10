// Ported from purecvisor ui/sw.js (Apache-2.0, same author) for the Desktop Node Web Console (ADR-0018,
// pcv-single-edge-frontend-structure-v1 §3). Desktop Node changes: root-scope paths instead of /ui/, the precache list is
// the Desktop Node web payload, /pcv-config.js (the Host-generated runtime config) and /api/ are never cached, requests to
// other origins (the Local API on its own port) pass through, and the Web Push handlers (push, pushsubscriptionchange,
// notificationclick) are dropped because the Desktop Node has no push backend. CACHE_NAME is bumped by
// scripts/build-served-asset.mjs whenever the bundle or a precache input changes.
const CACHE_NAME = 'pcv-ui-vbcc96961';
const OFFLINE_URL = '/offline.html';
const STATIC_ASSETS = [
  '/',
  '/index.html',
  '/docs.html',
  '/guide.html',
  '/guide-content.md',
  '/offline.html',
  '/style.css',
  '/app.bundle.js',
  '/i18n.js',
  '/vendor/chart.umd.min.js',
  '/vendor/pretendard/pretendard.css',
  '/vendor/coolicons/coolicons.svg',
  '/manifest.json',
  '/icon-192.png',
  '/icon-512.png'
];
const NETWORK_ONLY = ['/pcv-config.js'];

self.addEventListener('install', event => {
  event.waitUntil(
    caches.open(CACHE_NAME).then(cache =>
      Promise.all(STATIC_ASSETS.map(url =>
        cache.add(new Request(url, { cache: 'reload' })).catch(err => {
          console.warn('[SW] precache miss:', url, err.message);
        })
      ))
    )
  );
  self.skipWaiting();
});

self.addEventListener('activate', event => {
  event.waitUntil(
    caches.keys().then(keys =>
      Promise.all(keys.filter(k => k !== CACHE_NAME).map(k => caches.delete(k)))
    ).then(() => self.clients.claim())
  );
});

self.addEventListener('message', event => {
  if (event.data && event.data.type === 'SKIP_WAITING') {
    self.skipWaiting();
  }
  if (event.data && event.data.type === 'CLEAR_CACHE') {
    caches.keys().then(keys => Promise.all(keys.map(k => caches.delete(k))))
      .then(() => event.ports[0] && event.ports[0].postMessage({ ok: true }));
  }
});

function networkFirst(req, fallback) {
  return fetch(req).then(r => {
    if (r.ok) {
      const clone = r.clone();
      caches.open(CACHE_NAME).then(c => c.put(req, clone));
    }
    return r;
  }).catch(fallback);
}

self.addEventListener('fetch', event => {
  const req = event.request;
  if (req.method !== 'GET') return;
  const url = new URL(req.url);
  if (url.origin !== self.location.origin) return;
  if (url.pathname.startsWith('/api/') || NETWORK_ONLY.indexOf(url.pathname) !== -1) return;
  const acc = req.headers.get('accept') || '';
  if (req.mode === 'navigate' || acc.indexOf('text/html') !== -1) {
    event.respondWith(networkFirst(req, async () => {
      return (await caches.match(req))
        || (await caches.match(OFFLINE_URL))
        || (await caches.match('/index.html'));
    }));
    return;
  }
  if (STATIC_ASSETS.indexOf(url.pathname) !== -1 || url.pathname.startsWith('/vendor/')) {
    event.respondWith(networkFirst(req, () => caches.match(req)));
  }
});
