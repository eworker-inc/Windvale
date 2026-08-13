#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 1 || $1 != *.wvo ]]; then
    echo 'Usage: ./Tools/Native/Build-Os-Process-Object.sh <output.wvo>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
toolset="$repository_root/Artifacts/Native-Os-Process-Object-Toolset-Candidate"
output_directory=$(CDPATH= cd -- "$(dirname -- "$1")" && pwd -P) || exit 1
output="$output_directory/$(basename -- "$1")"
if [[ -e $output ]]; then
    echo 'The OS process-object output already exists.' >&2
    exit 1
fi

verify_identity() {
    local path=$1
    local bytes=$2
    local digest=$3
    [[ -f $path && $(wc -c < "$path") -eq $bytes ]] &&
        printf '%s  %s\n' "$digest" "$path" |
            sha256sum --check --strict --quiet
}

verify_identity "$toolset/linux-x64-boot-resource-object.elf" 389120 21f15f20769465b6b9c2272147a00f995e455b9e6fd2db354772bf932fc7194b || exit 1
verify_identity "$toolset/linux-x64-process-resource-store.elf" 49152 8707ca6ce129a2c7c3cb33586444201088cfd764d12742658638821c8c014bcd || exit 1
verify_identity "$toolset/linux-x64-process-directory-snapshot.elf" 49152 d5f97d27d6f51b88d9b552abc007d4e0a8fe7b32c153b5f24ae13f57fc33fdc4 || exit 1
verify_identity "$toolset/linux-x64-process-object.elf" 180224 8ff023ada9e6b903be5a0b06ad4335bba9737eb8dc65aaf611d67a51e24555f9 || exit 1
verify_identity "$toolset/normal-x64-process.bin" 46678 05938e22e02abac6d396fa5a64342d94609900a6401b112f18de0fb5421a41b5 || exit 1

work=$(mktemp -d "$output_directory/.windvale-os-process-object.XXXXXXXX") || exit 1
case "$work" in
    "$output_directory"/.windvale-os-process-object.*) ;;
    *)
        echo 'The OS process-object private path is outside the output directory.' >&2
        exit 1
        ;;
esac
cleanup() {
    rm -rf -- "$work"
}
trap cleanup EXIT

run_logged() {
    local log=$1
    shift
    if ! "$@" >"$work/$log" 2>&1; then
        cat -- "$work/$log" >&2
        exit 1
    fi
}

run_logged Build-Init.log "$script_directory/Build-Wvb.sh" "$repository_root/Projects/Operating-System/Windvale-Os-Init-Resource-Service.wvproj" "$work/Init.wvb"
verify_identity "$work/Init.wvb" 526 7cefa7dcf82ed05d6b6e133aa79b7da90372e2d8f8f993abe7449513398ede83 || exit 1
run_logged Lower-Init.log "$script_directory/Lower-Wvb-To-Wvo.sh" "$work/Init.wvb" "$work/Init-Main.wvo"
verify_identity "$work/Init-Main.wvo" 3424 1a1a8599e7e9f92ebdb9c8e8c2df202311de3ffe3a549f3f339efdce4ef47456 || exit 1
run_logged Rename-Init.log "$script_directory/Rename-Wvo-Export.sh" "$work/Init-Main.wvo" Main Windvale_init_resource_service_main "$work/Init.wvo"
verify_identity "$work/Init.wvo" 3455 4b8126d1baa38054fc70165be3c2f9519e7bea7e1f4d5596bcae36f2567ddf11 || exit 1

run_logged Build-Directory.log "$script_directory/Build-Wvb.sh" "$repository_root/Projects/Operating-System/Windvale-Os-Directory-Process-Service.wvproj" "$work/Directory.wvb"
verify_identity "$work/Directory.wvb" 474 f7410595f9824e510da9399f52a463013ff41240b67308cdf28b4f5b7484ab2b || exit 1
run_logged Lower-Directory.log "$script_directory/Lower-Wvb-To-Wvo.sh" "$work/Directory.wvb" "$work/Directory-Main.wvo"
verify_identity "$work/Directory-Main.wvo" 2768 f80f17b1ae73885eb8fa7b81d319a089ea680994439d4d7debad58ad952e179e || exit 1
run_logged Rename-Directory.log "$script_directory/Rename-Wvo-Export.sh" "$work/Directory-Main.wvo" Main Windvale_directory_process_service_main "$work/Directory.wvo"
verify_identity "$work/Directory.wvo" 2803 04339b8fd627c6b765a16903ad339408c86eaa9877bdc52357cbafa33e98679a || exit 1

run_logged Build-Interpreter.log "$script_directory/Build-Wvb.sh" "$repository_root/Projects/Operating-System/Windvale-Os-Bytecode-Interpreter.wvproj" "$work/Interpreter.wvb"
verify_identity "$work/Interpreter.wvb" 56307 e2024702919e9acd37c119a7afb9991a73904d97ef3bdb1defe8c5ea13e91a3d || exit 1
run_logged Lower-Interpreter.log "$script_directory/Lower-Wvb-To-Wvo.sh" "$work/Interpreter.wvb" "$work/Interpreter-Main.wvo"
verify_identity "$work/Interpreter-Main.wvo" 448737 dca63103e751f74e528514b25cb8650a7361e94172381a93dbfc8d5014844d78 || exit 1
run_logged Rename-Interpreter.log "$script_directory/Rename-Wvo-Export.sh" "$work/Interpreter-Main.wvo" Main Windvale_user_bytecode_interpreter_main "$work/Interpreter.wvo"
verify_identity "$work/Interpreter.wvo" 448772 7fb4a3d3a4aca6f44f6ab8bed3a2891147e319f275c6c2af3eab42e8c5763c4d || exit 1

