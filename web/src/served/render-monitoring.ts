// @ts-nocheck
function readNested(value, path) {
  return path.reduce((current, key) => current && typeof current === 'object' ? current[key] : undefined, value);
}

function formatPolicyValue(value) {
  if (value === true) return 'enabled';
  if (value === false) return 'disabled';
  if (value === null || value === undefined || value === '') return '-';
  return value;
}

function countJobsByStatus(statuses) {
  const wanted = new Set(statuses.map((status) => String(status).toLowerCase()));
  return buildActivityRows().filter(({ job }) => wanted.has(String(job?.status || '').toLowerCase())).length;
}

function getVmCheckpointCount(vm) {
  const raw = vm?.checkpoints?.count ?? vm?.checkpoints_count ?? 0;
  const parsed = Number(raw);
  return Number.isFinite(parsed) ? parsed : 0;
}

function countVmCheckpointWarnings() {
  return asArray(state.vms).filter((vm) => getVmCheckpointCount(vm) >= 10).length;
}

function countSelectedOldCheckpointWarnings() {
  const cutoff = Date.now() - (14 * 24 * 60 * 60 * 1000);
  return asArray(state.selectedVmCheckpoints).filter((checkpoint) => {
    const stamp = Date.parse(checkpoint?.created_at || checkpoint?.creation_time || checkpoint?.created || '');
    return Number.isFinite(stamp) && stamp < cutoff;
  }).length;
}

function buildMonitoringSignals() {
  const host = state.host || {};
  const policy = state.runtimePolicy || {};
  const summaryPolicy = state.opsSummary?.runtime_policy || state.opsSummary?.runtimePolicy || {};
  const vmmsRunning = readNested(host, ['hyperv', 'vmms_running']);
  const tokenStorage = readNested(policy, ['auth', 'token_storage']) || readNested(policy, ['token', 'storage']) || 'unknown';
  const exposure = readNested(policy, ['network', 'current_exposure']) || readNested(policy, ['network', 'bind']) || 'loopback';
  const hardening = readNested(policy, ['service', 'hardening']) || readNested(policy, ['hardening']) || readNested(summaryPolicy, ['service', 'hardening']) || readNested(summaryPolicy, ['hardening']) || {};
  const routeTimeout = readNested(hardening, ['route_timeout_seconds']) || readNested(policy, ['route_timeout_seconds']) || readNested(summaryPolicy, ['route_timeout_seconds']) || '-';
  const requestLimit = readNested(hardening, ['request_limit_per_minute']) || readNested(hardening, ['request_limit']) || readNested(policy, ['request_limit_per_minute']) || '-';
  const burstLimit = readNested(hardening, ['request_burst_limit']) || readNested(hardening, ['burst_limit']) || readNested(policy, ['request_burst_limit']) || '-';
  const retryAfter = readNested(hardening, ['retry_after_seconds']) || readNested(policy, ['retry_after_seconds']) || '-';
  const activeJobs = countJobsByStatus(['queued', 'running']);
  const failedJobs = countJobsByStatus(['failed']);
  const checkpointWarnings = countVmCheckpointWarnings();
  const oldCheckpointWarnings = countSelectedOldCheckpointWarnings();

  return [
    { key: 'service-api', label: 'Service/API', value: state.connectionState === 'connected' ? 'Connected' : 'Not connected', tone: state.connectionState === 'connected' ? 'ok' : 'warn' },
    { key: 'vmms', label: 'VMMS', value: formatPolicyValue(vmmsRunning), tone: vmmsRunning === true ? 'ok' : 'warn' },
    { key: 'active-jobs', label: 'Active jobs', value: activeJobs, tone: activeJobs > 0 ? 'warn' : 'ok' },
    { key: 'failed-jobs', label: 'Failed jobs', value: failedJobs, tone: failedJobs > 0 ? 'error' : 'ok' },
    { key: 'checkpoint-warning', label: 'Checkpoint warnings', value: checkpointWarnings + oldCheckpointWarnings, tone: checkpointWarnings + oldCheckpointWarnings > 0 ? 'warn' : 'ok' },
    { key: 'token-policy', label: 'Token policy', value: tokenStorage, tone: tokenStorage === 'none' ? 'warn' : 'ok' },
    { key: 'lan-exposure', label: 'LAN exposure', value: exposure, tone: String(exposure).toLowerCase().includes('lan') ? 'warn' : 'ok' },
    { key: 'route-timeout', label: 'Route timeout', value: routeTimeout === '-' ? '-' : `${routeTimeout}s`, tone: routeTimeout === '-' ? 'warn' : 'ok' },
    { key: 'request-limit', label: 'Request limit', value: requestLimit === '-' ? '-' : `${requestLimit}/min`, tone: requestLimit === '-' ? 'warn' : 'ok' },
    { key: 'burst-limit', label: 'Burst limit', value: burstLimit, tone: burstLimit === '-' ? 'warn' : 'ok' },
    { key: 'retry-after', label: 'Retry-After', value: retryAfter === '-' ? '-' : `${retryAfter}s`, tone: retryAfter === '-' ? 'warn' : 'ok' }
  ];
}

