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
"$script_directory/Build-Current-Wvb.sh" \
    "$repository_root/Projects/Tests/Windvale-Native-Test-Package-Bundle.wvproj" \
    "$work/Self-Test.wvb" || exit $?
verify_file "$work/Self-Test.wvb" 661335 \
    cfad643389cc66461aafea190b54e87242a93a51c61331e9f87a7ce9c5500f2f \
    'bundle self-test WVB' || exit 1
"$script_directory/Build-Current-Wvb.sh" \
    "$repository_root/Projects/Tools/Windvale-Package-Bundle-Writer.wvproj" \
    "$work/Writer.wvb" || exit $?
verify_file "$work/Writer.wvb" 613470 \
    ce17913d57ffab710abc296b1bbbdfc0b25dc3978b1259f3190673fdd9e3e7b1 \
    'bundle writer WVB' || exit 1
"$script_directory/Build-Current-Wvb.sh" \
    "$repository_root/Projects/Tools/Windvale-Package-Bundle-Verifier.wvproj" \
    "$work/Verifier.wvb" || exit $?
verify_file "$work/Verifier.wvb" 632763 \
    cb8c959e44b24aa380f2a0f6b838d371ed2815d51c586e3e96a36190f52319c7 \
    'bundle verifier WVB' || exit 1

echo 'native package bundle step=package-self-test item=2/7'
"$script_directory/Package-Segmented-Compiler-Wvb.sh" 6 \
    "$work/Self-Test.wvb" "$work/Self-Test.elf" || exit $?
"$work/Self-Test.elf"
[[ $? -eq 42 ]] || exit 1

echo 'native package bundle step=package-writer item=3/7'
"$script_directory/Package-Segmented-Compiler-Wvb.sh" 6 \
    "$work/Writer.wvb" "$work/Writer.elf" || exit $?
echo 'native package bundle step=package-independent-verifier item=4/7'
"$script_directory/Package-Segmented-Compiler-Wvb.sh" 6 \
    "$work/Verifier.wvb" "$work/Verifier.elf" || exit $?

echo 'native package bundle step=rebuild-locked-applications item=5/7 applications=2'
"$script_directory/Build-Wvdb-Query-Package.sh" \
    "$repository_root/Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.wvpack" \
    "$repository_root/Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.wvlock" \
    "$work/Wvdb-Query.wvb" || exit $?
verify_file "$work/Wvdb-Query.wvb" 26145 \
    77cb6034402942734be316b9a135d6c1b46ace5cb43a198b2aafe2d1b098027b \
    'locked WVDB Query WVB' || exit 1
"$script_directory/Build-Wvb-Inspector-Package.sh" \
    "$repository_root/Distribution/Applications/Wvb-Inspector/Windvale-Wvb-Inspector.wvpack" \
    "$repository_root/Distribution/Applications/Wvb-Inspector/Windvale-Wvb-Inspector.wvlock" \
    "$work/Wvb-Inspector.wvb" || exit $?
verify_file "$work/Wvb-Inspector.wvb" 76527 \
    293be3267ff95f9272e96684e036a5647abc060f2bc87a9e654beac7140af753 \
    'locked WVB Inspector WVB' || exit 1

echo 'native package bundle step=write-and-admit item=6/7 applications=2 candidates=4'
node -e "const fs=require('node:fs');const input=fs.readFileSync(process.argv[1],'utf8');const output=input.replaceAll('\r\n','\n');if(output.includes('\r')||output.includes('\0'))process.exit(1);fs.writeFileSync(process.argv[2],output);" \
    "$repository_root/LICENSE.md" "$work/LICENSE.md" || exit $?
verify_file "$work/LICENSE.md" 13249 \
    26fc8ccf707d50fcd569353b594345ac234d4bf6e367b2b03cefe6027e108bef \
    'canonical LF license' || exit 1
