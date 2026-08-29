#!/usr/bin/env bash
set -uo pipefail
[[ $# -eq 0 ]] || exit 64
script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
work=$(mktemp -d "${TMPDIR:-/tmp}/windvale-os-provider-images.XXXXXXXX") || exit 1
trap 'rm -f -- "$work"/*; rmdir -- "$work"' EXIT
verify() {
    [[ $(wc -c <"$1") -eq $2 ]] && [[ $(sha256sum "$1" | cut -d' ' -f1) == "$3" ]]
}
build() {
    local name=$1 project=$2 shim=$3 main=$4 entry=$5 wvb_bytes=$6 wvb_hash=$7 wvo_bytes=$8 wvo_hash=$9
    shift 9
    local shim_bytes=$1 shim_hash=$2 image_bytes=$3 image_hash=$4
    "$script_directory/Build-Wvb.sh" "$root/$project" "$work/$name.wvb" >/dev/null || return
    verify "$work/$name.wvb" "$wvb_bytes" "$wvb_hash" || return
    "$script_directory/Lower-Wvb-To-Wvo.sh" "$work/$name.wvb" "$work/$name-Main.wvo" >/dev/null || return
    verify "$work/$name-Main.wvo" "$wvo_bytes" "$wvo_hash" || return
    "$script_directory/Rename-Wvo-Export.sh" "$work/$name-Main.wvo" Main "$main" "$work/$name.wvo" >/dev/null || return
    "$script_directory/Assemble-Wva.sh" "$root/$shim" "$work/$name-Shim.wvo" >/dev/null || return
    verify "$work/$name-Shim.wvo" "$shim_bytes" "$shim_hash" || return
    "$script_directory/Link-Wvo.sh" 0 "$entry" "$work/$name.bin" "$work/$name-Shim.wvo" "$work/$name.wvo" >/dev/null || return
    verify "$work/$name.bin" "$image_bytes" "$image_hash"
}
build Filesystem Projects/Operating-System/Windvale-Os-Filesystem-Process-Service.wvproj Operating-System/Kernel/Filesystem-Process-Service-Shim.wva Windvale_filesystem_process_service_main Windvale_filesystem_process_user_entry 14812 054dc2c9b5c33e02e6263b644049fd84f1ed2e1219d642ec64c066af5bdc8fcf 196327 c0cbc0ce96f14858de9f3973da4cfb5335f6c7087cdd78e6397b480093d59fcc 302 aae81021f8e5d349570533299bbd1c4196358c3ad857eecc80b5b918c48f301c 195657 d40d9cdb16f9aa115a20bac2b27f572fad853eca27cf2539fe61dfd2ecbd7601 || exit 1
build Network Projects/Operating-System/Windvale-Os-Network-Process-Service.wvproj Operating-System/Kernel/Network-Process-Service-Shim.wva Windvale_network_process_service_main Windvale_network_process_user_entry 13543 32c595716af0a3706226d677924a5279ea2d7b97b0a4cbdf7c6c9eed808e1b2a 243124 892cfe18b81667c9e4d3e82a1889a9b1f77c45e350d2e75144694db3c2f49ca0 296 ffc757391199f456850bdb80a2f67b1815b7bc7c1dda9a1bf6b6ed1919df87af 242571 68182de6018a6c64d02c4a384355ea14c463a67d1939cb18db0c058223358e42 || exit 1
echo 'native os provider images status=Passed services=2 readiness=host-specific cases=8'
