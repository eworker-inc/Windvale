#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 2 && $# -ne 3 ]]; then
    echo 'Usage: ./Tools/Native/Construct-Hosted-Verifier-Publisher.sh [publisher|promoter|wvb-publisher] <windows|linux> <output.exe|output.elf>' >&2
    exit 64
fi

role=publisher
if [[ $# -eq 2 ]]; then
    target_name=$1
    output=$2
else
    role=$1
    target_name=$2
    output=$3
    if [[ $role != publisher && $role != promoter && $role != wvb-publisher ]]; then
        echo 'Usage: ./Tools/Native/Construct-Hosted-Verifier-Publisher.sh [publisher|promoter|wvb-publisher] <windows|linux> <output.exe|output.elf>' >&2
        exit 64
    fi
fi
case "$target_name:$output" in
    windows:*.exe)
        target=1
        console_leaf=Native-X64-Windows-Console-Output-Service.bin
        console_bytes=258
        console_sha256=10f3a500aca7f0236cdf9f6c20658591df88bc612e677264cdaa0bcef59a0a48
        file_input_leaf=Native-X64-Windows-File-Input-Service.bin
        file_input_bytes=1218
        file_input_sha256=3e2fd284d4991d0f713301514d3fbf6af8ec84af7bd7289698c08a41d434c52d
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
        base_sha256=2afd9d92422b063abd3cd20d8da6056efbbbff9e7ac8baeef9c8b60b391686c5
        application_bytes=256000
        application_sha256=17cb5c4228e8448693b17f1b73695fd0ecfd03d7ada922794a5bf3bd7594fc96
        if [[ $role == promoter ]]; then
            base_bytes=674816
            base_sha256=927476ca389c7449fb0c72341f26d68577a6a9e0c0ed02fa45ac8c4af935c77f
            application_bytes=681472
            application_sha256=598bd2de8247abd19d931efa1edcc8323adef7f56da51da1d41256933667eb23
        fi
        if [[ $role == wvb-publisher ]]; then
            base_bytes=1333760
            base_sha256=8fcdcfc755439ebae5086c72d88113fb52f397ba0687c785af247230a7732fff
            application_bytes=1340928
            application_sha256=71794a6a254ccfd652ffe3bad556c32f86e2d9210a5a3099bad576f97476a8f3
        fi
        ;;
    linux:*.elf)
        target=2
        console_leaf=Native-X64-Linux-Console-Output-Service.bin
        console_bytes=213
        console_sha256=c5ea073a24c46dd634b1a67a7e7041d476dbce856d058aa8adc2c4e680d3d226
        file_input_leaf=Native-X64-Linux-File-Input-Service.bin
        file_input_bytes=996
        file_input_sha256=cbd78340641fa02589d0d96b73d233a67f9404ab76c3df2b1346b2e31ca43701
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
        base_sha256=687338281ca78c9d3a4d08b601c1efbcc198ec3c8fcc96fbf34f5dc349cafae2
        application_bytes=254917
        application_sha256=babe721a573e29f89ec095c35677880077ff465d4e2129063f6742cd47591a97
        if [[ $role == promoter ]]; then
            base_bytes=675840
            base_sha256=768ca223c99e901d17a1c5d86744515e4b571a6feae329fb6fc3cf225215a133
            application_bytes=680901
            application_sha256=422332fb4f2824ae558bf93adadb6470597399d07810f5428f71aa4d971a4f58
        fi
        if [[ $role == wvb-publisher ]]; then
            base_bytes=1335296
            base_sha256=f53a4c8c5d292e999735cf5fd337b7c6997c0a8e6d2ba316ec94cd6b0838b090
            application_bytes=1340357
            application_sha256=7f2dbfaecf2734c5afdbd6e2e54263a5a74038b8a498eeb1e155ee71788b630c
        fi
        ;;
    *)
        echo 'Usage: ./Tools/Native/Construct-Hosted-Verifier-Publisher.sh [publisher|promoter|wvb-publisher] <windows|linux> <output.exe|output.elf>' >&2
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
publisher_object="$construction/Publisher.wvo"
variant=0
publisher_wvb_bytes=29170
publisher_wvb_sha256=77c6f34a823fc41175647c4d0c4708507ab8b97c7b1726c983188f962fd5509f
publisher_object_bytes=233804
publisher_object_sha256=ef0f5e49a07450e3d957e5576f819201849b705097bfbf75432c76d2c438ec23
native_entry=3001
fragment_bytes=232736
fragment_sha256=260e9f4f23c99dab13145ceb98724a4c74157fc579c5685194b7312c1a5cb115
if [[ $role == promoter ]]; then
    publisher_wvb="$construction/Publisher-Promoter.wvb"
    publisher_object="$construction/Publisher-Promoter.wvo"
    variant=1
    publisher_wvb_bytes=41268
    publisher_wvb_sha256=c0c7c88996ef837bc5a2ec3ceb1de61254b025fbd6504e4f3d7dc055c4140672
    publisher_object_bytes=660123
    publisher_object_sha256=ba5d9c5afde115fede472369d24c3d1fe466806de523773d2e445e6a9e004667
    native_entry=1178
    fragment_bytes=658339
    fragment_sha256=e06189a37c038a5237787ffd16fb53466df3d10519efd4129b219bd814f4def2
