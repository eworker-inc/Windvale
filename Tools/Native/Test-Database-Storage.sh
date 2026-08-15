#!/usr/bin/env bash
set -uo pipefail

development=0
prepare_only=0
if [[ $# -eq 1 && $1 == --development ]]; then
    development=1
elif [[ $# -eq 1 && $1 == --prepare-development-tools ]]; then
    development=1
    prepare_only=1
elif [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Database-Storage.sh [--development|--prepare-development-tools]' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-database-storage.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-database-storage.*)
            rm -rf -- "$temporary_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $temporary_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

build_driver_wvb="$temporary_directory/Build-Driver.wvb"
build_driver="$temporary_directory/Build-Driver.elf"
lowerer_wvb="$temporary_directory/Lowerer.wvb"
lowerer="$temporary_directory/Lowerer.elf"
workspace_path="$repository_root/Windvale.wvws"
project_checkpoint_host_storage=NotRun
project_checkpoint_host_tree_reader=NotRun
application_checkpoint_host_storage=NotRun
application_checkpoint_host_tree_reader=NotRun
project_wvb_checkpoint=NotRun
portable_project_checkpoints=
portable_application_checkpoints=
if ((development == 1)); then
    development_start=$SECONDS
    tools_start=$SECONDS
    echo 'START native database storage development phase=tools'
fi

verify_file() {
    local path=$1 expected_size=$2 expected_digest=$3
    [[ -f $path && $(wc -c < "$path") -eq $expected_size ]] || return 1
    local directory
    directory=$(CDPATH= cd -- "$(dirname -- "$path")" && pwd -P) || return 1
    (cd -- "$directory" && printf '%s  %s\n' \
        "$expected_digest" "$(basename -- "$path")" | sha256sum --check --strict --quiet)
}

sha256_file() {
    sha256sum -- "$1" | awk '{ print $1 }'
}

accept_build_driver_checkpoint() {
    local directory=$1 expected_key=$2 expected_input=$3
    local manifest="$directory/Checkpoint.txt"
    local application="$directory/Build-Driver.elf"
    [[ -f $manifest && -f $application && ! -L $directory &&
        ! -L $manifest && ! -L $application ]] || return 1
    [[ $(wc -c < "$manifest") -le 512 ]] || return 1
    local actual_bytes actual_sha256 expected_manifest
    actual_bytes=$(wc -c < "$application") || return 1
    [[ $actual_bytes -gt 0 && $actual_bytes -le 67108864 ]] || return 1
    actual_sha256=$(sha256_file "$application") || return 1
    expected_manifest="$temporary_directory/Checkpoint-Expected.txt"
    printf '%s\n' \
        'windvale-native-tool-checkpoint 1' \
        "key $expected_key" \
        "input-sha256 $expected_input" \
        "output-bytes $actual_bytes" \
        "output-sha256 $actual_sha256" \
        > "$expected_manifest" || return 1
    cmp --silent -- "$expected_manifest" "$manifest" || return 1
    build_driver=$application
}

prepare_cached_build_driver() {
    local input=$1
    local input_sha256 package_sha256 stage_sha256 link_sha256 transport_sha256
    local hosted_sha256 inventory_sha256 material key
    input_sha256=$(sha256_file "$input") || return 1
    package_sha256=$(sha256_file "$script_directory/Package-Segmented-Compiler-Wvb.sh") || return 1
    stage_sha256=$(sha256_file "$script_directory/Stage-Compiler-Wvb.sh") || return 1
    link_sha256=$(sha256_file "$script_directory/Link-Staged-Compiler-Wvo.sh") || return 1
    transport_sha256=$(sha256_file "$script_directory/Transport-Compiler-Image.sh") || return 1
    hosted_sha256=$(sha256_file "$script_directory/Package-Hosted-Wvb.sh") || return 1
    inventory_sha256=$(sha256_file \
        "$repository_root/Artifacts/Native-Hosted-Container-Toolset-Candidate/SHA256SUMS") || return 1
    material="build-driver-v1-linux-profile-2-$input_sha256-$package_sha256-$stage_sha256-$link_sha256-$transport_sha256-$hosted_sha256-$inventory_sha256"
    key=$(printf '%s\n' "$material" | sha256sum | awk '{ print $1 }') || return 1

    local cache_root_input
    if [[ -n ${WINDVALE_NATIVE_CACHE_ROOT:-} ]]; then
        cache_root_input=$WINDVALE_NATIVE_CACHE_ROOT
    else
        cache_root_input="${XDG_CACHE_HOME:-$HOME/.cache}/windvale/native-tool-cache"
    fi
    [[ ! -L $cache_root_input ]] || return 1
    mkdir -p -- "$cache_root_input" || return 1
    local cache_root
    cache_root=$(CDPATH= cd -- "$cache_root_input" && pwd -P) || return 1
    [[ -z $(find "$cache_root" -type l -print -quit) ]] || return 1
    local family="$cache_root/build-driver-v1/linux-profile-2"
    mkdir -p -- "$family" || return 1
    [[ ! -L $family ]] || return 1
    local directory="$family/$key"
    if [[ -e $directory ]]; then
        accept_build_driver_checkpoint "$directory" "$key" "$input_sha256" || return 1
        tool_checkpoint=Hit
        return 0
    fi

    local temporary
    temporary=$(mktemp -d "$family/.new-$key.XXXXXXXX") || return 1
    local candidate="$temporary/Build-Driver.elf"
    if ! "$script_directory/Package-Segmented-Compiler-Wvb.sh" \
        2 "$input" "$candidate" >/dev/null; then
        rm -f -- "$candidate"
        rmdir -- "$temporary"
        return 1
    fi
    local output_sha256 output_bytes
    output_sha256=$(sha256_file "$candidate") || return 1
    output_bytes=$(wc -c < "$candidate") || return 1
    [[ $output_bytes -gt 0 && $output_bytes -le 67108864 ]] || return 1
    printf '%s\n' \
        'windvale-native-tool-checkpoint 1' \
        "key $key" \
        "input-sha256 $input_sha256" \
        "output-bytes $output_bytes" \
        "output-sha256 $output_sha256" \
        > "$temporary/Checkpoint.txt" || return 1
    mv -- "$temporary" "$directory" || return 1
    accept_build_driver_checkpoint "$directory" "$key" "$input_sha256" || return 1
    tool_checkpoint=Created
}

if ((development == 1)); then
    project_wvb_report="$temporary_directory/Build-Driver-Wvb-Cache.txt"
    "$script_directory/Build-Cached-Project-Wvb.sh" \
        "$repository_root/Projects/Tools/Windvale-Compiler-Build-Driver.wvproj" \
        "$build_driver_wvb" > "$project_wvb_report" || exit $?
    project_wvb_checkpoint=$(sed -n \
        's/^native project wvb cache status=\([^ ]*\) key=[0-9a-f][0-9a-f]*$/\1/p' \
        "$project_wvb_report")
    [[ $project_wvb_checkpoint == Created || $project_wvb_checkpoint == Hit ]] || exit 1
    prepare_cached_build_driver "$build_driver_wvb" || exit $?
    lowerer="$repository_root/Artifacts/Native-Wvb-To-Wvo-Candidate/Wvb-To-Wvo.elf"
    verify_file "$lowerer" 7274496 \
        328640d04a2cdff6d1fe943b076554933a7538652185e0e1002fcc4cacbd3579 || exit $?
else
    "$script_directory/Build-Wvb.sh" \
        "$repository_root/Projects/Tools/Windvale-Compiler-Build-Driver.wvproj" \
        "$build_driver_wvb" >/dev/null || exit $?
    "$script_directory/Package-Segmented-Compiler-Wvb.sh" \
        2 "$build_driver_wvb" "$build_driver" >/dev/null || exit $?

    "$script_directory/Build-Wvb.sh" \
        "$repository_root/Projects/Compiler/Windvale-Native-X64-Lowering-Tool.wvproj" \
        "$lowerer_wvb" >/dev/null || exit $?
    "$script_directory/Package-Segmented-Compiler-Wvb.sh" \
        6 "$lowerer_wvb" "$lowerer" >/dev/null || exit $?
fi

if ((development == 1)); then
    tools_elapsed_ms=$(((SECONDS - tools_start) * 1000))
    echo "PASS  native database storage development phase=tools elapsed-ms=$tools_elapsed_ms tool=$tool_checkpoint project-wvb=$project_wvb_checkpoint"
fi

if ((prepare_only == 1)); then
    echo "native database storage development tools status=Passed checkpoint=$tool_checkpoint project-wvb=$project_wvb_checkpoint"
    exit 0
fi

verify_target() {
    local label=$1 project_path=$2
    local first_wvb="$temporary_directory/$label-First.wvb"
    local second_wvb="$temporary_directory/$label-Second.wvb"
    local first_wvo="$temporary_directory/$label-First.wvo"
    local second_wvo="$temporary_directory/$label-Second.wvo"
    local image="$temporary_directory/$label.bin"
    local image_prefix="$temporary_directory/$label-Image"
    local map="$temporary_directory/$label.map"
    local linux_application="$temporary_directory/$label.elf"
    local windows_application="$temporary_directory/$label.exe"
    local project_checkpoint=Rebuilt
    local link_checkpoint=Rebuilt
    local linux_application_checkpoint=Rebuilt
    local target_start=$SECONDS
    if ((development == 1)); then
        echo "START native database storage development target=$label"
    fi

    if ((development == 1)); then
        local project_cache_report="$temporary_directory/$label-Project-Cache.txt"
        "$script_directory/Build-Cached-Project-Object.sh" \
            "$project_path" "$build_driver" "$lowerer" "$first_wvb" "$first_wvo" \
            > "$project_cache_report" || return $?
        project_checkpoint=$(sed -n \
            's/^native project object cache status=\([^ ]*\) key=[0-9a-f][0-9a-f]*$/\1/p' \
            "$project_cache_report")
        [[ $project_checkpoint == Created || $project_checkpoint == Hit ]] || return 1
    else
        "$build_driver" --workspace "$workspace_path" --project "$project_path" "$first_wvb" >/dev/null || return $?
        "$build_driver" --workspace "$workspace_path" --project "$project_path" "$second_wvb" >/dev/null || return $?
        cmp --silent -- "$first_wvb" "$second_wvb" || return 1
        "$lowerer" "$first_wvb" "$first_wvo" >/dev/null || return $?
        "$lowerer" "$second_wvb" "$second_wvo" >/dev/null || return $?
        cmp --silent -- "$first_wvo" "$second_wvo" || return 1
    fi
    if ((development == 0)); then
        "$script_directory/Check-Wvo.sh" "$first_wvo" >/dev/null || return $?
    fi

    if ((development == 1)); then
        local link_cache_report="$temporary_directory/$label-Link-Cache.txt"
        "$script_directory/Build-Cached-Linked-Image.sh" \
            0 Main "$first_wvo" "$image" "$map" > "$link_cache_report" || return $?
        link_checkpoint=$(sed -n \
            's/^native linked image cache status=\([^ ]*\) key=[0-9a-f][0-9a-f]* entry=[0-9][0-9]*$/\1/p' \
            "$link_cache_report")
        [[ $link_checkpoint == Created || $link_checkpoint == Hit ]] || return 1
    else
        "$script_directory/Link-Wvo.sh" 0 Main "$image" "$first_wvo" >"$map" || return $?
    fi
    local entry_offset
    entry_offset=$(sed -n 's/^entry name=Main address=\([0-9][0-9]*\)$/\1/p' "$map")
    case "$entry_offset" in
        ''|*[!0-9]*) return 1 ;;
    esac
    cp -- "$image" "$image_prefix.chunk-0" || return $?

    if ((development == 1)); then
        local linux_application_cache_report="$temporary_directory/$label-Linux-Application-Cache.txt"
        "$script_directory/Build-Cached-Hosted-Application.sh" 6 \
            "$first_wvb" "$image_prefix" 1 "$entry_offset" "$linux_application" linux \
            > "$linux_application_cache_report" || return $?
        linux_application_checkpoint=$(sed -n \
            's/^native hosted application cache status=\([^ ]*\) key=[0-9a-f][0-9a-f]* target=linux$/\1/p' \
            "$linux_application_cache_report")
        [[ $linux_application_checkpoint == Created ||
            $linux_application_checkpoint == Hit ]] || return 1
    else
        "$script_directory/Package-Hosted-Wvb.sh" image 6 \
            "$first_wvb" "$image_prefix" 1 "$entry_offset" "$linux_application" linux \
            >/dev/null || return $?
    fi
    "$linux_application" >/dev/null
    local application_result=$?
    if [[ $application_result -ne 0 ]]; then
        echo "The $label database-storage case returned $application_result, expected 0." >&2
        return 1
    fi

    if ((development == 1)); then
        local target_elapsed_ms=$(((SECONDS - target_start) * 1000))
        echo "PASS  native database storage development target=$label elapsed-ms=$target_elapsed_ms project=$project_checkpoint link=$link_checkpoint host=linux-$linux_application_checkpoint"
        portable_project_checkpoints+="$label:$project_checkpoint/link-$link_checkpoint,"
        portable_application_checkpoints+="$label:linux-$linux_application_checkpoint,"
    else
        "$script_directory/Package-Hosted-Wvb.sh" image 6 \
            "$first_wvb" "$image_prefix" 1 "$entry_offset" "$windows_application" windows \
            >/dev/null || return $?
    fi
}

verify_storage_lowering() {
    local project_path=$1
    local first_wvb="$temporary_directory/StorageLowering-First.wvb"
    local second_wvb="$temporary_directory/StorageLowering-Second.wvb"
    local first_wvo="$temporary_directory/StorageLowering-First.wvo"
    local second_wvo="$temporary_directory/StorageLowering-Second.wvo"
    local first_report="$temporary_directory/StorageLowering-First.txt"
    local second_report="$temporary_directory/StorageLowering-Second.txt"
    local first_bridge="$temporary_directory/StorageLowering-Bridge-First.wvo"
    local second_bridge="$temporary_directory/StorageLowering-Bridge-Second.wvo"
    local image="$temporary_directory/StorageLowering.bin"
    local image_prefix="$temporary_directory/StorageLowering-Image"
    local map="$temporary_directory/StorageLowering.map"
    local linux_application="$temporary_directory/StorageLowering.elf"
    local windows_application="$temporary_directory/StorageLowering.exe"

    "$build_driver" --workspace "$workspace_path" --project "$project_path" "$first_wvb" >/dev/null || return $?
    "$build_driver" --workspace "$workspace_path" --project "$project_path" "$second_wvb" >/dev/null || return $?
    cmp --silent -- "$first_wvb" "$second_wvb" || return 1

    "$lowerer" "$first_wvb" "$first_wvo" >"$first_report" || return $?
    "$lowerer" "$second_wvb" "$second_wvo" >"$second_report" || return $?
    grep -q '^native x64 status=Valid abi=23 ' "$first_report" || return 1
    cmp --silent -- "$first_report" "$second_report" || return 1
    cmp --silent -- "$first_wvo" "$second_wvo" || return 1
    "$script_directory/Check-Wvo.sh" "$first_wvo" >/dev/null || return $?

    "$script_directory/Assemble-Wva.sh" \
        "$repository_root/Runtime/Native/X64-Random-Access-Storage-Describe-Probe.wva" \
        "$first_bridge" >/dev/null || return $?
    "$script_directory/Assemble-Wva.sh" \
        "$repository_root/Runtime/Native/X64-Random-Access-Storage-Describe-Probe.wva" \
        "$second_bridge" >/dev/null || return $?
    cmp --silent -- "$first_bridge" "$second_bridge" || return 1
    "$script_directory/Check-Wvo.sh" "$first_bridge" >/dev/null || return $?

    "$script_directory/Link-Wvo.sh" 0 Storage_describe_probe_entry \
        "$image" "$first_wvo" "$first_bridge" >"$map" || return $?
    local entry_offset
    entry_offset=$(sed -n \
        's/^entry name=Storage_describe_probe_entry address=\([0-9][0-9]*\)$/\1/p' \
        "$map")
    case "$entry_offset" in
        ''|*[!0-9]*) return 1 ;;
    esac
    cp -- "$image" "$image_prefix.chunk-0" || return $?

    "$script_directory/Package-Hosted-Wvb.sh" image 6 \
        "$first_wvb" "$image_prefix" 1 "$entry_offset" \
        "$linux_application" linux >/dev/null || return $?
    "$linux_application" >/dev/null
    local application_result=$?
    if [[ $application_result -ne 0 ]]; then
        echo "The ABI-23 storage describe execution returned $application_result, expected 0." >&2
        return 1
    fi

    "$script_directory/Package-Hosted-Wvb.sh" image 6 \
        "$first_wvb" "$image_prefix" 1 "$entry_offset" \
        "$windows_application" windows >/dev/null || return $?
}

verify_host_storage_interruption() {
    local application=$1 initial=$2 step=$3 scenario_root=$4
    local scenario_directory="$scenario_root/HostStorage-Interruption-$step"
    local scenario_storage="$scenario_directory/Windvale-Database-Storage.bin"
    mkdir -- "$scenario_directory" || return $?
    cp -- "$initial" "$scenario_storage" || return $?
    truncate -s $((4609 + step)) -- "$scenario_storage" || return $?

    (cd -- "$scenario_directory" && "$application" >/dev/null)
    local application_result=$?
    local expected_result=$((90 + step))
    if [[ $application_result -ne $expected_result ]]; then
        echo "The native host-storage interruption $step returned $application_result, expected $expected_result." >&2
        return 1
    fi
    (cd -- "$scenario_directory" && "$application" >/dev/null)
    application_result=$?
    if [[ $application_result -ne 0 ]]; then
        echo "The native host-storage restart $step returned $application_result, expected 0." >&2
        return 1
    fi
    local scenario_bytes
    scenario_bytes=$(wc -c < "$scenario_storage") || return 1
    if ((step <= 2 && scenario_bytes != 4608)); then return 1; fi
    if ((step == 3 && scenario_bytes != 4608 && scenario_bytes != 12800)); then
        return 1
    fi
    if ((step == 4 && scenario_bytes != 12800)); then return 1; fi
}

verify_host_storage() {
    local project_path=$1
    local first_wvb="$temporary_directory/HostStorage-First.wvb"
    local second_wvb="$temporary_directory/HostStorage-Second.wvb"
    local first_wvo="$temporary_directory/HostStorage-First.wvo"
    local second_wvo="$temporary_directory/HostStorage-Second.wvo"
    local common_first="$temporary_directory/HostStorage-Common-First.wvo"
    local common_second="$temporary_directory/HostStorage-Common-Second.wvo"
    local windows_platform="$temporary_directory/HostStorage-Windows.wvo"
    local linux_platform="$temporary_directory/HostStorage-Linux.wvo"
    local linux_image="$temporary_directory/HostStorage-Linux.bin"
    local linux_image_prefix="$temporary_directory/HostStorage-Linux-Image"
    local linux_map="$temporary_directory/HostStorage-Linux.map"
    local linux_application="$temporary_directory/HostStorage.elf"
    local windows_image="$temporary_directory/HostStorage-Windows.bin"
    local windows_image_prefix="$temporary_directory/HostStorage-Windows-Image"
    local windows_map="$temporary_directory/HostStorage-Windows.map"
    local windows_application="$temporary_directory/HostStorage.exe"
    local run_directory="$temporary_directory/HostStorage-Run"
    local storage_file="$run_directory/Windvale-Database-Storage.bin"
    local initial_file="$run_directory/Windvale-Database-Storage.initial"
    local host_storage_checkpoint=Rebuilt
    local host_storage_application_checkpoint=Rebuilt

    if ((development == 1)); then
        local cache_report="$temporary_directory/HostStorage-Cache.txt"
        "$script_directory/Build-Cached-Project-Object.sh" \
            "$project_path" "$build_driver" "$lowerer" "$first_wvb" "$first_wvo" \
            > "$cache_report" || return $?
        host_storage_checkpoint=$(sed -n \
            's/^native project object cache status=\([^ ]*\) key=[0-9a-f][0-9a-f]*$/\1/p' \
            "$cache_report")
        [[ $host_storage_checkpoint == Created || $host_storage_checkpoint == Hit ]] || return 1
    else
        "$build_driver" --workspace "$workspace_path" --project "$project_path" "$first_wvb" >/dev/null || return $?
        "$build_driver" --workspace "$workspace_path" --project "$project_path" "$second_wvb" >/dev/null || return $?
        cmp --silent -- "$first_wvb" "$second_wvb" || return 1
        "$lowerer" "$first_wvb" "$first_wvo" >/dev/null || return $?
        "$lowerer" "$second_wvb" "$second_wvo" >/dev/null || return $?
        cmp --silent -- "$first_wvo" "$second_wvo" || return 1
    fi
    if ((development == 0)); then
        "$script_directory/Check-Wvo.sh" "$first_wvo" >/dev/null || return $?
    fi

    "$script_directory/Assemble-Wva.sh" \
        "$repository_root/Runtime/Native/X64-Random-Access-Storage-Host.wva" \
        "$common_first" >/dev/null || return $?
    if ((development == 0)); then
        "$script_directory/Assemble-Wva.sh" \
            "$repository_root/Runtime/Native/X64-Random-Access-Storage-Host.wva" \
            "$common_second" >/dev/null || return $?
        cmp --silent -- "$common_first" "$common_second" || return 1
    fi
    "$script_directory/Check-Wvo.sh" "$common_first" >/dev/null || return $?

    "$script_directory/Assemble-Wva.sh" \
        "$repository_root/Runtime/Native/Linux-X64-Random-Access-Storage.wva" \
        "$linux_platform" >/dev/null || return $?
    "$script_directory/Check-Wvo.sh" "$linux_platform" >/dev/null || return $?
    "$script_directory/Assemble-Wva.sh" \
        "$repository_root/Runtime/Native/Windows-X64-Random-Access-Storage.wva" \
        "$windows_platform" >/dev/null || return $?
    "$script_directory/Check-Wvo.sh" "$windows_platform" >/dev/null || return $?

    "$script_directory/Link-Wvo.sh" 0 Storage_host_entry \
        "$linux_image" "$first_wvo" "$common_first" "$linux_platform" \
        >"$linux_map" || return $?
    local linux_entry
    linux_entry=$(sed -n \
        's/^entry name=Storage_host_entry address=\([0-9][0-9]*\)$/\1/p' \
        "$linux_map")
    case "$linux_entry" in
        ''|*[!0-9]*) return 1 ;;
    esac
    cp -- "$linux_image" "$linux_image_prefix.chunk-0" || return $?
    if ((development == 1)); then
        local application_cache_report="$temporary_directory/HostStorage-Application-Cache.txt"
        "$script_directory/Build-Cached-Hosted-Application.sh" 6 \
            "$first_wvb" "$linux_image_prefix" 1 "$linux_entry" \
            "$linux_application" linux > "$application_cache_report" || return $?
        host_storage_application_checkpoint=$(sed -n \
            's/^native hosted application cache status=\([^ ]*\) key=[0-9a-f][0-9a-f]* target=linux$/\1/p' \
            "$application_cache_report")
        [[ $host_storage_application_checkpoint == Created ||
            $host_storage_application_checkpoint == Hit ]] || return 1
    else
        "$script_directory/Package-Hosted-Wvb.sh" image 6 \
            "$first_wvb" "$linux_image_prefix" 1 "$linux_entry" \
            "$linux_application" linux >/dev/null || return $?
    fi

    mkdir -- "$run_directory" || return $?
    (cd -- "$run_directory" && "$linux_application" >/dev/null)
    local application_result=$?
    if [[ $application_result -ne 0 ]]; then
        echo "The native host-storage create run returned $application_result, expected 0." >&2
        return 1
    fi
    [[ $(wc -c < "$storage_file") -eq 4608 ]] || return 1
    cp -- "$storage_file" "$initial_file" || return $?

    truncate -s 4625 -- "$storage_file" || return $?
    [[ $(wc -c < "$storage_file") -eq 4625 ]] || return 1
    (cd -- "$run_directory" && "$linux_application" >/dev/null)
    application_result=$?
    if [[ $application_result -ne 0 ]]; then
        echo "The native host-storage recovery run returned $application_result, expected 0." >&2
        return 1
    fi
    [[ $(wc -c < "$storage_file") -eq 4608 ]] || return 1
    cmp --silent -- "$initial_file" "$storage_file" || return 1

    (cd -- "$run_directory" && "$linux_application" >/dev/null)
    application_result=$?
    if [[ $application_result -ne 0 ]]; then
        echo "The native host-storage stable reopen returned $application_result, expected 0." >&2
        return 1
    fi
    cmp --silent -- "$initial_file" "$storage_file" || return 1

    local step
    for step in 0 1 2 3 4; do
        verify_host_storage_interruption \
            "$linux_application" "$initial_file" "$step" "$temporary_directory" \
            || return $?
    done

    if ((development == 1)); then
        project_checkpoint_host_storage=$host_storage_checkpoint
        application_checkpoint_host_storage=$host_storage_application_checkpoint
        return 0
    fi

    "$script_directory/Link-Wvo.sh" 0 Storage_host_entry \
        "$windows_image" "$first_wvo" "$common_first" "$windows_platform" \
        >"$windows_map" || return $?
    local windows_entry
    windows_entry=$(sed -n \
        's/^entry name=Storage_host_entry address=\([0-9][0-9]*\)$/\1/p' \
        "$windows_map")
    case "$windows_entry" in
        ''|*[!0-9]*) return 1 ;;
    esac
    cp -- "$windows_image" "$windows_image_prefix.chunk-0" || return $?
    "$script_directory/Package-Hosted-Wvb.sh" image 6 \
        "$first_wvb" "$windows_image_prefix" 1 "$windows_entry" \
        "$windows_application" windows >/dev/null || return $?
}

verify_host_tree_reader() {
    local project_path=$1
    local first_wvb="$temporary_directory/HostTreeReader-First.wvb"
    local second_wvb="$temporary_directory/HostTreeReader-Second.wvb"
    local first_wvo="$temporary_directory/HostTreeReader-First.wvo"
    local second_wvo="$temporary_directory/HostTreeReader-Second.wvo"
    local common="$temporary_directory/HostStorage-Common-First.wvo"
    local linux_platform="$temporary_directory/HostStorage-Linux.wvo"
    local windows_platform="$temporary_directory/HostStorage-Windows.wvo"
    local linux_image="$temporary_directory/HostTreeReader-Linux.bin"
    local linux_image_prefix="$temporary_directory/HostTreeReader-Linux-Image"
    local linux_map="$temporary_directory/HostTreeReader-Linux.map"
    local linux_application="$temporary_directory/HostTreeReader.elf"
    local windows_image="$temporary_directory/HostTreeReader-Windows.bin"
    local windows_image_prefix="$temporary_directory/HostTreeReader-Windows-Image"
    local windows_map="$temporary_directory/HostTreeReader-Windows.map"
    local windows_application="$temporary_directory/HostTreeReader.exe"
    local initial_file="$temporary_directory/HostStorage-Run/Windvale-Database-Storage.initial"
    local run_directory="$temporary_directory/HostTreeReader-Run"
    local storage_file="$run_directory/Windvale-Database-Storage.bin"
    local depth_two_committed_file="$run_directory/Windvale-Database-Storage.depth-two"
    local committed_file="$run_directory/Windvale-Database-Storage.committed"
    local host_tree_reader_checkpoint=Rebuilt
    local host_tree_reader_application_checkpoint=Rebuilt

    if ((development == 1)); then
        local cache_report="$temporary_directory/HostTreeReader-Cache.txt"
        "$script_directory/Build-Cached-Project-Object.sh" \
            "$project_path" "$build_driver" "$lowerer" "$first_wvb" "$first_wvo" \
            > "$cache_report" || return $?
        host_tree_reader_checkpoint=$(sed -n \
            's/^native project object cache status=\([^ ]*\) key=[0-9a-f][0-9a-f]*$/\1/p' \
            "$cache_report")
        [[ $host_tree_reader_checkpoint == Created || $host_tree_reader_checkpoint == Hit ]] || return 1
    else
        "$build_driver" --workspace "$workspace_path" --project "$project_path" "$first_wvb" >/dev/null || return $?
        "$build_driver" --workspace "$workspace_path" --project "$project_path" "$second_wvb" >/dev/null || return $?
        cmp --silent -- "$first_wvb" "$second_wvb" || return 1
        "$lowerer" "$first_wvb" "$first_wvo" >/dev/null || return $?
        "$lowerer" "$second_wvb" "$second_wvo" >/dev/null || return $?
        cmp --silent -- "$first_wvo" "$second_wvo" || return 1
    fi
    [[ -f $common && -f $linux_platform ]] || return 1

    "$script_directory/Link-Wvo.sh" 0 Storage_host_entry \
        "$linux_image" "$first_wvo" "$common" "$linux_platform" \
        >"$linux_map" || return $?
    local linux_entry
    linux_entry=$(sed -n \
        's/^entry name=Storage_host_entry address=\([0-9][0-9]*\)$/\1/p' \
        "$linux_map")
    case "$linux_entry" in
        ''|*[!0-9]*) return 1 ;;
    esac
    cp -- "$linux_image" "$linux_image_prefix.chunk-0" || return $?
    if ((development == 1)); then
        local application_cache_report="$temporary_directory/HostTreeReader-Application-Cache.txt"
        "$script_directory/Build-Cached-Hosted-Application.sh" 6 \
            "$first_wvb" "$linux_image_prefix" 1 "$linux_entry" \
            "$linux_application" linux > "$application_cache_report" || return $?
        host_tree_reader_application_checkpoint=$(sed -n \
            's/^native hosted application cache status=\([^ ]*\) key=[0-9a-f][0-9a-f]* target=linux$/\1/p' \
            "$application_cache_report")
        [[ $host_tree_reader_application_checkpoint == Created ||
            $host_tree_reader_application_checkpoint == Hit ]] || return 1
    else
        "$script_directory/Package-Hosted-Wvb.sh" image 6 \
            "$first_wvb" "$linux_image_prefix" 1 "$linux_entry" \
            "$linux_application" linux >/dev/null || return $?
    fi

    mkdir -- "$run_directory" || return $?
    cp -- "$initial_file" "$storage_file" || return $?
    (cd -- "$run_directory" && "$linux_application" >/dev/null)
    local application_result=$?
    if [[ $application_result -ne 0 ]]; then
        echo "The native host tree-reader publication returned $application_result, expected 0." >&2
        return 1
    fi
    [[ $(wc -c < "$storage_file") -eq 20992 ]] || return 1
    cp -- "$storage_file" "$depth_two_committed_file" || return $?
    (cd -- "$run_directory" && "$linux_application" >/dev/null)
    application_result=$?
    if [[ $application_result -ne 0 ]]; then
        echo "The native host tree-reader depth-two update returned $application_result, expected 0." >&2
        return 1
    fi
    [[ $(wc -c < "$storage_file") -eq 33280 ]] || return 1
    cp -- "$storage_file" "$committed_file" || return $?
    (cd -- "$run_directory" && "$linux_application" >/dev/null)
    application_result=$?
    if [[ $application_result -ne 0 ]]; then
        echo "The native host tree-reader stable reopen returned $application_result, expected 0." >&2
        return 1
    fi
    cmp --silent -- "$committed_file" "$storage_file" || return 1
    local step
    for step in 0 1 2 3 4; do
        verify_host_tree_reader_interruption \
            "$linux_application" "$initial_file" "$step" "$temporary_directory" \
            || return $?
        verify_host_tree_reader_update_interruption \
            "$linux_application" "$depth_two_committed_file" "$step" "$temporary_directory" \
            || return $?
    done

    if ((development == 1)); then
        project_checkpoint_host_tree_reader=$host_tree_reader_checkpoint
        application_checkpoint_host_tree_reader=$host_tree_reader_application_checkpoint
        return 0
    fi
    [[ -f $windows_platform ]] || return 1
    "$script_directory/Link-Wvo.sh" 0 Storage_host_entry \
        "$windows_image" "$first_wvo" "$common" "$windows_platform" \
        >"$windows_map" || return $?
    local windows_entry
    windows_entry=$(sed -n \
        's/^entry name=Storage_host_entry address=\([0-9][0-9]*\)$/\1/p' \
        "$windows_map")
    case "$windows_entry" in
        ''|*[!0-9]*) return 1 ;;
    esac
    cp -- "$windows_image" "$windows_image_prefix.chunk-0" || return $?
    "$script_directory/Package-Hosted-Wvb.sh" image 6 \
        "$first_wvb" "$windows_image_prefix" 1 "$windows_entry" \
        "$windows_application" windows >/dev/null || return $?
}

