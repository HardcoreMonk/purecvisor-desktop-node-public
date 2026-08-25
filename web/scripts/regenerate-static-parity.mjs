import { existsSync, mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";
import ts from "typescript";

const webRoot = dirname(dirname(fileURLToPath(import.meta.url)));

const paths = {
  appSource: join(webRoot, "src", "app.ts"),
  manifest: join(webRoot, "generated", "parity", "static-asset-parity.manifest.json")
};

const args = new Set(process.argv.slice(2));
const shouldWrite = args.has("--write");
const shouldCheck = args.has("--check");

if (shouldWrite === shouldCheck) {
  console.error("usage: node scripts/regenerate-static-parity.mjs --write|--check");
  process.exit(2);
}

function parseSource(path) {
  return ts.createSourceFile(path, readFileSync(path, "utf8"), ts.ScriptTarget.Latest, true);
}

function unwrapInitializer(node) {
  let current = node;
  while (ts.isAsExpression(current) || ts.isSatisfiesExpression(current)) {
    current = current.expression;
  }

  return current;
}

function getPropertyNameText(name) {
  if (ts.isIdentifier(name) || ts.isStringLiteral(name) || ts.isNumericLiteral(name)) {
    return name.text;
  }

  throw new Error(`unsupported property name: ${name.getText()}`);
}

function literalToValue(node) {
  if (ts.isStringLiteral(node) || ts.isNoSubstitutionTemplateLiteral(node)) {
    return node.text;
  }

  if (ts.isNumericLiteral(node)) {
    return Number(node.text);
  }

  if (node.kind === ts.SyntaxKind.TrueKeyword) {
    return true;
  }

  if (node.kind === ts.SyntaxKind.FalseKeyword) {
    return false;
  }

  if (ts.isObjectLiteralExpression(node)) {
    return objectLiteralToPlainObject(node);
  }

  throw new Error(`unsupported literal value: ${node.getText()}`);
}

function objectLiteralToPlainObject(node) {
  const result = {};

  for (const property of node.properties) {
    if (!ts.isPropertyAssignment(property)) {
      throw new Error(`unsupported object member: ${property.getText()}`);
    }

    result[getPropertyNameText(property.name)] = literalToValue(unwrapInitializer(property.initializer));
  }

  return result;
}

function hasExportModifier(node) {
  return Boolean(node.modifiers?.some((modifier) => modifier.kind === ts.SyntaxKind.ExportKeyword));
}

function extractExportedConstObject(sourceFile, exportName) {
  for (const statement of sourceFile.statements) {
    if (!ts.isVariableStatement(statement) || !hasExportModifier(statement)) {
      continue;
    }

    for (const declaration of statement.declarationList.declarations) {
      if (!ts.isIdentifier(declaration.name) || declaration.name.text !== exportName || !declaration.initializer) {
        continue;
      }

      const initializer = unwrapInitializer(declaration.initializer);
      if (!ts.isObjectLiteralExpression(initializer)) {
        throw new Error(`${exportName} must be an object literal`);
      }

      return objectLiteralToPlainObject(initializer);
    }
  }

  throw new Error(`exported const ${exportName} was not found`);
}

function buildManifest() {
  const appSource = parseSource(paths.appSource);
  const scaffold = extractExportedConstObject(appSource, "phase25WebConsoleScaffold");
  const localApiRoutes = extractExportedConstObject(appSource, "localApiRoutes");

  return {
    generatedBy: "src/generate-parity-manifest.ts",
    regeneratedBy: "scripts/regenerate-static-parity.mjs",
    phase: "phase25",
    servedAsset: scaffold.servedAsset,
    replacesServedAsset: true,
    indexScriptSrc: "/app.js",
    typeScriptEntry: "src/app.ts",
    servedTypeScriptEntry: scaffold.servedTypeScriptEntry,
    userVisibleFixtureEntry: "src/user-visible-fixtures.ts",
    userVisibleFixtureNames: ["emptyInventory", "runningVmAndJob", "unsupportedHost"],
    scaffold,
    localApiRoutes,
    regeneration: {
      source: "src/app.ts",
      output: "generated/parity/static-asset-parity.manifest.json",
      writeCommand: "npm run generate:parity",
      checkCommand: "npm run verify:parity",
      replacesServedAsset: true
    },
    browserFixture: {
      script: "scripts/verify-browser-fixture.mjs",
      command: "npm run browser:fixture",
      mode: "node-vm-minimal-dom",
      mutating: false,
      replacesServedAsset: true
    }
  };
}

function renderJson(value) {
  return `${JSON.stringify(value, null, 2)}\n`;
}

const nextText = renderJson(buildManifest());

if (shouldWrite) {
  mkdirSync(dirname(paths.manifest), { recursive: true });
  writeFileSync(paths.manifest, nextText, "utf8");
  console.log("static parity manifest regenerated");
} else {
  if (!existsSync(paths.manifest)) {
    console.error("static parity verification failed: generated manifest is missing");
    process.exit(1);
  }

  const currentText = readFileSync(paths.manifest, "utf8");
  if (currentText !== nextText) {
    console.error("static parity verification failed: committed manifest is stale; run npm run generate:parity");
    process.exit(1);
  }

  console.log("static parity manifest is current");
}
