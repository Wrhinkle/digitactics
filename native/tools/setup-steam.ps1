$ErrorActionPreference = 'Stop'
$nativeRoot = Split-Path -Parent $PSScriptRoot
$destination = Join-Path $nativeRoot 'artifacts/steam-runtime'
$downloads = Join-Path $nativeRoot 'artifacts/steam-downloads'
$url = 'https://registry.npmjs.org/steamworks.js/-/steamworks.js-0.4.0.tgz'
$integrity = 'O5TTRs7ucCRql4IA/kYUIQYeghTsXqf3rAm81sC22RDId264LQYqQjuaMEUSqL60I5LdULiGu0W2/A+ZDcKBKA=='
$dllHash = '1add7f151fa644870a735ae86e68d1f019f296130d8e7c0a7ed3ecc7482dccbc'
New-Item -ItemType Directory -Force -Path $destination,$downloads | Out-Null
$archive = Join-Path $downloads 'steamworks.js-0.4.0.tgz'
function Verify-Archive([string]$Path) {
    $stream = [IO.File]::OpenRead($Path)
    $hash = [Security.Cryptography.SHA512]::Create()
    try { return [Convert]::ToBase64String($hash.ComputeHash($stream)) -ceq $integrity }
    finally { $hash.Dispose(); $stream.Dispose() }
}
if (-not (Test-Path -LiteralPath $archive)) {
    $temporary = $archive + '.' + [Guid]::NewGuid().ToString('N') + '.download'
    & curl.exe --proto '=https' --proto-redir '=https' --location --fail --silent --show-error --output $temporary $url
    if ($LASTEXITCODE -ne 0) { throw 'Could not download the pinned Steam dependency.' }
    if (-not (Verify-Archive $temporary)) { throw "Steam dependency integrity mismatch: $temporary" }
    Move-Item -LiteralPath $temporary -Destination $archive
}
if (-not (Verify-Archive $archive)) { throw 'Cached Steam dependency integrity mismatch.' }
$extract = Join-Path $downloads ([Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $extract | Out-Null
# Extract only these exact regular package paths, never the complete npm package.
& tar.exe -xzf $archive -C $extract 'package/dist/win64/steam_api64.dll' 'package/LICENSE'
if ($LASTEXITCODE -ne 0) { throw 'Could not extract the pinned Steam dependency.' }
$dll = Join-Path $extract 'package/dist/win64/steam_api64.dll'
if ((Get-FileHash -LiteralPath $dll -Algorithm SHA256).Hash.ToLowerInvariant() -cne $dllHash) { throw 'Steam DLL integrity mismatch.' }
Copy-Item -LiteralPath $dll -Destination $destination -Force
Copy-Item -LiteralPath (Join-Path $extract 'package/LICENSE') -Destination (Join-Path $destination 'steamworks-js-LICENSE.txt') -Force
[IO.File]::WriteAllText((Join-Path $destination 'steam-runtime-provenance.json'),(@{package='steamworks.js';version='0.4.0';url=$url;integrity=('sha512-'+$integrity);dllSha256=$dllHash}|ConvertTo-Json))
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'STEAM_RUNTIME_NOTICES.txt') -Destination $destination -Force
Write-Output $destination
