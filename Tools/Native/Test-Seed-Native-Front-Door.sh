#!/usr/bin/env bash
set -uo pipefail

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
artifact_root="$repository_root/Artifacts/Native-Front-Door"
manifest="$artifact_root/Manifest.json"
inventory="$artifact_root/SHA256SUMS"
verifier="$artifact_root/linux-x64/wvverify.elf"
manifest_digest=b5a27bbd6d143c73907f2908bd1022f30de474c5d922473086213dad9ab5319a
inventory_digest=cce8e76c7e16f9697241356490f18a87116b996a16f71284b161d6f0897996ad

verify_file() {
    local path=$1
    local bytes=$2
    local digest=$3
    local label=$4
    if [[ ! -f $path ]]; then
        echo "The $label is missing: $path" >&2
        return 1
    fi
    if [[ $bytes != 0 && $(wc -c < "$path") -ne $bytes ]]; then
        echo "The $label size differs: $path" >&2
        return 1
    fi
    local directory
    directory=$(CDPATH= cd -- "$(dirname -- "$path")" && pwd -P) || return 1
    if ! (cd -- "$directory" && printf '%s  %s\n' \
        "$digest" "$(basename -- "$path")" | sha256sum --check --strict --quiet); then
        echo "The $label digest differs: $path" >&2
        return 1
    fi
}

verify_file "$manifest" 6170 "$manifest_digest" 'front-door manifest' || exit 1
verify_file "$inventory" 1605 "$inventory_digest" 'front-door checksum inventory' || exit 1
if ! (cd -- "$artifact_root" && sha256sum --check --strict --quiet SHA256SUMS); then
    echo 'The native front-door artifact inventory differs.' >&2
    exit 1
fi

artifact_count=0
module_count=0
while read -r digest relative_path; do
    [[ -n $digest && -n $relative_path ]] || continue
    artifact_count=$((artifact_count + 1))
    if [[ $relative_path == *.wvb ]]; then
        module_count=$((module_count + 1))
        verify_error=$(mktemp "${TMPDIR:-/tmp}/windvale-front-door-admission.XXXXXXXX") || exit 1
        if ! verify_output=$("$verifier" "$artifact_root/$relative_path" 2> "$verify_error"); then
            echo "The current-host verifier rejected the native front-door module: $relative_path" >&2
            cat -- "$verify_error" >&2
            rm -f -- "$verify_error"
            exit 1
        fi
        if [[ -s $verify_error ]]; then
            echo "The current-host verifier diagnosed a native front-door module: $relative_path" >&2
            cat -- "$verify_error" >&2
            rm -f -- "$verify_error"
            exit 1
        fi
        rm -f -- "$verify_error"
        if [[ $verify_output != 'wvb status=Valid profile=compiler-aligned' ]]; then
            echo "The native front-door admission report differs: $relative_path" >&2
            exit 1
        fi
    fi
done < "$inventory"

if ((artifact_count != 18)); then
    echo "The native front-door artifact count differs: $artifact_count" >&2
    exit 1
fi
if ((module_count != 6)); then
    echo "The native front-door WVB admission count differs: $module_count" >&2
    exit 1
fi

echo 'Tests: 1, Passed: 1, Failed: 0'
