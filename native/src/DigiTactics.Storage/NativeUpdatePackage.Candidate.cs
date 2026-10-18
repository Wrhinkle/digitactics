using System.Text.Json;
using System.Text.Json.Serialization;

namespace DigiTactics.Storage;

public sealed class NativeCandidateLease : IDisposable
{
    private readonly List<FileStream> files;
    public string Executable {get;}
    internal NativeCandidateLease(string executable,List<FileStream> files) {Executable=executable;this.files=files;}
    public void Dispose() {foreach(var file in files)file.Dispose();files.Clear();}
}

public static partial class NativeUpdatePackage
{
    /// <summary>Revalidates every staged byte against the manifest pinned in launcher state and
    /// excludes Windows file replacement/writes until the returned lease is disposed.</summary>
    public static NativeCandidateLease OpenCandidate(string candidatesRoot,NativeReleaseReference release,int launcherVersion,CancellationToken cancellation=default)
    {
        NativeLaunchState.ValidateRelease(release);
        if(!Digest(release.ManifestSha256) || launcherVersion<1)throw new InvalidDataException("Candidate has no trusted manifest digest.");
        string directory=Path.GetFullPath(Path.Combine(candidatesRoot,release.Candidate));
        EnsureOrdinaryAncestors(directory);
        var leases=new List<FileStream>();
        FileStream Open(string path)
        {
            if((File.GetAttributes(path)&FileAttributes.ReparsePoint)!=0)throw new InvalidDataException("Candidate contains a linked file.");
            var stream=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read);leases.Add(stream);return stream;
        }
        try
        {
            var metadata=Open(Path.Combine(directory,ManifestName));
            if(metadata.Length>MaximumManifestBytes || Hash(metadata,metadata.Length,cancellation)!=release.ManifestSha256)throw new InvalidDataException("Candidate manifest differs from verified package.");
            metadata.Position=0;
            using var document=JsonDocument.Parse(metadata,new JsonDocumentOptions {MaxDepth=16});
            RejectDuplicateProperties(document.RootElement);
            var manifest=document.Deserialize<NativePackageManifest>(new JsonSerializerOptions {PropertyNamingPolicy=JsonNamingPolicy.CamelCase,UnmappedMemberHandling=JsonUnmappedMemberHandling.Disallow})??throw new InvalidDataException("Missing candidate manifest.");
            if(manifest.SchemaVersion!=1 || manifest.Version!=release.Version || manifest.Platform!="win-x64" || manifest.MinimumLauncher<1 || manifest.MinimumLauncher>launcherVersion || manifest.Files is null || manifest.Files.Length is <1 or >MaximumFiles)throw new InvalidDataException("Incompatible candidate metadata.");
            var paths=new HashSet<string>(StringComparer.OrdinalIgnoreCase) {ManifestName};
            long total=metadata.Length;
            foreach(var file in manifest.Files)
            {
                cancellation.ThrowIfCancellationRequested();
                if(file is null)throw new InvalidDataException("Missing candidate file metadata.");
                ValidatePath(file.Path);
                if(!paths.Add(file.Path) || !Digest(file.Sha256) || file.Size<0)throw new InvalidDataException("Invalid candidate file metadata.");
                total=checked(total+file.Size);if(total>MaximumExpandedBytes)throw new InvalidDataException("Candidate exceeds expanded limit.");
                string path=Path.Combine(directory,file.Path.Replace('/',Path.DirectorySeparatorChar));
                EnsureOrdinaryAncestors(Path.GetDirectoryName(path)!);
                var stream=Open(path);
                if(stream.Length!=file.Size || Hash(stream,file.Size,cancellation)!=file.Sha256)throw new InvalidDataException("Candidate content differs: "+file.Path);
            }
            if(!manifest.Files.Any(f=>f.Path=="DigiTactics.Desktop.exe"))throw new InvalidDataException("Candidate has no application executable.");
            var exact=new HashSet<string>(manifest.Files.Select(f=>f.Path),StringComparer.Ordinal) {ManifestName};
            int count=0;
            foreach(string path in PackageFiles(directory))
            {
                cancellation.ThrowIfCancellationRequested();count++;
                if(!exact.Contains(Path.GetRelativePath(directory,path).Replace('\\','/')))throw new InvalidDataException("Candidate contains unlisted or renamed files.");
            }
            if(count!=paths.Count)throw new InvalidDataException("Candidate file count differs.");
            return new(Path.Combine(directory,"DigiTactics.Desktop.exe"),leases);
        }
        catch {foreach(var lease in leases)lease.Dispose();throw;}
    }
}
