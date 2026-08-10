#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 1 || ! -d $1 ]]; then
    echo 'Usage: ./Tools/Verify/Verify-Seed-Native-Console-Aot.sh <output-directory>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
output_root=$(CDPATH= cd -- "$1" && pwd -P)
sum_module="$output_root/Sum-Data.wvb"
windows_application="$output_root/Sum-Data-Windows.exe"
linux_application="$output_root/Sum-Data-Linux.elf"

check_file() {
    local path=$1
    local bytes=$2
    local digest=$3
    local label=$4
    local actual_bytes
    local actual_digest
    actual_bytes=$(wc -c < "$path" | tr -d ' ')
    actual_digest=$(sha256sum "$path" | awk '{print $1}')
    if [[ $actual_bytes != "$bytes" || $actual_digest != "$digest" ]]; then
        echo "The native Seed $label identity is invalid." >&2
        return 1
    fi
}

check_file "$sum_module" 494 \
    76b4fa3c4c0cc37e6f1350e8191ccd78c6272224f146ef9816b5f987114c15df \
    'input WVB' || exit $?

temporary_root=$(CDPATH= cd -- "${TMPDIR:-/tmp}" && pwd -P) || exit 1
temporary_directory=$(mktemp -d "$temporary_root/windvale-seed-console-aot.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-seed-console-aot.*)
            rm -f -- "$temporary_directory/Sum-Data.wvo" "$temporary_directory/Sum-Data.bin"
            rmdir -- "$temporary_directory"
            ;;
        *)
            echo 'Refusing to remove an unexpected native Seed AOT temporary directory.' >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

wvo="$temporary_directory/Sum-Data.wvo"
image="$temporary_directory/Sum-Data.bin"
lower_output=$("$repository_root/Tools/Native/Lower-Wvb-To-Wvo.sh" \
    "$sum_module" "$wvo") || exit $?
if [[ $lower_output != \
    'native x64 status=Valid abi=22 code-bytes=3088 object-bytes=3288' ]]; then
    echo 'The native Seed Sum-Data lowering report is invalid.' >&2
    exit 1
fi
check_file "$wvo" 3288 \
    4e4958f8f0d611e00e912b925b837aa968e06f85abb116b721e3d6e9b8eed4e1 \
    'WVO' || exit $?

verify_output=$("$repository_root/Tools/Native/Verify-Wvo.sh" "$wvo") || exit $?
expected_verify_output=$(printf '%s\n%s' \
    'Verified object: X86ˉ64' \
    'SHA-256: 4e4958f8f0d611e00e912b925b837aa968e06f85abb116b721e3d6e9b8eed4e1')
if [[ $verify_output != "$expected_verify_output" ]]; then
    echo 'The native Seed Sum-Data WVO verification report is invalid.' >&2
    exit 1
fi

link_output=$("$repository_root/Tools/Native/Link-Wvo.sh" 0 Main "$image" "$wvo") || exit $?
expected_link_output=$(cat <<'EOF'
windvale-link-map 1
target name=flat-x86-64-v1 architecture=x86-64 base-address=0 image-bytes=3104
entry name=Main address=774
image sha256=8185a8893587d8d5a8d0430e53310c5e6725dea30a76073292864b90c5150c8a
inputs count=1
input index=0 sha256=4e4958f8f0d611e00e912b925b837aa968e06f85abb116b721e3d6e9b8eed4e1
sections count=2
section index=0 input=0 source-index=0 kind=code name=.text image-offset=0 address=0 memory-bytes=3088 data-bytes=3088 alignment=16
section index=1 input=0 source-index=1 kind=read-only-data name=.rodata image-offset=3088 address=3088 memory-bytes=16 data-bytes=16 alignment=16
defined-symbols count=3
symbol index=0 input=0 source-index=0 binding=local kind=data name=$data_0000 address=3088 size=16
symbol index=1 input=0 source-index=1 binding=local kind=function name=$function_0000 address=0 size=774
symbol index=2 input=0 source-index=2 binding=export kind=function name=Main address=774 size=2300
imports count=0
relocations count=1
relocation index=0 input=0 source-index=0 kind=relative-i32 patch-offset=2302 patch-address=2302 target=$data_0000 target-input=0 target-source-index=0 target-address=3088 addend=-4 value=782
EOF
)
if [[ $link_output != "$expected_link_output" ]]; then
    echo 'The native Seed Sum-Data link map is invalid.' >&2
    exit 1
fi
check_file "$image" 3104 \
    8185a8893587d8d5a8d0430e53310c5e6725dea30a76073292864b90c5150c8a \
    'flat image' || exit $?

windows_package_output=$("$repository_root/Tools/Native/Package-Console.sh" \
    windows-x64-console-v1 "$image" 774 "$windows_application") || exit $?
if [[ $windows_package_output != \
    'package status=Valid target=windows-x64-console-v1 native-image-bytes=3104 entry-offset=774 application-bytes=5120' ]]; then
    echo 'The native Seed Windows console package report is invalid.' >&2
    exit 1
fi
check_file "$windows_application" 5120 \
    5947c00a81f4cf94651d42d619f3173a622448d042f4fa20e3042940d4a56c77 \
    'Windows application' || exit $?

linux_package_output=$("$repository_root/Tools/Native/Package-Console.sh" \
    linux-x64-console-v1 "$image" 774 "$linux_application") || exit $?
if [[ $linux_package_output != \
    'package status=Valid target=linux-x64-console-v1 native-image-bytes=3104 entry-offset=774 application-bytes=8304' ]]; then
    echo 'The native Seed Linux console package report is invalid.' >&2
    exit 1
fi
check_file "$linux_application" 8304 \
    8af8b46c290965cfc4475d882ac2d5fbdb0ffe4c493a19883a19c2683a319ec4 \
    'Linux application' || exit $?
if [[ $(stat -c '%a' "$linux_application") != 755 ]]; then
    echo 'The native Seed Linux console application mode is invalid.' >&2
    exit 1
fi

"$linux_application" >/dev/null
application_status=$?
if ((application_status != 29)); then
    echo "The native Seed Linux console application returned $application_status instead of 29." >&2
    exit 1
fi

echo 'native Seed console AOT verification status=Complete artifacts=2 cases=1'
