#!/usr/bin/env bash
set -uo pipefail
[[ $# -eq 0 ]] || exit 64
script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
work=$(mktemp -d "$temporary_root/windvale-os-x64-code-emission.XXXXXXXX") || exit 1
cleanup() { case "$work" in "$temporary_root"/windvale-os-x64-code-emission.*) rm -f -- "$work"/*; rmdir -- "$work" ;; *) return 1 ;; esac; }
trap cleanup EXIT
verify() { local path=$1 bytes=$2 digest=$3; [[ $(wc -c < "$path") -eq $bytes ]] && printf '%s  %s\n' "$digest" "$path" | sha256sum --check --strict --quiet; }
echo 'step=code-emission item=1/38'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Code-Emission.wvproj" "$work/Test.wvb" >/dev/null || exit $?
verify "$work/Test.wvb" 13597 3bdfd99bb37c4ff037a2d57bfdd89e67a2f190df77f113b50effba1f9c6bd24f || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/Test.wvb" "$work/Test.wvo" >/dev/null || exit $?
verify "$work/Test.wvo" 187279 00dd63a5703136ed0ebe06e55b6e6907f0394fde3b27935ffdcaa60d18f8c4c9 || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/Test.bin" "$work/Test.wvo" >/dev/null || exit $?
verify "$work/Test.bin" 186526 3c1bac2e475b55721a65da9a3d39fefbfe442c3d50ddd75ea166b80fa65a77d2 || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/Test.bin" 0 "$work/Test.elf" >/dev/null || exit $?
verify "$work/Test.elf" 192624 6ed4a40989e0f33e6461d36f2dc8402894ff9e3d9b8417978159cc40df89b300 || exit 1
"$work/Test.elf" >/dev/null
[[ $? -eq 50 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/Test.bin" 0 "$work/Test.exe" >/dev/null || exit $?
verify "$work/Test.exe" 188416 2904c72b25c1d827b0547e839bc5b237694db3e6b4b52de182d774ec83853bec || exit 1
echo 'step=process-entry item=2/38'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Entry-Emission.wvproj" "$work/Entry.wvb" >/dev/null || exit $?
verify "$work/Entry.wvb" 18819 3d830d8788372bfb35e59f86f1cd2fce4bcbab38536d3e1da287f4cac4d15749 || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/Entry.wvb" "$work/Entry.wvo" >/dev/null || exit $?
verify "$work/Entry.wvo" 293142 503d0a912e6299c6ee2ae2a2d441c3d3efe2d99bdf463ea9929662c022ba9c36 || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/Entry.bin" "$work/Entry.wvo" >/dev/null || exit $?
verify "$work/Entry.bin" 291060 45d89b13ccdc220951e1b949920de44fc9e2948f3d30e58e7f57dbe415b9c15a || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/Entry.bin" 0 "$work/Entry.elf" >/dev/null || exit $?
verify "$work/Entry.elf" 299120 81733b1d317d4df43f3b46a1ebdb4e620d5fd38771a734834efc7c3ae1d4dd61 || exit 1
"$work/Entry.elf" >/dev/null
[[ $? -eq 51 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/Entry.bin" 0 "$work/Entry.exe" >/dev/null || exit $?
verify "$work/Entry.exe" 292864 89e0cb8b18666d51ebf7176a913523927f91b1e816a489ad755d891cdc394f30 || exit 1
echo 'step=process-coordinator item=3/38'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Coordinator-Emission.wvproj" "$work/Coordinator.wvb" >/dev/null || exit $?
verify "$work/Coordinator.wvb" 17360 da3d04e734f6057ce9665e1e1c48d6c9dfcdbe0a9396cd1a94397ac4d284a203 || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/Coordinator.wvb" "$work/Coordinator.wvo" >/dev/null || exit $?
verify "$work/Coordinator.wvo" 252088 aee75cbbb20681001780422c024d058281d89b1e90d1b62405c9edce186c6b77 || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/Coordinator.bin" "$work/Coordinator.wvo" >/dev/null || exit $?
verify "$work/Coordinator.bin" 249692 a8d608a9940f68b8de11988efa3749a6153a75292f38cd145c02ae1000a16732 || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/Coordinator.bin" 0 "$work/Coordinator.elf" >/dev/null || exit $?
verify "$work/Coordinator.elf" 254064 8c160bc19330784ca82ca837d5a33fe93fe44fc5701cf491c661b1d06e728318 || exit 1
"$work/Coordinator.elf" >/dev/null
[[ $? -eq 52 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/Coordinator.bin" 0 "$work/Coordinator.exe" >/dev/null || exit $?
verify "$work/Coordinator.exe" 251392 128269a0d5cedd8e2eed4ab4a569b355a1811b2300fd7b16a36078f3eee15c36 || exit 1
echo 'step=process-endpoint item=4/38'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Endpoint-Emission.wvproj" "$work/Endpoint.wvb" >/dev/null || exit $?
verify "$work/Endpoint.wvb" 14386 2d9bdb6b1705bdc0e2e2f3a9b5e5e98224545abc1730ced3c5f55ec0a5cd1391 || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/Endpoint.wvb" "$work/Endpoint.wvo" >/dev/null || exit $?
verify "$work/Endpoint.wvo" 213163 e5a62845e56b9c77b7adccdb8853c1a089f088b62b01088a2814de52d746df66 || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/Endpoint.bin" "$work/Endpoint.wvo" >/dev/null || exit $?
verify "$work/Endpoint.bin" 211641 93ba7b48ba58558471fc678e74a5bab841fd45268f289e1a759c912551e8b796 || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/Endpoint.bin" 0 "$work/Endpoint.elf" >/dev/null || exit $?
verify "$work/Endpoint.elf" 217200 b649ba1abe8db582942085afc90b14ad8d9cd44b542d232df3b7ea19f8a7eb2f || exit 1
"$work/Endpoint.elf" >/dev/null
[[ $? -eq 53 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/Endpoint.bin" 0 "$work/Endpoint.exe" >/dev/null || exit $?
verify "$work/Endpoint.exe" 213504 bb53be86bb8351e805fd0919c6b0836efb483894c36568a6b38dde039a369b20 || exit 1
echo 'step=init-allocation item=5/38'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Memory-Allocation-Emission.wvproj" "$work/Allocation.wvb" >/dev/null || exit $?
verify "$work/Allocation.wvb" 14586 1baa66d77b35db8c2629c0cc2478e29b716739b5ad2c3a2a9096ad9439011112 || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/Allocation.wvb" "$work/Allocation.wvo" >/dev/null || exit $?
verify "$work/Allocation.wvo" 205076 382ffcd386872dd42126c2e85a84746b17df4489ecbbb8c63597c1d710572c79 || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/Allocation.bin" "$work/Allocation.wvo" >/dev/null || exit $?
verify "$work/Allocation.bin" 203252 f17991141c256c3428221465ead0afb13b90787dfda8ae77509f325ebd222008 || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/Allocation.bin" 0 "$work/Allocation.elf" >/dev/null || exit $?
verify "$work/Allocation.elf" 209008 197947667b10fc4bb9a4df15117a0f34f9ff1237a950408679cf9c729fb008c8 || exit 1
"$work/Allocation.elf" >/dev/null
[[ $? -eq 54 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/Allocation.bin" 0 "$work/Allocation.exe" >/dev/null || exit $?
verify "$work/Allocation.exe" 205312 fe1aa700ae411cc3f02277bc13cc8980721fe62aa03f08b0862d81f5bf9e6270 || exit 1
echo 'step=init-record item=6/38'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Record-Emission.wvproj" "$work/Record.wvb" >/dev/null || exit $?
verify "$work/Record.wvb" 16069 be44b1d300abd532a5689755f9ab9ed75b49e7e4954395d3626ee175b9b97e13 || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/Record.wvb" "$work/Record.wvo" >/dev/null || exit $?
verify "$work/Record.wvo" 235764 b013b39333881cfe78f7c1915388ae663a912377b003aa80348993ca876513ee || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/Record.bin" "$work/Record.wvo" >/dev/null || exit $?
verify "$work/Record.bin" 233968 157e355ac94220da5f1b6df2cfc5d51a54b21506f675b06dc3aff0b328371e9b || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/Record.bin" 0 "$work/Record.elf" >/dev/null || exit $?
verify "$work/Record.elf" 241776 1ecaa2ac3dda959a632b88c753d4189ecd3213a2f04c69c886f5bc0f11db23c0 || exit 1
"$work/Record.elf" >/dev/null
[[ $? -eq 55 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/Record.bin" 0 "$work/Record.exe" >/dev/null || exit $?
verify "$work/Record.exe" 236032 693ce53db751bd537ade2933adc8f688ff42492aad6091e005ea9b6391d7ff16 || exit 1
echo 'step=init-paging item=7/38'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Paging-Emission.wvproj" "$work/Paging.wvb" >/dev/null || exit $?
verify "$work/Paging.wvb" 14379 e2f712fb99ecc186211c957a4bdf9f9b0991ad7c735dcb8d47c643e85f9fd50d || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/Paging.wvb" "$work/Paging.wvo" >/dev/null || exit $?
verify "$work/Paging.wvo" 206912 59dc7bcdd1a0ae0b74aa71a85f4330cc28a2f7d3e9d3d4cddce367885d9a6534 || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/Paging.bin" "$work/Paging.wvo" >/dev/null || exit $?
verify "$work/Paging.bin" 205144 eb4ffec315e7ed51c2de630f789cf3aadc44b5ff2ed81131f9b32b39af53608b || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/Paging.bin" 0 "$work/Paging.elf" >/dev/null || exit $?
verify "$work/Paging.elf" 213104 fd20a386a8a0e03a9efce86444498e119f7dffbd67263c3845659d1a7f949ef2 || exit 1
"$work/Paging.elf" >/dev/null
[[ $? -eq 56 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/Paging.bin" 0 "$work/Paging.exe" >/dev/null || exit $?
verify "$work/Paging.exe" 206848 857d384d8e62ccfb435986c4b607d8a7615b9d9bc8c78d1bd73efa38f0dc832e || exit 1
echo 'step=init-image item=8/38'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Image-Emission.wvproj" "$work/Image.wvb" >/dev/null || exit $?
verify "$work/Image.wvb" 16434 3207175a3928407f8b0fb1976e8f55c3643ffa5f0555a46fa9379354d90c0ae1 || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/Image.wvb" "$work/Image.wvo" >/dev/null || exit $?
verify "$work/Image.wvo" 212268 32133cb740952b9193017defc27d10937eabbae42bfa31138eddef259c6147aa || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/Image.bin" "$work/Image.wvo" >/dev/null || exit $?
verify "$work/Image.bin" 210560 a9c40a6cc7de6c3468efc70a675ec2681130344c0cf7e1da8d868bfab998008a || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/Image.bin" 0 "$work/Image.elf" >/dev/null || exit $?
verify "$work/Image.elf" 217200 58b42db3daa211c10f79426dae970fb635233ec19e7f135a1e54ed963e526a87 || exit 1
"$work/Image.elf" >/dev/null
[[ $? -eq 57 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/Image.bin" 0 "$work/Image.exe" >/dev/null || exit $?
verify "$work/Image.exe" 212480 722e4d867408a750d534ddd2ca55b43512ef934d68fd66aaf8e8ba1411d6c8e7 || exit 1
echo 'step=client-reservation item=9/38'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Reservation-Emission.wvproj" "$work/ClientReservation.wvb" >/dev/null || exit $?
verify "$work/ClientReservation.wvb" 14957 bd9bd8bb378642e707e5a328a783dd42df20457aa04c967fcbf63cf8845678b4 || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/ClientReservation.wvb" "$work/ClientReservation.wvo" >/dev/null || exit $?
verify "$work/ClientReservation.wvo" 212197 1d6543a6f9eeb86a0a0e7e64a39b4af66181ea28e88e02ae132b3150ca236391 || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/ClientReservation.bin" "$work/ClientReservation.wvo" >/dev/null || exit $?
verify "$work/ClientReservation.bin" 210223 5b1e36c70a22eab46561f58f73e18ea3241de6afe9ae01d5e2d41c6279c6afdb || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/ClientReservation.bin" 0 "$work/ClientReservation.elf" >/dev/null || exit $?
verify "$work/ClientReservation.elf" 217200 547f5351c84530e41436b51f03b25680f1added815d6238998dc5fe7915e0684 || exit 1
"$work/ClientReservation.elf" >/dev/null
[[ $? -eq 58 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/ClientReservation.bin" 0 "$work/ClientReservation.exe" >/dev/null || exit $?
verify "$work/ClientReservation.exe" 211968 b98c4e3351ea369e6eb70fb8476b03d61300065ae0b57e0d860de458a955196f || exit 1
echo 'step=directory-allocation item=10/38'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Directory-Allocation-Emission.wvproj" "$work/DirectoryAllocation.wvb" >/dev/null || exit $?
verify "$work/DirectoryAllocation.wvb" 14733 c75790ba9823172830b6da72f83a77ce9de2014e0ac9ce4730283a21e261d76f || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/DirectoryAllocation.wvb" "$work/DirectoryAllocation.wvo" >/dev/null || exit $?
verify "$work/DirectoryAllocation.wvo" 207898 0f56e3c872b673a3df7a6aa32aa5c0872588e5d8a867917ae31802818f766cf5 || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/DirectoryAllocation.bin" "$work/DirectoryAllocation.wvo" >/dev/null || exit $?
verify "$work/DirectoryAllocation.bin" 206024 01930821a30da4f113f69dbf7b71937d980a62bc9d8d1684e59df545b65462fa || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/DirectoryAllocation.bin" 0 "$work/DirectoryAllocation.elf" >/dev/null || exit $?
verify "$work/DirectoryAllocation.elf" 213104 551df680881fb91b911caa77f92cb60e02e5f68c11544ea24ffe9b3b634486a3 || exit 1
"$work/DirectoryAllocation.elf" >/dev/null
[[ $? -eq 59 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/DirectoryAllocation.bin" 0 "$work/DirectoryAllocation.exe" >/dev/null || exit $?
verify "$work/DirectoryAllocation.exe" 207872 45d79cbb35032809d41adb4711803772dad0f07a8696674614e832c651748d75 || exit 1
echo 'step=directory-record item=11/38'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Directory-Record-Emission.wvproj" "$work/DirectoryRecord.wvb" >/dev/null || exit $?
verify "$work/DirectoryRecord.wvb" 16076 b549bbb7566023e09cb8dfa65ad774c6c99a6d4cb4b5f7239d0be317833d40b3 || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/DirectoryRecord.wvb" "$work/DirectoryRecord.wvo" >/dev/null || exit $?
verify "$work/DirectoryRecord.wvo" 235780 b7c699def26f9e8b8967142a8ad0fe975ae04c9fa60b5b16e7fcf6a03373682c || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/DirectoryRecord.bin" "$work/DirectoryRecord.wvo" >/dev/null || exit $?
verify "$work/DirectoryRecord.bin" 233984 544f5033bbcf0c9f781bdbf493e34cfd4b6816b53fac64ff0d34f58a3d801bd8 || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/DirectoryRecord.bin" 0 "$work/DirectoryRecord.elf" >/dev/null || exit $?
verify "$work/DirectoryRecord.elf" 241776 b4c32f4820655131c2ba596f8003d78c3ffd16179a599c4f4fe77c9e36267e23 || exit 1
"$work/DirectoryRecord.elf" >/dev/null
[[ $? -eq 60 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/DirectoryRecord.bin" 0 "$work/DirectoryRecord.exe" >/dev/null || exit $?
verify "$work/DirectoryRecord.exe" 236032 865f82f369212f100f46d8e630bfef5a1aa5468e211ac8e15258bfe7c95f4b19 || exit 1
echo 'step=directory-paging item=12/38'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Directory-Paging-Emission.wvproj" "$work/DirectoryPaging.wvb" >/dev/null || exit $?
verify "$work/DirectoryPaging.wvb" 14228 caba027a75434fc07c2f44cafead16f595e7ce4fc13a84864041204d24cd5c17 || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/DirectoryPaging.wvb" "$work/DirectoryPaging.wvo" >/dev/null || exit $?
verify "$work/DirectoryPaging.wvo" 203856 cefb806e6d0af4d7cb531d0f7e4579397a9367e1b96a83e82ed04cccd29764c8 || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/DirectoryPaging.bin" "$work/DirectoryPaging.wvo" >/dev/null || exit $?
verify "$work/DirectoryPaging.bin" 202088 be7476e644e81062f637395946d8b3e8188c3d61b0b6b5e0854dc838c649b3d1 || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/DirectoryPaging.bin" 0 "$work/DirectoryPaging.elf" >/dev/null || exit $?
verify "$work/DirectoryPaging.elf" 209008 303eada707e4868fba8406ccc304e5764ce069d156808d6f44245e98629fb0d9 || exit 1
"$work/DirectoryPaging.elf" >/dev/null
[[ $? -eq 61 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/DirectoryPaging.bin" 0 "$work/DirectoryPaging.exe" >/dev/null || exit $?
verify "$work/DirectoryPaging.exe" 203776 0308cf1a5d01eeb2d463f43bc4ea3b3993f4922b5732cee7e8b23964e2d001c0 || exit 1
echo 'step=directory-image item=13/38'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Directory-Image-Emission.wvproj" "$work/DirectoryImage.wvb" >/dev/null || exit $?
verify "$work/DirectoryImage.wvb" 15098 589034ed2ae906ba8c96ebedb3e583decb9d9181527b70b389d64296f66a4171 || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/DirectoryImage.wvb" "$work/DirectoryImage.wvo" >/dev/null || exit $?
verify "$work/DirectoryImage.wvo" 204016 59380cede0d6d500f554dbbffa8bd8a98bd3cb3e68361c34a32a58fae8642e78 || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/DirectoryImage.bin" "$work/DirectoryImage.wvo" >/dev/null || exit $?
verify "$work/DirectoryImage.bin" 202288 5222772acdcf41ec237179a72083725d9fd5bec8e83324096a5ccba961bbe246 || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/DirectoryImage.bin" 0 "$work/DirectoryImage.elf" >/dev/null || exit $?
verify "$work/DirectoryImage.elf" 209008 4c66120f10ba53e10cf1e7e31ca600eef51d47874b5f629aec0f8c46091bef98 || exit 1
"$work/DirectoryImage.elf" >/dev/null
[[ $? -eq 62 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/DirectoryImage.bin" 0 "$work/DirectoryImage.exe" >/dev/null || exit $?
verify "$work/DirectoryImage.exe" 204288 b20d649b83c3b3ca54550118f77c7775a4937d789f0c08832c03444861c68fbd || exit 1
echo 'step=client-record item=14/38'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Record-Emission.wvproj" "$work/ClientRecord.wvb" >/dev/null || exit $?
verify "$work/ClientRecord.wvb" 16843 6182088b7f1ae89766d2a8cb20b2b022a4ca54571ba63312c7111379c1b15ef3 || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/ClientRecord.wvb" "$work/ClientRecord.wvo" >/dev/null || exit $?
verify "$work/ClientRecord.wvo" 251549 a618e37ff642f693a0e80c46307a595e7634041026c6130a82b2673850c66b79 || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/ClientRecord.bin" "$work/ClientRecord.wvo" >/dev/null || exit $?
verify "$work/ClientRecord.bin" 249563 e64360935ccd01c7146f3ef5890313d3afa180941e39dd22f8c9d6aa7a6fa4d5 || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/ClientRecord.bin" 0 "$work/ClientRecord.elf" >/dev/null || exit $?
verify "$work/ClientRecord.elf" 254064 08911fe6297712035388dd9ae1baaa9e03ddb6d905fd82aba485a33dc192f484 || exit 1
"$work/ClientRecord.elf" >/dev/null
[[ $? -eq 63 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/ClientRecord.bin" 0 "$work/ClientRecord.exe" >/dev/null || exit $?
verify "$work/ClientRecord.exe" 251392 2cbedd60fd226415ba274cffb121b7c39505fa74a6ed854fa628770d844d406b || exit 1
echo 'step=client-paging item=15/38'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Paging-Emission.wvproj" "$work/ClientPaging.wvb" >/dev/null || exit $?
verify "$work/ClientPaging.wvb" 14563 b848688f23ff1e1750044eaec3b4f1837454f7a0c73938699435ce56f81b8fe9 || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/ClientPaging.wvb" "$work/ClientPaging.wvo" >/dev/null || exit $?
verify "$work/ClientPaging.wvo" 206507 fdc55615cdcc3dfe88a0efc281e750074842e95a0661a2b419add0e2df3163c8 || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/ClientPaging.bin" "$work/ClientPaging.wvo" >/dev/null || exit $?
verify "$work/ClientPaging.bin" 204635 dfdc768e8d583879c116b2edb29a9a1740d4c38846d8bf93391ef026ca6511d8 || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/ClientPaging.bin" 0 "$work/ClientPaging.elf" >/dev/null || exit $?
verify "$work/ClientPaging.elf" 209008 bd58157bc0b8023ea2a413c50a5b275bf958b256d08fcbb310a8abb96cca740e || exit 1
"$work/ClientPaging.elf" >/dev/null
[[ $? -eq 64 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/ClientPaging.bin" 0 "$work/ClientPaging.exe" >/dev/null || exit $?
verify "$work/ClientPaging.exe" 206336 5e67969e9047f8b5d71ec79d0de6c86bfdaa77905fac314d12a6ab9d8e7cced7 || exit 1
echo 'step=client-image item=16/38'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Image-Emission.wvproj" "$work/ClientImage.wvb" >/dev/null || exit $?
verify "$work/ClientImage.wvb" 13798 e45446f9c0aa6d8806c3427d2aa3900266067112ff90c29b8d0dea2ea4f4aafd || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/ClientImage.wvb" "$work/ClientImage.wvo" >/dev/null || exit $?
verify "$work/ClientImage.wvo" 187723 7491dcf347e3ce772b9f18d1678191cba4624862575133886e63bbc4e545f88c || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/ClientImage.bin" "$work/ClientImage.wvo" >/dev/null || exit $?
verify "$work/ClientImage.bin" 186049 05acfd5941a598b6c8c43f0aa7406cdcb41da8e5e923a22f489d945c1e2b1a60 || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/ClientImage.bin" 0 "$work/ClientImage.elf" >/dev/null || exit $?
verify "$work/ClientImage.elf" 192624 a2b3880da1d0bdefaf491717d180bb638118d9b706f550f2100b7e596382c1fe || exit 1
"$work/ClientImage.elf" >/dev/null
[[ $? -eq 65 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/ClientImage.bin" 0 "$work/ClientImage.exe" >/dev/null || exit $?
verify "$work/ClientImage.exe" 187904 741049bdb17717f89fc617322a5aa07fe94a4e2c2e3e1286a5a83d62b285067f || exit 1
echo 'step=client-program-resource item=17/38'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Program-Resource-Emission.wvproj" "$work/ClientProgramResource.wvb" >/dev/null || exit $?
verify "$work/ClientProgramResource.wvb" 12763 d0c7e8f7890e6cbc0168dfe122564b48f03a2c4d5bfb658e4e20a9c4ec4e85a1 || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/ClientProgramResource.wvb" "$work/ClientProgramResource.wvo" >/dev/null || exit $?
verify "$work/ClientProgramResource.wvo" 168553 8a05720a44e34a749829ccd421a6dcce3ba9e867f029f245a4f543c93f666bfa || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/ClientProgramResource.bin" "$work/ClientProgramResource.wvo" >/dev/null || exit $?
verify "$work/ClientProgramResource.bin" 167129 b1fd079dde392a72817d30ca9e9a68ab81d926db54bb40c2c1aed76a380e94a3 || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/ClientProgramResource.bin" 0 "$work/ClientProgramResource.elf" >/dev/null || exit $?
verify "$work/ClientProgramResource.elf" 172144 d8b7bf66d482a976a7ecec2b3c0d408c52d942e0b0c75360883cf117aab3d72f || exit 1
"$work/ClientProgramResource.elf" >/dev/null
[[ $? -eq 66 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/ClientProgramResource.bin" 0 "$work/ClientProgramResource.exe" >/dev/null || exit $?
verify "$work/ClientProgramResource.exe" 168960 ac00e3dc1267d2c1c5ce11e389ea93711297930a7b99c1fb061d148b3c001f49 || exit 1
echo 'step=client-budget-resource item=18/38'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Budget-Resource-Emission.wvproj" "$work/ClientBudgetResource.wvb" >/dev/null || exit $?
verify "$work/ClientBudgetResource.wvb" 12586 080eec8cd90b5364bc374eed8fdd3dae520ce7ee9bfb48c0ff30e08aa7150939 || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/ClientBudgetResource.wvb" "$work/ClientBudgetResource.wvo" >/dev/null || exit $?
verify "$work/ClientBudgetResource.wvo" 165922 6b289d4388ef5f2b79d0e344cc2424b0fb823715167309b299347653d7a6f80c || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/ClientBudgetResource.bin" "$work/ClientBudgetResource.wvo" >/dev/null || exit $?
verify "$work/ClientBudgetResource.bin" 164598 ad05831ffbde9abed9e6d8f58bc7c2fb064ed23b28b0ab49e67a478b1e6acd8e || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/ClientBudgetResource.bin" 0 "$work/ClientBudgetResource.elf" >/dev/null || exit $?
verify "$work/ClientBudgetResource.elf" 172144 7461b66d1b74e3dbab07f682d00627c16b32bd1a15a99beef52a8da2aeeb288f || exit 1
"$work/ClientBudgetResource.elf" >/dev/null
[[ $? -eq 67 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/ClientBudgetResource.bin" 0 "$work/ClientBudgetResource.exe" >/dev/null || exit $?
verify "$work/ClientBudgetResource.exe" 166400 577eb58b87816cc15004096f1a20b5e042e3196b5ee3b71af691e91f89f92725 || exit 1
echo 'step=client-store-resource item=19/38'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Store-Resource-Emission.wvproj" "$work/ClientStoreResource.wvb" >/dev/null || exit $?
verify "$work/ClientStoreResource.wvb" 12594 e367cd4e99c842b1e18e9eba459ce034263b3cd6add89ee5d15153015e10dde6 || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/ClientStoreResource.wvb" "$work/ClientStoreResource.wvo" >/dev/null || exit $?
verify "$work/ClientStoreResource.wvo" 165932 ae32cd44aafa4c5f03766c40788d2340622c02846e801c5cc9e87bab945603fa || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/ClientStoreResource.bin" "$work/ClientStoreResource.wvo" >/dev/null || exit $?
verify "$work/ClientStoreResource.bin" 164608 7d797d3765d66df301d2f38e7eff6a17612988664405228b739e2a7081427848 || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/ClientStoreResource.bin" 0 "$work/ClientStoreResource.elf" >/dev/null || exit $?
verify "$work/ClientStoreResource.elf" 172144 96d9d990d5500af4975c54083e76a4837b2915cb873c390d0e32c7650bcb1987 || exit 1
"$work/ClientStoreResource.elf" >/dev/null
[[ $? -eq 68 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/ClientStoreResource.bin" 0 "$work/ClientStoreResource.exe" >/dev/null || exit $?
verify "$work/ClientStoreResource.exe" 166400 28d3812b8a5a627eda4a4c8eeb854a4ca266da49c35eef07997affcb05edc9ec || exit 1
echo 'step=client-directory-resource item=20/38'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Directory-Resource-Emission.wvproj" "$work/ClientDirectoryResource.wvb" >/dev/null || exit $?
verify "$work/ClientDirectoryResource.wvb" 12601 64cf8e6b7241e7fab1aa79d32977bdeb52efd72a376ac96a978093599a7c1c5e || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/ClientDirectoryResource.wvb" "$work/ClientDirectoryResource.wvo" >/dev/null || exit $?
verify "$work/ClientDirectoryResource.wvo" 165932 4cf991e23cfe523d61146fa97e8c66685fe040003ed4a72b7b1c898c73514655 || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/ClientDirectoryResource.bin" "$work/ClientDirectoryResource.wvo" >/dev/null || exit $?
verify "$work/ClientDirectoryResource.bin" 164608 03e9b50e55bae6206f2e58648d55d653ed10b9a10a85ec32a5efffe163daab23 || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/ClientDirectoryResource.bin" 0 "$work/ClientDirectoryResource.elf" >/dev/null || exit $?
verify "$work/ClientDirectoryResource.elf" 172144 d7addd829407b5ef37500c9f54ee08d194858c5bef28497b94e0cca2769fc1aa || exit 1
"$work/ClientDirectoryResource.elf" >/dev/null
[[ $? -eq 69 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/ClientDirectoryResource.bin" 0 "$work/ClientDirectoryResource.exe" >/dev/null || exit $?
verify "$work/ClientDirectoryResource.exe" 166400 8d6d31d9d7ba4f221fe331274e926c41310492f3b94eceb7dac9030c47df365f || exit 1
echo 'step=client-store-validation item=21/38'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Store-Validation-Emission.wvproj" "$work/ClientStoreValidation.wvb" >/dev/null || exit $?
verify "$work/ClientStoreValidation.wvb" 4504 8e0e5c8b0dcc5d58c6f89a517af6ae1bcc30fcf99da2e63fef09892d67c81ead || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/ClientStoreValidation.wvb" "$work/ClientStoreValidation.wvo" >/dev/null || exit $?
verify "$work/ClientStoreValidation.wvo" 62214 e0a46fb18221c75467a6e7d7b6d0c541bce1b9f3275ba389f6c555352ef753ef || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/ClientStoreValidation.bin" "$work/ClientStoreValidation.wvo" >/dev/null || exit $?
verify "$work/ClientStoreValidation.bin" 61794 a04b67dd98d2d0fb6ea60291466ff0adea2333e93c35f766e8033071142eeee3 || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/ClientStoreValidation.bin" 0 "$work/ClientStoreValidation.elf" >/dev/null || exit $?
verify "$work/ClientStoreValidation.elf" 69744 96f3b1fb420ac01b38c553051c88a2d9fca453d11cd417e99e8c8ae1aff6a699 || exit 1
"$work/ClientStoreValidation.elf" >/dev/null
[[ $? -eq 70 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/ClientStoreValidation.bin" 0 "$work/ClientStoreValidation.exe" >/dev/null || exit $?
verify "$work/ClientStoreValidation.exe" 63488 ec3353bc21a776fdb2970e709cf9ba1282e33d3f42e086c27054d925b2cf105f || exit 1
echo 'step=client-directory-validation item=22/38'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Directory-Validation-Emission.wvproj" "$work/ClientDirectoryValidation.wvb" >/dev/null || exit $?
verify "$work/ClientDirectoryValidation.wvb" 4544 9d04682e657cb5f3dbf2c1ce505e144458c2348c9248cb0862393b4ae143c23a || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/ClientDirectoryValidation.wvb" "$work/ClientDirectoryValidation.wvo" >/dev/null || exit $?
verify "$work/ClientDirectoryValidation.wvo" 62648 c0179f7de61f6c615756534d486f379954d0e011b3f3d54e7f08e5f266fca9b4 || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/ClientDirectoryValidation.bin" "$work/ClientDirectoryValidation.wvo" >/dev/null || exit $?
verify "$work/ClientDirectoryValidation.bin" 62278 2fda1d7de6488a445e8419c58e890f332a7be727881c7c8ab09f723b5e95d4b8 || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/ClientDirectoryValidation.bin" 0 "$work/ClientDirectoryValidation.elf" >/dev/null || exit $?
verify "$work/ClientDirectoryValidation.elf" 69744 60b016054f3205ad2726548b7ddc17d463179a9b7d204066addf63b1dd9c8d51 || exit 1
"$work/ClientDirectoryValidation.elf" >/dev/null
[[ $? -eq 71 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/ClientDirectoryValidation.bin" 0 "$work/ClientDirectoryValidation.exe" >/dev/null || exit $?
verify "$work/ClientDirectoryValidation.exe" 64000 7a0b611673c9d8aeea54a3e78ea8030f67d99cfce2de4c54cb0f99fe238c30d7 || exit 1
echo 'step=privileged-entry item=23/38'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Privileged-Entry-Emission.wvproj" "$work/PrivilegedEntry.wvb" >/dev/null || exit $?
verify "$work/PrivilegedEntry.wvb" 5205 ea4cd3684fc0a0cc87957bbed1a57d4e8e83848182b48d113d6ebbe230c133a5 || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/PrivilegedEntry.wvb" "$work/PrivilegedEntry.wvo" >/dev/null || exit $?
verify "$work/PrivilegedEntry.wvo" 51429 344ce7077348450390ed73fce32c44c8027c7e3c742356f33e36bd8cad4ec78c || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/PrivilegedEntry.bin" "$work/PrivilegedEntry.wvo" >/dev/null || exit $?
verify "$work/PrivilegedEntry.bin" 51041 f46e755ed2a76bf2a5a65b41e22edf1eef80e66653336f391f6a8bd4f6d2dbdd || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/PrivilegedEntry.bin" 0 "$work/PrivilegedEntry.elf" >/dev/null || exit $?
verify "$work/PrivilegedEntry.elf" 57456 4b1a3da08c2a9cd0c21d56ff44f1288dc5e92010191fdd6c53c83536b81bc6ed || exit 1
"$work/PrivilegedEntry.elf" >/dev/null
[[ $? -eq 72 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/PrivilegedEntry.bin" 0 "$work/PrivilegedEntry.exe" >/dev/null || exit $?
verify "$work/PrivilegedEntry.exe" 52736 0361f3a1d4be66ca32455a4fc3b103bbd0453380c8d26713cefc7cc37aadc901 || exit 1
echo 'step=thread-timer-state item=24/38'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Thread-Timer-State-Emission.wvproj" "$work/ThreadTimer.wvb" >/dev/null || exit $?
verify "$work/ThreadTimer.wvb" 2526 5341e329f3df812aa7ea81cd8505c95ddc27e3531cbda6a65b6bb3fbf0235d70 || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/ThreadTimer.wvb" "$work/ThreadTimer.wvo" >/dev/null || exit $?
verify "$work/ThreadTimer.wvo" 14482 2ef3ae4096144bc7dd45dfd1f6aecbd23ee7b45b4941bf02add3b33693c586bf || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/ThreadTimer.bin" "$work/ThreadTimer.wvo" >/dev/null || exit $?
verify "$work/ThreadTimer.bin" 14230 97349536af08373fe7a29ebf6ef19a4a238b37f7af10fddd4cccc61861558baa || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/ThreadTimer.bin" 0 "$work/ThreadTimer.elf" >/dev/null || exit $?
verify "$work/ThreadTimer.elf" 20592 bc928927aa085143e3c021941edaa88e4017d801d2ee492e67a9b87d5aab87b3 || exit 1
"$work/ThreadTimer.elf" >/dev/null
[[ $? -eq 73 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/ThreadTimer.bin" 0 "$work/ThreadTimer.exe" >/dev/null || exit $?
verify "$work/ThreadTimer.exe" 16384 7327ad985bd44588276c526ba2aac21336df53d5484be3309f48a2deb7d3ddf7 || exit 1
echo 'step=timer-activation item=25/38'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Timer-Activation-Emission.wvproj" "$work/TimerActivation.wvb" >/dev/null || exit $?
verify "$work/TimerActivation.wvb" 4446 0b95cf7586b996922129d2199bec80051253c14e15f2c263c19a65c07547fc09 || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/TimerActivation.wvb" "$work/TimerActivation.wvo" >/dev/null || exit $?
verify "$work/TimerActivation.wvo" 46353 adabb32058a5943b0103e902a2adafc759c26e59c73d2d27df9ca35767430fb3 || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/TimerActivation.bin" "$work/TimerActivation.wvo" >/dev/null || exit $?
verify "$work/TimerActivation.bin" 45965 6ec505bc781ab84bbcb458c24f03e5467a84d87cbf340c1d49bcf6ca1125d850 || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/TimerActivation.bin" 0 "$work/TimerActivation.elf" >/dev/null || exit $?
verify "$work/TimerActivation.elf" 53360 35f3ece863faf0eb9d93c29b8d98dfc19f209637bb0c35f5519506e6c88c6e08 || exit 1
"$work/TimerActivation.elf" >/dev/null
[[ $? -eq 74 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/TimerActivation.bin" 0 "$work/TimerActivation.exe" >/dev/null || exit $?
verify "$work/TimerActivation.exe" 47616 c906ae32935fe03af5670398fb52e61284d13981b67635790ab6073edaaf725d || exit 1
echo 'step=provider-user-transfer item=26/38'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Provider-User-Transfer-Emission.wvproj" "$work/ProviderTransfer.wvb" >/dev/null || exit $?
verify "$work/ProviderTransfer.wvb" 3860 afc6b4cf959b85feba02abf7f4ade0dc264a7626d6330cfb5eb53ae682e09c28 || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/ProviderTransfer.wvb" "$work/ProviderTransfer.wvo" >/dev/null || exit $?
verify "$work/ProviderTransfer.wvo" 35801 3ae69310cbd48a0fd407646a22b94eaaad0268ef788262ba9c4a2f04052f98fc || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/ProviderTransfer.bin" "$work/ProviderTransfer.wvo" >/dev/null || exit $?
verify "$work/ProviderTransfer.bin" 35363 7da4dbf2f9d6ab02665db4178b4bbdaa2e14133bd627a4d1e696c73ac521896c || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/ProviderTransfer.bin" 0 "$work/ProviderTransfer.elf" >/dev/null || exit $?
verify "$work/ProviderTransfer.elf" 41072 408af2b3e7d77567424a5aafe93eb0c2b185b13d60071e36e6181bb2e125f007 || exit 1
"$work/ProviderTransfer.elf" >/dev/null
[[ $? -eq 75 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/ProviderTransfer.bin" 0 "$work/ProviderTransfer.exe" >/dev/null || exit $?
verify "$work/ProviderTransfer.exe" 37376 df51b14ee5f4845a3962333235abba10ea59f64fd21bb3df98965c932538d768 || exit 1
echo 'step=provider-return-init-transfer item=27/38'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Provider-Return-Init-Transfer-Emission.wvproj" "$work/ProviderReturn.wvb" >/dev/null || exit $?
verify "$work/ProviderReturn.wvb" 3645 4f7ba1ef897096f9ae461539edde3f67f5fc2754fc2068533796ed35b6d72e18 || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/ProviderReturn.wvb" "$work/ProviderReturn.wvo" >/dev/null || exit $?
verify "$work/ProviderReturn.wvo" 26095 c0b76893649bdbb48145160e05cdf830606583bf27b45f6cb7e25a60f9ccd893 || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/ProviderReturn.bin" "$work/ProviderReturn.wvo" >/dev/null || exit $?
verify "$work/ProviderReturn.bin" 25607 a1d715cd3e8dd3c74305aaeb1f7a3465b5e1178161665a6664a9acc8523c255f || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/ProviderReturn.bin" 0 "$work/ProviderReturn.elf" >/dev/null || exit $?
verify "$work/ProviderReturn.elf" 32880 e6ad54583fbcdb6f5c020a1748caa02dd037af37522b46ea7fe149620662130f || exit 1
"$work/ProviderReturn.elf" >/dev/null
[[ $? -eq 76 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/ProviderReturn.bin" 0 "$work/ProviderReturn.exe" >/dev/null || exit $?
verify "$work/ProviderReturn.exe" 27648 36ba9a985fd48c19dcca036d88ee0dde1c8dde33b426c8582e8d7788a817931c || exit 1
echo 'step=init-return-program-validation item=28/38'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Init-Return-Program-Validation-Emission.wvproj" "$work/InitReturn.wvb" >/dev/null || exit $?
verify "$work/InitReturn.wvb" 3198 6c2bf662aa5156f525b21a011753174816c63526db82894032cec825cca0155f || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/InitReturn.wvb" "$work/InitReturn.wvo" >/dev/null || exit $?
verify "$work/InitReturn.wvo" 21983 5e5aa891bec04624daed3562bfc37095611b1cd8054ecf0271e62b572f207e38 || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/InitReturn.bin" "$work/InitReturn.wvo" >/dev/null || exit $?
verify "$work/InitReturn.bin" 21563 dda94557c1ed9f192d207c0e2cde4f7178115f809fc8f450f5c357397ef04a32 || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/InitReturn.bin" 0 "$work/InitReturn.elf" >/dev/null || exit $?
verify "$work/InitReturn.elf" 28784 d044ea33bb2a76f23ff8e621bd4ce5004afd53970d3b9cc07b96008afb566a93 || exit 1
"$work/InitReturn.elf" >/dev/null
[[ $? -eq 77 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/InitReturn.bin" 0 "$work/InitReturn.exe" >/dev/null || exit $?
verify "$work/InitReturn.exe" 23552 e72ef5b51f5b2adc6e3b26cd1983953ad96f28a4eeb9bc2490fe540106e8dff9 || exit 1
echo 'step=init-return-budget-validation item=29/38'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Init-Return-Budget-Validation-Emission.wvproj" "$work/BudgetValidation.wvb" >/dev/null || exit $?
verify "$work/BudgetValidation.wvb" 3019 947de07e02a5abeb8424f71ddb32188f1b1698a60a9a0f69eafdf05bb60e6940 || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/BudgetValidation.wvb" "$work/BudgetValidation.wvo" >/dev/null || exit $?
verify "$work/BudgetValidation.wvo" 21805 f5105e4ff72996d94ffff9ab89ebd1b75a1568f61f1b1bcbb7cf39a0d8240c7f || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/BudgetValidation.bin" "$work/BudgetValidation.wvo" >/dev/null || exit $?
verify "$work/BudgetValidation.bin" 21385 f6894fd05f1a4a18e3312e5d31568c14ff3e718e911969148944adf6f5bc2f00 || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/BudgetValidation.bin" 0 "$work/BudgetValidation.elf" >/dev/null || exit $?
verify "$work/BudgetValidation.elf" 28784 eadb2564cbeebee10cfd58cca34378b65adaeb9ea6f8cf5832abfbf02d23bc9c || exit 1
"$work/BudgetValidation.elf" >/dev/null
[[ $? -eq 78 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/BudgetValidation.bin" 0 "$work/BudgetValidation.exe" >/dev/null || exit $?
verify "$work/BudgetValidation.exe" 23040 ea17483f317867ed5162ca463ff54622f169a331efa4028fb00eff47a2693e9e || exit 1
echo 'step=init-return-store-directory-validation item=30/38'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Init-Return-Store-Directory-Validation-Emission.wvproj" "$work/StoreDirectoryValidation.wvb" >/dev/null || exit $?
verify "$work/StoreDirectoryValidation.wvb" 3267 296e72a6601c2364b1ad6215f69a7a3ac9f69b85ebcedc458e4524c7decaa05e || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/StoreDirectoryValidation.wvb" "$work/StoreDirectoryValidation.wvo" >/dev/null || exit $?
verify "$work/StoreDirectoryValidation.wvo" 22043 238b934e9c701cac294edbfefef99595e6222212959b09efee6712e63c2b74f2 || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/StoreDirectoryValidation.bin" "$work/StoreDirectoryValidation.wvo" >/dev/null || exit $?
verify "$work/StoreDirectoryValidation.bin" 21623 f9a6bbacfe87f5169f174dcb6d7ccc33e4a64c432f3e68aba622313c27d05436 || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/StoreDirectoryValidation.bin" 0 "$work/StoreDirectoryValidation.elf" >/dev/null || exit $?
verify "$work/StoreDirectoryValidation.elf" 28784 04c59fa6379fe7f833450b1d56fff045ca5fbc7a2f62f8f319a2eccde5d89c2c || exit 1
"$work/StoreDirectoryValidation.elf" >/dev/null
[[ $? -eq 79 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/StoreDirectoryValidation.bin" 0 "$work/StoreDirectoryValidation.exe" >/dev/null || exit $?
verify "$work/StoreDirectoryValidation.exe" 23552 ce5ebb7565f1bd26a3648f8d735d0646b3d9450b8ff890b322f5eaebc0710fe5 || exit 1
echo 'step=client-user-transfer item=31/38'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-User-Transfer-Emission.wvproj" "$work/ClientTransfer.wvb" >/dev/null || exit $?
verify "$work/ClientTransfer.wvb" 3861 396c95aacd156af86f6b56d2461a255de115cd5292267035a7d4e5ae4f2ea8a1 || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/ClientTransfer.wvb" "$work/ClientTransfer.wvo" >/dev/null || exit $?
verify "$work/ClientTransfer.wvo" 35804 dd434a07b5c4bae410c80789be9d1e29010c7575b8544ef3329e35501d46e73e || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/ClientTransfer.bin" "$work/ClientTransfer.wvo" >/dev/null || exit $?
verify "$work/ClientTransfer.bin" 35366 f02d2b9b7d84e8571917d2f4d0bdf7145a73e55c8886937702aacc8862482b54 || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/ClientTransfer.bin" 0 "$work/ClientTransfer.elf" >/dev/null || exit $?
verify "$work/ClientTransfer.elf" 41072 4ad53750962c1363f1c1460253c0d917139e581efac4df436c4979c4f60e82d4 || exit 1
"$work/ClientTransfer.elf" >/dev/null
[[ $? -eq 80 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/ClientTransfer.bin" 0 "$work/ClientTransfer.exe" >/dev/null || exit $?
verify "$work/ClientTransfer.exe" 37376 67eb2ded3d5b168c75b6b8300b30e026f5bf46110fed521f517277e69522effc || exit 1
echo 'step=client-return-init-transfer item=32/38'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Return-Init-Transfer-Emission.wvproj" "$work/ClientReturn.wvb" >/dev/null || exit $?
verify "$work/ClientReturn.wvb" 3813 96456642337d7eaf7ef4c8c497eb3f262fd6722ac71f0efe18b6ee9e12f84950 || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/ClientReturn.wvb" "$work/ClientReturn.wvo" >/dev/null || exit $?
verify "$work/ClientReturn.wvo" 26675 f3717d0358c12a4db8c325df347f0d260135145b64bac7372ae21c4f75713756 || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/ClientReturn.bin" "$work/ClientReturn.wvo" >/dev/null || exit $?
verify "$work/ClientReturn.bin" 26187 197aebc42566bec11f51059100feabd063ada5e7fc5df4b5d53bdf45ba3d3749 || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/ClientReturn.bin" 0 "$work/ClientReturn.elf" >/dev/null || exit $?
verify "$work/ClientReturn.elf" 32880 b2a09c2ee5aaa2255866247c6ca5ba22ca5526f3dd2dedce9c67832cc89f2213 || exit 1
"$work/ClientReturn.elf" >/dev/null
[[ $? -eq 81 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/ClientReturn.bin" 0 "$work/ClientReturn.exe" >/dev/null || exit $?
verify "$work/ClientReturn.exe" 28160 cf8fe7a00c3b5bd183bedb6a9878d8f1d2aad32f83d98fecf7700b3a3d10c553 || exit 1
echo 'step=init-reply-publish-resume item=33/38'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Init-Reply-Publish-Resume-Emission.wvproj" "$work/InitReply.wvb" >/dev/null || exit $?
verify "$work/InitReply.wvb" 3816 8f23f7f711f25908c4910ed5de9b2c4097d28d0ae6c1fdc57a0cbffb6cf5c92b || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/InitReply.wvb" "$work/InitReply.wvo" >/dev/null || exit $?
verify "$work/InitReply.wvo" 26680 c0c2538008b7b168488c72be3017369f14f818eb76d1d5d0ca23b71a82b2b9f7 || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/InitReply.bin" "$work/InitReply.wvo" >/dev/null || exit $?
verify "$work/InitReply.bin" 26192 9eb958555632b5cc9fdb178a9013c8f73bf3e3c8563a9fea3c21dbbecbabddeb || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/InitReply.bin" 0 "$work/InitReply.elf" >/dev/null || exit $?
verify "$work/InitReply.elf" 32880 72993c6a4b09d484d56d414b8d41b0ec900c748ce20314eb4a57bee8763c1f4c || exit 1
"$work/InitReply.elf" >/dev/null
[[ $? -eq 82 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/InitReply.bin" 0 "$work/InitReply.exe" >/dev/null || exit $?
verify "$work/InitReply.exe" 28160 68370c479947101120bc84ef6e910aa9b1b8d3f74f42d201934a8144c25f38d2 || exit 1
echo 'step=client-reply-delivery item=34/38'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Reply-Delivery-Emission.wvproj" "$work/ReplyDelivery.wvb" >/dev/null || exit $?
verify "$work/ReplyDelivery.wvb" 3806 668972466f58918a5d13930fdce2ff160d56d25d45907cd0d17214b2689cf44f || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/ReplyDelivery.wvb" "$work/ReplyDelivery.wvo" >/dev/null || exit $?
verify "$work/ReplyDelivery.wvo" 26675 49e5010ba3301cb265b8dccc2bdae2527e2037229fb349c7e4ca13f7b26592a1 || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/ReplyDelivery.bin" "$work/ReplyDelivery.wvo" >/dev/null || exit $?
verify "$work/ReplyDelivery.bin" 26187 304559d8922d82f3544a5c446e6d5a14f160a12f3951166948992f0935e71b50 || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/ReplyDelivery.bin" 0 "$work/ReplyDelivery.elf" >/dev/null || exit $?
verify "$work/ReplyDelivery.elf" 32880 1bce868f6b93bfafc8433cf501678736bef090a2511dd7c835eef9f8be6733c2 || exit 1
"$work/ReplyDelivery.elf" >/dev/null
[[ $? -eq 83 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/ReplyDelivery.bin" 0 "$work/ReplyDelivery.exe" >/dev/null || exit $?
verify "$work/ReplyDelivery.exe" 28160 11d820588e286c52ea4c6374a5e99c80c5803ed1840a9bce9833053fd9b4baa5 || exit 1
echo 'step=client-directory-request-delivery item=35/38'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Directory-Request-Delivery-Emission.wvproj" "$work/DirectoryRequest.wvb" >/dev/null || exit $?
verify "$work/DirectoryRequest.wvb" 3819 a7df225a45ad90cd6667ddbcea1c8005fb0f9f66fab8b48d5eb5b33d60be1a66 || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/DirectoryRequest.wvb" "$work/DirectoryRequest.wvo" >/dev/null || exit $?
verify "$work/DirectoryRequest.wvo" 26675 9cd740d089580da8598c067dad3808c1705c4b35675e87bd7febd168191e04f4 || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/DirectoryRequest.bin" "$work/DirectoryRequest.wvo" >/dev/null || exit $?
verify "$work/DirectoryRequest.bin" 26187 4c4ab3e2b288a9e9fcf40e20f616b4582b31ff63bcc2cc8b5163bd5b5cd67621 || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/DirectoryRequest.bin" 0 "$work/DirectoryRequest.elf" >/dev/null || exit $?
verify "$work/DirectoryRequest.elf" 32880 cc01218d71949cf91c0088b584e5ea931e656fe8fce71307fc2c5394e2802f46 || exit 1
"$work/DirectoryRequest.elf" >/dev/null
[[ $? -eq 84 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/DirectoryRequest.bin" 0 "$work/DirectoryRequest.exe" >/dev/null || exit $?
verify "$work/DirectoryRequest.exe" 28160 5ccda5639fa6b8350bcac8b64cd1c54f144463962932f16198ea8f31c6c4da88 || exit 1
echo 'step=directory-reply-publish-resume item=36/38'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Directory-Reply-Publish-Resume-Emission.wvproj" "$work/DirectoryReply.wvb" >/dev/null || exit $?
verify "$work/DirectoryReply.wvb" 3821 025112005cf1f4be915800b7ca53852ffbee06ee3abeb7d58b5143ecfdc55976 || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/DirectoryReply.wvb" "$work/DirectoryReply.wvo" >/dev/null || exit $?
verify "$work/DirectoryReply.wvo" 26680 9460d10ed4dc0ac79608626f3e5dc9ecc77baed737497ac1485645be3b187500 || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/DirectoryReply.bin" "$work/DirectoryReply.wvo" >/dev/null || exit $?
verify "$work/DirectoryReply.bin" 26192 ee71fee9030f70de743a14eaed01c25284816c43b1ce44181ad16c4a4988feb6 || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/DirectoryReply.bin" 0 "$work/DirectoryReply.elf" >/dev/null || exit $?
verify "$work/DirectoryReply.elf" 32880 b59095e5d2aeecb446bc702960a6996bfa14f9844e5fed112d962b72aa7d68f9 || exit 1
"$work/DirectoryReply.elf" >/dev/null
[[ $? -eq 85 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/DirectoryReply.bin" 0 "$work/DirectoryReply.exe" >/dev/null || exit $?
verify "$work/DirectoryReply.exe" 28160 7f6bf894790451deaa44c6da79431164b7263e129d615f2cf6a6ebad0daff22c || exit 1
echo 'step=client-directory-reply-delivery item=37/38'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Directory-Reply-Delivery-Emission.wvproj" "$work/ClientDirectoryReply.wvb" >/dev/null || exit $?
verify "$work/ClientDirectoryReply.wvb" 3817 c4a7768ab78055fb79a299e4208da0e98c64513f29888308bf9a84e6cfef8bfa || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/ClientDirectoryReply.wvb" "$work/ClientDirectoryReply.wvo" >/dev/null || exit $?
verify "$work/ClientDirectoryReply.wvo" 26675 67e95c51c89e487e4b9f19c1608f803f50d766cf2ef0d7fee848393141fcb800 || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/ClientDirectoryReply.bin" "$work/ClientDirectoryReply.wvo" >/dev/null || exit $?
verify "$work/ClientDirectoryReply.bin" 26187 e6ca3c27c2812103e84c689ee01975cdda5f68109d42a448e54312b93b378c36 || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/ClientDirectoryReply.bin" 0 "$work/ClientDirectoryReply.elf" >/dev/null || exit $?
verify "$work/ClientDirectoryReply.elf" 32880 8007f75463e70308075dd76ae5fdfd0adee0d4a2a30118e8978ec22cc432fce6 || exit 1
"$work/ClientDirectoryReply.elf" >/dev/null
[[ $? -eq 86 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/ClientDirectoryReply.bin" 0 "$work/ClientDirectoryReply.exe" >/dev/null || exit $?
verify "$work/ClientDirectoryReply.exe" 28160 c48b5fa577d9f715be43e69d4d22fa7374700e602867636ffd29c0663b3d32a3 || exit 1
echo 'step=client-completion-cleanup item=38/38'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Completion-Cleanup-Emission.wvproj" "$work/ClientCleanup.wvb" >/dev/null || exit $?
verify "$work/ClientCleanup.wvb" 4541 36b58e50809e26264419c1fca7e429b337fb08f71f4da91fc5a9887cb05306e2 || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/ClientCleanup.wvb" "$work/ClientCleanup.wvo" >/dev/null || exit $?
verify "$work/ClientCleanup.wvo" 23395 eee4dad46947de922bfd45b514a2eba5c256222278ed37b975d2847abdaad1c0 || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/ClientCleanup.bin" "$work/ClientCleanup.wvo" >/dev/null || exit $?
verify "$work/ClientCleanup.bin" 22975 5cb1bb0098b987d31867d4af7990b8dfc9bec7bdbd2ce8d6e05538394e95a91d || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/ClientCleanup.bin" 0 "$work/ClientCleanup.elf" >/dev/null || exit $?
verify "$work/ClientCleanup.elf" 28784 78130c20bab66eb81a42700f6a0c77c89db51457e56c808d674e7f7b1a9e495a || exit 1
"$work/ClientCleanup.elf" >/dev/null
[[ $? -eq 87 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/ClientCleanup.bin" 0 "$work/ClientCleanup.exe" >/dev/null || exit $?
verify "$work/ClientCleanup.exe" 25088 d231f32c41c0ef2d7493180edfeb3edb8f04aed8b1ccb1d5711b8772b0fc28eb || exit 1
echo 'step=client-reclamation-preflight item=39/39'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Reclamation-Preflight-Emission.wvproj" "$work/ClientReclamationPreflight.wvb" >/dev/null || exit $?
verify "$work/ClientReclamationPreflight.wvb" 5489 de9965e67eb1a0607567d4506ca8569083ef025244501b855a786cee37d781c2 || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/ClientReclamationPreflight.wvb" "$work/ClientReclamationPreflight.wvo" >/dev/null || exit $?
verify "$work/ClientReclamationPreflight.wvo" 26770 0a8c3132e27a04d24eb4611cbc6850f3e5706d24f4f8d0b5e63637a56ba367df || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/ClientReclamationPreflight.bin" "$work/ClientReclamationPreflight.wvo" >/dev/null || exit $?
verify "$work/ClientReclamationPreflight.bin" 26282 710c3e5954f04ad8b344b8828c6b3e5cf37cc0c3e947e966df8ec55e9fca75ef || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/ClientReclamationPreflight.bin" 0 "$work/ClientReclamationPreflight.elf" >/dev/null || exit $?
verify "$work/ClientReclamationPreflight.elf" 32880 2098fc92bf5a58448256beefb52207fda7aa974941b26567ee9941df496e0ed1 || exit 1
"$work/ClientReclamationPreflight.elf" >/dev/null
[[ $? -eq 88 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/ClientReclamationPreflight.bin" 0 "$work/ClientReclamationPreflight.exe" >/dev/null || exit $?
verify "$work/ClientReclamationPreflight.exe" 28160 ec6e05d9b84fa9364a18c6e423a9966eb961fcddfb8252fbaeab9b36ddfb2859 || exit 1
echo 'step=client-memory-recycle item=40/40'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Memory-Recycle-Emission.wvproj" "$work/ClientMemoryRecycle.wvb" >/dev/null || exit $?
verify "$work/ClientMemoryRecycle.wvb" 4205 6d43607fde70e4debb388d504d5197f5810377958917ca49c10d31bf3988907d || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/ClientMemoryRecycle.wvb" "$work/ClientMemoryRecycle.wvo" >/dev/null || exit $?
verify "$work/ClientMemoryRecycle.wvo" 34800 a4a98e1a839f6423f7bfb8b37a9a419725b91a7f5523f080dc46defb98d9ec8b || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/ClientMemoryRecycle.bin" "$work/ClientMemoryRecycle.wvo" >/dev/null || exit $?
verify "$work/ClientMemoryRecycle.bin" 34312 159a99df77bee31b100b2aacfd5e659b24aa1134a7a239f5d615ecb3af01b310 || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/ClientMemoryRecycle.bin" 0 "$work/ClientMemoryRecycle.elf" >/dev/null || exit $?
verify "$work/ClientMemoryRecycle.elf" 41072 5215b6db9ae314e336946db9af0be94a1b83be919a6adde2404e064053cdb315 || exit 1
"$work/ClientMemoryRecycle.elf" >/dev/null
[[ $? -eq 89 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/ClientMemoryRecycle.bin" 0 "$work/ClientMemoryRecycle.exe" >/dev/null || exit $?
verify "$work/ClientMemoryRecycle.exe" 36352 bd784204bcb993dd642d1122038af4add3efaf0b68dd695c57cf5be5b7bc402c || exit 1
echo 'step=client-generation-two-record item=41/41'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Generation-Two-Record-Emission.wvproj" "$work/ClientGenerationTwoRecord.wvb" >/dev/null || exit $?
verify "$work/ClientGenerationTwoRecord.wvb" 2246 408a51f39da581efc0ece5c54ba34207c553c82186cc218ae98c64e2a3b30030 || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/ClientGenerationTwoRecord.wvb" "$work/ClientGenerationTwoRecord.wvo" >/dev/null || exit $?
verify "$work/ClientGenerationTwoRecord.wvo" 14191 7355afeb166ae3502a7f6f33cb213060b09568b1e79fddd390757e3ac8f118c6 || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/ClientGenerationTwoRecord.bin" "$work/ClientGenerationTwoRecord.wvo" >/dev/null || exit $?
verify "$work/ClientGenerationTwoRecord.bin" 13939 64ed64bf5378380c8300f3577031f212602b10bbfd88fbec79f9054a12289241 || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/ClientGenerationTwoRecord.bin" 0 "$work/ClientGenerationTwoRecord.elf" >/dev/null || exit $?
verify "$work/ClientGenerationTwoRecord.elf" 20592 aa3700448dfb9d450afc48c64ed20d49f19ad780efe8ecd29031ecbdbba2c7b2 || exit 1
"$work/ClientGenerationTwoRecord.elf" >/dev/null
[[ $? -eq 90 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/ClientGenerationTwoRecord.bin" 0 "$work/ClientGenerationTwoRecord.exe" >/dev/null || exit $?
verify "$work/ClientGenerationTwoRecord.exe" 15872 9ad20271c35b181ead51fd1ff3d84e3d8f83cf44183092c601c72f81a644b85c || exit 1
echo 'step=client-generation-two-paging item=42/42'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Generation-Two-Paging-Emission.wvproj" "$work/ClientGenerationTwoPaging.wvb" >/dev/null || exit $?
verify "$work/ClientGenerationTwoPaging.wvb" 14544 f7e189d04bdf740c5c1b2224c5872a2e3c0159e6408dd4de69dbd6ab3a1db9f2 || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/ClientGenerationTwoPaging.wvb" "$work/ClientGenerationTwoPaging.wvo" >/dev/null || exit $?
verify "$work/ClientGenerationTwoPaging.wvo" 206347 1b56fd5301900cdcc756da7c84af1bec5ff4f509363806d4c9c58ad3e2b1448d || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/ClientGenerationTwoPaging.bin" "$work/ClientGenerationTwoPaging.wvo" >/dev/null || exit $?
verify "$work/ClientGenerationTwoPaging.bin" 204475 443cd01a8a604ff67e41d52a7f52fe8821dd8ae9dbe54708756e98e0c362a087 || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/ClientGenerationTwoPaging.bin" 0 "$work/ClientGenerationTwoPaging.elf" >/dev/null || exit $?
verify "$work/ClientGenerationTwoPaging.elf" 209008 bae80f3e33d79d1446cd54af10c066da7d2bcdb2e95f74ddd4b32eab2d9a1511 || exit 1
"$work/ClientGenerationTwoPaging.elf" >/dev/null
[[ $? -eq 91 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/ClientGenerationTwoPaging.bin" 0 "$work/ClientGenerationTwoPaging.exe" >/dev/null || exit $?
verify "$work/ClientGenerationTwoPaging.exe" 206336 62c74695812eec852cf3dddee37cec39596da28397e0f2abdbaf28e8475119c2 || exit 1
echo 'step=client-generation-two-image item=43/43'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Generation-Two-Image-Emission.wvproj" "$work/ClientGenerationTwoImage.wvb" >/dev/null || exit $?
verify "$work/ClientGenerationTwoImage.wvb" 13762 8758de24cc2954212d55bedab76d3746cfb584313bd455e11f2a0461fba40b1e || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/ClientGenerationTwoImage.wvb" "$work/ClientGenerationTwoImage.wvo" >/dev/null || exit $?
verify "$work/ClientGenerationTwoImage.wvo" 187483 1847ff8d4263ca70bf6d8e165fb1e09bf7f9621a58ef94c8d238b3b1f759d436 || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/ClientGenerationTwoImage.bin" "$work/ClientGenerationTwoImage.wvo" >/dev/null || exit $?
verify "$work/ClientGenerationTwoImage.bin" 185809 1e9357d1626073ab7b2148c5c0368cc977449b691f09180339058c923be86a95 || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/ClientGenerationTwoImage.bin" 0 "$work/ClientGenerationTwoImage.elf" >/dev/null || exit $?
verify "$work/ClientGenerationTwoImage.elf" 192624 fb2d1a9b64f06b8068602e05fe23d710c0e8e45ae1ff06803489c8f31bc6ad4e || exit 1
"$work/ClientGenerationTwoImage.elf" >/dev/null
[[ $? -eq 92 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/ClientGenerationTwoImage.bin" 0 "$work/ClientGenerationTwoImage.exe" >/dev/null || exit $?
verify "$work/ClientGenerationTwoImage.exe" 187904 a78e65b9424e4a10dc65cbf0f5cf5268a3ec04b39bdac854023b9fe35fb46386 || exit 1
echo 'step=client-generation-two-endpoint-rebind item=44/44'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Generation-Two-Endpoint-Rebind-Emission.wvproj" "$work/ClientGenerationTwoEndpointRebind.wvb" >/dev/null || exit $?
verify "$work/ClientGenerationTwoEndpointRebind.wvb" 3388 66c40838688bf09ec245b38b98196d13f62073119afee66b295e526aabc18d52 || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/ClientGenerationTwoEndpointRebind.wvb" "$work/ClientGenerationTwoEndpointRebind.wvo" >/dev/null || exit $?
verify "$work/ClientGenerationTwoEndpointRebind.wvo" 22228 46a33e3e75a085a7b7a2c5713cb976425e21d0e0a90a5326e362c91ab94b1794 || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/ClientGenerationTwoEndpointRebind.bin" "$work/ClientGenerationTwoEndpointRebind.wvo" >/dev/null || exit $?
verify "$work/ClientGenerationTwoEndpointRebind.bin" 21808 bede90cb4efadc65e09b61b0b8a40e2c3b03c2ef04408278fe0b49c1038581cc || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/ClientGenerationTwoEndpointRebind.bin" 0 "$work/ClientGenerationTwoEndpointRebind.elf" >/dev/null || exit $?
verify "$work/ClientGenerationTwoEndpointRebind.elf" 28784 522929410889169de460c55c4f936027c1f2c508c9541cd4f845d439bba3a22d || exit 1
"$work/ClientGenerationTwoEndpointRebind.elf" >/dev/null
[[ $? -eq 93 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/ClientGenerationTwoEndpointRebind.bin" 0 "$work/ClientGenerationTwoEndpointRebind.exe" >/dev/null || exit $?
verify "$work/ClientGenerationTwoEndpointRebind.exe" 23552 edae8ea5a71adfa34652e80f3daa68c0e39ee0e5aff825b329239e58b2077374 || exit 1
echo 'step=client-generation-two-reentry item=45/45'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Generation-Two-Reentry-Emission.wvproj" "$work/ClientGenerationTwoReentry.wvb" >/dev/null || exit $?
verify "$work/ClientGenerationTwoReentry.wvb" 3518 835c7a03de1da731172f5e5d8b515c18f5dc62a40ca030351be90ee9ed6760a3 || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/ClientGenerationTwoReentry.wvb" "$work/ClientGenerationTwoReentry.wvo" >/dev/null || exit $?
verify "$work/ClientGenerationTwoReentry.wvo" 23885 448053de1bc86b48f211668229d6c5f0af21c9e1da73273c36e802b8e995811c || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/ClientGenerationTwoReentry.bin" "$work/ClientGenerationTwoReentry.wvo" >/dev/null || exit $?
verify "$work/ClientGenerationTwoReentry.bin" 23465 77dc2b346ab09fd34f1458517371df4ce826a6f019f5736155c525c927e5f2eb || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/ClientGenerationTwoReentry.bin" 0 "$work/ClientGenerationTwoReentry.elf" >/dev/null || exit $?
verify "$work/ClientGenerationTwoReentry.elf" 28784 4c810ad307e38c6db1264147f23d5e51f3f89375648fcc7813ac4c6ab6e590ea || exit 1
"$work/ClientGenerationTwoReentry.elf" >/dev/null
[[ $? -eq 94 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/ClientGenerationTwoReentry.bin" 0 "$work/ClientGenerationTwoReentry.exe" >/dev/null || exit $?
verify "$work/ClientGenerationTwoReentry.exe" 25600 1cad43b99f4712ea1779d4bbc34238e02be1e1974cbcdaaa3957db83f7c64bcb || exit 1
echo 'step=client-generation-two-return-validation item=46/46'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Generation-Two-Return-Validation-Emission.wvproj" "$work/ClientGenerationTwoReturnValidation.wvb" >/dev/null || exit $?
verify "$work/ClientGenerationTwoReturnValidation.wvb" 4209 8bcbfd66daed3fb0b92c3977374725df6264e0c591444f7d40528badb2aeb1c9 || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/ClientGenerationTwoReturnValidation.wvb" "$work/ClientGenerationTwoReturnValidation.wvo" >/dev/null || exit $?
verify "$work/ClientGenerationTwoReturnValidation.wvo" 23047 8907202105823e1a422036ddd2f93cd578cbdc205f1fc6457f2fb3e08705edc2 || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/ClientGenerationTwoReturnValidation.bin" "$work/ClientGenerationTwoReturnValidation.wvo" >/dev/null || exit $?
verify "$work/ClientGenerationTwoReturnValidation.bin" 22627 976c872494037a1f83428cfbcd7d6b80f581469d6c59d473bc2ddfaf7202ad19 || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/ClientGenerationTwoReturnValidation.bin" 0 "$work/ClientGenerationTwoReturnValidation.elf" >/dev/null || exit $?
verify "$work/ClientGenerationTwoReturnValidation.elf" 28784 6e04e1f5c7420df1f91a0df1f1085bd8af15b428bd79a7108a8390d604de7926 || exit 1
"$work/ClientGenerationTwoReturnValidation.elf" >/dev/null
[[ $? -eq 95 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/ClientGenerationTwoReturnValidation.bin" 0 "$work/ClientGenerationTwoReturnValidation.exe" >/dev/null || exit $?
verify "$work/ClientGenerationTwoReturnValidation.exe" 24576 a26c69002b00dd46f04000ee40f2e6f4603033d2bd5028c171cd0f81dd5114f2 || exit 1
echo 'step=client-generation-two-user-transfer item=47/47'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Generation-Two-User-Transfer-Emission.wvproj" "$work/ClientGenerationTwoUserTransfer.wvb" >/dev/null || exit $?
verify "$work/ClientGenerationTwoUserTransfer.wvb" 4631 42360bdbb290b1bdbd404aaacccbaeb5ac39fc594a9816d0387b4b62a628550f || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/ClientGenerationTwoUserTransfer.wvb" "$work/ClientGenerationTwoUserTransfer.wvo" >/dev/null || exit $?
verify "$work/ClientGenerationTwoUserTransfer.wvo" 45972 fd48f7fb722a87c1fed401d62b0f7ab0176245af51057b53492272241eacbaad || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/ClientGenerationTwoUserTransfer.bin" "$work/ClientGenerationTwoUserTransfer.wvo" >/dev/null || exit $?
verify "$work/ClientGenerationTwoUserTransfer.bin" 45246 71e40197bee68552acdd22d9bd85a789ccd862c112a50e980e4f26a016364370 || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/ClientGenerationTwoUserTransfer.bin" 0 "$work/ClientGenerationTwoUserTransfer.elf" >/dev/null || exit $?
verify "$work/ClientGenerationTwoUserTransfer.elf" 53360 9a9c9e5ebed6472e6eed72f2d002c72e52629657a579c50c73fc917604790de9 || exit 1
"$work/ClientGenerationTwoUserTransfer.elf" >/dev/null
[[ $? -eq 96 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/ClientGenerationTwoUserTransfer.bin" 0 "$work/ClientGenerationTwoUserTransfer.exe" >/dev/null || exit $?
verify "$work/ClientGenerationTwoUserTransfer.exe" 47104 1fa2d7a6e9b1764dba3c3212ea4ec4c74cecbf5a79b6351a703e6cbe5571f7a7 || exit 1
echo 'step=client-generation-two-return-init-transfer item=48/48'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Generation-Two-Return-Init-Transfer-Emission.wvproj" "$work/ClientGenerationTwoReturnInitTransfer.wvb" >/dev/null || exit $?
verify "$work/ClientGenerationTwoReturnInitTransfer.wvb" 4615 b40078fd3d2d928280b697af647bca5a6b399eae9de598e1c43672289f33abcb || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/ClientGenerationTwoReturnInitTransfer.wvb" "$work/ClientGenerationTwoReturnInitTransfer.wvo" >/dev/null || exit $?
verify "$work/ClientGenerationTwoReturnInitTransfer.wvo" 36875 900e0571a75ad5f7ed06ddf2817aea32d26867cbfbed55b64d543aea5e5c5d18 || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/ClientGenerationTwoReturnInitTransfer.bin" "$work/ClientGenerationTwoReturnInitTransfer.wvo" >/dev/null || exit $?
verify "$work/ClientGenerationTwoReturnInitTransfer.bin" 36099 efc70741079a752de3c1e4a1e547dd84f365b5a0bd01fa3400d81ab85378319a || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/ClientGenerationTwoReturnInitTransfer.bin" 0 "$work/ClientGenerationTwoReturnInitTransfer.elf" >/dev/null || exit $?
verify "$work/ClientGenerationTwoReturnInitTransfer.elf" 41072 e57c926117d430ea068b294ec5d9bce0fe35bdf24a0463b15721bdd9b86f5775 || exit 1
"$work/ClientGenerationTwoReturnInitTransfer.elf" >/dev/null
[[ $? -eq 97 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/ClientGenerationTwoReturnInitTransfer.bin" 0 "$work/ClientGenerationTwoReturnInitTransfer.exe" >/dev/null || exit $?
verify "$work/ClientGenerationTwoReturnInitTransfer.exe" 37888 31a60d1b18a4bdb2f73b37e8e7141dcb9891196812101ba26cfcafbcaafbcbff || exit 1
echo 'step=client-generation-two-init-reply-publish-resume item=49/49'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Generation-Two-Init-Reply-Publish-Resume-Emission.wvproj" "$work/ClientGenerationTwoInitReplyPublishResume.wvb" >/dev/null || exit $?
verify "$work/ClientGenerationTwoInitReplyPublishResume.wvb" 4944 1aa2e1875648d7bbf0e6db9328719682990c59518a27d5fe8d55950d639cee05 || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/ClientGenerationTwoInitReplyPublishResume.wvb" "$work/ClientGenerationTwoInitReplyPublishResume.wvo" >/dev/null || exit $?
verify "$work/ClientGenerationTwoInitReplyPublishResume.wvo" 42432 87f854a0b1a54f1efde6c70dbe1281fc365cb099ed1791f17a6a02d4d5224c48 || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/ClientGenerationTwoInitReplyPublishResume.bin" "$work/ClientGenerationTwoInitReplyPublishResume.wvo" >/dev/null || exit $?
verify "$work/ClientGenerationTwoInitReplyPublishResume.bin" 41656 a7cded95095d0abb8a8f35704941fa0bcb7ecb2c2af0198eceff6ced5a089bb0 || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/ClientGenerationTwoInitReplyPublishResume.bin" 0 "$work/ClientGenerationTwoInitReplyPublishResume.elf" >/dev/null || exit $?
verify "$work/ClientGenerationTwoInitReplyPublishResume.elf" 49264 205324fcae6d30ae3f1f09a4971f452728487a4c37d6090cddb827b62db8f65b || exit 1
"$work/ClientGenerationTwoInitReplyPublishResume.elf" >/dev/null
[[ $? -eq 98 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/ClientGenerationTwoInitReplyPublishResume.bin" 0 "$work/ClientGenerationTwoInitReplyPublishResume.exe" >/dev/null || exit $?
verify "$work/ClientGenerationTwoInitReplyPublishResume.exe" 43520 d99e034e63f3cdd9d4571d44684bfe7ef16da45caee09bd2c7a6a1bceca28b3d || exit 1
echo 'step=client-generation-two-reply-delivery item=50/50'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Generation-Two-Reply-Delivery-Emission.wvproj" "$work/ClientGenerationTwoReplyDelivery.wvb" >/dev/null || exit $?
verify "$work/ClientGenerationTwoReplyDelivery.wvb" 5404 af84e26fb039d5b9d0e87f29665dbe6c4a8058f802dda1c94ec24836e95527bc || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/ClientGenerationTwoReplyDelivery.wvb" "$work/ClientGenerationTwoReplyDelivery.wvo" >/dev/null || exit $?
verify "$work/ClientGenerationTwoReplyDelivery.wvo" 51307 2583a5bf8d2c10d333f9dc22752ca2ae07101c336d2e8fbf5d8dfed70b29e111 || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/ClientGenerationTwoReplyDelivery.bin" "$work/ClientGenerationTwoReplyDelivery.wvo" >/dev/null || exit $?
verify "$work/ClientGenerationTwoReplyDelivery.bin" 50531 0e71455553ace04236eb3c62cfa33e61ea1e0fa14299b3a772b2fc7f32918719 || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/ClientGenerationTwoReplyDelivery.bin" 0 "$work/ClientGenerationTwoReplyDelivery.elf" >/dev/null || exit $?
verify "$work/ClientGenerationTwoReplyDelivery.elf" 57456 28c971ed9dde6f9cacd76ff459dca8f4780af9f816da92d6d125d62d6870f2ab || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/ClientGenerationTwoReplyDelivery.bin" 0 "$work/ClientGenerationTwoReplyDelivery.exe" >/dev/null || exit $?
verify "$work/ClientGenerationTwoReplyDelivery.exe" 52224 d6d0b0071c2487f30be46aca7b68b64b783a85ca4dc9075fffea6d54418d34a9 || exit 1
echo 'step=client-generation-two-directory-request-delivery item=51/51'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Generation-Two-Directory-Request-Delivery-Emission.wvproj" "$work/ClientGenerationTwoDirectoryRequestDelivery.wvb" >/dev/null || exit $?
verify "$work/ClientGenerationTwoDirectoryRequestDelivery.wvb" 4613 6a481e470606ca5ce95bbc280e5f71f1288d0f20a6cba414960c61bd84f0285e || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/ClientGenerationTwoDirectoryRequestDelivery.wvb" "$work/ClientGenerationTwoDirectoryRequestDelivery.wvo" >/dev/null || exit $?
verify "$work/ClientGenerationTwoDirectoryRequestDelivery.wvo" 36875 704d2e157f4834debb907f43767b173a5a6f361086774071eb191dd40f636f44 || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/ClientGenerationTwoDirectoryRequestDelivery.bin" "$work/ClientGenerationTwoDirectoryRequestDelivery.wvo" >/dev/null || exit $?
verify "$work/ClientGenerationTwoDirectoryRequestDelivery.bin" 36099 7a634cdf03181dd7cfdc292662410b9f964f0ef122ef3db9a714c15e30cc5d95 || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/ClientGenerationTwoDirectoryRequestDelivery.bin" 0 "$work/ClientGenerationTwoDirectoryRequestDelivery.elf" >/dev/null || exit $?
verify "$work/ClientGenerationTwoDirectoryRequestDelivery.elf" 41072 f9cdb5012700df4645d4aa892be1409274b35f440b281aab9f45890b2223efd2 || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/ClientGenerationTwoDirectoryRequestDelivery.bin" 0 "$work/ClientGenerationTwoDirectoryRequestDelivery.exe" >/dev/null || exit $?
verify "$work/ClientGenerationTwoDirectoryRequestDelivery.exe" 37888 b56d474e69dead5e916d662ad224567ce5c69b20104cf833e062f3f913d039bb || exit 1
echo 'step=directory-generation-two-reply-publish-resume item=52/52'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Directory-Generation-Two-Reply-Publish-Resume-Emission.wvproj" "$work/DirectoryGenerationTwoReply.wvb" >/dev/null || exit $?
verify "$work/DirectoryGenerationTwoReply.wvb" 4915 1724f0265104808a3f30920fef11a41afce50e48902a51ba3801bb3c4ba57273 || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/DirectoryGenerationTwoReply.wvb" "$work/DirectoryGenerationTwoReply.wvo" >/dev/null || exit $?
verify "$work/DirectoryGenerationTwoReply.wvo" 42432 8e25366bd48345a4dab57220a3c9c97697a46bc8db12f97a0ec8c7d59d568f98 || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/DirectoryGenerationTwoReply.bin" "$work/DirectoryGenerationTwoReply.wvo" >/dev/null || exit $?
verify "$work/DirectoryGenerationTwoReply.bin" 41656 ecec624e5629baf15081613123362a758246199436241378c3b98d8c1fd03fe5 || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/DirectoryGenerationTwoReply.bin" 0 "$work/DirectoryGenerationTwoReply.elf" >/dev/null || exit $?
verify "$work/DirectoryGenerationTwoReply.elf" 49264 d555b8543d60d8fc56fd7f19de40b00757a842afe5e6b19f4a5292a25e5b2e57 || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/DirectoryGenerationTwoReply.bin" 0 "$work/DirectoryGenerationTwoReply.exe" >/dev/null || exit $?
verify "$work/DirectoryGenerationTwoReply.exe" 43520 0d2288f7b5c856943350db3ce475dc620c6aef2c5b63ab385e1a62163d757a8b || exit 1
echo 'step=client-generation-two-directory-reply-lifecycle item=53/53'
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Generation-Two-Directory-Reply-Lifecycle-Emission.wvproj" "$work/ClientGenerationTwoDirectoryReplyLifecycle.wvb" >/dev/null || exit $?
verify "$work/ClientGenerationTwoDirectoryReplyLifecycle.wvb" 4847 ed735a92c2b8f058536762898ffc0f181b958bfb61aeb99e87d98f0b633d20fa || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/ClientGenerationTwoDirectoryReplyLifecycle.wvb" "$work/ClientGenerationTwoDirectoryReplyLifecycle.wvo" >/dev/null || exit $?
verify "$work/ClientGenerationTwoDirectoryReplyLifecycle.wvo" 31607 27c90523ef44d616e69f4fbd71f7f2122bc07d612c5b9760cdc778dc3223b247 || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/ClientGenerationTwoDirectoryReplyLifecycle.bin" "$work/ClientGenerationTwoDirectoryReplyLifecycle.wvo" >/dev/null || exit $?
verify "$work/ClientGenerationTwoDirectoryReplyLifecycle.bin" 31119 9b512b17b8286592b3b22f519458e000497d7bb68070e00f27f01293d9189c89 || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/ClientGenerationTwoDirectoryReplyLifecycle.bin" 0 "$work/ClientGenerationTwoDirectoryReplyLifecycle.elf" >/dev/null || exit $?
verify "$work/ClientGenerationTwoDirectoryReplyLifecycle.elf" 36976 eb19da01012c6a39b12522d791363f3fdee488337b323af638167ecd3c997fdb || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/ClientGenerationTwoDirectoryReplyLifecycle.bin" 0 "$work/ClientGenerationTwoDirectoryReplyLifecycle.exe" >/dev/null || exit $?
verify "$work/ClientGenerationTwoDirectoryReplyLifecycle.exe" 32768 25a67bbfdc92654cb380de563cb4bf13870467aed19288921162a229ae104242 || exit 1
echo 'native os x64 code emission status=Passed projects=53 cases=318 local-results=50/51/52/53/54/55/56/57/58/59/60/61/62/63/64/65/66/67/68/69/70/71/72/73/74/75/76/77/78/79/80/81/82/83/84/85/86/87/88/89/90/91/92/93/94/95/96/97/98/99/100/101/102 cross-host-images=Verified source-owned-bytes=29475 relocation-fields=334'
