#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Package-Bundle.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
work=$(mktemp -d "$temporary_root/windvale-package-bundle.XXXXXXXX") || exit 1
cleanup() {
    case "$work" in
        "$temporary_root"/windvale-package-bundle.*)
            rm -rf -- "$work"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $work" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

verify_file() {
    local path=$1
    local expected_bytes=$2
    local expected_sha256=$3
    local description=$4
    [[ -f "$path" ]] || { echo "Missing $description: $path" >&2; return 1; }
    local observed_bytes
    observed_bytes=$(wc -c <"$path" | tr -d '[:space:]') || return 1
    [[ "$observed_bytes" == "$expected_bytes" ]] || {
        echo "Invalid byte length for $description." >&2
        return 1
    }
    local observed_sha256
    observed_sha256=$(sha256sum -- "$path" | cut -d ' ' -f 1) || return 1
    [[ "$observed_sha256" == "$expected_sha256" ]] || {
        echo "Invalid SHA-256 for $description." >&2
        return 1
    }
}

echo 'native package bundle step=build-tools item=1/7'
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tests/Windvale-Native-Test-Package-Bundle.wvproj" \
    "$work/Self-Test.wvb" || exit $?
verify_file "$work/Self-Test.wvb" 312949 \
    5bff1f4aeb5c535396acd2b58e89ad39a01299f2acb5ae3e13ef31730745dbd1 \
    'bundle self-test WVB' || exit 1
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tools/Windvale-Package-Bundle-Writer.wvproj" \
    "$work/Writer.wvb" || exit $?
verify_file "$work/Writer.wvb" 265268 \
    5e6090061127550d8eb38dd3b3cdfbf3eab30d1cba4af6692711a2c2e094fb31 \
    'bundle writer WVB' || exit 1
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tools/Windvale-Package-Bundle-Verifier.wvproj" \
    "$work/Verifier.wvb" || exit $?
verify_file "$work/Verifier.wvb" 284561 \
    a4f381e9e2dec1c7f415aeb9be24973a971e337b7aff861ed3f84f8b1d7e29fb \
    'bundle verifier WVB' || exit 1

echo 'native package bundle step=package-self-test item=2/7'
"$script_directory/Package-Hosted-Wvb.sh" 6 \
    "$work/Self-Test.wvb" "$work/Self-Test.elf" linux || exit $?
"$work/Self-Test.elf"
[[ $? -eq 42 ]] || exit 1

echo 'native package bundle step=package-writer item=3/7'
"$script_directory/Package-Hosted-Wvb.sh" 6 \
    "$work/Writer.wvb" "$work/Writer.elf" linux || exit $?
echo 'native package bundle step=package-independent-verifier item=4/7'
"$script_directory/Package-Hosted-Wvb.sh" 6 \
    "$work/Verifier.wvb" "$work/Verifier.elf" linux || exit $?

echo 'native package bundle step=rebuild-locked-application item=5/7'
"$script_directory/Build-Wvdb-Query-Package.sh" \
    "$repository_root/Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.wvpack" \
    "$repository_root/Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.wvlock" \
    "$work/Wvdb-Query.wvb" || exit $?
verify_file "$work/Wvdb-Query.wvb" 26294 \
    61f7b9d739a0f4ac9eece1cb79e554e373f49375109cf23d332921395ae37dc2 \
    'locked WVDB Query WVB' || exit 1

echo 'native package bundle step=write-and-admit item=6/7 candidates=2'
for candidate in First Second; do
    "$work/Writer.elf" \
        "$repository_root/Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.wvpack" \
        "$repository_root/Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.wvlock" \
        "$work/Wvdb-Query.wvb" \
        "$repository_root/LICENSE.md" \
        "$repository_root/Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.wvprov" \
        "$work/$candidate.wvbundle" || exit $?
    verify_file "$work/$candidate.wvbundle" 43995 \
        48dff6cf6ce4d5e58e0e13d5a75a514deb86aa98d0b43b5ffbf69d7155b04b6d \
        'WVDB Query Bundle 1 candidate' || exit 1
    "$work/Verifier.elf" "$work/$candidate.wvbundle" || exit $?
done
cmp --silent "$work/First.wvbundle" "$work/Second.wvbundle" || exit 1

echo 'native package bundle step=publish-immutable-store item=7/7 attempts=2'
"$repository_root/Tools/Package/Publish-Admitted-Bundle.sh" \
    "$work/First.wvbundle" \
    48dff6cf6ce4d5e58e0e13d5a75a514deb86aa98d0b43b5ffbf69d7155b04b6d \
    "$work/Store" >"$work/First-Publish.txt" || exit $?
grep -Fx 'package store status=Published bundle=48dff6cf6ce4d5e58e0e13d5a75a514deb86aa98d0b43b5ffbf69d7155b04b6d objects=5 created=6 existing=0' \
    "$work/First-Publish.txt" >/dev/null || exit 1
cat -- "$work/First-Publish.txt"
"$repository_root/Tools/Package/Publish-Admitted-Bundle.sh" \
    "$work/First.wvbundle" \
    48dff6cf6ce4d5e58e0e13d5a75a514deb86aa98d0b43b5ffbf69d7155b04b6d \
    "$work/Store" >"$work/Second-Publish.txt" || exit $?
grep -Fx 'package store status=Published bundle=48dff6cf6ce4d5e58e0e13d5a75a514deb86aa98d0b43b5ffbf69d7155b04b6d objects=5 created=0 existing=6' \
    "$work/Second-Publish.txt" >/dev/null || exit 1
cat -- "$work/Second-Publish.txt"

echo 'native package bundle status=Passed cases=7 bundle=48dff6cf6ce4d5e58e0e13d5a75a514deb86aa98d0b43b5ffbf69d7155b04b6d objects=5 idempotent=Verified'
