#!/bin/sh
set -eu

version='@@VERSION@@'
target='@@TARGET@@'
payload='@@PAYLOAD_SHA256@@'
generation='@@GENERATION@@'
package_root=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
home_root=${HOME:?HOME is required}
data_home=${XDG_DATA_HOME:-$home_root/.local/share}
install_root=$data_home/windvale
bin_directory=${XDG_BIN_HOME:-${HOME:?HOME is required}/.local/bin}
publish_links=1

while [ "$#" -gt 0 ]; do
    case "$1" in
        --root)
            [ "$#" -ge 2 ] || { echo 'Missing --root value.' >&2; exit 64; }
            install_root=$2
            shift 2
            ;;
        --bin-dir)
            [ "$#" -ge 2 ] || { echo 'Missing --bin-dir value.' >&2; exit 64; }
            bin_directory=$2
            shift 2
            ;;
        --no-links)
            publish_links=0
            shift
            ;;
        *)
            echo 'Usage: ./install.sh [--root <path>] [--bin-dir <path>] [--no-links]' >&2
            exit 64
            ;;
    esac
done

install_root=$(realpath -m -- "$install_root")
bin_directory=$(realpath -m -- "$bin_directory")
home_root=$(realpath -m -- "$home_root")
data_home=$(realpath -m -- "$data_home")
[ "$install_root" != / ] || { echo 'The installation root is too broad.' >&2; exit 1; }
case "$home_root/" in "$install_root/"*) echo 'The installation root is too broad.' >&2; exit 1 ;; esac
case "$data_home/" in "$install_root/"*) echo 'The installation root is too broad.' >&2; exit 1 ;; esac

echo 'windvale installer step=verify-package item=1/5'
"$package_root/bin/wv-verify-installation" "$package_root" "$target" "$payload"

commands='wv wvbuild wvasm wvlink wvrun wvdump wvverify wvpublish'
if [ "$publish_links" -eq 1 ]; then
    mkdir -p -- "$bin_directory"
    for command in $commands; do
        link=$bin_directory/$command
        if [ -e "$link" ] || [ -L "$link" ]; then
            if [ ! -L "$link" ] || [ "$(readlink -- "$link")" != "$install_root/bin/$command" ]; then
                echo "Refusing to replace an unrelated command: $link" >&2
                exit 1
            fi
        fi
    done
fi

generations_root=$install_root/generations
generation_root=$generations_root/$generation
candidate_root=$generations_root/.candidate-$generation-$$
mkdir -p -- "$generations_root"
[ ! -e "$candidate_root" ] || { echo 'A prior installer candidate needs inspection.' >&2; exit 1; }

cleanup_candidate() {
    if [ -e "$candidate_root" ]; then
        case "$candidate_root" in
            "$generations_root"/.candidate-*) rm -rf -- "$candidate_root" ;;
            *) echo 'Refusing to remove an unexpected installer candidate.' >&2; return 1 ;;
        esac
    fi
}
trap cleanup_candidate EXIT HUP INT TERM

echo 'windvale installer step=publish-generation item=2/5'
if [ -d "$generation_root" ]; then
    "$generation_root/bin/wv-verify-installation" "$generation_root" "$target" "$payload"
else
    mkdir -- "$candidate_root"
    sed -n '4,$p' "$package_root/Payload-Manifest.txt" |
    while IFS=' ' read -r record expected_sha256 expected_bytes expected_mode relative_path; do
        destination=$candidate_root/$relative_path
        mkdir -p -- "$(dirname -- "$destination")"
        cp -p -- "$package_root/$relative_path" "$destination"
    done
    cp -p -- "$package_root/Payload-Manifest.txt" "$candidate_root/Payload-Manifest.txt"
    "$candidate_root/bin/wv-verify-installation" "$candidate_root" "$target" "$payload"
    mv -- "$candidate_root" "$generation_root"
fi

echo 'windvale installer step=publish-command-shims item=3/5'
mkdir -p -- "$install_root/bin"
for command in $commands; do
    executable=$command
    wrapper=$install_root/bin/$command
    candidate=$install_root/bin/.$command.candidate.$$
    {
        printf '%s\n' '#!/bin/sh' 'set -eu'
        printf '%s\n' \
            'script=$0' \
            'while [ -L "$script" ]; do' \
            '    directory=$(CDPATH= cd -- "$(dirname -- "$script")" && pwd -P)' \
            '    destination=$(readlink -- "$script")' \
            '    case "$destination" in' \
            '        /*) script=$destination ;;' \
            '        *) script=$directory/$destination ;;' \
            '    esac' \
            'done' \
            'script_directory=$(CDPATH= cd -- "$(dirname -- "$script")" && pwd -P)'
        if [ "$command" = wv ]; then
            printf '%s\n' \
                "if [ \"\${1:-}\" = doctor ]; then" \
                "    exec \"\$script_directory/../generations/$generation/bin/wv-verify-installation\" \"\$script_directory/../generations/$generation\" '$target' '$payload'" \
                'fi'
        fi
        printf '%s\n' "exec \"\$script_directory/../generations/$generation/bin/$executable\" \"\$@\""
    } >"$candidate"
    chmod 0755 "$candidate"
    mv -f -- "$candidate" "$wrapper"
done

echo 'windvale installer step=record-installation item=4/5'
mkdir -p -- "$install_root/installations"
printf '%s\n' \
    '@@INSTALLATION_RECORD@@' \
    "version $version" \
    "target $target" \
    "generation $generation" \
    "payload $payload" >"$install_root/installations/$generation.txt"
cp -p -- "$package_root/uninstall.sh" "$install_root/uninstall.sh"

echo 'windvale installer step=finish item=5/5'
if [ "$publish_links" -eq 1 ]; then
    for command in $commands; do
        link=$bin_directory/$command
        candidate=$bin_directory/.$command.candidate.$$
        ln -s -- "$install_root/bin/$command" "$candidate"
        mv -f -- "$candidate" "$link"
    done
    case ":${PATH:-}:" in
        *":$bin_directory:"*) ;;
        *) echo "Add this directory to PATH: $bin_directory" ;;
    esac
else
    echo "Add this directory to PATH when desired: $install_root/bin"
fi
printf '%s\n' "windvale installer status=Installed version=$version target=$target generation=$generation root=$install_root"
