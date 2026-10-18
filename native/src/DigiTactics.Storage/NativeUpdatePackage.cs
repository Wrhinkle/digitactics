using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace DigiTactics.Storage;

public sealed record NativePackageFile(string Path,long Size,string Sha256);
public sealed record NativePackageManifest(int SchemaVersion,string Version,string Platform,int MinimumLauncher,NativePackageFile[] Files);
public sealed record NativePackageExpectation(string Version,long Size,string Sha256);

/// <summary>Read-only verification of a native release archive before any staging or activation.</summary>
public static partial class NativeUpdatePackage
{
    public const string ManifestName="native-package.json";
    public const long MaximumArchiveBytes=16L*1024*1024*1024;
    public const long MaximumExpandedBytes=32L*1024*1024*1024;
    public const int MaximumFiles=100000;
    private const int MaximumManifestBytes=16*1024*1024;
    [GeneratedRegex(@"^(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\z",RegexOptions.CultureInvariant)]
    private static partial Regex VersionPattern();
    public static bool ValidVersion(string? value)=>value is not null && value.Length<=32 && VersionPattern().IsMatch(value) && value.Split('.').All(p=>int.TryParse(p,out _));
    private static bool Digest(string? value)=>value is {Length:64} && value.All(c=>c is >= '0' and <= '9' or >= 'a' and <= 'f');
    public static void ValidatePath(string path)
    {
        if(string.IsNullOrEmpty(path) || path.Length>240 || path.Contains('\\') || path.StartsWith('/') || path.Any(c=>c<32 || "<>:\"|?*".Contains(c))) throw new InvalidDataException("Unsafe package path.");
        foreach(string segment in path.Split('/'))
        {
            if(segment.Length==0 || segment is "." or ".." || segment.EndsWith('.') || segment.EndsWith(' '))throw new InvalidDataException("Noncanonical package path.");
            string stem=segment.Split('.')[0].ToUpperInvariant();
            if(stem is "CON" or "PRN" or "AUX" or "NUL" or "CLOCK$" or "CONIN$" or "CONOUT$" || (stem.StartsWith("COM") || stem.StartsWith("LPT")) && stem.Length==4 && "123456789\u00b9\u00b2\u00b3".Contains(stem[3])) throw new InvalidDataException("Reserved Windows package path.");
        }
    }
    public static NativePackageManifest Inspect(string archivePath,NativePackageExpectation expected,int launcherVersion,CancellationToken cancellation=default)
    {
        using var input=new FileStream(archivePath,FileMode.Open,FileAccess.Read,FileShare.Read);
        return Inspect(input,expected,launcherVersion,cancellation);
    }
    private static NativePackageManifest Inspect(FileStream input,NativePackageExpectation expected,int launcherVersion,CancellationToken cancellation)
    {
        if(!ValidVersion(expected.Version) || !Digest(expected.Sha256) || expected.Size<=0 || expected.Size>MaximumArchiveBytes || launcherVersion<1)throw new InvalidDataException("Invalid native update descriptor.");
        if(input.Length!=expected.Size)throw new InvalidDataException("Native update download length differs.");
        if(Hash(input,expected.Size,cancellation)!=expected.Sha256)throw new InvalidDataException("Native update download hash differs.");
        input.Position=0;
        using var zip=new ZipArchive(input,ZipArchiveMode.Read,leaveOpen:true);
        if(zip.Entries.Count is <2 or >MaximumFiles+1)throw new InvalidDataException("Invalid native package entry count.");
        var entries=new Dictionary<string,ZipArchiveEntry>(StringComparer.OrdinalIgnoreCase);
        long total=0;
        foreach(var entry in zip.Entries)
        {
            cancellation.ThrowIfCancellationRequested();ValidatePath(entry.FullName);
            int unixType=(entry.ExternalAttributes>>16)&0xf000;
            if(unixType is not (0 or 0x8000) || (entry.ExternalAttributes&0x410)!=0)throw new InvalidDataException("Package links and directories are not allowed.");
            if(!entries.TryAdd(entry.FullName,entry))throw new InvalidDataException("Duplicate native package path.");
            total=checked(total+entry.Length);
            if(entry.Length<0 || total>MaximumExpandedBytes)throw new InvalidDataException("Native package exceeds expanded size limit.");
        }
        if(!entries.TryGetValue(ManifestName,out var manifestEntry) || manifestEntry.FullName!=ManifestName || manifestEntry.Length>MaximumManifestBytes)throw new InvalidDataException("Missing or oversized native package manifest.");
        using var manifestStream=manifestEntry.Open();
        byte[] manifestBytes=new byte[checked((int)manifestEntry.Length)];
        manifestStream.ReadExactly(manifestBytes);
        if(manifestStream.ReadByte()!=-1)throw new InvalidDataException("Manifest expanded length differs.");
        using var document=JsonDocument.Parse(manifestBytes,new JsonDocumentOptions {MaxDepth=16});
        RejectDuplicateProperties(document.RootElement);
        var manifest=document.Deserialize<NativePackageManifest>(new JsonSerializerOptions {PropertyNamingPolicy=JsonNamingPolicy.CamelCase,UnmappedMemberHandling=JsonUnmappedMemberHandling.Disallow})??throw new InvalidDataException("Missing native package metadata.");
        if(manifest.SchemaVersion!=1 || manifest.Platform!="win-x64" || manifest.Version!=expected.Version || !ValidVersion(manifest.Version) || manifest.MinimumLauncher<1 || manifest.MinimumLauncher>launcherVersion || manifest.Files is null || manifest.Files.Length!=entries.Count-1)throw new InvalidDataException("Incompatible native package manifest.");
        var listed=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach(var file in manifest.Files)
        {
            if(file is null)throw new InvalidDataException("Missing package file metadata.");
            ValidatePath(file.Path);
            if(file.Path.Equals(ManifestName,StringComparison.OrdinalIgnoreCase) || !listed.Add(file.Path) || !Digest(file.Sha256) || file.Size<0 || !entries.TryGetValue(file.Path,out var entry) || entry.FullName!=file.Path || entry.Length!=file.Size)throw new InvalidDataException("Native package file manifest differs from archive.");
            using var contents=entry.Open();if(Hash(contents,file.Size,cancellation)!=file.Sha256)throw new InvalidDataException("Native package content hash differs: "+file.Path);
        }
        foreach(string path in listed)
        {
            int slash=path.IndexOf('/');while(slash>=0) {if(entries.ContainsKey(path[..slash]))throw new InvalidDataException("Package file/directory collision.");slash=path.IndexOf('/',slash+1);}
        }
        if(!manifest.Files.Any(f=>f.Path=="DigiTactics.Desktop.exe"))throw new InvalidDataException("Native package has no application executable.");
        return manifest;
    }
    private static string Hash(Stream stream,long expectedLength,CancellationToken cancellation)
    {
        using var hash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);byte[] buffer=new byte[128*1024];int read;long length=0;
        while((read=stream.Read(buffer))>0) {cancellation.ThrowIfCancellationRequested();length=checked(length+read);if(length>expectedLength)throw new InvalidDataException("Expanded file length exceeds metadata.");hash.AppendData(buffer,0,read);}
        if(length!=expectedLength)throw new InvalidDataException("Expanded file length differs.");
        cancellation.ThrowIfCancellationRequested();return Convert.ToHexStringLower(hash.GetHashAndReset());
    }
    private static void RejectDuplicateProperties(JsonElement value)
    {
        if(value.ValueKind==JsonValueKind.Object)
        {
            var names=new HashSet<string>(StringComparer.Ordinal);
            foreach(var property in value.EnumerateObject()) {if(!names.Add(property.Name))throw new InvalidDataException("Duplicate package manifest field.");RejectDuplicateProperties(property.Value);}
        }
        else if(value.ValueKind==JsonValueKind.Array)foreach(var item in value.EnumerateArray())RejectDuplicateProperties(item);
    }
}
