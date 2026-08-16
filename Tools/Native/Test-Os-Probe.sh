#!/usr/bin/env bash
set -uo pipefail

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-native-os-probe.XXXXXXXX") || exit 1
normal="$temporary_directory/Normal.efi"
invalid_opcode="$temporary_directory/Invalid-Opcode.efi"
general_protection="$temporary_directory/General-Protection.efi"
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

verify_output() {
    local path=$1
    local digest=$2
    local bytes=$3
    [[ $(wc -c < "$path") -eq $bytes ]] &&
        printf '%s  %s\n' "$digest" "$path" |
            sha256sum --check --strict --quiet
}

build_case() {
    local scenario=$1
    local output=$2
    local digest=$3
    local bytes=$4
    local standard_output="$temporary_directory/$scenario.out"
    local standard_error="$temporary_directory/$scenario.err"
    if [[ $scenario == normal ]]; then
        "$script_directory/Build-Os-Probe.sh" "$output" >"$standard_output" 2>"$standard_error"
    else
        "$script_directory/Build-Os-Probe.sh" "$output" "$scenario" >"$standard_output" 2>"$standard_error"
    fi
    if [[ $? -ne 0 || -s $standard_error ]] ||
        ! grep -Fxq 'windvale-os-probe-native-build 40' "$standard_output" ||
        ! grep -Fxq "scenario=$scenario" "$standard_output" ||
        ! grep -Fxq "efi-bytes=$bytes" "$standard_output" ||
        ! grep -Fxq "efi-sha256=$digest" "$standard_output" ||
        ! verify_output "$output" "$digest" "$bytes"; then
        cat -- "$standard_output" "$standard_error" >&2
        return 1
    fi
}

build_case normal "$normal" \
    6ffec58edefd6c09c7c552858316da1be02cbceb515715bea36ac5ef0a140018 \
    1698304 || exit 1

"$script_directory/Build-Os-Probe.sh" "$normal" normal \
    >"$temporary_directory/Repeat.out" 2>"$temporary_directory/Repeat.err"
repeat_status=$?
if [[ $repeat_status -ne 1 ]] ||
    ! grep -Fxq 'The native Probe 40 output already exists.' "$temporary_directory/Repeat.err" ||
    ! verify_output "$normal" \
        6ffec58edefd6c09c7c552858316da1be02cbceb515715bea36ac5ef0a140018 \
        1698304 ||
    find "$temporary_directory" -maxdepth 1 -name '.windvale-os-probe-native.*' -print -quit |
        grep -q .; then
    cat -- "$temporary_directory/Repeat.out" "$temporary_directory/Repeat.err" >&2
    exit 1
fi

build_case invalid-opcode "$invalid_opcode" \
    84d0c66f9b6a0ea7ed4c1f3c9416884d80481d42117693298b96baa71e888e3c \
    1698304 || exit 1
build_case general-protection "$general_protection" \
    9f0d51b6f057387f01e054ab22167fc261981748d5d593f37e6aee26256ef740 \
    1698304 || exit 1

echo 'Tests: 4, Passed: 4, Failed: 0'
