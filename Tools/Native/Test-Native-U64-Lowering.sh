#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Native-U64-Lowering.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-native-u64-lowering.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-native-u64-lowering.*)
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

verify_file() {
    local path=$1 expected_bytes=$2 expected_sha256=$3 label=$4
    [[ -f $path ]] || { echo "Missing $label: $path" >&2; return 1; }
    local actual_bytes digest_line actual_sha256
    actual_bytes=$(wc -c < "$path") || return 1
    [[ $actual_bytes -eq $expected_bytes ]] || {
        echo "The $label byte length differs." >&2
        return 1
    }
    digest_line=$(sha256sum -- "$path") || return 1
    actual_sha256=${digest_line%% *}
    [[ $actual_sha256 == "$expected_sha256" ]] || {
        echo "The $label digest differs." >&2
        return 1
    }
}

lowerer="$repository_root/Artifacts/Native-Wvb-To-Wvo-Candidate/Wvb-To-Wvo.elf"
target_wvb="$temporary_directory/Target.wvb"
target_wvo="$temporary_directory/Target.wvo"
image="$temporary_directory/Target.bin"
linux_application="$temporary_directory/Target.elf"
windows_application="$temporary_directory/Target.exe"
page_wvb="$temporary_directory/Page.wvb"
page_wvo="$temporary_directory/Page.wvo"
page_image="$temporary_directory/Page.bin"
page_image_prefix="$temporary_directory/Page-Image"
page_linux_application="$temporary_directory/Page.elf"
page_windows_application="$temporary_directory/Page.exe"
page_fixture="$repository_root/Tests/Fixtures/Database/Native-Hosted-Snapshot-Page.txt"

verify_file "$lowerer" 10076160 \
    9eb1ac6a547657a18e68b920b5e8523ae465de556a6f412f652680ccb9dd2d37 \
    'native lowerer candidate' || exit $?

"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tests/Windvale-Native-Test-Wvb-To-Wvo-U64.wvproj" \
    "$target_wvb" >/dev/null || exit $?
verify_file "$target_wvb" 2103 \
    754862810b90e638755edf253c4b88b045bca44c2b3b58d5d76d48eba35dfc2f \
    'u64 WVB' || exit $?

"$lowerer" "$target_wvb" "$target_wvo" >/dev/null || exit $?
"$script_directory/Check-Wvo.sh" "$target_wvo" >/dev/null || exit $?
verify_file "$target_wvo" 16178 \
    29158614e7f23ede1b6a3fdab8e97cff64c4f390cb576834dd573a7255bd88da \
    'u64 WVO' || exit $?

"$script_directory/Link-Wvo.sh" 0 Main "$image" "$target_wvo" >/dev/null || exit $?
verify_file "$image" 15960 \
    fc425d7b173cc97f97c4782647c74cd7d923e888c35b6a8f38218010587f4517 \
    'u64 flat image' || exit $?

"$script_directory/Package-Console.sh" linux-x64-console-v1 \
    "$image" 0 "$linux_application" >/dev/null || exit $?
verify_file "$linux_application" 20592 \
    9ce2307a029d3d50a56d11432b2c9d8813f756fa23e990e9814cf1692463ab66 \
    'u64 Linux application' || exit $?
"$linux_application" >/dev/null
application_result=$?
if [[ $application_result -ne 42 ]]; then
    echo "The u64 Linux application result is $application_result, expected 42." >&2
    exit 1
fi

"$script_directory/Package-Console.sh" windows-x64-console-v1 \
    "$image" 0 "$windows_application" >/dev/null || exit $?
verify_file "$windows_application" 17920 \
    774173b5499d3802d080da8c7e6f40a683ab50c022f782c305751af4cefc8a04 \
    'u64 Windows application' || exit $?

"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tests/Windvale-Native-Test-Native-Hosted-Snapshot-Page.wvproj" \
    "$page_wvb" >/dev/null || exit $?
verify_file "$page_wvb" 5386 \
    22a8b4a44a73b1cfbfdf7ba19ded9e5c921e6870fd4afd2f76a982c555805c00 \
    'native database-page WVB' || exit $?
verify_file "$page_fixture" 17 \
    4897fe28a3fa1ded2c3e9f79192b23671d1fe1e39c10f71ed94703d317886f73 \
    'native database-page fixture' || exit $?

"$lowerer" "$page_wvb" "$page_wvo" >/dev/null || exit $?
"$script_directory/Check-Wvo.sh" "$page_wvo" >/dev/null || exit $?
verify_file "$page_wvo" 74228 \
    1f652d116e9cd59f1e033831fc6b8c227d23c91a19a4b3e027e1fabb35880558 \
    'native database-page WVO' || exit $?
"$script_directory/Link-Wvo.sh" 0 Main "$page_image" "$page_wvo" >/dev/null || exit $?
verify_file "$page_image" 73888 \
    2792d693240b36122c0f9d2c706a80985a366bf61316bb50751ebd997f9b7d15 \
    'native database-page image' || exit $?
cp -- "$page_image" "$page_image_prefix.chunk-0" || exit $?

"$script_directory/Package-Hosted-Wvb.sh" image 6 \
    "$page_wvb" "$page_image_prefix" 1 0 "$page_linux_application" linux \
    >/dev/null || exit $?
verify_file "$page_linux_application" 94208 \
    36ed10422a46c6eb43a1435472d89476a5c0aea5029079321392e4917993067b \
    'native database-page Linux application' || exit $?
"$page_linux_application" "$page_fixture" >/dev/null
page_result=$?
if [[ $page_result -ne 42 ]]; then
    echo "The native database-page Linux result is $page_result, expected 42." >&2
    exit 1
fi

"$script_directory/Package-Hosted-Wvb.sh" image 6 \
    "$page_wvb" "$page_image_prefix" 1 0 "$page_windows_application" windows \
    >/dev/null || exit $?
verify_file "$page_windows_application" 92160 \
    4b51c69313be614d7cae3534cc6fad2a78848814838758914edb64986fb6ecb6 \
    'native database-page Windows application' || exit $?

echo 'native u64 lowering status=Passed local-result=42 database-page=42 cross-host-images=Verified'
