#!/usr/bin/env bash
set -uo pipefail

image_mode=0
if [[ ($# -eq 3 || $# -eq 4) && $1 =~ ^[1-8]$ && $2 == *.wvb ]]; then
    profile=$1
    input_argument=$2
    output_argument=$3
    target=${4:-linux}
elif [[ ($# -eq 7 || $# -eq 8) && $1 == image && $2 =~ ^[1-8]$ &&
        $3 == *.wvb && $5 =~ ^([1-9]|1[0-6])$ && $6 =~ ^[0-9]+$ ]]; then
    image_mode=1
    profile=$2
    input_argument=$3
    external_bundle_sources=$4
    fragment_count=$5
    native_entry=$6
    output_argument=$7
    target=${8:-linux}
else
    echo 'Usage: ./Tools/Native/Package-Hosted-Wvb.sh <profile-1-through-8> <input.wvb> <output.elf|output.exe> [linux|windows]' >&2
    echo '   or: ./Tools/Native/Package-Hosted-Wvb.sh image <profile-1-through-8> <input.wvb> <chunk-prefix> <fragment-chunks-1-through-16> <entry-offset> <output.elf|output.exe> [linux|windows]' >&2
    exit 64
fi
case "$target:$output_argument" in
    linux:*.elf|windows:*.exe) ;;
    *)
        echo 'The hosted-container target and output extension do not agree.' >&2
        exit 64
        ;;
esac
script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
toolset="$repository_root/Artifacts/Native-Hosted-Container-Toolset-Candidate"
enum_request_candidate="$repository_root/Artifacts/Native-Hosted-Enum-Request-Candidate"
service_root="$repository_root/Runtime/Windvale.Native/Consumers"
case "$target" in
    linux)
        startup="$repository_root/Linker/Reference/Consumers/Linux-X64-Hosted-Compiler.wvo"
        startup_bytes=2454
        startup_sha256=1b8c08308d3f7320b741ae86022400ced6748352314b7f27954ec1c5a7345946
        console_service="$service_root/Native-X64-Linux-Console-Output-Service.bin"
        console_service_bytes=213
        console_service_sha256=c5ea073a24c46dd634b1a67a7e7041d476dbce856d058aa8adc2c4e680d3d226
        file_input_service="$service_root/Native-X64-Linux-File-Input-Service.bin"
        file_input_service_bytes=996
        file_input_service_sha256=cbd78340641fa02589d0d96b73d233a67f9404ab76c3df2b1346b2e31ca43701
        diagnostic_service="$service_root/Native-X64-Linux-Diagnostic-Output-Service.bin"
        diagnostic_service_bytes=213
        diagnostic_service_sha256=1c81018143fa9b708373eaceda62722ca40fb1e11b20808f765fe5ece33406fe
        file_output_service="$service_root/Native-X64-Linux-File-Output-Service.bin"
        file_output_service_bytes=823
        file_output_service_sha256=fc688f2a84936dc1082fcb5654667a8a60b0581bff29b1868d48ef2d4af77422
        ;;
    windows)
        startup="$repository_root/Linker/Reference/Consumers/Windows-X64-Hosted-Compiler.wvo"
        startup_bytes=4488
        startup_sha256=6e97c4e610919291423764332eee926223ea556ea4631347c7f88f2aa1f154d5
        console_service="$service_root/Native-X64-Windows-Console-Output-Service.bin"
        console_service_bytes=258
        console_service_sha256=10f3a500aca7f0236cdf9f6c20658591df88bc612e677264cdaa0bcef59a0a48
        file_input_service="$service_root/Native-X64-Windows-File-Input-Service.bin"
        file_input_service_bytes=1218
        file_input_service_sha256=3e2fd284d4991d0f713301514d3fbf6af8ec84af7bd7289698c08a41d434c52d
        diagnostic_service="$service_root/Native-X64-Windows-Diagnostic-Output-Service.bin"
        diagnostic_service_bytes=258
        diagnostic_service_sha256=1b4068c01b2050c3055c78eb82303c71b8488e8766f7b628fab10ffb23e5ffe2
        file_output_service="$service_root/Native-X64-Windows-File-Output-Service.bin"
        file_output_service_bytes=787
        file_output_service_sha256=a331248b12fc5830587f6fd8ddf06a546859b8f57366e205032aa2c37db48bb1
        ;;
