// Ported from purecvisor eslint.config.js (Apache-2.0, same author) for the Desktop Node web root (ADR-0018).
// Lints the shipped classic scripts of the Single Edge structure: app.bundle.js, i18n.js, sw.js. The legacy
// TypeScript output web/app.js is ignored until campaign single-edge-frontend-structure-20261010 Task 16 removes it.
import js from "@eslint/js";
import globals from "globals";

export default [
  {
    ignores: ["vendor/**", "app.js", "node_modules/**", "generated/**", "mockups/**", "samples/**", "src/**"]
  },
  js.configs.recommended,
  {
    files: ["app.bundle.js", "i18n.js"],
    languageOptions: {
      ecmaVersion: 2022,
      sourceType: "script",
      globals: {
        ...globals.browser
      }
    },
    rules: {
      // Modules share one classic-script scope through window.PCV; cross-file globals are intended.
      "no-undef": "off",
      "no-unused-vars": ["warn", {
        args: "none",
        caughtErrors: "none",
        varsIgnorePattern: "^(PCV_UI_SOURCE_SHA1|_)"
      }],
      "no-empty": ["error", { allowEmptyCatch: true }]
    }
  },
  {
    files: ["sw.js"],
    languageOptions: {
      ecmaVersion: 2022,
      sourceType: "script",
      globals: {
        ...globals.serviceworker
      }
    },
    rules: {
      "no-undef": "off",
      "no-empty": ["error", { allowEmptyCatch: true }]
    }
  }
];
