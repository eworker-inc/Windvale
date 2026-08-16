#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 2 || $1 != *.wvproj || $2 != *.wvb ]]; then
    echo 'Usage: ./Tools/Native/Build-Cached-Project-Wvb.sh <project.wvproj> <output.wvb>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
project_directory=$(CDPATH= cd -- "$(dirname -- "$1")" && pwd -P) || exit 1
project="$project_directory/$(basename -- "$1")"
output_directory=$(CDPATH= cd -- "$(dirname -- "$2")" && pwd -P) || exit 1
output_wvb="$output_directory/$(basename -- "$2")"
key_tool="$script_directory/Get-Native-Project-Cache-Key.mjs"
front_door="$repository_root/Artifacts/Native-Front-Door"
inventory="$front_door/SHA256SUMS"
build_driver="$front_door/linux-x64/wvbuild.elf"
workspace="$repository_root/Windvale.wvws"

for candidate in "$project" "$key_tool" \
    "$inventory" "$build_driver" "$workspace"; do
    [[ -f $candidate ]] || exit 1
done
checkpoint_key=$(node "$key_tool" project-wvb-v2 "$project" \
    "$inventory" "$build_driver") || exit $?
[[ $checkpoint_key =~ ^[0-9a-f]{64}$ ]] || exit 1

if [[ -n ${WINDVALE_NATIVE_CACHE_ROOT:-} ]]; then
    checkpoint_root_input=$WINDVALE_NATIVE_CACHE_ROOT
else
    checkpoint_root_input="${XDG_CACHE_HOME:-$HOME/.cache}/windvale/native-tool-cache"
fi
[[ ! -L $checkpoint_root_input ]] || exit 1
mkdir -p -- "$checkpoint_root_input" || exit 1
checkpoint_root=$(CDPATH= cd -- "$checkpoint_root_input" && pwd -P) || exit 1
[[ -z $(find "$checkpoint_root" -type l -print -quit) ]] || exit 1
checkpoint_family="$checkpoint_root/project-wvb-v2/linux-x64"
mkdir -p -- "$checkpoint_family" || exit 1
[[ ! -L $checkpoint_family ]] || exit 1

checkpoint_directory="$checkpoint_family/$checkpoint_key"
checkpoint_manifest="$checkpoint_directory/Checkpoint.txt"
checkpoint_wvb="$checkpoint_directory/Product.wvb"
checkpoint_status=Hit

measure_file() {
    local candidate=$1
    [[ -f $candidate && ! -L $candidate ]] || return 1
    measured_bytes=$(wc -c < "$candidate") || return 1
    [[ $measured_bytes -gt 0 && $measured_bytes -le 67108864 ]] || return 1
    measured_sha256=$(sha256sum -- "$candidate" | awk '{ print $1 }') || return 1
    [[ $measured_sha256 =~ ^[0-9a-f]{64}$ ]] || return 1
}

validate_checkpoint() {
    [[ -d $checkpoint_directory && ! -L $checkpoint_directory &&
        -f $checkpoint_manifest && ! -L $checkpoint_manifest ]] || return 1
    [[ -z $(find "$checkpoint_directory" -type l -print -quit) ]] || return 1
    [[ $(wc -c < "$checkpoint_manifest") -le 1024 ]] || return 1
    measure_file "$checkpoint_wvb" || return 1
    local wvb_bytes=$measured_bytes wvb_sha256=$measured_sha256
    local expected_manifest="$output_directory/.windvale-project-wvb-cache-expected-$$-$RANDOM.txt"
    printf '%s\n' \
        'windvale-native-project-wvb-checkpoint 1' \
        "key $checkpoint_key" \
        "wvb-bytes $wvb_bytes" \
        "wvb-sha256 $wvb_sha256" \
        > "$expected_manifest" || return 1
    cmp --silent -- "$expected_manifest" "$checkpoint_manifest"
    local comparison=$?
    rm -f -- "$expected_manifest"
    [[ $comparison -eq 0 ]]
}

if [[ ! -e $checkpoint_directory ]]; then
    checkpoint_temporary=$(mktemp -d "$checkpoint_family/.new-$checkpoint_key.XXXXXXXX") || exit 1
    candidate_wvb="$checkpoint_temporary/Product.wvb"
    build_log="$checkpoint_temporary/Build.log"
    if ! "$build_driver" --workspace "$workspace" --project "$project" \
        "$candidate_wvb" >"$build_log" 2>&1; then
        echo 'The project-WVB cache build failed.' >&2
        cat -- "$build_log" >&2
        exit 1
    fi
    rm -f -- "$build_log"
    measure_file "$candidate_wvb" || exit 1
    candidate_bytes=$measured_bytes
    candidate_sha256=$measured_sha256
    printf '%s\n' \
        'windvale-native-project-wvb-checkpoint 1' \
        "key $checkpoint_key" \
        "wvb-bytes $candidate_bytes" \
        "wvb-sha256 $candidate_sha256" \
        > "$checkpoint_temporary/Checkpoint.txt" || exit 1
    mv -- "$checkpoint_temporary" "$checkpoint_directory" || exit 1
    checkpoint_status=Created
fi

validate_checkpoint || exit 1
cp -- "$checkpoint_wvb" "$output_wvb" || exit 1
cmp --silent -- "$checkpoint_wvb" "$output_wvb" || exit 1
echo "native project wvb cache status=$checkpoint_status key=$checkpoint_key"
