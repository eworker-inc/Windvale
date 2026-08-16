#!/usr/bin/env bash
set -uo pipefail

development_target=''
case $# in
    0) ;;
    2)
        [[ $1 == '--development-target' && $2 =~ ^[a-z0-9][a-z0-9-]*$ ]] || exit 64
        development_target=$2
        ;;
    *) exit 64 ;;
esac

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
target_plan=$repository_root/Tests/Native/Os-X64-Code-Emission-Development-Targets.txt
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

run_case() {
    "$script_directory/Build-Wvb.sh" \
        "$repository_root/$project" "$work/$artifact.wvb" >/dev/null || return $?
    verify "$work/$artifact.wvb" "$wvb_bytes" "$wvb_sha256" || return 1
    "$script_directory/Lower-Wvb-To-Wvo.sh" \
        "$work/$artifact.wvb" "$work/$artifact.wvo" >/dev/null || return $?
    verify "$work/$artifact.wvo" "$wvo_bytes" "$wvo_sha256" || return 1
    "$script_directory/Link-Wvo.sh" \
        0 Main "$work/$artifact.bin" "$work/$artifact.wvo" >/dev/null || return $?
    verify "$work/$artifact.bin" "$bin_bytes" "$bin_sha256" || return 1
    "$script_directory/Package-Console.sh" \
        linux-x64-console-v1 "$work/$artifact.bin" 0 "$work/$artifact.elf" \
        >/dev/null || return $?
    verify "$work/$artifact.elf" "$linux_bytes" "$linux_sha256" || return 1
    "$work/$artifact.elf" >/dev/null
    [[ $? -eq $expected_exit ]] || return 1
    "$script_directory/Package-Console.sh" \
        windows-x64-console-v1 "$work/$artifact.bin" 0 "$work/$artifact.exe" \
        >/dev/null || return $?
    verify "$work/$artifact.exe" "$windows_bytes" "$windows_sha256" || return 1
}

[[ -f $target_plan ]] || {
    echo 'Missing OS x64 code-emission target manifest.' >&2
    exit 1
}
IFS= read -r target_header <"$target_plan"
[[ $target_header == 'windvale-os-x64-code-emission-development-targets 2' ]] || {
    echo 'Invalid OS x64 code-emission target manifest.' >&2
    exit 1
}

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
else
    echo 'native os x64 code emission status=Passed projects=56 cases=336 local-results=50/51/52/53/54/55/56/57/58/59/60/61/62/63/64/65/66/67/68/69/70/71/72/73/74/75/76/77/78/79/80/81/82/83/84/85/86/87/88/89/90/91/92/93/94/95/96/97/98/99/100/101/102/103/104/105 cross-host-images=Verified source-owned-bytes=33826 relocation-fields=569'
fi
