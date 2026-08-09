#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 1 || $1 != *.efi ]]; then
    echo 'Usage: ./Tools/Native/Build-Os-Probe.sh <output.efi>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
object_root="$repository_root/Artifacts/Native-Os-Probe-40-Object-Candidate"
output_directory=$(dirname -- "$1")
output_name=$(basename -- "$1")
if [[ ! -d $output_directory ]]; then
    echo 'The native Probe 40 output directory does not exist.' >&2
    exit 1
fi
output_directory=$(CDPATH= cd -- "$output_directory" && pwd -P)
output="$output_directory/$output_name"
if [[ -e $output ]]; then
    echo 'The native Probe 40 output already exists.' >&2
    exit 1
fi

if ! (cd -- "$object_root" && sha256sum --check --strict --quiet SHA256SUMS); then
    echo 'The native Probe 40 object candidate is invalid.' >&2
    exit 1
fi

work=$(mktemp -d "$output_directory/.windvale-os-probe-native.XXXXXX") || exit 1
case "$work" in
    "$output_directory"/.windvale-os-probe-native.*) ;;
    *)
        echo 'The native Probe 40 private path is outside the output directory.' >&2
        exit 1
        ;;
esac
cleanup() {
    rm -rf -- "$work"
}
trap cleanup EXIT

if ! "$script_directory/Build-Wvb.sh" \
    "$repository_root/Windvale-Os-Wvb-Admission.wvproj" \
    "$work/02-wvb-admission.wvb" >"$work/02-build.log" 2>&1; then
    cat -- "$work/02-build.log" >&2
    exit 1
fi
if [[ $(wc -c < "$work/02-wvb-admission.wvb") -ne 4071 ]] ||
    ! printf '%s  %s\n' \
        '69727bb8151aea164690be4f69adcda481532b965d9ae02ec92db21087f3d669' \
        "$work/02-wvb-admission.wvb" | sha256sum --check --strict --quiet; then
    echo 'The native Probe 40 admission module is invalid.' >&2
    exit 1
fi
if ! "$script_directory/Lower-Wvb-To-Wvo.sh" \
    "$work/02-wvb-admission.wvb" \
    "$work/02-unrenamed.wvo" >"$work/02-lower.log" 2>&1; then
    cat -- "$work/02-lower.log" >&2
    exit 1
fi
if [[ $(wc -c < "$work/02-unrenamed.wvo") -ne 20316 ]] ||
    ! printf '%s  %s\n' \
        '676a91062e7f1b4483ca9f332b17614a6b75988d21f9ff99caabcbfd51839568' \
        "$work/02-unrenamed.wvo" | sha256sum --check --strict --quiet; then
    echo 'The native Probe 40 unrenamed admission object is invalid.' >&2
    exit 1
fi
if ! "$script_directory/Rename-Wvo-Export.sh" \
    "$work/02-unrenamed.wvo" Main Windvale_kernel_wvb_admit \
    "$work/02-wvb-admission-native.wvo" >"$work/02-rename.log" 2>&1; then
    cat -- "$work/02-rename.log" >&2
    exit 1
fi
if [[ $(wc -c < "$work/02-wvb-admission-native.wvo") -ne 20337 ]] ||
    ! printf '%s  %s\n' \
        '37e47bd2fed0242ad5cae9c9cc684927dc17041d4cd1d154658616be8b140c32' \
        "$work/02-wvb-admission-native.wvo" | sha256sum --check --strict --quiet; then
    echo 'The native Probe 40 renamed admission object is invalid.' >&2
    exit 1
fi

if ! "$script_directory/Build-Wvb.sh" \
    "$repository_root/Windvale-Os-Native-Wvb-Probe.wvproj" \
    "$work/03-native-wvb-probe.wvb" >"$work/03-build.log" 2>&1; then
    cat -- "$work/03-build.log" >&2
    exit 1
fi
if [[ $(wc -c < "$work/03-native-wvb-probe.wvb") -ne 930 ]] ||
    ! printf '%s  %s\n' \
        'af5f93c881f006be06565f15857efb72b201b8f694a6c7e40a90deeaa86cd2c2' \
        "$work/03-native-wvb-probe.wvb" | sha256sum --check --strict --quiet; then
    echo 'The native Probe 40 source module is invalid.' >&2
    exit 1
fi
if ! "$script_directory/Lower-Wvb-To-Wvo.sh" \
    "$work/03-native-wvb-probe.wvb" \
    "$work/03-native-wvb-probe.wvo" >"$work/03-lower.log" 2>&1; then
    cat -- "$work/03-lower.log" >&2
    exit 1
fi
if [[ $(wc -c < "$work/03-native-wvb-probe.wvo") -ne 7306 ]] ||
    ! printf '%s  %s\n' \
        '046f4fa32293b4f02bdc51a3ec71d562d7a064b31056ca77a43e2083b281cd2c' \
        "$work/03-native-wvb-probe.wvo" | sha256sum --check --strict --quiet; then
    echo 'The native Probe 40 lowered object is invalid.' >&2
    exit 1