function evidenceStatusMessage(status) {
  return {
    not_configured: 'Batch evidence root is not configured.',
    missing: 'Configured evidence root has no readable batch summary.',
    degraded: 'Latest batch supervisor evidence is partial or malformed.',
    unavailable: 'Batch evidence summary could not be read.',
    available: 'Latest batch supervisor evidence is loaded.'
  }[status] || 'Batch evidence summary could not be read.';
}

function renderEvidenceDashboard() {
  if (!els.evidencePanel) return;
  const evidence = getBatchEvidence();
  const currentEvidence = getCurrentEvidenceRollup();
  const status = normalizeEvidenceStatus(evidence);
  const errors = asArray(evidence?.errors);

  if (!evidence || evidence.configured === false || status === 'not_configured') {
    els.evidencePanel.innerHTML = `
      <div class="evidence-empty">
        ${renderEvidenceStatusBadge(evidence)}
        ${renderCurrentEvidenceSummary(currentEvidence)}
        <p class="muted">${escapeHtml(evidenceStatusMessage('not_configured'))}</p>
      </div>`;
    return;
  }

  const latest = asObject(evidence.latest);
  const release = asObject(latest.release);
  const gpu = asObject(latest.gpu_snapshots);
  const route = asObject(latest.route_msi_hyperv);
  const os = asObject(latest.os_mutation);
  const host = asObject(latest.host_final_state);
  const steps = asArray(latest.steps);

  const stepRows = steps.length
    ? steps.map((step) => {
        const stepStatus = step?.ok === true ? 'succeeded' : step?.ok === false ? 'failed' : 'unknown';
        return `<tr>
        <td>${escapeHtml(evidenceValue(step?.step_id, 'step'))}</td>
        <td>${stateBadge(stepStatus)}</td>
        <td>${escapeHtml(evidenceValue(step?.attempt_count, '0'))}</td>
        <td>${escapeHtml(evidenceValue(step?.retry_count, '0'))}</td>
        <td>${escapeHtml(step?.timed_out === true ? 'true' : 'false')}</td>
      </tr>`;
      }).join('')
    : '<tr><td colspan="5" class="muted">No step evidence is available.</td></tr>';

  const errorHtml = errors.length
    ? `<div class="activity-warning">${errors.map((error) => {
        const issue = normalizeSummaryIssue(error, 'PCV_BATCH_EVIDENCE');
        const detail = issue.detail && issue.detail !== issue.message
          ? ` <span class="muted">${escapeHtml(issue.detail)}</span>`
          : '';
        return `<strong>${escapeHtml(issue.code)}</strong> ${escapeHtml(issue.message)}${detail}`;
      }).join('<br>')}</div>`
    : '';

  els.evidencePanel.innerHTML = `
    ${errorHtml}
    <div class="evidence-header">
      <div>
        <span class="muted">Batch</span>
        <strong>${escapeHtml(evidenceValue(latest.batch_id, evidenceStatusLabel(status)))}</strong>
        <p class="muted">${escapeHtml(evidenceStatusMessage(status))}</p>
      </div>
      ${renderEvidenceStatusBadge(evidence)}
    </div>
    <div class="evidence-grid">
      <div class="evidence-metric"><span class="muted">Version</span><strong>${escapeHtml(evidenceValue(release.version))}</strong></div>
      <div class="evidence-metric"><span class="muted">Signing</span><strong>${escapeHtml(evidenceValue(release.signing_mode))}</strong></div>
      <div class="evidence-metric"><span class="muted">GPU snapshots</span><strong>${escapeHtml(evidenceValue(gpu.count, '0'))}</strong></div>
      <div class="evidence-metric"><span class="muted">GPU peak MiB</span><strong>${escapeHtml(evidenceValue(gpu.peak_adapter_mib))}</strong></div>
      <div class="evidence-metric"><span class="muted">Service</span><strong>${escapeHtml(evidenceValue(host.service_state))}</strong></div>
      <div class="evidence-metric"><span class="muted">Route/MSI</span><strong>${escapeHtml(evidenceBooleanLabel(route.ok))}</strong></div>
      <div class="evidence-metric"><span class="muted">OS gate</span><strong>${escapeHtml(evidenceBooleanLabel(os.ok))}</strong></div>
      <div class="evidence-metric"><span class="muted">Firewall final</span><strong>${escapeHtml(evidenceValue(host.firewall_rule_count ?? os.firewall_rule_count, '0'))}</strong></div>
    </div>
    <div class="evidence-boundary">
      <span>Public signing: ${escapeHtml(evidenceValue(release.public_trusted_signing, 'excluded'))}</span>
      <span>External publication: ${escapeHtml(evidenceValue(release.external_stable_publication, 'not-claimed'))}</span>
    </div>
    ${renderCurrentEvidenceSummary(currentEvidence)}
    <div class="evidence-table-wrap">
      <table class="evidence-table">
        <thead><tr><th>Step</th><th>Status</th><th>Attempts</th><th>Retries</th><th>Timed out</th></tr></thead>
        <tbody>${stepRows}</tbody>
      </table>
    </div>`;
}

