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
    for diagnostic in \
        Sha-Build.err Sha-Stage.out Sha-Stage.err Sha-Link.out Sha-Link.err \
        Compiler-Stage.out Compiler-Stage.err; do
        if [[ -n ${test_directory:-} && -f $test_directory/$diagnostic ]]; then
            cat -- "$test_directory/$diagnostic"
        fi
    done
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

"$script_directory/Construct-Segmented-Compiler-Toolset.sh" "$test_directory" || fail
echo 'INFO  segmented compiler toolset reconstruction step=construction status=Complete'

failure_step='WVO staging producer identity'
echo 'START segmented compiler toolset reconstruction phase=WVO-staging-producer item=1/5'
verify_family Wvo-Staging-Producer.wvb \
    windows-x64-wvstage.exe linux-x64-wvstage.elf || fail
pass 'WVO staging producer reconstruction'

failure_step='compiler-image staging identity'
echo 'START segmented compiler toolset reconstruction phase=compiler-image-staging item=2/5'
verify_family Compiler-Image-Staging.wvb \
    windows-x64-wvlinkstage.exe linux-x64-wvlinkstage.elf || fail
pass 'compiler-image staging reconstruction'

failure_step='compiler-image transport identity'
echo 'START segmented compiler toolset reconstruction phase=compiler-image-transport item=3/5'
verify_family Compiler-Image-Canonical-Transport.wvb \
    windows-x64-wvimagetransport.exe linux-x64-wvimagetransport.elf || fail
pass 'compiler-image transport reconstruction'

failure_step='SHA staging smoke build'
sha_wvb="$test_directory/Sha256-Smoke.wvb"
echo 'START segmented compiler toolset reconstruction phase=SHA-staging item=4/5 step=build'
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tests/Windvale-Native-Test-Wvb-To-Wvo-Sha256.wvproj" \
    "$sha_wvb" >"$test_directory/Sha-Build.out" \
    2>"$test_directory/Sha-Build.err" || fail
[[ $(stat -c %s -- "$sha_wvb") == 237 ]] || fail
printf '%s  %s\n' \
    d7962514021a6771efef7894472efabf339014b03051b54d97165cca030dafdf \
    "$sha_wvb" | sha256sum --check --strict --quiet || fail
chmod +x "$test_directory/linux-x64-wvstage.elf" \
    "$test_directory/linux-x64-wvlinkstage.elf" || fail
failure_step='SHA WVO native staging'
"$test_directory/linux-x64-wvstage.elf" "$sha_wvb" \
    "$test_directory/Sha-Object" "$test_directory/Sha-Object.wvop" \
    >"$test_directory/Sha-Stage.out" \
    2>"$test_directory/Sha-Stage.err" || fail
[[ ! -s $test_directory/Sha-Stage.err ]] || fail
[[ $(stat -c %s -- "$test_directory/Sha-Stage.out") == 80 ]] || fail
grep -Fx \
    'native x64 staging status=Complete object-bytes=2860 chunks=6 manifest-bytes=96' \
    "$test_directory/Sha-Stage.out" >/dev/null || fail
failure_step='SHA compiler-image staging'
"$test_directory/linux-x64-wvlinkstage.elf" \
    "$test_directory/Sha-Object" "$test_directory/Sha-Object.wvop" \
    "$test_directory/Sha-Image" "$test_directory/Sha-Image.wvli" \
    >"$test_directory/Sha-Link.out" \
    2>"$test_directory/Sha-Link.err" || fail
[[ ! -s $test_directory/Sha-Link.err ]] || fail
[[ $(stat -c %s -- "$test_directory/Sha-Link.out") == 108 ]] || fail
grep -Fx \
    'segmented compiler image staging status=Complete image-bytes=2672 entry-offset=0 chunks=2 manifest-bytes=52' \
    "$test_directory/Sha-Link.out" >/dev/null || fail
pass 'SHA WVB staging and private-helper image linking'

failure_step='compiler-scale bootstrap analyzer identity'
compiler_wvb="$repository_root/Artifacts/Language-1.0-Target-Aware-Emission-Bootstrap/Wvb/wvanalyze.wvb"
[[ $(stat -c %s -- "$compiler_wvb") == 992412 ]] || fail
printf '%s  %s\n' \
    26ea9bccfe8c2763fb887a5a14c2f0a086a27265523c3df84187b361616f9120 \
    "$compiler_wvb" | sha256sum --check --strict --quiet || fail
echo 'START segmented compiler toolset reconstruction phase=compiler-scale item=5/5'
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
    'native x64 staging status=Complete object-bytes=31736596 chunks=34 manifest-bytes=432' \
    "$test_directory/Compiler-Stage.out" >/dev/null || fail
pass 'compiler-scale WVB staging'

echo "Tests: $tests, Passed: $passed, Failed: 0"
