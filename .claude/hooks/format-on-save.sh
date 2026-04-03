#!/bin/bash
# Auto-formats files after Claude edits them.
# Used as a PostToolUse hook for Edit|Write operations.
# Auto-detects formatters — requires both the binary AND a config file to activate.

# Requires jq for JSON parsing
if ! command -v jq >/dev/null 2>&1; then
  exit 0
fi

INPUT=$(cat)
FILE_PATH=$(echo "$INPUT" | jq -r '.tool_input.file_path // empty')

if [ -z "$FILE_PATH" ] || [ ! -f "$FILE_PATH" ]; then
  exit 0
fi

EXTENSION="${FILE_PATH##*.}"
FORMATTED=false

# Find the project root (nearest directory with package.json, pyproject.toml, Cargo.toml, go.mod, or .git)
find_project_root() {
  local dir="$PWD"
  while [ "$dir" != "/" ]; do
    if [ -f "$dir/package.json" ] || [ -f "$dir/pyproject.toml" ] || [ -f "$dir/Cargo.toml" ] || [ -f "$dir/go.mod" ] || [ -d "$dir/.git" ]; then
      echo "$dir"
      return
    fi
    dir=$(dirname "$dir")
  done
  echo "$PWD"
}

ROOT=$(find_project_root)

# --- Biome (JS/TS all-in-one — check first, it's faster than Prettier) ---
if [ "$FORMATTED" = false ] && [ -f "$ROOT/node_modules/.bin/biome" ] && [ -f "$ROOT/biome.json" -o -f "$ROOT/biome.jsonc" ]; then
  case "$EXTENSION" in
    js|jsx|ts|tsx|json|css)
      npx biome format --write "$FILE_PATH" 2>/dev/null && FORMATTED=true
      ;;
  esac
fi

# --- Prettier (Node.js / TypeScript) ---
if [ "$FORMATTED" = false ] && [ -f "$ROOT/node_modules/.bin/prettier" ]; then
  # Check for Prettier config (any common format)
  HAS_PRETTIER_CONFIG=false
  for cfg in .prettierrc .prettierrc.json .prettierrc.yml .prettierrc.yaml .prettierrc.js .prettierrc.cjs .prettierrc.mjs .prettierrc.toml prettier.config.js prettier.config.cjs prettier.config.mjs; do
    if [ -f "$ROOT/$cfg" ]; then
      HAS_PRETTIER_CONFIG=true
      break
    fi
  done
  # Also check package.json for "prettier" key
  if [ "$HAS_PRETTIER_CONFIG" = false ] && [ -f "$ROOT/package.json" ] && grep -q '"prettier"' "$ROOT/package.json" 2>/dev/null; then
    HAS_PRETTIER_CONFIG=true
  fi

  if [ "$HAS_PRETTIER_CONFIG" = true ]; then
    case "$EXTENSION" in
      js|jsx|ts|tsx|json|css|scss|md|yaml|yml|html)
        npx prettier --write "$FILE_PATH" 2>/dev/null && FORMATTED=true
        ;;
    esac
  fi
fi

# --- Ruff (Python — modern replacement for Black + isort) ---
if [ "$FORMATTED" = false ] && command -v ruff >/dev/null 2>&1; then
  HAS_RUFF_CONFIG=false
  if [ -f "$ROOT/ruff.toml" ] || [ -f "$ROOT/.ruff.toml" ]; then
    HAS_RUFF_CONFIG=true
  elif [ -f "$ROOT/pyproject.toml" ] && grep -q '\[tool\.ruff\]' "$ROOT/pyproject.toml" 2>/dev/null; then
    HAS_RUFF_CONFIG=true
  fi

  if [ "$HAS_RUFF_CONFIG" = true ]; then
    case "$EXTENSION" in
      py)
        ruff format "$FILE_PATH" 2>/dev/null
        ruff check --fix "$FILE_PATH" 2>/dev/null
        FORMATTED=true
        ;;
    esac
  fi
fi

# --- Black + isort (Python — fallback if Ruff not configured) ---
if [ "$FORMATTED" = false ] && command -v black >/dev/null 2>&1; then
  HAS_BLACK_CONFIG=false
  if [ -f "$ROOT/pyproject.toml" ] && grep -q '\[tool\.black\]' "$ROOT/pyproject.toml" 2>/dev/null; then
    HAS_BLACK_CONFIG=true
  fi

  if [ "$HAS_BLACK_CONFIG" = true ]; then
    case "$EXTENSION" in
      py)
        black --quiet "$FILE_PATH" 2>/dev/null
        command -v isort >/dev/null 2>&1 && isort --quiet "$FILE_PATH" 2>/dev/null
        FORMATTED=true
        ;;
    esac
  fi
fi

# --- Rust (rustfmt is standard — no config check needed) ---
if [ "$FORMATTED" = false ] && command -v rustfmt >/dev/null 2>&1; then
  case "$EXTENSION" in
    rs)
      rustfmt "$FILE_PATH" 2>/dev/null && FORMATTED=true
      ;;
  esac
fi

# --- Go (gofmt is standard — no config check needed) ---
if [ "$FORMATTED" = false ] && command -v gofmt >/dev/null 2>&1; then
  case "$EXTENSION" in
    go)
      gofmt -w "$FILE_PATH" 2>/dev/null && FORMATTED=true
      ;;
  esac
fi

# --- dotnet format (C# — requires .editorconfig in the project hierarchy) ---
if [ "$FORMATTED" = false ] && command -v dotnet >/dev/null 2>&1; then
  case "$EXTENSION" in
    cs)
      NORM_PATH=$(realpath "$FILE_PATH" 2>/dev/null || echo "$FILE_PATH" | tr '\\' '/')

      # Walk up from the file's directory to find a solution or project file.
      # First pass prefers .slnx/.sln so all projects share one formatting run.
      # Second pass falls back to the nearest .csproj if no solution is found.
      CSPROJ=""
      for PATTERN in "*.slnx" "*.sln" "*.csproj"; do
        SEARCH_DIR=$(dirname "$NORM_PATH")
        while [ "$SEARCH_DIR" != "/" ] && [ -n "$SEARCH_DIR" ]; do
          FOUND=$(find "$SEARCH_DIR" -maxdepth 1 -name "$PATTERN" 2>/dev/null | head -1)
          if [ -n "$FOUND" ]; then
            CSPROJ=$(realpath "$FOUND" 2>/dev/null || echo "$FOUND")
            break 2
          fi
          SEARCH_DIR=$(dirname "$SEARCH_DIR")
        done
      done

      # Walk up from the file's directory to find .editorconfig
      SEARCH_DIR=$(dirname "$NORM_PATH")
      HAS_EDITORCONFIG=false
      while [ "$SEARCH_DIR" != "/" ] && [ -n "$SEARCH_DIR" ]; do
        if [ -f "$SEARCH_DIR/.editorconfig" ]; then
          HAS_EDITORCONFIG=true
          break
        fi
        SEARCH_DIR=$(dirname "$SEARCH_DIR")
      done

      if [ -n "$CSPROJ" ] && [ "$HAS_EDITORCONFIG" = true ]; then
        PROJ_DIR=$(dirname "$CSPROJ")
        REL_PATH="${NORM_PATH#${PROJ_DIR}/}"
        dotnet format "$CSPROJ" --include "$REL_PATH" --no-restore 2>/dev/null && FORMATTED=true
      fi
      ;;
  esac
fi

exit 0