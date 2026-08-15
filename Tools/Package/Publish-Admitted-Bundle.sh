#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 3 || ! $2 =~ ^[0-9a-f]{64}$ ]]; then
    echo 'Usage: ./Tools/Package/Publish-Admitted-Bundle.sh <bundle.wvbundle> <expected-sha256> <store-root>' >&2
    exit 64
fi

bundle_argument=$1
expected_sha256=$2
store_argument=$3
script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
bundle_directory=$(CDPATH= cd -- "$(dirname -- "$bundle_argument")" && pwd -P) || exit 1
bundle="$bundle_directory/$(basename -- "$bundle_argument")"
[[ -f $bundle && ! -L $bundle ]] || exit 1
bundle_bytes=$(wc -c < "$bundle") || exit 1
[[ $bundle_bytes -ge 128 && $bundle_bytes -le 4194304 ]] || exit 1
bundle_sha256=$(sha256sum -- "$bundle" | awk '{ print $1 }') || exit 1
[[ $bundle_sha256 == "$expected_sha256" ]] || exit 1
echo "package store step=admission-recheck bundle=$bundle_sha256 bytes=$bundle_bytes"

mkdir -p -- "$store_argument" || exit 1
[[ ! -L $store_argument ]] || exit 1
store_root=$(CDPATH= cd -- "$store_argument" && pwd -P) || exit 1
[[ -z $(find "$store_root" -type l -print -quit) ]] || exit 1
objects_root="$store_root/objects/sha256"
bundles_root="$store_root/bundles/sha256"
mkdir -p -- "$objects_root" "$bundles_root" || exit 1

work=$(mktemp -d "${TMPDIR:-/tmp}/windvale-admitted-bundle.XXXXXXXX") || exit 1
cleanup() {
    case "$work" in
        "${TMPDIR:-/tmp}"/windvale-admitted-bundle.*) rm -rf -- "$work" ;;
        *) echo "Refusing to remove unexpected temporary path: $work" >&2; return 1 ;;
    esac
}
trap cleanup EXIT

node "$repository_root/Tools/Package/Publish-Admitted-Bundle.mjs" \
    extract "$bundle" "$expected_sha256" "$work" > "$work/Inventory.txt" || exit $?
blob_count=$(grep -c '^blob ' "$work/Inventory.txt") || exit 1
created=0
existing=0
ordinal=0

publish_immutable() {
    local source=$1 digest=$2 destination=$3
    if [[ -e $destination ]]; then
        [[ -f $destination && ! -L $destination ]] || return 1
        local observed
        observed=$(sha256sum -- "$destination" | awk '{ print $1 }') || return 1
        [[ $observed == "$digest" ]] || return 1
        existing=$((existing + 1))
        return 0
    fi
    local parent candidate
    parent=$(dirname -- "$destination")
    mkdir -p -- "$parent" || return 1
    [[ ! -L $parent ]] || return 1
    candidate=$(mktemp "$parent/.new-$digest.XXXXXXXX") || return 1
    cp -- "$source" "$candidate" || return 1
    local reread
    reread=$(sha256sum -- "$candidate" | awk '{ print $1 }') || return 1
    [[ $reread == "$digest" ]] || return 1
    if mv -n -- "$candidate" "$destination"; then
        if [[ -e $candidate ]]; then
            rm -f -- "$candidate"
            observed=$(sha256sum -- "$destination" | awk '{ print $1 }') || return 1
            [[ $observed == "$digest" ]] || return 1
            existing=$((existing + 1))
        else
            created=$((created + 1))
        fi
    else
        rm -f -- "$candidate"
        return 1
    fi
}

while read -r kind digest bytes leaf; do
    [[ $kind == blob && $digest =~ ^[0-9a-f]{64}$ && $bytes =~ ^[0-9]+$ ]] || exit 1
    ordinal=$((ordinal + 1))
    echo "package store step=publish-object object=$ordinal/$blob_count sha256=$digest bytes=$bytes"
    destination="$objects_root/${digest:0:2}/${digest:2}"
    publish_immutable "$work/$leaf" "$digest" "$destination" || exit 1
done < "$work/Inventory.txt"

echo "package store step=publish-bundle sha256=$bundle_sha256"
publish_immutable "$bundle" "$bundle_sha256" \
    "$bundles_root/${bundle_sha256:0:2}/${bundle_sha256:2}.wvbundle" || exit 1
echo "package store status=Published bundle=$bundle_sha256 objects=$blob_count created=$created existing=$existing"
