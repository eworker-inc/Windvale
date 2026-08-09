#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Hosted-Wvb-Packaging.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
toolset="$repository_root/Artifacts/Native-Hosted-Container-Toolset-Candidate"
temporary_root=${TMPDIR:-/tmp}
test_directory=$(mktemp -d "$temporary_root/windvale-native-hosted-package-test.XXXXXXXX") || exit 1
export TMPDIR="$test_directory"
result=1

cleanup() {
    case "$test_directory" in
        "$temporary_root"/windvale-native-hosted-package-test.*)
            rm -f -- \
                "$test_directory/Valid.elf" \
                "$test_directory/Valid.out" \
                "$test_directory/Valid.err" \
                "$test_directory/Cross-Target.exe" \
                "$test_directory/Cross-Target.out" \
                "$test_directory/Cross-Target.err" \
                "$test_directory/Invalid.wvb" \
                "$test_directory/Destination.elf" \
                "$test_directory/Invalid.out" \
                "$test_directory/Invalid.err"
            rmdir -- "$test_directory"
            ;;
        *)
            echo "Refusing to remove unexpected test path: $test_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

fail() {
    echo "FAIL  hosted packaging: $1" >&2
    exit 1
}

check_file() {
    local path=$1
    local expected_bytes=$2
    local expected_sha256=$3
    local label=$4
    [[ -f $path ]] || fail "missing $label"
    local actual_bytes digest_line actual_sha256
    actual_bytes=$(wc -c < "$path") || fail "$label byte length is unavailable"
    [[ $actual_bytes -eq $expected_bytes ]] || fail "$label byte length differs"
    digest_line=$(sha256sum -- "$path") || fail "$label digest is unavailable"
    actual_sha256=${digest_line%% *}
    [[ $actual_sha256 == "$expected_sha256" ]] || fail "$label digest differs"
}

check_no_scratch() {
    local scratch=("$test_directory"/windvale-native-hosted-package.*)
    [[ ! -e ${scratch[0]} ]] || fail 'private package scratch remains'
}

if ! "$script_directory/Package-Hosted-Wvb.sh" 1 \
    "$toolset/Wvb/wvhostcontrol.wvb" "$test_directory/Valid.elf" \
    >"$test_directory/Valid.out" 2>"$test_directory/Valid.err"; then
    cat -- "$test_directory/Valid.out" "$test_directory/Valid.err" >&2
    fail 'valid packaging failed'
fi
[[ ! -s $test_directory/Valid.err ]] || fail 'valid packaging wrote a diagnostic'
check_file \
    "$test_directory/Valid.elf" 237568 \
    f7b40ac03478d54bdf8fed468fdfbe52a9449159a9fb45c05da6603935e24c67 \
    'valid package'
cmp --silent -- "$test_directory/Valid.elf" "$toolset/linux-x64/wvhostcontrol.elf" ||
    fail 'valid package differs from the candidate'
[[ -x $test_directory/Valid.elf ]] || fail 'valid package is not executable'
check_no_scratch
echo 'PASS  hosted packaging exact Linux application'

if ! "$script_directory/Package-Hosted-Wvb.sh" 1 \
    "$toolset/Wvb/wvhostcontrol.wvb" "$test_directory/Cross-Target.exe" windows \
    >"$test_directory/Cross-Target.out" 2>"$test_directory/Cross-Target.err"; then
    cat -- "$test_directory/Cross-Target.out" "$test_directory/Cross-Target.err" >&2
    fail 'cross-target Windows packaging failed'
fi
[[ ! -s $test_directory/Cross-Target.err ]] || fail 'cross-target packaging wrote a diagnostic'
check_file \
    "$test_directory/Cross-Target.exe" 236032 \
    eeec7c229b20ac006ed366849c91e2f03e035a9e3ee29da2e9aeb408c76b2709 \
    'cross-target Windows package'
cmp --silent -- "$test_directory/Cross-Target.exe" "$toolset/windows-x64/wvhostcontrol.exe" ||
    fail 'cross-target Windows package differs from the candidate'
check_no_scratch
echo 'PASS  hosted packaging exact cross-target Windows application'

cp -- "$toolset/SHA256SUMS" "$test_directory/Invalid.wvb" || fail 'invalid input staging failed'
cp -- "$toolset/SHA256SUMS" "$test_directory/Destination.elf" || fail 'sentinel staging failed'
"$script_directory/Package-Hosted-Wvb.sh" 1 \
    "$test_directory/Invalid.wvb" "$test_directory/Destination.elf" \
    >"$test_directory/Invalid.out" 2>"$test_directory/Invalid.err"
status=$?
[[ $status -ne 0 ]] || fail 'invalid WVB was accepted'
check_file \
    "$test_directory/Destination.elf" 5426 \
    35a48a3ed0080b5537dd38bdd6ccb3867794ac3a6f3d71c22f4afeaaa59f3e41 \
    'preserved destination'
check_file \
    "$test_directory/Invalid.wvb" 5426 \
    35a48a3ed0080b5537dd38bdd6ccb3867794ac3a6f3d71c22f4afeaaa59f3e41 \
    'preserved input'
check_no_scratch
echo 'PASS  hosted packaging rejects invalid WVB and preserves resources'
result=0
echo 'Tests: 3, Passed: 3, Failed: 0'
exit "$result"