fi
if [[ $role == wvb-publisher ]]; then
    publisher_wvb="$construction/Wvb-Publisher.wvb"
    publisher_object="$construction/Wvb-Publisher.wvo"
    variant=2
    publisher_wvb_bytes=159770
    publisher_wvb_sha256=8247539e0f4a5436b3902ec1fef33c6c39c231703de7bf505a6c65d66a764f96
    publisher_object_bytes=1319377
    publisher_object_sha256=edc49bbae0bfd16a38db4a08d9a6e636edfac35828e1c6b050c45d85d5e1f9e3
    native_entry=0
    fragment_bytes=1317613
    fragment_sha256=9003479563a043bb69113be43100289f653f6772356c48a17098c1c6700f5271
fi
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
    bca5cead0b3698f060c4cc5a165eb75dc52aaad5e81202ef95c54f16976d0ded \
    'hosted toolset inventory' || exit $?
(cd -- "$hosted_toolset" && sha256sum --check --strict --quiet SHA256SUMS) || exit $?
check_file "$construction/SHA256SUMS" 4980 \
    4989e21858705df8fb1776b36a26350144b6bf02fab5bd8d910e1711f2a7691d \
    'publisher construction inventory' || exit $?
(cd -- "$construction" && sha256sum --check --strict --quiet SHA256SUMS) || exit $?
check_file "$publisher_wvb" "$publisher_wvb_bytes" "$publisher_wvb_sha256" \
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
check_file "$temporary_directory/Publisher.wvo" "$publisher_object_bytes" \
    "$publisher_object_sha256" \
    'lowered publisher object' || exit $?
cmp --silent "$temporary_directory/Publisher.wvo" "$publisher_object" || exit 1
"$repository_root/Tools/Native/Link-Wvo.sh" 0 Main \
    "$temporary_directory/Publisher.bin" "$temporary_directory/Publisher.wvo" \
    > "$temporary_directory/Link.txt" || exit $?
grep -Fx "entry name=Main address=$native_entry" "$temporary_directory/Link.txt" >/dev/null || exit 1
check_file "$temporary_directory/Publisher.bin" "$fragment_bytes" \
    "$fragment_sha256" \
    'linked publisher fragment' || exit $?

"$hosted_toolset/linux-x64/wvhostverifierbundle.elf" \
    "$temporary_directory/Publisher.bin" "$service_root/$console_leaf" \
    "$service_root/Native-X64-Argument-Count-Service.bin" \
    "$service_root/Native-X64-Argument-Service.bin" \
    "$service_root/$file_input_leaf" "$service_root/Native-X64-Utf8-Service.bin" \
    "$service_root/$diagnostic_leaf" "$temporary_directory/Bundle-Request.wvsq" \
    >/dev/null || exit $?
"$publisher_tools/wvhostverifierpublisherbasemetadata.elf" "$target" "$native_entry" \
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

if [[ $variant -eq 0 ]]; then
    "$publisher_tools/wvhostverifierproducemetadata.elf" "$target" "$publisher_wvb" \
        "$consumer_root/$publisher_startup" "$temporary_directory/Publisher-Metadata.wvvp" \
        >/dev/null || exit $?
else
    "$publisher_tools/wvhostverifierproducemetadata.elf" "$variant" "$target" \
        "$publisher_wvb" "$consumer_root/$publisher_startup" \
        "$temporary_directory/Publisher-Metadata.wvvp" >/dev/null || exit $?
fi
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
    if [[ $variant -eq 0 ]]; then
        "$publisher_tools/wvhostverifierpublishimports.elf" \
            "$temporary_directory/Imports.wvim" >/dev/null || exit $?
    else
        "$publisher_tools/wvhostverifierpublishimports.elf" "$role" \
            "$temporary_directory/Imports.wvim" >/dev/null || exit $?
    fi
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
if [[ $role == publisher ]]; then
    printf 'publisher construction status=Valid target=%s bytes=%s\n' \
        "$target_name" "$application_bytes"
elif [[ $role == promoter ]]; then
    printf 'publisher promoter construction status=Valid target=%s bytes=%s\n' \
        "$target_name" "$application_bytes"
else
    printf 'WVB publisher construction status=Valid target=%s bytes=%s\n' \
        "$target_name" "$application_bytes"
fi
exit 0
