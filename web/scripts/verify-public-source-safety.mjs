import crypto from "node:crypto";
import { spawnSync } from "node:child_process";
import fs from "node:fs";
import path from "node:path";
import process from "node:process";
import { fileURLToPath, pathToFileURL } from "node:url";

const CONTRACT = "pcv-public-source-safety-v1";
const SYNTHETIC_MARKER = "public-safety: synthetic-rfc1918";
const PUBLIC_REPOSITORY = "HardcoreMonk/purecvisor-desktop-node-public";
const PRIVATE_ARCHIVE_REPOSITORY = PUBLIC_REPOSITORY.replace(/-public$/, "");
const SAFE_PROFILE_NAMES = new Set(["operator", "public", "default", "all"]);
const SAFE_EMAIL_DOMAINS = new Set(["users.noreply.github.com"]);
const BINARY_ARCHIVE_EXTENSIONS = new Set([".7z", ".gz", ".p12", ".pfx", ".tar", ".tgz", ".zip"]);
const PUBLIC_MEDIA_EXTENSIONS = new Set([".gif", ".ico", ".jpeg", ".jpg", ".png", ".webp", ".woff", ".woff2"]);
const BOUNDARY_DOCUMENTS = Object.freeze({
  "LICENSE": {
    missingRule: "boundary.license-missing",
    invalidRule: "boundary.license-invalid",
    patterns: [
      /all rights reserved/i,
      /no permission is granted/i,
      /reproduce/i,
      /modify/i,
      /redistribute/i,
      /sublicense/i,
      /sell/i
    ]
  },
  "SECURITY.md": {
    missingRule: "boundary.security-missing",
    invalidRule: "boundary.security-invalid",
    patterns: [/security/i, /private vulnerability reporting/i]
  },
  "docs/PUBLIC_SOURCE_AUTHORITY.md": {
    missingRule: "boundary.authority-missing",
    invalidRule: "boundary.authority-invalid",
    patterns: [
      new RegExp(escapeRegExp(PUBLIC_REPOSITORY), "i"),
      /public_trusted_signing=false/,
      /external_stable_publication=false/
    ]
  }
});

function escapeRegExp(value) {
  return value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
}

function normalizeTrackedPath(relativePath) {
  return relativePath.replaceAll("\\", "/");
}

function isPathEscape(relativePath) {
  const normalized = normalizeTrackedPath(relativePath);
  return normalized.length === 0
    || normalized.startsWith("/")
    || /^[A-Za-z]:\//.test(normalized)
    || normalized.split("/").includes("..");
}

function isSyntheticFixturePath(relativePath) {
  const normalized = normalizeTrackedPath(relativePath).toLowerCase();
  return normalized.includes("/test")
    || normalized.startsWith("test")
    || normalized.includes("/fixture")
    || normalized.endsWith(".schema.json")
    || normalized.startsWith("src/")
    || normalized.startsWith("web/");
}

function lineNumberAt(content, index) {
  let line = 1;
  for (let position = 0; position < index; position += 1) {
    if (content.charCodeAt(position) === 10) line += 1;
  }
  return line;
}

function firstMatchLine(content, expression) {
  const match = expression.exec(content);
  return match ? lineNumberAt(content, match.index) : null;
}

function privateIpv4Line(content) {
  const expression = /\b(?:10(?:\.\d{1,3}){3}|192\.168(?:\.\d{1,3}){2}|172\.(?:1[6-9]|2\d|3[01])(?:\.\d{1,3}){2})\b/g;
  const lines = content.split(/\r?\n/);
  for (let index = 0; index < lines.length; index += 1) {
    expression.lastIndex = 0;
    if (expression.test(lines[index])) return index + 1;
  }
  return null;
}

function hasMarkedPrivateIpv4(content) {
  const expression = /\b(?:10(?:\.\d{1,3}){3}|192\.168(?:\.\d{1,3}){2}|172\.(?:1[6-9]|2\d|3[01])(?:\.\d{1,3}){2})\b/g;
  const lines = content.split(/\r?\n/);
  for (let index = 0; index < lines.length; index += 1) {
    expression.lastIndex = 0;
    if (!expression.test(lines[index])) continue;
    const markerPresent = lines[index].includes(SYNTHETIC_MARKER)
      || (index > 0 && lines[index - 1].includes(SYNTHETIC_MARKER));
    if (!markerPresent) return false;
  }
  return true;
}

function isSchemaIdentifierMatch(content, relativePath, matchIndex) {
  if (!normalizeTrackedPath(relativePath).toLowerCase().endsWith(".schema.json")) return false;
  const identifierExpression = /"\$id"\s*:\s*"(?<value>(?:\\.|[^"\\])*)"/g;
  for (const identifier of content.matchAll(identifierExpression)) {
    const value = identifier.groups?.value ?? "";
    const valueOffset = identifier[0].indexOf(value);
    const valueStart = identifier.index + valueOffset;
    if (matchIndex >= valueStart && matchIndex < valueStart + value.length) return true;
  }
  return false;
}

