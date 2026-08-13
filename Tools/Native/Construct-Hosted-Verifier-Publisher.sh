#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 2 && $# -ne 3 ]]; then
    echo 'Usage: ./Tools/Native/Construct-Hosted-Verifier-Publisher.sh [publisher|promoter|wvb-publisher|wvo-publisher|console-application-publisher] <windows|linux> <output.exe|output.elf>' >&2
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
    if [[ $role != publisher && $role != promoter && $role != wvb-publisher && $role != wvo-publisher && $role != console-application-publisher ]]; then
        echo 'Usage: ./Tools/Native/Construct-Hosted-Verifier-Publisher.sh [publisher|promoter|wvb-publisher|wvo-publisher|console-application-publisher] <windows|linux> <output.exe|output.elf>' >&2
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
        hosted_startup_bytes=3651
        hosted_startup_sha256=4d97a1f30d9c871f2a72911cea2644b32d3ea29a2dbbc76105ec4ab1d001b95f
        publisher_startup=Windows-X64-Wvb-Publisher.wvo
        publisher_startup_bytes=168
        publisher_startup_sha256=bb136af0382b2f72efc8a07f58fb2368319fce7c119bc7bbfa1b94da6ded9367
        adapter=Windows-X64-Wvb-Publication-Adapter.wvo
        adapter_bytes=9544
        adapter_sha256=ef795dabbced735e0808fca04d0205b87d3735b26dd53ca23ed57a7e74453e93
        base_bytes=248832
    base_sha256=579ff68d6645797a08c71a3ead03be6a56c2b4fd7eda8a3db548038eb9ccc007
        application_bytes=256000
        application_sha256=2b165f5029798a4d5467412b65cba0ddffb05dfc449144fd80161d6117784e12
        if [[ $role == promoter ]]; then
            base_bytes=674816
        base_sha256=818b1dcb4ad7145f2beee18c5e9afbb2e5aeab3bb56df905a5f07ae8eb3082ec
            application_bytes=681472
        application_sha256=5690fb32c7fec85551e0c5cd58e4f56589a5ad4c09108b5dde86fa9fc7b3fb92
        fi
        if [[ $role == wvb-publisher ]]; then
            base_bytes=1363968
            base_sha256=243b763d8b49b34108585c56f46c90190eac085a80c59873c8a2cb3e88d16102
            application_bytes=1371136
            application_sha256=b9fd1b11bc1e4a726e4a43b16830a9351fe573b30e547ba8d8f6660f688ed421
        fi
        if [[ $role == wvo-publisher ]]; then
            base_bytes=422912
        base_sha256=22534a8a0ae42e977cd79daa3ff8b6fde5ef39d719edda07726410f95df6683d
            application_bytes=430080
            application_sha256=76f632ffa7998a6cce0386456fee98f02cbb5ec424d0d914a7e1f06ff3853910
        fi
        if [[ $role == console-application-publisher ]]; then
            base_bytes=1151488
        base_sha256=23bf32201666f99af52015d9b3c10ab27d48f088cb766c8701f3f1973b7ab69b
            application_bytes=1158656
            application_sha256=0bafe84096859f4b88dc14be92c6cdc5336d791b7c5b0a332dccb76b913dd24e
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
        adapter_bytes=5559
        adapter_sha256=1a97195d846626276f38dbb44be68a696dd057f701918f66eb46f6e9d7b5999e
        base_bytes=249856
    base_sha256=577bda8af2b1d8fca6f37e894c6b7f920e547f3e2b0bd1a28d2af518743a6629
        application_bytes=254965
        application_sha256=8c9a1dbbb177041c61e4606696ce9ddf9225a98407a7d3af0a4338069a15979e
        if [[ $role == promoter ]]; then
            base_bytes=675840
        base_sha256=848ee9ed30ffc5094f77b4f79b72e3b4a426b4f9e0fc8e26631ed6619596f782
            application_bytes=680949
        application_sha256=3cd1c82807495e34445345b5e61b8c5911434c84d2a6f49a11b21fd2521423f5
        fi
        if [[ $role == wvb-publisher ]]; then
            base_bytes=1363968
            base_sha256=2fc0332887c96ad0fa34d1987091d60ddbbe61f019739d41734cd491b8ca4b64
            application_bytes=1369077
            application_sha256=b8efb90f7d7c4eae99de01df6c0a3c24a7396d9b9e717ff69d005282ed3d63af
        fi
        if [[ $role == wvo-publisher ]]; then
            base_bytes=421888
            base_sha256=af61a601f4cd8e7fb81704353160a518d2e4f199084fde4b29518d27c89774f7
            application_bytes=426997
            application_sha256=2889237d7fdb20b1d420c05834f19183d18b02112e3f4eea0ed7ff43414814f2
        fi
        if [[ $role == console-application-publisher ]]; then
            base_bytes=1150976
            base_sha256=a12ab6d136b53c53322d4b7ff612a5f41a2653c30210a4f5dbfb27027bc29f5e
            application_bytes=1156085
            application_sha256=e9b8771978c9fb06c3a8ecc55c7b9a3ba1acd24faa541dc669920c10ed792925
        fi
        ;;
    *)
        echo 'Usage: ./Tools/Native/Construct-Hosted-Verifier-Publisher.sh [publisher|promoter|wvb-publisher|wvo-publisher|console-application-publisher] <windows|linux> <output.exe|output.elf>' >&2
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
publisher_wvb_sha256=7ecbd7f0b11bdd7ce0ab578767b1d697bc16653e4f8182858e0ad8b8d808fb9e
publisher_object_bytes=233804
publisher_object_sha256=fa18dbf680fd30f4bc9a5ab5ea8806d958f8af3be304e4e7791337e1a043418a
native_entry=3001
fragment_bytes=232736
fragment_sha256=c54b79c39810ba1e47adf332be46a05497b4e8436372376ea2080a526e6d89a8
if [[ $role == promoter ]]; then
    publisher_wvb="$construction/Publisher-Promoter.wvb"
    publisher_object="$construction/Publisher-Promoter.wvo"
    variant=1
    publisher_wvb_bytes=41268
        publisher_wvb_sha256=7ea1cda2842c4258f654ee17deb441c1b06a3fcedfc29f7382e9259b2f3800fe
    publisher_object_bytes=660123
        publisher_object_sha256=9ee875a6668b1661087dc6a59384c2427e6ef6febb5c83a4ed936e56cd13b44f
    native_entry=1178
    fragment_bytes=658339
        fragment_sha256=843094cf8ba3de92697568abab6788a276f0ea7bd193e65abfb5c7b56918fb43
