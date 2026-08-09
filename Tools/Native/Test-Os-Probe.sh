#!/usr/bin/env bash
set -uo pipefail

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-native-os-probe.XXXXXXXX") || exit 1
output="$temporary_directory/Probe40.efi"
standard_output="$temporary_directory/Build.out"
standard_error="$temporary_directory/Build.err"
repeat_output="$temporary_directory/Repeat.out"
repeat_error="$temporary_directory/Repeat.err"
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-native-os-probe.*)
            rm -rf -- "$temporary_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $temporary_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

if ! "$script_directory/Build-Os-Probe.sh" "$output" >"$standard_output" 2>"$standard_error"; then
    cat -- "$standard_output" "$standard_error" >&2
    exit 1
fi
if [[ -s $standard_error ]] ||
    ! grep -Fxq 'windvale-os-probe-native-build 40' "$standard_output" ||
    ! grep -Fxq 'scenario=normal' "$standard_output" ||
    [[ $(wc -c < "$output") -ne 683008 ]] ||
    ! printf '%s  %s\n' \
        '080b4d669e9a11fdc802bf7197ae5a044978b6ba39741b2b1c832296987f74d9' \
        "$output" | sha256sum --check --strict --quiet; then
    cat -- "$standard_output" "$standard_error" >&2
    exit 1
fi

"$script_directory/Build-Os-Probe.sh" "$output" >"$repeat_output" 2>"$repeat_error"
repeat_status=$?
if [[ $repeat_status -ne 1 ]] ||
    ! grep -Fxq 'The native Probe 40 output already exists.' "$repeat_error" ||
    [[ $(wc -c < "$output") -ne 683008 ]] ||
    ! printf '%s  %s\n' \
        '080b4d669e9a11fdc802bf7197ae5a044978b6ba39741b2b1c832296987f74d9' \
        "$output" | sha256sum --check --strict --quiet ||
    find "$temporary_directory" -maxdepth 1 -name '.windvale-os-probe-native.*' -print -quit |
        grep -q .; then
    cat -- "$repeat_output" "$repeat_error" >&2
    exit 1
fi

echo 'Tests: 2, Passed: 2, Failed: 0'
