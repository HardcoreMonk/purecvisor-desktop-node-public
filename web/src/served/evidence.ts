// @ts-nocheck

function stateBadge(value) {
  const text = String(value ?? 'unknown');
  const normalized = text.toLowerCase();
  const cls = normalized.includes('running') || normalized.includes('ready') || normalized === 'ok'
    ? 'badge-ok'
    : normalized.includes('fail') || normalized.includes('error') || normalized.includes('forbidden')
      ? 'badge-error'
      : 'badge-warn';
  return `<span class="badge ${cls}">${escapeHtml(text)}</span>`;
}

function asObject(value) {
  return value && typeof value === 'object' && !Array.isArray(value) ? value : {};
}

function getBatchEvidence() {
  const evidence = state.opsSummary?.batch_evidence || state.opsSummary?.batchEvidence;
  return evidence && typeof evidence === 'object' && !Array.isArray(evidence) ? evidence : null;
}

function getCurrentEvidenceRollup() {
  const rollup = state.opsSummary?.current_evidence || state.opsSummary?.currentEvidence;
  return rollup && typeof rollup === 'object' && !Array.isArray(rollup) ? rollup : null;
}

function getCurrentEvidenceFullAdmin(rollup = getCurrentEvidenceRollup()) {
  return asObject(readNested(rollup || {}, ['full_admin_host_mutation', 'latest']) || readNested(rollup || {}, ['fullAdminHostMutation', 'latest']));
}

function getCurrentEvidencePublicBoundary(rollup = getCurrentEvidenceRollup()) {
  return asObject(readNested(rollup || {}, ['public_boundary', 'latest_main_push']) || readNested(rollup || {}, ['publicBoundary', 'latestMainPush']));
}

function getCurrentEvidencePackagePair(rollup = getCurrentEvidenceRollup()) {
  return asObject(readNested(rollup || {}, ['manual_admin', 'latest_package_pair']) || readNested(rollup || {}, ['manualAdmin', 'latestPackagePair']));
}

function getCurrentEvidenceNextPackagePair(rollup = getCurrentEvidenceRollup()) {
  return asObject(readNested(rollup || {}, ['manual_admin', 'next_package_pair']) || readNested(rollup || {}, ['manualAdmin', 'nextPackagePair']));
}

function getCurrentEvidenceHostOps(rollup = getCurrentEvidenceRollup()) {
  return asObject(readNested(rollup || {}, ['host_ops', 'lifecycle_descriptor']) || readNested(rollup || {}, ['hostOps', 'lifecycleDescriptor']));
}

function getRuntimeApiRegistryBridge() {
  const summary = asObject(state.opsSummary);
  const installedRuntime = asObject(summary.installed_runtime || summary.installedRuntime);
  const diagnostics = asObject(installedRuntime.diagnostics);
  const bridge = diagnostics.runtime_api_registry_bridge || diagnostics.runtimeApiRegistryBridge;
  return asObject(bridge);
}

function normalizeEvidenceStatus(evidence = getBatchEvidence()) {
  const status = String(evidence?.status || (evidence ? 'unavailable' : 'not_configured')).toLowerCase();
  return ['not_configured', 'missing', 'unavailable', 'available', 'degraded'].includes(status) ? status : 'unavailable';
}

function evidenceTone(status) {
  if (status === 'available') return 'ok';
  if (status === 'not_configured') return 'neutral';
  return 'warn';
}

function evidenceStatusLabel(status) {
  return {
    not_configured: 'not configured',
    missing: 'missing',
    degraded: 'degraded',
    unavailable: 'unavailable',
    available: 'available'
  }[status] || 'unavailable';
}

function currentEvidenceTone(status) {
  if (['available', 'artifact-discovered', 'tracked-in-documentation', 'pass'].includes(status)) return 'ok';
  if (status === 'not_configured') return 'neutral';
  return 'warn';
}

function currentEvidenceStatusLabel(status) {
  return {
    not_configured: 'not configured',
    missing: 'missing',
    degraded: 'degraded',
    unavailable: 'unavailable',
    available: 'available',
    'artifact-discovered': 'artifact discovered',
    'tracked-in-documentation': 'tracked in documentation',
    pass: 'pass'
  }[status] || 'tracked';
}

