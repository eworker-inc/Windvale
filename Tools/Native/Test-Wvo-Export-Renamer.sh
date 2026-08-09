#!/usr/bin/env bash
set -uo pipefail

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-native-wvo-export-renamer.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-native-wvo-export-renamer.*)
            rm -rf -- "$temporary_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $temporary_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

input="$temporary_directory/Input.wvo"
expected="$temporary_directory/Expected.wvo"
output="$temporary_directory/Output.wvo"
"$script_directory/Assemble-Wva.sh" \
    "$repository_root/Tests/Native/Wvo-Export-Renamer/Input.wva" "$input" >/dev/null 2>&1 || exit 1
"$script_directory/Assemble-Wva.sh" \
    "$repository_root/Tests/Native/Wvo-Export-Renamer/Expected.wva" "$expected" >/dev/null 2>&1 || exit 1
input_sha256=$(sha256sum -- "$input") || exit 1
input_sha256=${input_sha256%% *}
expected_sha256=$(sha256sum -- "$expected") || exit 1
expected_sha256=${expected_sha256%% *}

"$script_directory/Rename-Wvo-Export.sh" \
    "$input" Main Renamed_entry "$output" >/dev/null 2>&1 || exit 1
cmp -s -- "$output" "$expected" || exit 1
verify_preserved() {
    printf '%s  %s\n%s  %s\n' \
        "$input_sha256" "$input" \
        "$expected_sha256" "$expected" | sha256sum --check --strict --quiet
}
verify_preserved || exit 1

if "$script_directory/Rename-Wvo-Export.sh" \
    "$input" Missing Renamed_entry "$temporary_directory/Missing.wvo" >/dev/null 2>&1; then
    exit 1
fi
[[ ! -e $temporary_directory/Missing.wvo ]] || exit 1
verify_preserved || exit 1

if "$script_directory/Rename-Wvo-Export.sh" \
    "$input" Main bad-name "$temporary_directory/Invalid.wvo" >/dev/null 2>&1; then
    exit 1
fi
[[ ! -e $temporary_directory/Invalid.wvo ]] || exit 1
verify_preserved || exit 1

if "$script_directory/Rename-Wvo-Export.sh" \
    "$input" Main Renamed_entry "$expected" >/dev/null 2>&1; then
    exit 1
fi
verify_preserved || exit 1

echo 'Tests: 4, Passed: 4, Failed: 0'
