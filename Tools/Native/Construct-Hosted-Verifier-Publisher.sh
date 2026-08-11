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
            base_sha256=17f4939071697344c5252478198713dab08ebc9c8fe476687fd758bad59f4d4e
            application_bytes=681472
            application_sha256=86c72f5485bd6eeba1bdb65841102d7f388a8714b8e07ca3d519250de2886d8b
        fi
        if [[ $role == wvb-publisher ]]; then
            base_bytes=1333760
            base_sha256=8fcdcfc755439ebae5086c72d88113fb52f397ba0687c785af247230a7732fff
            application_bytes=1340928
            application_sha256=71794a6a254ccfd652ffe3bad556c32f86e2d9210a5a3099bad576f97476a8f3
        fi
        if [[ $role == wvo-publisher ]]; then
            base_bytes=422912
            base_sha256=1f9361126c368f133693222cbaa4c21e2d0948e79df7bf945b7b037ac815e884
            application_bytes=430080
            application_sha256=76f632ffa7998a6cce0386456fee98f02cbb5ec424d0d914a7e1f06ff3853910
        fi
        if [[ $role == console-application-publisher ]]; then
            base_bytes=1151488
            base_sha256=922c9019308e837f6a3528c3b1edf6cd83b3e432bdb6a140111c958aa6ff5e97
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
        base_sha256=687338281ca78c9d3a4d08b601c1efbcc198ec3c8fcc96fbf34f5dc349cafae2
        application_bytes=254965
        application_sha256=510f5ce5d2a494eacf0adc7a613581bc2371c4ad0f5f985f501381edc1632fac
        if [[ $role == promoter ]]; then
            base_bytes=675840
            base_sha256=18fbe415177f5f96644ecca8bf5e38aa6c42e62108e9b4df794752dd5820ddc5
            application_bytes=680949
            application_sha256=700f3df624611abad03cbd70811bad2ab015136ecdacc6dff9cdd97f5fc81395
        fi
        if [[ $role == wvb-publisher ]]; then
            base_bytes=1335296
            base_sha256=f53a4c8c5d292e999735cf5fd337b7c6997c0a8e6d2ba316ec94cd6b0838b090
            application_bytes=1340405
            application_sha256=7024fc5f96181f819e01bc41bc5c34d9eaed4301ea459c0c2bc43b7f52b21095
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
        publisher_wvb_sha256=086bd4d93d93d51b0f9140a0adf9f54a7f205dc902d9cb5d732dc7a887e10edc
    publisher_object_bytes=660123
        publisher_object_sha256=ee5274c86d680640d3ab75754faf63585a639a44fc9626ea5b9f9bcce9779e8e
    native_entry=1178
    fragment_bytes=658339
        fragment_sha256=d50dc45866818c36a0332af71e914dc9a05052d97f43c0f60add4a75101bbec6
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
    430171a9157560acb57e6f84aa772429b436059867892ee2408839057e0eeebc \
    'hosted toolset inventory' || exit $?
(cd -- "$hosted_toolset" && sha256sum --check --strict --quiet SHA256SUMS) || exit $?
check_file "$construction/SHA256SUMS" 5064 \
    ac41be9f59a7db47f721e0c0485cfe7e10cfc888e902f67e91a3c1c6330b68eb \
    'publisher construction inventory' || exit $?
(cd -- "$construction" && sha256sum --check --strict --quiet SHA256SUMS) || exit $?
check_file "$publisher_wvb" "$publisher_wvb_bytes" "$publisher_wvb_sha256" \
    'publisher WVB' || exit $?
if [[ $role == wvo-publisher || $role == console-application-publisher ]]; then
    check_file "$raw_lowerer" 5996544 \
        55df0cd2a679491bc1a3da50b6bc67b5f512f2a4698721faea6becb556a2a46f \
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
