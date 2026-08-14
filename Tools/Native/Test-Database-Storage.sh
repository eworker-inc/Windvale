#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Database-Storage.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-database-storage.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-database-storage.*)
            rm -rf -- "$temporary_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $temporary_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

build_driver_wvb="$temporary_directory/Build-Driver.wvb"
build_driver="$temporary_directory/Build-Driver.elf"
lowerer_wvb="$temporary_directory/Lowerer.wvb"
lowerer="$temporary_directory/Lowerer.elf"
workspace_path="$repository_root/Windvale.wvws"

"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tools/Windvale-Compiler-Build-Driver.wvproj" \
    "$build_driver_wvb" >/dev/null || exit $?
"$script_directory/Package-Segmented-Compiler-Wvb.sh" \
    2 "$build_driver_wvb" "$build_driver" >/dev/null || exit $?

"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Compiler/Windvale-Native-X64-Lowering-Tool.wvproj" \
    "$lowerer_wvb" >/dev/null || exit $?
"$script_directory/Package-Segmented-Compiler-Wvb.sh" \
    6 "$lowerer_wvb" "$lowerer" >/dev/null || exit $?

verify_target() {
    local label=$1 project_path=$2
    local first_wvb="$temporary_directory/$label-First.wvb"
    local second_wvb="$temporary_directory/$label-Second.wvb"
    local first_wvo="$temporary_directory/$label-First.wvo"
    local second_wvo="$temporary_directory/$label-Second.wvo"
    local image="$temporary_directory/$label.bin"
    local image_prefix="$temporary_directory/$label-Image"
    local map="$temporary_directory/$label.map"
    local linux_application="$temporary_directory/$label.elf"
    local windows_application="$temporary_directory/$label.exe"

    "$build_driver" --workspace "$workspace_path" --project "$project_path" "$first_wvb" >/dev/null || return $?
    "$build_driver" --workspace "$workspace_path" --project "$project_path" "$second_wvb" >/dev/null || return $?
    cmp --silent -- "$first_wvb" "$second_wvb" || return 1

    "$lowerer" "$first_wvb" "$first_wvo" >/dev/null || return $?
    "$lowerer" "$second_wvb" "$second_wvo" >/dev/null || return $?
    cmp --silent -- "$first_wvo" "$second_wvo" || return 1
    "$script_directory/Verify-Wvo.sh" "$first_wvo" >/dev/null || return $?

    "$script_directory/Link-Wvo.sh" 0 Main "$image" "$first_wvo" >"$map" || return $?
    local entry_offset
    entry_offset=$(sed -n 's/^entry name=Main address=\([0-9][0-9]*\)$/\1/p' "$map")
    case "$entry_offset" in
        ''|*[!0-9]*) return 1 ;;
    esac
    cp -- "$image" "$image_prefix.chunk-0" || return $?

    "$script_directory/Package-Hosted-Wvb.sh" image 6 \
        "$first_wvb" "$image_prefix" 1 "$entry_offset" "$linux_application" linux \
        >/dev/null || return $?
    "$linux_application" >/dev/null
    local application_result=$?
    if [[ $application_result -ne 0 ]]; then
        echo "The $label database-storage case returned $application_result, expected 0." >&2
        return 1
    fi

    "$script_directory/Package-Hosted-Wvb.sh" image 6 \
        "$first_wvb" "$image_prefix" 1 "$entry_offset" "$windows_application" windows \
        >/dev/null || return $?
}

verify_storage_lowering() {
    local project_path=$1
    local first_wvb="$temporary_directory/StorageLowering-First.wvb"
    local second_wvb="$temporary_directory/StorageLowering-Second.wvb"
    local first_wvo="$temporary_directory/StorageLowering-First.wvo"
    local second_wvo="$temporary_directory/StorageLowering-Second.wvo"
    local first_report="$temporary_directory/StorageLowering-First.txt"
    local second_report="$temporary_directory/StorageLowering-Second.txt"

    "$build_driver" --workspace "$workspace_path" --project "$project_path" "$first_wvb" >/dev/null || return $?
    "$build_driver" --workspace "$workspace_path" --project "$project_path" "$second_wvb" >/dev/null || return $?
    cmp --silent -- "$first_wvb" "$second_wvb" || return 1

    "$lowerer" "$first_wvb" "$first_wvo" >"$first_report" || return $?
    "$lowerer" "$second_wvb" "$second_wvo" >"$second_report" || return $?
    grep -q '^native x64 status=Valid abi=23 ' "$first_report" || return 1
    cmp --silent -- "$first_report" "$second_report" || return 1
    cmp --silent -- "$first_wvo" "$second_wvo" || return 1
    "$script_directory/Verify-Wvo.sh" "$first_wvo" >/dev/null || return $?
}

verify_target Nested \
    "$repository_root/Projects/Tests/Windvale-Native-Test-Nested-Record-Fields.wvproj" || exit $?
verify_target Publication \
    "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Storage-Publication.wvproj" || exit $?
verify_target Recovery \
    "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Storage-Recovery.wvproj" || exit $?
verify_target ProviderTable \
    "$repository_root/Projects/Tests/Windvale-Native-Test-Capability-Provider-Table.wvproj" || exit $?
verify_target ProviderCall \
    "$repository_root/Projects/Tests/Windvale-Native-Test-X64-Provider-Call.wvproj" || exit $?
verify_target Context9 \
    "$repository_root/Projects/Tests/Windvale-Native-Test-Execution-Context-9.wvproj" || exit $?
verify_storage_lowering \
    "$repository_root/Projects/Tests/Windvale-Native-Test-X64-Storage-Random-Access.wvproj" || exit $?

echo 'native database storage status=Passed cases=7 local-results=0 cross-host-images=Verified'
