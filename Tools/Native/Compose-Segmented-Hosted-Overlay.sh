#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 5 || $2 != *.wvli || $3 != *.wvo || $4 != *.wvo ]]; then
    echo 'Usage: ./Tools/Native/Compose-Segmented-Hosted-Overlay.sh <application-chunk-prefix> <application.wvli> <common-provider.wvo> <platform-provider.wvo> <output-chunk-prefix>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
application_prefix=$1
manifest=$2
common_provider=$3
platform_provider=$4
output_prefix=$5
maximum_chunk_bytes=4194304
maximum_image_bytes=33554432

read_u32() {
    local path=$1 offset=$2 values
    read -r -a values <<< "$(od -An -v -tx1 -j "$offset" -N 4 -- "$path")" || return 1
    [[ ${#values[@]} -eq 4 ]] || return 1
    printf '%u\n' "$((16#${values[3]}${values[2]}${values[1]}${values[0]}))"
}

[[ -f $manifest && ! -L $manifest ]] || { echo 'The segmented application manifest is not one regular file.' >&2; exit 1; }
[[ $(od -An -v -tx1 -N 8 -- "$manifest" | tr -d ' \n') == 57564c4901000000 ]] || {
    echo 'The segmented application manifest identity is invalid.' >&2
    exit 1
}
manifest_bytes=$(wc -c < "$manifest") || exit 1
declared_manifest_bytes=$(read_u32 "$manifest" 8) || exit 1
application_bytes=$(read_u32 "$manifest" 12) || exit 1
application_entry=$(read_u32 "$manifest" 16) || exit 1
application_chunks=$(read_u32 "$manifest" 20) || exit 1
chunk_limit=$(read_u32 "$manifest" 24) || exit 1
if ((declared_manifest_bytes != manifest_bytes || application_bytes == 0 ||
    application_bytes > maximum_image_bytes || application_entry >= application_bytes ||
    application_chunks == 0 || application_chunks > 8 || chunk_limit != maximum_chunk_bytes ||
    manifest_bytes != 28 + application_chunks * 12)); then
    echo 'The segmented application manifest bounds are invalid.' >&2
    exit 1
fi

position=0
for ((index=0; index<application_chunks; index++)); do
    entry=$((28 + index * 12))
    chunk_index=$(read_u32 "$manifest" "$entry") || exit 1
    chunk_position=$(read_u32 "$manifest" "$((entry + 4))") || exit 1
    chunk_bytes=$(read_u32 "$manifest" "$((entry + 8))") || exit 1
    chunk_path="$application_prefix.chunk-$index"
    [[ -f $chunk_path && ! -L $chunk_path ]] || { echo "The segmented application chunk $index is not one regular file." >&2; exit 1; }
    actual_chunk_bytes=$(wc -c < "$chunk_path") || exit 1
    if ((chunk_index != index || chunk_position != position || chunk_bytes == 0 ||
        chunk_bytes > maximum_chunk_bytes || actual_chunk_bytes != chunk_bytes ||
        (index + 1 < application_chunks && chunk_bytes != maximum_chunk_bytes))); then
        echo "The segmented application chunk $index is invalid." >&2
        exit 1
    fi
    position=$((position + chunk_bytes))
done
((position == application_bytes)) || { echo 'The segmented application chunks do not cover the declared image.' >&2; exit 1; }
for ((index=0; index<8; index++)); do
    [[ ! -e $output_prefix.chunk-$index ]] || { echo "The overlay output chunk already exists: $output_prefix.chunk-$index" >&2; exit 1; }
done

output_directory=$(CDPATH= cd -- "$(dirname -- "$output_prefix")" && pwd -P) || exit 64
temporary_directory=$(mktemp -d "$output_directory/.windvale-segmented-overlay.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$output_directory"/.windvale-segmented-overlay.*)
            rm -f -- "$temporary_directory"/*
            rmdir -- "$temporary_directory"
            ;;
        *) echo "Refusing to remove unexpected temporary path: $temporary_directory" >&2; return 1 ;;
    esac
}
trap cleanup EXIT

"$script_directory/Assemble-Wva.sh" \
    "$repository_root/Runtime/Native/X64-Segmented-Hosted-Main-Trampoline.wva" \
    "$temporary_directory/Main-Trampoline.wvo" >"$temporary_directory/Assemble.txt" || exit $?
provider_start=$((((application_bytes + 15) / 16) * 16))
"$script_directory/Link-Wvo.sh" "$provider_start" Storage_host_entry \
    "$temporary_directory/Provider.bin" "$common_provider" "$platform_provider" \
    "$temporary_directory/Main-Trampoline.wvo" >"$temporary_directory/Provider.map" || exit $?

mapfile -t entry_matches < <(sed -n 's/^entry name=Storage_host_entry address=\([0-9][0-9]*\)$/\1/p' "$temporary_directory/Provider.map")
mapfile -t main_matches < <(sed -n 's/^symbol .* binding=export kind=function name=Main address=\([0-9][0-9]*\) size=5$/\1/p' "$temporary_directory/Provider.map")
[[ ${#entry_matches[@]} -eq 1 && ${#main_matches[@]} -eq 1 ]] || {
    echo 'The segmented hosted provider map does not contain one exact entry and trampoline.' >&2
    exit 1
}
provider_entry=${entry_matches[0]}
trampoline_address=${main_matches[0]}
provider_bytes=$(wc -c < "$temporary_directory/Provider.bin") || exit 1
if ((provider_bytes == 0 || provider_bytes > maximum_chunk_bytes ||
    provider_entry < provider_start || provider_entry >= provider_start + provider_bytes ||
    trampoline_address < provider_start || trampoline_address + 5 > provider_start + provider_bytes)); then
    echo 'The segmented hosted provider layout is invalid.' >&2
    exit 1
fi
trampoline_offset=$((trampoline_address - provider_start))
[[ $(od -An -v -tx1 -j "$trampoline_offset" -N 5 -- "$temporary_directory/Provider.bin" | tr -d ' \n') == e9fbffffff ]] || {
    echo 'The segmented hosted trampoline placeholder is invalid.' >&2
    exit 1
}
displacement=$((application_entry - trampoline_address - 5))
((displacement >= -2147483648 && displacement <= 2147483647)) || {
    echo 'The segmented hosted trampoline target is out of relative range.' >&2
    exit 1
}
bits=$displacement
((bits < 0)) && bits=$((bits + 4294967296))
for ((byte=0; byte<4; byte++)); do
    value=$(((bits >> (byte * 8)) & 255))
    printf "\\$(printf '%03o' "$value")" | dd of="$temporary_directory/Provider.bin" bs=1 seek=$((trampoline_offset + 1 + byte)) count=1 conv=notrunc status=none || exit 1
done

padding_bytes=$((provider_start - application_bytes))
last_index=$((application_chunks - 1))
last_application_bytes=$(read_u32 "$manifest" "$((28 + last_index * 12 + 8))") || exit 1
last_overlay_bytes=$((last_application_bytes + padding_bytes + provider_bytes))
if ((last_overlay_bytes > maximum_chunk_bytes || provider_start + provider_bytes > maximum_image_bytes)); then
    echo 'The provider overlay does not fit in the bounded image fragments.' >&2
    exit 1
fi
for ((index=0; index<last_index; index++)); do
    cp -- "$application_prefix.chunk-$index" "$temporary_directory/Result.chunk-$index" || exit 1
done
cp -- "$application_prefix.chunk-$last_index" "$temporary_directory/Result.chunk-$last_index" || exit 1
if ((padding_bytes > 0)); then
    dd if=/dev/zero bs=1 count="$padding_bytes" status=none >>"$temporary_directory/Result.chunk-$last_index" || exit 1
fi
cat -- "$temporary_directory/Provider.bin" >>"$temporary_directory/Result.chunk-$last_index" || exit 1
[[ $(wc -c < "$temporary_directory/Result.chunk-$last_index") -eq $last_overlay_bytes ]] || {
    echo 'The segmented hosted overlay final fragment length is invalid.' >&2
    exit 1
}
for ((index=0; index<application_chunks; index++)); do
    mv -- "$temporary_directory/Result.chunk-$index" "$output_prefix.chunk-$index" || exit 1
done
printf 'segmented hosted overlay status=Valid application-bytes=%u provider-bytes=%u image-bytes=%u fragments=%u application-entry=%u provider-entry=%u trampoline-address=%u padding-bytes=%u\n' \
    "$application_bytes" "$provider_bytes" "$((provider_start + provider_bytes))" \
    "$application_chunks" "$application_entry" "$provider_entry" \
    "$trampoline_address" "$padding_bytes"
