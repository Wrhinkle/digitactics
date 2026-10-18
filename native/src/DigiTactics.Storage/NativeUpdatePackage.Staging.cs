using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;

namespace DigiTactics.Storage;

public sealed record NativeStagedPackage(string Directory,NativePackageManifest Manifest);

public static partial class NativeUpdatePackage
{
    /// <summary>Creates a complete, uniquely named candidate. Never activates it or touches player data.
    /// The caller owns the staging root, which must not be writable by other users.</summary>
    public static NativeStagedPackage Stage(string archivePath,string stagingRoot,NativePackageExpectation expected,int launcherVersion,CancellationToken cancellation=default)
    {
        // Hold the same archive handle throughout verification and extraction. On Windows this lease
        // excludes writers, renames and deletion, so a verified download cannot be swapped underneath us.
        using var input=new FileStream(archivePath,FileMode.Open,FileAccess.Read,FileShare.Read);
        var manifest=Inspect(input,expected,launcherVersion,cancellation);
        string root=Path.GetFullPath(stagingRoot);
        EnsureOrdinaryAncestors(root);
        Directory.CreateDirectory(root);
        EnsureOrdinaryAncestors(root);
        string token=Guid.NewGuid().ToString("N");
        string partial=Path.Combine(root,".partial-"+token);
        string ready=Path.Combine(root,manifest.Version+"-"+token);
        if(Directory.Exists(partial) || File.Exists(partial))throw new IOException("Staging directory already exists.");
        Directory.CreateDirectory(partial);
        // Failed or interrupted extraction stays under .partial for later launcher cleanup.
        input.Position=0;
        using(var zip=new ZipArchive(input,ZipArchiveMode.Read,leaveOpen:true))
        {
            byte[] buffer=new byte[128*1024];
            foreach(var file in manifest.Files)
            {
                cancellation.ThrowIfCancellationRequested();
                string destination=Path.GetFullPath(Path.Combine(partial,file.Path.Replace('/',Path.DirectorySeparatorChar)));
                if(!destination.StartsWith(partial+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))throw new InvalidDataException("Package file escapes staging directory.");
                string parent=Path.GetDirectoryName(destination)!;
                EnsureOrdinaryAncestors(parent);Directory.CreateDirectory(parent);EnsureOrdinaryAncestors(parent);
                using var source=zip.GetEntry(file.Path)!.Open();
                using var output=new FileStream(destination,FileMode.CreateNew,FileAccess.Write,FileShare.None);
                using var hash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
                long length=0;int read;
                while((read=source.Read(buffer))>0)
                {
                    cancellation.ThrowIfCancellationRequested();length=checked(length+read);
                    if(length>file.Size)throw new InvalidDataException("Staged file exceeds its expected length.");
                    output.Write(buffer,0,read);hash.AppendData(buffer,0,read);
                }
                if(length!=file.Size || Convert.ToHexStringLower(hash.GetHashAndReset())!=file.Sha256)throw new InvalidDataException("Staged file content differs.");
                output.Flush(flushToDisk:true);
            }
        }
        cancellation.ThrowIfCancellationRequested();
        using(var marker=new FileStream(Path.Combine(partial,ManifestName),FileMode.CreateNew,FileAccess.Write,FileShare.None))
        {
            JsonSerializer.Serialize(marker,manifest,new JsonSerializerOptions {PropertyNamingPolicy=JsonNamingPolicy.CamelCase});
            marker.Flush(flushToDisk:true);
        }
        // Only complete candidates become visible to activation code. An interrupted extraction
        // remains .partial and cannot be mistaken for a runnable release.
        EnsureOrdinaryAncestors(partial);
        Directory.Move(partial,ready);
        return new(ready,manifest);
    }

    private static void EnsureOrdinaryAncestors(string path)
    {
        for(string? current=path;current is not null;current=Path.GetDirectoryName(current))
        {
            if(!Path.Exists(current))continue;
            var attributes=File.GetAttributes(current);
            if((attributes&FileAttributes.ReparsePoint)!=0 || (attributes&FileAttributes.Directory)==0)
                throw new IOException("Native staging requires ordinary directories without links.");
        }
    }
}
