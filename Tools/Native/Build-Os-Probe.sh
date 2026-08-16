#!/usr/bin/env bash
set -uo pipefail

if [[ $# -lt 1 || $# -gt 2 || $1 != *.efi ]]; then
    echo 'Usage: ./Tools/Native/Build-Os-Probe.sh <output.efi> [normal|invalid-opcode|general-protection]' >&2
    exit 64
fi
scenario=${2:-normal}
case $scenario in
    normal)
        memory_role=memory
        memory_bytes=1529
        memory_digest=2668e17c3181e168415fb7bdee530873e2ddc8fa2d100af94bcc7b74909df3ed
        efi_digest=e9a113b0b108a9da0bf31a0802d1fa7ae58f4c1888a1e30a0eb7d090732d40d9
        efi_bytes=1696768
        code_tail_offset=791424
        ;;
    invalid-opcode)
        memory_role=memory-invalid-opcode
        memory_bytes=1545
        memory_digest=09aa0fcfe12c561b79367cb26569dbc6f1f47ca3b98dc892426ca57b4328f868
        efi_digest=af1cacbc0d139958e6f8d083d68493b35e4987a8a843939506e27f9595a133e2
        efi_bytes=1696768
        code_tail_offset=791440
        ;;
    general-protection)
        memory_role=memory-general-protection
        memory_bytes=1545
        memory_digest=23a052f9d47a9416618c9b7a50a382c68c46d3bf7834410cc79f8fef2aa461e0
        efi_digest=eea4961a1a4b2287737ccd238f088b53ae4274714bf2c88f5a2ef0f7c4bdb384
        efi_bytes=1696768
        code_tail_offset=791440
        ;;
    *)
        echo 'Usage: ./Tools/Native/Build-Os-Probe.sh <output.efi> [normal|invalid-opcode|general-protection]' >&2
        exit 64
        ;;
esac

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
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

if ! "$script_directory/Produce-Os-Probe-Object.sh" loader \
    "$work/00-loader.wvo" >"$work/00.log" 2>&1; then
    cat -- "$work/00.log" >&2
    exit 1
fi
if [[ $(wc -c < "$work/00-loader.wvo") -ne 6336 ]] ||
    ! printf '%s  %s\n' \
        'b310bc0e9aebc7b14c0892bb3dd4b833d42539c2194427a8f333b511d6af3804' \
        "$work/00-loader.wvo" | sha256sum --check --strict --quiet; then
    echo 'The native Probe 40 loader object is invalid.' >&2
    exit 1
fi

if ! "$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Operating-System/Windvale-Os-Kernel-Markers.wvproj" \
    "$work/01-kernel.wvb" >"$work/01-build.log" 2>&1; then
    cat -- "$work/01-build.log" >&2
    exit 1
fi
if [[ $(wc -c < "$work/01-kernel.wvb") -ne 1581 ]] ||
    ! printf '%s  %s\n' \
        '795734982cded8b3605cb5cf0f110667b71140d5639185c3ef94cde3174b3bc0' \
        "$work/01-kernel.wvb" | sha256sum --check --strict --quiet; then
    echo 'The native Probe 40 kernel module is invalid.' >&2
    exit 1
fi
if ! "$script_directory/Lower-Os-Kernel-Wvb.sh" \
    "$work/01-kernel.wvb" \
    "$work/01-kernel.wvo" >"$work/01-lower.log" 2>&1; then
    cat -- "$work/01-lower.log" >&2
    exit 1
fi
if [[ $(wc -c < "$work/01-kernel.wvo") -ne 13454 ]] ||
    ! printf '%s  %s\n' \
        '4bf896ac2b349d9e786bbb7cae0165cb47273aa82ff2985a7ff33c3185978e8b' \
        "$work/01-kernel.wvo" | sha256sum --check --strict --quiet; then
    echo 'The native Probe 40 kernel object is invalid.' >&2
    exit 1
fi

if ! "$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Operating-System/Windvale-Os-Wvb-Admission.wvproj" \
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
    "$repository_root/Projects/Operating-System/Windvale-Os-Native-Wvb-Probe.wvproj" \
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

if ! "$script_directory/Build-Os-Process-Policy-Object.sh" \
    "$work/04-process-policy.wvo" >"$work/04.log" 2>&1; then
    cat -- "$work/04.log" >&2
    exit 1
