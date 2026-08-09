#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 2 ]]; then
    echo 'Usage: ./Tools/Native/Construct-Hosted-Verifier-Publisher.sh <windows|linux> <output.exe|output.elf>' >&2
    exit 64
fi

target_name=$1
output=$2
case "$target_name:$output" in
    windows:*.exe)
        target=1
        console_leaf=Native-X64-Windows-Console-Output-Service.bin
        console_bytes=258
        console_sha256=10f3a500aca7f0236cdf9f6c20658591df88bc612e677264cdaa0bcef59a0a48
        file_input_leaf=Native-X64-Windows-File-Input-Service.bin
        file_input_bytes=1218
        file_input_sha256=3d2fffc028083cdc4cfd39e553dea603e9a1ae661bb5df3f14ca438c4d3e3cf8
        diagnostic_leaf=Native-X64-Windows-Diagnostic-Output-Service.bin
        diagnostic_bytes=258
        diagnostic_sha256=1b4068c01b2050c3055c78eb82303c71b8488e8766f7b628fab10ffb23e5ffe2
        hosted_startup=Windows-X64-Hosted-Verifier.wvo
        hosted_startup_bytes=3561
        hosted_startup_sha256=755ffb99cba6a838dd9eec353ce72d4adfb3af130ec4bce5a2278828dd136616
        publisher_startup=Windows-X64-Wvb-Publisher.wvo
        publisher_startup_bytes=168
        publisher_startup_sha256=bb136af0382b2f72efc8a07f58fb2368319fce7c119bc7bbfa1b94da6ded9367
        adapter=Windows-X64-Wvb-Publication-Adapter.wvo
        adapter_bytes=9544
        adapter_sha256=ef795dabbced735e0808fca04d0205b87d3735b26dd53ca23ed57a7e74453e93
        base_bytes=248832
        base_sha256=cf204201e5c26d71e78da1112de2bc724d389a5222cc835d48dbe8cd8bbc5988
        application_bytes=256000
        application_sha256=735320b5ff33419d685925044add6f254bf402c0d49fc575c77f6110fac705f6
        ;;
    linux:*.elf)
        target=2
        console_leaf=Native-X64-Linux-Console-Output-Service.bin
        console_bytes=213
        console_sha256=c5ea073a24c46dd634b1a67a7e7041d476dbce856d058aa8adc2c4e680d3d226
        file_input_leaf=Native-X64-Linux-File-Input-Service.bin
        file_input_bytes=996
        file_input_sha256=55ae4524c463f064aee0964d7f9b64438701fb4375a97c53d11f2f17902c12cb
        diagnostic_leaf=Native-X64-Linux-Diagnostic-Output-Service.bin
        diagnostic_bytes=213
        diagnostic_sha256=1c81018143fa9b708373eaceda62722ca40fb1e11b20808f765fe5ece33406fe
        hosted_startup=Linux-X64-Hosted-Verifier.wvo
        hosted_startup_bytes=1925
        hosted_startup_sha256=08a7afefb69904af8d8c899a86bec76e957dfe255d397dbd9015d9acaa018ae8
        publisher_startup=Linux-X64-Wvb-Publisher.wvo
        publisher_startup_bytes=164
        publisher_startup_sha256=eee997412ced0d7edacaf39dae9c4a3c51e859dce4537045f3972be990b115a4
        adapter=Linux-X64-Wvb-Publication-Adapter.wvo
        adapter_bytes=5507
        adapter_sha256=9272c17b0d7234218a6cd7c31131e9d25e62b6c1ccd976d94975e9b436b2ca5a
        base_bytes=249856
        base_sha256=0bdeee07a49f75781767934884cbbc7dd085abff4507e2f78210fa225638539a
        application_bytes=254917
        application_sha256=de4f06f6d837eb58457a31b4757c3410e389ecc3c11fd79daf229dbdeb23e02a
        ;;
    *)
        echo 'Usage: ./Tools/Native/Construct-Hosted-Verifier-Publisher.sh <windows|linux> <output.exe|output.elf>' >&2
        exit 64
        ;;
esac

if [[ -e $output ]]; then
    echo 'Refusing to replace an existing publisher construction output.' >&2
    exit 1
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
hosted_toolset="$repository_root/Artifacts/Native-Hosted-Container-Toolset-Candidate"
construction="$repository_root/Artifacts/Native-Hosted-Verifier-Publisher-Construction-Candidate"
publisher_tools="$construction/linux-x64"
publisher_wvb="$repository_root/Artifacts/Native-Hosted-Verifier-Application-Publisher-Candidate/Hosted-Verifier-Application-Publisher.wvb"
service_root="$repository_root/Runtime/Windvale.Native/Consumers"
consumer_root="$repository_root/Linker/Reference/Consumers"

check_hash() {
    local path=$1
    local digest=$2
    local directory
    directory=$(CDPATH= cd -- "$(dirname -- "$path")" && pwd -P) || return 1
    (cd -- "$directory" && printf '%s  %s\n' "$digest" "$(basename -- "$path")" |
        sha256sum --check --strict --quiet)
}

