#!/bin/bash
clear

set -e

read -p "Enter path of the JB4A-SDK-iOS source: " sdk_path

if [ ! -d "$sdk_path" ]; then
	echo "JB4A-SDK-iOS source not found at path: $sdk_path" >&2
    exit 1
fi

sdk_header_dir="$sdk_path/JB4A-SDK/"
sdk_main_header="$sdk_header_dir/JB4ASDK-Bridging-Header.h"

if [ ! -f "$sdk_main_header" ]; then
	echo "JB4ASDK-Bridging-Header.h source not found: $sdk_main_header" >&2
    exit 1
fi 

bind_output_dir=JB4A-SDK-iOS.intermediates

if [  -d "$bind_output_dir" ]; then
	rm -f $bind_output_dir
fi

sharpie bind --output=$bind_output_dir --namespace=JB4ASDK --sdk=iphoneos11.1 $sdk_main_header -scope $sdk_header_dir
