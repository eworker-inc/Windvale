#!/usr/bin/env bash
set -uo pipefail

image_mode=0
if [[ $# -eq 3 && $1 =~ ^[1-7]$ && $2 == *.wvb && $3 == *.elf ]]; then
    profile=$1
    input_argument=$2
    output_argument=$3
elif [[ $# -eq 7 && $1 == image && $2 =~ ^[1-7]$ &&
        $3 == *.wvb && $5 =~ ^[1-8]$ && $6 =~ ^[0-9]+$ && $7 == *.elf ]]; then
    image_mode=1
    profile=$2
    input_argument=$3
    external_bundle_sources=$4
    fragment_count=$5
    native_entry=$6
    output_argument=$7
else
    echo 'Usage: ./Tools/Native/Package-Hosted-Wvb.sh <profile-1-through-7> <input.wvb> <output.elf>' >&2
    echo '   or: ./Tools/Native/Package-Hosted-Wvb.sh image <profile-1-through-7> <input.wvb> <chunk-prefix> <fragment-chunks-1-through-8> <entry-offset> <output.elf>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
toolset="$repository_root/Artifacts/Native-Hosted-Container-Toolset-Candidate"
service_root="$repository_root/Runtime/Windvale.Native/Consumers"
startup="$repository_root/Linker/Reference/Consumers/Linux-X64-Hosted-Compiler.wvo"
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

verify_file "$toolset/SHA256SUMS" 5426 6237a4131ab079ed03992e969375d8569f3c546bb415a50c25b19c982f516522 'hosted toolset inventory' || exit 1
(cd -- "$toolset" && sha256sum --check --strict --quiet SHA256SUMS) || {
    echo 'The hosted toolset artifact inventory is invalid.' >&2
    exit 1
}
verify_file "$service_root/Native-X64-Linux-Console-Output-Service.bin" 213 c5ea073a24c46dd634b1a67a7e7041d476dbce856d058aa8adc2c4e680d3d226 'console service' || exit 1
verify_file "$service_root/Native-X64-Argument-Count-Service.bin" 5 2358e7e2c72d6476cfe05134db4f0eb5e6987fcca1b10894a8588a28d3929829 'argument-count service' || exit 1
verify_file "$service_root/Native-X64-Argument-Service.bin" 70 2253e1435f141df5b68f9f7e9e9aa0de448410c42dcf33ad76dcf131afea65d1 'argument service' || exit 1
verify_file "$service_root/Native-X64-Linux-File-Input-Service.bin" 996 55ae4524c463f064aee0964d7f9b64438701fb4375a97c53d11f2f17902c12cb 'file-input service' || exit 1
verify_file "$service_root/Native-X64-Utf8-Service.bin" 800 4c3d2e370d62c8d2f54a3c453f39b94cf46ddabd6db3c2f3d6b65f0713b68aaf 'UTF-8 service' || exit 1
verify_file "$service_root/Native-X64-Linux-Diagnostic-Output-Service.bin" 213 1c81018143fa9b708373eaceda62722ca40fb1e11b20808f765fe5ece33406fe 'diagnostic service' || exit 1
verify_file "$service_root/Native-X64-Text-Concat-Service.bin" 249 75c5588117e1f5f58a593a23aae6156a3a68a6302df5f50153b977bccbaaa3a0 'text-concat service' || exit 1
verify_file "$service_root/Native-X64-U32-Format-Service.bin" 191 b98f2d55e30bb7369e233f94e4ade5f3e8917a7730114446f1ebc81f353e1e43 'u32-format service' || exit 1
verify_file "$service_root/Native-X64-Linux-File-Output-Service.bin" 823 fc688f2a84936dc1082fcb5654667a8a60b0581bff29b1868d48ef2d4af77422 'file-output service' || exit 1
verify_file "$startup" 2390 0df0525b35bbeb63492929d974326f328c247ce9313111ee6a8c1e321a2c22ff 'hosted startup object' || exit 1

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
"$host/wvhostfixedservices.elf" linux "$bundle_sources" "$fragment_count" \
    "$service_root/Native-X64-Linux-Console-Output-Service.bin" \
    "$service_root/Native-X64-Argument-Count-Service.bin" \
    "$service_root/Native-X64-Argument-Service.bin" \
    "$service_root/Native-X64-Linux-File-Input-Service.bin" \
    "$service_root/Native-X64-Utf8-Service.bin" \
    "$service_root/Native-X64-Linux-Diagnostic-Output-Service.bin" \
    "$service_root/Native-X64-Text-Concat-Service.bin" \
    "$service_root/Native-X64-U32-Format-Service.bin" \
    "$service_root/Native-X64-Linux-File-Output-Service.bin" || exit $?
"$host/wvhostenumrequest.elf" "$input" "$temporary_directory/Enum.wveq" || exit $?
"$host/wvhostenumservice.elf" "$temporary_directory/Enum.wveq" "$bundle_sources.chunk-7" || exit $?
"$host/wvhostsourcegeometry.elf" "$bundle_sources" "$fragment_count" "$temporary_directory/Bundle-Sources.wvsg" || exit $?
"$host/wvhostpublicationrequest.elf" "$temporary_directory/Bundle-Sources.wvsg" "$temporary_directory/Publication.wvpq" || exit $?
"$host/wvhostcontrol.elf" evidence "$temporary_directory/Bundle-Sources.wvsg" "$temporary_directory/Evidence.wvhs" || exit $?
"$host/wvhostcontrol.elf" metadata linux "$profile" "$native_entry" "$temporary_directory/Metadata-Input.wvmi" || exit $?
"$host/wvhostrequest.elf" "$temporary_directory/Metadata-Input.wvmi" "$temporary_directory/Publication.wvpq" "$temporary_directory/Evidence.wvhs" "$bundle_sources" "$temporary_directory/Metadata-Request.wvhq" || exit $?
"$host/wvhostmetadata.elf" "$temporary_directory/Metadata-Request.wvhq" "$temporary_directory/Metadata.wvhm" || exit $?
"$host/wvhostruntime.elf" "$temporary_directory/Metadata.wvhm" "$temporary_directory/Runtime.wvhr" || exit $?
"$host/wvhostplan.elf" "$temporary_directory/Runtime.wvhr" "$temporary_directory/Plan.wvcd" || exit $?
"$host/wvhostbytes.elf" "$temporary_directory/Plan.wvcd" "$temporary_directory/Platform.wvhb" || exit $?
"$host/wvhoststartup.elf" "$temporary_directory/Plan.wvcd" "$startup" "$temporary_directory/Startup.wvsd" || exit $?

"$host/wvhostbundlerequest.elf" "$temporary_directory/Publication.wvpq" "$temporary_directory/Bundle-Sources.wvsg" "$bundle_sources" count >"$temporary_directory/Bundle-Count.txt" || exit $?
bundle_count=$(sed -n 's/^hosted service-bundle request status=Valid segments=//p' "$temporary_directory/Bundle-Count.txt")
case "$bundle_count" in
    [1-9]) ;;
    *) echo 'The native service-bundle count is invalid.' >&2; exit 1 ;;
