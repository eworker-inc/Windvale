#!/usr/bin/env bash
set -uo pipefail

development_target=''
development_cache=false
case $# in
    0) ;;
    1)
        [[ $1 == '--development-all' ]] || exit 64
        development_cache=true
        ;;
    2)
        [[ $1 == '--development-target' && $2 =~ ^[a-z0-9][a-z0-9-]*$ ]] || exit 64
        development_target=$2
        development_cache=true
        ;;
    *) exit 64 ;;
esac

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
target_plan=$repository_root/Tests/Native/Os-X64-Code-Emission-Development-Targets.txt
front_door=$repository_root/Artifacts/Native-Front-Door
build_driver=$front_door/linux-x64/wvbuild.elf
wvb_publisher=$front_door/linux-x64/wvpublish.elf
lowerer=$repository_root/Artifacts/Native-Wvb-To-Wvo-Candidate/Wvb-To-Wvo.elf
wvo_publisher=$repository_root/Artifacts/Native-Wvo-Publisher-Candidate/linux-x64-wvopublish.elf
linker=$repository_root/Artifacts/Native-Wv-Linker-Candidate/Wv-Linker.elf
packager=$repository_root/Artifacts/Native-Console-Packager-Candidate/Console-Packager.elf
console_publisher=$repository_root/Artifacts/Native-Console-Application-Publisher-Candidate/linux-x64-wvappublish.elf
cached_project_builder=$repository_root/Tools/Native/Build-Cached-Os-X64-Project-Wvbs.mjs
temporary_root=${TMPDIR:-/tmp}
work=$(mktemp -d "$temporary_root/windvale-os-x64-code-emission.XXXXXXXX") || exit 1
cleanup() {
    case "$work" in
        "$temporary_root"/windvale-os-x64-code-emission.*)
            rm -f -- "$work"/*
            rmdir -- "$work"
            ;;
        *) return 1 ;;
    esac
}
trap cleanup EXIT

verify() {
    local path=$1 bytes=$2 digest=$3
    [[ $(wc -c < "$path") -eq $bytes ]] &&
        printf '%s  %s\n' "$digest" "$path" |
            sha256sum --check --strict --quiet
}

verify_tool() {
    local path=$1 digest=$2
    printf '%s  %s\n' "$digest" "$path" |
        sha256sum --check --strict --quiet
}

run_case() {
    local candidate_wvb=$work/$artifact.candidate.wvb
    local candidate_wvo=$work/$artifact.candidate.wvo
    local candidate_exe=$work/$artifact.candidate.exe
    local candidate_elf=$work/$artifact.candidate.elf
    if [[ $development_cache != true ]]; then
        "$build_driver" --workspace "$workspace_path" \
            --project "$repository_root/$project" "$candidate_wvb" >/dev/null || return $?
    fi
    [[ -f $candidate_wvb ]] || return 1
    "$wvb_publisher" "$candidate_wvb" "$work/$artifact.wvb" >/dev/null || return $?
    verify "$work/$artifact.wvb" "$wvb_bytes" "$wvb_sha256" || return 1
    "$lowerer" "$work/$artifact.wvb" "$candidate_wvo" >/dev/null || return $?
    "$wvo_publisher" "$candidate_wvo" "$work/$artifact.wvo" >/dev/null || return $?
    verify "$work/$artifact.wvo" "$wvo_bytes" "$wvo_sha256" || return 1
    "$linker" 0 Main "$work/$artifact.bin" "$work/$artifact.wvo" >/dev/null
    local linker_status=$?
    if ((linker_status != 0)); then
        ((linker_status == 73)) && return 1
        return "$linker_status"
    fi
    verify "$work/$artifact.bin" "$bin_bytes" "$bin_sha256" || return 1
    "$packager" linux-x64-console-v1 "$work/$artifact.bin" 0 "$candidate_elf" \
        >/dev/null || return $?
    "$console_publisher" "$candidate_elf" "$work/$artifact.elf" \
        >/dev/null || return $?
    verify "$work/$artifact.elf" "$linux_bytes" "$linux_sha256" || return 1
    "$work/$artifact.elf" >/dev/null
    [[ $? -eq $expected_exit ]] || return 1
    "$packager" windows-x64-console-v1 "$work/$artifact.bin" 0 "$candidate_exe" \
        >/dev/null || return $?
    "$console_publisher" "$candidate_exe" "$work/$artifact.exe" \
        >/dev/null || return $?
    verify "$work/$artifact.exe" "$windows_bytes" "$windows_sha256" || return 1
}

[[ -f $target_plan ]] || {
    echo 'Missing OS x64 code-emission target manifest.' >&2
    exit 1
}
if [[ $development_cache == true && ! -f $cached_project_builder ]]; then
    echo 'The native project-WVB checkpoint builder is missing.' >&2
    exit 1
fi
IFS= read -r target_header <"$target_plan"
[[ $target_header == 'windvale-os-x64-code-emission-development-targets 2' ]] || {
    echo 'Invalid OS x64 code-emission target manifest.' >&2
    exit 1
}
if ! (cd -- "$front_door" && sha256sum --check --strict --quiet SHA256SUMS); then
    echo 'The Linux native-front-door artifact inventory is invalid.' >&2
    exit 1
fi
if ! cp -- "$build_driver" "$work/wvbuild.elf" ||
    ! cp -- "$wvb_publisher" "$work/wvpublish.elf" ||
    ! cp -- "$lowerer" "$work/Wvb-To-Wvo.elf" ||
    ! cp -- "$wvo_publisher" "$work/wvopublish.elf" ||
    ! cp -- "$linker" "$work/Wv-Linker.elf" ||
    ! cp -- "$packager" "$work/Console-Packager.elf" ||
    ! cp -- "$console_publisher" "$work/wvappublish.elf"; then
    echo 'The Linux native OS x64 verification toolchain could not be staged.' >&2
    exit 1
fi
build_driver=$work/wvbuild.elf
wvb_publisher=$work/wvpublish.elf
lowerer=$work/Wvb-To-Wvo.elf
wvo_publisher=$work/wvopublish.elf
linker=$work/Wv-Linker.elf
packager=$work/Console-Packager.elf
console_publisher=$work/wvappublish.elf
if ! verify_tool "$build_driver" d228db89c17cc8124776d6bd39cb061a1414168a22ca075168e44439b1253969 ||
    ! verify_tool "$wvb_publisher" b8efb90f7d7c4eae99de01df6c0a3c24a7396d9b9e717ff69d005282ed3d63af ||
    ! verify_tool "$lowerer" 2ee161ac0a6e885e988e12f9e242005fdb8218776991bfb08ffc6d8417ac1e28 ||
    ! verify_tool "$wvo_publisher" 2889237d7fdb20b1d420c05834f19183d18b02112e3f4eea0ed7ff43414814f2 ||
    ! verify_tool "$linker" 8a220bfd6c7ef684897583e728419ecd6d383c8e8cf40094edbcfb695e3d6d7a ||
    ! verify_tool "$packager" d399c935e906ab42d7572e337226577055396cb6204766106e21790e22ea43af ||
    ! verify_tool "$console_publisher" e9b8771978c9fb06c3a8ecc55c7b9a3ba1acd24faa541dc669920c10ed792925; then
    echo 'The Linux native OS x64 verification toolchain digest is invalid.' >&2
    exit 1
fi
workspace_path=$repository_root/Windvale.wvws
[[ -f $workspace_path ]] || {
    echo 'The native workspace marker is missing.' >&2
    exit 1
}
if [[ -L $repository_root ]] || [[ -n $(find "$repository_root" -type l -print -quit) ]]; then
    echo 'The native workspace must not contain symbolic links.' >&2
    exit 1
fi
if [[ $development_cache == true ]]; then
    cache_target=${development_target:-all}
    node "$cached_project_builder" "$target_plan" "$work" \
        "$build_driver" "$cache_target" || exit $?
fi

declare -A seen_targets=()
declare -A seen_artifacts=()
total_projects=0
selected_count=0
while IFS='|' read -r \
    target project artifact expected_exit \
    wvb_bytes wvb_sha256 wvo_bytes wvo_sha256 bin_bytes bin_sha256 \
    windows_bytes windows_sha256 linux_bytes linux_sha256 \
    input_one input_two input_three extra; do
    total_projects=$((total_projects + 1))
    if [[ ! $target =~ ^[a-z0-9][a-z0-9-]*$ ||
          ! $project =~ ^Projects/Tests/Windvale-Native-Test-Os-X64-.+-Emission\.wvproj$ ||
          ! $artifact =~ ^[A-Za-z][A-Za-z0-9]*$ ||
          ! $expected_exit =~ ^[0-9]+$ ||
          $expected_exit -ne $((49 + total_projects)) ||
          ! $wvb_bytes =~ ^[0-9]+$ || ! $wvb_sha256 =~ ^[0-9a-f]{64}$ ||
          ! $wvo_bytes =~ ^[0-9]+$ || ! $wvo_sha256 =~ ^[0-9a-f]{64}$ ||
          ! $bin_bytes =~ ^[0-9]+$ || ! $bin_sha256 =~ ^[0-9a-f]{64}$ ||
          ! $windows_bytes =~ ^[0-9]+$ || ! $windows_sha256 =~ ^[0-9a-f]{64}$ ||
          ! $linux_bytes =~ ^[0-9]+$ || ! $linux_sha256 =~ ^[0-9a-f]{64}$ ||
          -z $input_one || -z $input_two || -n $extra ||
          -n ${seen_targets[$target]+present} ||
          -n ${seen_artifacts[$artifact]+present} ]]; then
        echo "Invalid OS x64 code-emission target manifest entry: $target" >&2
        exit 1
    fi
    for input_path in "$project" "$input_one" "$input_two"; do
        [[ -f $repository_root/$input_path ]] || {
            echo "Missing OS x64 code-emission input: $input_path" >&2
            exit 1
        }
    done
    if [[ -n $input_three && ! -f $repository_root/$input_three ]]; then
        echo "Missing OS x64 code-emission input: $input_three" >&2
        exit 1
    fi
    seen_targets[$target]=1
    seen_artifacts[$artifact]=1
    if [[ -n $development_target && $development_target != "$target" ]]; then
        continue
    fi
    selected_count=$((selected_count + 1))
    echo "step=$target item=$total_projects/56"
    run_case || exit $?
done < <(tail -n +2 "$target_plan")

if ((total_projects != 56)); then
    echo 'Invalid OS x64 code-emission target count.' >&2
    exit 1
fi
if [[ -n $development_target ]]; then
    if ((selected_count != 1)); then
        echo "Unknown OS x64 code-emission development target: $development_target" >&2
        exit 64
    fi
    echo "native os x64 code emission development status=Passed target=$development_target projects=1 cases=6 cross-host-images=Verified"
elif [[ $development_cache == true ]]; then
    echo 'native os x64 code emission development status=Passed target=all projects=56 cases=336 cross-host-images=Verified source-owned-bytes=33826 relocation-fields=569'
else
    echo 'native os x64 code emission status=Passed projects=56 cases=336 local-results=50/51/52/53/54/55/56/57/58/59/60/61/62/63/64/65/66/67/68/69/70/71/72/73/74/75/76/77/78/79/80/81/82/83/84/85/86/87/88/89/90/91/92/93/94/95/96/97/98/99/100/101/102/103/104/105 cross-host-images=Verified source-owned-bytes=33826 relocation-fields=569'
fi
