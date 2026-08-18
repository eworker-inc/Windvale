#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Compiler-Source-Sentinel.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=$(CDPATH= cd -- "${TMPDIR:-/tmp}" && pwd -P) || exit 1
work=$(mktemp -d "$temporary_root/windvale-compiler-source-sentinel.XXXXXXXX") || exit 1
work=$(CDPATH= cd -- "$work" && pwd -P) || exit 1
cleanup() {
    case "$work" in
        "$temporary_root"/windvale-compiler-source-sentinel.*)
            rm -rf -- "$work"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $work" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

echo 'START compiler source sentinel phase=compiler item=1/5'
segmented_report=$("$script_directory/Build-Cached-Segmented-Project.sh" \
    "$repository_root/Projects/Examples/Windvale-Compiler.wvproj" \
    "$repository_root/Artifacts/Native-Compiler-Reconstruction-Candidate/linux-x64/wvbuild.elf" \
    "$work/Compiler.wvb" "$work/Compiler-Image" "$work/Compiler.wvli") || exit $?
printf '%s\n' "$segmented_report"
compiler_entry=$(printf '%s\n' "$segmented_report" | sed -n \
    's/^native segmented project cache status=[A-Za-z]* key=[0-9a-f]* entry-offset=\([0-9][0-9]*\) fragments=[1-8]$/\1/p')
compiler_fragments=$(printf '%s\n' "$segmented_report" | sed -n \
    's/^native segmented project cache status=[A-Za-z]* key=[0-9a-f]* entry-offset=[0-9][0-9]* fragments=\([1-8]\)$/\1/p')
[[ $compiler_entry =~ ^(0|[1-9][0-9]*)$ && $compiler_fragments =~ ^[1-8]$ ]] || exit 1
"$script_directory/Build-Cached-Hosted-Application.sh" 1 \
    "$work/Compiler.wvb" "$work/Compiler-Image" "$compiler_fragments" \
    "$compiler_entry" "$work/Compiler.elf" linux || exit $?
echo 'PASS  compiler source sentinel phase=compiler item=1/5'

echo 'START compiler source sentinel phase=compile item=2/5'
"$work/Compiler.elf" \
    "$repository_root/Tests/Fixtures/Source-Wvb/Function-Only.wv" \
    "$work/Sentinel-A.wvb" >"$work/Compile-A.out" 2>"$work/Compile-A.err" || exit $?
[[ ! -s $work/Compile-A.err ]] || exit 1
cat -- "$work/Compile-A.out"
echo 'PASS  compiler source sentinel phase=compile item=2/5'

echo 'START compiler source sentinel phase=determinism item=3/5'
"$work/Compiler.elf" \
    "$repository_root/Tests/Fixtures/Source-Wvb/Function-Only.wv" \
    "$work/Sentinel-B.wvb" >"$work/Compile-B.out" 2>"$work/Compile-B.err" || exit $?
[[ ! -s $work/Compile-B.err ]] || exit 1
cmp -s -- "$work/Compile-A.out" "$work/Compile-B.out" || exit 1
cmp -s -- "$work/Sentinel-A.wvb" "$work/Sentinel-B.wvb" || exit 1
wvb_bytes=$(wc -c < "$work/Sentinel-A.wvb") || exit $?
echo "PASS  compiler source sentinel phase=determinism item=3/5 bytes=$wvb_bytes"

echo 'START compiler source sentinel phase=verification item=4/5'
"$script_directory/Verify-Wvb.sh" "$work/Sentinel-A.wvb" || exit $?
echo 'PASS  compiler source sentinel phase=verification item=4/5'

echo 'START compiler source sentinel phase=execution item=5/5'
"$script_directory/Run-Wvb.sh" "$work/Sentinel-A.wvb" \
    >"$work/Run.out" 2>"$work/Run.err" || exit $?
[[ ! -s $work/Run.err ]] || exit 1
printf 'Result: 6\n' >"$work/Expected.out"
cmp -s -- "$work/Expected.out" "$work/Run.out" || exit 1
cat -- "$work/Run.out"
echo 'PASS  compiler source sentinel phase=execution item=5/5'
echo 'native compiler source sentinel status=Passed cases=5 source-functions=4 result=6'
