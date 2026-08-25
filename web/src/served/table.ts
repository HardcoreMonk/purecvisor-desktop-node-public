// @ts-nocheck
function normalizeSearchText(value) {
  return String(value ?? '').trim().toLowerCase();
}

function collectRowText(row, keys = []) {
  if (typeof row === 'string' || typeof row === 'number' || typeof row === 'boolean') {
    return String(row);
  }
  if (!row || typeof row !== 'object') return '';
  const record = row;
  if (keys.length > 0) {
    return keys.map((key) => formatObjectValue(readNested(record, key.split('.')))).join(' ');
  }
  return Object.values(record).map(formatObjectValue).join(' ');
}

function filterRowsByQuery(rows, query, projector) {
  const normalized = normalizeSearchText(query);
  if (!normalized) return rows;
  return rows.filter((row) => normalizeSearchText(projector ? projector(row) : collectRowText(row)).includes(normalized));
}

function sortRowsByKey(rows, sort, selectors = {}) {
  const [rawKey, rawDirection] = String(sort || '').split(':');
  const key = rawKey || 'name';
  const direction = rawDirection === 'desc' ? -1 : 1;
  const selector = selectors[key] || ((row) => row?.[key]);
  return [...rows].sort((left, right) => {
    const leftValue = selector(left);
    const rightValue = selector(right);
    const leftNumber = Number(leftValue);
    const rightNumber = Number(rightValue);
    if (Number.isFinite(leftNumber) && Number.isFinite(rightNumber)) {
      return (leftNumber - rightNumber) * direction;
    }
    return String(leftValue ?? '').localeCompare(String(rightValue ?? ''), undefined, { numeric: true, sensitivity: 'base' }) * direction;
  });
}

function renderTableStateSummary(label, shown, total, query = '', extra = '') {
  const filterText = query ? ` / filter=${query}` : '';
  return `<div class="table-state-summary"><strong>${escapeHtml(label)}</strong> ${escapeHtml(shown)} shown of ${escapeHtml(total)}${escapeHtml(filterText)}${extra ? ` / ${escapeHtml(extra)}` : ''}</div>`;
}

function getVmActionKey(vmId) {
  return String(vmId || '');
}

function isVmActionPending(vmId, action = '') {
  const pending = state.pendingVmActions[getVmActionKey(vmId)];
  return Boolean(pending && (!action || pending === action));
}

function setVmActionPending(vmId, action) {
  state.pendingVmActions[getVmActionKey(vmId)] = action;
}

function clearVmActionPending(vmId) {
  delete state.pendingVmActions[getVmActionKey(vmId)];
}

function getCheckpointActionKey(vmId, checkpointId = 'create') {
  return `${vmId || ''}:${checkpointId || 'create'}`;
}

function isCheckpointActionPending(vmId, checkpointId = 'create') {
  return Boolean(state.pendingCheckpoints[getCheckpointActionKey(vmId, checkpointId)]);
}

function setCheckpointActionPending(vmId, checkpointId, action) {
  state.pendingCheckpoints[getCheckpointActionKey(vmId, checkpointId)] = action;
}

function clearCheckpointActionPending(vmId, checkpointId = 'create') {
  delete state.pendingCheckpoints[getCheckpointActionKey(vmId, checkpointId)];
}

