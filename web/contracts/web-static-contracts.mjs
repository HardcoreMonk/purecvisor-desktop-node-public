import {
  WEB_CONTRACT_ERROR_CODES,
  WebContractError
} from "./web-contract-harness.mjs";

const DOMAINS = new Set([
  "shell-assets",
  "routes-actions",
  "operations-evidence",
  "typescript-parity"
]);

const OWNER_IDS = new Set([
  "feature-surface",
  "typescript",
  "served-asset",
  "frontend-batches",
  "static-parity",
  "browser-fixture",
  "node-check",
  "static-contract"
]);

const METADATA_LEDGER = [
  ["web.static.feature-surface-ledger", "validates and wires the stable Feature ID surface ledger", "shell-assets", ["feature-surface"], "43-54"],
  ["web.static.root-assets", "ships index, stylesheet, and script assets under the Desktop Node web root", "shell-assets", ["static-contract"], "55-66"],
  ["web.static.inline-favicon", "declares an inline favicon to avoid favicon.ico console noise", "shell-assets", ["static-contract"], "67-74"],
  ["web.static.single-edge-isolation", "keeps the Desktop Node web console isolated from the Single Edge ui tree", "shell-assets", ["static-contract"], "75-84"],
  ["web.static.design-boundary", "ships a Desktop Node web design contract without importing Single Edge runtime routes", "shell-assets", ["static-contract"], "85-102"],
  ["web.static.supanova-tokens", "uses Desktop Node Supanova operation-console tokens in the active stylesheet", "shell-assets", ["static-contract"], "103-119"],
  ["web.static.visual-shell", "ports the Single Edge visual shell into the active Desktop Node console without importing runtime routes", "shell-assets", ["static-contract"], "120-142"],
  ["web.static.workbench-frame", "clones the Single console workbench frame while keeping Linux service surfaces excluded", "shell-assets", ["static-contract"], "143-168"],
  ["web.static.frontend-mockups", "ships frontend completion mockup sample screens inside the Desktop Node web root", "shell-assets", ["static-contract"], "169-191"],
  ["web.static.frontend-batches", "declares the 1-25 frontend completion work as five automatic staged batches", "shell-assets", ["frontend-batches"], "192-241"],
  ["web.static.phase2h-endpoints", "declares the Phase 2H API endpoints used by the console", "routes-actions", ["static-contract"], "242-252"],
  ["web.static.local-api-registry", "centralizes Local API access behind the Desktop Node frontend service registry", "routes-actions", ["feature-surface", "static-contract"], "253-276"],
  ["web.static.qos-guest-readback", "declares the Web VM QoS and guest readback operator surface as read-only routes", "routes-actions", ["static-contract"], "277-299"],
  ["web.static.qos-guest-control", "opens Web VM QoS and ADR-0009 Guest Execution direct control routes with explicit operator controls", "routes-actions", ["static-contract"], "300-332"],
  ["web.static.guest-exec-cancel", "exposes running guest execution cancel affordance on Web job rows", "routes-actions", ["static-contract"], "333-345"],
  ["web.static.search-event-table", "adds Windows-local command palette, global search, event center, and table helpers from the Single Edge borrowing map", "routes-actions", ["static-contract"], "346-373"],
  ["web.static.served-source-parts", "splits frontend service logic into staged source parts before generating app.js", "routes-actions", ["served-asset", "static-contract"], "374-397"],
  ["web.static.optional-bearer", "supports optional bearer token requests", "routes-actions", ["static-contract"], "398-405"],
  ["web.static.account-rbac-console", "declares account RBAC JWT login, refresh, session, and console capability UX", "routes-actions", ["static-contract"], "406-437"],
  ["web.static.listener-api-base", "loads listener-provided API base URL before the served app starts", "routes-actions", ["static-contract"], "438-449"],
  ["web.static.vm-create-payload", "declares the VM create payload fields expected by POST /api/v1/vms", "routes-actions", ["static-contract"], "450-459"],
  ["web.static.vm-lifecycle-routes", "declares the Phase 3B VM detail and lifecycle endpoints used by the console", "routes-actions", ["static-contract"], "460-474"],
  ["web.static.vm-detail-mount", "ships a VM detail panel mount point", "routes-actions", ["static-contract"], "475-487"],
  ["web.static.vm-lifecycle-actions", "declares lifecycle action handlers and destructive confirmation", "routes-actions", ["static-contract"], "488-508"],
  ["web.static.checkpoint-actions", "declares checkpoint UI actions used by the console", "routes-actions", ["static-contract"], "509-518"],
  ["web.static.browser-job-history", "persists tracked browser job history locally", "routes-actions", ["static-contract"], "519-530"],
  ["web.static.job-orchestration", "hardens browser job orchestration with scoped pending state, polling backoff, and next-page loading", "routes-actions", ["static-contract"], "531-547"],
  ["web.static.shell-controls", "binds the Single-style shell controls to Desktop Node view and asset state", "routes-actions", ["static-contract"], "548-571"],
  ["web.static.activity-troubleshooting", "declares operator activity and troubleshooting console surfaces", "operations-evidence", ["static-contract"], "572-596"],
  ["web.static.ops-cockpit", "declares the ops cockpit multi-view shell and summary route", "operations-evidence", ["static-contract", "browser-fixture"], "597-635"],
  ["web.static.evidence-dashboard", "declares the batch evidence dashboard surface", "operations-evidence", ["static-contract", "browser-fixture"], "636-671"],
  ["web.static.evidence-degradation", "declares troubleshooting evidence degradation and failed job triage surfaces", "operations-evidence", ["static-contract", "browser-fixture"], "672-687"],
  ["web.static.diagnostic-bundle", "declares diagnostic bundle API create and download UX without direct host mutation commands", "operations-evidence", ["static-contract", "browser-fixture"], "688-732"],
  ["web.static.operator-terms", "keeps operator surface terms aligned with internal distribution boundary", "operations-evidence", ["static-contract"], "733-751"],
  ["web.static.frontend-edge-cases", "hardens final frontend service edge cases before installed-listener evidence", "operations-evidence", ["static-contract", "browser-fixture"], "752-808"],
  ["web.static.token-rotation", "declares token rotation operator UX without service token mutation", "operations-evidence", ["static-contract", "browser-fixture"], "809-824"],
  ["web.static.beta-followup", "declares a beta follow-up status surface without browser-started host mutation", "operations-evidence", ["static-contract", "browser-fixture"], "825-842"],
  ["web.static.monitoring", "declares read-only monitoring auth and checkpoint warning surfaces", "operations-evidence", ["static-contract", "browser-fixture"], "843-859"],
  ["web.static.network-inventory", "declares a read-only network inventory view", "operations-evidence", ["feature-surface", "browser-fixture"], "860-880"],
  ["web.static.workflow-polish", "declares P2 operator workflow polish and quality gates", "operations-evidence", ["static-contract"], "881-892"],
  ["web.static.javascript-syntax", "passes JavaScript syntax validation", "typescript-parity", ["node-check"], "893-897"],
  ["web.static.served-typescript-output", "treats the served app.js asset as TypeScript build output", "typescript-parity", ["served-asset", "static-parity"], "898-915"],
  ["web.static.typescript-scaffold", "declares a Phase 25 TypeScript scaffold that owns the served app.js asset", "typescript-parity", ["typescript", "static-contract"], "916-933"],
  ["web.static.typescript-contract-mirror", "keeps TypeScript source as the Local API contract mirror and served app source", "typescript-parity", ["typescript", "static-contract"], "934-956"],
  ["web.static.parity-manifest", "ships a generated TypeScript parity manifest for the served static asset", "typescript-parity", ["static-parity"], "957-1002"],
  ["web.static.user-visible-fixtures", "ships user-visible fixture parity snapshots for the TypeScript-owned app.js", "typescript-parity", ["static-parity"], "1003-1024"],
  ["web.static.verifier-wiring", "declares generated static parity and served asset verification scripts", "typescript-parity", ["served-asset", "static-parity", "browser-fixture", "frontend-batches"], "1025-1077"],
  ["web.static.generated-parity-alignment", "keeps generated static parity artifacts aligned with the TypeScript-owned served app.js route contract", "typescript-parity", ["static-parity", "browser-fixture"], "1078-1125"],
  ["web.static.secret-mutation-guard", "does not place secrets or host mutation command strings in parity scripts, TypeScript source, or generated output", "typescript-parity", ["static-contract", "static-parity"], "1126-1148"],
  ["web.static.no-fabricated-values", "keeps fabricated operational values out of the static console shell", "typescript-parity", ["static-contract", "browser-fixture"], "1149-1206"]
];

const m = (id, legacyName, domain, owners, legacyLines) =>
  Object.freeze({ id, legacyName, domain, owners: Object.freeze(owners), legacyLines });

function metadataInvalid() {
  throw new WebContractError(WEB_CONTRACT_ERROR_CODES.configInvalid, "metadata=invalid");
}

export function validateWebStaticContractMetadata(metadata) {
  if (!Array.isArray(metadata) || metadata.length !== METADATA_LEDGER.length) {
    metadataInvalid();
  }

  const ids = new Set();
  const legacyNames = new Set();
  let previousLineEnd = 0;

  for (let ordinal = 0; ordinal < metadata.length; ordinal += 1) {
    const item = metadata[ordinal];
    const expected = METADATA_LEDGER[ordinal];
    const keys = item && typeof item === "object" ? Object.keys(item) : [];
    const lineMatch = typeof item?.legacyLines === "string"
      ? /^(\d+)-(\d+)$/.exec(item.legacyLines)
      : null;
    const ownersValid = Array.isArray(item?.owners)
      && item.owners.length > 0
      && new Set(item.owners).size === item.owners.length
      && item.owners.every((owner) => OWNER_IDS.has(owner));

    if (
      keys.length !== 5
      || !["id", "legacyName", "domain", "owners", "legacyLines"].every((key) => keys.includes(key))
      || typeof item.id !== "string"
      || !/^web\.static\.[a-z0-9]+(?:-[a-z0-9]+)*$/.test(item.id)
      || typeof item.legacyName !== "string"
      || item.legacyName.length === 0
      || !DOMAINS.has(item.domain)
      || !ownersValid
      || !lineMatch
    ) {
      metadataInvalid();
    }

    const lineStart = Number(lineMatch[1]);
    const lineEnd = Number(lineMatch[2]);
    if (
      lineStart < 1
      || lineStart > lineEnd
      || lineStart <= previousLineEnd
      || ids.has(item.id)
      || legacyNames.has(item.legacyName)
      || item.id !== expected[0]
      || item.legacyName !== expected[1]
      || item.domain !== expected[2]
      || item.legacyLines !== expected[4]
      || item.owners.length !== expected[3].length
      || item.owners.some((owner, index) => owner !== expected[3][index])
    ) {
      metadataInvalid();
    }

    previousLineEnd = lineEnd;
    ids.add(item.id);
    legacyNames.add(item.legacyName);
  }

  return metadata;
}

function legacyDeclarationsAmbiguous() {
  throw new WebContractError(
    WEB_CONTRACT_ERROR_CODES.configInvalid,
    "legacy_declarations=ambiguous"
  );
}

function scanPowerShellDeclarationLines(source) {
  const lines = source.split(/\r?\n/);
  const declarationLines = [];
  let blockCommentDepth = 0;
  let hereStringQuote;

  for (const line of lines) {
    const code = Array.from(line, () => " ");

    if (hereStringQuote !== undefined) {
      const closingPattern = hereStringQuote === "'"
        ? /^'@\s*$/
        : /^"@\s*$/;
      if (closingPattern.test(line)) {
        hereStringQuote = undefined;
      }
      declarationLines.push(code.join(""));
      continue;
    }

    let cursor = 0;
    while (cursor < line.length) {
      if (blockCommentDepth > 0) {
        if (line.startsWith("<#", cursor)) {
          blockCommentDepth += 1;
          cursor += 2;
        } else if (line.startsWith("#>", cursor)) {
          blockCommentDepth -= 1;
          cursor += 2;
        } else {
          cursor += 1;
        }
        continue;
      }

      if (line.startsWith("<#", cursor)) {
        blockCommentDepth = 1;
        cursor += 2;
        continue;
      }
      if (line[cursor] === "#") {
        break;
      }
      if (
        line[cursor] === "@"
        && (line[cursor + 1] === "'" || line[cursor + 1] === '"')
        && /^[\t ]*$/.test(line.slice(cursor + 2))
      ) {
        hereStringQuote = line[cursor + 1];
        break;
      }

      const quote = line[cursor];
      if (quote === "'" || quote === '"') {
        code[cursor] = quote;
        cursor += 1;
        while (cursor < line.length) {
          code[cursor] = line[cursor];
          if (quote === "'" && line[cursor] === "'") {
            if (line[cursor + 1] === "'") {
              code[cursor + 1] = "'";
              cursor += 2;
              continue;
            }
            cursor += 1;
            break;
          }
          if (quote === '"' && line[cursor] === "`" && cursor + 1 < line.length) {
            code[cursor + 1] = line[cursor + 1];
            cursor += 2;
            continue;
          }
          if (quote === '"' && line[cursor] === '"') {
            cursor += 1;
            break;
          }
          cursor += 1;
        }
        continue;
      }

      code[cursor] = line[cursor];
      if (line[cursor] === "`" && cursor + 1 < line.length) {
        code[cursor + 1] = line[cursor + 1];
        cursor += 2;
      } else {
        cursor += 1;
      }
    }

    declarationLines.push(code.join(""));
  }

  if (blockCommentDepth !== 0 || hereStringQuote !== undefined) {
    legacyDeclarationsAmbiguous();
  }

  return declarationLines;
}

function containsUnescapedDollar(value) {
  for (let index = 0; index < value.length; index += 1) {
    if (value[index] === "`") {
      index += 1;
    } else if (value[index] === "$") {
      return true;
    }
  }
  return false;
}

