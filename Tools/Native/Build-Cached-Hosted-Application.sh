#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 7 || ! $1 =~ ^[1-8]$ || $2 != *.wvb ||
      ! $4 =~ ^([1-9]|1[0-6])$ || ! $5 =~ ^(0|[1-9][0-9]*)$ ]]; then
    echo 'Usage: ./Tools/Native/Build-Cached-Hosted-Application.sh <profile-1-through-8> <input.wvb> <chunk-prefix> <fragment-count-1-through-16> <entry> <output.elf|output.exe> <linux|windows>' >&2
    exit 64
fi
case "$7:$6" in
    linux:*.elf) product_leaf=Product.elf ;;
    windows:*.exe) product_leaf=Product.exe ;;
    *)
        echo 'The cached hosted-application target and output extension do not agree.' >&2
        exit 64
        ;;
esac

profile=$1
input_argument=$2
chunk_prefix_argument=$3
fragment_count=$4
native_entry=$5
output_argument=$6
target=$7
script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
input_directory=$(CDPATH= cd -- "$(dirname -- "$input_argument")" && pwd -P) || exit 1
input="$input_directory/$(basename -- "$input_argument")"
chunk_prefix_directory=$(CDPATH= cd -- "$(dirname -- "$chunk_prefix_argument")" && pwd -P) || exit 1
chunk_prefix="$chunk_prefix_directory/$(basename -- "$chunk_prefix_argument")"
output_directory=$(CDPATH= cd -- "$(dirname -- "$output_argument")" && pwd -P) || exit 1
output="$output_directory/$(basename -- "$output_argument")"
packager="$script_directory/Package-Hosted-Wvb.sh"
key_tool="$script_directory/Get-Native-Hosted-Application-Cache-Key.mjs"

[[ -f $input && -f $packager && -f $key_tool ]] || exit 1
checkpoint_key=$(node "$key_tool" hosted-application-v1 "$target" "$profile" \
    "$input" "$chunk_prefix" "$fragment_count" "$native_entry" "$packager") || exit $?
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
checkpoint_family="$checkpoint_root/hosted-application-v1/linux-x64"
mkdir -p -- "$checkpoint_family" || exit 1
[[ ! -L $checkpoint_family ]] || exit 1

checkpoint_directory="$checkpoint_family/$checkpoint_key"
checkpoint_manifest="$checkpoint_directory/Checkpoint.txt"
checkpoint_product="$checkpoint_directory/$product_leaf"
checkpoint_status=Hit
checkpoint_temporary=

remove_checkpoint_temporary() {
    [[ -n $checkpoint_temporary ]] || return 0
    local temporary_parent temporary_leaf
    temporary_parent=$(CDPATH= cd -- "$(dirname -- "$checkpoint_temporary")" && pwd -P) || return 1
    temporary_leaf=$(basename -- "$checkpoint_temporary")
    [[ $temporary_parent == "$checkpoint_family" &&
       $temporary_leaf == ".new-$checkpoint_key."???????? ]] || return 1
    rm -f -- \
        "$checkpoint_temporary/Package.log" \
        "$checkpoint_temporary/$product_leaf" \
        "$checkpoint_temporary/Checkpoint.txt"
    rmdir -- "$checkpoint_temporary" 2>/dev/null || true
    checkpoint_temporary=
}
trap remove_checkpoint_temporary EXIT

measure_file() {
    local candidate=$1
    [[ -f $candidate && ! -L $candidate && -x $candidate ]] || return 1
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
    measure_file "$checkpoint_product" || return 1
    local application_bytes=$measured_bytes application_sha256=$measured_sha256
    local expected_manifest="$output_directory/.windvale-hosted-application-cache-expected-$$-$RANDOM.txt"
    printf '%s\n' \
        'windvale-native-hosted-application-checkpoint 1' \
        "key $checkpoint_key" \
        "target $target" \
        "application-bytes $application_bytes" \
        "application-sha256 $application_sha256" \
        > "$expected_manifest" || return 1
    cmp --silent -- "$expected_manifest" "$checkpoint_manifest"
    local comparison=$?
    rm -f -- "$expected_manifest"
    [[ $comparison -eq 0 ]]
}

if [[ ! -e $checkpoint_directory ]]; then
    checkpoint_temporary=$(mktemp -d "$checkpoint_family/.new-$checkpoint_key.XXXXXXXX") || exit 1
    candidate_product="$checkpoint_temporary/$product_leaf"
    package_log="$checkpoint_temporary/Package.log"
    if ! "$packager" image "$profile" "$input" "$chunk_prefix" \
        "$fragment_count" "$native_entry" "$candidate_product" "$target" \
        >"$package_log" 2>&1; then
        echo 'The hosted-application cache packager failed.' >&2
        cat -- "$package_log" >&2
        exit 1
    fi
    rm -f -- "$package_log"
    measure_file "$candidate_product" || exit 1
    candidate_bytes=$measured_bytes
    candidate_sha256=$measured_sha256
    printf '%s\n' \
        'windvale-native-hosted-application-checkpoint 1' \
        "key $checkpoint_key" \
        "target $target" \
        "application-bytes $candidate_bytes" \
        "application-sha256 $candidate_sha256" \
        > "$checkpoint_temporary/Checkpoint.txt" || exit 1
    mv -T -- "$checkpoint_temporary" "$checkpoint_directory" || exit 1
    checkpoint_temporary=
    checkpoint_status=Created
fi

validate_checkpoint || exit 1
cp -- "$checkpoint_product" "$output" || exit 1
cmp --silent -- "$checkpoint_product" "$output" || exit 1
[[ -x $output ]] || exit 1
echo "native hosted application cache status=$checkpoint_status key=$checkpoint_key target=$target"
