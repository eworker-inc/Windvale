#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 1 || $1 != *.wvo ]]; then
    echo 'Usage: ./Tools/Native/Build-Os-Process-Policy-Object.sh <output.wvo>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
output_directory=$(CDPATH= cd -- "$(dirname -- "$1")" && pwd -P) || exit 1
output="$output_directory/$(basename -- "$1")"
if [[ -e $output ]]; then
    echo 'The OS process-policy output already exists.' >&2
    exit 1
fi
work=$(mktemp -d "$output_directory/.windvale-os-process-policy.XXXXXXXX") || exit 1
case "$work" in
    "$output_directory"/.windvale-os-process-policy.*) ;;
    *)
        echo 'The OS process-policy private path is outside the output directory.' >&2
        exit 1
        ;;
esac
cleanup() {
    rm -rf -- "$work"
}
trap cleanup EXIT

verify_identity() {
    local path=$1
    local bytes=$2
    local digest=$3
    [[ -f $path && $(wc -c < "$path") -eq $bytes ]] &&
        printf '%s  %s\n' "$digest" "$path" |
            sha256sum --check --strict --quiet
}

if ! "$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Operating-System/Windvale-Os-Process-Policy.wvproj" \
    "$work/Process-Policy.wvb" >"$work/Build.log" 2>&1; then
    cat -- "$work/Build.log" >&2
    exit 1
fi
verify_identity "$work/Process-Policy.wvb" 33786 \
    26a540bc1435114608aa597545c805e0786c9593b6e8ba19e8919b9f7718b0c1 || exit 1

if ! "$script_directory/Lower-Wvb-To-Wvo.sh" \
    "$work/Process-Policy.wvb" "$work/Process-Policy-Main.wvo" \
    >"$work/Lower.log" 2>&1; then
    cat -- "$work/Lower.log" >&2
    exit 1
fi
verify_identity "$work/Process-Policy-Main.wvo" 583390 \
    dcee27f6384933ef07cf99eefd5f3355e25edbf690c332c7b201a397a0031d95 || exit 1

if ! "$script_directory/Rename-Wvo-Export.sh" \
    "$work/Process-Policy-Main.wvo" Main Windvale_kernel_process_policy \
    "$work/Process-Policy.wvo" >"$work/Rename.log" 2>&1; then
    cat -- "$work/Rename.log" >&2
    exit 1
fi
if ! verify_identity "$work/Process-Policy.wvo" 583416 \
        4d3ffefc6be3c4edb48f1032415d96987bbd62899cdadd1fb4f0dc91ca319428 ||
    ! "$script_directory/Verify-Wvo.sh" "$work/Process-Policy.wvo" >/dev/null 2>&1; then
    echo 'The native OS process-policy object build failed.' >&2
    exit 1
fi
if ! "$script_directory/Publish-Wvo.sh" \
    "$work/Process-Policy.wvo" "$output" >"$work/Publish.log" 2>&1; then
    cat -- "$work/Publish.log" >&2
    exit 1
fi
