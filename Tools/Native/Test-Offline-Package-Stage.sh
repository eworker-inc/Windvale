#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Offline-Package-Stage.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
creator=$repository_root/Tools/Release/Create-Release-Envelope.mjs
release_verifier=$repository_root/Tools/Release/Verify-Release-Envelope.mjs
approval_verifier=$repository_root/Tools/Release/Verify-Wvdb-Approval-Records.mjs
stage_tool=$repository_root/Tools/Package/Create-Offline-Package-Stage-Input.mjs
fixture_tool=$repository_root/Tools/Native/Create-Release-Envelope-Fixture.mjs
generation_publisher=$repository_root/Tools/Package/Publish-Installation-Generation.mjs
temporary_root=${TMPDIR:-/tmp}
work=$(mktemp -d "$temporary_root/windvale-offline-package-stage.XXXXXXXX") || exit 1
cleanup() {
    case "$work" in
        "$temporary_root"/windvale-offline-package-stage.*) rm -rf -- "$work" ;;
        *) echo "Refusing to remove unexpected temporary path: $work" >&2; return 1 ;;
    esac
}
trap cleanup EXIT
for directory in Stage-Input Root-Key Release-Key Policy First Second Tampered Installed; do
    mkdir -- "$work/$directory" || exit 1
done

verify_file() {
    local file=$1 expected_bytes=$2 expected_sha256=$3
    [[ -f $file && $(wc -c <"$file") -eq $expected_bytes ]] || return 1
    [[ $(sha256sum -- "$file" | cut -d ' ' -f 1) == "$expected_sha256" ]]
}

echo 'native offline package stage step=build-tools item=1/8'
"$script_directory/Build-Current-Wvb.sh" \
    "$repository_root/Projects/Tools/Windvale-Package-Bundle-Writer.wvproj" \
    "$work/Writer.wvb" || exit $?
verify_file "$work/Writer.wvb" 613470 \
    ce17913d57ffab710abc296b1bbbdfc0b25dc3978b1259f3190673fdd9e3e7b1 || exit 1
"$script_directory/Build-Current-Wvb.sh" \
    "$repository_root/Projects/Tools/Windvale-Package-Bundle-Verifier.wvproj" \
    "$work/Verifier.wvb" || exit $?
verify_file "$work/Verifier.wvb" 632763 \
    cb8c959e44b24aa380f2a0f6b838d371ed2815d51c586e3e96a36190f52319c7 || exit 1
"$script_directory/Build-Current-Wvb.sh" \
    "$repository_root/Projects/Tools/Windvale-Installation-Generation-Verifier.wvproj" \
    "$work/Generation-Verifier.wvb" || exit $?
verify_file "$work/Generation-Verifier.wvb" 42364 \
    2beb02ba0ea13b1552a0c3bf9b92bebe438ac65b2eb49000a4fc1762ed8f7e9f || exit 1
"$script_directory/Package-Segmented-Compiler-Wvb.sh" 6 \
    "$work/Writer.wvb" "$work/Writer.elf" || exit $?
"$script_directory/Package-Segmented-Compiler-Wvb.sh" 6 \
    "$work/Verifier.wvb" "$work/Verifier.elf" || exit $?
"$script_directory/Package-Hosted-Wvb.sh" 6 \
    "$work/Generation-Verifier.wvb" "$work/Generation-Verifier.elf" linux || exit $?

echo 'native offline package stage step=build-packages item=2/8 packages=2'
"$script_directory/Build-Wvdb-Query-Package.sh" \
    "$repository_root/Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.wvpack" \
    "$repository_root/Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.wvlock" \
    "$work/Wvdb-Query.wvb" || exit $?
"$script_directory/Build-Wvb-Inspector-Package.sh" \
    "$repository_root/Distribution/Applications/Wvb-Inspector/Windvale-Wvb-Inspector.wvpack" \
    "$repository_root/Distribution/Applications/Wvb-Inspector/Windvale-Wvb-Inspector.wvlock" \
    "$work/Wvb-Inspector.wvb" || exit $?

echo 'native offline package stage step=write-and-admit-bundles item=3/8 packages=2'
node -e "const fs=require('node:fs');const input=fs.readFileSync(process.argv[1],'utf8');const output=input.replaceAll('\r\n','\n');if(output.includes('\r')||output.includes('\0'))process.exit(1);fs.writeFileSync(process.argv[2],output);" \
    "$repository_root/LICENSE.md" "$work/LICENSE.md" || exit $?
"$work/Writer.elf" \
    "$repository_root/Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.wvpack" \
    "$repository_root/Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.wvlock" \
    "$work/Wvdb-Query.wvb" "$work/LICENSE.md" \
    "$repository_root/Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.wvprov" \
    "$work/Wvdb-Query.wvbundle" || exit $?
"$work/Verifier.elf" "$work/Wvdb-Query.wvbundle" >/dev/null || exit $?
"$work/Writer.elf" \
    "$repository_root/Distribution/Applications/Wvb-Inspector/Windvale-Wvb-Inspector.wvpack" \
    "$repository_root/Distribution/Applications/Wvb-Inspector/Windvale-Wvb-Inspector.wvlock" \
    "$work/Wvb-Inspector.wvb" "$work/LICENSE.md" \
    "$repository_root/Distribution/Applications/Wvb-Inspector/Windvale-Wvb-Inspector.wvprov" \
    "$work/Wvb-Inspector.wvbundle" || exit $?
