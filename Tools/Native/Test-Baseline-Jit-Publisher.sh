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
image="$temporary_directory/Linux.bin"
map="$temporary_directory/Linux.wvmap"
application="$temporary_directory/Baseline-Jit-Publisher.elf"
application_error="$temporary_directory/Application.err"
published_application="$repository_root/Artifacts/Baseline-Jit-Publisher/linux-x64/Baseline-Jit-Publisher.elf"

"$repository_root/Tools/Native/Assemble-Wva.sh" \
    "$repository_root/Runtime/Native/Baseline-Jit-Patch-Plan-X64.wva" \
    "$plan_wvo" >/dev/null || exit $?
"$repository_root/Tools/Native/Verify-Wvo.sh" "$plan_wvo" >/dev/null || exit $?
check_hash "$plan_wvo" \
    '9074413259924bb50e8a98ca14690e0ec34a65b28c15f0d27a69799c7071f763' \
    'shared-plan WVO' || exit $?

"$repository_root/Tools/Native/Assemble-Wva.sh" \
    "$repository_root/Runtime/Native/Linux-X64-Baseline-Jit-Publisher.wva" \
    "$platform_wvo" >/dev/null || exit $?
"$repository_root/Tools/Native/Verify-Wvo.sh" "$platform_wvo" >/dev/null || exit $?
check_hash "$platform_wvo" \
    'b3cfb37c9d9bf17821673ad04a1e3fcd2a6cbb28d65df59838c56599626867c7' \
    'Linux-adapter WVO' || exit $?

"$repository_root/Tools/Native/Link-Wvo.sh" 1048576 Main \
    "$image" "$platform_wvo" "$plan_wvo" > "$map" || exit $?
check_hash "$image" \
    '991b6218758fe34514733b5ca71ff98baf61f1ab6103f15dc8c6b4c6b6623902' \
    'Linux flat image' || exit $?

entry_address=$(sed -n 's/^entry name=Main address=//p' "$map")
if [[ ! $entry_address =~ ^[0-9]+$ ]]; then
    echo 'The native baseline-JIT publisher entry is missing from the link map.' >&2
    exit 1
fi
entry_offset=$((entry_address - 1048576))
if ((entry_offset != 389)); then
    echo "The native baseline-JIT publisher entry offset is $entry_offset, expected 389." >&2
    exit 1
fi

"$repository_root/Tools/Native/Package-Console.sh" linux-x64-console-v1 \
    "$image" "$entry_offset" "$application" >/dev/null || exit $?
check_hash "$application" \
    '371f0aaaa5200c5767947892f99376e3c649b86dfa8ae5d78e2474aad4a667ea' \
    'reconstructed Linux application' || exit $?
check_hash "$published_application" \
    '371f0aaaa5200c5767947892f99376e3c649b86dfa8ae5d78e2474aad4a667ea' \
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