fi
if [[ $(wc -c < "$work/04-process-policy.wvo") -ne 699394 ]] ||
    ! printf '%s  %s\n' \
        'dea015f8cafac002eddb9383691e2de10cbdcd0c0a589a88d88fbef95241f5b5' \
        "$work/04-process-policy.wvo" | sha256sum --check --strict --quiet; then
    echo 'The native Probe 40 process-policy object is invalid.' >&2
    exit 1
fi

if ! "$script_directory/Build-Os-Process-Object.sh" \
    "$work/05-process.wvo" >"$work/05.log" 2>&1; then
    cat -- "$work/05.log" >&2
    exit 1
fi
if [[ $(wc -c < "$work/05-process.wvo") -ne 956230 ]] ||
    ! printf '%s  %s\n' \
        '6c54a37dbe4e08d43068fed9bfb98edea536ae097666fa2c793a1c1bea9f9ac3' \
        "$work/05-process.wvo" | sha256sum --check --strict --quiet; then
    echo 'The native Probe 40 process object is invalid.' >&2
    exit 1
fi

if ! "$script_directory/Assemble-Wva.sh" \
    "$repository_root/Operating-System/Kernel/X64-Application-Start-Syscall-Context.wva" \
    "$work/14-application-start-context.wvo" >"$work/14.log" 2>&1; then
    cat -- "$work/14.log" >&2
    exit 1
fi
if ! "$script_directory/Assemble-Wva.sh" \
    "$repository_root/Operating-System/Kernel/X64-Application-Start-User-Copy.wva" \
    "$work/15-application-start-copy.wvo" >"$work/15.log" 2>&1; then
    cat -- "$work/15.log" >&2
    exit 1
fi
if ! "$script_directory/Assemble-Wva.sh" \
    "$repository_root/Operating-System/Kernel/X64-Application-Start-Publication.wva" \
    "$work/16-application-start-publication.wvo" >"$work/16.log" 2>&1; then
    cat -- "$work/16.log" >&2
    exit 1
fi
if ! printf '%s  %s\n%s  %s\n%s  %s\n' \
    'd639056eb9831f89ef3baa33b06b522437d2da4444f74e2db1d58229656dc04b' "$work/14-application-start-context.wvo" \
    '74978b1f6124517b44205cba52aaf6c161cf5d00e39ff9ab3ad883d527c87ddb' "$work/15-application-start-copy.wvo" \
    '0c73a88e301ce3fd321da2369661def3ae7c5e644e6a1ac0498fee6e7ad3d37a' "$work/16-application-start-publication.wvo" |
    sha256sum --check --strict --quiet; then
    echo 'A native Probe 40 application-start object is invalid.' >&2
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
if ! "$script_directory/Produce-Os-Probe-Object.sh" "$memory_role" \
    "$work/08-memory.wvo" >"$work/08.log" 2>&1; then
    cat -- "$work/08.log" >&2
    exit 1
fi
if ! "$script_directory/Produce-Os-Probe-Object.sh" exceptions \
    "$work/09-exceptions.wvo" >"$work/09.log" 2>&1; then
    cat -- "$work/09.log" >&2
    exit 1
fi
if ! "$script_directory/Produce-Os-Probe-Object.sh" paging \
    "$work/10-paging.wvo" >"$work/10.log" 2>&1; then
    cat -- "$work/10.log" >&2
    exit 1
fi
if ! "$script_directory/Produce-Os-Probe-Object.sh" wvb-admission-bridge \
    "$work/12-wvb-admission-bridge.wvo" >"$work/12.log" 2>&1; then
    cat -- "$work/12.log" >&2
    exit 1
fi
if ! "$script_directory/Produce-Os-Probe-Object.sh" native-bridge-and-support \
    "$work/13-native-bridge-and-support.wvo" >"$work/13.log" 2>&1; then
    cat -- "$work/13.log" >&2
    exit 1
fi
if ! "$script_directory/Assemble-Wva.sh" \
    "$repository_root/Operating-System/Kernel/X64-Kernel-Shims.wva" \
    "$work/11-kernel-shims.wvo" >"$work/11.log" 2>&1; then
    cat -- "$work/11.log" >&2
    exit 1