export function parseLegacyPesterTests(source) {
  if (typeof source !== "string") {
    legacyDeclarationsAmbiguous();
  }

  const lines = scanPowerShellDeclarationLines(source);
  const broadCount = lines.filter((line) => /^\s*It\b/.test(line)).length;
  const parsed = [];
  const declaration = /^\s*It\s+(?:'((?:[^']|'')*)'|"((?:[^"`]|`.)*)")\s*\{/;

  for (let index = 0; index < lines.length; index += 1) {
    if (!/^\s*It\b/.test(lines[index])) {
      continue;
    }
    const match = declaration.exec(lines[index]);
    if (!match) {
      continue;
    }
    if (match[2] !== undefined && containsUnescapedDollar(match[2])) {
      continue;
    }
    const name = match[1] === undefined
      ? match[2].replace(/`(.)/g, "$1")
      : match[1].replace(/''/g, "'");
    parsed.push({ name, line: index + 1 });
  }

  if (parsed.length !== broadCount) {
    legacyDeclarationsAmbiguous();
  }

  return parsed;
}

export const WEB_STATIC_CONTRACT_METADATA = Object.freeze(
  METADATA_LEDGER.map(([id, legacyName, domain, owners, legacyLines]) =>
    m(id, legacyName, domain, owners, legacyLines))
);

validateWebStaticContractMetadata(WEB_STATIC_CONTRACT_METADATA);

function assertObject(context, value, label) {
  context.assertEqual(
    value !== null && typeof value === "object" && !Array.isArray(value),
    true,
    label
  );
}

function assertArray(context, value, label) {
  context.assertEqual(Array.isArray(value), true, label);
}

function assertObjectKeys(context, value, allowed, required, label) {
  assertObject(context, value, `${label}:object`);
  const keys = Object.keys(value);
  context.assertEqual(
    keys.every((key) => allowed.includes(key))
      && required.every((key) => keys.includes(key)),
    true,
    `${label}:keys`
  );
}

function assertExactUniqueMembers(context, values, expected, label) {
  assertArray(context, values, `${label}:array`);
  context.assertEqual(
    values.length === expected.length
      && new Set(values).size === values.length
      && expected.every((value) => values.includes(value)),
    true,
    label
  );
}

function assertNonemptyArray(context, value, label) {
  assertArray(context, value, `${label}:array`);
  context.assertEqual(value.length > 0, true, label);
}

function verifyFeatureSurfaceLedger(context) {
  const ledgerPath = "config/desktop-node-feature-surface-ledger.json";
  const schemaPath = "config/desktop-node-feature-surface-ledger.schema.json";
  context.assertExists(ledgerPath, "feature-surface-ledger:file");
  context.assertExists(schemaPath, "feature-surface-ledger:schema-file");
  context.assertExists("web/scripts/verify-feature-surface-parity.mjs", "feature-surface-ledger:verifier");

  const schema = context.readJson(schemaPath);
  assertObject(context, schema, "feature-surface-ledger:schema-object");
  context.assertEqual(schema.$schema, "https://json-schema.org/draft/2020-12/schema", "feature-surface-ledger:schema-draft");
  context.assertEqual(schema.properties?.$schema?.const, "desktop-node-feature-surface-ledger.schema.json", "feature-surface-ledger:schema-name-const");
  context.assertEqual(schema.properties?.schema_version?.const, 1, "feature-surface-ledger:schema-version-const");
  context.assertEqual(schema.properties?.contract?.const, "pcv-feature-surface-ledger-v1", "feature-surface-ledger:contract-const");

  context.assertMatch(context.readText(ledgerPath), /"\$schema"\s*:/, "feature-surface-ledger:schema-key");
  const ledger = context.readJson(ledgerPath);
  assertObjectKeys(
    context,
    ledger,
    ["$schema", "schema_version", "contract", "target_surfaces", "features"],
    ["$schema", "schema_version", "contract", "target_surfaces", "features"],
    "feature-surface-ledger"
  );
  context.assertEqual(ledger.$schema, "desktop-node-feature-surface-ledger.schema.json", "feature-surface-ledger:schema");
  context.assertEqual(ledger.schema_version, 1, "feature-surface-ledger:schema-version");
  context.assertEqual(ledger.contract, "pcv-feature-surface-ledger-v1", "feature-surface-ledger:contract");
  assertExactUniqueMembers(context, ledger.target_surfaces, ["api", "cli", "web"], "feature-surface-ledger:target-surfaces");
  assertNonemptyArray(context, ledger.features, "feature-surface-ledger:features");

  for (const [featureIndex, feature] of ledger.features.entries()) {
    const featureLabel = `feature-surface-ledger:feature-${featureIndex}`;
    assertObjectKeys(context, feature, ["feature_id", "title", "routes"], ["feature_id", "title", "routes"], featureLabel);
    context.assertMatch(feature.feature_id, /^pcv\.[a-z0-9._-]+$/, `${featureLabel}:id`);
    context.assertEqual(typeof feature.title === "string" && feature.title.length > 0, true, `${featureLabel}:title`);
    assertNonemptyArray(context, feature.routes, `${featureLabel}:routes`);

    for (const [routeIndex, route] of feature.routes.entries()) {
      const routeLabel = `${featureLabel}:route-${routeIndex}`;
      assertObjectKeys(
        context,
        route,
        ["operation_id", "method", "route_template", "required_permission", "present_surfaces", "excluded_surfaces", "surface_bindings"],
        ["operation_id", "method", "route_template", "required_permission", "present_surfaces", "excluded_surfaces"],
        routeLabel
      );
      context.assertMatch(route.operation_id, /^[a-z0-9.-]+$/, `${routeLabel}:operation-id`);
      context.assertIncludes(["GET", "POST", "DELETE"], route.method, `${routeLabel}:method`);
      context.assertMatch(route.route_template, /^\/api\/v1\//, `${routeLabel}:route-template`);
      context.assertEqual(
        route.required_permission === null || typeof route.required_permission === "string",
        true,
        `${routeLabel}:required-permission`
      );
      assertNonemptyArray(context, route.present_surfaces, `${routeLabel}:present-surfaces`);
      const present = new Set(route.present_surfaces);
      context.assertEqual(
        present.size === route.present_surfaces.length
          && route.present_surfaces.every((surface) => ["api", "cli", "web"].includes(surface))
          && present.has("api"),
        true,
        `${routeLabel}:present-surfaces`
      );
      assertArray(context, route.excluded_surfaces, `${routeLabel}:excluded-surfaces`);
      const excluded = new Set();
      for (const [excludedIndex, excludedSurface] of route.excluded_surfaces.entries()) {
        const excludedLabel = `${routeLabel}:excluded-${excludedIndex}`;
        assertObjectKeys(context, excludedSurface, ["surface", "reason"], ["surface", "reason"], excludedLabel);
        context.assertIncludes(["cli", "web"], excludedSurface.surface, `${excludedLabel}:surface`);
        context.assertEqual(
          typeof excludedSurface.reason === "string" && excludedSurface.reason.length > 0,
          true,
          `${excludedLabel}:reason`
        );
        context.assertEqual(excluded.has(excludedSurface.surface), false, `${routeLabel}:excluded-surfaces-unique`);
        excluded.add(excludedSurface.surface);
      }

      const projectedSurfaces = new Set([...present, ...excluded]);
      context.assertEqual(
        [...present].every((surface) => !excluded.has(surface))
          && projectedSurfaces.size === ledger.target_surfaces.length
          && ledger.target_surfaces.every((surface) => projectedSurfaces.has(surface)),
        true,
        `${routeLabel}:surface-partition`
      );

      if (route.surface_bindings !== undefined) {
        assertObjectKeys(context, route.surface_bindings, ["web", "cli"], [], `${routeLabel}:surface-bindings`);
        if (route.surface_bindings.web !== undefined) {
          assertObjectKeys(context, route.surface_bindings.web, ["coverage_id"], ["coverage_id"], `${routeLabel}:web-binding`);
          context.assertEqual(
            typeof route.surface_bindings.web.coverage_id === "string"
              && route.surface_bindings.web.coverage_id.length > 0,
            true,
            `${routeLabel}:web-coverage-id`
          );
        }
        if (route.surface_bindings.cli !== undefined) {
          assertObjectKeys(context, route.surface_bindings.cli, ["command"], ["command"], `${routeLabel}:cli-binding`);
          assertNonemptyArray(context, route.surface_bindings.cli.command, `${routeLabel}:cli-command`);
          context.assertEqual(
            route.surface_bindings.cli.command.length >= 2
              && route.surface_bindings.cli.command.every((part) => typeof part === "string" && part.length > 0),
            true,
            `${routeLabel}:cli-command`
          );
        }
      }
      const bindings = route.surface_bindings ?? {};
      context.assertEqual(
        ["cli", "web"].every(
          (surface) => Object.prototype.hasOwnProperty.call(bindings, surface) === present.has(surface)
        ),
        true,
        `${routeLabel}:surface-binding-membership`
      );
    }
  }

  const packageJson = context.readJson("web/package.json");
  context.assertEqual(
    packageJson.scripts?.["check:feature-surfaces"],
    "node scripts/verify-feature-surface-parity.mjs",
    "feature-surface-ledger:package-script"
  );
  context.assertMatch(packageJson.scripts?.test, /^npm run check:feature-surfaces &&/i, "feature-surface-ledger:test-wiring");
  context.assertMatch(context.readServedSource(), /featureId:\s*['"]pcv\./i, "feature-surface-ledger:served-source");
}

function verifyRootAssets(context) {
  for (const [relativePath, label] of [
    ["web/index.html", "index"],
    ["web/styles.css", "styles"],
    ["web/app.js", "app"]
  ]) {
    context.assertExists(relativePath, `root-assets:${label}`);
  }
  const index = context.readText("web/index.html");
  context.assertMatch(index, /PureCVisor Desktop Node/i, "root-assets:title");
  context.assertMatch(index, /styles\.css/i, "root-assets:styles-link");
  context.assertMatch(index, /app\.js/i, "root-assets:script-link");
  context.assertMatch(index, /id="app-root"/i, "root-assets:app-root");
}

function verifyInlineFavicon(context) {
  const index = context.readText("web/index.html");
  context.assertMatch(index, /<link rel="icon"/i, "inline-favicon:link");
  context.assertMatch(index, /data:image\/svg\+xml/i, "inline-favicon:data-uri");
  context.assertNotMatch(index, /href="\/favicon\.ico"/i, "inline-favicon:no-file-request");
}

function verifySingleEdgeIsolation(context) {
  for (const relativePath of ["web/index.html", "web/app.js", "web/styles.css"]) {
    context.assertNotMatch(context.readText(relativePath), /\.\.\/\.\.\/ui\//i, `single-edge-isolation:${relativePath}`);
  }
}

function verifyDesignBoundary(context) {
  context.assertExists("web/DESIGN.md", "design-boundary:file");
  const design = context.readText("web/DESIGN.md");
  for (const [pattern, label] of [
    [/PureCVisor Desktop Node Web DESIGN\.md/i, "title"],
    [/Supanova/i, "supanova"],
    [/Windows Desktop Node/i, "windows-node"],
    [/Single UI Clone Mapping/i, "clone-mapping"],
    [/asset explorer/i, "asset-explorer"],
    [/status bar/i, "status-bar"],
    [/Linux runtime screens are excluded/i, "linux-exclusion"],
    [/web\/src\/served-app\.ts/i, "served-source"],
    [/npm run verify:parity --prefix web/i, "parity-command"]
  ]) {
    context.assertMatch(design, pattern, `design-boundary:${label}`);
  }
  context.assertNotMatch(design, /\/auth\/token|\/ws\/events|\/containers|(?<!\/qos)\/storage|\/ovn|\/networks/i, "design-boundary:no-runtime-routes");
  context.assertNotMatch(design, /journalctl|libvirt|purecvisorsd/i, "design-boundary:no-linux-services");
}

function verifySupanovaTokens(context) {
  const styles = context.readText("web/styles.css");
  for (const [pattern, label] of [
    [/color-scheme:\s*dark/i, "color-scheme"],
    [/--bg:\s*#0a0f1a/i, "bg"],
    [/--bg2:\s*#0f1525/i, "bg2"],
    [/--bg3:\s*#141c2e/i, "bg3"],
    [/--bg-panel:\s*rgba\(15,\s*21,\s*37/i, "bg-panel"],
    [/--accent:\s*#22d3ee/i, "accent"],
    [/--green:\s*#34d399/i, "green"],
    [/--yellow:\s*#fbbf24/i, "yellow"],
    [/--red:\s*#f43f5e/i, "red"],
    [/var\(--font-mono\)/i, "font-mono"]
  ]) {
    context.assertMatch(styles, pattern, `supanova-tokens:${label}`);
  }
  context.assertNotMatch(styles, /\/auth\/token|\/ws\/events|\/containers|(?<!\/qos)\/storage|\/ovn|\/networks/i, "supanova-tokens:no-runtime-routes");
  context.assertNotMatch(styles, /journalctl|libvirt|purecvisorsd/i, "supanova-tokens:no-linux-services");
}

function verifyVisualShell(context) {
  const index = context.readText("web/index.html");
  const styles = context.readText("web/styles.css");
  const combined = index + styles;
  for (const [pattern, label] of [
    [/data-ui-port="single-edge-visual-shell"/i, "port"],
    [/class="shell supanova-console[^"]*"/i, "console"],
    [/class="topbar glass-topbar[^"]*"/i, "topbar"],
    [/class="connection-form control-strip"/i, "controls"],
    [/class="sidebar nav-rail[^"]*"/i, "rail"]
  ]) {
    context.assertMatch(index, pattern, `visual-shell:${label}`);
  }
  for (const [pattern, label] of [
    [/--supanova-glass:\s*rgba/i, "glass-token"],
    [/--supanova-rail:\s*rgba/i, "rail-token"],
    [/--supanova-shadow:/i, "shadow-token"],
    [/\.supanova-console/i, "console-selector"],
    [/\.glass-topbar/i, "topbar-selector"],
    [/\.control-strip/i, "control-selector"],
    [/\.nav-rail/i, "rail-selector"],
    [/\.nav-rail a\.nav-active::before/i, "active-selector"],
    [/body::before/i, "body-selector"]
  ]) {
    context.assertMatch(styles, pattern, `visual-shell:${label}`);
  }
  context.assertNotMatch(combined, /<base\s+href="\/ui\/"|\/auth\/token|\/ws\/events|\/containers|(?<!\/qos)\/storage|\/ovn|\/networks/i, "visual-shell:no-runtime-routes");
  context.assertNotMatch(combined, /journalctl|libvirt|KVM|ZFS|OVS|\bOVN\b|purecvisorsd|Create Linux VM|ubuntu-24\.04/i, "visual-shell:no-linux-literals");
}

function verifyWorkbenchFrame(context) {
  const index = context.readText("web/index.html");
  const styles = context.readText("web/styles.css");
  const combined = index + styles;
  for (const className of [
    "menu-bar",
    "activity-rail",
    "asset-tabs",
    "asset-search",
    "asset-list",
    "workspace-tabbar",
    "status-bar",
    "dashboard-hero",
    "dashboard-pills",
    "quick-action-grid"
  ]) {
    context.assertMatch(index, new RegExp(`class="${className}"`, "i"), `workbench-frame:index-${className}`);
  }
  for (const selector of [
    "single-clone-shell",
    "menu-bar",
    "activity-rail",
    "asset-explorer",
    "workspace-tabbar",
    "status-bar",
    "dashboard-hero",
    "quick-action-grid"
  ]) {
    context.assertMatch(styles, new RegExp(`\\.${selector}`, "i"), `workbench-frame:styles-${selector}`);
  }
  context.assertNotMatch(combined, /컨테이너|LXC|ZFS|\bOVN\b|OVS|KVM|libvirt|purecvisorsd|\/containers|\/storage|\/ovn|\/networks/i, "workbench-frame:no-linux-surfaces");
}

function verifyFrontendMockups(context) {
  const mockupPath = "web/mockups/frontend-completion-samples.html";
  context.assertExists(mockupPath, "frontend-mockups:file");
  const mockup = context.readText(mockupPath);
  for (const [pattern, label] of [
    [/<!doctype html>/i, "doctype"],
    [/Frontend Completion Samples/i, "title"],
    [/Ops Cockpit/i, "ops-cockpit"],
    [/VM Workbench/i, "vm-workbench"],
    [/Network Evidence/i, "network-evidence"],
    [/Recovery Desk/i, "recovery-desk"],
    [/PCV_RATE_LIMIT_EXCEEDED/i, "rate-limit"],
    [/PCV_ROUTE_TIMEOUT/i, "route-timeout"],
    [/Public signing: excluded/i, "signing-boundary"],
    [/External publication: not-claimed/i, "publication-boundary"],
    [/@media \(max-width:\s*820px\)/i, "responsive"]
  ]) {
    context.assertMatch(mockup, pattern, `frontend-mockups:${label}`);
  }
  context.assertMatch(
    mockup,
    /<a\s+class="active"\s+href="#ops-cockpit">\s*Ops Cockpit\s*<\/a>/i,
    "frontend-mockups:ops-cockpit-navigation"
  );
  context.assertMatch(
    mockup,
    /<h2>\s*Ops Cockpit\s*<\/h2>/i,
    "frontend-mockups:ops-cockpit-heading"
  );
  context.assertNotMatch(mockup, /https?:\/\/|\/\/cdn|src="http|href="http/i, "frontend-mockups:no-external-assets");
  context.assertNotMatch(mockup, /\/auth\/token|\/ws\/events|\/containers|(?<!\/qos)\/storage|\/ovn|\/networks/i, "frontend-mockups:no-runtime-routes");
  context.assertNotMatch(mockup, /journalctl|libvirt|KVM|ZFS|OVS|\bOVN\b|purecvisorsd|Create Linux VM|ubuntu-24\.04/i, "frontend-mockups:no-linux-literals");
  context.assertNotMatch(mockup, /Bearer\s+(?!\$\{)[A-Za-z0-9._~+/=-]{24,}/i, "frontend-mockups:no-bearer-literal");
  context.assertNotMatch(mockup, /Restart-Computer|msiexec|New-NetFirewallRule|Register-EventSource|New-EventLog|Register-ScheduledTask|New-VM|Remove-VM/i, "frontend-mockups:no-host-mutation");
}

function verifyFrontendBatches(context) {
  const planPath = "docs/superpowers/plans/2026-05-09-purecvisor-desktop-node-frontend-completion-auto-batches.json";
  const docPath = "docs/superpowers/plans/2026-05-09-purecvisor-desktop-node-frontend-completion-auto-batches.md";
  const validatorPath = "web/scripts/validate-frontend-completion-batches.mjs";
  context.assertExists(planPath, "frontend-batches:plan");
  context.assertExists(docPath, "frontend-batches:doc");
  context.assertExists(validatorPath, "frontend-batches:validator");

  const planText = context.readText(planPath);
  const plan = context.readJson(planPath);
  assertObject(context, plan, "frontend-batches:plan-object");
  const doc = context.readText(docPath);
  const packageJson = context.readJson("web/package.json");
  context.assertEqual(plan.schema_version, 1, "frontend-batches:schema-version");
  context.assertEqual(plan.plan_id, "purecvisor-desktop-node-frontend-completion-auto-batches-2026-05-09", "frontend-batches:plan-id");
  context.assertEqual(plan.batch_count, 5, "frontend-batches:batch-count");
  context.assertEqual(plan.work_item_count, 25, "frontend-batches:item-count");
  context.assertEqual(plan.host_mutation_performed, false, "frontend-batches:no-host-mutation-performed");
  context.assertEqual(plan.host_mutation_required, false, "frontend-batches:no-host-mutation-required");
  context.assertEqual(plan.linux_runtime_excluded, true, "frontend-batches:linux-excluded");
  context.assertEqual(plan.single_ui_clone_required, true, "frontend-batches:single-clone");
  assertArray(context, plan.batches, "frontend-batches:batches-array");
  context.assertEqual(plan.batches.length, 5, "frontend-batches:batches-count");

  const items = plan.batches.flatMap((batch, batchIndex) => {
    const batchLabel = `frontend-batches:batch-${batchIndex}`;
    assertObject(context, batch, `${batchLabel}:object`);
    assertNonemptyArray(context, batch.write_scope, "frontend-batches:write-scope");
    assertNonemptyArray(context, batch.verification_commands, "frontend-batches:verification-commands");
    assertArray(context, batch.work_items, "frontend-batches:work-items-array");
    context.assertEqual(batch.work_items.length, 5, "frontend-batches:batch-item-count");
    for (const [itemIndex, item] of batch.work_items.entries()) {
      assertObject(context, item, `${batchLabel}:item-${itemIndex}:object`);
      context.assertEqual(item.automatable, true, "frontend-batches:item-automatable");
      assertNonemptyArray(context, item.target_files, "frontend-batches:item-target-files");
      assertNonemptyArray(context, item.acceptance, "frontend-batches:item-acceptance");
      assertNonemptyArray(context, item.verification, "frontend-batches:item-verification");
    }
    return batch.work_items;
  });
  context.assertEqual(items.length, 25, "frontend-batches:flattened-item-count");
  assertExactUniqueMembers(
    context,
    items.map((item) => item.id),
    Array.from({ length: 25 }, (_, index) => index + 1),
    "frontend-batches:item-ids"
  );

  for (const command of [
    "Invoke-Pester -Path web/tests/PcvDesktopWeb.Static.Tests.ps1 -Output Detailed",
    "npm test --prefix web",
    "npm run verify:parity --prefix web",
    "node --check web/app.js",
    "git diff --check"
  ]) {
    context.assertIncludes(plan.final_verification_commands, command, `frontend-batches:final-command:${command}`);
  }
  context.assertEqual(
    packageJson.scripts?.["check:frontend-batches"],
    "node scripts/validate-frontend-completion-batches.mjs",
    "frontend-batches:package-script"
  );
  context.assertMatch(packageJson.scripts?.test, /check:frontend-batches/i, "frontend-batches:test-wiring");
  context.assertMatch(doc, /Batch 1: Shell And Session/i, "frontend-batches:doc-batch-1");
  context.assertMatch(doc, /Batch 5: Parity Visual A11y Evidence/i, "frontend-batches:doc-batch-5");
  context.assertNotMatch(planText + doc, /Bearer\s+(?!\$\{)[A-Za-z0-9._~+/=-]{24,}/i, "frontend-batches:no-bearer-literal");
  context.assertNotMatch(planText, /Restart-Computer|msiexec|New-NetFirewallRule|Register-EventSource|New-EventLog|Register-ScheduledTask|New-VM|Remove-VM/i, "frontend-batches:no-host-mutation");
}

function verifyPhase2hEndpoints(context) {
  const app = context.readText("web/app.js");
  for (const [pattern, label] of [
    [/\/api\/v1\/host\/status/i, "host-status"],
    [/\/api\/v1\/vms/i, "vms"],
    [/\/api\/v1\/jobs\//i, "jobs"],
    [/jobAction:.*['"]cancel['"]/i, "cancel-action"],
    [/jobAction:.*['"]retry['"]/i, "retry-action"],
    [/jobAction:.*['"]reconcile['"]/i, "reconcile-action"]
  ]) {
    context.assertMatch(app, pattern, `phase2h-endpoints:${label}`);
  }
}

function assertSourceAndGenerated(context, source, generated, checks, prefix) {
  for (const [pattern, label] of checks) {
    context.assertMatch(source, pattern, `${prefix}:${label}:source`);
    context.assertMatch(generated, pattern, `${prefix}:${label}:generated`);
  }
}

function verifyLocalApiRegistry(context) {
  const app = context.readText("web/app.js");
  const routesSource = context.readText("web/src/served/routes.ts");
  const apiClientSource = context.readText("web/src/served/api-client.ts");
  const servedSource = context.readServedSource();
  context.assertMatch(routesSource, /const\s+DESKTOP_NODE_ROUTE_COVERAGE/i, "local-api-registry:route-coverage-source");
  assertSourceAndGenerated(context, routesSource, app, [
    [/const\s+DESKTOP_NODE_API_ROUTES/i, "api-routes"],
    [/DESKTOP_NODE_ROUTE_COVERAGE/i, "route-coverage"],
    [/hostStatus:\s*['"]\/api\/v1\/host\/status/i, "host-status"],
    [/runtimePolicy:\s*['"]\/api\/v1\/runtime\/policy/i, "runtime-policy"],
    [/opsSummary:\s*['"]\/api\/v1\/ops\/summary/i, "ops-summary"],
    [/networkInventory:\s*['"]\/api\/v1\/network\/inventory/i, "network-inventory"],
    [/jobsPage/i, "jobs-page"],
    [/vmAction/i, "vm-action"],
    [/checkpointAction/i, "checkpoint-action"],
    [/job\.reconcile/i, "job-reconcile"]
  ], "local-api-registry");
  assertSourceAndGenerated(context, apiClientSource, app, [
    [/const\s+desktopApi/i, "desktop-api"],
    [/function\s+unwrapApiEnvelope\s*\(/i, "unwrap-envelope"],
    [/function\s+unwrapApiList\s*\(/i, "unwrap-list"]
  ], "local-api-registry");
  context.assertNotMatch(servedSource, /apiFetch\(\s*['"`]\/api\/v1/i, "local-api-registry:no-direct-api-fetch");
  context.assertNotMatch(servedSource, /\/auth\/token|\/ws\/events|\/containers|(?<!\/qos)\/storage|\/ovn|\/networks/i, "local-api-registry:no-forbidden-routes");
  context.assertNotMatch(app, /\/auth\/token|\/ws\/events|\/containers|(?<!\/qos)\/storage|\/ovn|\/networks/i, "local-api-registry:no-forbidden-routes-generated");
}

