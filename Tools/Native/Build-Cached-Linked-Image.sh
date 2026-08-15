#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 5 || ! $1 =~ ^(0|[1-9][0-9]*)$ ||
      ! $2 =~ ^[A-Za-z_][A-Za-z0-9_]*$ || $3 != *.wvo ||
      $4 != *.bin || $5 != *.map ]]; then
    echo 'Usage: ./Tools/Native/Build-Cached-Linked-Image.sh <base-address> <entry> <input.wvo> <output.bin> <output.map>' >&2
    exit 64
fi

base_address=$1
entry=$2
input_argument=$3
output_image_argument=$4
output_map_argument=$5
script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
input_directory=$(CDPATH= cd -- "$(dirname -- "$input_argument")" && pwd -P) || exit 1
input="$input_directory/$(basename -- "$input_argument")"
output_image_directory=$(CDPATH= cd -- "$(dirname -- "$output_image_argument")" && pwd -P) || exit 1
output_image="$output_image_directory/$(basename -- "$output_image_argument")"
output_map_directory=$(CDPATH= cd -- "$(dirname -- "$output_map_argument")" && pwd -P) || exit 1
output_map="$output_map_directory/$(basename -- "$output_map_argument")"
front_door="$script_directory/Link-Wvo.sh"
linker="$repository_root/Artifacts/Native-Wv-Linker-Candidate/Wv-Linker.elf"
key_tool="$script_directory/Get-Native-Linked-Image-Cache-Key.mjs"

[[ -f $input && -f $front_door && -f $linker && -f $key_tool ]] || exit 1
checkpoint_key=$(node "$key_tool" linked-image-v1 "$base_address" "$entry" \
    "$input" "$front_door" "$linker") || exit $?
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
checkpoint_family="$checkpoint_root/linked-image-v1/linux-x64"
mkdir -p -- "$checkpoint_family" || exit 1
[[ ! -L $checkpoint_family ]] || exit 1

checkpoint_directory="$checkpoint_family/$checkpoint_key"
checkpoint_manifest="$checkpoint_directory/Checkpoint.txt"
checkpoint_image="$checkpoint_directory/Product.bin"
checkpoint_map="$checkpoint_directory/Product.map"
checkpoint_status=Hit

measure_file() {
    local candidate=$1
    [[ -f $candidate && ! -L $candidate ]] || return 1
    measured_bytes=$(wc -c < "$candidate") || return 1
    [[ $measured_bytes -gt 0 && $measured_bytes -le 67108864 ]] || return 1
    measured_sha256=$(sha256sum -- "$candidate" | awk '{ print $1 }') || return 1
    [[ $measured_sha256 =~ ^[0-9a-f]{64}$ ]] || return 1
}

read_entry() {
    local candidate=$1
    measured_entry=$(sed -n \
        "s/^entry name=$entry address=\(0\|[1-9][0-9]*\)$/\1/p" "$candidate")
    [[ $measured_entry =~ ^(0|[1-9][0-9]*)$ ]]
}

validate_checkpoint() {
    [[ -d $checkpoint_directory && ! -L $checkpoint_directory &&
        -f $checkpoint_manifest && ! -L $checkpoint_manifest ]] || return 1
    [[ -z $(find "$checkpoint_directory" -type l -print -quit) ]] || return 1
    [[ $(wc -c < "$checkpoint_manifest") -le 1024 ]] || return 1
    read_entry "$checkpoint_map" || return 1
    local entry_offset=$measured_entry
    measure_file "$checkpoint_image" || return 1
    local image_bytes=$measured_bytes image_sha256=$measured_sha256
    measure_file "$checkpoint_map" || return 1
    local map_bytes=$measured_bytes map_sha256=$measured_sha256
    local expected_manifest="$output_map_directory/.windvale-linked-image-cache-expected-$$-$RANDOM.txt"
    printf '%s\n' \
        'windvale-native-linked-image-checkpoint 1' \
        "key $checkpoint_key" \
        "entry-offset $entry_offset" \
        "image-bytes $image_bytes" \
        "image-sha256 $image_sha256" \
        "map-bytes $map_bytes" \
        "map-sha256 $map_sha256" \
        > "$expected_manifest" || return 1
    cmp --silent -- "$expected_manifest" "$checkpoint_manifest"
    local comparison=$?
    rm -f -- "$expected_manifest"
    [[ $comparison -eq 0 ]]
}

if [[ ! -e $checkpoint_directory ]]; then
    checkpoint_temporary=$(mktemp -d "$checkpoint_family/.new-$checkpoint_key.XXXXXXXX") || exit 1
    candidate_image="$checkpoint_temporary/Product.bin"
    candidate_map="$checkpoint_temporary/Product.map"
    "$front_door" "$base_address" "$entry" "$candidate_image" "$input" \
        > "$candidate_map" || exit $?
    read_entry "$candidate_map" || exit 1
    candidate_entry=$measured_entry
    measure_file "$candidate_image" || exit 1
    candidate_image_bytes=$measured_bytes
    candidate_image_sha256=$measured_sha256
    measure_file "$candidate_map" || exit 1
    candidate_map_bytes=$measured_bytes
    candidate_map_sha256=$measured_sha256
    printf '%s\n' \
        'windvale-native-linked-image-checkpoint 1' \
        "key $checkpoint_key" \
        "entry-offset $candidate_entry" \
        "image-bytes $candidate_image_bytes" \
        "image-sha256 $candidate_image_sha256" \
        "map-bytes $candidate_map_bytes" \
        "map-sha256 $candidate_map_sha256" \
        > "$checkpoint_temporary/Checkpoint.txt" || exit 1
    mv -- "$checkpoint_temporary" "$checkpoint_directory" || exit 1
    checkpoint_status=Created
fi

validate_checkpoint || exit 1
read_entry "$checkpoint_map" || exit 1
checkpoint_entry=$measured_entry
cp -- "$checkpoint_image" "$output_image" || exit 1
cp -- "$checkpoint_map" "$output_map" || exit 1
cmp --silent -- "$checkpoint_image" "$output_image" || exit 1
cmp --silent -- "$checkpoint_map" "$output_map" || exit 1
echo "native linked image cache status=$checkpoint_status key=$checkpoint_key entry=$checkpoint_entry"
