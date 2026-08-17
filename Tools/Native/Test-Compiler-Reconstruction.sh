#!/usr/bin/env bash
set -uo pipefail

development=false
if [[ $# -ne 0 ]]; then
    if [[ $# -ne 1 || $1 != '--development' ]]; then
        echo 'Usage: ./Tools/Native/Test-Compiler-Reconstruction.sh [--development]' >&2
        exit 64
    fi
    development=true
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
candidate="$repository_root/Artifacts/Native-Compiler-Reconstruction-Candidate"
tests=0
passed=0

verify_file() {
    local path=$1
    local expected_bytes=$2
    local expected_sha256=$3
    [[ -f $path ]] || return 1
    local actual_bytes
    actual_bytes=$(wc -c < "$path") || return 1
    [[ $actual_bytes -eq $expected_bytes ]] || return 1
    local digest_line
    local actual_sha256
    digest_line=$(sha256sum -- "$path") || return 1
    actual_sha256=${digest_line%% *}
    [[ $actual_sha256 == "$expected_sha256" ]]
}
pass() {
    tests=$((tests + 1))
    passed=$((passed + 1))
    echo "PASS  $1"
}
fail() {
    tests=$((tests + 1))
    echo 'FAIL  compiler reconstruction'
    echo "Tests: $tests, Passed: $passed, Failed: $((tests - passed))"
    exit 1
}

verify_file "$candidate/Wvb/Windvale-Compiler.wvb" 935163 \
    a7d47b2de29faee089c7a22ef23eac4657f719331dc02044eb2d818457dac5b6 || fail
verify_file "$candidate/windows-x64/wvcompiler.exe" 28172800 \
    a5db938a814471fdacda75efcf57d28934ae52b3b2290732627c14ba173fd70d || fail
verify_file "$candidate/linux-x64/wvcompiler.elf" 28172288 \
    da11ab3b70b428087cbcb9de5614a2dbdccd31afc6861cc15881fd65c12ff19b || fail
verify_file "$candidate/Wvb/Compiler-Build-Driver.wvb" 1142818 \
    125d2b4080889615877d843a36b2f9f6b50d049d011cc06fa8ab426ab83c0574 || fail
verify_file "$candidate/windows-x64/wvbuild.exe" 30071296 \
    f556f0e2c794d9424cbcd9f5e3f8e5aee54f49373c7c18ea1d4829facea7dc6f || fail
verify_file "$candidate/linux-x64/wvbuild.elf" 30072832 \
    628fd60ea702c4a3b3ffb01d32cba7ba9708477acccf190cc6506a56f159d7a9 || fail
pass 'candidate inventory'

if "$script_directory/Construct-Compiler-Reconstruction.sh" >/dev/null 2>&1; then
    fail
elif [[ $? -ne 64 ]]; then
    fail
fi
pass 'usage rejection'

temporary_root=${TMPDIR:-/tmp}
test_directory=$(mktemp -d "$temporary_root/windvale-compiler-reconstruction-test.XXXXXXXX") || fail
cleanup() {
    case "$test_directory" in
        "$temporary_root"/windvale-compiler-reconstruction-test.*)
            rm -rf -- "$test_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $test_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

if $development; then
    "$candidate/linux-x64/wvcompiler.elf" \
        "$repository_root/Tests/Fixtures/Source-Wvb/Function-Only.wv" \
        "$test_directory/Direct.wvb" \
        >"$test_directory/Direct.out" 2>"$test_directory/Direct.err" || fail
    "$script_directory/Build-Current-Wvb.sh" \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Function-Only.wvproj" \
        "$test_directory/Project.wvb" \
        >"$test_directory/Project.out" 2>"$test_directory/Project.err" || fail
    verify_file "$test_directory/Direct.wvb" 816 \
        28d215b982a7b7185cfa80c4cc5346666bd0181582fe80bec8b7035d514da936 || fail
    verify_file "$test_directory/Project.wvb" 816 \
        28d215b982a7b7185cfa80c4cc5346666bd0181582fe80bec8b7035d514da936 || fail
    cmp -- "$test_directory/Direct.wvb" "$test_directory/Project.wvb" || fail
    "$script_directory/Verify-Wvb.sh" "$test_directory/Direct.wvb" \
        >"$test_directory/Verify.out" 2>"$test_directory/Verify.err" || fail
    [[ ! -s $test_directory/Direct.err ]] || fail
    [[ ! -s $test_directory/Project.err ]] || fail
    [[ ! -s $test_directory/Verify.err ]] || fail
    pass 'current candidate compiler and build-driver smoke'
    echo "Tests: $tests, Passed: $passed, Failed: 0"
    exit 0
fi

"$script_directory/Construct-Compiler-Reconstruction.sh" "$test_directory" \
    >"$test_directory/Construct.out" 2>"$test_directory/Construct.err" || fail
grep -Fx 'native compiler reconstruction status=Complete compiler-bytes=935163 native-bytes=28141686 entry-offset=51356 chunks=7 build-driver-bytes=1142818 build-driver-entry-offset=220460 build-driver-chunks=8' \
    "$test_directory/Construct.out" >/dev/null || fail
[[ ! -s $test_directory/Construct.err ]] || fail
verify_file "$test_directory/Wvb/Windvale-Compiler.wvb" 935163 \
    a7d47b2de29faee089c7a22ef23eac4657f719331dc02044eb2d818457dac5b6 || fail
verify_file "$test_directory/windows-x64/wvcompiler.exe" 28172800 \
    a5db938a814471fdacda75efcf57d28934ae52b3b2290732627c14ba173fd70d || fail
verify_file "$test_directory/linux-x64/wvcompiler.elf" 28172288 \
    da11ab3b70b428087cbcb9de5614a2dbdccd31afc6861cc15881fd65c12ff19b || fail
verify_file "$test_directory/Wvb/Compiler-Build-Driver.wvb" 1142818 \
    125d2b4080889615877d843a36b2f9f6b50d049d011cc06fa8ab426ab83c0574 || fail
verify_file "$test_directory/windows-x64/wvbuild.exe" 30071296 \
    f556f0e2c794d9424cbcd9f5e3f8e5aee54f49373c7c18ea1d4829facea7dc6f || fail
verify_file "$test_directory/linux-x64/wvbuild.elf" 30072832 \
    628fd60ea702c4a3b3ffb01d32cba7ba9708477acccf190cc6506a56f159d7a9 || fail
pass 'native paired reconstruction'

echo "Tests: $tests, Passed: $passed, Failed: 0"