function verifyQosGuestReadback(context) {
  const app = context.readText("web/app.js");
  const routesSource = context.readText("web/src/served/routes.ts");
  const renderSource = context.readText("web/src/served/render-qos.ts");
  const loadSource = context.readText("web/src/served/load.ts");
  const servedSource = context.readServedSource();
  context.assertMatch(app, /function\s+renderVmQosGuestReadback\s*\(/i, "qos-guest-readback:render");
  assertSourceAndGenerated(context, routesSource, app, [
    [/vmBlkio\s*:/i, "vm-blkio"],
    [/vmBandwidth/i, "vm-bandwidth"],
    [/vmGuestAgentStatus/i, "guest-agent-status"],
    [/vmGuestAgentPing/i, "guest-agent-ping"],
    [/\/api\/v1\/vms\/\$\{encodeRouteSegment\(vmId\)\}\/blkio/i, "blkio-route"],
    [/\/api\/v1\/vms\/\$\{encodeRouteSegment\(vmId\)\}\/bandwidth/i, "bandwidth-route"],
    [/\/api\/v1\/vms\/\$\{encodeRouteSegment\(vmId\)\}\/guest-agent\/status/i, "guest-status-route"],
    [/\/api\/v1\/vms\/\$\{encodeRouteSegment\(vmId\)\}\/guest-agent\/ping/i, "guest-ping-route"]
  ], "qos-guest-readback");
  assertSourceAndGenerated(context, renderSource, app, [
    [/renderVmQosGuestReadback/i, "render-combined"],
    [/data-action="vm-qos-guest-refresh"/i, "refresh-action"],
    [/linux_blkio_compatible/i, "blkio-compatible"],
    [/linux_bandwidth_compatible/i, "bandwidth-compatible"],
    [/qemu_guest_agent/i, "qemu-agent"],
    [/guest_heartbeat_verified/i, "heartbeat"]
  ], "qos-guest-readback");
  assertSourceAndGenerated(context, loadSource, app, [
    [/function\s+loadVmQosGuestReadbacks\s*\(/i, "load"]
  ], "qos-guest-readback");
  context.assertNotMatch(servedSource, /blkio-set/i, "qos-guest-readback:no-blkio-set-source");
  context.assertNotMatch(app, /blkio-set/i, "qos-guest-readback:no-blkio-set-generated");
}

function verifyQosGuestControl(context) {
  const app = context.readText("web/app.js");
  const routesSource = context.readText("web/src/served/routes.ts");
  const renderSource = context.readText("web/src/served/render-qos.ts");
  const servedSource = context.readServedSource();
  const design = context.readText("web/DESIGN.md");
  context.assertMatch(app, /vmQosStoragePreview:\s*\(vmId\)/i, "qos-guest-control:storage-preview-route");
  context.assertMatch(routesSource, /vmQosStoragePreview:\s*\(vmId/i, "qos-guest-control:storage-preview-route-source");
  assertSourceAndGenerated(context, routesSource, app, [
    [/vmQosStorage\s*:/i, "storage"],
    [/vmQosNetworkPreview\s*:/i, "network-preview"],
    [/vmQosNetwork\s*:/i, "network"],
    [/\/api\/v1\/vms\/\$\{encodeRouteSegment\(vmId\)\}\/qos\/storage\/preview/i, "storage-preview-path"],
    [/\/api\/v1\/vms\/\$\{encodeRouteSegment\(vmId\)\}\/qos\/storage/i, "storage-path"],
    [/\/api\/v1\/vms\/\$\{encodeRouteSegment\(vmId\)\}\/qos\/network\/preview/i, "network-preview-path"],
    [/\/api\/v1\/vms\/\$\{encodeRouteSegment\(vmId\)\}\/qos\/network/i, "network-path"],
    [/vmGuestExec\s*:/i, "guest-exec"],
    [/vmGuestChannelVerify\s*:/i, "channel-verify"],
    [/vmGuestChannelEnsure\s*:/i, "channel-ensure"],
    [/\/api\/v1\/vms\/\$\{encodeRouteSegment\(vmId\)\}\/guest\/exec/i, "guest-exec-path"],
    [/\/api\/v1\/vms\/\$\{encodeRouteSegment\(vmId\)\}\/guest\/channel\/verify/i, "channel-verify-path"],
    [/\/api\/v1\/vms\/\$\{encodeRouteSegment\(vmId\)\}\/guest\/channel/i, "channel-path"]
  ], "qos-guest-control");
  assertSourceAndGenerated(context, renderSource, app, [
    [/data-action="vm-qos-storage-preview"/i, "storage-preview-action"],
    [/data-action="vm-qos-storage-apply"/i, "storage-apply-action"],
    [/data-action="vm-qos-network-preview"/i, "network-preview-action"],
    [/data-action="vm-qos-network-apply"/i, "network-apply-action"],
    [/data-action="vm-guest-exec"/i, "guest-exec-action"],
    [/data-action="guest-agent-ensure-channel"/i, "channel-action"],
    [/guest\.exec/i, "guest-exec-rbac"],
    [/guest\.channel\.configure/i, "channel-rbac"]
  ], "qos-guest-control");
  context.assertMatch(design, /ADR-0009/i, "qos-guest-control:adr-0009:design");
  context.assertMatch(design, /ADR-0010/i, "qos-guest-control:adr-0010:design");
  context.assertNotMatch(servedSource, /data-action="novnc-target-apply"/i, "qos-guest-control:no-novnc-control-source");
  context.assertNotMatch(app, /data-action="novnc-target-apply"/i, "qos-guest-control:no-novnc-control-generated");
  context.assertNotMatch(design, /data-action="novnc-target-apply"/i, "qos-guest-control:no-novnc-control-design");
}

function verifyGuestExecCancel(context) {
  const app = context.readText("web/app.js");
  const renderJobsSource = context.readText("web/src/served/render-jobs.ts");
  const jobPollingSource = context.readText("web/src/served/job-polling.ts");
  context.assertMatch(app, /Cancel running guest exec/i, "guest-exec-cancel:label");
  context.assertMatch(renderJobsSource, /Cancel running guest exec/i, "guest-exec-cancel:label-source");
  assertSourceAndGenerated(context, renderJobsSource, app, [
    [/function\s+formatJobCancelLabel\s*\(/i, "format-label"],
    [/function\s+getJobCancelScope\s*\(/i, "cancel-scope"],
    [/data-job-cancel-scope="\$\{escapeHtml\(cancelScope\)\}"/i, "scope-attribute"],
    [/running-guest-execution/i, "running-scope"]
  ], "guest-exec-cancel");
  assertSourceAndGenerated(context, jobPollingSource, app, [
    [/requireRbac\(['"]operate['"],\s*['"]running guest execution cancel['"]\)/i, "rbac"]
  ], "guest-exec-cancel");
}

function verifySearchEventTable(context) {
  const index = context.readText("web/index.html");
  const styles = context.readText("web/styles.css");
  const app = context.readText("web/app.js");
  const servedSource = context.readServedSource();
  const shellSource = context.readText("web/src/served/render-shell.ts");
  const activitySource = context.readText("web/src/served/render-activity.ts");
  const tableSource = context.readText("web/src/served/table.ts");
  for (const [pattern, label] of [
    [/id="global-search-input"/i, "global-search"],
    [/id="command-palette"/i, "command-palette"],
    [/id="command-palette-input"/i, "command-palette-input"],
    [/id="command-palette-results"/i, "command-palette-results"],
    [/id="event-center-panel"/i, "event-center"],
    [/id="job-filter"/i, "job-filter"],
    [/id="network-filter"/i, "network-filter"]
  ]) {
    context.assertMatch(index, pattern, `search-event-table:${label}`);
  }
  assertSourceAndGenerated(context, shellSource, app, [
    [/function\s+buildCommandPaletteItems\s*\(/i, "build-command-palette"],
    [/function\s+renderCommandPalette\s*\(/i, "render-command-palette"],
    [/function\s+handleCommandSearch\s*\(/i, "handle-command-search"]
  ], "search-event-table");
  assertSourceAndGenerated(context, activitySource, app, [
    [/function\s+buildEventCenterItems\s*\(/i, "build-event-center"],
    [/function\s+renderEventCenter\s*\(/i, "render-event-center"]
  ], "search-event-table");
  assertSourceAndGenerated(context, tableSource, app, [
    [/filterRowsByQuery/i, "filter-rows"],
    [/sortRowsByKey/i, "sort-rows"],
    [/function\s+renderTableStateSummary\s*\(/i, "table-summary"]
  ], "search-event-table");
  context.assertMatch(index, /data-command-palette/i, "search-event-table:command-palette-data:index");
  context.assertMatch(activitySource, /event-severity-lane/i, "search-event-table:severity-lane:source");
  context.assertMatch(styles, /event-severity-lane/i, "search-event-table:severity-lane:styles");
  context.assertMatch(app, /event-severity-lane/i, "search-event-table:severity-lane:generated");
  for (const [source, label] of [[index, "index"], [styles, "styles"], [servedSource, "source"], [app, "generated"]]) {
    context.assertNotMatch(source, /\/auth\/token|\/ws\/events|\/containers|(?<!\/qos)\/storage|\/ovn|\/networks/i, `search-event-table:no-forbidden-routes:${label}`);
    context.assertNotMatch(source, /journalctl|libvirt|KVM|ZFS|OVS|\bOVN\b|purecvisorsd|Create Linux VM|ubuntu-24\.04/i, `search-event-table:no-linux-runtime:${label}`);
  }
}

function verifyServedSourceParts(context) {
  const buildScript = context.readText("web/scripts/build-served-asset.mjs");
  const expectedParts = ["types.ts", "state.ts", "routes.ts", "errors.ts", "api-client.ts"];
  for (const part of expectedParts) {
    context.assertExists(`web/src/served/${part}`, `served-source-parts:file-${part}`);
    context.assertMatch(
      buildScript,
      new RegExp(`['\"]src/served/${part.replace(".", "\\.")}['\"]`, "i"),
      `served-source-parts:part-${part}`
    );
  }
  context.readServedSource();
  context.assertMatch(buildScript, /servedSourceParts/i, "served-source-parts:registry");
  context.assertMatch(context.readText("web/app.js"), /Generated from staged src\/served/i, "served-source-parts:generated-marker");
}

function verifyOptionalBearer(context) {
  const app = context.readText("web/app.js");
  context.assertMatch(app, /headers\.set\(['"]Authorization['"]/i, "optional-bearer:authorization");
  context.assertMatch(app, /Bearer/i, "optional-bearer:bearer");
  context.assertMatch(app, /apiToken/i, "optional-bearer:api-token");
  context.assertNotMatch(app, /Bearer\s+(?!\$\{)[A-Za-z0-9._~+/=-]{24,}/i, "optional-bearer:no-bearer-literal");
}

function verifyAccountRbacConsole(context) {
  const index = context.readText("web/index.html");
  const app = context.readText("web/app.js");
  const routesSource = context.readText("web/src/served/routes.ts");
  const actionsSource = context.readText("web/src/served/actions.ts");
  const stateSource = context.readText("web/src/served/state.ts");
  const rbacSource = context.readText("web/src/served/rbac.ts");
  const panelsSource = context.readText("web/src/served/render-panels.ts");
  const consoleSource = context.readText("web/src/served/render-console.ts");
  const servedSource = context.readServedSource();
  for (const [pattern, label] of [
    [/id="account-login-form"/i, "login-form"],
    [/id="account-console-panel"/i, "console-panel"],
    [/name="username"/i, "username"],
    [/name="password"/i, "password"]
  ]) {
    context.assertMatch(index, pattern, `account-rbac-console:${label}`);
  }
  assertSourceAndGenerated(context, routesSource, app, [
    [/\/api\/v1\/auth\/login/i, "login-route"],
    [/\/api\/v1\/auth\/loopback-session/i, "loopback-session-route"],
    [/\/api\/v1\/auth\/refresh/i, "refresh-route"],
    [/\/api\/v1\/auth\/logout/i, "logout-route"],
    [/\/api\/v1\/auth\/session/i, "session-route"],
    [/\/api\/v1\/auth\/rbac/i, "rbac-route"],
    [/\/api\/v1\/console\/capabilities/i, "console-capabilities"],
    [/\/api\/v1\/vms\/\$\{encodeRouteSegment\(vmId\)\}\/console/i, "vm-console-route"],
    [/id:\s*['"]auth\.logout['"]/i, "logout-action"]
  ], "account-rbac-console");
  assertSourceAndGenerated(context, actionsSource, app, [
    [/function\s+ensureLoopbackSession\s*\(/i, "ensure-loopback-session"],
    [/function\s+refreshAccountSession\s*\(/i, "refresh-session"]
  ], "account-rbac-console");
  assertSourceAndGenerated(context, stateSource, app, [
    [/authAccessToken:\s*['"]/i, "access-token-state"],
    [/authRefreshToken:\s*['"]/i, "refresh-token-state"]
  ], "account-rbac-console");
  assertSourceAndGenerated(context, rbacSource, app, [
    [/function\s+rbacAllows\s*\(/i, "rbac-allows"]
  ], "account-rbac-console");
  assertSourceAndGenerated(context, panelsSource, app, [
    [/function\s+renderAccountSession\s*\(/i, "render-session"]
  ], "account-rbac-console");
  assertSourceAndGenerated(context, consoleSource, app, [
    [/function\s+renderConsolePanel\s*\(/i, "render-console"],
    [/noVNC/i, "novnc"],
    [/vmconnect/i, "vmconnect"]
  ], "account-rbac-console");
  context.assertNotMatch(index, /access_token\s*[:=]/i, "account-rbac-console:no-literal-access-token");
  context.assertNotMatch(servedSource, /\/auth\/token|\/ws\/events/i, "account-rbac-console:no-forbidden-auth-routes:source");
  context.assertNotMatch(app, /\/auth\/token|\/ws\/events/i, "account-rbac-console:no-forbidden-auth-routes:generated");
}

function verifyListenerApiBase(context) {
  const index = context.readText("web/index.html");
  const app = context.readText("web/app.js");
  const servedState = context.readText("web/src/served/state.ts");
  context.assertMatch(index, /\/pcv-config\.js/i, "listener-api-base:config-script");
  context.assertBefore(index, "/pcv-config.js", "/app.js", "listener-api-base:script-order");
  assertSourceAndGenerated(context, servedState, app, [
    [/PCV_DESKTOP_NODE_CONFIG/i, "config-object"],
    [/apiBaseUrl/i, "api-base-url"],
    [/window\.location\.origin/i, "origin-fallback"]
  ], "listener-api-base");
}

function verifyVmCreatePayload(context) {
  const app = context.readText("web/app.js");
  const source = context.readText("web/src/served/job-polling.ts");
  assertSourceAndGenerated(context, source, app, [
    [/function\s+readCreatePayload\s*\(/i, "payload-reader"],
    [/iso_path:\s*String\(data\.get\('iso_path'\)/i, "iso-path"],
    [/memory_mb:\s*Number\(data\.get\('memory_mb'\)\)/i, "memory-mb"],
    [/disk_gb:\s*Number\(data\.get\('disk_gb'\)\)/i, "disk-gb"],
    [/vm_root:\s*String\(data\.get\('vm_root'\)/i, "vm-root"],
    [/generation:\s*Number\(data\.get\('generation'\)\)/i, "generation"]
  ], "vm-create-payload");
}

function verifyVmLifecycleRoutes(context) {
  const app = context.readText("web/app.js");
  const routes = context.readText("web/src/served/routes.ts");
  assertSourceAndGenerated(context, routes, app, [
    [/\/api\/v1\/vms\//i, "vm-route"],
    [/vmAction:\s*\(vmId/i, "vm-action"],
    [/'start'/i, "start"],
    [/'shutdown'/i, "shutdown"],
    [/'poweroff'/i, "poweroff"],
    [/'restart'/i, "restart"],
    [/\/api\/v1\/vms\/\{vm_id\}\/attach/i, "attach"],
    [/\/api\/v1\/vms\/\{vm_id\}\/save/i, "save"],
    [/\/api\/v1\/vms\/\{vm_id\}\/resume-saved/i, "resume-saved"],
    [/\/api\/v1\/vms\/\{vm_id\}\/manage/i, "manage"]
  ], "vm-lifecycle-routes");
}

function verifyVmDetailMount(context) {
  const index = context.readText("web/index.html");
  const app = context.readText("web/app.js");
  const stateSource = context.readText("web/src/served/state.ts");
  const servedAppSource = context.readText("web/src/served-app.ts");
  const inventorySource = context.readText("web/src/served/render-inventory.ts");
  for (const [pattern, label] of [
    [/id="vm-detail-panel"/i, "panel"],
    [/id="vm-detail-content"/i, "content"],
    [/id="vm-state-filter"/i, "state-filter"],
    [/id="vm-sort"/i, "sort"]
  ]) {
    context.assertMatch(index, pattern, `vm-detail-mount:${label}`);
  }
  assertSourceAndGenerated(context, stateSource, app, [
    [/vmStateFilter:\s*'all'/i, "state-filter"],
    [/vmSort:\s*'name'/i, "sort"]
  ], "vm-detail-mount");
  assertSourceAndGenerated(context, servedAppSource, app, [
    [/els\.vmStateFilter\?\.addEventListener/i, "state-filter-binding"],
    [/els\.vmSort\?\.addEventListener/i, "sort-binding"]
  ], "vm-detail-mount");
  assertSourceAndGenerated(context, inventorySource, app, [
    [/function\s+compareVms\s*\(/i, "compare"]
  ], "vm-detail-mount");
}

function verifyVmLifecycleActions(context) {
  const app = context.readText("web/app.js");
  const detailSource = context.readText("web/src/served/render-vm-detail.ts");
  const mutateSource = context.readText("web/src/served/mutate.ts");
  const errorsSource = context.readText("web/src/served/errors.ts");
  const clientSource = context.readText("web/src/served/api-client.ts");
  assertSourceAndGenerated(context, detailSource, app, [
    [/data-action="vm-start"/i, "start"],
    [/data-action="vm-shutdown"/i, "shutdown"],
    [/data-action="vm-poweroff"/i, "poweroff"],
    [/data-action="vm-restart"/i, "restart"],
    [/data-action="vm-save"/i, "save"],
    [/data-action="vm-resume-saved"/i, "resume-saved"],
    [/Resume saved/i, "resume-saved-label"],
    [/data-action="vm-manage"/i, "manage"],
    [/Manage VM/i, "manage-label"],
    [/data-action="vm-delete"/i, "delete"]
  ], "vm-lifecycle-actions");
  assertSourceAndGenerated(context, mutateSource, app, [
    [/function\s+queueVmManage\s*\(/i, "queue-manage"],
    [/PCV_VM_DELETE_RUNNING_BLOCKED/i, "running-delete-guard"],
    [/window\.confirm\(/i, "destructive-confirmation"]
  ], "vm-lifecycle-actions");
  assertSourceAndGenerated(context, errorsSource, app, [
    [/PCV_VM_NOT_MANAGED_BY_PURECVISOR/i, "unmanaged-delete-guard"],
    [/Unmanaged delete refusal remains/i, "unmanaged-delete-refusal"]
  ], "vm-lifecycle-actions");
  assertSourceAndGenerated(context, clientSource, app, [
    [/queueVmManage:\s*\(vmId/i, "manage-client"],
    [/confirm_name:\s*confirmName/i, "confirm-name"]
  ], "vm-lifecycle-actions");
}

function verifyCheckpointActions(context) {
  const app = context.readText("web/app.js");
  const routesSource = context.readText("web/src/served/routes.ts");
  const detailSource = context.readText("web/src/served/render-vm-detail.ts");
  const qosSource = context.readText("web/src/served/render-qos.ts");
  const servedAppSource = context.readText("web/src/served-app.ts");
  const loadSource = context.readText("web/src/served/load.ts");
  assertSourceAndGenerated(context, routesSource, app, [
    [/\/checkpoints/i, "route"],
  ], "checkpoint-actions");
  assertSourceAndGenerated(context, detailSource, app, [
    [/checkpoint-create/i, "create"],
  ], "checkpoint-actions");
  assertSourceAndGenerated(context, qosSource, app, [
    [/data-action="checkpoint-restore"/i, "restore"],
    [/checkpoint-delete/i, "delete"]
  ], "checkpoint-actions");
  assertSourceAndGenerated(context, servedAppSource, app, [
    [/button\.dataset\.action === 'checkpoint-restore'/i, "restore-binding"],
    [/button\.dataset\.action === 'checkpoint-delete'/i, "delete-binding"]
  ], "checkpoint-actions");
  assertSourceAndGenerated(context, loadSource, app, [
    [/async\s+function\s+loadCheckpoints\s*\(/i, "load"]
  ], "checkpoint-actions");
}

function verifyBrowserJobHistory(context) {
  const index = context.readText("web/index.html");
  const app = context.readText("web/app.js");
  const stateSource = context.readText("web/src/served/state.ts");
  const pollingSource = context.readText("web/src/served/job-polling.ts");
  const servedAppSource = context.readText("web/src/served-app.ts");
  context.assertMatch(index, /id="clear-job-history"/i, "browser-job-history:clear-control");
  assertSourceAndGenerated(context, stateSource, app, [
    [/localStorage/i, "local-storage"],
    [/pcvDesktopTrackedJobs\.v1/i, "storage-key"],
    [/JOB_HISTORY_LIMIT\s*=\s*50/i, "limit"],
    [/loadTrackedJobsFromStorage/i, "load"],
    [/saveTrackedJobsToStorage/i, "save"],
    [/clearTrackedJobHistory/i, "clear"],
    [/function\s+loadTrackedJobsFromStorage\s*\([^)]*\)(?:\s*:\s*any\[\])?\s*\{(?:(?!function\s).)*?\.slice\(0, JOB_HISTORY_LIMIT\);/is, "load-retention"],
    [/function\s+saveTrackedJobsToStorage\s*\([^)]*\)(?:\s*:\s*void)?\s*\{(?:(?!function\s).)*?const jobs = state\.trackedJobs\.slice\(0, JOB_HISTORY_LIMIT\);/is, "save-retention"]
  ], "browser-job-history");
  assertSourceAndGenerated(context, pollingSource, app, [
    [/function\s+trackJob\s*\(/i, "track"],
    [/state\.trackedJobs = state\.trackedJobs\.slice\(0, JOB_HISTORY_LIMIT\)/i, "track-retention"]
  ], "browser-job-history");
  assertSourceAndGenerated(context, servedAppSource, app, [
    [/els\.clearJobHistory\.addEventListener\('click', clearTrackedJobHistory\)/i, "clear-binding"],
    [/state\.trackedJobs = loadTrackedJobsFromStorage\(\)/i, "load-binding"]
  ], "browser-job-history");
}

function verifyJobOrchestration(context) {
  const app = context.readText("web/app.js");
  const stateSource = context.readText("web/src/served/state.ts");
  const tableSource = context.readText("web/src/served/table.ts");
  const pollingSource = context.readText("web/src/served/job-polling.ts");
  const routesSource = context.readText("web/src/served/routes.ts");
  const clientSource = context.readText("web/src/served/api-client.ts");
  const jobsSource = context.readText("web/src/served/render-jobs.ts");
  const activitySource = context.readText("web/src/served/render-activity.ts");
  const servedAppSource = context.readText("web/src/served-app.ts");
  assertSourceAndGenerated(context, stateSource, app, [
    [/pendingVmActions/i, "pending-vm-actions"],
    [/pendingCheckpoints/i, "pending-checkpoints"],
    [/jobPollDelayMs:\s*2000/i, "poll-delay"]
  ], "job-orchestration");
  assertSourceAndGenerated(context, tableSource, app, [
    [/isVmActionPending/i, "vm-action-pending"],
    [/isCheckpointActionPending/i, "checkpoint-action-pending"]
  ], "job-orchestration");
  assertSourceAndGenerated(context, pollingSource, app, [
    [/scheduleNextPoll/i, "schedule-next-poll"],
    [/state\.jobPollDelayMs = 2000/i, "poll-delay-reset"],
    [/Math\.min\(Math\.round\(state\.jobPollDelayMs \* 1\.5\), 15000\)/i, "poll-backoff"],
    [/window\.setTimeout/i, "poll-timeout"],
    [/loadNextJobPage/i, "load-next-page"],
    [/next_offset/i, "next-offset"],
    [/async\s+function\s+cancelJob\s*\(/i, "cancel"],
    [/async\s+function\s+retryJob\s*\(/i, "retry"],
    [/async\s+function\s+reconcileJob\s*\(/i, "reconcile"],
    [/desktopApi\.cancelJob\(jobId\)/i, "cancel-call"],
    [/desktopApi\.retryJob\(jobId\)/i, "retry-call"],
    [/desktopApi\.reconcileJob\(jobId\)/i, "reconcile-call"]
  ], "job-orchestration");
  assertSourceAndGenerated(context, routesSource, app, [
    [/jobAction:\s*\(jobId/i, "job-action-route"],
    [/'cancel', 'retry', 'reconcile'/i, "job-action-members"]
  ], "job-orchestration");
  assertSourceAndGenerated(context, clientSource, app, [
    [/cancelJob:\s*\(jobId/i, "cancel-client"],
    [/retryJob:\s*\(jobId/i, "retry-client"],
    [/reconcileJob:\s*\(jobId/i, "reconcile-client"]
  ], "job-orchestration");
  assertSourceAndGenerated(context, jobsSource, app, [
    [/data-action="cancel-job"/i, "cancel-control"],
    [/data-action="retry-job"/i, "retry-control"],
    [/data-action="reconcile-job"/i, "reconcile-control"]
  ], "job-orchestration");
  assertSourceAndGenerated(context, activitySource, app, [
    [/next_offset/i, "render-next-offset"],
    [/data-action="load-next-jobs"/i, "load-next-jobs"]
  ], "job-orchestration");
  assertSourceAndGenerated(context, servedAppSource, app, [
    [/if\s*\(button\.dataset\.action === 'cancel-job'\)\s*await cancelJob\(button\.dataset\.jobId, button\.dataset\.jobCancelScope\)/i, "cancel-binding"],
    [/button\.dataset\.action === 'retry-job'/i, "retry-binding"],
    [/button\.dataset\.action === 'reconcile-job'/i, "reconcile-binding"],
    [/button\.dataset\.action === 'load-next-jobs'/i, "load-next-binding"]
  ], "job-orchestration");
}

function verifyShellControls(context) {
  const index = context.readText("web/index.html");
  const app = context.readText("web/app.js");
  const shellSource = context.readText("web/src/served/render-shell.ts");
  const actionsSource = context.readText("web/src/served/actions.ts");
  const loadSource = context.readText("web/src/served/load.ts");
  const servedAppSource = context.readText("web/src/served-app.ts");
  const servedSource = context.readServedSource();
  for (const [pattern, label] of [
    [/data-menu-command="refresh"/i, "refresh-command"],
    [/data-menu-command="clear-browser-state"/i, "clear-browser-state-command"],
    [/data-view-link="vms"/i, "vms-view"],
    [/data-view-link="network"/i, "network-view"],
    [/id="vm-asset-list"/i, "vm-asset-list"],
    [/id="workspace-tabbar"/i, "workspace-tabbar"],
    [/data-shell-action="open-create-vm"/i, "open-create-vm"],
    [/id="live-viewer-state"/i, "live-viewer-state"],
    [/id="logout-boundary"/i, "logout-boundary"]
  ]) {
    context.assertMatch(index, pattern, `shell-controls:${label}`);
  }
  assertSourceAndGenerated(context, shellSource, app, [
    [/function\s+renderVmAssetList\s*\(/i, "render-vm-asset-list"],
    [/function\s+renderWorkspaceTabs\s*\(/i, "render-workspace-tabs"]
  ], "shell-controls");
  assertSourceAndGenerated(context, actionsSource, app, [
    [/handleShellCommand/i, "handle-command"],
    [/clearBrowserState/i, "clear-browser-state"]
  ], "shell-controls");
  assertSourceAndGenerated(context, loadSource, app, [
    [/selectVmFromShell/i, "select-vm"]
  ], "shell-controls");
  assertSourceAndGenerated(context, servedAppSource, app, [
    [/closest\('\[data-view-link\]'\)/i, "view-link-binding"]
  ], "shell-controls");
  for (const [source, label] of [[index, "index"], [servedSource, "source"], [app, "generated"]]) {
    context.assertNotMatch(source, /\/auth\/token|\/ws\/events/i, `shell-controls:no-forbidden-auth-routes:${label}`);
  }
}

function verifyActivityTroubleshooting(context) {
  const index = context.readText("web/index.html");
  const app = context.readText("web/app.js");
  const servedSource = context.readText("web/src/served-app.ts");
  const combined = app + servedSource;
  for (const [pattern, label] of [
    [/id="activity"/i, "activity-view"],
    [/id="activity-panel"/i, "activity-panel"],
    [/id="troubleshooting"/i, "troubleshooting-view"],
    [/id="troubleshooting-panel"/i, "troubleshooting-panel"]
  ]) {
    context.assertMatch(index, pattern, `activity-troubleshooting:${label}`);
  }
  for (const [pattern, label] of [
    [/\/api\/v1\/runtime\/policy/i, "runtime-policy-route"],
    [/\/api\/v1\/jobs/i, "jobs-route"],
    [/loadRuntimePolicy/i, "load-runtime-policy"],
    [/loadServerJobs/i, "load-server-jobs"],
    [/renderActivity/i, "render-activity"],
    [/formatCorrelationValue/i, "format-correlation"],
    [/request_id/i, "request-id"],
    [/correlation_id/i, "correlation-id"],
    [/renderTroubleshooting/i, "render-troubleshooting"],
    [/PCV_JOB_STORE_SCHEMA_UNSUPPORTED/i, "job-store-schema-error"],
    [/PCV_VM_NOT_MANAGED_BY_PURECVISOR/i, "unmanaged-vm-error"]
  ]) {
    context.assertMatch(app, pattern, `activity-troubleshooting:${label}`);
  }
  for (const [pattern, label] of [
    [/jobsPage/i, "jobs-page"],
    [/desktopApi\.listJobs\(50,\s*0(?:,\s*options)?\)/i, "list-jobs-page"],
    [/pagination|next_offset|retention/i, "pagination-retention"]
  ]) {
    context.assertMatch(combined, pattern, `activity-troubleshooting:${label}`);
  }
}

function verifyOpsCockpit(context) {
  const index = context.readText("web/index.html");
  const app = context.readText("web/app.js");
  const servedSource = context.readText("web/src/served-app.ts");
  const apiTypes = context.readText("web/src/api-types.ts");
  const appSource = context.readText("web/src/app.ts");
  const combined = app + servedSource;
  for (const [pattern, label] of [
    [/id="view-dashboard"/i, "dashboard-view"],
    [/id="view-vms"/i, "vms-view"],
    [/id="view-troubleshooting"/i, "troubleshooting-view"],
    [/id="ops-summary-panel"/i, "ops-summary-panel"],
    [/id="priority-panel"/i, "priority-panel"],
    [/id="dashboard-activity-panel"/i, "dashboard-activity-panel"],
    [/id="vm-workbench-context"/i, "vm-workbench-context"],
    [/id="incident-panel"/i, "incident-panel"],
    [/data-view-link="dashboard"/i, "dashboard-link"],
    [/data-view-link="vms"/i, "vms-link"],
    [/data-view-link="jobs"/i, "jobs-link"],
    [/data-view-link="activity"/i, "activity-link"],
    [/data-view-link="troubleshooting"/i, "troubleshooting-link"]
  ]) {
    context.assertMatch(index, pattern, `ops-cockpit:${label}`);
  }
  context.assertMatch(app + servedSource + appSource + apiTypes, /\/api\/v1\/ops\/summary/i, "ops-cockpit:summary-route");
  context.assertMatch(app + servedSource + apiTypes, /checkpoint_warnings/i, "ops-cockpit:checkpoint-warnings");
  for (const [pattern, label] of [
    [/activeView/i, "active-view"],
    [/loadOpsSummary/i, "load-summary"],
    [/renderOpsCockpit/i, "render-cockpit"],
    [/renderDashboardActivity/i, "render-dashboard-activity"],
    [/recent_activity/i, "recent-activity"],
    [/summary-errors/i, "summary-errors"],
    [/renderVmWorkbenchContext/i, "render-vm-context"],
    [/renderIncidentCommand/i, "render-incident-command"],
    [/summaryError/i, "summary-error"]
  ]) {
    context.assertMatch(combined, pattern, `ops-cockpit:${label}`);
  }
  context.assertMatch(apiTypes, /OpsSummaryResponse/i, "ops-cockpit:response-type");
  context.assertMatch(appSource, /opsSummary/i, "ops-cockpit:app-source-summary");
  const allSources = index + app + servedSource + apiTypes + appSource;
  context.assertNotMatch(allSources, /Create Linux VM|ubuntu-24\.04|journalctl|libvirt|purecvisorsd|KVM|ZFS|OVS|\bOVN\b/i, "ops-cockpit:no-linux-runtime");
  context.assertNotMatch(allSources, /Bearer\s+(?!\$\{)[A-Za-z0-9._~+/=-]{24,}/i, "ops-cockpit:no-literal-bearer");
}

function verifyEvidenceDashboard(context) {
  const index = context.readText("web/index.html");
  const app = context.readText("web/app.js");
  const servedSource = context.readText("web/src/served-app.ts");
  const apiTypes = context.readText("web/src/api-types.ts");
  const fixtures = context.readText("web/src/user-visible-fixtures.ts");
  const combined = app + servedSource;
  for (const [pattern, label] of [
    [/data-view-link="evidence"/i, "evidence-link"],
    [/id="evidence"/i, "evidence-view"],
    [/id="evidence-panel"/i, "evidence-panel"]
  ]) {
    context.assertMatch(index, pattern, `evidence-dashboard:${label}`);
  }
  for (const [pattern, label] of [
    [/renderEvidenceDashboard/i, "render-dashboard"],
    [/renderEvidenceStatusBadge/i, "render-status-badge"],
    [/batch_evidence/i, "batch-evidence"],
    [/renderCurrentEvidenceStatusBadge/i, "render-current-status-badge"]
  ]) {
    context.assertMatch(combined, pattern, `evidence-dashboard:${label}`);
  }
  const typedFixtures = app + servedSource + apiTypes + fixtures;
  for (const [pattern, label] of [
    [/current_evidence/i, "current-evidence"],
    [/public_boundary/i, "public-boundary"],
    [/full_admin_host_mutation/i, "full-admin-host-mutation"],
    [/latest_package_pair/i, "latest-package-pair"],
    [/next_package_pair/i, "next-package-pair"],
    [/artifact-discovered/i, "artifact-discovered"],
    [/batch_evidence_artifact/i, "batch-evidence-artifact"],
    [/gpu_snapshots/i, "gpu-snapshots"],
    [/route_msi_hyperv/i, "route-msi-hyperv"],
    [/os_mutation/i, "os-mutation"]
  ]) {
    context.assertMatch(typedFixtures, pattern, `evidence-dashboard:${label}`);
  }
  const visibleFixtures = app + servedSource + fixtures;
  for (const [pattern, label] of [
    [/Public boundary head/i, "public-boundary-head"],
    [/26578120570/i, "public-boundary-run"],
    [/7a7d5de822bdb058b04149eeeef0a7eb462828b5/i, "public-boundary-commit"],
    [/Manual admin next/i, "manual-admin-next"],
    [/opened-public-boundary-current-evidence-rollup-payload/i, "current-evidence-payload"],
    [/not_configured|missing|degraded|unavailable|available/i, "status-values"]
  ]) {
    context.assertMatch(visibleFixtures, pattern, `evidence-dashboard:${label}`);
  }
  for (const status of ["available", "degraded", "missing", "unavailable"]) {
    context.assertMatch(fixtures, new RegExp(`status: "${status}"`, "i"), `evidence-dashboard:fixture-status-${status}`);
  }
}

function verifyEvidenceDegradation(context) {
  const index = context.readText("web/index.html");
  const app = context.readText("web/app.js");
  const servedSource = context.readText("web/src/served-app.ts");
  const fixtures = context.readText("web/src/user-visible-fixtures.ts");
  const combined = app + servedSource;
  context.assertMatch(app, /collectEvidenceIssues/i, "evidence-degradation:collect-evidence-issues:generated");
  context.assertMatch(index, /id="troubleshooting-panel"/i, "evidence-degradation:troubleshooting-panel");
  for (const [pattern, label] of [
    [/collectEvidenceIssues/i, "collect-evidence-issues"],
    [/renderTroubleshootingEvidence/i, "render-troubleshooting-evidence"],
    [/renderFailedJobTriageRows/i, "render-failed-job-triage"],
    [/batch-evidence/i, "batch-evidence"],
    [/retryable/i, "retryable"]
  ]) {
    context.assertMatch(combined, pattern, `evidence-degradation:${label}`);
  }
  context.assertMatch(app + servedSource + fixtures, /PCV_BATCH_EVIDENCE_PARSE_FAILED|PCV_BATCH_EVIDENCE_ROOT_MISSING/i, "evidence-degradation:error-codes");
  context.assertNotMatch(combined, /Restart-Computer|msiexec|New-VM|Remove-VM|New-NetFirewallRule/i, "evidence-degradation:no-host-mutation");
}

function verifyDiagnosticBundle(context) {
  const index = context.readText("web/index.html");
  const app = context.readText("web/app.js");
  const servedSource = context.readText("web/src/served-app.ts");
  const fixtures = context.readText("web/src/user-visible-fixtures.ts");
  const combined = app + servedSource;
  context.assertMatch(app, /renderDiagnosticBundleList/i, "diagnostic-bundle:render-list:generated");
  context.assertMatch(index, /id="diagnostics-panel"/i, "diagnostic-bundle:panel");
  for (const [pattern, label] of [
    [/renderDiagnosticsBundle/i, "render"],
    [/getRuntimeApiRegistryBridge/i, "get-registry-bridge"],
    [/renderRuntimeApiRegistryBridge/i, "render-registry-bridge"],
    [/renderHostOpsLifecycleBucketTable/i, "render-hostops-buckets"],
    [/renderDiagnosticBundleList/i, "render-list"],
    [/createDiagnosticBundle/i, "create"],
    [/listDiagnosticBundles/i, "list"],
    [/state\.diagnosticBundleError\s*=\s*null;\s*state\.diagnosticBundlePage\s*=\s*page/is, "list-page-state"],
    [/downloadDiagnosticBundle/i, "download"],
    [/data-action="diagnostic-create"/i, "create-action"],
    [/data-action="diagnostic-download"/i, "download-action"],
    [/data-action="diagnostic-list-next"/i, "list-next-action"],
    [/\/api\/v1\/diagnostics\/bundles/i, "bundles-route"],
    [/next_offset/i, "next-offset"],
    [/max_bundle_count/i, "retention-count"],
    [/\/download/i, "download-route"],
    [/CollectDiagnostics/i, "collect-operation"],
    [/Runtime\/API registry bridge/i, "registry-bridge-label"],
    [/ops summary direct expose/i, "ops-summary-exposure"],
    [/route detail metadata only/i, "route-detail-boundary"],
    [/diagnostics-route-list/i, "route-list"],
    [/routeDetailHtml/i, "route-detail-html"],
    [/Host Ops lifecycle buckets/i, "hostops-label"],
    [/diagnostics-hostops-table/i, "hostops-table"],
    [/Host mutation: not performed by diagnostics view/i, "host-mutation-boundary"],
    [/runtime_api_registry_bridge|runtimeApiRegistryBridge/i, "registry-bridge-key"],
    [/API action/i, "api-action"],
    [/token values/i, "token-redaction"],
    [/Authorization headers/i, "authorization-redaction"],
    [/no host mutation/i, "no-host-mutation-label"]
  ]) {
    context.assertMatch(combined, pattern, "diagnostic-bundle:" + label);
  }
  for (const [pattern, label] of [
    [/service-action-eventlog-firewall-truststore-credential-manager-data-root-separated/i, "hostops-bucket-contract"],
    [/credential-manager-system-proof/i, "credential-manager-proof"],
    [/allowlisted-programdata-root/i, "allowlisted-data-root"]
  ]) {
    context.assertMatch(combined + fixtures, pattern, "diagnostic-bundle:" + label);
  }
  context.assertEqual(
    combined.includes("PureCVisor\\\\desktop-node\\\\diagnostics"),
    true,
    "diagnostic-bundle:diagnostics-path"
  );
  context.assertNotMatch(
    combined,
    /pwsh|powershell|Restart-Computer|msiexec|New-VM|Remove-VM|New-NetFirewallRule|Register-EventSource|New-EventLog|Register-ScheduledTask/i,
    "diagnostic-bundle:no-powershell-host-mutation"
  );
  context.assertNotMatch(
    combined,
    /Bearer\s+(?![$][{])[A-Za-z0-9._~+/=-]{24,}/i,
    "diagnostic-bundle:no-literal-bearer"
  );
}

function verifyOperatorTerms(context) {
  const index = context.readText("web/index.html");
  const servedSource = context.readServedSource();
  const terms = context.readText("docs/OPERATOR_SURFACE_TERMS.md");
  const webSurface = servedSource + index;
  for (const [value, label] of [
    ["배포 경계: 내부 사설망 전용", "distribution-boundary"],
    ["Diagnostic bundle: redaction이 적용된 server-side support bundle", "diagnostic-bundle-term"],
    ["VM delete 확인: Web Console과 CLI는 destructive VM delete 전에 명시 확인을 요구한다", "vm-delete-confirmation-term"],
    ["Checkpoint mutation: Web Console은 checkpoint restore/delete confirmation dialog를 요구한다", "checkpoint-confirmation-term"]
  ]) {
    context.assertEqual(terms.includes(value), true, "operator-terms:" + label);
  }
  for (const [pattern, label] of [
    [/Diagnostic bundle|diagnostic bundle/i, "diagnostic-bundle-surface"],
    [/Only PureCVisor-managed VMs can be deleted/i, "managed-vm-delete-copy"],
    [/window\.confirm\(buildVmDeleteConfirmation/i, "vm-delete-confirmation"],
    [/window\.confirm\(buildCheckpointRestoreConfirmation/i, "checkpoint-restore-confirmation"],
    [/window\.confirm\(buildCheckpointDeleteConfirmation/i, "checkpoint-delete-confirmation"]
  ]) {
    context.assertMatch(webSurface, pattern, "operator-terms:" + label);
  }
  context.assertNotMatch(
    webSurface,
    /(public trusted signing|winget public submission|winget submission|external stable publication|public release).{0,80}(available|ready|pass|claimed|enabled)/is,
    "operator-terms:no-public-distribution-claim"
  );
}

function verifyFrontendEdgeCases(context) {
  const app = context.readText("web/app.js");
  const servedSource = context.readServedSource();
  const styles = context.readText("web/styles.css");
  const design = context.readText("web/DESIGN.md");
  const installedQaScript = context.readText("web/scripts/capture-installed-listener-qa.mjs");
  const combined = app + servedSource;
  for (const [pattern, label] of [
    [/getDiagnosticActionPolicy/i, "diagnostic-action-policy"],
    [/PCV_DIAGNOSTIC_BUNDLE_API_UNSUPPORTED/i, "diagnostic-api-unsupported"],
    [/diagnostic-retry/i, "diagnostic-retry"],
    [/401/i, "http-401"],
    [/404/i, "http-404"],
    [/500/i, "http-500"],
    [/timeout/i, "timeout"],
    [/tokenRequiredRouteStatus/i, "token-required-status"],
    [/browser token cleared/i, "token-cleared-copy"],
    [/all views refresh/i, "all-views-refresh"],
    [/pending jobs are rechecked/i, "pending-jobs-rechecked"],
    [/renderJobEdgeSummary/i, "job-edge-summary"],
    [/No jobs on this page/i, "empty-job-page"],
    [/retained terminal jobs/i, "retained-terminal-jobs"],
    [/failed job retry/i, "failed-job-retry"],
    [/vm\.delete/i, "vm-delete"],
    [/Reconcile delete/i, "reconcile-delete"],
    [/checkpoint\.create/i, "checkpoint-create"],
    [/Reconcile checkpoint/i, "reconcile-checkpoint"],
    [/checkpoint\.restore/i, "checkpoint-restore"],
    [/Reconcile restore/i, "reconcile-restore"],
    [/reconcileSelectedVm/i, "reconcile-selected-vm"],
    [/PCV_SELECTED_VM_STALE/i, "selected-vm-stale"],
    [/buildCheckpointRestoreConfirmation/i, "checkpoint-restore-confirmation"],
    [/buildCheckpointDeleteConfirmation/i, "checkpoint-delete-confirmation"],
    [/network-empty-state/i, "network-empty-state"],
    [/PCV_NATIVE_PARITY_INCOMPLETE/i, "native-parity-incomplete"],
    [/native parity failure/i, "native-parity-failure"]
  ]) {
    context.assertMatch(combined, pattern, "frontend-edge-cases:" + label);
  }
  context.assertMatch(app, /PCV_SELECTED_VM_STALE/i, "frontend-edge-cases:selected-vm-stale:generated");
  for (const [pattern, label] of [
    [/:focus-visible/i, "focus-visible"],
    [/@media \(prefers-reduced-motion: reduce\)/i, "reduced-motion"],
    [/@media \(max-width: 900px\)/i, "responsive-breakpoint"]
  ]) {
    context.assertMatch(styles, pattern, "frontend-edge-cases:" + label);
  }
  for (const [pattern, label] of [
    [/Static parity snapshot policy/i, "static-parity-policy"],
    [/npm run build:served/i, "build-served-command"],
    [/npm run generate:parity/i, "generate-parity-command"]
  ]) {
    context.assertMatch(design, pattern, "frontend-edge-cases:" + label);
  }
  for (const [pattern, label] of [
    [/installed-listener-qa/i, "installed-qa-marker"],
    [/PCV_BROWSER_QA_ACCOUNT_USERNAME/i, "qa-username"],
    [/PCV_BROWSER_QA_ACCOUNT_PASSWORD/i, "qa-password"],
    [/account-login-form/i, "login-form"],
    [/diagnostic_create_clicked/i, "diagnostic-create-clicked"],
    [/value_observed:\s*false/i, "secret-value-not-observed"],
    [/--disable-background-networking/i, "disable-background-networking"],
    [/Browser\.close/i, "browser-close"],
    [/taskkill\.exe/i, "taskkill"],
    [/SIGKILL/i, "sigkill"],
    [/removeProfileDirectory/i, "remove-profile-directory"],
    [/rmSync\(profileDir/i, "profile-rmsync"]
  ]) {
    context.assertMatch(installedQaScript, pattern, "frontend-edge-cases:" + label);
  }
  context.assertNotMatch(
    installedQaScript,
    /Bearer\s+(?![$][{])[A-Za-z0-9._~+/=-]{24,}/i,
    "frontend-edge-cases:no-literal-bearer"
  );
}

function verifyTokenRotation(context) {
  const index = context.readText("web/index.html");
  const app = context.readText("web/app.js");
  const servedSource = context.readText("web/src/served-app.ts");
  const combined = app + servedSource;
  context.assertMatch(index, /id="token-rotation-panel"/i, "token-rotation:panel");
  for (const [pattern, label] of [
    [/renderTokenRotation/i, "render"],
    [/Token Rotation/i, "title"],
    [/api-token\.dpapi\.json/i, "protected-token-file"],
    [/rotation handoff/i, "handoff"],
    [/Clear browser token/i, "clear-browser-token"],
    [/no service token mutation/i, "no-service-token-mutation"]
  ]) {
    context.assertMatch(combined, pattern, `token-rotation:${label}`);
  }
  context.assertNotMatch(
    combined,
    /Restart-Computer|msiexec|New-VM|Remove-VM|New-NetFirewallRule|Register-EventSource|New-EventLog|Register-ScheduledTask/i,
    "token-rotation:no-host-mutation"
  );
  context.assertNotMatch(
    combined,
    /Bearer\s+(?!\$\{)[A-Za-z0-9._~+/=-]{24,}/i,
    "token-rotation:no-literal-bearer"
  );
}

function verifyBetaFollowup(context) {
  const index = context.readText("web/index.html");
  const app = context.readText("web/app.js");
  const servedSource = context.readText("web/src/served-app.ts");
  const combined = app + servedSource;
  context.assertMatch(index, /id="beta-followup-panel"/i, "beta-followup:panel");
  for (const [pattern, label] of [
    [/renderBetaFollowup/i, "render"],
    [/Installed listener QA automation/i, "installed-listener-qa"],
    [/service token revoke handoff/i, "service-token-revoke-handoff"],
    [/diagnostic retention pagination/i, "diagnostic-retention-pagination"],
    [/VM delete guarded/i, "vm-delete-guarded"],
    [/ops cockpit P0\/P1\/P2/i, "ops-cockpit-priorities"],
    [/public distribution bundle/i, "public-distribution-bundle"],
    [/host mutation not started from browser/i, "host-mutation-boundary"]
  ]) {
    context.assertMatch(combined, pattern, `beta-followup:${label}`);
  }
  context.assertNotMatch(
    combined,
    /Restart-Computer|msiexec|New-VM|Remove-VM|New-NetFirewallRule|Register-EventSource|New-EventLog|Register-ScheduledTask/i,
    "beta-followup:no-host-mutation"
  );
  context.assertNotMatch(
    combined,
    /Bearer\s+(?!\$\{)[A-Za-z0-9._~+/=-]{24,}/i,
    "beta-followup:no-literal-bearer"
  );
}

function verifyMonitoring(context) {
  const index = context.readText("web/index.html");
  const app = context.readText("web/app.js");
  for (const [pattern, label] of [
    [/id="monitoring"/i, "view"],
    [/id="monitoring-panel"/i, "panel"]
  ]) {
    context.assertMatch(index, pattern, `monitoring:${label}`);
  }
  for (const [pattern, label] of [
    [/renderMonitoring/i, "render"],
    [/buildMonitoringSignals/i, "build-signals"],
    [/checkpoint-warning/i, "checkpoint-warning"],
    [/token-policy/i, "token-policy"],
    [/lan-exposure/i, "lan-exposure"],
    [/route-timeout/i, "route-timeout"],
    [/request-limit/i, "request-limit"],
    [/burst-limit/i, "burst-limit"],
    [/retry-after/i, "retry-after"]
  ]) {
    context.assertMatch(app, pattern, `monitoring:${label}`);
  }
}

function verifyNetworkInventory(context) {
  const index = context.readText("web/index.html");
  const app = context.readText("web/app.js");
  const servedSource = context.readText("web/src/served-app.ts");
  const apiTypes = context.readText("web/src/api-types.ts");
  const fixtures = context.readText("web/src/user-visible-fixtures.ts");
  const combined = app + servedSource;
  for (const [pattern, label] of [
    [/data-view-link="network"/i, "network-link"],
    [/id="network"/i, "network-view"],
    [/id="view-network"/i, "view-network"],
    [/id="network-inventory-panel"/i, "panel"]
  ]) {
    context.assertMatch(index, pattern, `network-inventory:${label}`);
  }
  context.assertMatch(
    app + servedSource + apiTypes,
    /\/api\/v1\/network\/inventory/i,
    "network-inventory:route"
  );
  for (const [pattern, label] of [
    [/networkInventory/i, "network-inventory"],
    [/loadNetworkInventory/i, "load"],
    [/renderNetworkInventory/i, "render"]
  ]) {
    context.assertMatch(combined, pattern, `network-inventory:${label}`);
  }
  for (const [pattern, label] of [
    [/Default Switch/i, "default-switch"],
    [/fixture-ethernet/i, "fixture-ethernet"]
  ]) {
    context.assertMatch(combined + fixtures, pattern, `network-inventory:${label}`);
  }
  context.assertMatch(combined, /read-only/i, "network-inventory:read-only");
  context.assertNotMatch(
    combined,
    /New-VMSwitch|Remove-VMSwitch|New-NetIPAddress|Set-NetFirewallRule/i,
    "network-inventory:no-network-mutation"
  );
}

function verifyWorkflowPolish(context) {
  const index = context.readText("web/index.html");
  const app = context.readText("web/app.js");
  for (const [pattern, label] of [
    [/id="vm-filter"/i, "vm-filter"],
    [/id="asset-status"/i, "asset-status"]
  ]) {
    context.assertMatch(index, pattern, `workflow-polish:${label}`);
  }
  for (const [pattern, label] of [
    [/renderAssetStatus/i, "render-asset-status"],
    [/buildVmLifecycleConfirmation/i, "lifecycle-confirmation"],
    [/vmFilter/i, "vm-filter"]
  ]) {
    context.assertMatch(app, pattern, `workflow-polish:${label}`);
  }
  context.assertNotMatch(
    index + app,
    /Create Linux VM|ubuntu-24\.04|journalctl|libvirt|purecvisorsd|ZFS|OVS|\bOVN\b/i,
    "workflow-polish:no-linux-runtime"
  );
}

async function verifyJavaScriptSyntax(context) {
  await context.runOwners(["node-check"]);
}

async function verifyServedTypeScriptOutput(context) {
  await context.runOwners(["served-asset", "static-parity"]);
  context.assertExists("web/src/served-app.ts", "served-typescript-output:served-entry");
  const packageJson = context.readJson("web/package.json");
  context.assertEqual(packageJson.scripts?.["build:served"], "node scripts/build-served-asset.mjs --write", "served-typescript-output:build-served");
  context.assertEqual(packageJson.scripts?.["check:served"], "node scripts/build-served-asset.mjs --check", "served-typescript-output:check-served");
  context.assertMatch(packageJson.scripts?.test, /check:served/i, "served-typescript-output:test-check-served");
  context.assertMatch(packageJson.scripts?.["verify:parity"], /check:served/i, "served-typescript-output:verify-check-served");
  const manifest = context.readJson("web/generated/parity/static-asset-parity.manifest.json");
  context.assertEqual(manifest.scaffold?.runtimeReplacement, "default", "served-typescript-output:runtime-replacement");
  context.assertEqual(manifest.replacesServedAsset, true, "served-typescript-output:replaces-served-asset");
  context.assertEqual(manifest.servedTypeScriptEntry, "src/served-app.ts", "served-typescript-output:served-entry-manifest");
  context.assertEqual(manifest.regeneration?.replacesServedAsset, true, "served-typescript-output:regeneration-replaces-served-asset");
  context.assertEqual(manifest.browserFixture?.replacesServedAsset, true, "served-typescript-output:fixture-replaces-served-asset");
}

async function verifyTypeScriptScaffold(context) {
  await context.runOwners(["typescript", "static-contract"]);
  for (const [file, label] of [
    ["web/package.json", "package"], ["web/tsconfig.json", "tsconfig"], ["web/src/api-types.ts", "api-types"],
    ["web/src/view-model.ts", "view-model"], ["web/src/app.ts", "app"], ["web/src/served-app.ts", "served-app"],
    ["web/src/user-visible-fixtures.ts", "user-visible-fixtures"]
  ]) context.assertExists(file, `typescript-scaffold:${label}`);
  const packageJson = context.readJson("web/package.json");
  context.assertEqual(packageJson.private, true, "typescript-scaffold:private");
  context.assertEqual(packageJson.scripts?.test, "npm run check:feature-surfaces && tsc --noEmit -p tsconfig.json && npm run check:served && npm run check:frontend-batches", "typescript-scaffold:test-script");
  const index = context.readText("web/index.html");
  context.assertMatch(index, /app\.js/i, "typescript-scaffold:index-app-js");
  context.assertNotMatch(index, /src\/app\.ts/i, "typescript-scaffold:index-no-source-app");
}

async function verifyTypeScriptContractMirror(context) {
  await context.runOwners(["typescript", "static-contract"]);
  const apiTypes = context.readText("web/src/api-types.ts");
  const viewModel = context.readText("web/src/view-model.ts");
  const app = context.readText("web/src/app.ts");
  const served = context.readServedSource();
  for (const [value, label] of [[apiTypes, "runtime-policy-response"], [apiTypes, "job-runtime-policy"], [apiTypes, "virtual-machine-summary"], [apiTypes, "job-summary"], [viewModel, "view-model"], [app, "scaffold"], [served, "dom-content-loaded"], [served, "api-fetch"]]) {
    const patterns = { "runtime-policy-response": /RuntimePolicyResponse/i, "job-runtime-policy": /JobRuntimePolicy/i, "virtual-machine-summary": /VirtualMachineSummary/i, "job-summary": /JobSummary/i, "view-model": /buildDashboardViewModel/i, scaffold: /static-asset-parity-scaffold-first/i, "dom-content-loaded": /DOMContentLoaded/i, "api-fetch": /apiFetch/i };
    context.assertMatch(value, patterns[label], `typescript-contract-mirror:${label}`);
  }
  const combined = apiTypes + viewModel + app + served;
  for (const [pattern, label] of [[/\/api\/v1\/runtime\/policy/i, "runtime-policy-route"], [/\/api\/v1\/host\/status/i, "host-status-route"], [/\/api\/v1\/vms/i, "vm-route"]]) context.assertMatch(combined, pattern, `typescript-contract-mirror:${label}`);
  context.assertNotMatch(combined, /Restart-Computer|msiexec|Register-ScheduledTask|New-VM|Remove-VM|New-NetFirewallRule/i, "typescript-contract-mirror:no-host-mutation");
  context.assertNotMatch(combined, /journalctl|libvirt|KVM|ZFS|OVS|\bOVN\b|purecvisorsd/i, "typescript-contract-mirror:no-linux-runtime");
  context.assertNotMatch(combined, /Bearer\s+(?!\$\{)[A-Za-z0-9._~+/=-]{24,}/i, "typescript-contract-mirror:no-literal-bearer");
}

async function verifyParityManifest(context) {
  await context.runOwners(["static-parity"]);
  context.assertExists("web/src/generate-parity-manifest.ts", "parity-manifest:generator");
  context.assertExists("web/generated/parity/static-asset-parity.manifest.json", "parity-manifest:file");
  const manifestText = context.readText("web/generated/parity/static-asset-parity.manifest.json");
  const manifest = context.readJson("web/generated/parity/static-asset-parity.manifest.json");
  for (const [actual, expected, label] of [[manifest.generatedBy, "src/generate-parity-manifest.ts", "generated-by"], [manifest.regeneratedBy, "scripts/regenerate-static-parity.mjs", "regenerated-by"], [manifest.scaffold?.decisionCandidate, "static-asset-parity-scaffold-first", "decision-candidate"], [manifest.scaffold?.runtimeReplacement, "default", "runtime-replacement"], [manifest.servedAsset, "app.js", "served-asset"], [manifest.indexScriptSrc, "/app.js", "index-script-src"], [manifest.typeScriptEntry, "src/app.ts", "typescript-entry"], [manifest.servedTypeScriptEntry, "src/served-app.ts", "served-typescript-entry"], [manifest.userVisibleFixtureEntry, "src/user-visible-fixtures.ts", "fixture-entry"]]) context.assertEqual(actual, expected, `parity-manifest:${label}`);
  context.assertEqual(manifest.replacesServedAsset, true, "parity-manifest:replaces-served-asset");
  assertArray(context, manifest.userVisibleFixtureNames, "parity-manifest:fixture-names-array");
  for (const fixture of ["emptyInventory", "runningVmAndJob", "unsupportedHost"]) context.assertIncludes(manifest.userVisibleFixtureNames, fixture, `parity-manifest:fixture-${fixture}`);
  for (const [key, route, label] of [["runtimePolicy", "/api/v1/runtime/policy", "runtime-policy"], ["hostStatus", "/api/v1/host/status", "host-status"], ["networkInventory", "/api/v1/network/inventory", "network-inventory"], ["vmList", "/api/v1/vms", "vm-list"], ["jobList", "/api/v1/jobs", "job-list"], ["jobsPage", "/api/v1/jobs?limit={limit}&offset={offset}", "jobs-page"], ["vmDetail", "/api/v1/vms/{vm_id}", "vm-detail"], ["vmAction", "/api/v1/vms/{vm_id}/{action}", "vm-action"], ["vmCheckpoints", "/api/v1/vms/{vm_id}/checkpoints", "vm-checkpoints"], ["checkpointAction", "/api/v1/vms/{vm_id}/checkpoints/{checkpoint_id}/{action}", "checkpoint-action"], ["jobAction", "/api/v1/jobs/{job_id}/{action}", "job-action"]]) context.assertEqual(manifest.localApiRoutes?.[key], route, `parity-manifest:${label}-route`);
  for (const [key, expected, label] of [["source", "src/app.ts", "regeneration-source"], ["output", "generated/parity/static-asset-parity.manifest.json", "regeneration-output"], ["writeCommand", "npm run generate:parity", "regeneration-write-command"], ["checkCommand", "npm run verify:parity", "regeneration-check-command"]]) context.assertEqual(manifest.regeneration?.[key], expected, `parity-manifest:${label}`);
  context.assertEqual(manifest.regeneration?.replacesServedAsset, true, "parity-manifest:regeneration-replaces-served-asset");
  const index = context.readText("web/index.html");
  context.assertMatch(index, /<script src="\/app\.js" defer><\/script>/i, "parity-manifest:index-script");
  context.assertNotMatch(index, /src\/app\.ts/i, "parity-manifest:index-no-source-app");
  context.assertNotMatch(manifestText, /Bearer\s+(?!\$\{)[A-Za-z0-9._~+/=-]{24,}/i, "parity-manifest:no-literal-bearer");
  context.assertNotMatch(manifestText, /Restart-Computer|msiexec|New-NetFirewallRule|Register-EventSource|New-EventLog|Register-ScheduledTask|New-VM|Remove-VM/i, "parity-manifest:no-host-mutation");
}

async function verifyUserVisibleFixtures(context) {
  await context.runOwners(["static-parity"]);
  context.assertExists("web/src/user-visible-fixtures.ts", "user-visible-fixtures:file");
  const fixtures = context.readText("web/src/user-visible-fixtures.ts");
  const manifest = context.readText("web/generated/parity/static-asset-parity.manifest.json");
  for (const [pattern, label] of [[/webConsoleUserVisibleParityFixtures/i, "fixtures"], [/buildStaticParitySnapshot/i, "snapshot"], [/emptyInventory/i, "empty-inventory"], [/runningVmAndJob/i, "running-vm-and-job"], [/unsupportedHost/i, "unsupported-host"], [/default/i, "default"], [/app\.js/i, "app-js"], [/src\/served-app\.ts/i, "served-app"]]) context.assertMatch(fixtures, pattern, `user-visible-fixtures:${label}`);
  context.assertNotMatch(fixtures, /document\.|window\.|dist\//i, "user-visible-fixtures:no-browser-or-dist");
  for (const fixture of ["src/user-visible-fixtures.ts", "emptyInventory", "runningVmAndJob", "unsupportedHost"]) context.assertMatch(manifest, new RegExp(fixture.replace(/[.*+?^${}()|[\]\\]/g, "\\$&"), "i"), `user-visible-fixtures:manifest-${fixture}`);
}

async function verifyVerifierWiring(context) {
  await context.runOwners(["served-asset", "static-parity", "browser-fixture", "frontend-batches"]);
  for (const [file, label] of [["web/scripts/verify-static-parity.mjs", "verify-script"], ["web/scripts/regenerate-static-parity.mjs", "regenerate-script"], ["web/scripts/build-served-asset.mjs", "served-builder"], ["web/scripts/verify-browser-fixture.mjs", "browser-fixture"], ["web/scripts/validate-frontend-completion-batches.mjs", "frontend-batches"]]) context.assertExists(file, `verifier-wiring:${label}`);
  const packageJson = context.readJson("web/package.json");
  for (const name of ["build:served", "check:served", "generate:parity", "verify:parity", "check:frontend-batches"]) context.assertIncludes(Object.keys(packageJson.scripts ?? {}), name, `verifier-wiring:${name.replace(":", "-")}-script`);
  for (const [name, value, label] of [["build:served", "node scripts/build-served-asset.mjs --write", "build-served"], ["check:served", "node scripts/build-served-asset.mjs --check", "check-served"], ["generate:parity", "node scripts/regenerate-static-parity.mjs --write", "generate-parity"], ["browser:fixture", "node scripts/verify-browser-fixture.mjs", "browser-fixture"], ["check:frontend-batches", "node scripts/validate-frontend-completion-batches.mjs", "frontend-batches"]]) context.assertEqual(packageJson.scripts?.[name], value, `verifier-wiring:${label}-script`);
  for (const [pattern, label] of [[/check:served/i, "check-served"], [/scripts\/regenerate-static-parity\.mjs --check/i, "regenerate"], [/scripts\/verify-static-parity\.mjs/i, "verify"], [/browser:fixture/i, "browser-fixture"]]) context.assertMatch(packageJson.scripts?.["verify:parity"], pattern, `verifier-wiring:verify-parity-${label}`);
  const verify = context.readText("web/scripts/verify-static-parity.mjs"); const regenerate = context.readText("web/scripts/regenerate-static-parity.mjs"); const builder = context.readText("web/scripts/build-served-asset.mjs"); const browser = context.readText("web/scripts/verify-browser-fixture.mjs"); const batches = context.readText("web/scripts/validate-frontend-completion-batches.mjs");
  for (const [source, pattern, label] of [[verify, /static-asset-parity\.manifest\.json/i, "verify-manifest"], [verify, /src\/generate-parity-manifest\.ts/i, "verify-generator"], [verify, /src\/app\.ts/i, "verify-app"], [verify, /src\/served-app\.ts/i, "verify-served"], [verify, /index\.html/i, "verify-index"], [verify, /app\.js/i, "verify-output"], [verify, /verify-browser-fixture\.mjs/i, "verify-browser"], [builder, /typescript/i, "builder-typescript"], [builder, /src\/served-app\.ts/i, "builder-served"], [builder, /app\.js/i, "builder-output"], [regenerate, /typescript/i, "regenerate-typescript"], [regenerate, /phase25WebConsoleScaffold/i, "regenerate-scaffold"], [regenerate, /localApiRoutes/i, "regenerate-routes"], [regenerate, /browser:fixture/i, "regenerate-browser"], [regenerate, /--write/i, "regenerate-write"], [regenerate, /--check/i, "regenerate-check"], [browser, /DOMContentLoaded/i, "browser-dom"], [browser, /pcv-browser-fixture/i, "browser-fixture"], [browser, /job-browser-fixture/i, "browser-job"], [batches, /frontend-completion-auto-batches\.json/i, "batches-plan"], [batches, /work_item_count/i, "batches-count"]]) context.assertMatch(source, pattern, `verifier-wiring:${label}`);
}

async function verifyGeneratedParityAlignment(context) {
  await context.runOwners(["static-parity", "browser-fixture"]);
  const index = context.readText("web/index.html"); const manifestText = context.readText("web/generated/parity/static-asset-parity.manifest.json"); const manifest = context.readJson("web/generated/parity/static-asset-parity.manifest.json"); const verify = context.readText("web/scripts/verify-static-parity.mjs"); const regenerate = context.readText("web/scripts/regenerate-static-parity.mjs"); const app = context.readText("web/src/app.ts"); const served = context.readText("web/src/served-app.ts"); const fixtures = context.readText("web/src/user-visible-fixtures.ts"); const generator = context.readText("web/src/generate-parity-manifest.ts");
  context.assertMatch(index, /<script src="\/app\.js" defer><\/script>/i, "generated-parity-alignment:index-script"); context.assertNotMatch(index, /src\/app\.ts|dist\//i, "generated-parity-alignment:index-no-source-or-dist");
  for (const [actual, expected, label] of [[manifest.servedAsset, "app.js", "served-asset"], [manifest.indexScriptSrc, "/app.js", "index-script"], [manifest.typeScriptEntry, "src/app.ts", "typescript-entry"], [manifest.servedTypeScriptEntry, "src/served-app.ts", "served-entry"], [manifest.userVisibleFixtureEntry, "src/user-visible-fixtures.ts", "fixture-entry"], [manifest.scaffold?.decisionCandidate, "static-asset-parity-scaffold-first", "decision-candidate"], [manifest.scaffold?.runtimeReplacement, "default", "runtime-replacement"], [manifest.localApiRoutes?.runtimePolicy, "/api/v1/runtime/policy", "runtime-policy"], [manifest.localApiRoutes?.hostStatus, "/api/v1/host/status", "host-status"], [manifest.localApiRoutes?.vmList, "/api/v1/vms", "vm-list"], [manifest.localApiRoutes?.jobList, "/api/v1/jobs", "job-list"], [manifest.localApiRoutes?.vmAction, "/api/v1/vms/{vm_id}/{action}", "vm-action"], [manifest.localApiRoutes?.checkpointAction, "/api/v1/vms/{vm_id}/checkpoints/{checkpoint_id}/{action}", "checkpoint-action"], [manifest.localApiRoutes?.jobAction, "/api/v1/jobs/{job_id}/{action}", "job-action"], [manifest.regeneration?.output, "generated/parity/static-asset-parity.manifest.json", "regeneration-output"], [manifest.regeneration?.writeCommand, "npm run generate:parity", "regeneration-write"], [manifest.regeneration?.checkCommand, "npm run verify:parity", "regeneration-check"], [manifest.browserFixture?.script, "scripts/verify-browser-fixture.mjs", "browser-fixture-script"], [manifest.browserFixture?.command, "npm run browser:fixture", "browser-fixture-command"], [manifest.browserFixture?.mode, "node-vm-minimal-dom", "browser-fixture-mode"]]) context.assertEqual(actual, expected, `generated-parity-alignment:${label}`);
  context.assertEqual(manifest.replacesServedAsset, true, "generated-parity-alignment:replaces-served-asset"); context.assertEqual(manifest.regeneration?.replacesServedAsset, true, "generated-parity-alignment:regeneration-replaces-served-asset"); context.assertEqual(manifest.browserFixture?.mutating, false, "generated-parity-alignment:browser-fixture-mutating"); context.assertEqual(manifest.browserFixture?.replacesServedAsset, true, "generated-parity-alignment:browser-fixture-replaces-served-asset");
  const combined = verify + regenerate + context.readText("web/scripts/verify-browser-fixture.mjs") + app + served + fixtures + generator + manifestText;
  for (const pattern of [/static-asset-parity-scaffold-first/i, /src\/served-app\.ts/i, /\/api\/v1\/runtime\/policy/i, /\/api\/v1\/host\/status/i, /\/api\/v1\/vms/i, /\/api\/v1\/jobs/i]) context.assertMatch(combined, pattern, `generated-parity-alignment:combined-${pattern.source}`);
}

async function verifySecretMutationGuard(context) {
  await context.runOwners(["static-contract", "static-parity"]);
  const combined = context.readCombined(["web/package.json", "web/scripts/verify-static-parity.mjs", "web/scripts/regenerate-static-parity.mjs", "web/scripts/verify-browser-fixture.mjs", "web/src/generate-parity-manifest.ts", "web/scripts/build-served-asset.mjs", "web/scripts/validate-frontend-completion-batches.mjs", "web/src/app.ts", "web/src/served-app.ts", "web/src/user-visible-fixtures.ts", "web/src/api-types.ts", "web/src/view-model.ts", "web/generated/parity/static-asset-parity.manifest.json"]) + context.readServedSource();
  context.assertNotMatch(combined, /Bearer\s+(?!\$\{)[A-Za-z0-9._~+/=-]{24,}/i, "secret-mutation-guard:no-literal-bearer");
  context.assertNotMatch(combined, /Restart-Computer|msiexec|New-NetFirewallRule|Register-EventSource|New-EventLog|Register-ScheduledTask|New-VM|Remove-VM/i, "secret-mutation-guard:no-host-mutation");
}

async function verifyNoFabricatedValues(context) {
  await context.runOwners(["static-contract", "browser-fixture"]);
  const index = context.readText("web/index.html");
  for (const literal of [">Connected<", "VM: 3/3", "API: 10ms avg", "Updated 0s ago", "<strong>4/5</strong>", "pcv-node-a", "pcv-node-b", "lab-vm-01"]) context.assertNotMatch(index, new RegExp(literal.replace(/[.*+?^${}()|[\]\\]/g, "\\$&"), "i"), `no-fabricated-values:literal-${literal}`);
  for (const [pattern, label] of [[/id="status-connection"[^>]*>\s*Connected\b/i, "connection"], [/id="status-host"[^>]*>\s*\w/i, "host"], [/id="status-updated"[^>]*>\s*Updated\s+\d/i, "updated"], [/id="status-vm-count"[^>]*>\s*VM:\s*\d/i, "vm-count"], [/id="hero-workload"[^>]*>\s*\d/i, "workload"], [/id="hero-host-mode"[^>]*>\s*\w/i, "host-mode"], [/id="hero-alerts"[^>]*>\s*\d/i, "alerts"], [/id="asset-count"[^>]*>\s*\d/i, "asset-count"]]) context.assertNotMatch(index, pattern, `no-fabricated-values:binding-${label}`);
  for (const id of ["status-connection", "status-host", "status-updated", "status-vm-count", "status-view", "hero-workload", "hero-host-mode", "hero-alerts"]) context.assertMatch(index, new RegExp(`id="${id}"`, "i"), `no-fabricated-values:required-${id}`);
}

export const WEB_STATIC_VERIFIERS = Object.freeze({
  "web.static.feature-surface-ledger": verifyFeatureSurfaceLedger,
  "web.static.root-assets": verifyRootAssets,
  "web.static.inline-favicon": verifyInlineFavicon,
  "web.static.single-edge-isolation": verifySingleEdgeIsolation,
  "web.static.design-boundary": verifyDesignBoundary,
  "web.static.supanova-tokens": verifySupanovaTokens,
  "web.static.visual-shell": verifyVisualShell,
  "web.static.workbench-frame": verifyWorkbenchFrame,
  "web.static.frontend-mockups": verifyFrontendMockups,
  "web.static.frontend-batches": verifyFrontendBatches,
  "web.static.phase2h-endpoints": verifyPhase2hEndpoints,
  "web.static.local-api-registry": verifyLocalApiRegistry,
  "web.static.qos-guest-readback": verifyQosGuestReadback,
  "web.static.qos-guest-control": verifyQosGuestControl,
  "web.static.guest-exec-cancel": verifyGuestExecCancel,
  "web.static.search-event-table": verifySearchEventTable,
  "web.static.served-source-parts": verifyServedSourceParts,
  "web.static.optional-bearer": verifyOptionalBearer,
  "web.static.account-rbac-console": verifyAccountRbacConsole,
  "web.static.listener-api-base": verifyListenerApiBase,
  "web.static.vm-create-payload": verifyVmCreatePayload,
  "web.static.vm-lifecycle-routes": verifyVmLifecycleRoutes,
  "web.static.vm-detail-mount": verifyVmDetailMount,
  "web.static.vm-lifecycle-actions": verifyVmLifecycleActions,
  "web.static.checkpoint-actions": verifyCheckpointActions,
  "web.static.browser-job-history": verifyBrowserJobHistory,
  "web.static.job-orchestration": verifyJobOrchestration,
  "web.static.shell-controls": verifyShellControls,
  "web.static.activity-troubleshooting": verifyActivityTroubleshooting,
  "web.static.ops-cockpit": verifyOpsCockpit,
  "web.static.evidence-dashboard": verifyEvidenceDashboard,
  "web.static.evidence-degradation": verifyEvidenceDegradation,
  "web.static.diagnostic-bundle": verifyDiagnosticBundle,
  "web.static.operator-terms": verifyOperatorTerms,
  "web.static.frontend-edge-cases": verifyFrontendEdgeCases,
  "web.static.token-rotation": verifyTokenRotation,
  "web.static.beta-followup": verifyBetaFollowup,
  "web.static.monitoring": verifyMonitoring,
  "web.static.network-inventory": verifyNetworkInventory,
  "web.static.workflow-polish": verifyWorkflowPolish,
  "web.static.javascript-syntax": verifyJavaScriptSyntax,
  "web.static.served-typescript-output": verifyServedTypeScriptOutput,
  "web.static.typescript-scaffold": verifyTypeScriptScaffold,
  "web.static.typescript-contract-mirror": verifyTypeScriptContractMirror,
  "web.static.parity-manifest": verifyParityManifest,
  "web.static.user-visible-fixtures": verifyUserVisibleFixtures,
  "web.static.verifier-wiring": verifyVerifierWiring,
  "web.static.generated-parity-alignment": verifyGeneratedParityAlignment,
  "web.static.secret-mutation-guard": verifySecretMutationGuard,
  "web.static.no-fabricated-values": verifyNoFabricatedValues
});

const metadataIds = WEB_STATIC_CONTRACT_METADATA.map((contract) => contract.id);
const verifierIds = Object.keys(WEB_STATIC_VERIFIERS);
if (metadataIds.length !== 50 || verifierIds.length !== 50 || metadataIds.length !== verifierIds.length || metadataIds.some((id, index) => id !== verifierIds[index])) {
  throw new WebContractError(WEB_CONTRACT_ERROR_CODES.registryMismatch, "metadata-verifier-set=not-exact");
}

export const WEB_STATIC_CONTRACTS = Object.freeze(WEB_STATIC_CONTRACT_METADATA.map((metadata) =>
  Object.freeze({ ...metadata, verify: WEB_STATIC_VERIFIERS[metadata.id] })
));
