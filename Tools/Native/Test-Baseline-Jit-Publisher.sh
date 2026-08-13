#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Baseline-Jit-Publisher.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-baseline-jit-publisher.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-baseline-jit-publisher.*)
            rm -f -- \
                "$temporary_directory/Plan.wvo" \
                "$temporary_directory/Linux.wvo" \
                "$temporary_directory/Bridge.wvb" \
                "$temporary_directory/Linux.bin" \
                "$temporary_directory/Linux.wvmap" \
                "$temporary_directory/Baseline-Jit-Publisher.elf" \
                "$temporary_directory/Application.err"
            rmdir -- "$temporary_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $temporary_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

check_hash() {
    local path=$1
    local digest=$2
    local label=$3
    local directory
    directory=$(CDPATH= cd -- "$(dirname -- "$path")" && pwd -P) || return 1
    if ! (cd -- "$directory" && printf '%s  %s\n' \
        "$digest" "$(basename -- "$path")" | sha256sum --check --strict --quiet); then
        echo "The native baseline-JIT publisher $label identity differs." >&2
        return 1
    fi
}

plan_wvo="$temporary_directory/Plan.wvo"
platform_wvo="$temporary_directory/Linux.wvo"
bridge_wvb="$temporary_directory/Bridge.wvb"
retained_bridge_wvb="$repository_root/Artifacts/Baseline-Jit-Publisher/Wvb/Baseline-Jit-Patch-Plan-Bridge.wvb"
bridge_wvo="$repository_root/Artifacts/Baseline-Jit-Publisher/Wvo/Baseline-Jit-Patch-Plan-Bridge.wvo"
image="$temporary_directory/Linux.bin"
map="$temporary_directory/Linux.wvmap"
application="$temporary_directory/Baseline-Jit-Publisher.elf"
application_error="$temporary_directory/Application.err"
published_application="$repository_root/Artifacts/Baseline-Jit-Publisher/linux-x64/Baseline-Jit-Publisher.elf"

"$repository_root/Tools/Native/Build-Wvb.sh" \
    "$repository_root/Projects/Compiler/Windvale-Native-Baseline-Jit-Patch-Plan-Bridge.wvproj" \
    "$bridge_wvb" >/dev/null || exit $?
check_hash "$bridge_wvb" \
    '2dc536e9d3511d4fde3191e1084d9634543154a525623fd3c7c669f9d3bf20d9' \
    'producer-bridge WVB' || exit $?
check_hash "$retained_bridge_wvb" \
    '2dc536e9d3511d4fde3191e1084d9634543154a525623fd3c7c669f9d3bf20d9' \
    'retained producer-bridge WVB' || exit $?
if ! cmp --silent -- "$bridge_wvb" "$retained_bridge_wvb"; then
    echo 'The rebuilt and retained native baseline-JIT producer-bridge WVBs differ.' >&2
    exit 1
fi
check_hash "$bridge_wvo" \
    'bcc02cdc6134da2388265ad308d3dc739a7e10c1911effa918d5f2577c86ae8c' \
    'retained producer-bridge WVO' || exit $?
"$repository_root/Tools/Native/Verify-Wvo.sh" "$bridge_wvo" >/dev/null || exit $?

"$repository_root/Tools/Native/Assemble-Wva.sh" \
    "$repository_root/Runtime/Native/Baseline-Jit-Patch-Plan-X64.wva" \
    "$plan_wvo" >/dev/null || exit $?
"$repository_root/Tools/Native/Verify-Wvo.sh" "$plan_wvo" >/dev/null || exit $?
check_hash "$plan_wvo" \
    '8cc9c7460229a479adf34631a970c9d196b37361ceaa35fdea85e15fce9d91b1' \
    'shared-plan WVO' || exit $?

"$repository_root/Tools/Native/Assemble-Wva.sh" \
    "$repository_root/Runtime/Native/Linux-X64-Baseline-Jit-Publisher.wva" \
    "$platform_wvo" >/dev/null || exit $?
"$repository_root/Tools/Native/Verify-Wvo.sh" "$platform_wvo" >/dev/null || exit $?
check_hash "$platform_wvo" \
    '7a6556a0b5f59935edfa5fd380874a63ae594ac91deaeea88fd31383a60267b8' \
    'Linux-adapter WVO' || exit $?

"$repository_root/Tools/Native/Link-Wvo.sh" 1048576 Linux_baseline_jit_entry \
    "$image" "$platform_wvo" "$plan_wvo" "$bridge_wvo" > "$map" || exit $?
check_hash "$image" \
    'c77ab84774f7c1f188855c095b30b7e8182c31523d579a6a72b4735d7524c78a' \
    'Linux flat image' || exit $?

entry_address=$(sed -n 's/^entry name=Linux_baseline_jit_entry address=//p' "$map")
if [[ ! $entry_address =~ ^[0-9]+$ ]]; then
    echo 'The native baseline-JIT publisher entry is missing from the link map.' >&2
    exit 1
fi
entry_offset=$((entry_address - 1048576))
if ((entry_offset != 595)); then
    echo "The native baseline-JIT publisher entry offset is $entry_offset, expected 595." >&2
    exit 1
fi

"$repository_root/Tools/Native/Package-Console.sh" linux-x64-console-v1 \
    "$image" "$entry_offset" "$application" >/dev/null || exit $?
check_hash "$application" \
    '29538c93d28bcd1feae175519f5b2950d5e8dfcde24afa3f0039863fb1706a90' \
    'reconstructed Linux application' || exit $?
check_hash "$published_application" \
    '29538c93d28bcd1feae175519f5b2950d5e8dfcde24afa3f0039863fb1706a90' \
    'published Linux application' || exit $?
if ! cmp --silent -- "$application" "$published_application"; then
    echo 'The reconstructed and published Linux applications differ.' >&2
    exit 1
fi

"$published_application" >/dev/null 2> "$application_error"
application_result=$?
if ((application_result != 0)); then
    echo "The native baseline-JIT publisher result is $application_result, expected 0." >&2
    if [[ -s $application_error ]]; then
        cat -- "$application_error" >&2
    fi
    exit 1
fi
if [[ -s $application_error ]]; then
    echo 'The native baseline-JIT publisher wrote a diagnostic.' >&2
    cat -- "$application_error" >&2
    exit 1
fi

echo 'native baseline jit publisher status=Passed result=0 platform=linux-x64'
