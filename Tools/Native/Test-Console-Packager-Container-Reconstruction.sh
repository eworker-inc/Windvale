#!/usr/bin/env bash
set -uo pipefail

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
ordinary_candidate="$repository_root/Artifacts/Native-Console-Packager-Candidate"
segmented_candidate="$repository_root/Artifacts/Native-Console-Segmented-Packager-Candidate"
tests=0
passed=0

check_file() {
    local path=$1
    local expected_bytes=$2
    local expected_sha256=$3
    local actual_bytes
    local digest_line
    local actual_sha256
    [[ -f $path ]] || return 1
    actual_bytes=$(wc -c < "$path") || return 1
    [[ $actual_bytes -eq $expected_bytes ]] || return 1
    digest_line=$(sha256sum -- "$path") || return 1
    actual_sha256=${digest_line%% *}
    [[ $actual_sha256 == "$expected_sha256" ]]
}
check_equal() {
    [[ -f $1 && -f $2 ]] || return 1
    cmp --silent -- "$1" "$2"
}
pass() {
    tests=$((tests + 1))
    passed=$((passed + 1))
    echo "PASS  $1"
}
fail() {
    tests=$((tests + 1))
    echo 'FAIL  console packager container reconstruction'
    echo "Tests: $tests, Passed: $passed, Failed: $((tests - passed))"
    exit 1
}

check_file "$ordinary_candidate/Console-Packager.wvb" 60797 \
    f4c75495321736bbce22582213133e7cc09157a8439dc198d9848ec95683e89c || fail
check_file "$ordinary_candidate/Console-Packager.exe" 708608 \
    0dddbe6cfd38c37e3fd5332567b3323480a5548a6fbeb41b6b50aed0e57ac3d2 || fail
check_file "$ordinary_candidate/Console-Packager.elf" 708608 \
    d399c935e906ab42d7572e337226577055396cb6204766106e21790e22ea43af || fail
pass 'ordinary candidate inventory'

check_file "$segmented_candidate/Console-Segmented-Packager.wvb" 70033 \
    c4941f396f76467cb6455472f7f4711c21a6f65c12c09a9b5f4135987628f20e || fail
check_file "$segmented_candidate/Console-Segmented-Packager.exe" 805376 \
    954c4b2aaba56149c21e16e19ca6f16434069513e1d1b3034423dab457635412 || fail
check_file "$segmented_candidate/Console-Segmented-Packager.elf" 806912 \
    8916fb509f81e29dabca7ed0202c0ad250f129e78b70b701630dbfcd55a1d30d || fail
pass 'segmented candidate inventory'

if "$script_directory/Construct-Console-Packager-Reconstruction.sh" >/dev/null 2>&1; then
    fail
elif [[ $? -ne 64 ]]; then
    fail
fi

temporary_root=${TMPDIR:-/tmp}
test_directory=$(mktemp -d "$temporary_root/windvale-console-packager-container-reconstruction-test.XXXXXXXX") || fail
cleanup() {
    case "$test_directory" in
        "$temporary_root"/windvale-console-packager-container-reconstruction-test.*)
            rm -rf -- "$test_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $test_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

"$script_directory/Construct-Console-Packager-Reconstruction.sh" "$test_directory" \
    >"$test_directory/Construct.out" 2>"$test_directory/Construct.err" || fail
printf '%s\n' 'native console packager reconstruction status=Complete families=2 artifacts=6' \
    >"$test_directory/Expected.out" || fail
check_equal "$test_directory/Construct.out" "$test_directory/Expected.out" || fail
[[ ! -s $test_directory/Construct.err ]] || fail

ordinary_output="$test_directory/Native-Console-Packager-Candidate"
check_equal "$ordinary_output/Console-Packager.wvb" "$ordinary_candidate/Console-Packager.wvb" || fail
check_equal "$ordinary_output/Console-Packager.exe" "$ordinary_candidate/Console-Packager.exe" || fail
check_equal "$ordinary_output/Console-Packager.elf" "$ordinary_candidate/Console-Packager.elf" || fail
pass 'ordinary container reconstruction'

segmented_output="$test_directory/Native-Console-Segmented-Packager-Candidate"
check_equal "$segmented_output/Console-Segmented-Packager.wvb" "$segmented_candidate/Console-Segmented-Packager.wvb" || fail
check_equal "$segmented_output/Console-Segmented-Packager.exe" "$segmented_candidate/Console-Segmented-Packager.exe" || fail
check_equal "$segmented_output/Console-Segmented-Packager.elf" "$segmented_candidate/Console-Segmented-Packager.elf" || fail
pass 'segmented container reconstruction'

echo "Tests: $tests, Passed: $passed, Failed: 0"
