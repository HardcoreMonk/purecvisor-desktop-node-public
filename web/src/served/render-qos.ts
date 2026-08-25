// @ts-nocheck
function renderCheckpointList(vmId) {
  const checkpoints = asArray(state.selectedVmCheckpoints);
  const canOperate = rbacAllows('operate');
  if (checkpoints.length === 0) {
    return '<p class="muted">No checkpoints returned for this VM.</p>';
  }

  return checkpoints.map((checkpoint) => {
    const checkpointId = getCheckpointId(checkpoint);
    const checkpointDisabled = isCheckpointActionPending(vmId, checkpointId) || !canOperate ? ' disabled' : '';
    return `<div class="checkpoint-row">
      <div>
        <strong>${escapeHtml(getCheckpointName(checkpoint))}</strong>
        <div class="muted">${escapeHtml(formatCheckpointMeta(checkpoint))}</div>
      </div>
      <div class="checkpoint-actions">
        <button data-action="checkpoint-restore" data-vm-id="${escapeHtml(vmId)}" data-checkpoint-id="${escapeHtml(checkpointId)}"${checkpointDisabled}>Restore</button>
        <button class="danger-button" data-action="checkpoint-delete" data-vm-id="${escapeHtml(vmId)}" data-checkpoint-id="${escapeHtml(checkpointId)}"${checkpointDisabled}>Delete</button>
      </div>
    </div>`;
  }).join('');
}

function getSelectedVmReadbacks(vmId) {
  const readbacks = state.selectedVmReadbacks;
  return readbacks && readbacks.vm_id === vmId ? readbacks : null;
}

function readbackBucket(payload, bucketName) {
  const record = asObject(payload);
  return asObject(record[bucketName] || readNested(record, ['data', bucketName]));
}

function readbackFieldSummary(payload, bucketName, fields) {
  const bucket = readbackBucket(payload, bucketName);
  const parts = fields
    .map((field) => {
      const value = bucket[field];
      return value === undefined || value === null || value === '' ? '' : `${field}=${formatObjectValue(value)}`;
    })
    .filter(Boolean);
  return parts.length > 0 ? parts.join(' / ') : 'payload available';
}

function readbackErrorFor(readbacks, key) {
  return asArray(readbacks?.errors).find((error) => error.key === key) || null;
}

function renderReadbackCard(readbacks, key, title, bucketName, fields) {
  const payload = readbacks?.values?.[key];
  const error = readbackErrorFor(readbacks, key);
  const status = error ? 'degraded' : payload ? 'available' : readbacks?.loading ? 'loading' : 'pending';
  const body = error
    ? `${error.code}: ${error.message}`
    : payload
      ? readbackFieldSummary(payload, bucketName, fields)
      : readbacks?.loading
        ? 'Loading'
        : 'Not loaded';
  const detail = error?.detail || '';
  return `<div class="qos-readback-card">
    <span class="muted">${escapeHtml(title)}</span>
    ${stateBadge(status)}
    <strong>${escapeHtml(body)}</strong>
    ${detail ? `<p class="muted">${escapeHtml(detail)}</p>` : ''}
  </div>`;
}

function renderVmQosGuestReadback(vmId) {
  const readbacks = getSelectedVmReadbacks(vmId);
  const updated = readbacks?.updated_at ? new Date(readbacks.updated_at).toLocaleString() : '-';
  const loadingLabel = readbacks?.loading ? 'Loading' : 'Refresh';
  return `<section class="qos-readback-panel">
    <div class="mini-section-header">
      <div>
        <p class="eyebrow">QoS / Guest Readback</p>
        <h3>Hyper-V readback surface</h3>
      </div>
      <button data-action="vm-qos-guest-refresh" data-vm-id="${escapeHtml(vmId)}"${readbacks?.loading ? ' disabled' : ''}>${escapeHtml(loadingLabel)}</button>
    </div>
    <div class="qos-readback-grid">
      ${renderReadbackCard(readbacks, 'blkio', 'blkio', 'storage_qos', ['linux_blkio_compatible', 'mutation_supported'])}
      ${renderReadbackCard(readbacks, 'bandwidth', 'bandwidth', 'network_qos', ['linux_bandwidth_compatible', 'mutation_supported'])}
      ${renderReadbackCard(readbacks, 'guest_agent', 'guest-agent-status', 'guest_agent', ['status', 'qemu_guest_agent', 'guest_exec_supported'])}
      ${renderReadbackCard(readbacks, 'guest_ping', 'guest-ping', 'guest_ping', ['reachable', 'guest_heartbeat_verified'])}
    </div>
    <p class="muted">updated_at=${escapeHtml(updated)} / vm.limit remains CLI/API queued mutation</p>
  </section>`;
}

function getSelectedVmQosControl(vmId) {
  const control = state.selectedVmQosControl;
  return control && control.vm_id === vmId ? control : null;
}

