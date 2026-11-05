// @ts-nocheck
// Desktop Node module (ADR-0019, ui-redesign-20261105 Task 2): P1 shared components.
// Confirm, toast, and empty state already live on PCV.ui. Those call sites stay until later tasks
// wire screens. This module adds the Task 2 contract on PCV.components and reuses PCV.modalCore
// (backdropDismiss) plus PCV.uxlib.el. Tabs, field validation, job progress, and load errors are new.
window.PCV = window.PCV || {};
(function (PCV) {
  var seq = 0;

  function el(tag, attrs) {
    var mk = PCV.uxlib && PCV.uxlib.el;
    var node = mk ? mk(tag, attrs) : document.createElement(tag);
    for (var i = 2; i < arguments.length; i++) append(node, arguments[i]);
    return node;
  }

  function append(node, child) {
    if (child == null || child === false) return;
    if (Array.isArray(child)) {
      child.forEach(function (item) { append(node, item); });
      return;
    }
    if (child instanceof Node) {
      node.appendChild(child);
      return;
    }
    node.appendChild(document.createTextNode(String(child)));
  }

  function text(key, params) {
    if (typeof t === 'function') return t(key, params);
    return key;
  }

  function isNetWhitespace(code) {
    if (code === 0x20 || code === 0x09 || code === 0x0A || code === 0x0B || code === 0x0C || code === 0x0D) return true;
    if (code === 0x85 || code === 0xA0 || code === 0x1680) return true;
    if (code >= 0x2000 && code <= 0x200A) return true;
    return code === 0x2028 || code === 0x2029 || code === 0x202F || code === 0x205F || code === 0x3000;
  }

  function netTrim(value) {
    var start = 0;
    var end = value.length;
    while (start < end && isNetWhitespace(value.charCodeAt(start))) start++;
    while (end > start && isNetWhitespace(value.charCodeAt(end - 1))) end--;
    return value.slice(start, end);
  }

  function isControlUnit(code) {
    return code <= 0x1F || (code >= 0x7F && code <= 0x9F);
  }

  function isLetterOrDigitUnit(code) {
    if (code >= 0xD800 && code <= 0xDFFF) return false;
    var ch = String.fromCharCode(code);
    return /^\p{L}$/u.test(ch) || /^\p{Nd}$/u.test(ch);
  }

  function isValidHyperVName(name) {
    if (name.length < 1 || name.length > 128 || name !== netTrim(name)) return false;
    for (var i = 0; i < name.length; i++) {
      var ch = name.charAt(i);
      if (isControlUnit(name.charCodeAt(i)) || ch === '/' || ch === '\\') return false;
    }
    return true;
  }

  function isValidVmCreateName(name) {
    if (name.length < 1 || name.length > 63 || !isLetterOrDigitUnit(name.charCodeAt(0))) return false;
    for (var i = 0; i < name.length; i++) {
      var ch = name.charAt(i);
      if (!(isLetterOrDigitUnit(name.charCodeAt(i)) || ch === '.' || ch === '_' || ch === '-')) return false;
    }
    return true;
  }

  function invalid(code, messageKey) {
    return { code: code, messageKey: messageKey };
  }

  function asName(value) {
    return value == null ? '' : String(value);
  }

  var validators = {
    required: function (value) {
      if (asName(value).trim() === '') return invalid('PCV_VM_NAME_REQUIRED', 'vm.name.required');
      return null;
    },
    hyperVName: function (value) {
      var name = asName(value);
      if (!isValidHyperVName(name) || name === '.' || name === '..') return invalid('PCV_VM_NAME_INVALID', 'vm.name.hyperv_invalid');
      return null;
    },
    vmCreateName: function (value) {
      if (!isValidVmCreateName(asName(value))) return invalid('PCV_VM_NAME_INVALID', 'vm.name.create_invalid');
      return null;
    },
    notSameAs: function (source) {
      var expected = asName(source);
      return function (value) {
        if (asName(value) === expected) return invalid('PCV_VM_CLONE_NAME_CONFLICT', 'vm.name.same_as_source');
        return null;
      };
    },
    notIn: function (names) {
      var list = Array.isArray(names) ? names : [];
      return function (value) {
        var needle = asName(value).toUpperCase();
        for (var i = 0; i < list.length; i++) {
          if (String(list[i]).toUpperCase() === needle) return invalid('PCV_VM_ALREADY_EXISTS', 'vm.name.duplicate');
        }
        return null;
      };
    }
  };

  function fieldElement(form, name) {
    if (form.elements && typeof form.elements.namedItem === 'function') {
      var named = form.elements.namedItem(name);
      if (named && !named.length) return named;
    }
    var nodes = form.querySelectorAll('[name]');
    for (var i = 0; i < nodes.length; i++) {
      if (nodes[i].getAttribute('name') === name) return nodes[i];
    }
    return null;
  }

  function nextElement(node) {
    var sib = node.nextSibling;
    while (sib && sib.nodeType !== 1) sib = sib.nextSibling;
    return sib;
  }

  function errorNode(field) {
    var sib = nextElement(field);
    if (sib && sib.classList && sib.classList.contains('pcv-field-error')) return sib;
    var node = el('p', { class: 'pcv-field-error', role: 'alert' });
    node.hidden = true;
    if (field.parentNode) field.parentNode.insertBefore(node, field.nextSibling);
    return node;
  }

  function runValidators(value, list) {
    for (var i = 0; i < list.length; i++) {
      var result = list[i](value);
      if (result) return result;
    }
    return null;
  }

  function bindValidation(form, rules, submitButton) {
    var names = Object.keys(rules || {});
    var touched = {};
    function refresh() {
      var allOk = true;
      names.forEach(function (name) {
        var field = fieldElement(form, name);
        if (!field) {
          allOk = false;
          return;
        }
        var failure = runValidators(field.value, rules[name] || []);
        if (failure) allOk = false;
        if (!touched[name]) return;
        var message = errorNode(field);
        if (!message.id) message.id = 'pcv-field-error-' + (++seq);
        if (failure) {
          message.hidden = false;
          message.textContent = text(failure.messageKey);
          field.setAttribute('aria-invalid', 'true');
          field.setAttribute('aria-describedby', message.id);
        } else {
          message.hidden = true;
          message.textContent = '';
          field.setAttribute('aria-invalid', 'false');
          field.removeAttribute('aria-describedby');
        }
      });
      if (submitButton) submitButton.disabled = !allOk;
      return allOk;
    }
    function mark(name) {
      touched[name] = true;
      refresh();
    }
    names.forEach(function (name) {
      var field = fieldElement(form, name);
      if (!field) return;
      field.addEventListener('input', function () { mark(name); });
      field.addEventListener('blur', function () { mark(name); });
      field.addEventListener('change', refresh);
    });
    refresh();
    return refresh;
  }

  function focusable(root) {
    var nodes = root.querySelectorAll('button, [href], input, select, textarea, [tabindex]');
    var list = [];
    for (var i = 0; i < nodes.length; i++) {
      var node = nodes[i];
      if (node.disabled) continue;
      if (node.getAttribute('tabindex') === '-1') continue;
      list.push(node);
    }
    return list;
  }

  function restoreFocus(previous) {
    try {
      if (previous && typeof previous.focus === 'function' && previous.isConnected !== false) previous.focus();
    } catch (err) { /* the opener may already be gone */ }
  }

  function confirmDialog(opts) {
    opts = opts || {};
    var previous = document.activeElement;
    var danger = opts.danger === true;
    var typed = Object.prototype.hasOwnProperty.call(opts, 'typedName') && opts.typedName != null;
    var titleId = 'pcv-confirm-title-' + (++seq);
    var descId = 'pcv-confirm-desc-' + (++seq);
    var title = el('h2', { id: titleId }, opts.title || '');
    var message = el('p', { id: descId, class: 'pcv-confirm-message' }, opts.message || '');
    var input = typed ? el('input', {
      class: 'pcv-confirm-typed',
      type: 'text',
      autocomplete: 'off',
      'aria-label': opts.title || text('ui.confirm.confirm')
    }) : null;
    var cancelBtn = el('button', { type: 'button', class: 'btn', 'data-pcv-confirm': 'cancel' }, opts.cancelLabel || text('ui.confirm.cancel'));
    var acceptBtn = el('button', {
      type: 'button',
      class: danger ? 'btn pcv-confirm-danger' : 'btn',
      'data-pcv-confirm': 'accept'
    }, opts.confirmLabel || text('ui.confirm.confirm'));
    if (typed) acceptBtn.disabled = true;
    var body = [title, message];
    if (input) body.push(input);
    body.push(el('div', { class: 'pcv-confirm-actions' }, cancelBtn, acceptBtn));
    return new Promise(function (resolve) {
      var settled = false;
      var dlg = null;
      function finish(value) {
        if (settled) return;
        settled = true;
        var node = dlg;
        try {
          if (node && PCV.modalCore) PCV.modalCore.closeDialog(node);
        } catch (err) { /* already closing */ }
        restoreFocus(previous);
        resolve(value === true);
      }
      dlg = PCV.modalCore.open(body, {
        backdropDismiss: !danger,
        onClose: function () { finish(false); }
      });
      dlg.setAttribute('role', 'dialog');
      dlg.setAttribute('aria-modal', 'true');
      dlg.setAttribute('aria-labelledby', titleId);
      dlg.setAttribute('aria-describedby', descId);
      dlg.classList.add('pcv-confirm');
      dlg.addEventListener('keydown', function (event) {
        if (event.key === 'Escape') {
          event.preventDefault();
          finish(false);
          return;
        }
        if (event.key !== 'Tab') return;
        var items = focusable(dlg);
        if (!items.length) return;
        var index = items.indexOf(document.activeElement);
        var next = event.shiftKey
          ? (index <= 0 ? items.length - 1 : index - 1)
          : (index === -1 || index >= items.length - 1 ? 0 : index + 1);
        event.preventDefault();
        items[next].focus();
      });
      cancelBtn.addEventListener('click', function () { finish(false); });
      acceptBtn.addEventListener('click', function () {
        if (acceptBtn.disabled) return;
        finish(true);
      });
      if (input) {
        var syncTyped = function () { acceptBtn.disabled = input.value !== opts.typedName; };
        input.addEventListener('input', syncTyped);
        syncTyped();
      }
      var first = input || (danger ? cancelBtn : acceptBtn);
      if (first && typeof first.focus === 'function') first.focus();
    });
  }

  var TOAST_MS = 5000;
  var JOB_POLL_MS = 2000;

  function toast(message, options) {
    options = options || {};
    var kind = options.kind === 'success' || options.kind === 'error' || options.kind === 'info' ? options.kind : 'info';
    var container = document.getElementById('toasts');
    if (!container) {
      container = el('div', { id: 'toasts' });
      if (document.body) document.body.appendChild(container);
    }
    var action = options.actionLabel ? el('button', {
      type: 'button',
      class: 'btn pcv-toast-action'
    }, options.actionLabel) : null;
    var closeBtn = el('button', {
      type: 'button',
      class: 'btn pcv-toast-close',
      'aria-label': text('ui.toast.close')
    }, text('ui.toast.close'));
    var node = el('div', { class: 'toast pcv-toast pcv-toast-' + kind, role: 'status' },
      el('div', { class: 'pcv-toast-message' }, message == null ? '' : String(message)),
      options.detail ? el('div', { class: 'pcv-toast-detail' }, String(options.detail)) : null,
      action,
      closeBtn);
    if (action && typeof options.onAction === 'function') {
      action.addEventListener('click', options.onAction);
    }
    var timer = 0;
    var token = 0;
    var holds = 0;
    var remaining = TOAST_MS;
    var started = 0;
    function dismiss() {
      token++;
      if (timer) clearTimeout(timer);
      timer = 0;
      if (node.parentNode) node.parentNode.removeChild(node);
    }
    function arm() {
      if (kind === 'error' || holds > 0) return;
      var mine = ++token;
      started = Date.now();
      timer = setTimeout(function () {
        if (mine !== token) return;
        dismiss();
      }, remaining);
    }
    function hold() {
      if (kind === 'error') return;
      holds++;
      if (holds !== 1) return;
      token++;
      if (!timer) return;
      clearTimeout(timer);
      timer = 0;
      remaining = Math.max(0, remaining - (Date.now() - started));
    }
    function release() {
      if (kind === 'error') return;
      holds = Math.max(0, holds - 1);
      if (holds === 0) arm();
    }
    closeBtn.addEventListener('click', dismiss);
    node.addEventListener('mouseenter', hold);
    node.addEventListener('mouseleave', release);
    node.addEventListener('focusin', hold);
    node.addEventListener('focusout', release);
    container.appendChild(node);
    arm();
    return node;
  }

  function jobRawStatus(job) {
    var raw = job && (job.status != null ? job.status : job.state);
    return raw == null ? '' : String(raw);
  }

  function knownJobStatus(status) {
    return status === 'queued' || status === 'running' || status === 'succeeded' || status === 'failed' || status === 'canceled';
  }

  function jobPhase(job) {
    var folded = jobRawStatus(job).toLowerCase();
    return knownJobStatus(folded) ? folded : 'queued';
  }

  function jobPhaseAttr(job) {
    var raw = jobRawStatus(job);
    var folded = raw.toLowerCase();
    return knownJobStatus(folded) ? folded : raw;
  }

  function jobError(job) {
    var error = job && job.error;
    if (error && typeof error === 'object') return error;
    return {
      code: job && (job.error_code || job.code) || '',
      message: typeof error === 'string' ? error : '',
      detail: job && job.detail || ''
    };
  }

  function elapsedSeconds(job) {
    var start = Date.parse(job && (job.created_at || job.createdAt) || '');
    if (!isFinite(start)) return null;
    var phase = jobPhase(job);
    var end = Date.parse(job.updated_at || job.updatedAt || '');
    var until = (phase === 'succeeded' || phase === 'failed' || phase === 'canceled') && isFinite(end) ? end : Date.now();
    return Math.max(0, Math.round((until - start) / 1000));
  }

  function renderJobProgress(job) {
    var phase = jobPhase(job);
    var seconds = elapsedSeconds(job);
    var error = jobError(job);
    var code = error.code || '';
    var detailText = [error.message, error.detail].filter(function (part) { return part; }).join(' ');
    var detail = el('p', { class: 'pcv-job-detail' }, detailText);
    detail.hidden = true;
    var detailsBtn = phase === 'failed' ? el('button', { type: 'button', class: 'btn pcv-job-details' }, text('ui.job.details')) : null;
    if (detailsBtn) {
      detailsBtn.addEventListener('click', function () { detail.hidden = !detail.hidden; });
    }
    var phaseKey = phase === 'succeeded' ? 'ui.job.succeeded'
      : phase === 'failed' ? 'ui.job.failed'
      : phase === 'canceled' ? 'ui.job.canceled'
      : phase === 'running' ? 'ui.job.running'
      : 'ui.job.queued';
    return el('div', { class: 'pcv-job pcv-job-' + phase, 'data-phase': jobPhaseAttr(job) },
      el('span', { class: 'pcv-job-phase' }, text(phaseKey)),
      seconds == null ? null : el('span', { class: 'pcv-job-elapsed' }, text('ui.job.elapsed', { seconds: String(seconds) })),
      phase === 'failed' && code ? el('span', { class: 'pcv-job-code' }, text('ui.job.code', { code: code })) : null,
      detailsBtn,
      phase === 'failed' ? detail : null);
  }

  function unwrapJob(payload) {
    if (payload && typeof payload === 'object' && payload.data && typeof payload.data === 'object' && payload.data.status) return payload.data;
    return payload || {};
  }

  function readJob(jobId) {
    var desktop = PCV.desktopApi;
    if (desktop && typeof desktop.getJob === 'function') {
      return Promise.resolve(desktop.getJob(jobId)).then(unwrapJob);
    }
    var api = PCV.api;
    var jobUrl = typeof EP !== 'undefined' && EP && typeof EP.JOB === 'function' ? EP.JOB(jobId) : null;
    if (api && typeof api.fetchGet === 'function' && jobUrl) {
      return Promise.resolve(api.fetchGet(jobUrl)).then(unwrapJob);
    }
    return Promise.reject(new Error('PCV_JOB_READ_UNAVAILABLE'));
  }

  function isTerminal(job) {
    var folded = jobRawStatus(job).toLowerCase();
    return folded === 'succeeded' || folded === 'failed' || folded === 'canceled';
  }

  function trackJob(jobId, onUpdate) {
    var stopped = false;
    var timer = 0;
    function stop() {
      stopped = true;
      if (timer) clearTimeout(timer);
      timer = 0;
    }
    function schedule() {
      if (stopped) return;
      timer = setTimeout(tick, JOB_POLL_MS);
    }
    function tick() {
      if (stopped) return;
      readJob(jobId).then(function (job) {
        if (stopped) return;
        if (typeof onUpdate === 'function') onUpdate(job);
        if (isTerminal(job)) {
          stop();
          return;
        }
        schedule();
      }, function () {
        if (!stopped) schedule();
      });
    }
    tick();
    return stop;
  }

  function readRemembered(key) {
    if (!key) return null;
    try { return localStorage.getItem(key); } catch (err) { return null; }
  }

  function writeRemembered(key, id) {
    if (!key) return;
    try { localStorage.setItem(key, id); } catch (err) { /* storage may be blocked */ }
  }

  function renderTabs(container, tabs, activeId, onSelect, options) {
    options = options || {};
    var items = Array.isArray(tabs) ? tabs : [];
    var remembered = readRemembered(options.rememberKey);
    var current = activeId || remembered;
    if (!items.some(function (tab) { return tab.id === current; })) current = items.length ? items[0].id : '';
    var buttons = [];
    var panels = [];
    function select(id, notify) {
      current = id;
      buttons.forEach(function (button, index) {
        var on = items[index].id === id;
        button.setAttribute('aria-selected', on ? 'true' : 'false');
        button.tabIndex = on ? 0 : -1;
        panels[index].hidden = !on;
      });
      writeRemembered(options.rememberKey, id);
      if (notify && typeof onSelect === 'function') onSelect(id);
    }
    function move(from, key) {
      if (!buttons.length) return;
      var index = buttons.indexOf(from);
      if (index < 0) index = buttons.findIndex(function (button) { return button.getAttribute('aria-selected') === 'true'; });
      if (index < 0) index = 0;
      var next = index;
      if (key === 'Home') next = 0;
      else if (key === 'End') next = buttons.length - 1;
      else if (key === 'ArrowRight' || key === 'ArrowDown') next = (index + 1) % buttons.length;
      else if (key === 'ArrowLeft' || key === 'ArrowUp') next = (index - 1 + buttons.length) % buttons.length;
      else return;
      select(items[next].id, true);
      buttons[next].focus();
    }
    items.forEach(function (tab) {
      var panelId = 'pcv-tabpanel-' + tab.id + '-' + (++seq);
      var on = tab.id === current;
      var button = el('button', {
        type: 'button',
        class: 'pcv-tab',
        role: 'tab',
        id: 'pcv-tab-' + tab.id + '-' + seq,
        'aria-selected': on ? 'true' : 'false',
        'aria-controls': panelId,
        tabindex: on ? '0' : '-1'
      }, text(tab.labelKey));
      button.addEventListener('click', function () { select(tab.id, true); });
      button.tabIndex = on ? 0 : -1;
      var panel = el('div', { class: 'pcv-tabpanel', role: 'tabpanel', id: panelId, 'aria-labelledby': button.id });
      panel.hidden = !on;
      buttons.push(button);
      panels.push(panel);
    });
    var tablist = el('div', { class: 'pcv-tablist', role: 'tablist' }, buttons);
    tablist.addEventListener('keydown', function (event) {
      if (event.key !== 'ArrowRight' && event.key !== 'ArrowLeft' && event.key !== 'ArrowDown' && event.key !== 'ArrowUp' && event.key !== 'Home' && event.key !== 'End') return;
      event.preventDefault();
      var target = event.target;
      if (!target || buttons.indexOf(target) < 0) target = buttons.filter(function (button) { return button.getAttribute('aria-selected') === 'true'; })[0];
      move(target, event.key);
    });
    if (PCV.uxlib && PCV.uxlib.clearEl) PCV.uxlib.clearEl(container);
    else while (container.firstChild) container.removeChild(container.firstChild);
    container.appendChild(tablist);
    panels.forEach(function (panel) { container.appendChild(panel); });
    writeRemembered(options.rememberKey, current);
    return { tablist: tablist, panels: panels, select: function (id) { select(id, true); } };
  }

  function headingTag(level) {
    var n = Number(level);
    if (n === 2 || n === 3 || n === 4) return 'h' + n;
    return 'h3';
  }

  function emptyState(opts) {
    opts = opts || {};
    var action = opts.actionLabelKey ? el('button', { type: 'button', class: 'btn pcv-empty-action' }, text(opts.actionLabelKey)) : null;
    if (action && typeof opts.onAction === 'function') action.addEventListener('click', opts.onAction);
    return el('div', { class: 'pcv-empty', role: 'status' },
      el(headingTag(opts.headingLevel), { class: 'pcv-empty-title' }, text(opts.titleKey)),
      el('p', { class: 'pcv-empty-body' }, text(opts.bodyKey)),
      action);
  }

  function loadError(opts) {
    opts = opts || {};
    var retry = el('button', { type: 'button', class: 'btn pcv-load-error-retry' }, text('ui.load_error.retry'));
    if (typeof opts.onRetry === 'function') retry.addEventListener('click', opts.onRetry);
    return el('div', { class: 'pcv-load-error', role: 'alert' },
      el(headingTag(opts.headingLevel), { class: 'pcv-load-error-title' }, text(opts.titleKey)),
      el('p', { class: 'pcv-load-error-detail' }, opts.detail == null ? '' : String(opts.detail)),
      retry);
  }

  PCV.components = {
    confirmDialog: confirmDialog,
    bindValidation: bindValidation,
    validators: validators,
    toast: toast,
    renderJobProgress: renderJobProgress,
    trackJob: trackJob,
    renderTabs: renderTabs,
    emptyState: emptyState,
    loadError: loadError
  };
})(window.PCV);