esac
index=0
while [[ $index -lt $bundle_count ]]; do
    "$host/wvhostbundlerequest.elf" "$temporary_directory/Publication.wvpq" "$temporary_directory/Bundle-Sources.wvsg" "$bundle_sources" "$index" "$bundle_segments.request-$index" || exit $?
    "$host/wvhostbundle.elf" "$bundle_segments.request-$index" "$bundle_segments.response-$index" || exit $?
    index=$((index + 1))
done

"$host/wvhostsources.elf" "$temporary_directory/Plan.wvcd" "$temporary_directory/Platform.wvhb" "$temporary_directory/Startup.wvsd" "$bundle_segments" "$temporary_directory/Runtime.wvhr" "$application_sources" "$temporary_directory/Application-Sources.wvsg" || exit $?
"$host/wvhostsegmentrequest.elf" "$temporary_directory/Plan.wvcd" "$temporary_directory/Application-Sources.wvsg" "$application_sources" count >"$temporary_directory/Application-Count.txt" || exit $?
application_count=$(sed -n 's/^hosted container segment request status=Valid segments=//p' "$temporary_directory/Application-Count.txt")
case "$application_count" in
    ''|*[!0-9]*) echo 'The native application-segment count is invalid.' >&2; exit 1 ;;
esac
if [[ $application_count -lt 1 || $application_count -gt 31 ]]; then
    echo 'The native application-segment count is outside the bounded range.' >&2
    exit 1
fi
index=0
while [[ $index -lt $application_count ]]; do
    "$host/wvhostsegmentrequest.elf" "$temporary_directory/Plan.wvcd" "$temporary_directory/Application-Sources.wvsg" "$application_sources" "$index" "$application_segments.request-$index" || exit $?
    "$host/wvhostsegment.elf" "$application_segments.request-$index" "$application_segments.response-$index" || exit $?
    index=$((index + 1))
done
"$host/wvhostsegmentmanifest.elf" "$temporary_directory/Plan.wvcd" "$application_segments" "$temporary_directory/Application-Segments.wvhm" || exit $?
"$host/wvhostpublish.elf" "$temporary_directory/Plan.wvcd" "$application_segments" "$temporary_directory/Application-Segments.wvhm" "$output"
