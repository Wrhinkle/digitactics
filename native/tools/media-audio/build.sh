#!/usr/bin/env bash
set -euo pipefail
media_tools="$(/usr/bin/cygpath -u "$DT_MEDIA_BUILD_TOOLS")"
msvc_dir="$(/usr/bin/cygpath -u "$VCToolsInstallDir")"
export PATH="$media_tools/make/usr/bin:$media_tools/nasm/nasm-2.16.03:$msvc_dir/bin/Hostx64/x64:$PATH:/usr/bin"
cd "$1"
./configure --toolchain=msvc --arch=x86_64 --target-os=win64 --prefix=/digitactics-media-runtime \
 --disable-autodetect --disable-everything --disable-doc --disable-debug \
 --disable-avdevice --disable-swscale --disable-network --disable-ffplay --disable-ffprobe \
 --enable-ffmpeg --enable-shared --disable-static --enable-w32threads --enable-swresample --enable-avfilter \
 --enable-protocol=file,pipe --enable-muxer=pcm_f32le --enable-encoder=pcm_f32le \
 --enable-filter=aresample,aformat,anull --enable-demuxer=aac,aiff,asf,flac,matroska,mov,mp3,ogg,wav \
 --enable-decoder=aac,aac_fixed,alac,flac,mp3,mp3float,mp3adu,mp3adufloat,mp3on4,mp3on4float,opus,vorbis,wmav1,wmav2,wmapro,wmalossless,pcm_alaw,pcm_mulaw,pcm_s8,pcm_u8,pcm_s16le,pcm_s16be,pcm_u16le,pcm_u16be,pcm_s24le,pcm_s24be,pcm_u24le,pcm_u24be,pcm_s32le,pcm_s32be,pcm_u32le,pcm_u32be,pcm_s64le,pcm_s64be,pcm_f16le,pcm_f24le,pcm_f32le,pcm_f32be,pcm_f64le,pcm_f64be,adpcm_ima_wav,adpcm_ms \
 --enable-parser=aac,aac_latm,flac,mpegaudio,opus,vorbis \
 --extra-version=digitactics-audio-29e619e767 --extra-cflags=-Brepro --extra-ldflags=-Brepro
grep -q '^#define CONFIG_PCM_F32LE_MUXER 1$' config_components.h
grep -q '^#define CONFIG_GPL 0$' config.h
grep -q '^#define CONFIG_NONFREE 0$' config.h
make -j"${DT_MEDIA_BUILD_JOBS:-4}"
make install DESTDIR="$(/usr/bin/cygpath -u "$2")"
