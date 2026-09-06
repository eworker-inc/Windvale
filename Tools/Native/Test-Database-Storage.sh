#!/usr/bin/env bash
set -uo pipefail

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
if [[ $# -eq 0 ]]; then
    exec node "$script_directory/Run-Database-Storage-Qualification.mjs"
fi

development=0
prepare_only=0
development_target=all
qualification_target=all
qualification_step_target=
qualification_support_steps=0
if [[ $# -eq 1 && $1 == --development ]]; then
    development=1
elif [[ $# -eq 2 &&
    ($1 == --development-target || $1 == --development-target-set) ]]; then
    development=1
    development_target=$2
elif [[ $# -eq 1 && $1 == --prepare-development-tools ]]; then
    development=1
    prepare_only=1
elif [[ $# -eq 1 && $1 == --qualification-portable ]]; then
    qualification_target=portable
elif [[ $# -eq 1 && $1 == --qualification-hosted ]]; then
    qualification_target=hosted
elif [[ $# -eq 2 && $1 == --qualification-step ]]; then
    qualification_target=focused
    qualification_step_target=$2
else
    echo 'Usage: ./Tools/Native/Test-Database-Storage.sh [--development|--development-target <target>|--development-target-set <target+...>|--prepare-development-tools|--qualification-portable|--qualification-hosted|--qualification-step <step>]' >&2
    exit 64
fi

development_case_set=
development_bundle_set=
development_bundled_case_set=
if ((development == 0 || prepare_only == 1)); then
    selected_cases=0
    selected_executions=0
else
    development_plan=$(node \
        "$script_directory/Plan-Database-Storage-Development.mjs" \
        "$development_target") || exit $?
    IFS='|' read -r development_plan_format development_target \
        selected_cases selected_executions development_cases \
        development_bundles development_bundled_cases <<< "$development_plan"
    if [[ $development_plan_format != \
            windvale-database-storage-development-plan-2 ||
        ! $selected_cases =~ ^[0-9]+$ ||
        ! $selected_executions =~ ^[0-9]+$ ||
        -z $development_cases ]]; then
        echo 'The database development case plan is malformed.' >&2
        exit 1
    fi
    development_case_set=,$development_cases,
    if [[ $development_bundles != - ]]; then
        development_bundle_set=,$development_bundles,
    fi
    if [[ $development_bundled_cases != - ]]; then
        development_bundled_case_set=,$development_bundled_cases,
    fi
fi
progress_total=$((selected_executions + 1))
progress_current=1
qualification_step=0
qualification_steps=0
qualification_cases=0
qualification_prerequisites=0
qualification_portable_steps=0
qualification_hosted_steps=0
qualification_portable_cases=0
qualification_hosted_cases=0
if ((development == 0)); then
    qualification_counts=$(node \
        "$script_directory/Plan-Database-Storage-Qualification.mjs" \
        --counts) || exit $?
    IFS='|' read -r qualification_plan_format qualification_steps \
        qualification_cases qualification_prerequisites \
        qualification_portable_steps qualification_hosted_steps \
        qualification_portable_cases qualification_hosted_cases \
        <<< "$qualification_counts"
    if [[ $qualification_plan_format != \
            windvale-database-storage-qualification-counts-1 ||
        ! $qualification_steps =~ ^[0-9]+$ ||
        ! $qualification_cases =~ ^[0-9]+$ ||
        ! $qualification_prerequisites =~ ^[0-9]+$ ||
        ! $qualification_portable_steps =~ ^[0-9]+$ ||
        ! $qualification_hosted_steps =~ ^[0-9]+$ ||
        ! $qualification_portable_cases =~ ^[0-9]+$ ||
        ! $qualification_hosted_cases =~ ^[0-9]+$ ]]; then
        echo 'The database qualification count plan is malformed.' >&2
        exit 1
    fi
    if [[ $qualification_target == portable ]]; then
        qualification_steps=$qualification_portable_steps
    elif [[ $qualification_target == hosted ]]; then
        qualification_steps=$qualification_hosted_steps
    elif [[ -n $qualification_step_target ]]; then
        qualification_steps=0
    fi
fi

repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-database-storage.XXXXXXXX") || exit 1
hosted_application_session="$script_directory/Build-Cached-Hosted-Application-Session.mjs"
hosted_application_session_ready="$temporary_directory/Hosted-Application-Session.txt"
hosted_application_session_log="$temporary_directory/Hosted-Application-Session.log"
hosted_application_session_pid=
linked_image_set="$script_directory/Build-Cached-Linked-Image-Set.mjs"
cleanup() {
    local result=$?
    if [[ -n $hosted_application_session_pid ]]; then
        if [[ -f $hosted_application_session_ready ]]; then
            node "$hosted_application_session" shutdown \
                "$hosted_application_session_ready" >/dev/null 2>&1 || {
                    kill "$hosted_application_session_pid" >/dev/null 2>&1 || true
                    result=1
                }
        elif kill -0 "$hosted_application_session_pid" >/dev/null 2>&1; then
            kill "$hosted_application_session_pid" >/dev/null 2>&1 || true
            result=1
        fi
        wait "$hosted_application_session_pid" || result=1
    fi
    case "$temporary_directory" in
        "$temporary_root"/windvale-database-storage.*)
            rm -rf -- "$temporary_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $temporary_directory" >&2
            return 1
            ;;
    esac
    return "$result"
}
trap cleanup EXIT

build_driver="$repository_root/Artifacts/Native-Compiler-Reconstruction-Candidate/linux-x64/wvbuild.elf"
lowerer="$repository_root/Artifacts/Native-Wvb-To-Wvo-Candidate/Wvb-To-Wvo.elf"
workspace_path="$repository_root/Windvale.wvws"
project_checkpoint_host_storage=NotRun
project_checkpoint_host_root_writer=NotRun
project_checkpoint_host_local_service=NotRun
project_checkpoint_host_tree_reader=NotRun
project_checkpoint_host_tree_scan=NotRun
project_checkpoint_host_tree_delete=NotRun
project_checkpoint_engine=NotRun
project_checkpoint_host_tree_writer=NotRun
project_checkpoint_persistent_transaction_writer=NotRun
application_checkpoint_host_storage=NotRun
application_checkpoint_host_root_writer=NotRun
application_checkpoint_host_local_service=NotRun
application_checkpoint_host_tree_reader=NotRun
application_checkpoint_host_tree_scan=NotRun
application_checkpoint_host_tree_delete=NotRun
application_checkpoint_engine=NotRun
application_checkpoint_host_tree_writer=NotRun
application_checkpoint_persistent_transaction_writer=NotRun
project_wvb_checkpoint=NotRun
portable_project_checkpoints=
portable_application_checkpoints=
if ((development == 1)); then
    development_start=$SECONDS
    tools_start=$SECONDS
    echo "START native database storage development step=tools item=$progress_current/$progress_total target=$development_target"
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

build_cached_hosted_application() {
    node "$hosted_application_session" request \
        "$hosted_application_session_ready" "$@"
    local result=$?
    if [[ $result -ne 75 ]]; then
        return "$result"
    fi
    "$script_directory/Build-Cached-Hosted-Application.sh" "$@"
}

build_cached_project_object() {
    node "$hosted_application_session" project-request \
        "$hosted_application_session_ready" "$1" "$2" "$3"
    local result=$?
    if [[ $result -ne 75 ]]; then
        return "$result"
    fi
    "$script_directory/Build-Cached-Project-Object.sh" \
        "$1" "$build_driver" "$lowerer" "$2" "$3"
}

build_cached_linked_image_set() {
    local entry=$1 image=$2 map=$3
    shift 3
    node "$linked_image_set" 0 "$entry" "$image" "$map" "$#" "$@"
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

echo 'Progress: step=database-storage-tools item=1/2 detail=verify-build-driver'
verify_file "$build_driver" 30072832 \
    628fd60ea702c4a3b3ffb01d32cba7ba9708477acccf190cc6506a56f159d7a9 || exit $?
echo 'Progress: step=database-storage-tools item=2/2 detail=verify-lowerer'
verify_file "$lowerer" 10657792 \
    4f7aa0abdf870ada362defee6258ba4e6b8ce1f0f67329563d20ed3eb6c9ff24 || exit $?
tool_checkpoint=Retained
project_wvb_checkpoint=NotBuilt

if ((development == 1 && prepare_only == 0)); then
    echo 'Progress: step=database-storage-tools item=3/3 detail=start-hosted-session'
    node "$hosted_application_session" serve \
        "$hosted_application_session_ready" linux "$build_driver" "$lowerer" \
        >"$hosted_application_session_log" 2>&1 &
    hosted_application_session_pid=$!
    if ! node "$hosted_application_session" wait \
        "$hosted_application_session_ready" >/dev/null; then
        cat -- "$hosted_application_session_log" >&2
        exit 1
    fi
fi
if ((development == 1 && prepare_only == 1)); then
    echo 'Progress: step=database-storage-tools item=3/3 detail=tools-ready'
fi

if ((development == 1)); then
    tools_elapsed_ms=$(((SECONDS - tools_start) * 1000))
    echo "PASS  native database storage development step=tools item=$progress_current/$progress_total target=$development_target elapsed-ms=$tools_elapsed_ms tool=$tool_checkpoint project-wvb=$project_wvb_checkpoint"
fi

if ((prepare_only == 1)); then
    echo "native database storage development tools status=Passed checkpoint=$tool_checkpoint project-wvb=$project_wvb_checkpoint"
    exit 0
fi

start_qualification_step() {
    if ((development == 1)); then
        return 0
    fi
    qualification_step=$((qualification_step + 1))
    echo "Progress: step=database-storage-cases item=$qualification_step/$qualification_steps detail=$1"
}

verify_segmented_target() {
    start_qualification_step "$1"
    local label=$1 project_path=$2
    local first_wvb="$temporary_directory/$label-Segmented-First.wvb"
    local object_prefix="$temporary_directory/$label-Object"
    local object_manifest="$temporary_directory/$label-Object.wvop"
    local image_prefix="$temporary_directory/$label-Staged-Image"
    local image_manifest="$temporary_directory/$label-Staged-Image.wvli"
    local canonical_prefix="$temporary_directory/$label-Canonical"
    local canonical_manifest="$temporary_directory/$label-Canonical.wvli"
    local stage_report="$temporary_directory/$label-Stage.txt"
    local link_report="$temporary_directory/$label-Staged-Link.txt"
    local transport_report="$temporary_directory/$label-Transport.txt"
    local linux_application="$temporary_directory/$label-Segmented.elf"
    local windows_application="$temporary_directory/$label-Segmented.exe"
    local project_checkpoint=Rebuilt
    local link_checkpoint=Segmented
    local application_checkpoint=Rebuilt
    local target_start=$SECONDS

    local entry_offset fragment_count
    if ((development == 1)); then
        "$script_directory/Build-Cached-Segmented-Project.sh" \
            "$project_path" "$build_driver" "$first_wvb" \
            "$canonical_prefix" "$canonical_manifest" \
            > "$transport_report" || return $?
        project_checkpoint=$(sed -n \
            's/^native segmented project cache status=\([^ ]*\) key=[0-9a-f][0-9a-f]* entry-offset=[0-9][0-9]* fragments=[1-8]$/\1/p' \
            "$transport_report")
        entry_offset=$(sed -n \
            's/^native segmented project cache status=[^ ]* key=[0-9a-f][0-9a-f]* entry-offset=\([0-9][0-9]*\) fragments=[1-8]$/\1/p' \
            "$transport_report")
        fragment_count=$(sed -n \
            's/^native segmented project cache status=[^ ]* key=[0-9a-f][0-9a-f]* entry-offset=[0-9][0-9]* fragments=\([1-8]\)$/\1/p' \
            "$transport_report")
        [[ $project_checkpoint == Created || $project_checkpoint == Hit ]] || return 1
        link_checkpoint=$project_checkpoint
    else
        # Portable database behavior binds one admitted construction. Aggregate
        # qualification owns compiler construction reproducibility separately.
        "$build_driver" --workspace "$workspace_path" --project "$project_path" \
            "$first_wvb" >/dev/null || return $?
        "$script_directory/Stage-Compiler-Wvb.sh" \
            "$first_wvb" "$object_prefix" "$object_manifest" \
            > "$stage_report" || return $?
        "$script_directory/Link-Staged-Compiler-Wvo.sh" \
            "$object_prefix" "$object_manifest" "$image_prefix" "$image_manifest" \
            > "$link_report" || return $?
        "$script_directory/Transport-Compiler-Image.sh" \
            "$image_prefix" "$image_manifest" "$canonical_prefix" "$canonical_manifest" \
            > "$transport_report" || return $?
        entry_offset=$(sed -n \
            's/^compiler image transport status=Complete image-bytes=[0-9][0-9]* entry-offset=\([0-9][0-9]*\) chunks=[1-8] manifest-bytes=[0-9][0-9]*$/\1/p' \
            "$transport_report")
        fragment_count=$(sed -n \
            's/^compiler image transport status=Complete image-bytes=[0-9][0-9]* entry-offset=[0-9][0-9]* chunks=\([1-8]\) manifest-bytes=[0-9][0-9]*$/\1/p' \
            "$transport_report")
    fi
    case "$entry_offset" in ''|*[!0-9]*) return 1 ;; esac
    case "$fragment_count" in [1-8]) ;; *) return 1 ;; esac

    if ((development == 1)); then
        local application_cache_report="$temporary_directory/$label-Segmented-Application-Cache.txt"
        build_cached_hosted_application 6 \
            "$first_wvb" "$canonical_prefix" "$fragment_count" "$entry_offset" \
            "$linux_application" linux > "$application_cache_report" || return $?
        application_checkpoint=$(sed -n \
            's/^native hosted application cache status=\([^ ]*\) key=[0-9a-f][0-9a-f]* target=linux$/\1/p' \
            "$application_cache_report")
        [[ $application_checkpoint == Created ||
            $application_checkpoint == Hit ]] || return 1
    else
        "$script_directory/Package-Hosted-Wvb.sh" image 6 \
            "$first_wvb" "$canonical_prefix" "$fragment_count" "$entry_offset" \
            "$linux_application" linux >/dev/null || return $?
    fi
    "$linux_application" >/dev/null
    local application_result=$?
    if [[ $application_result -ne 0 ]]; then
        echo "The $label segmented database-storage case returned $application_result, expected 0." >&2
        return 1
    fi
    if ((development == 1)); then
        local target_elapsed_ms=$(((SECONDS - target_start) * 1000))
        echo "PASS  native database storage development step=portable-segmented-target item=$progress_current/$progress_total target=$development_target case=$label elapsed-ms=$target_elapsed_ms project=$project_checkpoint link=$link_checkpoint application=linux-$application_checkpoint"
        portable_project_checkpoints+="$label:$project_checkpoint/link-$link_checkpoint,"
        portable_application_checkpoints+="$label:linux-$application_checkpoint,"
    fi
}

verify_target() {
    start_qualification_step "$1"
    local label=$1 project_path=$2
    local case_names=${3:-$label}
    local first_wvb="$temporary_directory/$label-First.wvb"
    local first_wvo="$temporary_directory/$label-First.wvo"
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
        local project_cache_report="$temporary_directory/$label-Project-Cache.txt"
        build_cached_project_object \
            "$project_path" "$first_wvb" "$first_wvo" \
            > "$project_cache_report" || return $?
        project_checkpoint=$(sed -n \
            's/^native project object cache status=\([^ ]*\) key=[0-9a-f][0-9a-f]*$/\1/p' \
            "$project_cache_report")
        [[ $project_checkpoint == Created || $project_checkpoint == Hit ]] || return 1
    else
        # Portable database behavior binds one admitted construction. Aggregate
        # qualification owns compiler and lowerer reproducibility separately.
        "$build_driver" --workspace "$workspace_path" --project "$project_path" "$first_wvb" >/dev/null || return $?
        "$lowerer" "$first_wvb" "$first_wvo" >/dev/null || return $?
    fi
    if ((development == 0)); then
        "$script_directory/Check-Wvo.sh" "$first_wvo" >/dev/null || return $?
    fi

    if ((development == 1)); then
        local link_cache_report="$temporary_directory/$label-Link-Cache.txt"
        build_cached_linked_image_set Main "$image" "$map" "$first_wvo" \
            > "$link_cache_report" || return $?
        link_checkpoint=$(sed -n \
            's/^native linked image cache status=\([^ ]*\) key=[0-9a-f][0-9a-f]* entry=[0-9][0-9]* inputs=1$/\1/p' \
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
        build_cached_hosted_application 6 \
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
        echo "PASS  native database storage development step=portable-target item=$progress_current/$progress_total target=$development_target cases=$case_names elapsed-ms=$target_elapsed_ms project=$project_checkpoint link=$link_checkpoint application=linux-$linux_application_checkpoint"
        portable_project_checkpoints+="$label:$project_checkpoint/link-$link_checkpoint,"
        portable_application_checkpoints+="$label:linux-$linux_application_checkpoint,"
    fi
}

verify_storage_lowering() {
    start_qualification_step StorageLowering
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

build_qualification_project_object() {
    # Complete qualification owns compiler/lowerer reproducibility separately.
    # Hosted database cases construct once and retain structural admission.
    local project_path=$1 first_wvb=$2 first_wvo=$3
    echo 'Progress: step=database-hosted-construction phase=source-build'
    "$build_driver" --workspace "$workspace_path" --project "$project_path" "$first_wvb" >/dev/null || return $?
    echo 'Progress: step=database-hosted-construction phase=lower'
    "$lowerer" "$first_wvb" "$first_wvo" >/dev/null || return $?
    echo 'Progress: step=database-hosted-construction phase=admit'
    "$script_directory/Check-Wvo.sh" "$first_wvo" >/dev/null || return $?
}

verify_host_storage() {
    start_qualification_step HostStorage
    local project_path=$1
    local first_wvb="$temporary_directory/HostStorage-First.wvb"
    local first_wvo="$temporary_directory/HostStorage-First.wvo"
    local common_first="$temporary_directory/HostStorage-Common-First.wvo"
    local common_second="$temporary_directory/HostStorage-Common-Second.wvo"
    local linux_platform="$temporary_directory/HostStorage-Linux.wvo"
    local linux_image="$temporary_directory/HostStorage-Linux.bin"
    local linux_image_prefix="$temporary_directory/HostStorage-Linux-Image"
    local linux_map="$temporary_directory/HostStorage-Linux.map"
    local linux_application="$temporary_directory/HostStorage.elf"
    local run_directory="$temporary_directory/HostStorage-Run"
    local storage_file="$run_directory/Windvale-Database-Storage.bin"
    local initial_file="$run_directory/Windvale-Database-Storage.initial"
    local host_storage_checkpoint=Rebuilt
    local host_storage_application_checkpoint=Rebuilt

    if ((development == 1)); then
        local cache_report="$temporary_directory/HostStorage-Cache.txt"
        build_cached_project_object \
            "$project_path" "$first_wvb" "$first_wvo" \
            > "$cache_report" || return $?
        host_storage_checkpoint=$(sed -n \
            's/^native project object cache status=\([^ ]*\) key=[0-9a-f][0-9a-f]*$/\1/p' \
            "$cache_report")
        [[ $host_storage_checkpoint == Created || $host_storage_checkpoint == Hit ]] || return 1
    else
        build_qualification_project_object "$project_path" "$first_wvb" "$first_wvo" || return $?
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

    if ((development == 1)); then
        build_cached_linked_image_set Storage_host_entry \
            "$linux_image" "$linux_map" \
            "$first_wvo" "$common_first" "$linux_platform" || return $?
    else
        "$script_directory/Link-Wvo.sh" 0 Storage_host_entry \
            "$linux_image" "$first_wvo" "$common_first" "$linux_platform" \
            >"$linux_map" || return $?
    fi
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
        build_cached_hosted_application 6 \
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

    # Opposite-host construction belongs to the focused packager owners.
    return 0
}

verify_host_root_writer() {
    start_qualification_step HostRootWriter
    local project_path=$1
    local first_wvb="$temporary_directory/HostRootWriter-First.wvb"
    local first_wvo="$temporary_directory/HostRootWriter-First.wvo"
    local common="$temporary_directory/HostStorage-Common-First.wvo"
    local linux_platform="$temporary_directory/HostStorage-Linux.wvo"
    local linux_image="$temporary_directory/HostRootWriter-Linux.bin"
    local linux_image_prefix="$temporary_directory/HostRootWriter-Linux-Image"
    local linux_map="$temporary_directory/HostRootWriter-Linux.map"
    local linux_application="$temporary_directory/HostRootWriter.elf"
    local initial_file="$temporary_directory/HostStorage-Run/Windvale-Database-Storage.initial"
    local run_directory="$temporary_directory/HostRootWriter-Run"
    local storage_file="$run_directory/Windvale-Database-Storage.bin"
    local committed_file="$run_directory/Windvale-Database-Storage.committed"
    local host_root_writer_checkpoint=Rebuilt
    local host_root_writer_application_checkpoint=Rebuilt

    if ((development == 1)); then
        local cache_report="$temporary_directory/HostRootWriter-Cache.txt"
        build_cached_project_object \
            "$project_path" "$first_wvb" "$first_wvo" \
            > "$cache_report" || return $?
        host_root_writer_checkpoint=$(sed -n \
            's/^native project object cache status=\([^ ]*\) key=[0-9a-f][0-9a-f]*$/\1/p' \
            "$cache_report")
        [[ $host_root_writer_checkpoint == Created ||
            $host_root_writer_checkpoint == Hit ]] || return 1
    else
        build_qualification_project_object "$project_path" "$first_wvb" "$first_wvo" || return $?
    fi
    [[ -f $common && -f $linux_platform ]] || return 1

    if ((development == 1)); then
        build_cached_linked_image_set Storage_host_entry \
            "$linux_image" "$linux_map" \
            "$first_wvo" "$common" "$linux_platform" || return $?
    else
        "$script_directory/Link-Wvo.sh" 0 Storage_host_entry \
            "$linux_image" "$first_wvo" "$common" "$linux_platform" \
            >"$linux_map" || return $?
    fi
    local linux_entry
    linux_entry=$(sed -n \
        's/^entry name=Storage_host_entry address=\([0-9][0-9]*\)$/\1/p' \
        "$linux_map")
    case "$linux_entry" in
        ''|*[!0-9]*) return 1 ;;
    esac
    cp -- "$linux_image" "$linux_image_prefix.chunk-0" || return $?
    if ((development == 1)); then
        local application_cache_report="$temporary_directory/HostRootWriter-Application-Cache.txt"
        build_cached_hosted_application 6 \
            "$first_wvb" "$linux_image_prefix" 1 "$linux_entry" \
            "$linux_application" linux > "$application_cache_report" || return $?
        host_root_writer_application_checkpoint=$(sed -n \
            's/^native hosted application cache status=\([^ ]*\) key=[0-9a-f][0-9a-f]* target=linux$/\1/p' \
            "$application_cache_report")
        [[ $host_root_writer_application_checkpoint == Created ||
            $host_root_writer_application_checkpoint == Hit ]] || return 1
    else
        "$script_directory/Package-Hosted-Wvb.sh" image 6 \
            "$first_wvb" "$linux_image_prefix" 1 "$linux_entry" \
            "$linux_application" linux >/dev/null || return $?
    fi

    [[ -f $initial_file ]] || return 1
    mkdir -- "$run_directory" || return $?
    cp -- "$initial_file" "$storage_file" || return $?
    (cd -- "$run_directory" && "$linux_application" >/dev/null)
    local application_result=$?
    if [[ $application_result -ne 0 ]]; then
        echo "The native host root-writer publication returned $application_result, expected 0." >&2
        return 1
    fi
    [[ $(wc -c < "$storage_file") -eq 12800 ]] || return 1
    cp -- "$storage_file" "$committed_file" || return $?
    (cd -- "$run_directory" && "$linux_application" >/dev/null) || return $?
    cmp --silent -- "$committed_file" "$storage_file" || return 1
    local step
    for step in 0 1 2 3 4; do
        verify_host_root_writer_interruption \
            "$linux_application" "$initial_file" "$committed_file" "$step" \
            "$temporary_directory" || return $?
    done

    if ((development == 1)); then
        project_checkpoint_host_root_writer=$host_root_writer_checkpoint
        application_checkpoint_host_root_writer=$host_root_writer_application_checkpoint
        return 0
    fi
    # Opposite-host construction belongs to the focused packager owners.
    return 0
}

verify_host_root_writer_interruption() {
    local application=$1 initial=$2 committed=$3 step=$4 scenario_root=$5
    local scenario_directory="$scenario_root/HostRootWriter-Interruption-$step"
    local scenario_storage="$scenario_directory/Windvale-Database-Storage.bin"
    mkdir -- "$scenario_directory" || return $?
    cp -- "$initial" "$scenario_storage" || return $?
    truncate -s $((4609 + step)) -- "$scenario_storage" || return $?
    (cd -- "$scenario_directory" && "$application" >/dev/null)
    local application_result=$?
    local expected_result=$((100 + step))
    if [[ $application_result -ne $expected_result ]]; then
        echo "The native host root-writer interruption $step returned $application_result, expected $expected_result." >&2
        return 1
    fi
    (cd -- "$scenario_directory" && "$application" >/dev/null) || return $?
    [[ $(wc -c < "$scenario_storage") -eq 12800 ]] || return 1
    cmp --silent -- "$committed" "$scenario_storage" || return 1
}

verify_host_root_split_writer() {
    start_qualification_step HostRootSplitWriter
    local fill_project=$1 split_project=$2 get_project=$3
    local fill_application="$temporary_directory/HostLocal-RootFill.elf"
    local split_application="$temporary_directory/HostLocal-RootSplit.elf"
    local get_application="$temporary_directory/HostLocal-RootSplitGet.elf"
    local initial_file="$temporary_directory/HostStorage-Run/Windvale-Database-Storage.initial"
    local run_directory="$temporary_directory/HostRootSplit-Run"
    local storage_file="$run_directory/Windvale-Database-Storage.bin"
    local committed_file="$run_directory/Windvale-Database-Storage.committed"

    build_host_local_component "$fill_project" RootFill "$fill_application" || return $?
    local fill_project_checkpoint=$host_local_component_project_checkpoint
    local fill_application_checkpoint=$host_local_component_application_checkpoint
    build_host_local_component "$split_project" RootSplit "$split_application" || return $?
    local split_project_checkpoint=$host_local_component_project_checkpoint
    local split_application_checkpoint=$host_local_component_application_checkpoint
    build_host_local_component "$get_project" RootSplitGet "$get_application" || return $?
    local get_project_checkpoint=$host_local_component_project_checkpoint
    local get_application_checkpoint=$host_local_component_application_checkpoint

    [[ -f $initial_file ]] || return 1
    mkdir -- "$run_directory" || return $?
    cp -- "$initial_file" "$storage_file" || return $?
    (cd -- "$run_directory" && "$fill_application" >/dev/null) || return $?
    [[ $(wc -c < "$storage_file") -eq 12800 ]] || return 1
    (cd -- "$run_directory" && "$split_application" >/dev/null) || return $?
    [[ $(wc -c < "$storage_file") -eq 29184 ]] || return 1
    cp -- "$storage_file" "$committed_file" || return $?
    (cd -- "$run_directory" && "$get_application" >/dev/null) || return $?
    cmp --silent -- "$committed_file" "$storage_file" || return 1
    if ((development == 1)); then
        project_checkpoint_host_root_writer+=",Fill:$fill_project_checkpoint,Split:$split_project_checkpoint,Get:$get_project_checkpoint"
        application_checkpoint_host_root_writer+=",Fill:$fill_application_checkpoint,Split:$split_application_checkpoint,Get:$get_application_checkpoint"
    fi
}

build_host_local_component() {
    local project_path=$1 component=$2 linux_application=$3
    if [[ $component == LogicalTreePut ]]; then
        build_host_segmented_component "$project_path" "$component" "$linux_application"
        return $?
    fi
    local first_wvb="$temporary_directory/HostLocal-$component-First.wvb"
    local first_wvo="$temporary_directory/HostLocal-$component-First.wvo"
    local common="$temporary_directory/HostStorage-Common-First.wvo"
    local linux_platform="$temporary_directory/HostStorage-Linux.wvo"
    local linux_image="$temporary_directory/HostLocal-$component-Linux.bin"
    local linux_image_prefix="$temporary_directory/HostLocal-$component-Linux-Image"
    local linux_map="$temporary_directory/HostLocal-$component-Linux.map"
    host_local_component_project_checkpoint=Rebuilt
    host_local_component_application_checkpoint=Rebuilt

    if ((development == 1)); then
        local cache_report="$temporary_directory/HostLocal-$component-Cache.txt"
        build_cached_project_object \
            "$project_path" "$first_wvb" "$first_wvo" \
            > "$cache_report" || return $?
        host_local_component_project_checkpoint=$(sed -n \
            's/^native project object cache status=\([^ ]*\) key=[0-9a-f][0-9a-f]*$/\1/p' \
            "$cache_report")
        [[ $host_local_component_project_checkpoint == Created ||
            $host_local_component_project_checkpoint == Hit ]] || return 1
    else
        build_qualification_project_object "$project_path" "$first_wvb" "$first_wvo" || return $?
    fi
    [[ -f $common && -f $linux_platform ]] || return 1
    if ((development == 1)); then
        build_cached_linked_image_set Storage_host_entry \
            "$linux_image" "$linux_map" \
            "$first_wvo" "$common" "$linux_platform" || return $?
    else
        "$script_directory/Link-Wvo.sh" 0 Storage_host_entry \
            "$linux_image" "$first_wvo" "$common" "$linux_platform" \
            >"$linux_map" || return $?
    fi
    local linux_entry
    linux_entry=$(sed -n \
        's/^entry name=Storage_host_entry address=\([0-9][0-9]*\)$/\1/p' \
        "$linux_map")
    case "$linux_entry" in
        ''|*[!0-9]*) return 1 ;;
    esac
    cp -- "$linux_image" "$linux_image_prefix.chunk-0" || return $?
    if ((development == 1)); then
        local application_report="$temporary_directory/HostLocal-$component-Application-Cache.txt"
        build_cached_hosted_application 6 \
            "$first_wvb" "$linux_image_prefix" 1 "$linux_entry" \
            "$linux_application" linux > "$application_report" || return $?
        host_local_component_application_checkpoint=$(sed -n \
            's/^native hosted application cache status=\([^ ]*\) key=[0-9a-f][0-9a-f]* target=linux$/\1/p' \
            "$application_report")
        [[ $host_local_component_application_checkpoint == Created ||
            $host_local_component_application_checkpoint == Hit ]] || return 1
    else
        "$script_directory/Package-Hosted-Wvb.sh" image 6 \
            "$first_wvb" "$linux_image_prefix" 1 "$linux_entry" \
            "$linux_application" linux >/dev/null || return $?
    fi
}

build_host_segmented_component() {
    local project_path=$1 component=$2 linux_application=$3
    local first_wvb="$temporary_directory/HostLocal-$component-First.wvb"
    local object_prefix="$temporary_directory/HostLocal-$component-Object"
    local object_manifest="$temporary_directory/HostLocal-$component-Object.wvop"
    local staged_prefix="$temporary_directory/HostLocal-$component-Staged"
    local staged_manifest="$temporary_directory/HostLocal-$component-Staged.wvli"
    local canonical_prefix="$temporary_directory/HostLocal-$component-Canonical"
    local canonical_manifest="$temporary_directory/HostLocal-$component-Canonical.wvli"
    local common="$temporary_directory/HostStorage-Common-First.wvo"
    local linux_platform="$temporary_directory/HostStorage-Linux.wvo"
    local linux_prefix="$temporary_directory/HostLocal-$component-Linux-Image"
    local project_report="$temporary_directory/HostLocal-$component-Segmented-Cache.txt"
    local overlay_report="$temporary_directory/HostLocal-$component-Linux-Overlay.txt"

    host_local_component_project_checkpoint=Segmented
    if ((development == 1)); then
        "$script_directory/Build-Cached-Segmented-Project.sh" \
            "$project_path" "$build_driver" "$first_wvb" \
            "$canonical_prefix" "$canonical_manifest" \
            > "$project_report" || return $?
        host_local_component_project_checkpoint=$(sed -n \
            's/^native segmented project cache status=\([^ ]*\) key=[0-9a-f][0-9a-f]* entry-offset=[0-9][0-9]* fragments=[1-8]$/\1/p' \
            "$project_report")
        [[ $host_local_component_project_checkpoint == Created ||
            $host_local_component_project_checkpoint == Hit ]] || return 1
    else
        "$build_driver" --workspace "$workspace_path" --project "$project_path" \
            "$first_wvb" >/dev/null || return $?
        "$script_directory/Stage-Compiler-Wvb.sh" \
            "$first_wvb" "$object_prefix" "$object_manifest" >/dev/null || return $?
        "$script_directory/Link-Staged-Compiler-Wvo.sh" \
            "$object_prefix" "$object_manifest" "$staged_prefix" "$staged_manifest" \
            >/dev/null || return $?
        "$script_directory/Transport-Compiler-Image.sh" \
            "$staged_prefix" "$staged_manifest" "$canonical_prefix" "$canonical_manifest" \
            >/dev/null || return $?
    fi
    [[ -f $common && -f $linux_platform ]] || return 1
    "$script_directory/Compose-Segmented-Hosted-Overlay.sh" \
        "$canonical_prefix" "$canonical_manifest" "$common" "$linux_platform" \
        "$linux_prefix" >"$overlay_report" || return $?
    local fragment_count linux_entry
    fragment_count=$(sed -n 's/^segmented hosted overlay status=Valid .* fragments=\([1-8]\) .*$/\1/p' "$overlay_report")
    linux_entry=$(sed -n 's/^segmented hosted overlay status=Valid .* provider-entry=\([0-9][0-9]*\) .*$/\1/p' "$overlay_report")
    case "$fragment_count" in [1-8]) ;; *) return 1 ;; esac
    case "$linux_entry" in ''|*[!0-9]*) return 1 ;; esac
    host_local_component_application_checkpoint=Rebuilt

    if ((development == 1)); then
        local application_report="$temporary_directory/HostLocal-$component-Application-Cache.txt"
        build_cached_hosted_application 6 \
            "$first_wvb" "$linux_prefix" "$fragment_count" "$linux_entry" \
            "$linux_application" linux >"$application_report" || return $?
        host_local_component_application_checkpoint=$(sed -n \
            's/^native hosted application cache status=\([^ ]*\) key=[0-9a-f][0-9a-f]* target=linux$/\1/p' \
            "$application_report")
        [[ $host_local_component_application_checkpoint == Created ||
            $host_local_component_application_checkpoint == Hit ]] || return 1
    else
        "$script_directory/Package-Hosted-Wvb.sh" image 6 \
            "$first_wvb" "$linux_prefix" "$fragment_count" "$linux_entry" \
            "$linux_application" linux >/dev/null || return $?
    fi
}

verify_host_tree_delete_interruption() {
    local application=$1 committed=$2 step=$3
    local scenario_directory="$temporary_directory/HostTreeDelete-Interruption-$step"
    local scenario_storage="$scenario_directory/Windvale-Database-Storage.bin"
    mkdir -- "$scenario_directory" || return $?
    cp -- "$committed" "$scenario_storage" || return $?
    truncate -s $((20993 + step)) -- "$scenario_storage" || return $?
    (cd -- "$scenario_directory" && "$application" >/dev/null)
    local application_result=$?
    local expected_result=$((110 + step))
    if [[ $application_result -ne $expected_result ]]; then
        echo "The native host tree-delete interruption $step returned $application_result, expected $expected_result." >&2
        return 1
    fi
    (cd -- "$scenario_directory" && "$application" >/dev/null) || return $?
    [[ $(wc -c < "$scenario_storage") -eq 33280 ]] || return 1
}

verify_host_tree_delete() {
    start_qualification_step HostTreeDelete
    local project_path=$1
    local application="$temporary_directory/HostLocal-TreeDelete.elf"
    local depth_two_committed_file="$temporary_directory/HostTreeReader-Run/Windvale-Database-Storage.depth-two"
    local run_directory="$temporary_directory/HostTreeDelete-Run"
    local storage_file="$run_directory/Windvale-Database-Storage.bin"
    local committed_file="$run_directory/Windvale-Database-Storage.committed"

    build_host_local_component "$project_path" TreeDelete "$application" || return $?
    project_checkpoint_host_tree_delete=$host_local_component_project_checkpoint
    application_checkpoint_host_tree_delete=$host_local_component_application_checkpoint
    [[ -f $depth_two_committed_file ]] || return 1
    mkdir -- "$run_directory" || return $?
    cp -- "$depth_two_committed_file" "$storage_file" || return $?
    (cd -- "$run_directory" && "$application" >/dev/null)
    local application_result=$?
    if [[ $application_result -ne 0 ]]; then
        echo "The native host tree-delete publication returned $application_result, expected 0." >&2
        return 1
    fi
    [[ $(wc -c < "$storage_file") -eq 33280 ]] || return 1
    cp -- "$storage_file" "$committed_file" || return $?
    (cd -- "$run_directory" && "$application" >/dev/null) || return $?
    cmp --silent -- "$committed_file" "$storage_file" || return 1
    local step
    for step in 0 1 2 3 4; do
        verify_host_tree_delete_interruption \
            "$application" "$depth_two_committed_file" "$step" || return $?
    done
}

verify_host_tree_scan() {
    start_qualification_step HostTreeScan
    local project_path=$1
    local application="$temporary_directory/HostLocal-TreeScan.elf"
    local depth_two_committed_file="$temporary_directory/HostTreeReader-Run/Windvale-Database-Storage.depth-two"
    local updated_committed_file="$temporary_directory/HostTreeReader-Run/Windvale-Database-Storage.committed"
    local run_directory="$temporary_directory/HostTreeScan-Run"
    local storage_file="$run_directory/Windvale-Database-Storage.bin"
    local before_file="$run_directory/Windvale-Database-Storage.before"

    build_host_local_component "$project_path" TreeScan "$application" || return $?
    project_checkpoint_host_tree_scan=$host_local_component_project_checkpoint
    application_checkpoint_host_tree_scan=$host_local_component_application_checkpoint
    [[ -f $depth_two_committed_file && -f $updated_committed_file ]] || return 1
    mkdir -- "$run_directory" || return $?

    cp -- "$depth_two_committed_file" "$storage_file" || return $?
    cp -- "$storage_file" "$before_file" || return $?
    (cd -- "$run_directory" && "$application" >/dev/null)
    local application_result=$?
    if [[ $application_result -ne 0 ]]; then
        echo "The native host tree-scan committed-generation run returned $application_result, expected 0." >&2
        return 1
    fi
    cmp --silent -- "$before_file" "$storage_file" || return 1

    cp -- "$updated_committed_file" "$storage_file" || return $?
    cp -- "$storage_file" "$before_file" || return $?
    (cd -- "$run_directory" && "$application" >/dev/null)
    application_result=$?
    if [[ $application_result -ne 0 ]]; then
        echo "The native host tree-scan updated-generation run returned $application_result, expected 0." >&2
        return 1
    fi
    cmp --silent -- "$before_file" "$storage_file" || return 1
}

verify_host_local_service() {
    start_qualification_step HostLocalService
    local put_project=$1 get_project=$2
    local put_application="$temporary_directory/HostLocal-Put.elf"
    local get_application="$temporary_directory/HostLocal-Get.elf"
    local initial_file="$temporary_directory/HostStorage-Run/Windvale-Database-Storage.initial"
    local run_directory="$temporary_directory/HostLocalService-Run"
    local storage_file="$run_directory/Windvale-Database-Storage.bin"
    local committed_file="$run_directory/Windvale-Database-Storage.committed"

    build_host_local_component "$put_project" Put "$put_application" || return $?
    local put_project_checkpoint=$host_local_component_project_checkpoint
    local put_application_checkpoint=$host_local_component_application_checkpoint
    build_host_local_component "$get_project" Get "$get_application" || return $?
    local get_project_checkpoint=$host_local_component_project_checkpoint
    local get_application_checkpoint=$host_local_component_application_checkpoint

    [[ -f $initial_file ]] || return 1
    mkdir -- "$run_directory" || return $?
    cp -- "$initial_file" "$storage_file" || return $?
    (cd -- "$run_directory" && "$put_application" >/dev/null)
    local application_result=$?
    if [[ $application_result -ne 0 ]]; then
        echo "The native local-service put returned $application_result, expected 0." >&2
        return 1
    fi
    [[ $(wc -c < "$storage_file") -eq 12800 ]] || return 1
    cp -- "$storage_file" "$committed_file" || return $?
    (cd -- "$run_directory" && "$get_application" >/dev/null)
    application_result=$?
    if [[ $application_result -ne 0 ]]; then
        echo "The native local-service restart get returned $application_result, expected 0." >&2
        return 1
    fi
    cmp --silent -- "$committed_file" "$storage_file" || return 1
    if ((development == 1)); then
        project_checkpoint_host_local_service="Put:$put_project_checkpoint,Get:$get_project_checkpoint"
        application_checkpoint_host_local_service="Put:$put_application_checkpoint,Get:$get_application_checkpoint"
    fi
}

verify_host_tree_reader() {
    start_qualification_step HostTreeReader
    local project_path=$1
    local first_wvb="$temporary_directory/HostTreeReader-First.wvb"
    local first_wvo="$temporary_directory/HostTreeReader-First.wvo"
    local common="$temporary_directory/HostStorage-Common-First.wvo"
    local linux_platform="$temporary_directory/HostStorage-Linux.wvo"
    local linux_image="$temporary_directory/HostTreeReader-Linux.bin"
    local linux_image_prefix="$temporary_directory/HostTreeReader-Linux-Image"
    local linux_map="$temporary_directory/HostTreeReader-Linux.map"
    local linux_application="$temporary_directory/HostTreeReader.elf"
    local initial_file="$temporary_directory/HostStorage-Run/Windvale-Database-Storage.initial"
    local run_directory="$temporary_directory/HostTreeReader-Run"
    local storage_file="$run_directory/Windvale-Database-Storage.bin"
    local depth_two_committed_file="$run_directory/Windvale-Database-Storage.depth-two"
    local committed_file="$run_directory/Windvale-Database-Storage.committed"
    local host_tree_reader_checkpoint=Rebuilt
    local host_tree_reader_application_checkpoint=Rebuilt

    if ((development == 1)); then
        local cache_report="$temporary_directory/HostTreeReader-Cache.txt"
        build_cached_project_object \
            "$project_path" "$first_wvb" "$first_wvo" \
            > "$cache_report" || return $?
        host_tree_reader_checkpoint=$(sed -n \
            's/^native project object cache status=\([^ ]*\) key=[0-9a-f][0-9a-f]*$/\1/p' \
            "$cache_report")
        [[ $host_tree_reader_checkpoint == Created || $host_tree_reader_checkpoint == Hit ]] || return 1
    else
        build_qualification_project_object "$project_path" "$first_wvb" "$first_wvo" || return $?
    fi
    [[ -f $common && -f $linux_platform ]] || return 1

    if ((development == 1)); then
        build_cached_linked_image_set Storage_host_entry \
            "$linux_image" "$linux_map" \
            "$first_wvo" "$common" "$linux_platform" || return $?
    else
        "$script_directory/Link-Wvo.sh" 0 Storage_host_entry \
            "$linux_image" "$first_wvo" "$common" "$linux_platform" \
            >"$linux_map" || return $?
    fi
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
        build_cached_hosted_application 6 \
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
    # Opposite-host construction belongs to the focused packager owners.
    return 0
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

verify_host_engine() {
    start_qualification_step HostEngine
    local project_path=$1
    local first_wvb="$temporary_directory/Engine-First.wvb"
    local first_wvo="$temporary_directory/Engine-First.wvo"
    local common="$temporary_directory/HostStorage-Common-First.wvo"
    local linux_platform="$temporary_directory/HostStorage-Linux.wvo"
    local linux_image="$temporary_directory/Engine-Linux.bin"
    local linux_image_prefix="$temporary_directory/Engine-Linux-Image"
    local linux_map="$temporary_directory/Engine-Linux.map"
    local linux_application="$temporary_directory/Engine.elf"
    local depth_two_committed_file="$temporary_directory/HostTreeReader-Run/Windvale-Database-Storage.depth-two"
    local run_directory="$temporary_directory/Engine-Run"
    local storage_file="$run_directory/Windvale-Database-Storage.bin"
    local create_directory="$temporary_directory/Engine-Create"
    local create_storage="$create_directory/Windvale-Database-Storage.bin"
    local create_snapshot="$create_directory/Windvale-Database-Storage.created"
    local engine_checkpoint=Rebuilt
    local engine_application_checkpoint=Rebuilt

    if ((development == 1)); then
        local cache_report="$temporary_directory/Engine-Cache.txt"
        build_cached_project_object \
            "$project_path" "$first_wvb" "$first_wvo" \
            > "$cache_report" || return $?
        engine_checkpoint=$(sed -n \
            's/^native project object cache status=\([^ ]*\) key=[0-9a-f][0-9a-f]*$/\1/p' \
            "$cache_report")
        [[ $engine_checkpoint == Created || $engine_checkpoint == Hit ]] || return 1
    else
        build_qualification_project_object "$project_path" "$first_wvb" "$first_wvo" || return $?
    fi
    [[ -f $common && -f $linux_platform ]] || return 1

    if ((development == 1)); then
        build_cached_linked_image_set Storage_host_entry \
            "$linux_image" "$linux_map" \
            "$first_wvo" "$common" "$linux_platform" || return $?
    else
        "$script_directory/Link-Wvo.sh" 0 Storage_host_entry \
            "$linux_image" "$first_wvo" "$common" "$linux_platform" \
            >"$linux_map" || return $?
    fi
    local linux_entry
    linux_entry=$(sed -n \
        's/^entry name=Storage_host_entry address=\([0-9][0-9]*\)$/\1/p' \
        "$linux_map")
    case "$linux_entry" in
        ''|*[!0-9]*) return 1 ;;
    esac
    cp -- "$linux_image" "$linux_image_prefix.chunk-0" || return $?
    if ((development == 1)); then
        local application_cache_report="$temporary_directory/Engine-Application-Cache.txt"
        build_cached_hosted_application 6 \
            "$first_wvb" "$linux_image_prefix" 1 "$linux_entry" \
            "$linux_application" linux > "$application_cache_report" || return $?
        engine_application_checkpoint=$(sed -n \
            's/^native hosted application cache status=\([^ ]*\) key=[0-9a-f][0-9a-f]* target=linux$/\1/p' \
            "$application_cache_report")
        [[ $engine_application_checkpoint == Created ||
            $engine_application_checkpoint == Hit ]] || return 1
    else
        "$script_directory/Package-Hosted-Wvb.sh" image 6 \
            "$first_wvb" "$linux_image_prefix" 1 "$linux_entry" \
            "$linux_application" linux >/dev/null || return $?
    fi

    mkdir -- "$create_directory" || return $?
    (cd -- "$create_directory" && "$linux_application" >/dev/null)
    local create_result=$?
    if [[ $create_result -ne 70 ]]; then
        echo "The native engine fresh lifecycle returned $create_result, expected 70." >&2
        return 1
    fi
    [[ $(wc -c < "$create_storage") -eq 4608 ]] || return 1
    cp -- "$create_storage" "$create_snapshot" || return $?
    (cd -- "$create_directory" && "$linux_application" >/dev/null)
    create_result=$?
    if [[ $create_result -ne 71 ]]; then
        echo "The native engine initial reopen returned $create_result, expected 71." >&2
        return 1
    fi
    cmp --silent -- "$create_snapshot" "$create_storage" || return 1

    [[ -f $depth_two_committed_file ]] || return 1
    mkdir -- "$run_directory" || return $?
    cp -- "$depth_two_committed_file" "$storage_file" || return $?
    (cd -- "$run_directory" && "$linux_application" >/dev/null) || return $?
    cmp --silent -- "$depth_two_committed_file" "$storage_file" || return 1
    local step
    for step in 0 1 2; do
        verify_host_engine_recovery \
            "$linux_application" "$depth_two_committed_file" "$step" \
            "$temporary_directory" || return $?
    done
    local invalid_directory="$temporary_directory/Engine-Invalid-Header"
    local invalid_storage="$invalid_directory/Windvale-Database-Storage.bin"
    mkdir -- "$invalid_directory" || return $?
    cp -- "$depth_two_committed_file" "$invalid_storage" || return $?
    truncate -s 511 -- "$invalid_storage" || return $?
    (cd -- "$invalid_directory" && "$linux_application" >/dev/null)
    [[ $? -eq 91 ]] || return 1

    if ((development == 1)); then
        project_checkpoint_engine=$engine_checkpoint
        application_checkpoint_engine=$engine_application_checkpoint
        return 0
    fi
    # Opposite-host construction belongs to the focused packager owners.
    return 0
}

verify_host_engine_recovery() {
    local application=$1 committed=$2 step=$3 scenario_root=$4
    local scenario_directory="$scenario_root/Engine-Recovery-$step"
    local scenario_storage="$scenario_directory/Windvale-Database-Storage.bin"
    mkdir -- "$scenario_directory" || return $?
    cp -- "$committed" "$scenario_storage" || return $?
    truncate -s $((20993 + step)) -- "$scenario_storage" || return $?
    (cd -- "$scenario_directory" && "$application" >/dev/null)
    local application_result=$?
    local expected_result=0
    if [[ $step -eq 0 ]]; then expected_result=100; fi
    if [[ $step -eq 1 ]]; then expected_result=101; fi
    [[ $application_result -eq $expected_result ]] || return 1
    if [[ $step -eq 0 ]]; then
        [[ $(wc -c < "$scenario_storage") -eq 20993 ]] || return 1
    else
        [[ $(wc -c < "$scenario_storage") -eq 20992 ]] || return 1
        cmp --silent -- "$committed" "$scenario_storage" || return 1
    fi
}

verify_host_tree_writer() {
    start_qualification_step HostTreeWriter
    local project_path=$1
    local linux_application="$temporary_directory/HostTreeWriter.elf"
    local depth_two_committed_file="$temporary_directory/HostTreeReader-Run/Windvale-Database-Storage.depth-two"
    local run_directory="$temporary_directory/HostTreeWriter-Run"
    local storage_file="$run_directory/Windvale-Database-Storage.bin"
    local committed_file="$run_directory/Windvale-Database-Storage.committed"
    local host_tree_writer_checkpoint=Rebuilt
    local host_tree_writer_application_checkpoint=Rebuilt

    build_host_segmented_component "$project_path" HostTreeWriter "$linux_application" || return $?
    host_tree_writer_checkpoint=$host_local_component_project_checkpoint
    host_tree_writer_application_checkpoint=$host_local_component_application_checkpoint

    [[ -f $depth_two_committed_file ]] || return 1
    mkdir -- "$run_directory" || return $?
    cp -- "$depth_two_committed_file" "$storage_file" || return $?
    (cd -- "$run_directory" && "$linux_application" >/dev/null)
    local application_result=$?
    if [[ $application_result -ne 0 ]]; then
        echo "The native host tree-writer publication returned $application_result, expected 0." >&2
        return 1
    fi
    [[ $(wc -c < "$storage_file") -eq 33280 ]] || return 1
    cp -- "$storage_file" "$committed_file" || return $?
    (cd -- "$run_directory" && "$linux_application" >/dev/null) || return $?
    cmp --silent -- "$committed_file" "$storage_file" || return 1
    local step
    for step in 0 1 2 3 4; do
        verify_host_tree_writer_interruption \
            "$linux_application" "$depth_two_committed_file" "$step" \
            "$temporary_directory" || return $?
    done

    if ((development == 1)); then
        project_checkpoint_host_tree_writer=$host_tree_writer_checkpoint
        application_checkpoint_host_tree_writer=$host_tree_writer_application_checkpoint
        return 0
    fi
}

verify_host_logical_tree_writer() {
    start_qualification_step HostLogicalTreeWriter
    local put_project=$1 get_project=$2
    local put_application="$temporary_directory/HostLocal-LogicalTreePut.elf"
    local get_application="$temporary_directory/HostLocal-LogicalTreeGet.elf"
    local depth_two_committed_file="$temporary_directory/HostTreeReader-Run/Windvale-Database-Storage.depth-two"
    local run_directory="$temporary_directory/HostLogicalTree-Run"
    local storage_file="$run_directory/Windvale-Database-Storage.bin"
    local committed_file="$run_directory/Windvale-Database-Storage.committed"

    build_host_local_component "$put_project" LogicalTreePut "$put_application" || return $?
    local put_project_checkpoint=$host_local_component_project_checkpoint
    local put_application_checkpoint=$host_local_component_application_checkpoint
    build_host_local_component "$get_project" LogicalTreeGet "$get_application" || return $?
    local get_project_checkpoint=$host_local_component_project_checkpoint
    local get_application_checkpoint=$host_local_component_application_checkpoint

    [[ -f $depth_two_committed_file ]] || return 1
    mkdir -- "$run_directory" || return $?
    cp -- "$depth_two_committed_file" "$storage_file" || return $?
    (cd -- "$run_directory" && "$put_application" >/dev/null)
    local application_result=$?
    if [[ $application_result -ne 0 ]]; then
        echo "The native logical tree-writer put returned $application_result, expected 0." >&2
        return 1
    fi
    [[ $(wc -c < "$storage_file") -eq 33280 ]] || return 1
    cp -- "$storage_file" "$committed_file" || return $?
    (cd -- "$run_directory" && "$get_application" >/dev/null)
    application_result=$?
    if [[ $application_result -ne 0 ]]; then
        echo "The native logical tree-writer restart get returned $application_result, expected 0." >&2
        return 1
    fi
    cmp --silent -- "$committed_file" "$storage_file" || return 1
    if ((development == 1)); then
        project_checkpoint_host_tree_writer+=",LogicalPut:$put_project_checkpoint,LogicalGet:$get_project_checkpoint"
        application_checkpoint_host_tree_writer+=",LogicalPut:$put_application_checkpoint,LogicalGet:$get_application_checkpoint"
    fi
}

verify_host_tree_writer_interruption() {
    local application=$1 committed=$2 step=$3 scenario_root=$4
    local scenario_directory="$scenario_root/HostTreeWriter-Interruption-$step"
    local scenario_storage="$scenario_directory/Windvale-Database-Storage.bin"
    mkdir -- "$scenario_directory" || return $?
    cp -- "$committed" "$scenario_storage" || return $?
    truncate -s $((20993 + step)) -- "$scenario_storage" || return $?
    (cd -- "$scenario_directory" && "$application" >/dev/null)
    local application_result=$?
    local expected_result=$((110 + step))
    if [[ $application_result -ne $expected_result ]]; then
        echo "The native host tree-writer interruption $step returned $application_result, expected $expected_result." >&2
        return 1
    fi
    (cd -- "$scenario_directory" && "$application" >/dev/null) || return $?
    [[ $(wc -c < "$scenario_storage") -eq 33280 ]] || return 1
}

verify_persistent_transaction_writer() {
    start_qualification_step PersistentTransactionWriter
    local project_path=$1
    local linux_application="$temporary_directory/PersistentTransactionWriter.elf"
    local depth_two_committed_file="$temporary_directory/HostTreeReader-Run/Windvale-Database-Storage.depth-two"
    local run_directory="$temporary_directory/PersistentTransactionWriter-Run"
    local storage_file="$run_directory/Windvale-Database-Storage.bin"
    local writer_project_checkpoint=Segmented
    local writer_application_checkpoint=Rebuilt

    build_host_segmented_component \
        "$project_path" PersistentTransactionWriter "$linux_application" || return $?
    writer_project_checkpoint=$host_local_component_project_checkpoint
    writer_application_checkpoint=$host_local_component_application_checkpoint

    [[ -f $depth_two_committed_file ]] || return 1
    mkdir -- "$run_directory" || return $?
    cp -- "$depth_two_committed_file" "$storage_file" || return $?
    (cd -- "$run_directory" && "$linux_application" >/dev/null)
    local application_result=$?
    if [[ $application_result -ne 0 ]]; then
        echo "The native persistent transaction-writer returned $application_result, expected 0." >&2
        return 1
    fi
    [[ $(wc -c < "$storage_file") -gt $(wc -c < "$depth_two_committed_file") ]] || return 1
    if ((development == 1)); then
        project_checkpoint_persistent_transaction_writer=$writer_project_checkpoint
        application_checkpoint_persistent_transaction_writer=$writer_application_checkpoint
    fi
}

verify_development_target() {
    local label=$1 target=$2 project=$3
    if ! development_case_selected "$label"; then
        return 0
    fi
    if development_case_bundled "$label"; then
        return 0
    fi
    progress_current=$((progress_current + 1))
    echo "START native database storage development step=$target item=$progress_current/$progress_total target=$development_target"
    verify_target "$label" "$project" || {
        echo "The native database storage development $target stage failed." >&2
        return 1
    }
}

verify_development_bundle() {
    local label=$1 target=$2 project=$3 case_names=$4 handler=${5:-project}
    if ! development_bundle_selected "$label"; then
        return 0
    fi
    progress_current=$((progress_current + 1))
    echo "START native database storage development step=$target item=$progress_current/$progress_total target=$development_target cases=$case_names"
    local verifier
    case "$handler" in
        project) verifier=verify_target ;;
        segmented-project) verifier=verify_segmented_target ;;
        *) echo "Unknown database bundle handler: $handler" >&2; return 1 ;;
    esac
    "$verifier" "$label" "$project" "$case_names" || {
        echo "The native database storage development $target bundle stage failed." >&2
        return 1
    }
}

verify_segmented_development_target() {
    local label=$1 target=$2 project=$3
    if ! development_case_selected "$label"; then
        return 0
    fi
    progress_current=$((progress_current + 1))
    echo "START native database storage development step=$target item=$progress_current/$progress_total target=$development_target"
    verify_segmented_target "$label" "$project" || {
        echo "The native database storage development $target segmented stage failed." >&2
        return 1
    }
}

development_case_selected() {
    [[ $development_case_set == *,$1,* ]]
}

development_case_bundled() {
    [[ $development_bundled_case_set == *,$1,* ]]
}

development_bundle_selected() {
    [[ $development_bundle_set == *,$1,* ]]
}

verify_development_host_targets() {
    host_storage_elapsed_ms=0
    host_root_writer_elapsed_ms=0
    host_local_service_elapsed_ms=0
    host_tree_reader_elapsed_ms=0
    host_tree_delete_elapsed_ms=0
    host_tree_scan_elapsed_ms=0
    engine_elapsed_ms=0
    host_tree_writer_elapsed_ms=0
    persistent_transaction_writer_elapsed_ms=0
    if ! development_case_selected HostStorage; then return 0; fi

    progress_current=$((progress_current + 1))
    host_storage_start=$SECONDS
    echo "START native database storage development step=host-storage item=$progress_current/$progress_total target=$development_target"
    verify_host_storage \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Host-Storage.wvproj" || {
            echo 'The native database storage development host-storage stage failed.' >&2
            return 1
        }
    host_storage_elapsed_ms=$(((SECONDS - host_storage_start) * 1000))
    echo "PASS  native database storage development step=host-storage item=$progress_current/$progress_total target=$development_target elapsed-ms=$host_storage_elapsed_ms project=$project_checkpoint_host_storage application=$application_checkpoint_host_storage"
    if development_case_selected HostRootWriter; then
        progress_current=$((progress_current + 1))
        host_root_writer_start=$SECONDS
        echo "START native database storage development step=host-root-writer item=$progress_current/$progress_total target=$development_target"
        verify_host_root_writer \
            "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Host-Root-Writer.wvproj" || {
                echo 'The native database storage development host-root-writer stage failed.' >&2
                return 1
            }
        verify_host_root_split_writer \
            "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Host-Root-Fill.wvproj" \
            "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Host-Root-Split-Writer.wvproj" \
            "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Host-Logical-Tree-Get.wvproj" || {
                echo 'The native database storage development root-split writer stage failed.' >&2
                return 1
            }
        host_root_writer_elapsed_ms=$(((SECONDS - host_root_writer_start) * 1000))
        echo "PASS  native database storage development step=host-root-writer item=$progress_current/$progress_total target=$development_target elapsed-ms=$host_root_writer_elapsed_ms project=$project_checkpoint_host_root_writer application=$application_checkpoint_host_root_writer"
    fi

    if development_case_selected HostLocalService; then
        progress_current=$((progress_current + 1))
        host_local_service_start=$SECONDS
        echo "START native database storage development step=host-local-service item=$progress_current/$progress_total target=$development_target"
        verify_host_local_service \
            "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Host-Local-Put.wvproj" \
            "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Host-Local-Get.wvproj" || {
                echo 'The native database storage development host-local-service stage failed.' >&2
                return 1
            }
        host_local_service_elapsed_ms=$(((SECONDS - host_local_service_start) * 1000))
        echo "PASS  native database storage development step=host-local-service item=$progress_current/$progress_total target=$development_target elapsed-ms=$host_local_service_elapsed_ms project=$project_checkpoint_host_local_service application=$application_checkpoint_host_local_service"
    fi

    if development_case_selected HostTreeReader; then
        progress_current=$((progress_current + 1))
        host_tree_reader_start=$SECONDS
        echo "START native database storage development step=host-tree-reader item=$progress_current/$progress_total target=$development_target"
        verify_host_tree_reader \
            "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Host-Tree-Reader.wvproj" || {
                echo 'The native database storage development host-tree-reader stage failed.' >&2
                return 1
            }
        host_tree_reader_elapsed_ms=$(((SECONDS - host_tree_reader_start) * 1000))
        echo "PASS  native database storage development step=host-tree-reader item=$progress_current/$progress_total target=$development_target elapsed-ms=$host_tree_reader_elapsed_ms project=$project_checkpoint_host_tree_reader application=$application_checkpoint_host_tree_reader"
    fi

    if development_case_selected HostTreeDelete; then
        progress_current=$((progress_current + 1))
        host_tree_delete_start=$SECONDS
        echo "START native database storage development step=host-tree-delete item=$progress_current/$progress_total target=$development_target"
        verify_host_tree_delete \
            "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Host-Tree-Delete.wvproj" || {
                echo 'The native database storage development host-tree-delete stage failed.' >&2
                return 1
            }
        host_tree_delete_elapsed_ms=$(((SECONDS - host_tree_delete_start) * 1000))
        echo "PASS  native database storage development step=host-tree-delete item=$progress_current/$progress_total target=$development_target elapsed-ms=$host_tree_delete_elapsed_ms project=$project_checkpoint_host_tree_delete application=$application_checkpoint_host_tree_delete"
    fi

    if development_case_selected HostTreeScan; then
        progress_current=$((progress_current + 1))
        host_tree_scan_start=$SECONDS
        echo "START native database storage development step=host-tree-scan item=$progress_current/$progress_total target=$development_target"
        verify_host_tree_scan \
            "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Host-Tree-Scan.wvproj" || {
                echo 'The native database storage development host-tree-scan stage failed.' >&2
                return 1
            }
        host_tree_scan_elapsed_ms=$(((SECONDS - host_tree_scan_start) * 1000))
        echo "PASS  native database storage development step=host-tree-scan item=$progress_current/$progress_total target=$development_target elapsed-ms=$host_tree_scan_elapsed_ms project=$project_checkpoint_host_tree_scan application=$application_checkpoint_host_tree_scan"
    fi

    if development_case_selected Engine; then
        progress_current=$((progress_current + 1))
        engine_start=$SECONDS
        echo "START native database storage development step=engine item=$progress_current/$progress_total target=$development_target"
        verify_host_engine \
            "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Engine.wvproj" || {
                echo 'The native database storage development engine stage failed.' >&2
                return 1
            }
        engine_elapsed_ms=$(((SECONDS - engine_start) * 1000))
        echo "PASS  native database storage development step=engine item=$progress_current/$progress_total target=$development_target elapsed-ms=$engine_elapsed_ms project=$project_checkpoint_engine application=$application_checkpoint_engine"
    fi

    if development_case_selected HostTreeWriter; then
        progress_current=$((progress_current + 1))
        host_tree_writer_start=$SECONDS
        echo "START native database storage development step=host-tree-writer item=$progress_current/$progress_total target=$development_target"
        verify_host_tree_writer \
            "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Host-Tree-Writer.wvproj" || {
                echo 'The native database storage development host-tree-writer stage failed.' >&2
                return 1
            }
        verify_host_logical_tree_writer \
            "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Host-Logical-Tree-Writer.wvproj" \
            "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Host-Logical-Tree-Get.wvproj" || {
                echo 'The native database storage development logical tree-writer stage failed.' >&2
                return 1
            }
        host_tree_writer_elapsed_ms=$(((SECONDS - host_tree_writer_start) * 1000))
        echo "PASS  native database storage development step=host-tree-writer item=$progress_current/$progress_total target=$development_target elapsed-ms=$host_tree_writer_elapsed_ms project=$project_checkpoint_host_tree_writer application=$application_checkpoint_host_tree_writer"
    fi

    if development_case_selected PersistentTransactionWriter; then
        progress_current=$((progress_current + 1))
        persistent_transaction_writer_start=$SECONDS
        echo "START native database storage development step=persistent-transaction-writer item=$progress_current/$progress_total target=$development_target"
        verify_persistent_transaction_writer \
            "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Persistent-Transaction-Writer.wvproj" || {
                echo 'The native database storage development persistent transaction-writer stage failed.' >&2
                return 1
            }
        persistent_transaction_writer_elapsed_ms=$(((SECONDS - persistent_transaction_writer_start) * 1000))
        echo "PASS  native database storage development step=persistent-transaction-writer item=$progress_current/$progress_total target=$development_target elapsed-ms=$persistent_transaction_writer_elapsed_ms project=$project_checkpoint_persistent_transaction_writer application=$application_checkpoint_persistent_transaction_writer"
    fi
}

dispatch_qualification_step() {
    local label=$1
    local handler=$2
    local project_1="$repository_root/$3"
    local project_2="$repository_root/$4"
    local project_3="$repository_root/$5"
    local cases=$6
    local started=$SECONDS
    case "$handler" in
        project)
            verify_target "$label" "$project_1" || return $?
            ;;
        segmented-project)
            verify_segmented_target "$label" "$project_1" || return $?
            ;;
        storage-lowering)
            verify_storage_lowering "$project_1" || return $?
            ;;
        host-storage)
            verify_host_storage "$project_1" || return $?
            ;;
        host-root-writer)
            verify_host_root_writer "$project_1" || return $?
            ;;
        host-root-split-writer)
            verify_host_root_split_writer "$project_1" "$project_2" "$project_3" || return $?
            ;;
        host-local-service)
            verify_host_local_service "$project_1" "$project_2" || return $?
            ;;
        host-tree-reader)
            verify_host_tree_reader "$project_1" || return $?
            ;;
        host-tree-delete)
            verify_host_tree_delete "$project_1" || return $?
            ;;
        host-tree-scan)
            verify_host_tree_scan "$project_1" || return $?
            ;;
        host-engine)
            verify_host_engine "$project_1" || return $?
            ;;
        host-tree-writer)
            verify_host_tree_writer "$project_1" || return $?
            ;;
        host-logical-tree-writer)
            verify_host_logical_tree_writer "$project_1" "$project_2" || return $?
            ;;
        persistent-transaction-writer)
            verify_persistent_transaction_writer "$project_1" || return $?
            ;;
        *)
            echo "Unknown database qualification handler: $handler." >&2
            return 1
            ;;
    esac
    echo "PASS  native database qualification step=$label item=$qualification_step/$qualification_steps cases=$cases elapsed-ms=$(((SECONDS - started) * 1000)) handler=$handler"
}