check_file() {
    local path=$1
    local bytes=$2
    local digest=$3
    local label=$4
    if [[ ! -f $path ]]; then
        echo "Missing $label: $path" >&2
        return 1
    fi
    if [[ $(wc -c < "$path") -ne $bytes ]]; then
        echo "The $label byte length is invalid." >&2
        return 1
    fi
    if ! check_hash "$path" "$digest"; then
        echo "The $label digest is invalid: $path" >&2
        return 1
    fi
}

check_file "$hosted_toolset/SHA256SUMS" 6927 \
    a7eb43d58a81ee57881f800b2c17b70c2014c26ce4454fa299feb2986348fb58 \
    'hosted toolset inventory' || exit $?
(cd -- "$hosted_toolset" && sha256sum --check --strict --quiet SHA256SUMS) || exit $?
check_file "$construction/SHA256SUMS" 4634 \
    83df3a245217c20bd704685e79d296c03bbdd85ee0377cd046a38f995735e273 \
    'publisher construction inventory' || exit $?
(cd -- "$construction" && sha256sum --check --strict --quiet SHA256SUMS) || exit $?
check_file "$publisher_wvb" 29170 \
    77c6f34a823fc41175647c4d0c4708507ab8b97c7b1726c983188f962fd5509f \
    'publisher WVB' || exit $?
check_file "$service_root/$console_leaf" "$console_bytes" "$console_sha256" \
    'console service' || exit $?
check_file "$service_root/Native-X64-Argument-Count-Service.bin" 5 \
    2358e7e2c72d6476cfe05134db4f0eb5e6987fcca1b10894a8588a28d3929829 \
    'argument-count service' || exit $?
check_file "$service_root/Native-X64-Argument-Service.bin" 70 \
    2253e1435f141df5b68f9f7e9e9aa0de448410c42dcf33ad76dcf131afea65d1 \
    'argument service' || exit $?
check_file "$service_root/$file_input_leaf" "$file_input_bytes" \
    "$file_input_sha256" 'file-input service' || exit $?
check_file "$service_root/Native-X64-Utf8-Service.bin" 800 \
    4c3d2e370d62c8d2f54a3c453f39b94cf46ddabd6db3c2f3d6b65f0713b68aaf \
    'UTF-8 service' || exit $?
check_file "$service_root/$diagnostic_leaf" "$diagnostic_bytes" \
    "$diagnostic_sha256" 'diagnostic service' || exit $?
check_file "$consumer_root/$hosted_startup" "$hosted_startup_bytes" \
    "$hosted_startup_sha256" 'hosted-verifier startup object' || exit $?
check_file "$consumer_root/$publisher_startup" "$publisher_startup_bytes" \
    "$publisher_startup_sha256" 'publisher startup object' || exit $?
check_file "$consumer_root/$adapter" "$adapter_bytes" "$adapter_sha256" \
    'publication adapter object' || exit $?
check_file "$consumer_root/X64-Wvb-Publication-Sha256.wvo" 2176 \
    380af02cf29f85be1f63a4ea1f02ca3cc027e63091659e214a023b03730f6608 \
    'publication SHA-256 object' || exit $?

temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d \
    "$temporary_root/windvale-hosted-verifier-publisher.XXXXXXXX") || exit 1
