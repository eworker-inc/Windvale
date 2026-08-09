#!/usr/bin/env bash
set -uo pipefail

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-native-os-process-policy.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-native-os-process-policy.*)
            rm -rf -- "$temporary_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $temporary_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

object="$temporary_directory/04-process-policy.wvo"
existing="$temporary_directory/Existing.wvo"

"$script_directory/Build-Os-Process-Policy-Object.sh" "$object" \
    >/dev/null 2>&1 || exit 1
if [[ $(wc -c < "$object") -ne 129310 ]] ||
    ! printf '%s  %s\n' \
        '35d751147a7285fb926ba68e77da4ef554bcf68a58963520153f23ea3e8c4678' \
        "$object" | sha256sum --check --strict --quiet; then
    exit 1
fi
"$script_directory/Verify-Wvo.sh" "$object" >/dev/null 2>&1 || exit 1

printf '%s\n' preserved > "$existing"
existing_sha256=$(sha256sum -- "$existing") || exit 1
existing_sha256=${existing_sha256%% *}
if "$script_directory/Build-Os-Process-Policy-Object.sh" "$existing" \
    >/dev/null 2>&1; then
    exit 1
fi
printf '%s  %s\n' "$existing_sha256" "$existing" |
    sha256sum --check --strict --quiet || exit 1

echo 'Tests: 2, Passed: 2, Failed: 0'
