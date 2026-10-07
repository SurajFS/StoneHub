#!/usr/bin/env bash
# Deploys StoneHub.Api to the EC2 host, matching how prod runs today: a self-contained
# linux-x64 build in /opt/stonehub, started by the `stonehub-api` systemd service behind Caddy.
#
#   deploy/deploy.sh            build HEAD, upload, swap, restart, smoke-check
#   SKIP_TESTS=1 deploy/deploy.sh
#
# Settings come from deploy/deploy.env (copy deploy.env.example). Secrets never pass through
# this script: they stay in the server's systemd unit / appsettings, which are not overwritten.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
# shellcheck source=/dev/null
source "$REPO_ROOT/deploy/deploy.env"
: "${DEPLOY_HOST:?set in deploy/deploy.env}" "${DEPLOY_SSH_KEY:?}" "${DEPLOY_PUBLIC_URL:?}"

readonly SERVICE_NAME="stonehub-api"
readonly APP_DIR="/opt/stonehub"
readonly RELEASES_DIR="/opt/stonehub-releases"
readonly BACKUPS_TO_KEEP=3
readonly SMOKE_TIMEOUT_SECONDS=90
readonly API_PROJECT="StoneHub/src/Api/StoneHub.Api/StoneHub.Api.csproj"

log() { printf '\n==> %s\n' "$*"; }
fail() { printf '\nDEPLOY FAILED: %s\n' "$*" >&2; exit 1; }
remote() { ssh -i "$DEPLOY_SSH_KEY" -o BatchMode=yes "$DEPLOY_HOST" "$@"; }

# Restores the backup taken just before the swap. EF migrations applied on startup are not
# reverted; they are additive, so the previous build keeps working against the newer schema.
rollback() {
  log "$1 — rolling back"
  remote "sudo bash -s" <<EOF
set -euo pipefail
backup="\$(cat $RELEASES_DIR/LAST_BACKUP)"
systemctl stop $SERVICE_NAME || true
rm -rf "$APP_DIR"
cp -a "\$backup" "$APP_DIR"
systemctl start $SERVICE_NAME
journalctl -u $SERVICE_NAME --no-pager -n 40 -o cat
EOF
  fail "$1; previous build restored (see log above)"
}

cd "$REPO_ROOT"

# --- 1. Only deploy what's committed and pushed, so prod always maps to a real commit ------
log "Checking git state"
[[ -z "$(git status --porcelain)" ]] || fail "working tree has uncommitted changes"
git fetch --quiet origin
branch="$(git rev-parse --abbrev-ref HEAD)"
[[ "$(git rev-parse HEAD)" == "$(git rev-parse "origin/$branch")" ]] \
  || fail "HEAD is not pushed to origin/$branch"
sha="$(git rev-parse --short HEAD)"
echo "Deploying $sha ($(git log -1 --format=%s))"

# --- 2. Test + publish ---------------------------------------------------------------------
if [[ "${SKIP_TESTS:-0}" != "1" ]]; then
  log "Running tests"
  dotnet test StoneHub/StoneHub.slnx --nologo -v quiet
fi

work="$(mktemp -d)"
trap 'rm -rf "$work"' EXIT
log "Publishing self-contained linux-x64"
dotnet publish "$API_PROJECT" -c Release -r linux-x64 --self-contained true \
  -o "$work/publish" --nologo -v quiet
# The server keeps its own config; never ship the repo's placeholder appsettings over it.
rm -f "$work/publish"/appsettings*.json
echo "$sha" > "$work/publish/REVISION"
tar -C "$work/publish" -czf "$work/release.tgz" .

# --- 3. Upload + swap ----------------------------------------------------------------------
log "Uploading release ($(du -h "$work/release.tgz" | cut -f1))"
scp -i "$DEPLOY_SSH_KEY" -o BatchMode=yes -q "$work/release.tgz" "$DEPLOY_HOST:/tmp/stonehub-$sha.tgz"

log "Installing on server"
remote "sudo bash -s" <<EOF || install_failed=1
set -euo pipefail
# Cleared first so a failure before the backup below can never roll back to an older one.
rm -f "$RELEASES_DIR/LAST_BACKUP"
ts="\$(date -u +%Y%m%d%H%M%S)"
owner="\$(stat -c %U:%G $APP_DIR)"
mkdir -p "$RELEASES_DIR/$sha"
tar -C "$RELEASES_DIR/$sha" --no-same-owner -xzf "/tmp/stonehub-$sha.tgz"
rm -f "/tmp/stonehub-$sha.tgz"

# Full copy of what's running now, for rollback (includes server-only config files).
cp -a "$APP_DIR" "$RELEASES_DIR/backup-\$ts"
echo "$RELEASES_DIR/backup-\$ts" > "$RELEASES_DIR/LAST_BACKUP"

systemctl stop $SERVICE_NAME
# Overlay, not replace: files that exist only on the server (config) are preserved.
cp -a "$RELEASES_DIR/$sha/." "$APP_DIR/"
chown -R "\$owner" "$APP_DIR"
chmod +x "$APP_DIR/StoneHub.Api"
systemctl start $SERVICE_NAME

# Prune old backups and unpacked releases.
ls -1dt "$RELEASES_DIR"/backup-* | tail -n +$((BACKUPS_TO_KEEP + 1)) | xargs -r rm -rf
find "$RELEASES_DIR" -mindepth 1 -maxdepth 1 -type d ! -name 'backup-*' ! -name "$sha" -exec rm -rf {} +
EOF
if [[ "${install_failed:-0}" == "1" ]]; then
  # Only roll back if a backup was actually taken for this run (swap had started).
  if remote "test -f $RELEASES_DIR/LAST_BACKUP"; then
    rollback "install step failed"
  fi
  fail "install step failed before the running build was touched"
fi

# --- 4. Smoke check, roll back on failure --------------------------------------------------
status_of() { curl -s -o /dev/null -w '%{http_code}' --max-time 10 "$DEPLOY_PUBLIC_URL$1" || true; }

log "Smoke-checking $DEPLOY_PUBLIC_URL"
deadline=$((SECONDS + SMOKE_TIMEOUT_SECONDS))
healthy=0
while (( SECONDS < deadline )); do
  # Public endpoint must answer 200; an auth-only endpoint from the newest module must
  # answer 401 (exists, needs a token) rather than 404 (old build still running).
  if [[ "$(status_of /api/v1/categories)" == "200" && "$(status_of /api/v1/billing/me)" == "401" ]]; then
    healthy=1
    break
  fi
  sleep 3
done

if (( healthy == 0 )); then
  rollback "new build did not become healthy"
fi

log "Deployed $sha — live at $DEPLOY_PUBLIC_URL"