run_logged Build-Program.log "$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Function-Only.wvproj" "$work/Program.wvb"
verify_identity "$work/Program.wvb" 816 28d215b982a7b7185cfa80c4cc5346666bd0181582fe80bec8b7035d514da936 || exit 1

run_logged Assemble-Init.log "$script_directory/Assemble-Wva.sh" "$repository_root/Operating-System/Kernel/Init-Resource-Service-Shim.wva" "$work/Init-Shim.wvo"
verify_identity "$work/Init-Shim.wvo" 2118 52098aac184961fda7c3a23c8577851df6c18736555cb169b340d7b0c7249359 || exit 1
run_logged Assemble-Directory.log "$script_directory/Assemble-Wva.sh" "$repository_root/Operating-System/Kernel/Directory-Process-Service-Shim.wva" "$work/Directory-Shim.wvo"
verify_identity "$work/Directory-Shim.wvo" 1549 c0a7524130b8733ed17a3ce52fc04986cb449394c9ee509280120b86a3ed8c88 || exit 1
run_logged Assemble-Boot.log "$script_directory/Assemble-Wva.sh" "$repository_root/Operating-System/Runtime/Boot-Resource-Service.wva" "$work/Boot-Stencil.wvo"
verify_identity "$work/Boot-Stencil.wvo" 462 fde44aad9549731d53c5ccf3a57733b3619df94369b61ef27a693e1059784bc9 || exit 1
run_logged Assemble-User.log "$script_directory/Assemble-Wva.sh" "$repository_root/Operating-System/Kernel/Process-User-Shim.wva" "$work/User-Shim.wvo"
verify_identity "$work/User-Shim.wvo" 1510 69ea7402a3a752e5c4b45689aeeb902b7e2ff1ce87a34bc9bad81417a3992fe6 || exit 1

run_logged Boot-Resource.log "$toolset/linux-x64-boot-resource-object.elf" "$work/Boot-Stencil.wvo" "$work/Boot-Service.wvo"
verify_identity "$work/Boot-Service.wvo" 462 ecb940abb9de8086d50ae418853021cf1f7566a9415a5a3a3b4e5cc45ed5e78c || exit 1

run_logged Link-Init.log "$script_directory/Link-Wvo.sh" 0 Windvale_init_resource_user_entry "$work/Init.bin" "$work/Init-Shim.wvo" "$work/Init.wvo"
verify_identity "$work/Init.bin" 5159 e9624ebe3b857b77d8b1024a4edfdaf23e040ee61f9dfc484e590ce1e5aa18f0 || exit 1
run_logged Link-Directory.log "$script_directory/Link-Wvo.sh" 0 Windvale_directory_process_user_entry "$work/Directory.bin" "$work/Directory-Shim.wvo" "$work/Directory.wvo"
verify_identity "$work/Directory.bin" 3911 f4d047c6f311b1561a5621b98f3db2868a969c54bb81dac2f75d599b7207f3fb || exit 1
run_logged Link-Client.log "$script_directory/Link-Wvo.sh" 0 Windvale_process_user_entry "$work/Client.bin" "$work/User-Shim.wvo" "$work/Interpreter.wvo" "$work/Boot-Service.wvo"
verify_identity "$work/Client.bin" 449261 be4f88ad2460a17e5902670a9ca2bf70021d8b5ce46e2414f00f940a8f4d32b6 || exit 1

run_logged Resource-Store.log "$toolset/linux-x64-process-resource-store.elf" "$work/Program.wvb" "$work/Resources.wvrs"
verify_identity "$work/Resources.wvrs" 1196 624ece2d2e032f6f0929675a8f79ceb223538d84bccace264ecbbfdce5eca4ad || exit 1
run_logged Directory-Snapshot.log "$toolset/linux-x64-process-directory-snapshot.elf" "$work/Directory.wvds"
verify_identity "$work/Directory.wvds" 3184 0f793a41a701240b9cf41179dafa252384b43cd23214646ff021d245657c235a || exit 1

run_logged Process-Object.log "$toolset/linux-x64-process-object.elf" "$toolset/normal-x64-process.bin" "$work/Init.bin" "$work/Client.bin" "$work/Program.wvb" "$work/Resources.wvrs" "$work/Directory.wvds" "$work/Directory.bin" "$work/Process.wvo"
verify_identity "$work/Process.wvo" 512978 dff07c3f6a52dedf6bcd96181221cba50c831359502ec763ee77f6aaaaafdfaa || exit 1
"$script_directory/Verify-Wvo.sh" "$work/Process.wvo" >/dev/null 2>&1 || exit 1
run_logged Publish.log "$script_directory/Publish-Wvo.sh" "$work/Process.wvo" "$output"