esac
input_directory=$(CDPATH= cd -- "$(dirname -- "$input_argument")" && pwd -P) || exit 64
input="$input_directory/$(basename -- "$input_argument")"
output_directory=$(CDPATH= cd -- "$(dirname -- "$output_argument")" && pwd -P) || exit 64
output="$output_directory/$(basename -- "$output_argument")"

verify_file() {
    local path=$1
    local expected_bytes=$2
    local expected_sha256=$3
    local label=$4
    [[ -f $path ]] || { echo "Missing $label: $path" >&2; return 1; }
    local actual_bytes
    actual_bytes=$(wc -c < "$path") || return 1
    [[ $actual_bytes -eq $expected_bytes ]] || {
        echo "The $label byte length is invalid." >&2
        return 1
    }
    local digest_line actual_sha256
    digest_line=$(sha256sum -- "$path") || return 1
    actual_sha256=${digest_line%% *}
    [[ $actual_sha256 == "$expected_sha256" ]] || {
        echo "The $label digest is invalid: $path" >&2
        return 1
    }
}

verify_file "$toolset/SHA256SUMS" 6927 d8b23c4b371c4841b6386f64940166be57a81930a2987a541a7c04648ddb016a 'hosted toolset inventory' || exit 1
(cd -- "$toolset" && sha256sum --check --strict --quiet SHA256SUMS) || {
    echo 'The hosted toolset artifact inventory is invalid.' >&2
    exit 1
}
verify_file "$enum_request_candidate/Wvb/wvhostenumrequest.wvb" 82115 69a4ef3b33875e26f068e1545c60a0ae7bee60ac566869c05e55ad27c0aa9b36 'hosted enum-request WVB' || exit 1
verify_file "$enum_request_candidate/windows-x64/wvhostenumrequest.exe" 888832 adabe0902e164bcb68561796ef2d60d446399cd51e70e326daf366623365ced0 'hosted enum-request Windows application' || exit 1
verify_file "$enum_request_candidate/linux-x64/wvhostenumrequest.elf" 888832 06f6b9fe4812ec9f1c4c37fd47ae3153ac9b870ffb9b4173b2705c6517c586f8 'hosted enum-request Linux application' || exit 1
verify_file "$console_service" "$console_service_bytes" "$console_service_sha256" 'console service' || exit 1
verify_file "$service_root/Native-X64-Argument-Count-Service.bin" 5 2358e7e2c72d6476cfe05134db4f0eb5e6987fcca1b10894a8588a28d3929829 'argument-count service' || exit 1
verify_file "$service_root/Native-X64-Argument-Service.bin" 70 2253e1435f141df5b68f9f7e9e9aa0de448410c42dcf33ad76dcf131afea65d1 'argument service' || exit 1
verify_file "$file_input_service" "$file_input_service_bytes" "$file_input_service_sha256" 'file-input service' || exit 1
verify_file "$service_root/Native-X64-Utf8-Service.bin" 800 4c3d2e370d62c8d2f54a3c453f39b94cf46ddabd6db3c2f3d6b65f0713b68aaf 'UTF-8 service' || exit 1
verify_file "$diagnostic_service" "$diagnostic_service_bytes" "$diagnostic_service_sha256" 'diagnostic service' || exit 1
verify_file "$service_root/Native-X64-Text-Concat-Service.bin" 249 75c5588117e1f5f58a593a23aae6156a3a68a6302df5f50153b977bccbaaa3a0 'text-concat service' || exit 1
verify_file "$service_root/Native-X64-U32-Format-Service.bin" 191 b98f2d55e30bb7369e233f94e4ade5f3e8917a7730114446f1ebc81f353e1e43 'u32-format service' || exit 1
verify_file "$file_output_service" "$file_output_service_bytes" "$file_output_service_sha256" 'file-output service' || exit 1
verify_file "$startup" "$startup_bytes" "$startup_sha256" 'hosted startup object' || exit 1

temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-native-hosted-package.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-native-hosted-package.*)
            rm -f -- "$temporary_directory"/*
            rmdir -- "$temporary_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $temporary_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

bundle_sources="$temporary_directory/Bundle-Sources"
bundle_segments="$temporary_directory/Bundle-Segments"
application_sources="$temporary_directory/Application-Sources"
application_segments="$temporary_directory/Application-Segments"

if [[ $image_mode -eq 1 ]]; then
    bundle_sources=$external_bundle_sources
else
    "$script_directory/Lower-Wvb-To-Wvo.sh" "$input" "$temporary_directory/Input.wvo" >"$temporary_directory/Lower.txt" || exit $?
    "$script_directory/Link-Wvo.sh" 0 Main "$temporary_directory/Native.bin" "$temporary_directory/Input.wvo" >"$temporary_directory/Link.txt" || exit $?
    native_entry=$(sed -n 's/^entry name=Main address=//p' "$temporary_directory/Link.txt")
    case "$native_entry" in
        ''|*[!0-9]*) echo 'The native linker did not report one decimal Main address.' >&2; exit 1 ;;
    esac
    fragment_count=1
    cp -- "$temporary_directory/Native.bin" "$bundle_sources.chunk-0" || exit 1
fi

host="$toolset/linux-x64"
metadata_request_tool="$host/wvhostrequest.elf"
source_set_tool="$host/wvhostsources.elf"
"$host/wvhostfixedservices.elf" "$target" "$bundle_sources" "$fragment_count" \
    "$console_service" \
    "$service_root/Native-X64-Argument-Count-Service.bin" \
    "$service_root/Native-X64-Argument-Service.bin" \
    "$file_input_service" \
    "$service_root/Native-X64-Utf8-Service.bin" \
    "$diagnostic_service" \
    "$service_root/Native-X64-Text-Concat-Service.bin" \
    "$service_root/Native-X64-U32-Format-Service.bin" \
    "$file_output_service" || exit $?
"$enum_request_candidate/linux-x64/wvhostenumrequest.elf" "$input" "$temporary_directory/Enum.wveq" || exit $?
enum_source_index=$((fragment_count + 6))
"$host/wvhostenumservice.elf" "$temporary_directory/Enum.wveq" "$bundle_sources.chunk-$enum_source_index" || exit $?
"$host/wvhostsourcegeometry.elf" "$bundle_sources" "$fragment_count" "$temporary_directory/Bundle-Sources.wvsg" || exit $?
"$host/wvhostpublicationrequest.elf" "$temporary_directory/Bundle-Sources.wvsg" "$temporary_directory/Publication.wvpq" || exit $?
"$host/wvhostcontrol.elf" evidence "$temporary_directory/Bundle-Sources.wvsg" "$temporary_directory/Evidence.wvhs" || exit $?
"$host/wvhostcontrol.elf" metadata "$target" "$profile" "$native_entry" "$temporary_directory/Metadata-Input.wvmi" || exit $?
"$metadata_request_tool" "$temporary_directory/Metadata-Input.wvmi" "$temporary_directory/Publication.wvpq" "$temporary_directory/Evidence.wvhs" "$bundle_sources" "$temporary_directory/Metadata-Request.wvhq" || exit $?
"$host/wvhostmetadata.elf" "$temporary_directory/Metadata-Request.wvhq" "$temporary_directory/Metadata.wvhm" || exit $?
"$host/wvhostruntime.elf" "$temporary_directory/Metadata.wvhm" "$temporary_directory/Runtime.wvhr" || exit $?
publication_plan="$temporary_directory/Plan.wvcd"
if [[ $profile == 8 ]]; then
    publication_plan="$temporary_directory/Publication-Plan.wvcd"
    "$host/wvhostplan.elf" "$temporary_directory/Runtime.wvhr" "$temporary_directory/Plan.wvcd" "$publication_plan" || exit $?
else
    "$host/wvhostplan.elf" "$temporary_directory/Runtime.wvhr" "$temporary_directory/Plan.wvcd" || exit $?
fi
"$host/wvhostbytes.elf" "$temporary_directory/Plan.wvcd" "$temporary_directory/Platform.wvhb" || exit $?
"$host/wvhoststartup.elf" "$temporary_directory/Plan.wvcd" "$startup" "$temporary_directory/Startup.wvsd" || exit $?

"$host/wvhostbundlerequest.elf" "$temporary_directory/Publication.wvpq" "$temporary_directory/Bundle-Sources.wvsg" "$bundle_sources" count >"$temporary_directory/Bundle-Count.txt" || exit $?
bundle_count=$(sed -n 's/^hosted service-bundle request status=Valid segments=//p' "$temporary_directory/Bundle-Count.txt")
case "$bundle_count" in
    [1-9]|1[0-7]) ;;
    *) echo 'The native service-bundle count is invalid.' >&2; exit 1 ;;
esac
index=0
while [[ $index -lt $bundle_count ]]; do
    "$host/wvhostbundlerequest.elf" "$temporary_directory/Publication.wvpq" "$temporary_directory/Bundle-Sources.wvsg" "$bundle_sources" "$index" "$bundle_segments.request-$index" || exit $?
    "$host/wvhostbundle.elf" "$bundle_segments.request-$index" "$bundle_segments.response-$index" || exit $?
    index=$((index + 1))
done

echo 'hosted package step=application-sources status=Started'
"$source_set_tool" "$temporary_directory/Plan.wvcd" "$temporary_directory/Platform.wvhb" "$temporary_directory/Startup.wvsd" "$bundle_segments" "$temporary_directory/Runtime.wvhr" "$application_sources" "$temporary_directory/Application-Sources.wvsg" || exit $?
echo 'hosted package step=application-sources status=Complete'
echo 'hosted package step=application-segment-count status=Started'
"$host/wvhostsegmentrequest.elf" "$publication_plan" "$temporary_directory/Application-Sources.wvsg" "$application_sources" count >"$temporary_directory/Application-Count.txt" || exit $?
application_count=$(sed -n 's/^hosted container segment request status=Valid segments=//p' "$temporary_directory/Application-Count.txt")
case "$application_count" in
    ''|*[!0-9]*) echo 'The native application-segment count is invalid.' >&2; exit 1 ;;
esac
if [[ $application_count -lt 1 || $application_count -gt 31 ]]; then
    echo 'The native application-segment count is outside the bounded range.' >&2
    exit 1
fi
echo "hosted package step=application-segment-count status=Complete segments=$application_count"
index=0
while [[ $index -lt $application_count ]]; do
    echo "hosted package step=application-segment item=$index/$((application_count - 1)) status=Started"
    "$host/wvhostsegmentrequest.elf" "$publication_plan" "$temporary_directory/Application-Sources.wvsg" "$application_sources" "$index" "$application_segments.request-$index" || exit $?
    "$host/wvhostsegment.elf" "$application_segments.request-$index" "$application_segments.response-$index" || exit $?
    echo "hosted package step=application-segment item=$index/$((application_count - 1)) status=Complete"
    index=$((index + 1))
done
echo 'hosted package step=application-manifest status=Started'
"$host/wvhostsegmentmanifest.elf" "$publication_plan" "$application_segments" "$temporary_directory/Application-Segments.wvhm" || exit $?
echo 'hosted package step=application-manifest status=Complete'
echo 'hosted package step=publication status=Started'
"$host/wvhostpublish.elf" "$publication_plan" "$application_segments" "$temporary_directory/Application-Segments.wvhm" "$output" || exit $?
echo 'hosted package step=publication status=Complete'