fi
if [[ $role == wvb-publisher ]]; then
    publisher_wvb="$construction/Wvb-Publisher.wvb"
    publisher_object="$construction/Wvb-Publisher.wvo"
    variant=2
    publisher_wvb_bytes=163300
    publisher_wvb_sha256=9ebfe92eef070dfdcf18c4d176b5f32f64ad3f80751340b8a59ab2f1d567ec2a
    publisher_object_bytes=1349361
    publisher_object_sha256=43a594776b4e280575ac14e2866b4708961dd1290d643b41779a4933a8ba5991
    native_entry=0
    fragment_bytes=1347597
    fragment_sha256=3d419d28b606408e7b2430cceacf4c0b7b109bcd511df4e98ca0d41b871f1c2d
fi
if [[ $role == wvo-publisher ]]; then
    publisher_wvb="$repository_root/Artifacts/Native-Wvo-Publisher-Candidate/Wvo-Publisher.wvb"
    publisher_object="$construction/Wvo-Publisher.wvo"
    variant=3
    publisher_wvb_bytes=41365
    publisher_wvb_sha256=4e8c81da38f5eb06f9334c2d2c5e35120a13e73bac3a9375b5e6a2eff04438c5
    publisher_object_bytes=408284
    publisher_object_sha256=29c1cc269b9387944b4d43fe9215392044996ad47da55be45a1d177f26e5bafb
    native_entry=0
    fragment_bytes=406840
    fragment_sha256=591231b7900aecea5700e139dfd67e36afa3e04a68a87d255aa2be3eb852c828
fi
if [[ $role == console-application-publisher ]]; then
    publisher_wvb="$repository_root/Artifacts/Native-Console-Application-Publisher-Candidate/Console-Application-Publisher.wvb"
    publisher_object="$repository_root/Artifacts/Native-Console-Application-Publisher-Candidate/Console-Application-Publisher.wvo"
    variant=4
    publisher_wvb_bytes=115107
    publisher_wvb_sha256=e8121fb76c7cc39b159d53a3c28d1da8bc2d44968d630495c692a7761656923d
    publisher_object_bytes=1139440
    publisher_object_sha256=259c7d746c3a217c32706bfd617cf66894066bd2e50850cbe5733ac3338e4952
    native_entry=18902
    fragment_bytes=1135424
    fragment_sha256=c6b199644be8ca19cce0110a5090e84c736220a130f9b48a4366caf36254e6e2
fi
service_root="$repository_root/Runtime/Windvale.Native/Consumers"
consumer_root="$repository_root/Linker/Reference/Consumers"
raw_lowerer="$repository_root/Artifacts/Native-Wvb-To-Wvo-Candidate/Wvb-To-Wvo.elf"

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
    3051a9c328c04a53dd0f0a54a8f83c7d1f12c3947df3bd19d7ad066ac3f09954 \
    'hosted toolset inventory' || exit $?
(cd -- "$hosted_toolset" && sha256sum --check --strict --quiet SHA256SUMS) || exit $?
check_file "$construction/SHA256SUMS" 5064 \
    d9a41516b7d5f768afe377fd957e897bcb1cd3552fdf4c9510af3fc6969a7edc \
    'publisher construction inventory' || exit $?
(cd -- "$construction" && sha256sum --check --strict --quiet SHA256SUMS) || exit $?
check_file "$publisher_wvb" "$publisher_wvb_bytes" "$publisher_wvb_sha256" \
    'publisher WVB' || exit $?
if [[ $role == wvo-publisher || $role == console-application-publisher ]]; then
    check_file "$raw_lowerer" 6205440 \
        8debddbbeefc325ba15aae700c77921bb077460067222b9100618d1ec6bec76f \
        'raw native WVB-to-WVO lowerer' || exit $?
fi
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

if [[ $role == wvo-publisher || $role == console-application-publisher ]]; then
    "$raw_lowerer" "$publisher_wvb" "$temporary_directory/Publisher.wvo" \
        >/dev/null || exit $?
else
    "$repository_root/Tools/Native/Lower-Wvb-To-Wvo.sh" \
        "$publisher_wvb" "$temporary_directory/Publisher.wvo" >/dev/null || exit $?
fi
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
elif [[ $role == wvb-publisher ]]; then
    printf 'WVB publisher construction status=Valid target=%s bytes=%s\n' \
        "$target_name" "$application_bytes"
elif [[ $role == wvo-publisher ]]; then
    printf 'WVO publisher construction status=Valid target=%s bytes=%s\n' \
        "$target_name" "$application_bytes"
else
    printf 'console-application publisher construction status=Valid target=%s bytes=%s\n' \
        "$target_name" "$application_bytes"
fi
exit 0