"$work/Verifier.elf" "$work/Wvb-Inspector.wvbundle" >/dev/null || exit $?

echo 'native offline package stage step=create-exact-input item=4/8 policy-records=8'
node "$approval_verifier" verify >/dev/null || exit $?
node "$approval_verifier" verify-inspector >/dev/null || exit $?
revision=$(git -c "safe.directory=$repository_root" -C "$repository_root" \
    rev-parse HEAD) || exit $?
tree=$(git -c "safe.directory=$repository_root" -C "$repository_root" \
    rev-parse HEAD:) || exit $?
node "$stage_tool" "$work/Wvdb-Query.wvbundle" "$work/Wvb-Inspector.wvbundle" \
    "$revision" "$tree" "$work/Stage-Input" || exit $?

echo 'native offline package stage step=sign-first item=5/8 channel=stage'
node "$creator" generate-test-key root "$work/Root-Key" >/dev/null || exit $?
node "$creator" generate-test-key release "$work/Release-Key" >/dev/null || exit $?
node "$creator" create-root "$work/Stage-Input/Root-Input.json" \
    "$work/Root-Key/root-private.pem" "$work/Release-Key/release-public.pem" \
    "$work/Policy" >/dev/null || exit $?
node "$creator" create-release "$work/Policy" \
    "$work/Release-Key/release-private.pem" "$work/Stage-Input/Release-Input.json" \
    "$work/Stage-Input/Sources" "$work/First" >/dev/null || exit $?

echo 'native offline package stage step=prove-determinism item=6/8'
node "$creator" create-release "$work/Policy" \
    "$work/Release-Key/release-private.pem" "$work/Stage-Input/Release-Input.json" \
    "$work/Stage-Input/Sources" "$work/Second" >/dev/null || exit $?
node "$fixture_tool" compare "$work/First" "$work/Second" >/dev/null || exit $?

echo 'native offline package stage step=verify-offline-directory item=7/8 packages=2'
node "$release_verifier" verify "$work/Root-Key/root-public.pem" \
    "$work/First" >"$work/Verify.txt" || exit $?
grep -F 'release verify status=Valid version=0.1.0 channel=stage' \
    "$work/Verify.txt" >/dev/null || exit 1
grep -F 'artifact package windvale.wvb-inspector a9be069d9eaab7a612a8833d8ce621d1598e01d250ba53a62a2ab4b2126fc4a9 92781' \
    "$work/First/Release-Manifest.txt" >/dev/null || exit 1
grep -F 'artifact package windvale.wvdb-query 40c09378e20b5ac49d41fada61c24e786363e89bf839925cac8d9f3c715a9378 43598' \
    "$work/First/Release-Manifest.txt" >/dev/null || exit 1
grep -F 'artifact generation linux-x64 d09fa80957b663d3f6d2e85fa8b1ccf7cb3a79c35e250a94d4a6e9f86fec4bd5 726' \
    "$work/First/Release-Manifest.txt" >/dev/null || exit 1
grep -F 'artifact generation windows-x64 7ab98618e589b58850f94a1390c9a2d887f51127c6e6d6cc4c674b319d7a4999 728' \
    "$work/First/Release-Manifest.txt" >/dev/null || exit 1
"$work/Generation-Verifier.elf" \
    "$work/First/Artifacts/Generations/Generation-1.windows-x64.txt" \
    >"$work/Generation-Windows.txt" || exit $?
grep -Fx 'generation status=Valid target=windows-x64 packages=2 commands=2' \
    "$work/Generation-Windows.txt" >/dev/null || exit 1
"$work/Generation-Verifier.elf" \
    "$work/First/Artifacts/Generations/Generation-1.linux-x64.txt" \
    >"$work/Generation-Linux.txt" || exit $?
grep -Fx 'generation status=Valid target=linux-x64 packages=2 commands=2' \
    "$work/Generation-Linux.txt" >/dev/null || exit 1
node "$generation_publisher" publish "$work/Installed" \
    "$work/First/Artifacts/Generations/Generation-1.linux-x64.txt" \
    d09fa80957b663d3f6d2e85fa8b1ccf7cb3a79c35e250a94d4a6e9f86fec4bd5 \
    >/dev/null || exit $?
node "$generation_publisher" verify "$work/Installed" \
    d09fa80957b663d3f6d2e85fa8b1ccf7cb3a79c35e250a94d4a6e9f86fec4bd5 \
    >/dev/null || exit $?

echo 'native offline package stage step=reject-package-tamper item=8/8'
node "$fixture_tool" copy "$work/First" "$work/Tampered" >/dev/null || exit $?
printf x >>"$work/Tampered/Artifacts/Packages/Windvale-Wvb-Inspector.wvbundle"
if node "$release_verifier" verify "$work/Root-Key/root-public.pem" \
    "$work/Tampered" >/dev/null 2>&1; then exit 1; fi

echo 'native offline package stage status=Passed cases=8 packages=2 policy-records=8 generations=2 published=1 artifacts=14 deterministic=Verified tamper=Rejected'
