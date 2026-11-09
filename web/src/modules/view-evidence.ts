// @ts-nocheck
// Desktop Node module (ADR-0018): evidence operator cards on the U4a view registry.
// #evidence-panel stays on the legacy painter because its English strings are pinned.
window.PCV = window.PCV || {};
(function (PCV) {
  function node() {
    return PCV.uxlib.el;
  }

  function text(value) {
    if (value === undefined || value === null || value === '') return '';
    return String(value);
  }

  function dash(value) {
    return text(value) || '-';
  }

  function sources() {
    var evidence = typeof getBatchEvidence === 'function' ? getBatchEvidence() : null;
    var latest = evidence && typeof asObject === 'function' ? asObject(evidence.latest) : {};
    var rollup = typeof getCurrentEvidenceRollup === 'function' ? getCurrentEvidenceRollup() : null;
    return {
      evidence: evidence || {},
      latest: latest,
      release: typeof asObject === 'function' ? asObject(latest.release) : {},
      gpu: typeof asObject === 'function' ? asObject(latest.gpu_snapshots) : {},
      route: typeof asObject === 'function' ? asObject(latest.route_msi_hyperv) : {},
      os: typeof asObject === 'function' ? asObject(latest.os_mutation) : {},
      host: typeof asObject === 'function' ? asObject(latest.host_final_state) : {},
      rollup: rollup || {},
      fullAdmin: typeof getCurrentEvidenceFullAdmin === 'function' ? getCurrentEvidenceFullAdmin(rollup) : {},
      boundary: typeof getCurrentEvidencePublicBoundary === 'function' ? getCurrentEvidencePublicBoundary(rollup) : {},
      packagePair: typeof getCurrentEvidencePackagePair === 'function' ? getCurrentEvidencePackagePair(rollup) : {},
      nextPair: typeof getCurrentEvidenceNextPackagePair === 'function' ? getCurrentEvidenceNextPackagePair(rollup) : {},
      hostOps: typeof getCurrentEvidenceHostOps === 'function' ? getCurrentEvidenceHostOps(rollup) : {}
    };
  }

  function installedVersion(src) {
    return dash(src.release.version);
  }

  function operationalCurrent(src) {
    return dash(text(src.fullAdmin.version) || text(src.fullAdmin.batch_id));
  }

  function lastVerification(src) {
    return dash(
      text(src.latest.generated_at) ||
      text(src.latest.completed_at) ||
      text(src.latest.finished_at) ||
      text(src.latest.observed_at) ||
      text(src.evidence.generated_at) ||
      text(src.rollup.generated_at)
    );
  }

  function flag(value) {
    if (value === true || value === false) {
      return typeof evidenceBooleanLabel === 'function' ? evidenceBooleanLabel(value) : String(value);
    }
    return dash(value);
  }

  function card(el, name, label, value) {
    return el('section', { 'data-evidence-card': name },
      el('span', { class: 'muted' }, label),
      el('strong', { 'data-evidence-value': name }, value));
  }

  function extra(el, key, label, value) {
    return el('div', { 'data-evidence-extra': key },
      el('span', { class: 'muted' }, label),
      el('strong', null, value));
  }

  function resolveMount(passed) {
    if (passed && typeof passed.hasAttribute === 'function' && passed.hasAttribute('data-view-mount')) return passed;
    var byId = document.getElementById('evidence-cards');
    if (byId && byId !== passed) return byId;
    return null;
  }

  function paintCards(target, src) {
    var el = node();
    PCV.uxlib.clearEl(target);
    target.appendChild(card(el, 'installed', t('copy.evidence.card.installed'), installedVersion(src)));
    target.appendChild(card(el, 'current', t('copy.evidence.card.current'), operationalCurrent(src)));
    target.appendChild(card(el, 'verified', t('copy.evidence.card.verified'), lastVerification(src)));
  }

  function paintRelease(src) {
    var body = document.getElementById('evidence-release-body');
    if (!body || !PCV.uxlib) return;
    var el = node();
    var gpuCount = text(src.gpu.count) || '0';
    var firewall = src.host.firewall_rule_count;
    if (firewall === undefined || firewall === null || firewall === '') firewall = src.os.firewall_rule_count;
    PCV.uxlib.clearEl(body);
    body.appendChild(el('div', { 'data-evidence-gpu': '1' },
      el('span', { class: 'muted' }, 'GPU snapshots'),
      el('strong', { 'data-evidence-gpu-count': '1' }, gpuCount),
      el('span', { class: 'muted' }, 'GPU peak MiB'),
      el('strong', { 'data-evidence-gpu-peak': '1' }, dash(src.gpu.peak_adapter_mib))));
    body.appendChild(el('div', { 'data-evidence-boundary': '1' },
      el('span', { class: 'muted' }, 'Public boundary head'),
      el('strong', { 'data-evidence-head': '1' }, dash(src.boundary.head_sha))));
    body.appendChild(extra(el, 'signing', 'Signing', dash(src.release.signing_mode)));
    body.appendChild(extra(el, 'service', 'Service', dash(src.host.service_state)));
    body.appendChild(extra(el, 'route', 'Route/MSI', flag(src.route.ok)));
    body.appendChild(extra(el, 'os', 'OS gate', flag(src.os.ok)));
    body.appendChild(extra(el, 'firewall', 'Firewall final', dash(firewall === undefined || firewall === null || firewall === '' ? '' : firewall)));
    body.appendChild(extra(el, 'public-signing', 'Public signing', dash(text(src.release.public_trusted_signing) || 'excluded')));
    body.appendChild(extra(el, 'publication', 'External publication', dash(text(src.release.external_stable_publication) || 'not-claimed')));
    body.appendChild(extra(el, 'boundary-run', 'Public boundary', dash(text(src.boundary.run_id) || text(src.boundary.status))));
    body.appendChild(extra(el, 'full-admin', 'Full admin', dash(text(src.fullAdmin.version) || text(src.fullAdmin.batch_id))));
    body.appendChild(extra(el, 'manual-admin', 'Manual admin', dash(text(src.packagePair.package_pair) || text(src.packagePair.status))));
    body.appendChild(extra(el, 'manual-next', 'Manual admin next', dash(text(src.nextPair.package_pair) || text(src.nextPair.status))));
    body.appendChild(extra(el, 'host-ops', 'Host Ops', dash(text(src.hostOps.contract_key) || text(src.hostOps.status))));
  }

  function renderEvidenceView(mount, ctx) {
    if (ctx && typeof ctx.isCurrent === 'function' && !ctx.isCurrent()) return;
    if (!PCV.uxlib || typeof PCV.uxlib.el !== 'function' || typeof PCV.uxlib.clearEl !== 'function') return;
    var src = sources();
    var target = resolveMount(mount);
    if (target) paintCards(target, src);
    paintRelease(src);
  }

  PCV.viewEvidence = { render: renderEvidenceView };
  PCV.views.register('evidence', { render: renderEvidenceView });
})(window.PCV);
