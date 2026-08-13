#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Wv-Linker-Reconstruction.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
candidate="$repository_root/Artifacts/Native-Wv-Linker-Candidate"
tests=0
passed=0

check_file() {
    local path=$1 expected_bytes=$2 expected_sha256=$3
    local actual_bytes digest_line actual_sha256
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
    echo 'FAIL  Wv-Linker reconstruction'
    echo "Tests: $tests, Passed: $passed, Failed: $((tests - passed))"
    exit 1
}

check_file "$candidate/Wv-Linker.wvb" 135740 \
    02f727a8ce2d6826c8414cada0933c7d5a54893ea061621d08147984c3d6f874 || fail
check_file "$candidate/Wv-Linker.wvo" 1786271 \
    0141219773241e8780e2520f30ab8377914bf89a72f57da091871ac40d68a287 || fail
check_file "$candidate/Wv-Linker.bin" 1777781 \
    d30e0c4dce7159bf98c546a0200e8b541797612ab67d6f21e3d8ee876af27480 || fail
check_file "$candidate/Wv-Linker.exe" 1796608 \
    f47a952867203fbff53abb131ea155b4fe9e14a8be153cc61c0ca5fd8e4a74e0 || fail
check_file "$candidate/Wv-Linker.elf" 1798144 \
    8a220bfd6c7ef684897583e728419ecd6d383c8e8cf40094edbcfb695e3d6d7a || fail
[[ -x $candidate/Wv-Linker.elf ]] || fail
pass 'candidate inventory'

if "$script_directory/Construct-Wv-Linker-Reconstruction.sh" >/dev/null 2>&1; then
    fail
elif [[ $? -ne 64 ]]; then
    fail
fi

temporary_root=${TMPDIR:-/tmp}
test_directory=$(mktemp -d "$temporary_root/windvale-wv-linker-reconstruction-test.XXXXXXXX") || fail
cleanup() {
    case "$test_directory" in
        "$temporary_root"/windvale-wv-linker-reconstruction-test.*)
            rm -rf -- "$test_directory"
            ;;
        *)
            echo "Refusing to remove unexpected path: $test_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

"$script_directory/Construct-Wv-Linker-Reconstruction.sh" "$test_directory" \
    >"$test_directory/Construct.out" 2>"$test_directory/Construct.err" || fail
printf '%s\n' 'native Wv-Linker reconstruction status=Complete artifacts=5' \
    >"$test_directory/Construct.expected" || fail
check_equal "$test_directory/Construct.out" "$test_directory/Construct.expected" || fail
[[ ! -s $test_directory/Construct.err ]] || fail
check_equal "$test_directory/Wv-Linker.wvb" "$candidate/Wv-Linker.wvb" || fail
check_equal "$test_directory/Wv-Linker.wvo" "$candidate/Wv-Linker.wvo" || fail
check_equal "$test_directory/Wv-Linker.bin" "$candidate/Wv-Linker.bin" || fail
check_equal "$test_directory/Wv-Linker.exe" "$candidate/Wv-Linker.exe" || fail
check_equal "$test_directory/Wv-Linker.elf" "$candidate/Wv-Linker.elf" || fail
pass 'exact independent paired reconstruction'

main="$test_directory/Main.wvo"
provider="$test_directory/Provider.wvo"
"$script_directory/Assemble-Wva.sh" \
    "$repository_root/Examples/Assembler/Hello-Object.wva" "$main" \
    >"$test_directory/Main-Assemble.out" 2>"$test_directory/Main-Assemble.err" || fail
[[ ! -s $test_directory/Main-Assemble.err ]] || fail
"$script_directory/Assemble-Wva.sh" \
    "$repository_root/Examples/Linker/Console-Provider.wva" "$provider" \
    >"$test_directory/Provider-Assemble.out" 2>"$test_directory/Provider-Assemble.err" || fail
[[ ! -s $test_directory/Provider-Assemble.err ]] || fail
check_file "$main" 218 \
    992c298a4f9b68dec27b7203a2770f2a37ef2016ea45e88d33ee21994060fe85 || fail
check_file "$provider" 91 \
    486134e34bb32abadd233d1c3303acd9c313aa69d3874cafdce0fcb61b6e72ab || fail
application="$test_directory/Wv-Linker.elf"
image="$test_directory/Application.bin"
map="$test_directory/Application.wvmap"
application_error="$test_directory/Application.err"
"$application" 0 Main "$image" "$main" "$provider" >"$map" 2>"$application_error" || fail
check_file "$image" 24 \
    7612954be9dc08e12ab06510e6539a37ab797bc381ee8844908b5f7c475d16a5 || fail
check_file "$map" 1644 \
    df43f1b8381a7f5778bbb81a0d6b3fd589f0565603eef5296e2816146816ea97 || fail
[[ ! -s $application_error ]] || fail

if "$application" 0 Main >"$test_directory/Usage.out" 2>"$test_directory/Usage.err"; then
    fail
elif [[ $? -ne 64 ]]; then
    fail
fi
[[ ! -s $test_directory/Usage.out ]] || fail
check_file "$test_directory/Usage.err" 85 \
    c7a8e24b9be3d5a2678c5eb27bd88a39019694177fa970ece70dab92da2e8eee || fail
check_file "$main" 218 \
    992c298a4f9b68dec27b7203a2770f2a37ef2016ea45e88d33ee21994060fe85 || fail
check_file "$provider" 91 \
    486134e34bb32abadd233d1c3303acd9c313aa69d3874cafdce0fcb61b6e72ab || fail
pass 'current-host link, usage, and input preservation'

echo "Tests: $tests, Passed: $passed, Failed: 0"
