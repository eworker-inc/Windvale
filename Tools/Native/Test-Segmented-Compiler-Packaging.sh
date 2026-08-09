#!/usr/bin/env bash
set -uo pipefail

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
candidate="$repository_root/Artifacts/Native-Segmented-Compiler-Toolset-Candidate"
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-segmented-compiler-package-test.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-segmented-compiler-package-test.*)
            rm -f -- "$temporary_directory"/*
            rmdir -- "$temporary_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $temporary_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

output="$temporary_directory/Compiler-Image-Staging.elf"
"$script_directory/Package-Segmented-Compiler-Wvb.sh" 6 \
    "$candidate/Compiler-Image-Staging.wvb" "$output" >/dev/null || exit $?
actual_bytes=$(wc -c < "$output") || exit 1
[[ $actual_bytes -eq 851968 ]] || {
    echo "Expected 851968 output bytes, found $actual_bytes." >&2
    exit 1
}
actual_line=$(sha256sum -- "$output") || exit 1
actual_sha256=${actual_line%% *}
[[ $actual_sha256 == '02b07d23b763fa4dd2d11bb9c9ca94be32bdbd698b1f9ce7b466af90b768eef8' ]] || {
    echo "Unexpected Linux output digest: $actual_sha256" >&2
    exit 1
}
cmp --silent -- "$output" "$candidate/linux-x64-wvlinkstage.elf" || {
    echo 'The segmented Linux package differs from its pinned candidate.' >&2
    exit 1
}
[[ -x $output ]] || {
    echo 'The segmented Linux package is not executable.' >&2
    exit 1
}

echo 'PASS  segmented compiler packaging reproduces exact Linux application'
echo 'Tests: 1, Passed: 1, Failed: 0'
