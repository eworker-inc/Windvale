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
    6da0c529425e3d301657501411573b268e64d4c13347d8ae74c9fcb7a45cb354 \
    1692160 || exit 1

"$script_directory/Build-Os-Probe.sh" "$normal" normal \
    >"$temporary_directory/Repeat.out" 2>"$temporary_directory/Repeat.err"
repeat_status=$?
if [[ $repeat_status -ne 1 ]] ||
    ! grep -Fxq 'The native Probe 40 output already exists.' "$temporary_directory/Repeat.err" ||
    ! verify_output "$normal" \
        6da0c529425e3d301657501411573b268e64d4c13347d8ae74c9fcb7a45cb354 \
        1692160 ||
    find "$temporary_directory" -maxdepth 1 -name '.windvale-os-probe-native.*' -print -quit |
        grep -q .; then
    cat -- "$temporary_directory/Repeat.out" "$temporary_directory/Repeat.err" >&2
    exit 1
fi

build_case invalid-opcode "$invalid_opcode" \
    7eadb0fa7ab96611a3cbe259c9860b7da381011e4dda80fd8e18535eaa71ca1b \
    1692160 || exit 1
build_case general-protection "$general_protection" \
    5f9dcaaaeaa2a179ec348a417752b62853876f22124bf61ec16f7ef11a3bf9e2 \
    1692160 || exit 1

echo 'Tests: 4, Passed: 4, Failed: 0'