function renderEvidenceStatusBadge(evidence = getBatchEvidence()) {
  const status = normalizeEvidenceStatus(evidence);
  const latest = asObject(evidence?.latest);
  const label = latest.batch_id || evidenceStatusLabel(status);
  return `<span class="status-badge evidence-${status} ${evidenceTone(status)}" title="${escapeHtml(evidenceStatusLabel(status))}">${escapeHtml(label)}</span>`;
}

function renderCurrentEvidenceStatusBadge() {
  const rollup = getCurrentEvidenceRollup();
  const fullAdmin = getCurrentEvidenceFullAdmin(rollup);
  const status = String(fullAdmin.status || (rollup ? 'tracked' : 'not_configured')).toLowerCase();
  const normalized = status === 'tracked' ? 'tracked-in-documentation' : status;
  const label = fullAdmin.batch_id || fullAdmin.version || currentEvidenceStatusLabel(normalized);
  return `<span class="status-badge current-evidence ${currentEvidenceTone(normalized)}" title="${escapeHtml(currentEvidenceStatusLabel(normalized))}">${escapeHtml(label)}</span>`;
}

function evidenceValue(value, fallback = 'Unavailable') {
  return value === undefined || value === null || value === '' ? fallback : String(value);
}

function evidenceBooleanLabel(value) {
  if (value === true) return 'Passed';
  if (value === false) return 'Failed';
  return 'Unavailable';
}

function evidenceDashboardDetail(evidence = getBatchEvidence()) {
  const status = normalizeEvidenceStatus(evidence);
  if (!evidence || evidence.configured === false || status === 'not_configured') {
    return 'Evidence root not configured for this listener.';
  }
  const latest = asObject(evidence.latest);
  const gpu = asObject(latest.gpu_snapshots);
  const route = asObject(latest.route_msi_hyperv);
  const os = asObject(latest.os_mutation);
  return [
    `run=${evidenceValue(latest.status || status, status)}`,
    `gpu=${evidenceValue(gpu.count, '0')}`,
    `route=${evidenceBooleanLabel(route.ok)}`,
    `os=${evidenceBooleanLabel(os.ok)}`
  ].join(' / ');
}

function currentEvidenceDashboardDetail() {
  const rollup = getCurrentEvidenceRollup();
  if (!rollup) {
    return 'Current evidence rollup is not reported by this listener.';
  }

  const publicBoundary = getCurrentEvidencePublicBoundary(rollup);
  const fullAdmin = getCurrentEvidenceFullAdmin(rollup);
  const packagePair = getCurrentEvidencePackagePair(rollup);
  const nextPackagePair = getCurrentEvidenceNextPackagePair(rollup);
  const hostOps = getCurrentEvidenceHostOps(rollup);
  return [
    `public=${evidenceValue(publicBoundary.run_id || publicBoundary.status, 'not configured')}`,
    `full=${evidenceValue(fullAdmin.batch_id || fullAdmin.status, 'not configured')}`,
    `manual=${evidenceValue(packagePair.package_pair || packagePair.status, 'not configured')}`,
    `next=${evidenceValue(nextPackagePair.package_pair || nextPackagePair.status, 'not configured')}`,
    `hostops=${evidenceValue(hostOps.contract_key || hostOps.status, 'not configured')}`
  ].join(' / ');
}

function evidenceIssueTone(issue) {
  const tone = String(issue?.tone || '').toLowerCase();
  return tone === 'error' ? 'error' : 'warn';
}

function collectEvidenceIssues() {
  const evidence = getBatchEvidence();
  if (!evidence) return [];
  const issues = [];
  const status = normalizeEvidenceStatus(evidence);
  if (status !== 'available' && status !== 'not_configured') {
    issues.push({
      code: `batch-evidence-${status}`,
      message: `Batch evidence ${status}`,
      detail: status,
      tone: status === 'unavailable' ? 'error' : 'warn'
    });
  }
  for (const error of asArray(evidence.errors)) {
    const issue = normalizeSummaryIssue(error, 'PCV_BATCH_EVIDENCE');
    issues.push({
      code: issue.code || 'PCV_BATCH_EVIDENCE',
      message: issue.message || 'Batch evidence issue',
      detail: issue.detail || status,
      tone: issue.code === 'PCV_BATCH_EVIDENCE_PARSE_FAILED' ? 'error' : 'warn'
    });
  }
  return issues;
}

