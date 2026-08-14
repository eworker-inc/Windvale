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
command_log="$temporary_directory/Command.log"
stage='initialize'
fail() {
    echo "The native WVO export-renamer focused test failed at stage: $stage." >&2
    if [[ -s $command_log ]]; then
        cat -- "$command_log" >&2
    fi
    exit 1
}
stage='assemble input fixture'
"$script_directory/Assemble-Wva.sh" \
    "$repository_root/Tests/Native/Wvo-Export-Renamer/Input.wva" "$input" >"$command_log" 2>&1 || fail
stage='assemble expected fixture'
"$script_directory/Assemble-Wva.sh" \
    "$repository_root/Tests/Native/Wvo-Export-Renamer/Expected.wva" "$expected" >"$command_log" 2>&1 || fail
stage='hash input fixture'
input_sha256=$(sha256sum -- "$input" 2>"$command_log") || fail
input_sha256=${input_sha256%% *}
stage='hash expected fixture'
expected_sha256=$(sha256sum -- "$expected" 2>"$command_log") || fail
expected_sha256=${expected_sha256%% *}

stage='rename existing export'
"$script_directory/Rename-Wvo-Export.sh" \
    "$input" Main Renamed_entry "$output" >"$command_log" 2>&1 || fail
stage='compare renamed object'
: >"$command_log"
cmp -s -- "$output" "$expected" || fail
verify_preserved() {
    printf '%s  %s\n%s  %s\n' \
        "$input_sha256" "$input" \
        "$expected_sha256" "$expected" | sha256sum --check --strict --quiet >"$command_log" 2>&1
}
stage='verify fixtures after successful rename'
verify_preserved || fail

stage='reject missing export'
if "$script_directory/Rename-Wvo-Export.sh" \
    "$input" Missing Renamed_entry "$temporary_directory/Missing.wvo" >"$command_log" 2>&1; then
    fail
fi
stage='reject missing export without output'
[[ ! -e $temporary_directory/Missing.wvo ]] || fail
stage='verify fixtures after missing-export rejection'
verify_preserved || fail

stage='reject invalid export name'
if "$script_directory/Rename-Wvo-Export.sh" \
    "$input" Main bad-name "$temporary_directory/Invalid.wvo" >"$command_log" 2>&1; then
    fail
fi
stage='reject invalid export name without output'
[[ ! -e $temporary_directory/Invalid.wvo ]] || fail
stage='verify fixtures after invalid-name rejection'
verify_preserved || fail

stage='reject destination overwrite'
if "$script_directory/Rename-Wvo-Export.sh" \
    "$input" Main Renamed_entry "$expected" >"$command_log" 2>&1; then
    fail
fi
stage='verify fixtures after overwrite rejection'
verify_preserved || fail

echo 'Tests: 4, Passed: 4, Failed: 0'
