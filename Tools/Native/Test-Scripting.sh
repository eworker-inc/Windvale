#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Scripting.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
work=$(mktemp -d "$temporary_root/windvale-scripting.XXXXXXXX") || exit 1
cleanup() {
    case "$work" in
        "$temporary_root"/windvale-scripting.*) rm -rf -- "$work" ;;
        *) echo "Refusing to remove unexpected temporary path: $work" >&2; return 1 ;;
    esac
}
trap cleanup EXIT

bin=$work/Installed/bin
mkdir -p -- "$bin"
cp -- "$repository_root/Distribution/Installers/Templates/linux-x64/wv" "$bin/wv"
cp -- "$repository_root/Artifacts/Native-Front-Door/linux-x64/wvbuild.elf" "$bin/wvbuild"
cp -- "$repository_root/Artifacts/Native-Front-Door/linux-x64/wvverify.elf" "$bin/wvverify"
cp -- "$repository_root/Artifacts/Native-Wvb-Runner-Candidate/linux-x64-wvrun.elf" "$bin/wvrun"
chmod 755 "$bin/wv" "$bin/wvbuild" "$bin/wvverify" "$bin/wvrun"

echo 'START native scripting cases=6'
set +e
"$bin/wv" run >"$work/Usage.out" 2>"$work/Usage.err"
status=$?
set -e
[[ $status -eq 64 ]]
grep -Fqx 'Usage: wv run <source.wv> [argument ...]' "$work/Usage.err"
echo 'PASS  native scripting case=usage'

"$bin/wv" run "$repository_root/Tests/Fixtures/Scripting/Portable-Main.wv" >"$work/Portable.out" 2>"$work/Portable.err"
[[ ! -s $work/Portable.out && ! -s $work/Portable.err ]]
echo 'PASS  native scripting case=portable'

set +e
"$bin/wv" run "$repository_root/Tests/Fixtures/Scripting/Arguments-And-Output.wv" -flag 'snow day' >"$work/Arguments.out" 2>"$work/Arguments.err"
status=$?
set -e
[[ $status -eq 7 ]]
grep -Fqx 'first=-flag' "$work/Arguments.out"
grep -Fqx 'second=snow day' "$work/Arguments.err"
echo 'PASS  native scripting case=arguments'

set +e
"$bin/wv" run "$repository_root/Tests/Fixtures/Scripting/Unsupported-Authority.wv" >"$work/Authority.out" 2>"$work/Authority.err"
status=$?
set -e
[[ $status -eq 1 ]]
if ! grep -Fqx 'wvb run status=Unsupported profile=script-main-i32 phase=envelope' "$work/Authority.err"; then
    cat -- "$work/Authority.err" >&2
    exit 1
fi
echo 'PASS  native scripting case=authority'

set +e
"$bin/wv" run "$repository_root/Tests/Fixtures/Scripting/Malformed.wv" >"$work/Malformed.out" 2>"$work/Malformed.err"
status=$?
set -e
[[ $status -eq 1 ]]
[[ ! -e $repository_root/Tests/Fixtures/Scripting/Malformed.wvb ]]
echo 'PASS  native scripting case=malformed'

set +e
"$bin/wv" run "$repository_root/Tests/Fixtures/Scripting/Arguments-And-Output.wv" -- value >"$work/Dash.out" 2>"$work/Dash.err"
status=$?
set -e
[[ $status -eq 7 ]]
grep -Fqx 'first=--' "$work/Dash.out"
grep -Fqx 'second=value' "$work/Dash.err"
echo 'PASS  native scripting case=dash-argument'

echo 'PASS  native scripting compile=hidden verification=mandatory arguments=immutable authority=base-only cleanup=verified'
echo 'Tests: 6, Passed: 6, Failed: 0'
