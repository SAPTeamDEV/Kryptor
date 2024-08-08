#!/bin/bash

cd $(dirname $0)

./publish.sh -d

cd ../bin/Publish/Cli/Debug

adb shell rm -rf //data/data/com.termux/files/usr/var/lib/proot-distro/installed-rootfs/ubuntu/root/linux-x64-net6.0
adb push linux-x64-net6.0 //data/data/com.termux/files/usr/var/lib/proot-distro/installed-rootfs/ubuntu/root/
adb shell chmod +x //data/data/com.termux/files/usr/var/lib/proot-distro/installed-rootfs/ubuntu/root/linux-x64-net6.0/Kryptor