run_qualification_lane() {
    local lane=$1
    local expected_steps
    local plan_file="$temporary_directory/Qualification-$lane.plan"
    local executed=0
    local label handler project_1 project_2 project_3 cases extra
    if [[ $lane == portable ]]; then
        expected_steps=$qualification_portable_steps
    else
        expected_steps=$qualification_hosted_steps
    fi
    node "$script_directory/Plan-Database-Storage-Qualification.mjs" \
        --rows "$lane" >"$plan_file" || return $?
    while IFS='|' read -r label handler project_1 project_2 project_3 cases extra; do
        if [[ -z $label || -z $handler || -z $project_1 ||
            -z $project_2 || -z $project_3 || -z $cases || -n $extra ]]; then
            echo "The database qualification $lane row plan is malformed." >&2
            return 1
        fi
        dispatch_qualification_step "$label" "$handler" \
            "$project_1" "$project_2" "$project_3" "$cases" || return $?
        executed=$((executed + 1))
    done <"$plan_file"
    if ((executed != expected_steps)); then
        echo "The database qualification $lane plan executed $executed steps, expected $expected_steps." >&2
        return 1
    fi
}

if ((development == 1)); then
    portable_start=$SECONDS
    verify_development_bundle PublicationRecovery publication-recovery \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Storage-Publication-Recovery-Bundle.wvproj" \
        Publication,Recovery || exit $?
    verify_development_target Publication publication \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Storage-Publication.wvproj" || exit $?
    verify_development_target Recovery recovery \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Storage-Recovery.wvproj" || exit $?
    verify_development_target SingleWriter single-writer \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Single-Writer-Commit.wvproj" || exit $?
    verify_development_target TreeNode tree-node \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Tree-Node.wvproj" tree-scan || exit $?
    verify_development_target LogicalRecord logical-record \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Logical-Record.wvproj" || exit $?
    verify_development_target TypedRow typed-row \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Typed-Row.wvproj" typed-query typed-query-sql || exit $?
    verify_development_target SecondaryIndex secondary-index \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Secondary-Index.wvproj" || exit $?
    verify_development_target SecondaryIndexMutations secondary-index-mutations \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Secondary-Index-Mutations.wvproj" || exit $?
    verify_development_target TransactionMutations transaction-mutations \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Transaction-Mutations.wvproj" transaction || exit $?
    verify_development_target TransactionLeafRewrite transaction-leaf-rewrite \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Transaction-Leaf-Rewrite.wvproj" transaction || exit $?
    verify_development_target TransactionPaths transaction-paths \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Transaction-Paths.wvproj" transaction || exit $?
    verify_development_bundle TransactionLeafGroupsPagesBundle transaction-leaf-groups-pages \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Transaction-Leaf-Groups-Pages-Bundle.wvproj" \
        TransactionLeafGroups,TransactionLeafPages || exit $?
    verify_development_target TransactionLeafGroups transaction-leaf-groups \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Transaction-Leaf-Groups.wvproj" transaction transaction-paths transaction-leaf-partition || exit $?
    verify_development_target TransactionLeafPartition transaction-leaf-partition \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Transaction-Leaf-Partition.wvproj" transaction || exit $?
    verify_development_target TransactionLeafPages transaction-leaf-pages \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Transaction-Leaf-Pages.wvproj" transaction transaction-paths transaction-leaf-groups transaction-leaf-partition || exit $?
    verify_development_target TransactionBranchPartition transaction-branch-partition \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Transaction-Branch-Partition.wvproj" || exit $?
    verify_development_target TransactionParentGroups transaction-parent-groups \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Transaction-Parent-Groups.wvproj" \
        transaction transaction-paths transaction-leaf-groups transaction-leaf-partition \
        transaction-leaf-pages transaction-branch-partition || exit $?
    verify_development_bundle TransactionBranchPagesBundle transaction-branch-pages-bundle \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Transaction-Branch-Pages-Bundle.wvproj" \
        TransactionBranchPages,TransactionBranchPagesValidation,TransactionBranchPagesDepthThree \
        segmented-project || exit $?
    verify_development_target TransactionBranchPages transaction-branch-pages \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Transaction-Branch-Pages.wvproj" \
        transaction transaction-paths transaction-leaf-groups transaction-leaf-partition \
        transaction-leaf-pages transaction-branch-partition transaction-parent-groups || exit $?
    verify_development_target TransactionBranchPagesValidation transaction-branch-pages \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Transaction-Branch-Pages-Validation.wvproj" \
        transaction transaction-paths transaction-leaf-groups transaction-leaf-partition \
        transaction-leaf-pages transaction-branch-partition transaction-parent-groups || exit $?
    verify_development_target TransactionBranchPagesDepthThree transaction-branch-pages \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Transaction-Branch-Pages-Depth-Three.wvproj" \
        transaction transaction-paths transaction-leaf-groups transaction-leaf-partition \
        transaction-leaf-pages transaction-branch-partition transaction-parent-groups || exit $?
    verify_development_bundle TransactionAncestorGroupsBundle transaction-ancestor-groups \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Transaction-Ancestor-Groups-Bundle.wvproj" \
        TransactionAncestorGroups,TransactionAncestorGroupsDepthFour || exit $?
    verify_development_target TransactionAncestorGroups transaction-ancestor-groups \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Transaction-Ancestor-Groups.wvproj" \
        transaction transaction-paths transaction-branch-partition || exit $?
    verify_development_target TransactionAncestorGroupsDepthFour transaction-ancestor-groups \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Transaction-Ancestor-Groups-Depth-Four.wvproj" \
        transaction transaction-paths transaction-branch-partition || exit $?
    verify_development_bundle TransactionAncestorPagesBundle transaction-ancestor-pages \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Transaction-Ancestor-Pages-Bundle.wvproj" \
        TransactionAncestorPages,TransactionAncestorPagesIntermediate || exit $?
    verify_development_target TransactionAncestorPages transaction-ancestor-pages \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Transaction-Ancestor-Pages.wvproj" \
        transaction transaction-paths transaction-branch-partition transaction-ancestor-groups || exit $?
    verify_development_target TransactionAncestorPagesIntermediate transaction-ancestor-pages \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Transaction-Ancestor-Pages-Intermediate.wvproj" \
        transaction transaction-paths transaction-branch-partition transaction-ancestor-groups || exit $?
    verify_development_bundle TransactionRootGrowthBundle transaction-root-growth \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Transaction-Root-Growth-Bundle.wvproj" \
        TransactionRootGrowth,TransactionRootGrowthMultiLevel || exit $?
    verify_development_target TransactionRootGrowth transaction-root-growth \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Transaction-Root-Growth.wvproj" \
        transaction transaction-branch-partition || exit $?
    verify_development_target TransactionRootGrowthMultiLevel transaction-root-growth \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Transaction-Root-Growth-Multi-Level.wvproj" \
        transaction transaction-branch-partition || exit $?
    verify_segmented_development_target TransactionTreeCompletion transaction-tree-completion \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Transaction-Tree-Completion.wvproj" \
        transaction transaction-paths transaction-leaf-groups transaction-leaf-partition \
        transaction-leaf-pages transaction-parent-groups transaction-branch-pages \
        transaction-branch-partition transaction-ancestor-groups transaction-ancestor-pages \
        transaction-root-growth || exit $?
    verify_segmented_development_target TransactionTreeCompletionRootGrowth transaction-tree-completion \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Transaction-Tree-Completion-Root-Growth.wvproj" \
        transaction transaction-paths transaction-leaf-groups transaction-leaf-partition \
        transaction-leaf-pages transaction-parent-groups transaction-branch-pages \
        transaction-branch-partition transaction-ancestor-groups transaction-ancestor-pages \
        transaction-root-growth || exit $?
    verify_development_target TransactionCommitCapacity transaction-commit \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Commit-Batch-Capacity.wvproj" \
        transaction || exit $?
    verify_segmented_development_target TransactionCommit transaction-commit \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Transaction-Commit.wvproj" \
        transaction || exit $?
    verify_development_target QueryIr query-ir \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Query-Ir.wvproj" typed-query query-sql typed-query-sql || exit $?
    verify_development_target SqlLowerer sql-lowerer \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Sql-Lowerer.wvproj" query-sql typed-query-sql || exit $?
    verify_development_target JsonValue json-value \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Json-Value.wvproj" json || exit $?
    verify_development_target JsonProtocol json-protocol \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Json-Protocol.wvproj" json || exit $?
    verify_development_target LocalService local-service \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Local-Database-Service.wvproj" || exit $?
    verify_development_target CollectionCatalog collection-catalog \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Collection-Catalog.wvproj" || exit $?
    verify_development_target Bootstrap bootstrap \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Bootstrap.wvproj" || exit $?
    verify_development_target SingleLeaf single-leaf \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Single-Leaf-Upsert.wvproj" || exit $?
    verify_development_target BranchSplit branch-split \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Branch-Split.wvproj" || exit $?
    verify_development_bundle RootSplitDepthTwoBundle root-split-depth-two \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Root-Split-Depth-Two-Bundle.wvproj" \
        RootSplit,DepthTwo || exit $?
    verify_development_target RootSplit root-split \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Root-Split.wvproj" || exit $?
    verify_development_target DepthTwo depth-two \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Depth-Two-Upsert.wvproj" || exit $?
    verify_development_target DepthThree depth-three \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Depth-Three-Root-Growth.wvproj" || exit $?
    verify_development_target DepthThreeUpsert depth-three-upsert \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Depth-Three-Upsert.wvproj" || exit $?
    verify_development_target TreePathUpsert tree-path-upsert \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Tree-Path-Upsert.wvproj" || exit $?
    verify_development_target TreePathDelete tree-path-delete \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Tree-Path-Delete.wvproj" host-tree-delete || exit $?
    portable_elapsed_ms=$(((SECONDS - portable_start) * 1000))
    verify_development_host_targets || exit $?
    if ((progress_current != progress_total)); then
        echo "The database development plan executed $((progress_current - 1)) steps, expected $selected_executions." >&2
        exit 1
    fi
    development_elapsed_ms=$(((SECONDS - development_start) * 1000))
    echo "native database storage development timing target=$development_target tools-ms=$tools_elapsed_ms portable-ms=$portable_elapsed_ms host-storage-ms=$host_storage_elapsed_ms host-root-writer-ms=$host_root_writer_elapsed_ms host-local-service-ms=$host_local_service_elapsed_ms host-tree-reader-ms=$host_tree_reader_elapsed_ms host-tree-delete-ms=$host_tree_delete_elapsed_ms host-tree-scan-ms=$host_tree_scan_elapsed_ms engine-ms=$engine_elapsed_ms host-tree-writer-ms=$host_tree_writer_elapsed_ms persistent-transaction-writer-ms=$persistent_transaction_writer_elapsed_ms total-ms=$development_elapsed_ms"
    echo "native database storage development status=Passed target=$development_target cases=$selected_cases executions=$selected_executions local-results=0 tools=$tool_checkpoint project-wvb=$project_wvb_checkpoint portable-projects=$portable_project_checkpoints portable-applications=$portable_application_checkpoints projects=HostStorage:$project_checkpoint_host_storage,HostRootWriter:$project_checkpoint_host_root_writer,HostLocalService:$project_checkpoint_host_local_service,HostTreeReader:$project_checkpoint_host_tree_reader,HostTreeDelete:$project_checkpoint_host_tree_delete,HostTreeScan:$project_checkpoint_host_tree_scan,Engine:$project_checkpoint_engine,HostTreeWriter:$project_checkpoint_host_tree_writer,PersistentTransactionWriter:$project_checkpoint_persistent_transaction_writer applications=HostStorage:$application_checkpoint_host_storage,HostRootWriter:$application_checkpoint_host_root_writer,HostLocalService:$application_checkpoint_host_local_service,HostTreeReader:$application_checkpoint_host_tree_reader,HostTreeDelete:$application_checkpoint_host_tree_delete,HostTreeScan:$application_checkpoint_host_tree_scan,Engine:$application_checkpoint_engine,HostTreeWriter:$application_checkpoint_host_tree_writer,PersistentTransactionWriter:$application_checkpoint_persistent_transaction_writer"
    exit 0