function renderCurrentEvidenceSummary(rollup = getCurrentEvidenceRollup()) {
  if (!rollup) {
    return '';
  }

  const publicBoundary = getCurrentEvidencePublicBoundary(rollup);
  const fullAdmin = getCurrentEvidenceFullAdmin(rollup);
  const packagePair = getCurrentEvidencePackagePair(rollup);
  const nextPackagePair = getCurrentEvidenceNextPackagePair(rollup);
  const hostOps = getCurrentEvidenceHostOps(rollup);
  return `
    <div class="evidence-header current-evidence-rollup">
      <div>
        <span class="muted">Current evidence</span>
        <strong>${escapeHtml(rollup.contract_key || 'runtime-api-current-evidence-rollup-v1')}</strong>
        <p class="muted">${escapeHtml(currentEvidenceDashboardDetail())}</p>
      </div>
      ${renderCurrentEvidenceStatusBadge()}
    </div>
    <div class="evidence-grid current-evidence-grid">
      <div class="evidence-metric"><span class="muted">Public boundary</span><strong>${escapeHtml(evidenceValue(publicBoundary.run_id || publicBoundary.status))}</strong></div>
      <div class="evidence-metric"><span class="muted">Public boundary head</span><strong>${escapeHtml(evidenceValue(publicBoundary.head_sha))}</strong></div>
      <div class="evidence-metric"><span class="muted">Full admin</span><strong>${escapeHtml(evidenceValue(fullAdmin.version || fullAdmin.batch_id))}</strong></div>
      <div class="evidence-metric"><span class="muted">Manual admin</span><strong>${escapeHtml(evidenceValue(packagePair.package_pair || packagePair.status))}</strong></div>
      <div class="evidence-metric"><span class="muted">Manual admin next</span><strong>${escapeHtml(evidenceValue(nextPackagePair.package_pair || nextPackagePair.status))}</strong></div>
      <div class="evidence-metric"><span class="muted">Next decision</span><strong>${escapeHtml(evidenceValue(nextPackagePair.decision))}</strong></div>
      <div class="evidence-metric"><span class="muted">Descriptor</span><strong>${escapeHtml(evidenceValue(packagePair.current_card_descriptor_batch_id || packagePair.descriptor_batch_id))}</strong></div>
      <div class="evidence-metric"><span class="muted">Host Ops</span><strong>${escapeHtml(evidenceValue(hostOps.contract_key || hostOps.status))}</strong></div>
    </div>`;
}

function renderMonitoring() {
  const signals = buildMonitoringSignals();
  els.monitoringPanel.innerHTML = `
    <div class="monitoring-grid">
      ${signals.map((signal) => `<div class="monitoring-card signal-${escapeHtml(signal.tone)}" data-signal="${escapeHtml(signal.key)}"><span class="muted">${escapeHtml(signal.label)}</span><strong>${escapeHtml(signal.value)}</strong></div>`).join('')}
    </div>`;
}

function renderTroubleshootingEvidence() {
  const evidence = getBatchEvidence();
  const issues = collectEvidenceIssues();
  const latest = asObject(evidence?.latest);
  const release = asObject(latest.release);
  const issueHtml = issues.length
    ? `<div class="triage-list">${issues.map((issue) => `<div class="triage-row">
        <div>
          <strong>${escapeHtml(issue.code)}</strong>
          <span class="muted">${escapeHtml(issue.message)}</span>
        </div>
        <span class="status-badge ${escapeHtml(evidenceIssueTone(issue))}">${escapeHtml(issue.detail || normalizeEvidenceStatus(evidence))}</span>
      </div>`).join('')}</div>`
    : '<p class="muted">No batch evidence degradation is visible.</p>';

  return `<div class="troubleshooting-card batch-evidence-troubleshooting">
    <span class="muted">Batch evidence</span>
    <strong>${escapeHtml(latest.batch_id || normalizeEvidenceStatus(evidence))}</strong>
    ${renderEvidenceStatusBadge(evidence)}
    ${issueHtml}
    <div class="boundary-chip-row">
      <span>Public signing: ${escapeHtml(evidenceValue(release.public_trusted_signing, 'excluded'))}</span>
      <span>External publication: ${escapeHtml(evidenceValue(release.external_stable_publication, 'not-claimed'))}</span>
    </div>
  </div>`;
}

