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
                "$test_directory/Verifier-Request.elf" \
                "$test_directory/Verifier-Request.exe" \
                "$test_directory/Verifier-Request.out" \
                "$test_directory/Verifier-Request.err" \
                "$test_directory/Verifier-Request-Windows.out" \
                "$test_directory/Verifier-Request-Windows.err" \
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
    45c8bf1163556c851db8b7fecb2556e899c816d06bd39209d65db942fea3c44a \
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
    d8b10130bc946261526ee0accc9fcbd42dbe2a5d9fd3e4d4f349038550c8c559 \
    'cross-target Windows package'
cmp --silent -- "$test_directory/Cross-Target.exe" "$toolset/windows-x64/wvhostcontrol.exe" ||
    fail 'cross-target Windows package differs from the candidate'
check_no_scratch
echo 'PASS  hosted packaging exact cross-target Windows application'

if ! "$script_directory/Package-Hosted-Wvb.sh" 1 \
    "$toolset/Wvb/wvhostverifierrequest.wvb" "$test_directory/Verifier-Request.elf" \
    >"$test_directory/Verifier-Request.out" 2>"$test_directory/Verifier-Request.err"; then
    cat -- "$test_directory/Verifier-Request.out" "$test_directory/Verifier-Request.err" >&2
    fail 'verifier request packaging failed'
fi
[[ ! -s $test_directory/Verifier-Request.err ]] || fail 'verifier request packaging wrote a diagnostic'
check_file \
    "$test_directory/Verifier-Request.elf" 200704 \
    4492bcaa51983185d8e9681bacca1770f9117e5b7c28806aa1eaf629497b09c4 \
    'verifier request package'
cmp --silent -- "$test_directory/Verifier-Request.elf" "$toolset/linux-x64/wvhostverifierrequest.elf" ||
    fail 'verifier request package differs from the candidate'
[[ -x $test_directory/Verifier-Request.elf ]] || fail 'verifier request package is not executable'
check_no_scratch
echo 'PASS  hosted packaging exact verifier request Linux application'

if ! "$script_directory/Package-Hosted-Wvb.sh" 1 \
    "$toolset/Wvb/wvhostverifierrequest.wvb" "$test_directory/Verifier-Request.exe" windows \
    >"$test_directory/Verifier-Request-Windows.out" 2>"$test_directory/Verifier-Request-Windows.err"; then
    cat -- "$test_directory/Verifier-Request-Windows.out" "$test_directory/Verifier-Request-Windows.err" >&2
    fail 'cross-target verifier request packaging failed'
fi
[[ ! -s $test_directory/Verifier-Request-Windows.err ]] || fail 'cross-target verifier request packaging wrote a diagnostic'
check_file \
    "$test_directory/Verifier-Request.exe" 200192 \
    32ae4e859fc373acee698e7295837694a859808868232bf2f6328294a6e90e28 \
    'cross-target verifier request package'
cmp --silent -- "$test_directory/Verifier-Request.exe" "$toolset/windows-x64/wvhostverifierrequest.exe" ||
    fail 'cross-target verifier request package differs from the candidate'
check_no_scratch
echo 'PASS  hosted packaging exact cross-target verifier request Windows application'

cp -- "$toolset/SHA256SUMS" "$test_directory/Invalid.wvb" || fail 'invalid input staging failed'
cp -- "$toolset/SHA256SUMS" "$test_directory/Destination.elf" || fail 'sentinel staging failed'
"$script_directory/Package-Hosted-Wvb.sh" 1 \
    "$test_directory/Invalid.wvb" "$test_directory/Destination.elf" \
    >"$test_directory/Invalid.out" 2>"$test_directory/Invalid.err"
status=$?
[[ $status -ne 0 ]] || fail 'invalid WVB was accepted'
check_file \
    "$test_directory/Destination.elf" 6927 \
    430171a9157560acb57e6f84aa772429b436059867892ee2408839057e0eeebc \
    'preserved destination'
check_file \
    "$test_directory/Invalid.wvb" 6927 \
    430171a9157560acb57e6f84aa772429b436059867892ee2408839057e0eeebc \
    'preserved input'
check_no_scratch
echo 'PASS  hosted packaging rejects invalid WVB and preserves resources'
result=0
echo 'Tests: 5, Passed: 5, Failed: 0'
exit "$result"
