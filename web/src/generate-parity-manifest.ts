import { localApiRoutes, phase25WebConsoleScaffold } from "./app";

export type StaticAssetParityManifest = {
  generatedBy: "src/generate-parity-manifest.ts";
  regeneratedBy: "scripts/regenerate-static-parity.mjs";
  phase: "phase25";
  servedAsset: "app.js";
  replacesServedAsset: true;
  indexScriptSrc: "/app.js";
  typeScriptEntry: "src/app.ts";
  servedTypeScriptEntry: "src/served-app.ts";
  userVisibleFixtureEntry: "src/user-visible-fixtures.ts";
  userVisibleFixtureNames: readonly ["emptyInventory", "runningVmAndJob", "unsupportedHost"];
  scaffold: typeof phase25WebConsoleScaffold;
  localApiRoutes: typeof localApiRoutes;
  regeneration: {
    source: "src/app.ts";
    output: "generated/parity/static-asset-parity.manifest.json";
    writeCommand: "npm run generate:parity";
    checkCommand: "npm run verify:parity";
    replacesServedAsset: true;
  };
  browserFixture: {
    script: "scripts/verify-browser-fixture.mjs";
    command: "npm run browser:fixture";
    mode: "node-vm-minimal-dom";
    mutating: false;
    replacesServedAsset: true;
  };
};

export function buildStaticAssetParityManifest(): StaticAssetParityManifest {
  return {
    generatedBy: "src/generate-parity-manifest.ts",
    regeneratedBy: "scripts/regenerate-static-parity.mjs",
    phase: "phase25",
    servedAsset: phase25WebConsoleScaffold.servedAsset,
    replacesServedAsset: true,
    indexScriptSrc: "/app.js",
    typeScriptEntry: "src/app.ts",
    servedTypeScriptEntry: phase25WebConsoleScaffold.servedTypeScriptEntry,
    userVisibleFixtureEntry: "src/user-visible-fixtures.ts",
    userVisibleFixtureNames: ["emptyInventory", "runningVmAndJob", "unsupportedHost"],
    scaffold: phase25WebConsoleScaffold,
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

export const staticAssetParityManifest = buildStaticAssetParityManifest();
