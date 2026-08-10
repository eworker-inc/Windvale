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
    [[ $(wc -c < "$path") -eq 683008 ]] &&
        printf '%s  %s\n' "$digest" "$path" |
            sha256sum --check --strict --quiet
}

build_case() {
    local scenario=$1
    local output=$2
    local digest=$3
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
        ! grep -Fxq 'efi-bytes=683008' "$standard_output" ||
        ! grep -Fxq "efi-sha256=$digest" "$standard_output" ||
        ! verify_output "$output" "$digest"; then
        cat -- "$standard_output" "$standard_error" >&2
        return 1
    fi
}

build_case normal "$normal" +    080b4d669e9a11fdc802bf7197ae5a044978b6ba39741b2b1c832296987f74d9 || exit 1

"$script_directory/Build-Os-Probe.sh" "$normal" normal +    >"$temporary_directory/Repeat.out" 2>"$temporary_directory/Repeat.err"
repeat_status=$?
if [[ $repeat_status -ne 1 ]] ||
    ! grep -Fxq 'The native Probe 40 output already exists.' "$temporary_directory/Repeat.err" ||
    ! verify_output "$normal" +        080b4d669e9a11fdc802bf7197ae5a044978b6ba39741b2b1c832296987f74d9 ||
    find "$temporary_directory" -maxdepth 1 -name '.windvale-os-probe-native.*' -print -quit |
        grep -q .; then
    cat -- "$temporary_directory/Repeat.out" "$temporary_directory/Repeat.err" >&2
    exit 1
fi

build_case invalid-opcode "$invalid_opcode" +    8af8a705da7a63e895e39a94a1ff60dae52bfa1ad0b9c0984adeafe538bae734 || exit 1
build_case general-protection "$general_protection" +    47f5ae37b48edb0212c6d439237e43ee2ca8064061786010f9644acf70f7ad4b || exit 1

echo 'Tests: 4, Passed: 4, Failed: 0'