function privateHostnameLine(content, relativePath) {
  const expression = /https?:\/\/[A-Z0-9._-]+\.(?:internal|local|corp|lan)(?=[:/\s]|$)/gi;
  for (const match of content.matchAll(expression)) {
    if (!isSchemaIdentifierMatch(content, relativePath, match.index)) {
      return lineNumberAt(content, match.index);
    }
  }
  return null;
}

function personalEmailLine(content) {
  const expression = /\b[A-Z0-9._%+-]+@(?<domain>(?:[A-Z0-9-]+\.)+[A-Z]{2,})\b/gi;
  for (const match of content.matchAll(expression)) {
    if (!SAFE_EMAIL_DOMAINS.has(match.groups.domain.toLowerCase())) {
      return lineNumberAt(content, match.index);
    }
  }
  return null;
}

function sortFindings(findings) {
  return findings.sort((left, right) => left.ruleId.localeCompare(right.ruleId)
    || left.path.localeCompare(right.path)
    || left.line - right.line);
}

function addFinding(findings, seen, ruleId, relativePath, line = 1) {
  const normalizedPath = normalizeTrackedPath(relativePath);
  const key = `${ruleId}\0${normalizedPath}`;
  if (seen.has(key)) return;
  seen.add(key);
  findings.push({ ruleId, path: normalizedPath, line });
}

function scanText(findings, seen, relativePath, content, forbiddenIdentities) {
  const profileExpression = /\b[A-Za-z]:[\\/]+Users[\\/]+([A-Za-z0-9._-]+)/giu;
  for (const match of content.matchAll(profileExpression)) {
    if (!SAFE_PROFILE_NAMES.has(match[1].toLowerCase())) {
      addFinding(findings, seen, "identity.absolute-profile", relativePath, lineNumberAt(content, match.index));
    }
  }

  for (const identity of forbiddenIdentities) {
    if (!identity) continue;
    const index = content.toLocaleLowerCase("en-US").indexOf(identity.toLocaleLowerCase("en-US"));
    if (index >= 0) addFinding(findings, seen, "identity.forbidden-token", relativePath, lineNumberAt(content, index));
  }

  const privateLine = privateIpv4Line(content);
  if (privateLine !== null) {
    if (!isSyntheticFixturePath(relativePath)) {
      addFinding(findings, seen, "network.observed-private-endpoint", relativePath, privateLine);
    } else if (!hasMarkedPrivateIpv4(content)) {
      addFinding(findings, seen, "network.synthetic-marker-missing", relativePath, privateLine);
    }
  }

  const emailLine = personalEmailLine(content);
  if (emailLine !== null) addFinding(findings, seen, "identity.personal-email", relativePath, emailLine);

  const privateHostLine = privateHostnameLine(content, relativePath);
  if (privateHostLine !== null) addFinding(findings, seen, "network.private-hostname", relativePath, privateHostLine);

  const credentialUrlLine = firstMatchLine(content, /https?:\/\/[^\s/:@]+:[^\s/@]+@/gi);
  if (credentialUrlLine !== null) addFinding(findings, seen, "secret.credential-url", relativePath, credentialUrlLine);

  const privateKeyLine = firstMatchLine(content, /-----BEGIN (?:[A-Z0-9]+ )*PRIVATE KEY-----/gi);
  if (privateKeyLine !== null) addFinding(findings, seen, "secret.private-key", relativePath, privateKeyLine);

  const privateArchiveExpression = new RegExp(`${escapeRegExp(PRIVATE_ARCHIVE_REPOSITORY)}(?!-public)(?=[^A-Za-z0-9-]|$)`, "gi");
  const privateArchiveLine = firstMatchLine(content, privateArchiveExpression);
  if (privateArchiveLine !== null) addFinding(findings, seen, "provider.private-archive", relativePath, privateArchiveLine);
}

function scanBoundaryDocuments(findings, seen, repositoryRoot, trackedSet) {
  for (const [relativePath, contract] of Object.entries(BOUNDARY_DOCUMENTS)) {
    if (!trackedSet.has(relativePath)) {
      addFinding(findings, seen, contract.missingRule, relativePath);
      continue;
    }
    let content;
    try {
      content = fs.readFileSync(path.join(repositoryRoot, ...relativePath.split("/")), "utf8");
    } catch {
      addFinding(findings, seen, contract.invalidRule, relativePath);
      continue;
    }
    if (contract.patterns.some((pattern) => !pattern.test(content))) {
      addFinding(findings, seen, contract.invalidRule, relativePath);
    }
  }
}