for candidate in First Second; do
    "$work/Writer.elf" \
        "$repository_root/Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.wvpack" \
        "$repository_root/Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.wvlock" \
        "$work/Wvdb-Query.wvb" \
        "$work/LICENSE.md" \
        "$repository_root/Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.wvprov" \
        "$work/Wvdb-$candidate.wvbundle" || exit $?
    verify_file "$work/Wvdb-$candidate.wvbundle" 43598 \
        40c09378e20b5ac49d41fada61c24e786363e89bf839925cac8d9f3c715a9378 \
        'WVDB Query Bundle 1 candidate' || exit 1
    "$work/Verifier.elf" "$work/Wvdb-$candidate.wvbundle" || exit $?

    "$work/Writer.elf" \
        "$repository_root/Distribution/Applications/Wvb-Inspector/Windvale-Wvb-Inspector.wvpack" \
        "$repository_root/Distribution/Applications/Wvb-Inspector/Windvale-Wvb-Inspector.wvlock" \
        "$work/Wvb-Inspector.wvb" \
        "$work/LICENSE.md" \
        "$repository_root/Distribution/Applications/Wvb-Inspector/Windvale-Wvb-Inspector.wvprov" \
        "$work/Inspector-$candidate.wvbundle" || exit $?
    verify_file "$work/Inspector-$candidate.wvbundle" 92781 \
        a9be069d9eaab7a612a8833d8ce621d1598e01d250ba53a62a2ab4b2126fc4a9 \
        'WVB Inspector Bundle 1 candidate' || exit 1
    "$work/Verifier.elf" "$work/Inspector-$candidate.wvbundle" || exit $?
done
cmp --silent "$work/Wvdb-First.wvbundle" "$work/Wvdb-Second.wvbundle" || exit 1
cmp --silent "$work/Inspector-First.wvbundle" "$work/Inspector-Second.wvbundle" || exit 1

echo 'native package bundle step=publish-shared-immutable-store item=7/7 applications=2 attempts=4'
"$repository_root/Tools/Package/Publish-Admitted-Bundle.sh" \
    "$work/Wvdb-First.wvbundle" \
    40c09378e20b5ac49d41fada61c24e786363e89bf839925cac8d9f3c715a9378 \
    "$work/Store" >"$work/First-Publish.txt" || exit $?
grep -Fx 'package store status=Published bundle=40c09378e20b5ac49d41fada61c24e786363e89bf839925cac8d9f3c715a9378 objects=5 created=6 existing=0' \
    "$work/First-Publish.txt" >/dev/null || exit 1
cat -- "$work/First-Publish.txt"
"$repository_root/Tools/Package/Publish-Admitted-Bundle.sh" \
    "$work/Wvdb-First.wvbundle" \
    40c09378e20b5ac49d41fada61c24e786363e89bf839925cac8d9f3c715a9378 \
    "$work/Store" >"$work/Second-Publish.txt" || exit $?
grep -Fx 'package store status=Published bundle=40c09378e20b5ac49d41fada61c24e786363e89bf839925cac8d9f3c715a9378 objects=5 created=0 existing=6' \
    "$work/Second-Publish.txt" >/dev/null || exit 1
cat -- "$work/Second-Publish.txt"
"$repository_root/Tools/Package/Publish-Admitted-Bundle.sh" \
    "$work/Inspector-First.wvbundle" \
    a9be069d9eaab7a612a8833d8ce621d1598e01d250ba53a62a2ab4b2126fc4a9 \
    "$work/Store" >"$work/Third-Publish.txt" || exit $?
grep -Fx 'package store status=Published bundle=a9be069d9eaab7a612a8833d8ce621d1598e01d250ba53a62a2ab4b2126fc4a9 objects=5 created=5 existing=1' \
    "$work/Third-Publish.txt" >/dev/null || exit 1
cat -- "$work/Third-Publish.txt"
"$repository_root/Tools/Package/Publish-Admitted-Bundle.sh" \
    "$work/Inspector-First.wvbundle" \
    a9be069d9eaab7a612a8833d8ce621d1598e01d250ba53a62a2ab4b2126fc4a9 \
    "$work/Store" >"$work/Fourth-Publish.txt" || exit $?
grep -Fx 'package store status=Published bundle=a9be069d9eaab7a612a8833d8ce621d1598e01d250ba53a62a2ab4b2126fc4a9 objects=5 created=0 existing=6' \
    "$work/Fourth-Publish.txt" >/dev/null || exit 1
cat -- "$work/Fourth-Publish.txt"

echo 'native package bundle status=Passed cases=12 applications=2 bundles=2 objects=9 shared=1 idempotent=Verified'