fi

if ! "$script_directory/Assemble-Wva.sh" \
    "$repository_root/Operating-System/Kernel/X64-Memory-Object-Shims.wva" \
    "$work/06-memory-object-shims.wvo" >"$work/06.log" 2>&1; then
    cat -- "$work/06.log" >&2
    exit 1
fi
if ! "$script_directory/Assemble-Wva.sh" \
    "$repository_root/Operating-System/Kernel/X64-Timer-Shims.wva" \
    "$work/07-timer-shims.wvo" >"$work/07.log" 2>&1; then
    cat -- "$work/07.log" >&2
    exit 1
fi
if ! "$script_directory/Produce-Os-Probe-Object.sh" exceptions \
    "$work/09-exceptions.wvo" >"$work/09.log" 2>&1; then
    cat -- "$work/09.log" >&2
    exit 1
fi
if ! "$script_directory/Produce-Os-Probe-Object.sh" wvb-admission-bridge \
    "$work/12-wvb-admission-bridge.wvo" >"$work/12.log" 2>&1; then
    cat -- "$work/12.log" >&2
    exit 1
fi
if ! "$script_directory/Assemble-Wva.sh" \
    "$repository_root/Operating-System/Kernel/X64-Kernel-Shims.wva" \
    "$work/11-kernel-shims.wvo" >"$work/11.log" 2>&1; then
    cat -- "$work/11.log" >&2
    exit 1
fi
if ! printf '%s  %s\n%s  %s\n%s  %s\n%s  %s\n%s  %s\n' \
    'fe0a94461b743be58319d2e2f8b737840ec1216e61a98ee7e210f96f97f85bee' "$work/06-memory-object-shims.wvo" \
    'e331a1db404b8b8359d35d410792496683a63acee621ff64f128a6eae128c344' "$work/07-timer-shims.wvo" \
    '9caeb7ce353bca33e3bbac729ecca0423d59f8ce6b65ccd6b54fa53c381d617c' "$work/09-exceptions.wvo" \
    '271c378b1f12bb4affa33474d865611cbf14e5b1b8996c703cb3d3cbe22eee7d' "$work/12-wvb-admission-bridge.wvo" \
    '845d45d6787ec819ca300ffc81a9ffe3e86c7b3998f3dd2a50a017a353d86193' "$work/11-kernel-shims.wvo" |
    sha256sum --check --strict --quiet; then
    echo 'A native Probe 40 top-level WVA object is invalid.' >&2
    exit 1
fi

if ! "$script_directory/Link-Wvo.sh" 0 Windvale_boot_probe "$work/Probe40.bin" \
    "$object_root/00-loader.wvo" \
    "$object_root/01-kernel.wvo" \
    "$work/02-wvb-admission-native.wvo" \
    "$work/03-native-wvb-probe.wvo" \
    "$object_root/04-process-policy.wvo" \
    "$object_root/05-process.wvo" \
    "$work/06-memory-object-shims.wvo" \
    "$work/07-timer-shims.wvo" \
    "$object_root/08-memory.wvo" \
    "$work/09-exceptions.wvo" \
    "$object_root/10-paging.wvo" \
    "$work/11-kernel-shims.wvo" \
    "$work/12-wvb-admission-bridge.wvo" \
    "$object_root/13-native-bridge-and-support.wvo" >"$work/Link.map" 2>&1; then
    cat -- "$work/Link.map" >&2
    exit 1
fi
if ! grep -Fxq 'entry name=Windvale_boot_probe address=0' "$work/Link.map"; then
    echo 'The native Probe 40 linker reported an unexpected entry.' >&2
    exit 1
fi

if ! "$script_directory/Package-Uefi.sh" \
    "$work/Probe40.bin" 0 "$work/Probe40.efi" >"$work/Package.log" 2>&1; then
    cat -- "$work/Package.log" >&2
    exit 1
fi
if [[ $(wc -c < "$work/Probe40.efi") -ne 683008 ]] ||
    ! printf '%s  %s\n' \
        '080b4d669e9a11fdc802bf7197ae5a044978b6ba39741b2b1c832296987f74d9' \
        "$work/Probe40.efi" | sha256sum --check --strict --quiet; then
    echo 'The native Probe 40 EFI candidate is invalid.' >&2
    exit 1
fi

if ! mv -- "$work/Probe40.efi" "$output"; then
    echo 'The native Probe 40 EFI could not be published.' >&2
    exit 1
fi
printf '%s\n' \
    'windvale-os-probe-native-build 40' \
    'scenario=normal' \
    'efi-bytes=683008' \
    'efi-sha256=080b4d669e9a11fdc802bf7197ae5a044978b6ba39741b2b1c832296987f74d9' \
    "output=$output"
