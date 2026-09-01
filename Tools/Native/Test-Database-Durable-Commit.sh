#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Database-Durable-Commit.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-database-durable-commit.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-database-durable-commit.*)
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
    digest_line=$(sha256sum -- "$path") || return 1
    actual_sha256=${digest_line%% *}
    [[ $actual_bytes -eq $expected_bytes ]] || {
        echo "The $label byte length differs: bytes=$actual_bytes expected=$expected_bytes sha256=$actual_sha256." >&2
        return 1
    }
    [[ $actual_sha256 == "$expected_sha256" ]] || {
        echo "The $label digest differs: sha256=$actual_sha256 expected=$expected_sha256." >&2
        return 1
    }
}

lowerer="$repository_root/Artifacts/Native-Wvb-To-Wvo-Candidate/Wvb-To-Wvo.elf"
first_wvb="$temporary_directory/Commit-First.wvb"
second_wvb="$temporary_directory/Commit-Second.wvb"
first_wvo="$temporary_directory/Commit-First.wvo"
second_wvo="$temporary_directory/Commit-Second.wvo"
image="$temporary_directory/Commit.bin"
image_prefix="$temporary_directory/Commit-Image"
map="$temporary_directory/Commit.map"
linux_application="$temporary_directory/Commit.elf"
windows_application="$temporary_directory/Commit.exe"

echo 'START native database durable commit phase=tools item=1/4 retained-tools=1'
verify_file "$lowerer" 9752576 \
    377675961465fbfa2b2038ed5cf301ef483907d642355a6b6ebf42d23fa29703 \
    'retained lowerer' || exit 1
echo 'PASS  native database durable commit phase=tools item=1/4'

echo 'START native database durable commit phase=compile item=2/4'
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Durable-Commit.wvproj" \
    "$first_wvb" >/dev/null || exit $?
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Durable-Commit.wvproj" \
    "$second_wvb" >/dev/null || exit $?
cmp -s -- "$first_wvb" "$second_wvb" || {
    echo 'The database-durable-commit WVB is not deterministic.' >&2
    exit 1
}
verify_file "$first_wvb" 107828 \
    479e631466733ae421d3477f61cedf1f716aa993cfecd7da560818a9d6dc4b60 \
    'database-durable-commit WVB' || exit $?

"$lowerer" "$first_wvb" "$first_wvo" >/dev/null || exit $?
"$lowerer" "$second_wvb" "$second_wvo" >/dev/null || exit $?
cmp -s -- "$first_wvo" "$second_wvo" || {
    echo 'The database-durable-commit WVO is not deterministic.' >&2
    exit 1
}
"$script_directory/Check-Wvo.sh" "$first_wvo" >/dev/null || exit $?
verify_file "$first_wvo" 2011950 \
    39eaa1823df0e4dfabda085eb3894d47b940a06a4d44a4f0d637aa08a5a4a4a5 \
    'database-durable-commit WVO' || exit $?
echo 'PASS  native database durable commit phase=compile item=2/4'

echo 'START native database durable commit phase=link item=3/4'
"$script_directory/Link-Wvo.sh" 0 Main "$image" "$first_wvo" >"$map" || exit $?
entry_offset=$(sed -n 's/^entry name=Main address=//p' "$map")
[[ $entry_offset == 151017 ]] || {
    echo "The database-durable-commit entry offset is $entry_offset, expected 151017." >&2
    exit 1
}
verify_file "$image" 2008436 \
    2f1182f785ad22e1011b0c76e1202b3fc436548c76d70d2be8fb5aa1f175e929 \
    'database-durable-commit image' || exit $?
cp -- "$image" "$image_prefix.chunk-0" || exit $?
echo 'PASS  native database durable commit phase=link item=3/4'

echo 'START native database durable commit phase=package-and-execute item=4/4'
"$script_directory/Package-Hosted-Wvb.sh" image 6 \
    "$first_wvb" "$image_prefix" 1 "$entry_offset" "$linux_application" linux \
    >/dev/null || exit $?
verify_file "$linux_application" 2031616 \
    6969a296c7d0819175b9a5b1dd4c64c5245d056be9d674b947f08d92f3ab0a5e \
    'database-durable-commit Linux application' || exit $?
for test_case in A B C D E F G H I J K L; do
    "$linux_application" "$test_case" >/dev/null
    application_result=$?
    if [[ $application_result -ne 42 ]]; then
        echo "The database-durable-commit case $test_case returned $application_result, expected 42." >&2
        exit 1
    fi
done

"$script_directory/Package-Hosted-Wvb.sh" image 6 \
    "$first_wvb" "$image_prefix" 1 "$entry_offset" "$windows_application" windows \
    >/dev/null || exit $?
verify_file "$windows_application" 2029568 \
    680d56c853b502b5bb76bffc3526752290da697eba707fa768ace644fb144b15 \
    'database-durable-commit Windows application' || exit $?
echo 'PASS  native database durable commit phase=package-and-execute item=4/4'

echo 'native database durable commit status=Passed cases=12 local-result=42 cross-host-images=Verified'
