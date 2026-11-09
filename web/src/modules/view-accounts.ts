// @ts-nocheck
// Desktop Node module (ADR-0018): accounts directory on the U4a view registry.
// Creation and password dialogs stay on the ADR-0019 modals in accounts.ts.
window.PCV = window.PCV || {};
(function (PCV) {
  function node() {
    return PCV.uxlib.el;
  }

  function resolveMount(passed) {
    if (passed && passed.id === 'accounts-panel') return passed;
    if (passed && typeof passed.querySelector === 'function') {
      var nested = passed.querySelector('#accounts-panel');
      if (nested) return nested;
    }
    return document.getElementById('accounts-panel');
  }

  function roleCounts(accounts) {
    var counts = {};
    accounts.forEach(function (account) {
      var role = String(account.role || '-');
      counts[role] = (counts[role] || 0) + 1;
    });
    return counts;
  }

  function paintDirectory(root) {
    var el = node();
    PCV.uxlib.clearEl(root);
    if (typeof bindAccountsPanel === 'function') bindAccountsPanel();
    if (typeof canManageAccounts === 'function' && !canManageAccounts()) {
      root.appendChild(el('div', { class: 'pcv-account-denied', role: 'status' },
        el('h2', null, t('account.denied.title')),
        el('p', null, t('account.denied.body'))));
      return;
    }
    var error = state.accountDirectoryError;
    if (error) {
      var serverText = [error.message, error.detail].filter(Boolean).join(' ');
      var lastAdmin = error.code === 'PCV_ACCOUNT_LAST_ADMIN' ? t('account.error.last_admin') : '';
      root.appendChild(el('div', { class: 'pcv-account-error', role: 'alert' },
        el('strong', null, String(error.code || '')),
        el('p', null, [lastAdmin, serverText].filter(Boolean).join(' '))));
    }
    var accounts = (typeof asArray === 'function' ? asArray(state.accountDirectory) : []).filter(function (account) {
      return account && account.username;
    });
    var counts = roleCounts(accounts);
    var chips = el('div', { class: 'boundary-chip-row', 'data-account-roles': '1' },
      el('span', { 'data-account-role-chip': 'all' }, t('account.role.all') + ' ' + String(accounts.length)));
    Object.keys(counts).forEach(function (role) {
      chips.appendChild(el('span', { 'data-account-role-chip': role, class: 'status-badge' }, role + ' ' + String(counts[role])));
    });
    root.appendChild(chips);
    var create = el('button', { type: 'button', class: 'btn', 'data-action': 'account-create-open' }, t('account.create.open'));
    create.disabled = !!state.accountManagePending;
    root.appendChild(create);
    var host = el('div', { id: 'accounts-directory' });
    root.appendChild(host);
    var rows = accounts.map(function (account) {
      var username = String(account.username || '');
      var role = String(account.role || '-');
      var enabled = account.enabled !== false;
      var disable = el('button', {
        type: 'button',
        class: 'btn',
        'data-action': 'account-disable',
        'data-username': username
      }, t('account.disable.action'));
      disable.disabled = !!state.accountManagePending || !enabled;
      var reset = el('button', {
        type: 'button',
        class: 'btn',
        'data-action': 'account-reset-password',
        'data-username': username
      }, t('account.reset.open'));
      reset.disabled = !!state.accountManagePending;
      return [
        username,
        el('span', { class: 'status-badge', 'data-account-role-pill': role }, role),
        typeof accountEnabledText === 'function' ? accountEnabledText(account) : (enabled ? t('account.list.enabled') : t('account.list.disabled')),
        el('span', null, disable, reset)
      ];
    });
    createDataTable(host, {
      tableId: 'dt-accounts',
      headers: [
        { label: t('account.list.username'), key: 'username' },
        { label: t('account.list.role'), key: 'role' },
        { label: t('account.list.state'), key: 'state' },
        { label: '', key: 'actions', sortable: false }
      ],
      rows: rows,
      emptyText: t('account.list.empty')
    });
  }

  function renderAccountsView(mount, ctx) {
    if (ctx && typeof ctx.isCurrent === 'function' && !ctx.isCurrent()) return;
    if (!PCV.uxlib || typeof PCV.uxlib.el !== 'function' || typeof PCV.uxlib.clearEl !== 'function') return;
    if (typeof createDataTable !== 'function') return;
    var root = resolveMount(mount);
    if (!root) return;
    paintDirectory(root);
  }

  PCV.viewAccounts = { render: renderAccountsView };
  PCV.views.register('accounts', { render: renderAccountsView });
})(window.PCV);
