#!/usr/bin/env bash
set -uo pipefail

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
candidate="$repository_root/Artifacts/Native-Segmented-Compiler-Toolset-Candidate"
tests=0
passed=0
failure_step='usage-contract'

pass() {
    tests=$((tests + 1))
    passed=$((passed + 1))
    echo "PASS  $1"
}
fail() {
    tests=$((tests + 1))
    echo "FAIL  step=$failure_step"
    if [[ -n ${test_directory:-} ]]; then
        [[ -f $test_directory/Construct.out ]] &&
            cat -- "$test_directory/Construct.out"
        [[ -f $test_directory/Construct.err ]] &&
            cat -- "$test_directory/Construct.err" >&2
    fi
    echo 'FAIL  segmented compiler toolset reconstruction'
    echo "Tests: $tests, Passed: $passed, Failed: $((tests - passed))"
    exit 1
}
verify_exact() {
    [[ -f $1 && -f $2 ]] || return 1
    cmp --silent -- "$1" "$2"
}
verify_family() {
    verify_exact "$test_directory/$1" "$candidate/$1" || return 1
    verify_exact "$test_directory/$2" "$candidate/$2" || return 1
    verify_exact "$test_directory/$3" "$candidate/$3"
}

echo 'START segmented compiler toolset reconstruction step=construction'
if "$script_directory/Construct-Segmented-Compiler-Toolset.sh" >/dev/null 2>&1; then
    fail
elif [[ $? -ne 64 ]]; then
    fail
fi

failure_step='construction'
temporary_root=${TMPDIR:-/tmp}
test_directory=$(mktemp -d "$temporary_root/windvale-segmented-toolset-reconstruction-test.XXXXXXXX") || fail
cleanup() {
    case "$test_directory" in
        "$temporary_root"/windvale-segmented-toolset-reconstruction-test.*)
            rm -rf -- "$test_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $test_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

"$script_directory/Construct-Segmented-Compiler-Toolset.sh" "$test_directory" \
    >"$test_directory/Construct.out" 2>"$test_directory/Construct.err" || fail
grep -Fx 'native segmented compiler toolset construction status=Complete artifacts=9' \
    "$test_directory/Construct.out" >/dev/null || fail
[[ ! -s $test_directory/Construct.err ]] || fail
echo 'INFO  segmented compiler toolset reconstruction step=construction status=Complete'

failure_step='WVO staging producer identity'
echo 'START segmented compiler toolset reconstruction phase=WVO-staging-producer item=1/4'
verify_family Wvo-Staging-Producer.wvb \
    windows-x64-wvstage.exe linux-x64-wvstage.elf || fail
pass 'WVO staging producer reconstruction'

failure_step='compiler-image staging identity'
echo 'START segmented compiler toolset reconstruction phase=compiler-image-staging item=2/4'
verify_family Compiler-Image-Staging.wvb \
    windows-x64-wvlinkstage.exe linux-x64-wvlinkstage.elf || fail
pass 'compiler-image staging reconstruction'

failure_step='compiler-image transport identity'
echo 'START segmented compiler toolset reconstruction phase=compiler-image-transport item=3/4'
verify_family Compiler-Image-Canonical-Transport.wvb \
    windows-x64-wvimagetransport.exe linux-x64-wvimagetransport.elf || fail
pass 'compiler-image transport reconstruction'

failure_step='compiler-scale bootstrap analyzer identity'
compiler_wvb="$repository_root/Artifacts/Language-1.0-Target-Aware-Emission-Bootstrap/Wvb/wvanalyze.wvb"
[[ $(stat -c %s -- "$compiler_wvb") == 992412 ]] || fail
printf '%s  %s\n' \
    26ea9bccfe8c2763fb887a5a14c2f0a086a27265523c3df84187b361616f9120 \
    "$compiler_wvb" | sha256sum --check --strict --quiet || fail
echo 'START segmented compiler toolset reconstruction phase=compiler-scale item=4/4'
echo 'INFO  segmented compiler toolset reconstruction phase=compiler-scale step=input-identity status=Complete bytes=992412'
chmod +x "$test_directory/linux-x64-wvstage.elf" || fail
failure_step='compiler-scale native staging'
echo 'START segmented compiler toolset reconstruction phase=compiler-scale step=native-staging'
"$test_directory/linux-x64-wvstage.elf" "$compiler_wvb" \
    "$test_directory/Compiler-Object" "$test_directory/Compiler-Object.wvop" \
    >"$test_directory/Compiler-Stage.out" \
    2>"$test_directory/Compiler-Stage.err" || fail
failure_step='compiler-scale native staging diagnostic'
[[ ! -s $test_directory/Compiler-Stage.err ]] || fail
failure_step='compiler-scale native staging report'
grep -Fx \
    'native x64 staging status=Complete object-bytes=31736596 chunks=41 manifest-bytes=516' \
    "$test_directory/Compiler-Stage.out" >/dev/null || fail
pass 'compiler-scale WVB staging'

echo "Tests: $tests, Passed: $passed, Failed: 0"