verify_host_tree_reader_interruption() {
    local application=$1 initial=$2 step=$3 scenario_root=$4
    local scenario_directory="$scenario_root/HostTreeReader-Interruption-$step"
    local scenario_storage="$scenario_directory/Windvale-Database-Storage.bin"
    mkdir -- "$scenario_directory" || return $?
    cp -- "$initial" "$scenario_storage" || return $?
    truncate -s $((4609 + step)) -- "$scenario_storage" || return $?
    (cd -- "$scenario_directory" && "$application" >/dev/null)
    local application_result=$?
    local expected_result=$((100 + step))
    if [[ $application_result -ne $expected_result ]]; then
        echo "The native host tree-reader interruption $step returned $application_result, expected $expected_result." >&2
        return 1
    fi
    (cd -- "$scenario_directory" && "$application" >/dev/null)
    application_result=$?
    if [[ $application_result -ne 0 ]]; then
        echo "The native host tree-reader restart $step returned $application_result, expected 0." >&2
        return 1
    fi
    (cd -- "$scenario_directory" && "$application" >/dev/null)
    application_result=$?
    if [[ $application_result -ne 0 ]]; then
        echo "The native host tree-reader convergence $step returned $application_result, expected 0." >&2
        return 1
    fi
    [[ $(wc -c < "$scenario_storage") -eq 33280 ]] || return 1
}

