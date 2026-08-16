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
build Filesystem Projects/Operating-System/Windvale-Os-Filesystem-Process-Service.wvproj Operating-System/Kernel/Filesystem-Process-Service-Shim.wva Windvale_filesystem_process_service_main Windvale_filesystem_process_user_entry 14812 054dc2c9b5c33e02e6263b644049fd84f1ed2e1219d642ec64c066af5bdc8fcf 196327 5ee235d5dca7bfdab8a5a1b7c54874b6545725e69da754e22e06f72f578ebdb3 302 dc212ce43b59102a05521531e6df4674291851c72a1be8990eff049ea46879dd 195657 453cef870da3f375400d1c58cc8ebd385f761c2eafbdf3b3fb70603db8520dab || exit 1
build Network Projects/Operating-System/Windvale-Os-Network-Process-Service.wvproj Operating-System/Kernel/Network-Process-Service-Shim.wva Windvale_network_process_service_main Windvale_network_process_user_entry 13543 32c595716af0a3706226d677924a5279ea2d7b97b0a4cbdf7c6c9eed808e1b2a 243124 892cfe18b81667c9e4d3e82a1889a9b1f77c45e350d2e75144694db3c2f49ca0 296 628852893fcbc32e610261517a79c3acd56714ce0c197beab1c0a3917dedf726 242571 57067da10da68fc1d35b41784e147d8f60ed1e05441cb68bc803ad5a9682f6d1 || exit 1
echo 'native os provider images status=Passed services=2 readiness=host-specific cases=8'
