#!/usr/bin/env bash
set -uo pipefail

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
output_directory=$(mktemp -d "$temporary_root/windvale-seed-native-front-door.XXXXXXXX") || exit 1

cleanup() {
    case "$output_directory" in
        "$temporary_root"/windvale-seed-native-front-door.*)
            rm -rf -- "$output_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $output_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

"$repository_root/Tools/Verify/Verify-Seed-Native-Front-Door.sh" "$output_directory" || exit $?
echo 'Tests: 1, Passed: 1, Failed: 0'
