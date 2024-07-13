#!/bin/bash
VER=$1
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
WORKROOT=$DIR/work-plugin-feed-filter
WORK=$WORKROOT/Plugins/FeedFilter
ROOT=$DIR/../..
BUILD=$ROOT/VirtualRadar/bin/x86/Release/Plugins/FeedFilter
OUTPUT=$DIR/output

if [ -d $WORKROOT ]; then
  rm -r $WORKROOT;
fi
if [ ! -d $OUTPUT ]; then
  mkdir $OUTPUT;
fi
mkdir -p $WORK

cp -r $BUILD/* $WORK
cd $WORKROOT
tar -czf     $OUTPUT/Plugin-FeedFilter-$VER.tar.gz *
echo Created $OUTPUT/Plugin-FeedFilter-$VER.tar.gz
