// @ts-nocheck
// Desktop Node module (ADR-0018): read-only network inventory on the U4a view registry.
// Switch create, NAT, DHCP, and VM connect stay out of this screen.
window.PCV = window.PCV || {};
(function (PCV) {
  var FACET = 'networkType';

  function inventoryOf() {
    if (Array.isArray(state.networkInventory)) {
      return { source: 'network.inventory', mutating: false, switches: state.networkInventory };
    }
    return asObject(state.networkInventory);
  }

  function switchesOf() {
    var inventory = inventoryOf();
    return asArray(inventory.switches || inventory.items || inventory.networks);
  }

  function switchName(item) {
    return String((item && item.name) || '');
  }

  function switchType(item) {
    return String((item && (item.type || item.switch_type)) || '');
  }

  function adapterText(item) {
    return String((item && (item.net_adapter_interface_description || item.adapter || item.adapter_name)) || '-');
  }

  function isDefaultSwitch(item) {
    return !!(item && (item.is_default === true || switchName(item).toLowerCase() === 'default switch'));
  }

  function adapterSwitchName(entry) {
    if (entry == null) return '';
    if (typeof entry === 'string') return entry;
    return String(entry.switch || entry.switch_name || '');
  }

  function vmAdapters(vm) {
    var out = [];
    [vm && vm.network, vm && vm.network_adapters, vm && vm.adapters, vm && vm.nics].forEach(function (list) {
      asArray(list).forEach(function (entry) { out.push(entry); });
    });
    return out;
  }

  function connectedVmCount(name) {
    var target = String(name || '').toLowerCase();
    if (!target) return 0;
    var count = 0;
    asArray(state.vms).forEach(function (vm) {
      var hit = vmAdapters(vm).some(function (entry) {
        return adapterSwitchName(entry).toLowerCase() === target;
      });
      if (hit) count += 1;
    });
    return count;
  }

  function selectedType() {
    var fs = PCV.ui && PCV.ui.filterState;
    if (!fs) return '';
    var values = (fs.current() || {})[FACET] || [];
    return values.length === 1 ? String(values[0]) : '';
  }

  function fact(label, value) {
    var el = PCV.uxlib.el;
    return el('div', { class: 'diagnostics-fact' },
      el('span', { class: 'muted' }, label),
      el('strong', null, String(value)));
  }

  function renderNetworkChangeReadback(switches) {
    var el = PCV.uxlib.el;
    var rows = asArray(switches);
    var productCount = rows.filter(function (item) {
      return switchName(item).toLowerCase().indexOf('pcv-') === 0;
    }).length;
    var reservedCount = rows.filter(function (item) {
      var name = switchName(item).toLowerCase();
      return name === 'default switch' || name.indexOf('wsl') === 0;
    }).length;
    var externalCount = rows.filter(function (item) {
      return switchType(item).toLowerCase() === 'external';
    }).length;
    return el('div', { class: 'network-change-readback' },
      el('div', { class: 'diagnostics-grid' },
        fact(t('vm.net.switch_create'), t('vm.net.service_only')),
        fact(t('vm.net.product'), String(productCount)),
        fact(t('vm.net.reserved'), String(reservedCount)),
        fact(t('vm.net.external'), externalCount === 0 ? t('vm.net.none') : t('vm.net.present')),
        fact(t('vm.net.connect_label'), t('vm.net.connect_value')),
        fact(t('vm.net.nat'), t('vm.net.excluded')),
        fact(t('vm.net.mutation'), t('vm.net.readonly'))),
      el('div', { class: 'boundary-chip-row' },
        el('span', null, t('vm.net.chip_create')),
        el('span', null, t('vm.net.chip_nat')),
        el('span', null, t('vm.net.chip_dhcp')),
        el('span', null, t('vm.net.chip_connect'))));
  }

  function errorDetail(error) {
    var code = (error && error.code) || 'PCV_NETWORK_INVENTORY_UNAVAILABLE';
    var parity = code === 'PCV_NATIVE_PARITY_INCOMPLETE' ||
      String((error && (error.detail || error.message)) || '').toLowerCase().indexOf('parity') !== -1;
    var detail = parity ? t('vm.net.error_parity') : ((error && error.detail) || t('vm.net.error_fallback'));
    var message = (error && error.message) || t('vm.net.error_unavailable');
    return code + ' ' + message + ' ' + detail;
  }

  function renderNetworkView(mount, ctx) {
    if (!mount) return;
    if (ctx && typeof ctx.isCurrent === 'function' && !ctx.isCurrent()) return;
    var el = PCV.uxlib.el;
    var clearEl = PCV.uxlib.clearEl;
    var loaded = state.networkInventory != null;
    var failed = !!state.networkError;
    clearEl(mount);
    if (!loaded && !failed) {
      var skel = el('div', { class: 'network-loading', role: 'status' });
      mount.appendChild(skel);
      if (typeof showSkeleton === 'function') showSkeleton(skel, 3);
      else skel.appendChild(document.createTextNode(t('vm.readback.loading')));
      return;
    }
    var all = failed ? [] : switchesOf();
    var counts = {};
    all.forEach(function (item) {
      var type = switchType(item) || '-';
      counts[type] = (counts[type] || 0) + 1;
    });
    var tools = el('div', { class: 'network-toolbar' });
    tools.appendChild(HN.filterBar([{
      key: FACET,
      label: t('vm.net.col.type'),
      singleSelect: true,
      showAll: true,
      allLabel: t('copy.network.all'),
      allCount: all.length,
      options: Object.keys(counts).sort().map(function (type) {
        return { value: type, label: type, count: counts[type] };
      })
    }]));
    mount.appendChild(tools);
    if (failed) {
      mount.appendChild(PCV.components.loadError({
        titleKey: 'ui.load_error.title',
        detail: errorDetail(state.networkError),
        onRetry: function () {
          if (typeof loadNetworkInventory !== 'function') return;
          Promise.resolve(loadNetworkInventory()).then(function () {
            if (typeof render === 'function') render();
          });
        }
      }));
    } else {
      var chosen = selectedType();
      var queried = filterRowsByQuery(all, state.networkFilter, function (item) {
        return [
          switchName(item),
          switchType(item),
          adapterText(item),
          isDefaultSwitch(item) ? 'default' : ''
        ].join(' ');
      });
      var visible = chosen
        ? queried.filter(function (item) { return switchType(item) === chosen; })
        : queried;
      var tableHost = el('div', { class: 'network-view-table' });
      mount.appendChild(tableHost);
      if (!visible.length) {
        var empty = PCV.components.emptyState({
          titleKey: 'vm.net.empty_title',
          bodyKey: 'vm.net.empty'
        });
        empty.classList.add('network-empty-state');
        tableHost.appendChild(empty);
      } else {
        createDataTable(tableHost, { tableId: 'dt-network',
          headers: [
            { label: t('vm.net.col.name'), key: 'name', sortable: false },
            { label: t('vm.net.col.type'), key: 'type', sortable: false },
            { label: t('vm.net.col.connected'), key: 'connected', sortable: false },
            { label: t('vm.net.col.adapter'), key: 'adapter', sortable: false },
            { label: t('copy.status'), key: 'status', sortable: false }
          ],
          rows: visible.map(function (item) {
            var name = switchName(item) || '-';
            var pill = isDefaultSwitch(item)
              ? HN.statusPill('ok', 'default')
              : HN.statusPill('idle', switchType(item) || '-');
            return [name, switchType(item) || '-', String(connectedVmCount(name)), adapterText(item), pill];
          })
        });
      }
      mount.appendChild(renderNetworkChangeReadback(all));
    }
  }

  if (PCV.ui && PCV.ui.filterState && typeof PCV.ui.filterState.subscribe === 'function') {
    PCV.ui.filterState.subscribe(function () {
      var active = (PCV.nav && PCV.nav.activeView && PCV.nav.activeView()) ||
        (typeof state !== 'undefined' && state.activeView);
      if (active === 'network' && typeof render === 'function') render();
    });
  }

  PCV.viewNetwork = { render: renderNetworkView };
  PCV.views.register('network', { render: renderNetworkView });
})(window.PCV);