fi

if [[ -n $qualification_step_target ]]; then
    qualification_step_plan="$temporary_directory/Qualification-Focused.plan"
    WINDVALE_DATABASE_QUALIFICATION_STEP=$qualification_step_target \
        node "$script_directory/Plan-Database-Storage-Qualification.mjs" \
        --closure-env >"$qualification_step_plan" || exit $?
    qualification_steps=$(wc -l <"$qualification_step_plan") || exit $?
    case "$qualification_steps" in
        ''|*[!0-9]*|0) echo 'The focused database qualification plan selected no steps.' >&2; exit 1 ;;
    esac
    qualification_step_rows=0
    qualification_step_cases=
    while IFS='|' read -r label handler project_1 project_2 project_3 cases extra; do
        if [[ -z $label || -z $handler || -z $project_1 ||
            -z $project_2 || -z $project_3 || -z $cases || -n $extra ]]; then
            echo 'The focused database qualification row is malformed.' >&2
            exit 1
        fi
        dispatch_qualification_step "$label" "$handler" \
            "$project_1" "$project_2" "$project_3" "$cases" || exit $?
        if [[ $label == "$qualification_step_target" ]]; then
            qualification_step_cases=$cases
        fi
        qualification_step_rows=$((qualification_step_rows + 1))
    done <"$qualification_step_plan"
    if ((qualification_step_rows != qualification_steps)) ||
        [[ -z $qualification_step_cases ]]; then
        echo "The focused database qualification plan returned an invalid $qualification_step_rows-step closure." >&2
        exit 1
    fi
    qualification_support_steps=$((qualification_steps - 1))
    echo "native database storage qualification step status=Passed step=$qualification_step_target cases=$qualification_step_cases support-steps=$qualification_support_steps local-results=0 current-host-behavior=Verified portable-reproducibility=Delegated cross-target-packaging=Delegated"
    exit 0
fi

if [[ $qualification_target != hosted ]]; then
    run_qualification_lane portable || exit $?
fi
if [[ $qualification_target == portable ]]; then
    echo "native database storage portable status=Passed cases=$qualification_portable_cases local-results=0 current-host-behavior=Verified portable-reproducibility=Delegated cross-target-packaging=Delegated"
    exit 0
fi
if [[ $qualification_target != portable ]]; then
    run_qualification_lane hosted || exit $?
fi
if [[ $qualification_target == hosted ]]; then
    echo "native database storage hosted status=Passed cases=$qualification_hosted_cases local-results=0 current-host-behavior=Verified portable-reproducibility=Delegated cross-target-packaging=Delegated"
    exit 0
fi
echo "native database storage status=Passed cases=$qualification_cases local-results=0 current-host-behavior=Verified portable-reproducibility=Delegated cross-target-packaging=Delegated"
exit 0
