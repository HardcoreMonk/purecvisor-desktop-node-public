// Shared argument parser for the S1, S2, and S3 scenario scripts (BL-0022). The old getArg read only --name=value,
// so a spaced "--out <path>" was ignored and an earlier summary was overwritten. This parser takes --name value and
// --name=value, rejects unknown, duplicate, positional, and valueless arguments with a usage error (exit 2), and
// never accepts --password. It follows run-s4-lan-scenario.mjs parseArgs.
import { isAbsolute, relative, resolve } from "node:path";

export class ScenarioArgumentError extends Error {
  constructor(kind, name) {
    super(`argument error: ${kind} --${name}`);
    this.kind = kind;
    this.argument = name;
  }
}

export function parseScenarioArgs(argv, { values = [], booleans = [] } = {}) {
  const valueNames = new Set(values);
  const booleanNames = new Set(booleans);
  const parsed = { values: {}, flags: new Set() };
  const seen = new Set();
  for (let index = 0; index < argv.length; index += 1) {
    const token = argv[index];
    if (token === "--password" || token.startsWith("--password=")) throw new ScenarioArgumentError("rejected", "password");
    if (!token.startsWith("--") || token === "--") throw new ScenarioArgumentError("unknown", token.replace(/^-+/, "") || "argument");
    const equals = token.indexOf("=");
    const hasInline = equals !== -1;
    const name = hasInline ? token.slice(2, equals) : token.slice(2);
    if (!name || (!valueNames.has(name) && !booleanNames.has(name))) throw new ScenarioArgumentError("unknown", name || "argument");
    if (seen.has(name)) throw new ScenarioArgumentError("duplicate", name);
    seen.add(name);
    if (booleanNames.has(name)) {
      if (hasInline) throw new ScenarioArgumentError("unexpected value", name);
      parsed.flags.add(name);
      continue;
    }
    let raw = hasInline ? token.slice(equals + 1) : "";
    if (!hasInline) {
      const next = argv[index + 1];
      if (next === undefined || next.startsWith("--")) throw new ScenarioArgumentError("missing value", name);
      raw = next;
      index += 1;
    }
    if (raw === "") throw new ScenarioArgumentError("missing value", name);
    parsed.values[name] = raw;
  }
  return parsed;
}

export function usageText(script, { values = [], booleans = [] } = {}) {
  const options = [...values.map((name) => `[--${name} <value>]`), ...booleans.map((name) => `[--${name}]`)];
  return `usage: node scripts/${script} ${options.join(" ")}`;
}

// Prints the usage error without any argument value and exits 2, so a mistyped option never runs a scenario.
export function readScenarioArgs(argv, script, spec) {
  try {
    return parseScenarioArgs(argv, spec);
  } catch (error) {
    if (!(error instanceof ScenarioArgumentError)) throw error;
    process.stderr.write(`${error.message}\n${usageText(script, spec)}\n`);
    process.exit(2);
  }
}

// Local run date (yyyymmdd) for the default artifacts folder, the same form run-s4-lan-scenario.mjs uses.
export function formatRunDay(date = new Date()) {
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  return `${date.getFullYear()}${month}${day}`;
}

// Repository-relative form of a path for plan output, so the plan does not print a user profile path.
export function displayRepoPath(repoRoot, target) {
  const relativePath = relative(repoRoot, resolve(target));
  if (isAbsolute(relativePath)) return "<path-outside-repo>";
  const rel = relativePath.split("\\").join("/");
  if (rel === "" || rel === ".." || rel.startsWith("../")) return "<path-outside-repo>";
  return rel;
}
