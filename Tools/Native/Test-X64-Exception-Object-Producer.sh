#!/usr/bin/env bash
set -uo pipefail

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-native-x64-exception-object.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-native-x64-exception-object.*)
            rm -rf -- "$temporary_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $temporary_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

output="$temporary_directory/09-exceptions.wvo"
existing="$temporary_directory/Existing.wvo"
invalid="$temporary_directory/Invalid.bin"

"$script_directory/Produce-X64-Exception-Object.sh" "$output" >/dev/null 2>&1 || exit 1
if [[ $(wc -c < "$output") -ne 483 ]] ||
    ! printf '%s  %s\n' \
        '9caeb7ce353bca33e3bbac729ecca0423d59f8ce6b65ccd6b54fa53c381d617c' \
        "$output" | sha256sum --check --strict --quiet; then
    exit 1
fi
"$script_directory/Verify-Wvo.sh" "$output" >/dev/null 2>&1 || exit 1

printf '%s\n' preserved > "$existing"
existing_sha256=$(sha256sum -- "$existing") || exit 1
existing_sha256=${existing_sha256%% *}
if "$script_directory/Produce-X64-Exception-Object.sh" "$existing" >/dev/null 2>&1; then
    exit 1
fi
printf '%s  %s\n' "$existing_sha256" "$existing" |
    sha256sum --check --strict --quiet || exit 1

"$script_directory/Produce-X64-Exception-Object.sh" "$invalid" >/dev/null 2>&1
invalid_status=$?
if [[ $invalid_status -ne 64 || -e $invalid ]]; then
    exit 1
fi

echo 'Tests: 3, Passed: 3, Failed: 0'
