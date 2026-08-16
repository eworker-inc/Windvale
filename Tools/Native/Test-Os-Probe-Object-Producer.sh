#!/usr/bin/env bash
set -uo pipefail

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-native-os-probe-object.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-native-os-probe-object.*)
            rm -rf -- "$temporary_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $temporary_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

exceptions="$temporary_directory/09-exceptions.wvo"
admission="$temporary_directory/12-wvb-admission-bridge.wvo"
native_bridge="$temporary_directory/13-native-bridge-and-support.wvo"
paging="$temporary_directory/10-paging.wvo"
memory="$temporary_directory/08-memory.wvo"
invalid_opcode_memory="$temporary_directory/08-memory-invalid-opcode.wvo"
general_protection_memory="$temporary_directory/08-memory-general-protection.wvo"
loader="$temporary_directory/00-loader.wvo"
existing="$temporary_directory/Existing.wvo"
unknown="$temporary_directory/Unknown.wvo"
invalid="$temporary_directory/Invalid.bin"

verify_output() {
    local path=$1
    local bytes=$2
    local digest=$3
    [[ $(wc -c < "$path") -eq $bytes ]] &&
        printf '%s  %s\n' "$digest" "$path" |
            sha256sum --check --strict --quiet
}

"$script_directory/Produce-Os-Probe-Object.sh" exceptions "$exceptions" >/dev/null 2>&1 || exit 1
verify_output "$exceptions" 483 \
    9caeb7ce353bca33e3bbac729ecca0423d59f8ce6b65ccd6b54fa53c381d617c || exit 1
"$script_directory/Verify-Wvo.sh" "$exceptions" >/dev/null 2>&1 || exit 1

"$script_directory/Produce-Os-Probe-Object.sh" wvb-admission-bridge "$admission" >/dev/null 2>&1 || exit 1
verify_output "$admission" 484 \
    271c378b1f12bb4affa33474d865611cbf14e5b1b8996c703cb3d3cbe22eee7d || exit 1
"$script_directory/Verify-Wvo.sh" "$admission" >/dev/null 2>&1 || exit 1

"$script_directory/Produce-Os-Probe-Object.sh" native-bridge-and-support "$native_bridge" >/dev/null 2>&1 || exit 1
verify_output "$native_bridge" 461 \
    472a0fbe6497525e634a4785e92aa9ee62c3c7d70fff7510e45acbea644eea0b || exit 1
"$script_directory/Verify-Wvo.sh" "$native_bridge" >/dev/null 2>&1 || exit 1

"$script_directory/Produce-Os-Probe-Object.sh" paging "$paging" >/dev/null 2>&1 || exit 1
verify_output "$paging" 1292 \
    5d5ba8237cebf85f14482996b43b44628f1e87fbea0a19377631f3974334b29b || exit 1
"$script_directory/Verify-Wvo.sh" "$paging" >/dev/null 2>&1 || exit 1

"$script_directory/Produce-Os-Probe-Object.sh" memory "$memory" >/dev/null 2>&1 || exit 1
verify_output "$memory" 1529 \
    2668e17c3181e168415fb7bdee530873e2ddc8fa2d100af94bcc7b74909df3ed || exit 1
"$script_directory/Verify-Wvo.sh" "$memory" >/dev/null 2>&1 || exit 1

"$script_directory/Produce-Os-Probe-Object.sh" memory-invalid-opcode "$invalid_opcode_memory" >/dev/null 2>&1 || exit 1
verify_output "$invalid_opcode_memory" 1545 \
    09aa0fcfe12c561b79367cb26569dbc6f1f47ca3b98dc892426ca57b4328f868 || exit 1
"$script_directory/Verify-Wvo.sh" "$invalid_opcode_memory" >/dev/null 2>&1 || exit 1

"$script_directory/Produce-Os-Probe-Object.sh" memory-general-protection "$general_protection_memory" >/dev/null 2>&1 || exit 1
verify_output "$general_protection_memory" 1545 \
    23a052f9d47a9416618c9b7a50a382c68c46d3bf7834410cc79f8fef2aa461e0 || exit 1
"$script_directory/Verify-Wvo.sh" "$general_protection_memory" >/dev/null 2>&1 || exit 1

"$script_directory/Produce-Os-Probe-Object.sh" loader "$loader" >/dev/null 2>&1 || exit 1
verify_output "$loader" 6336 \
    b310bc0e9aebc7b14c0892bb3dd4b833d42539c2194427a8f333b511d6af3804 || exit 1
"$script_directory/Verify-Wvo.sh" "$loader" >/dev/null 2>&1 || exit 1

printf '%s\n' preserved > "$existing"
existing_sha256=$(sha256sum -- "$existing") || exit 1
existing_sha256=${existing_sha256%% *}
if "$script_directory/Produce-Os-Probe-Object.sh" exceptions "$existing" >/dev/null 2>&1; then
    exit 1
fi
printf '%s  %s\n' "$existing_sha256" "$existing" |
    sha256sum --check --strict --quiet || exit 1

"$script_directory/Produce-Os-Probe-Object.sh" unknown "$unknown" >/dev/null 2>&1
unknown_status=$?
if [[ $unknown_status -ne 64 || -e $unknown ]]; then
    exit 1
fi

"$script_directory/Produce-Os-Probe-Object.sh" exceptions "$invalid" >/dev/null 2>&1
invalid_status=$?
if [[ $invalid_status -ne 64 || -e $invalid ]]; then
    exit 1
fi

echo 'Tests: 11, Passed: 11, Failed: 0'