verify_host_tree_reader_update_interruption() {
    local application=$1 committed=$2 step=$3 scenario_root=$4
    local scenario_directory="$scenario_root/HostTreeReader-Update-Interruption-$step"
    local scenario_storage="$scenario_directory/Windvale-Database-Storage.bin"
    mkdir -- "$scenario_directory" || return $?
    cp -- "$committed" "$scenario_storage" || return $?
    truncate -s $((20993 + step)) -- "$scenario_storage" || return $?
    (cd -- "$scenario_directory" && "$application" >/dev/null)
    local application_result=$?
    local expected_result=$((110 + step))
    if [[ $application_result -ne $expected_result ]]; then
        echo "The native host tree-reader update interruption $step returned $application_result, expected $expected_result." >&2
        return 1
    fi
    (cd -- "$scenario_directory" && "$application" >/dev/null)
    application_result=$?
    if [[ $application_result -ne 0 ]]; then
        echo "The native host tree-reader update restart $step returned $application_result, expected 0." >&2
        return 1
    fi
    [[ $(wc -c < "$scenario_storage") -eq 33280 ]] || return 1
}

if ((development == 1)); then
    portable_start=$SECONDS
    verify_target TreeNode \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Tree-Node.wvproj" || {
            echo 'The native database storage development tree-node stage failed.' >&2
            exit 1
        }
    verify_target SingleLeaf \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Single-Leaf-Upsert.wvproj" || {
            echo 'The native database storage development single-leaf stage failed.' >&2
            exit 1
        }
    verify_target BranchSplit \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Branch-Split.wvproj" || {
            echo 'The native database storage development branch-split stage failed.' >&2
            exit 1
        }
    verify_target RootSplit \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Root-Split.wvproj" || {
            echo 'The native database storage development root-split stage failed.' >&2
            exit 1
        }
    verify_target DepthTwo \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Depth-Two-Upsert.wvproj" || {
            echo 'The native database storage development depth-two stage failed.' >&2
            exit 1
        }
    verify_target DepthThree \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Depth-Three-Root-Growth.wvproj" || {
            echo 'The native database storage development depth-three stage failed.' >&2
            exit 1
        }
    verify_target DepthThreeUpsert \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Depth-Three-Upsert.wvproj" || {
            echo 'The native database storage development depth-three-upsert stage failed.' >&2
            exit 1
        }
    portable_elapsed_ms=$(((SECONDS - portable_start) * 1000))
    echo "PASS  native database storage development phase=portable-targets elapsed-ms=$portable_elapsed_ms"
