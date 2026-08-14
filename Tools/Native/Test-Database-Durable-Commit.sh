#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Database-Durable-Commit.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-database-durable-commit.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-database-durable-commit.*)
            rm -f -- "$temporary_directory"/*
            rmdir -- "$temporary_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $temporary_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

verify_file() {
    local path=$1 expected_bytes=$2 expected_sha256=$3 label=$4
    [[ -f $path ]] || { echo "Missing $label: $path" >&2; return 1; }
    local actual_bytes digest_line actual_sha256
    actual_bytes=$(wc -c < "$path") || return 1
    [[ $actual_bytes -eq $expected_bytes ]] || {
        echo "The $label byte length differs." >&2
        return 1
    }
    digest_line=$(sha256sum -- "$path") || return 1
    actual_sha256=${digest_line%% *}
    [[ $actual_sha256 == "$expected_sha256" ]] || {
        echo "The $label digest differs." >&2
        return 1
    }
}

lowerer_wvb="$temporary_directory/Lowerer.wvb"
lowerer="$temporary_directory/Lowerer.elf"
first_wvb="$temporary_directory/Commit-First.wvb"
second_wvb="$temporary_directory/Commit-Second.wvb"
first_wvo="$temporary_directory/Commit-First.wvo"
second_wvo="$temporary_directory/Commit-Second.wvo"
image="$temporary_directory/Commit.bin"
image_prefix="$temporary_directory/Commit-Image"
map="$temporary_directory/Commit.map"
linux_application="$temporary_directory/Commit.elf"
windows_application="$temporary_directory/Commit.exe"

"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Compiler/Windvale-Native-X64-Lowering-Tool.wvproj" \
    "$lowerer_wvb" >/dev/null || exit $?
"$script_directory/Package-Segmented-Compiler-Wvb.sh" \
    6 "$lowerer_wvb" "$lowerer" >/dev/null || exit $?

"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Durable-Commit.wvproj" \
    "$first_wvb" >/dev/null || exit $?
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Durable-Commit.wvproj" \
    "$second_wvb" >/dev/null || exit $?
cmp -s -- "$first_wvb" "$second_wvb" || {
    echo 'The database-durable-commit WVB is not deterministic.' >&2
    exit 1
}
verify_file "$first_wvb" 107155 \
    1a026edee89222585e5c6b7a7367fca807846d5cfdd58010fc85d872f7f2973c \
    'database-durable-commit WVB' || exit $?

"$lowerer" "$first_wvb" "$first_wvo" >/dev/null || exit $?
"$lowerer" "$second_wvb" "$second_wvo" >/dev/null || exit $?
cmp -s -- "$first_wvo" "$second_wvo" || {
    echo 'The database-durable-commit WVO is not deterministic.' >&2
    exit 1
}
"$script_directory/Check-Wvo.sh" "$first_wvo" >/dev/null || exit $?
verify_file "$first_wvo" 2001802 \
    2abe19205e0f1e64afb7d49931697ab7f96646e315adf76531f52c50ddff14b5 \
    'database-durable-commit WVO' || exit $?

"$script_directory/Link-Wvo.sh" 0 Main "$image" "$first_wvo" >"$map" || exit $?
entry_offset=$(sed -n 's/^entry name=Main address=//p' "$map")
[[ $entry_offset == 151017 ]] || {
    echo "The database-durable-commit entry offset is $entry_offset, expected 151017." >&2
    exit 1
}
verify_file "$image" 1998308 \
    60da45fe57c3d1614024588be4c22044f4057fa04512693d82df47f90aebfbe1 \
    'database-durable-commit image' || exit $?
cp -- "$image" "$image_prefix.chunk-0" || exit $?

"$script_directory/Package-Hosted-Wvb.sh" image 6 \
    "$first_wvb" "$image_prefix" 1 "$entry_offset" "$linux_application" linux \
    >/dev/null || exit $?
verify_file "$linux_application" 2019328 \
    965b4a1fb73b6aaf33aec2478443329b6759a6b20c93e3e3f83067476b81125d \
    'database-durable-commit Linux application' || exit $?
for test_case in A B C D E F G H I J K L; do
    "$linux_application" "$test_case" >/dev/null
    application_result=$?
    if [[ $application_result -ne 42 ]]; then
        echo "The database-durable-commit case $test_case returned $application_result, expected 42." >&2
        exit 1
    fi
done

"$script_directory/Package-Hosted-Wvb.sh" image 6 \
    "$first_wvb" "$image_prefix" 1 "$entry_offset" "$windows_application" windows \
    >/dev/null || exit $?
verify_file "$windows_application" 2019328 \
    36d24784407890f07b1a279276d52c7f979e6cb06340cc5ae08baa4f37cd286f \
    'database-durable-commit Windows application' || exit $?

echo 'native database durable commit status=Passed cases=12 local-result=42 cross-host-images=Verified'
