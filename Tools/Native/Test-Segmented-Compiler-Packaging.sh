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
current_lowerer_wvb="$temporary_directory/Current-Lowerer.wvb"
current_lowerer="$temporary_directory/Current-Lowerer.elf"
descriptor_wvb="$temporary_directory/Descriptor-Main.wvb"
descriptor_wvo="$temporary_directory/Descriptor-Main.wvo"
bridge_wvo="$temporary_directory/Baseline-Jit-Patch-Plan-Bridge.wvo"
"$script_directory/Package-Segmented-Compiler-Wvb.sh" 6 \
    "$candidate/Compiler-Image-Staging.wvb" "$output" >/dev/null || exit $?
actual_bytes=$(wc -c < "$output") || exit 1
[[ $actual_bytes -eq 851968 ]] || {
    echo "Expected 851968 output bytes, found $actual_bytes." >&2
    exit 1
}
actual_line=$(sha256sum -- "$output") || exit 1
actual_sha256=${actual_line%% *}
[[ $actual_sha256 == '28dad5b1be0795c5372887ed11e6dc4a6e826dc8952f0f8a6d97f187666328ff' ]] || {
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

"$script_directory/Build-Wvb.sh" \
    "$repository_root/Windvale-Native-X64-Lowering-Tool.wvproj" \
    "$current_lowerer_wvb" >/dev/null || exit $?
current_lowerer_wvb_bytes=$(wc -c < "$current_lowerer_wvb") || exit 1
[[ $current_lowerer_wvb_bytes -eq 399691 ]] || {
    echo "Expected 399691 current-lowerer WVB bytes, found $current_lowerer_wvb_bytes." >&2
    exit 1
}
current_lowerer_wvb_line=$(sha256sum -- "$current_lowerer_wvb") || exit 1
current_lowerer_wvb_sha256=${current_lowerer_wvb_line%% *}
[[ $current_lowerer_wvb_sha256 == '92655af0632b4dd3525c2b2de98353b095fa1df94b524a94aa47f16014f1e508' ]] || {
    echo "Unexpected current-lowerer WVB digest: $current_lowerer_wvb_sha256" >&2
    exit 1
}
"$script_directory/Package-Segmented-Compiler-Wvb.sh" 6 \
    "$current_lowerer_wvb" "$current_lowerer" >/dev/null || exit $?
current_lowerer_bytes=$(wc -c < "$current_lowerer") || exit 1
[[ $current_lowerer_bytes -eq 5791744 ]] || {
    echo "Expected 5791744 current Linux lowerer bytes, found $current_lowerer_bytes." >&2
    exit 1
}
current_lowerer_line=$(sha256sum -- "$current_lowerer") || exit 1
current_lowerer_sha256=${current_lowerer_line%% *}
[[ $current_lowerer_sha256 == 'a9d4ae08d449aa2b1238120efb6bab9720e97f2e2a99354abf15bf086be4cb1e' ]] || {
    echo "Unexpected current Linux lowerer digest: $current_lowerer_sha256" >&2
    exit 1
}
[[ -x $current_lowerer ]] || {
    echo 'The current Linux lowerer package is not executable.' >&2
    exit 1
}

"$script_directory/Build-Wvb.sh" \
    "$repository_root/Windvale-Native-Test-Wvb-To-Wvo-Descriptor-Main.wvproj" \
    "$descriptor_wvb" >/dev/null || exit $?
"$current_lowerer" "$descriptor_wvb" "$descriptor_wvo" >/dev/null || exit $?
descriptor_wvo_bytes=$(wc -c < "$descriptor_wvo") || exit 1
[[ $descriptor_wvo_bytes -eq 793 ]] || {
    echo "Expected 793 descriptor-Main WVO bytes, found $descriptor_wvo_bytes." >&2
    exit 1
}
descriptor_wvo_line=$(sha256sum -- "$descriptor_wvo") || exit 1
descriptor_wvo_sha256=${descriptor_wvo_line%% *}
[[ $descriptor_wvo_sha256 == '9936663f45c194441bfc5e8464286e57f83cd3a18948597a8011af608a4faa51' ]] || {
    echo "Unexpected descriptor-Main WVO digest: $descriptor_wvo_sha256" >&2
    exit 1
}

"$current_lowerer" \
    "$repository_root/Artifacts/Baseline-Jit-Publisher/Wvb/Baseline-Jit-Patch-Plan-Bridge.wvb" \
    "$bridge_wvo" >/dev/null || exit $?
cmp --silent -- "$bridge_wvo" \
    "$repository_root/Artifacts/Baseline-Jit-Publisher/Wvo/Baseline-Jit-Patch-Plan-Bridge.wvo" || {
    echo 'The current native lowerer did not reproduce the retained baseline-JIT bridge WVO.' >&2
    exit 1
}

echo 'PASS  segmented compiler packaging reconstructs the current native lowerer'
echo 'Tests: 2, Passed: 2, Failed: 0'