else
    verify_target Nested \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Nested-Record-Fields.wvproj" || exit $?
    verify_target Publication \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Storage-Publication.wvproj" || exit $?
    verify_target Recovery \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Storage-Recovery.wvproj" || exit $?
    verify_target SingleWriter \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Single-Writer-Commit.wvproj" || exit $?
    verify_target TreeNode \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Tree-Node.wvproj" || exit $?
    verify_target SingleLeaf \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Single-Leaf-Upsert.wvproj" || exit $?
    verify_target BranchSplit \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Branch-Split.wvproj" || exit $?
    verify_target RootSplit \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Root-Split.wvproj" || exit $?
    verify_target DepthTwo \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Depth-Two-Upsert.wvproj" || exit $?
    verify_target DepthThree \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Depth-Three-Root-Growth.wvproj" || exit $?
    verify_target DepthThreeUpsert \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Depth-Three-Upsert.wvproj" || exit $?
    verify_target ProviderTable \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Capability-Provider-Table.wvproj" || exit $?
    verify_target ProviderCall \
        "$repository_root/Projects/Tests/Windvale-Native-Test-X64-Provider-Call.wvproj" || exit $?
    verify_target Context9 \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Execution-Context-9.wvproj" || exit $?
    verify_storage_lowering \
        "$repository_root/Projects/Tests/Windvale-Native-Test-X64-Storage-Random-Access.wvproj" || exit $?
