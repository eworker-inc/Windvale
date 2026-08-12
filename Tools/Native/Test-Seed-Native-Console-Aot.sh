#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Seed-Native-Console-Aot.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=$(CDPATH= cd -- "${TMPDIR:-/tmp}" && pwd -P) || exit 1
output_directory=$(mktemp -d "$temporary_root/windvale-seed-native-console-aot.XXXXXXXX") || exit 1
build_output="$output_directory/Build.out"
build_error="$output_directory/Build.err"

cleanup() {
    case "$output_directory" in
        "$temporary_root"/windvale-seed-native-console-aot.*)
            rm -rf -- "$output_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $output_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

"$repository_root/Tools/Native/Build-Wvb.sh" \
    "$repository_root/Examples/Seed/Sum-Data.wvproj" \
    "$output_directory/Sum-Data.wvb" > "$build_output" 2> "$build_error"
build_status=$?
if ((build_status != 0)); then
    cat -- "$build_output" "$build_error" >&2
    exit "$build_status"
fi
if [[ -s $build_error ]]; then
    echo 'The native Seed console AOT input build wrote standard error.' >&2
    cat -- "$build_error" >&2
    exit 1
fi

"$repository_root/Tools/Verify/Verify-Seed-Native-Console-Aot.sh" \
    "$output_directory" || exit $?
echo 'Tests: 1, Passed: 1, Failed: 0'