export function scanPublicSourceTree({
  repositoryRoot,
  trackedPaths = [],
  additionalTrackedPaths = [],
  forbiddenIdentities = [],
  requireBoundaryDocuments = false
} = {}) {
  if (!repositoryRoot) throw new TypeError("repositoryRoot is required");
  const resolvedRoot = path.resolve(repositoryRoot);
  const allTrackedPaths = [...trackedPaths, ...additionalTrackedPaths];
  const trackedSet = new Set(allTrackedPaths.map(normalizeTrackedPath));
  const findings = [];
  const seen = new Set();

  for (const rawRelativePath of allTrackedPaths) {
    const relativePath = normalizeTrackedPath(rawRelativePath);
    if (isPathEscape(relativePath)) {
      addFinding(findings, seen, "repository.path-escape", relativePath || "[empty]");
      continue;
    }
    if (relativePath.split("/").some((part) => part.toLowerCase() === ".git")) {
      addFinding(findings, seen, "repository.nested-git", relativePath);
    }
    if (BINARY_ARCHIVE_EXTENSIONS.has(path.extname(relativePath).toLowerCase())) {
      addFinding(findings, seen, "repository.binary-archive", relativePath);
    }

    const absolutePath = path.resolve(resolvedRoot, ...relativePath.split("/"));
    const rootRelative = path.relative(resolvedRoot, absolutePath);
    if (rootRelative === ".." || rootRelative.startsWith(`..${path.sep}`) || path.isAbsolute(rootRelative)) {
      addFinding(findings, seen, "repository.path-escape", relativePath);
      continue;
    }

    let stat;
    try {
      stat = fs.lstatSync(absolutePath);
    } catch {
      addFinding(findings, seen, "repository.file-unreadable", relativePath);
      continue;
    }
    if (stat.isSymbolicLink()) {
      addFinding(findings, seen, "repository.symlink", relativePath);
      continue;
    }
    if (stat.isDirectory()) {
      addFinding(findings, seen, "repository.gitlink-or-directory", relativePath);
      continue;
    }
    if (!stat.isFile()) {
      addFinding(findings, seen, "repository.non-regular-file", relativePath);
      continue;
    }
    if (PUBLIC_MEDIA_EXTENSIONS.has(path.extname(relativePath).toLowerCase())) continue;

    let content;
    try {
      content = fs.readFileSync(absolutePath, "utf8");
    } catch {
      addFinding(findings, seen, "repository.file-unreadable", relativePath);
      continue;
    }
    if (content.includes("\0")) {
      addFinding(findings, seen, "repository.binary-content", relativePath);
      continue;
    }
    scanText(findings, seen, relativePath, content, forbiddenIdentities);
  }

  if (requireBoundaryDocuments) scanBoundaryDocuments(findings, seen, resolvedRoot, trackedSet);
  return { contract: CONTRACT, findings: sortFindings(findings) };
}

export function formatSafetyReport(result) {
  const findings = result.findings.map((finding) => ({
    line: finding.line,
    path: finding.path,
    rule_id: finding.ruleId
  }));
  const payload = {
    contract: CONTRACT,
    finding_count: findings.length,
    findings
  };
  const reportSha256 = crypto.createHash("sha256").update(JSON.stringify(payload)).digest("hex");
  return { ...payload, report_sha256: reportSha256 };
}

export function listTrackedFiles(repositoryRoot) {
  const child = spawnSync("git", ["-C", repositoryRoot, "ls-files", "-z"], {
    encoding: "utf8",
    shell: false,
    windowsHide: true
  });
  if (child.status !== 0) throw new Error("tracked-file-enumeration-failed");
  return child.stdout.split("\0").filter(Boolean);
}

function parseCli(arguments_) {
  const options = {
    repositoryRoot: path.resolve(path.dirname(fileURLToPath(import.meta.url)), "../.."),
    requireBoundaryDocuments: false
  };
  for (let index = 0; index < arguments_.length; index += 1) {
    const argument = arguments_[index];
    if (argument === "--root") {
      const root = arguments_[index + 1];
      if (!root) throw new Error("root-argument-missing");
      options.repositoryRoot = path.resolve(root);
      index += 1;
    } else if (argument === "--require-boundaries") {
      options.requireBoundaryDocuments = true;
    } else {
      throw new Error(`unknown-argument:${argument}`);
    }
  }
  return options;
}

export function runCli(arguments_ = process.argv.slice(2)) {
  const options = parseCli(arguments_);
  const forbiddenIdentities = (process.env.PCV_PUBLIC_SAFETY_FORBIDDEN_IDENTITIES ?? "")
    .split(";")
    .map((value) => value.trim())
    .filter(Boolean);
  const result = scanPublicSourceTree({
    ...options,
    forbiddenIdentities,
    trackedPaths: listTrackedFiles(options.repositoryRoot)
  });
  process.stdout.write(`${JSON.stringify(formatSafetyReport(result))}\n`);
  return result.findings.length === 0 ? 0 : 1;
}

const directInvocation = process.argv[1]
  && import.meta.url === pathToFileURL(path.resolve(process.argv[1])).href;
if (directInvocation) {
  try {
    process.exitCode = runCli();
  } catch (error) {
    process.stderr.write(`public-source-safety:error=${error instanceof Error ? error.message : "unknown"}\n`);
    process.exitCode = 2;
  }
}