fi
if ((development == 1)); then
    host_storage_start=$SECONDS
    echo 'START native database storage development phase=host-storage'
fi
verify_host_storage \
    "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Host-Storage.wvproj" || {
        echo 'The native database storage host-storage stage failed.' >&2
        exit 1
    }
if ((development == 1)); then
    host_storage_elapsed_ms=$(((SECONDS - host_storage_start) * 1000))
    echo "PASS  native database storage development phase=host-storage elapsed-ms=$host_storage_elapsed_ms"
    host_tree_reader_start=$SECONDS
    echo 'START native database storage development phase=host-tree-reader'
fi
verify_host_tree_reader \
    "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Host-Tree-Reader.wvproj" || {
        echo 'The native database storage tree-update stage failed.' >&2
        exit 1
    }

if ((development == 1)); then
    host_tree_reader_elapsed_ms=$(((SECONDS - host_tree_reader_start) * 1000))
    echo "PASS  native database storage development phase=host-tree-reader elapsed-ms=$host_tree_reader_elapsed_ms"
    development_elapsed_ms=$(((SECONDS - development_start) * 1000))
    echo "native database storage development timing tools-ms=$tools_elapsed_ms portable-ms=$portable_elapsed_ms host-storage-ms=$host_storage_elapsed_ms host-tree-reader-ms=$host_tree_reader_elapsed_ms total-ms=$development_elapsed_ms"
    echo "native database storage development status=Passed cases=9 local-results=0 tools=$tool_checkpoint project-wvb=$project_wvb_checkpoint portable-projects=$portable_project_checkpoints portable-applications=$portable_application_checkpoints projects=HostStorage:$project_checkpoint_host_storage,HostTreeReader:$project_checkpoint_host_tree_reader applications=HostStorage:$application_checkpoint_host_storage,HostTreeReader:$application_checkpoint_host_tree_reader"
    exit 0
fi
echo 'native database storage status=Passed cases=18 local-results=0 cross-host-images=Verified'
