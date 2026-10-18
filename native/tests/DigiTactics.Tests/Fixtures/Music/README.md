These ten small files contain original synthetic 441 Hz sine waves at 32 kHz,
mono. They contain no sampled recordings. Nine 0.3-second clips test decoding
and resampling; the 60-second FLAC tests bounded buffering and cancellation.
`manifest.json` records the exact generating command, encoder provenance,
sizes and SHA-256 hashes. Tests verify each fixture before copying it into
temporary paths containing Unicode and shell metacharacters.

Normal test/build runs use these checked-in fixtures and need only the small
production decoder. To regenerate them intentionally, provision the development
encoder with `native/tools/setup-media-fixture-encoder.ps1`, then run
`python native/tools/generate-music-fixtures.py`. Ogg/WebM container metadata can
vary between regeneration runs; review and commit the new bytes and manifest
together. `--fixture-ffmpeg <executable>` remains an optional test argument for
generating temporary fixtures directly; it does not overwrite these files.
