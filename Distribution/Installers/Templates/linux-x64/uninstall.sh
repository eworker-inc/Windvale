#!/bin/sh
set -eu

version='@@VERSION@@'
target='@@TARGET@@'
payload='@@PAYLOAD_SHA256@@'
generation='@@GENERATION@@'
home_root=${HOME:?HOME is required}
data_home=${XDG_DATA_HOME:-$home_root/.local/share}
install_root=$data_home/windvale
bin_directory=${XDG_BIN_HOME:-${HOME:?HOME is required}/.local/bin}
remove_links=1

while [ "$#" -gt 0 ]; do
    case "$1" in
        --root) [ "$#" -ge 2 ] || exit 64; install_root=$2; shift 2 ;;
        --bin-dir) [ "$#" -ge 2 ] || exit 64; bin_directory=$2; shift 2 ;;
        --no-links) remove_links=0; shift ;;
        *) echo 'Usage: ./uninstall.sh [--root <path>] [--bin-dir <path>] [--no-links]' >&2; exit 64 ;;
    esac
done

install_root=$(realpath -m -- "$install_root")
bin_directory=$(realpath -m -- "$bin_directory")
home_root=$(realpath -m -- "$home_root")
data_home=$(realpath -m -- "$data_home")
[ "$install_root" != / ] || { echo 'Refusing to remove a broad filesystem root.' >&2; exit 1; }
case "$home_root/" in "$install_root/"*) echo 'Refusing to remove a broad filesystem root.' >&2; exit 1 ;; esac
case "$data_home/" in "$install_root/"*) echo 'Refusing to remove a broad filesystem root.' >&2; exit 1 ;; esac
record=$install_root/installations/$generation.txt
expected_record=$(printf '%s\n' \
    '@@INSTALLATION_RECORD@@' \
    "version $version" \
    "target $target" \
    "generation $generation" \
    "payload $payload")
[ -f "$record" ] && [ "$(cat -- "$record")" = "$expected_record" ] || {
    echo 'The exact @@INSTALLATION_DESCRIPTION@@ record is absent.' >&2
    exit 1
}

if [ "$remove_links" -eq 1 ]; then
    for command in wv wvbuild wvasm wvlink wvrun wvdump wvverify wvpublish; do
        link=$bin_directory/$command
        if [ -L "$link" ] && [ "$(readlink -- "$link")" = "$install_root/bin/$command" ]; then
            rm -- "$link"
        fi
    done
fi

echo 'windvale uninstaller step=remove-installation item=1/1'
rm -rf -- "$install_root"
printf '%s\n' "windvale uninstaller status=Removed version=$version target=$target root=$install_root"
