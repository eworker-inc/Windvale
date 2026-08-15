#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Installation-Command-Resolution.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
work=$(mktemp -d "$temporary_root/windvale-command-resolution.XXXXXXXX") || exit 1
cleanup() {
    case "$work" in
        "$temporary_root"/windvale-command-resolution.*) rm -rf -- "$work" ;;
        *) echo "Refusing to remove unexpected temporary path: $work" >&2; return 1 ;;
    esac
}
trap cleanup EXIT

verify_file() {
    local file=$1 expected_bytes=$2 expected_sha256=$3
    [[ -f $file && $(wc -c <"$file") -eq $expected_bytes ]] || return 1
    [[ $(sha256sum -- "$file" | cut -d ' ' -f 1) == "$expected_sha256" ]]
}

echo 'native installation command resolution step=build item=1/3'
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tools/Windvale-Installation-Command-Resolver.wvproj" \
    "$work/Resolver.wvb" || exit $?
verify_file "$work/Resolver.wvb" 60732 \
    521cd77ee53f20cec3157208e4f0b9c93841c212dcabec88f4e7cbc6a9229679 || exit 1

echo 'native installation command resolution step=package item=2/3 target=linux-x64'
"$script_directory/Package-Hosted-Wvb.sh" 6 \
    "$work/Resolver.wvb" "$work/Resolver.elf" linux || exit $?

echo 'native installation command resolution step=resolve item=3/3 cases=8'
node "$repository_root/Tools/Package/Verify-Installation-Command-Resolver.mjs" \
    "$work/Resolver.elf" linux-x64