output_created=0
cleanup() {
    local status=$?
    if ((status != 0 && output_created == 1)); then
        rm -f -- "$output"
    fi
    case "$temporary_directory" in
        "$temporary_root"/windvale-hosted-verifier-publisher.*)
            rm -f -- "$temporary_directory"/*
            rmdir -- "$temporary_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $temporary_directory" >&2
            return 1
            ;;
    esac
    return "$status"
}
trap cleanup EXIT

"$repository_root/Tools/Native/Lower-Wvb-To-Wvo.sh" \
    "$publisher_wvb" "$temporary_directory/Publisher.wvo" >/dev/null || exit $?
check_file "$temporary_directory/Publisher.wvo" 233804 \
    ef0f5e49a07450e3d957e5576f819201849b705097bfbf75432c76d2c438ec23 \
    'lowered publisher object' || exit $?
cmp --silent "$temporary_directory/Publisher.wvo" "$construction/Publisher.wvo" || exit 1
"$repository_root/Tools/Native/Link-Wvo.sh" 0 Main \
    "$temporary_directory/Publisher.bin" "$temporary_directory/Publisher.wvo" \
    > "$temporary_directory/Link.txt" || exit $?
grep -Fx 'entry name=Main address=3001' "$temporary_directory/Link.txt" >/dev/null || exit 1
check_file "$temporary_directory/Publisher.bin" 232736 \
    260e9f4f23c99dab13145ceb98724a4c74157fc579c5685194b7312c1a5cb115 \
    'linked publisher fragment' || exit $?

"$hosted_toolset/linux-x64/wvhostverifierbundle.elf" \
    "$temporary_directory/Publisher.bin" "$service_root/$console_leaf" \
    "$service_root/Native-X64-Argument-Count-Service.bin" \
    "$service_root/Native-X64-Argument-Service.bin" \
    "$service_root/$file_input_leaf" "$service_root/Native-X64-Utf8-Service.bin" \
    "$service_root/$diagnostic_leaf" "$temporary_directory/Bundle-Request.wvsq" \
    >/dev/null || exit $?
"$publisher_tools/wvhostverifierpublisherbasemetadata.elf" "$target" 3001 \
    "$temporary_directory/Bundle-Request.wvsq" "$temporary_directory/Metadata.wvhv" || exit $?
"$publisher_tools/wvhostverifierpublisherbaseruntime.elf" \
    "$temporary_directory/Metadata.wvhv" "$temporary_directory/Runtime.wvhr" || exit $?
"$hosted_toolset/linux-x64/wvhostbundle.elf" \
    "$temporary_directory/Bundle-Request.wvsq" "$temporary_directory/Bundle.wvsi" \
    >/dev/null || exit $?
"$hosted_toolset/linux-x64/wvhostverifierbytes.elf" \
    "$temporary_directory/Runtime.wvhr" "$temporary_directory/Platform.wvhb" \
    >/dev/null || exit $?
"$hosted_toolset/linux-x64/wvhostverifierstartup.elf" \
    "$temporary_directory/Runtime.wvhr" "$consumer_root/$hosted_startup" \
    "$temporary_directory/Startup.wvsd" >/dev/null || exit $?
"$hosted_toolset/linux-x64/wvhostverifiercompose.elf" \
    "$temporary_directory/Runtime.wvhr" "$temporary_directory/Platform.wvhb" \
    "$temporary_directory/Startup.wvsd" "$temporary_directory/Bundle.wvsi" \
    "$temporary_directory/Base.application" >/dev/null || exit $?
check_file "$temporary_directory/Base.application" "$base_bytes" "$base_sha256" \
    'publisher base application' || exit $?

"$publisher_tools/wvhostverifierproducemetadata.elf" "$target" "$publisher_wvb" \
    "$consumer_root/$publisher_startup" "$temporary_directory/Publisher-Metadata.wvvp" \
    >/dev/null || exit $?
"$publisher_tools/wvhostverifieridentity.elf" "$target" "$publisher_wvb" \
    "$temporary_directory/Publisher.wvo" "$consumer_root/$publisher_startup" \
    "$consumer_root/$adapter" "$consumer_root/X64-Wvb-Publication-Sha256.wvo" \
    "$temporary_directory/Publisher-Metadata.wvvp" "$temporary_directory/Identity.wvpi" \
    >/dev/null || exit $?
"$publisher_tools/wvhostverifierstructure.elf" \
    "$temporary_directory/Identity.wvpi" "$temporary_directory/Structure.wvps" \
    >/dev/null || exit $?
"$publisher_tools/wvhostverifierconstructrequest.elf" \
    "$temporary_directory/Structure.wvps" "$temporary_directory/Construction.wvcr" \
    >/dev/null || exit $?
"$publisher_tools/wvhostverifiertargets.elf" \
    "$temporary_directory/Structure.wvps" "$temporary_directory/Targets.wvpt" \
    >/dev/null || exit $?
"$publisher_tools/wvhostverifierpublishobjects.elf" \
    "$temporary_directory/Construction.wvcr" "$temporary_directory/Targets.wvpt" \
    "$consumer_root/$publisher_startup" "$consumer_root/$adapter" \
    "$consumer_root/X64-Wvb-Publication-Sha256.wvo" \
    "$temporary_directory/Objects.wvio" >/dev/null || exit $?
if [[ $target -eq 1 ]]; then
    "$publisher_tools/wvhostverifierpublishimports.elf" \
        "$temporary_directory/Imports.wvim" >/dev/null || exit $?
    "$publisher_tools/wvhostverifierpublishwindows.elf" \
        "$temporary_directory/Base.application" "$temporary_directory/Construction.wvcr" \
        "$temporary_directory/Objects.wvio" "$temporary_directory/Publisher-Metadata.wvvp" \
        "$temporary_directory/Imports.wvim" "$temporary_directory/Publisher.application" \
        >/dev/null || exit $?
else
    "$publisher_tools/wvhostverifierpublishlinux.elf" \
        "$temporary_directory/Base.application" "$temporary_directory/Construction.wvcr" \
        "$temporary_directory/Objects.wvio" "$temporary_directory/Publisher-Metadata.wvvp" \
        "$temporary_directory/Publisher.application" >/dev/null || exit $?
fi
check_file "$temporary_directory/Publisher.application" "$application_bytes" \
    "$application_sha256" 'completed publisher application' || exit $?
output_created=1
cp -- "$temporary_directory/Publisher.application" "$output" || exit $?
check_file "$output" "$application_bytes" "$application_sha256" \
    'published construction output' || exit $?
printf 'publisher construction status=Valid target=%s bytes=%s\n' \
    "$target_name" "$application_bytes"
exit 0
