#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 5 || $1 != *.wvproj || $4 != *.wvb || $5 != *.wvo ]]; then
    echo 'Usage: ./Tools/Native/Build-Cached-Project-Object.sh <project.wvproj> <build-driver.elf> <lowerer.elf> <output.wvb> <output.wvo>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
project_directory=$(CDPATH= cd -- "$(dirname -- "$1")" && pwd -P) || exit 1
project="$project_directory/$(basename -- "$1")"
build_driver_directory=$(CDPATH= cd -- "$(dirname -- "$2")" && pwd -P) || exit 1
build_driver="$build_driver_directory/$(basename -- "$2")"
lowerer_directory=$(CDPATH= cd -- "$(dirname -- "$3")" && pwd -P) || exit 1
lowerer="$lowerer_directory/$(basename -- "$3")"
output_wvb_directory=$(CDPATH= cd -- "$(dirname -- "$4")" && pwd -P) || exit 1
output_wvb="$output_wvb_directory/$(basename -- "$4")"
output_wvo_directory=$(CDPATH= cd -- "$(dirname -- "$5")" && pwd -P) || exit 1
output_wvo="$output_wvo_directory/$(basename -- "$5")"
workspace="$repository_root/Windvale.wvws"
key_tool="$script_directory/Get-Native-Project-Cache-Key.mjs"

[[ -f $project && -f $build_driver && -f $lowerer && -f $workspace && -f $key_tool ]] || exit 1
checkpoint_key=$(node "$key_tool" database-project-object-v1 \
    "$project" "$build_driver" "$lowerer") || exit $?
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
checkpoint_family="$checkpoint_root/project-object-v1/linux-x64"
mkdir -p -- "$checkpoint_family" || exit 1
[[ ! -L $checkpoint_family ]] || exit 1

checkpoint_directory="$checkpoint_family/$checkpoint_key"
checkpoint_manifest="$checkpoint_directory/Checkpoint.txt"
checkpoint_wvb="$checkpoint_directory/Product.wvb"
checkpoint_wvo="$checkpoint_directory/Product.wvo"
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
    measure_file "$checkpoint_wvo" || return 1
    local wvo_bytes=$measured_bytes wvo_sha256=$measured_sha256
    local expected_manifest="$output_wvo_directory/.windvale-project-cache-expected-$$-$RANDOM.txt"
    printf '%s\n' \
        'windvale-native-project-object-checkpoint 1' \
        "key $checkpoint_key" \
        "wvb-bytes $wvb_bytes" \
        "wvb-sha256 $wvb_sha256" \
        "wvo-bytes $wvo_bytes" \
        "wvo-sha256 $wvo_sha256" \
        > "$expected_manifest" || return 1
    cmp --silent -- "$expected_manifest" "$checkpoint_manifest"
    local comparison=$?
    rm -f -- "$expected_manifest"
    [[ $comparison -eq 0 ]]
}

if [[ ! -e $checkpoint_directory ]]; then
    checkpoint_temporary=$(mktemp -d "$checkpoint_family/.new-$checkpoint_key.XXXXXXXX") || exit 1
    candidate_wvb="$checkpoint_temporary/Product.wvb"
    candidate_wvo="$checkpoint_temporary/Product.wvo"
    "$build_driver" --workspace "$workspace" --project "$project" "$candidate_wvb" >/dev/null || exit $?
    "$lowerer" "$candidate_wvb" "$candidate_wvo" >/dev/null || exit $?
    "$script_directory/Check-Wvo.sh" "$candidate_wvo" >/dev/null || exit $?
    measure_file "$candidate_wvb" || exit 1
    candidate_wvb_bytes=$measured_bytes
    candidate_wvb_sha256=$measured_sha256
    measure_file "$candidate_wvo" || exit 1
    candidate_wvo_bytes=$measured_bytes
    candidate_wvo_sha256=$measured_sha256
    printf '%s\n' \
        'windvale-native-project-object-checkpoint 1' \
        "key $checkpoint_key" \
        "wvb-bytes $candidate_wvb_bytes" \
        "wvb-sha256 $candidate_wvb_sha256" \
        "wvo-bytes $candidate_wvo_bytes" \
        "wvo-sha256 $candidate_wvo_sha256" \
        > "$checkpoint_temporary/Checkpoint.txt" || exit 1
    mv -- "$checkpoint_temporary" "$checkpoint_directory" || exit 1
    checkpoint_status=Created
fi

validate_checkpoint || exit 1
cp -- "$checkpoint_wvb" "$output_wvb" || exit 1
cp -- "$checkpoint_wvo" "$output_wvo" || exit 1
cmp --silent -- "$checkpoint_wvb" "$output_wvb" || exit 1
cmp --silent -- "$checkpoint_wvo" "$output_wvo" || exit 1
"$script_directory/Check-Wvo.sh" "$output_wvo" >/dev/null || exit $?
echo "native project object cache status=$checkpoint_status key=$checkpoint_key"
