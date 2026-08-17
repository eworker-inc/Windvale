#!/usr/bin/env bash
set -uo pipefail

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
candidate="$repository_root/Artifacts/Native-Segmented-Compiler-Toolset-Candidate"
tests=0
passed=0

pass() {
    tests=$((tests + 1))
    passed=$((passed + 1))
    echo "PASS  $1"
}
fail() {
    tests=$((tests + 1))
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

if "$script_directory/Construct-Segmented-Compiler-Toolset.sh" >/dev/null 2>&1; then
    fail
elif [[ $? -ne 64 ]]; then
    fail
fi

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

verify_family Wvo-Staging-Producer.wvb \
    windows-x64-wvstage.exe linux-x64-wvstage.elf || fail
pass 'WVO staging producer reconstruction'

verify_family Compiler-Image-Staging.wvb \
    windows-x64-wvlinkstage.exe linux-x64-wvlinkstage.elf || fail
pass 'compiler-image staging reconstruction'

verify_family Compiler-Image-Canonical-Transport.wvb \
    windows-x64-wvimagetransport.exe linux-x64-wvimagetransport.elf || fail
pass 'compiler-image transport reconstruction'

build_driver="$repository_root/Artifacts/Native-Front-Door/linux-x64/wvbuild.elf"
printf '%s  %s\n' \
    d228db89c17cc8124776d6bd39cb061a1414168a22ca075168e44439b1253969 \
    "$build_driver" | sha256sum --check --strict --quiet || fail
workspace="$repository_root/Windvale.wvws"
compiler_project="$repository_root/Projects/Tools/Windvale-Compiler-Build-Driver.wvproj"
compiler_wvb="$test_directory/Compiler-Build-Driver.wvb"
"$build_driver" --workspace "$workspace" --project "$compiler_project" \
    "$compiler_wvb" >"$test_directory/Compiler-Build.out" \
    2>"$test_directory/Compiler-Build.err" || fail
[[ ! -s $test_directory/Compiler-Build.err ]] || fail
[[ $(stat -c %s -- "$compiler_wvb") == 1142818 ]] || fail
printf '%s  %s\n' \
    125d2b4080889615877d843a36b2f9f6b50d049d011cc06fa8ab426ab83c0574 \
    "$compiler_wvb" | sha256sum --check --strict --quiet || fail
chmod +x "$test_directory/linux-x64-wvstage.elf" || fail
"$test_directory/linux-x64-wvstage.elf" "$compiler_wvb" \
    "$test_directory/Compiler-Object" "$test_directory/Compiler-Object.wvop" \
    >"$test_directory/Compiler-Stage.out" \
    2>"$test_directory/Compiler-Stage.err" || fail
[[ ! -s $test_directory/Compiler-Stage.err ]] || fail
grep -Fx \
    'native x64 staging status=Complete object-bytes=30378291 chunks=39 manifest-bytes=492' \
    "$test_directory/Compiler-Stage.out" >/dev/null || fail
pass 'compiler-scale WVB staging'

echo "Tests: $tests, Passed: $passed, Failed: 0"
