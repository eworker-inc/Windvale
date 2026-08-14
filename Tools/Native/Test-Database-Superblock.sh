#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Database-Superblock.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-database-superblock.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-database-superblock.*)
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
first_wvb="$temporary_directory/Superblock-First.wvb"
second_wvb="$temporary_directory/Superblock-Second.wvb"
first_wvo="$temporary_directory/Superblock-First.wvo"
second_wvo="$temporary_directory/Superblock-Second.wvo"
image="$temporary_directory/Superblock.bin"
image_prefix="$temporary_directory/Superblock-Image"
map="$temporary_directory/Superblock.map"
linux_application="$temporary_directory/Superblock.elf"
windows_application="$temporary_directory/Superblock.exe"

"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Compiler/Windvale-Native-X64-Lowering-Tool.wvproj" \
    "$lowerer_wvb" >/dev/null || exit $?
"$script_directory/Package-Segmented-Compiler-Wvb.sh" \
    6 "$lowerer_wvb" "$lowerer" >/dev/null || exit $?

"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Durable-Superblock.wvproj" \
    "$first_wvb" >/dev/null || exit $?
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Durable-Superblock.wvproj" \
    "$second_wvb" >/dev/null || exit $?
cmp -s -- "$first_wvb" "$second_wvb" || {
    echo 'The database-superblock WVB is not deterministic.' >&2
    exit 1
}
verify_file "$first_wvb" 58784 \
    c5934333b5254b767dbbccd630ca9f0320860ae0fc5b0ed4c73f41c8a5fced63 \
    'database-superblock WVB' || exit $?

"$lowerer" "$first_wvb" "$first_wvo" >/dev/null || exit $?
"$lowerer" "$second_wvb" "$second_wvo" >/dev/null || exit $?
cmp -s -- "$first_wvo" "$second_wvo" || {
    echo 'The database-superblock WVO is not deterministic.' >&2
    exit 1
}
"$script_directory/Check-Wvo.sh" "$first_wvo" >/dev/null || exit $?
verify_file "$first_wvo" 1098332 \
    c126573f46f5f7a85422fcc6b37a6751b05d58b43f75380206641612a6aee352 \
    'database-superblock WVO' || exit $?

"$script_directory/Link-Wvo.sh" 0 Main "$image" "$first_wvo" >"$map" || exit $?
entry_offset=$(sed -n 's/^entry name=Main address=//p' "$map")
[[ $entry_offset == 171555 ]] || {
    echo "The database-superblock entry offset is $entry_offset, expected 171555." >&2
    exit 1
}
verify_file "$image" 1095856 \
    50cc4d33e1b0a47b75c3c089cb000d36c76b0f4e09ee7962704f4c23e1b73956 \
    'database-superblock image' || exit $?
cp -- "$image" "$image_prefix.chunk-0" || exit $?

"$script_directory/Package-Hosted-Wvb.sh" image 6 \
    "$first_wvb" "$image_prefix" 1 "$entry_offset" "$linux_application" linux \
    >/dev/null || exit $?
verify_file "$linux_application" 1114112 \
    a1c62f7075d85c20da3e7e5b1fb50c05c654ddc209a0f7a312e7b916616661ec \
    'database-superblock Linux application' || exit $?
for test_case in A B C D E F G H I J K L M; do
    "$linux_application" "$test_case" >/dev/null
    application_result=$?
    if [[ $application_result -ne 42 ]]; then
        echo "The database-superblock case $test_case returned $application_result, expected 42." >&2
        exit 1
    fi
done

"$script_directory/Package-Hosted-Wvb.sh" image 6 \
    "$first_wvb" "$image_prefix" 1 "$entry_offset" "$windows_application" windows \
    >/dev/null || exit $?
verify_file "$windows_application" 1114624 \
    ae83fdfbfca118e033cc8c7716805f62c9595c6fb2d7407ce805a0e0c8a5f3f3 \
    'database-superblock Windows application' || exit $?

echo 'native database superblock status=Passed cases=13 local-result=42 cross-host-images=Verified'