function renderQosControlResult(control) {
  if (!control) {
    return '<p class="muted">No QoS direct-control preview or apply has been run for this VM in this browser session.</p>';
  }

  const status = control.error ? 'degraded' : control.loading ? 'loading' : control.mode || 'tracked';
  const detail = control.error
    ? `${control.error.code}: ${control.error.message}`
    : control.result
      ? formatObjectValue(control.result)
      : 'pending';
  const updated = control.updated_at ? new Date(control.updated_at).toLocaleString() : '-';
  return `<div class="qos-control-result">
    <span class="muted">${escapeHtml(control.kind || 'qos')} ${escapeHtml(control.mode || 'control')}</span>
    ${stateBadge(status)}
    <strong>${escapeHtml(detail)}</strong>
    <p class="muted">updated_at=${escapeHtml(updated)}</p>
  </div>`;
}

function renderVmQosDirectControl(vmId) {
  const canOperate = rbacAllows('operate');
  const canGuestExec = rbacAllows('guest.exec');
  const canGuestChannel = rbacAllows('guest.channel.configure');
  const actionDisabled = isVmActionPending(vmId) || !canOperate ? ' disabled' : '';
  const guestExecDisabled = isVmActionPending(vmId) || !canGuestExec ? ' disabled' : '';
  const guestChannelDisabled = isVmActionPending(vmId) || !canGuestChannel ? ' disabled' : '';
  const control = getSelectedVmQosControl(vmId);
  return `<section class="qos-control-panel">
    <div class="mini-section-header">
      <div>
        <p class="eyebrow">QoS Direct Control</p>
        <h3>ADR-0008 preview and apply</h3>
      </div>
      <span class="status-badge ${canOperate ? 'ok' : 'warn'}">${canOperate ? 'operate' : 'operate required'}</span>
    </div>
    <div class="qos-control-grid">
      <form class="qos-control-form" data-qos-kind="storage" data-vm-id="${escapeHtml(vmId)}">
        <label>Disk<input name="disk" autocomplete="off" value="disk0"${actionDisabled}></label>
        <label>Maximum IOPS<input name="maximum_iops" type="number" min="0" step="1" value="120"${actionDisabled}></label>
        <label>Minimum IOPS<input name="minimum_iops" type="number" min="0" step="1" value="0"${actionDisabled}></label>
        <div class="qos-control-actions">
          <button type="submit" data-action="vm-qos-storage-preview"${actionDisabled}>Preview</button>
          <button type="submit" class="danger-button" data-action="vm-qos-storage-apply"${actionDisabled}>Apply</button>
        </div>
      </form>
      <form class="qos-control-form" data-qos-kind="network" data-vm-id="${escapeHtml(vmId)}">
        <label>Adapter<input name="adapter" autocomplete="off" value="adapter0"${actionDisabled}></label>
        <label>Maximum Kbps<input name="maximum_kbps" type="number" min="0" step="1" value="20480"${actionDisabled}></label>
        <label>Minimum Kbps<input name="minimum_kbps" type="number" min="0" step="1" value="0"${actionDisabled}></label>
        <div class="qos-control-actions">
          <button type="submit" data-action="vm-qos-network-preview"${actionDisabled}>Preview</button>
          <button type="submit" class="danger-button" data-action="vm-qos-network-apply"${actionDisabled}>Apply</button>
        </div>
      </form>
    </div>
    ${renderQosControlResult(control)}
    <div class="mini-section-header">
      <div>
        <p class="eyebrow">Guest Execution Direct Control</p>
        <h3>ADR-0009 queued execution and channel lifecycle</h3>
      </div>
      <span class="status-badge ${canGuestExec && canGuestChannel ? 'ok' : 'warn'}">guest.exec + guest.channel.configure required</span>
    </div>
    <div class="qos-control-grid">
      <form class="qos-control-form" data-guest-execution-kind="exec" data-vm-id="${escapeHtml(vmId)}">
        <label>Credential reference<input name="credential_ref" autocomplete="off" placeholder="wincred:target"${guestExecDisabled}></label>
        <label>Timeout seconds<input name="timeout_sec" type="number" min="1" max="600" step="1" value="60"${guestExecDisabled}></label>
        <label>Command<input name="command" autocomplete="off" placeholder="hostname"${guestExecDisabled}></label>
        <div class="qos-control-actions">
          <button type="submit" class="danger-button" data-action="vm-guest-exec"${guestExecDisabled}>Queue exec</button>
        </div>
      </form>
      <form class="qos-control-form" data-guest-execution-kind="channel" data-vm-id="${escapeHtml(vmId)}">
        <label>Credential reference<input name="credential_ref" autocomplete="off" placeholder="wincred:target"${guestChannelDisabled}></label>
        <label>Timeout seconds<input name="timeout_sec" type="number" min="1" max="600" step="1" value="30"${guestChannelDisabled}></label>
        <div class="qos-control-actions">
          <button type="submit" data-action="guest-agent-ensure-channel" data-guest-channel-mode="verify"${guestChannelDisabled}>Verify channel</button>
          <button type="submit" class="danger-button" data-action="guest-agent-ensure-channel" data-guest-channel-mode="repair"${guestChannelDisabled}>Repair channel</button>
        </div>
      </form>
    </div>
    <p class="muted">Guest command output is reduced to audit digests; raw stdout/stderr and credential values are not rendered.</p>
    <p class="muted">Account/noVNC target config mutation remains ADR-0010 deferred.</p>
  </section>`;
}