fi
if ! printf '%s  %s\n%s  %s\n%s  %s\n%s  %s\n%s  %s\n%s  %s\n%s  %s\n%s  %s\n' \
    'fe0a94461b743be58319d2e2f8b737840ec1216e61a98ee7e210f96f97f85bee' "$work/06-memory-object-shims.wvo" \
    'e331a1db404b8b8359d35d410792496683a63acee621ff64f128a6eae128c344' "$work/07-timer-shims.wvo" \
    "$memory_digest" "$work/08-memory.wvo" \
    '9caeb7ce353bca33e3bbac729ecca0423d59f8ce6b65ccd6b54fa53c381d617c' "$work/09-exceptions.wvo" \
    'a76c4a199d46f6d91c0d3cd76aec7439a5e3fa72403cfc753fa6c36cd5b9b871' "$work/10-paging.wvo" \
    '271c378b1f12bb4affa33474d865611cbf14e5b1b8996c703cb3d3cbe22eee7d' "$work/12-wvb-admission-bridge.wvo" \
    '472a0fbe6497525e634a4785e92aa9ee62c3c7d70fff7510e45acbea644eea0b' "$work/13-native-bridge-and-support.wvo" \
    '845d45d6787ec819ca300ffc81a9ffe3e86c7b3998f3dd2a50a017a353d86193' "$work/11-kernel-shims.wvo" |
    sha256sum --check --strict --quiet; then
    echo 'A native Probe 40 top-level WVA object is invalid.' >&2
    exit 1
fi
if [[ $(wc -c < "$work/08-memory.wvo") -ne $memory_bytes ]]; then
    echo 'The native Probe 40 memory object has an invalid length.' >&2
    exit 1
fi

if ! "$script_directory/Link-Wvo.sh" 0 Windvale_boot_probe "$work/Probe40.bin" \
    "$work/00-loader.wvo" \
    "$work/01-kernel.wvo" \
    "$work/02-wvb-admission-native.wvo" \
    "$work/03-native-wvb-probe.wvo" \
    "$work/04-process-policy.wvo" \
    "$work/05-process.wvo" \
    "$work/14-application-start-context.wvo" \
    "$work/15-application-start-copy.wvo" \
    "$work/16-application-start-publication.wvo" \
    "$work/06-memory-object-shims.wvo" \
    "$work/07-timer-shims.wvo" \
    "$work/08-memory.wvo" \
    "$work/09-exceptions.wvo" \
    "$work/10-paging.wvo" \
    "$work/11-kernel-shims.wvo" \
    "$work/12-wvb-admission-bridge.wvo" \
    "$work/13-native-bridge-and-support.wvo" >"$work/Link.map" 2>&1; then
    cat -- "$work/Link.map" >&2
    exit 1
fi
if ! grep -Fxq 'entry name=Windvale_boot_probe address=0' "$work/Link.map"; then
    echo 'The native Probe 40 linker reported an unexpected entry.' >&2
    exit 1
fi
if ! grep -Fxq "section index=18 input=16 source-index=1 kind=code name=.text.support image-offset=$code_tail_offset address=$code_tail_offset memory-bytes=23 data-bytes=23 alignment=16" "$work/Link.map"; then
    echo 'The native Probe 40 linker reported an unexpected supervisor code boundary.' >&2
    exit 1
fi

if ! "$script_directory/Package-Uefi.sh" \
    "$work/Probe40.bin" 0 "$work/Probe40.efi" >"$work/Package.log" 2>&1; then
    cat -- "$work/Package.log" >&2
    exit 1
fi
if [[ $(wc -c < "$work/Probe40.efi") -ne $efi_bytes ]] ||
    ! printf '%s  %s\n' \
        "$efi_digest" \
        "$work/Probe40.efi" | sha256sum --check --strict --quiet; then
    echo 'The native Probe 40 EFI candidate is invalid.' >&2
    printf 'Probe40.efi bytes=%s\n' "$(wc -c < "$work/Probe40.efi")" >&2
    sha256sum -- "$work/Probe40.efi" >&2
    exit 1
fi

if ! mv -- "$work/Probe40.efi" "$output"; then
    echo 'The native Probe 40 EFI could not be published.' >&2
    exit 1
fi
printf '%s\n' \
    'windvale-os-probe-native-build 40' \
    "scenario=$scenario" \
    "efi-bytes=$efi_bytes" \
    "efi-sha256=$efi_digest" \
    "output=$output"
