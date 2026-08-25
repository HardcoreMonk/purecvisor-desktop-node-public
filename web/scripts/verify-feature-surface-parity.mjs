import fs from 'node:fs';
import path from 'node:path';
import vm from 'node:vm';
import { fileURLToPath } from 'node:url';
import ts from 'typescript';

const scriptRoot = path.dirname(fileURLToPath(import.meta.url));
const webRoot = path.resolve(scriptRoot, '..');
const repoRoot = path.resolve(webRoot, '..');
const ledgerPath = path.join(repoRoot, 'config', 'desktop-node-feature-surface-ledger.json');
const routesPath = path.join(webRoot, 'src', 'served', 'routes.ts');

function fail(message) {
  throw new Error(`PCV_FEATURE_SURFACE_PARITY_FAILED|surface=web|${message}`);
}

function normalizeRouteTemplate(route) {
  return String(route)
    .replaceAll('{vm_id}', '{vmId}')
    .replaceAll('{job_id}', '{jobId}')
    .replaceAll('{checkpoint_id}', '{checkpointId}')
    .replaceAll('{bundle_id}', '{bundleId}');
}

function expandRouteTemplate(route) {
  const normalized = normalizeRouteTemplate(route);
  const separator = normalized.lastIndexOf('/');
  const prefix = normalized.slice(0, separator + 1);
  const finalSegment = normalized.slice(separator + 1);
  return finalSegment.includes('|')
    ? finalSegment.split('|').map((segment) => `${prefix}${segment}`)
    : [normalized];
}

function routeKey(method, route) {
  return `${String(method).toUpperCase()} ${route}`;
}

function loadWebCoverage() {
  const source = fs.readFileSync(routesPath, 'utf8');
  const compiled = ts.transpileModule(source, {
    compilerOptions: {
      module: ts.ModuleKind.None,
      target: ts.ScriptTarget.ES2022
    },
    fileName: routesPath
  }).outputText;
  const context = vm.createContext({
    normalizeError: (value) => value
  });
  vm.runInContext(
    `${compiled}\nglobalThis.__pcvFeatureRouteCoverage = DESKTOP_NODE_ROUTE_COVERAGE;`,
    context,
    { filename: routesPath }
  );
  return Array.from(context.__pcvFeatureRouteCoverage ?? []);
}

const ledger = JSON.parse(fs.readFileSync(ledgerPath, 'utf8'));
const expected = new Map();
let excludedCount = 0;

for (const feature of ledger.features ?? []) {
  for (const route of feature.routes ?? []) {
    const webPresent = route.present_surfaces?.includes('web') === true;
    const webBinding = route.surface_bindings?.web;
    if (!webPresent) {
      excludedCount += 1;
      if (webBinding) {
        fail(`excluded route has binding|feature_id=${feature.feature_id}|method=${route.method}|route=${route.route_template}`);
      }
      continue;
    }
    if (!webBinding?.coverage_id) {
      fail(`present route missing binding|feature_id=${feature.feature_id}|method=${route.method}|route=${route.route_template}`);
    }
    const key = routeKey(route.method, route.route_template);
    if (expected.has(key)) {
      fail(`duplicate ledger route|key=${key}`);
    }
    expected.set(key, {
      featureId: feature.feature_id,
      coverageId: webBinding.coverage_id
    });
  }
}

const actual = new Map();
for (const coverage of loadWebCoverage()) {
  for (const route of expandRouteTemplate(coverage.route)) {
    const key = routeKey(coverage.method, route);
    if (actual.has(key)) {
      fail(`duplicate Web coverage route|key=${key}`);
    }
    actual.set(key, {
      featureId: coverage.featureId,
      coverageId: coverage.id
    });
  }
}

for (const [key, expectedBinding] of expected) {
  const actualBinding = actual.get(key);
  if (!actualBinding) {
    fail(`missing Web route|key=${key}|feature_id=${expectedBinding.featureId}`);
  }
  if (actualBinding.featureId !== expectedBinding.featureId) {
    fail(`Feature ID mismatch|key=${key}|expected=${expectedBinding.featureId}|actual=${actualBinding.featureId ?? 'missing'}`);
  }
  if (actualBinding.coverageId !== expectedBinding.coverageId) {
    fail(`coverage ID mismatch|key=${key}|expected=${expectedBinding.coverageId}|actual=${actualBinding.coverageId}`);
  }
}

for (const key of actual.keys()) {
  if (!expected.has(key)) {
    fail(`unexpected Web route|key=${key}`);
  }
}

if (expected.size !== 52 || excludedCount !== 8) {
  fail(`count mismatch|present=${expected.size}|excluded=${excludedCount}|expected_present=52|expected_excluded=8`);
}

process.stdout.write(`Feature surface parity PASS: web=${expected.size} excluded=${excludedCount}\n`);
