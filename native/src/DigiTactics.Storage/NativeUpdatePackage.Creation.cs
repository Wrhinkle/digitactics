using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;

namespace DigiTactics.Storage;

public static partial class NativeUpdatePackage
{
    /// <summary>Packages a published native application without modifying it. Output must be outside
    /// the source tree. Existing release archives are never overwritten.</summary>
    public static NativePackageExpectation Create(string sourceDirectory,string archivePath,string version,int minimumLauncher,CancellationToken cancellation=default)
    {
        if(!ValidVersion(version) || minimumLauncher<1)throw new InvalidDataException("Invalid native release version.");
        string source=Path.GetFullPath(sourceDirectory),destination=Path.GetFullPath(archivePath);
        EnsureOrdinaryAncestors(source);
        if(!Directory.Exists(source))throw new DirectoryNotFoundException(source);
        if(destination.StartsWith(Path.TrimEndingDirectorySeparator(source)+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))throw new IOException("Release archive must be outside its source tree.");
        if(Path.Exists(destination))throw new IOException("Release archive already exists.");
        string parent=Path.GetDirectoryName(destination)!;
        EnsureOrdinaryAncestors(parent);Directory.CreateDirectory(parent);EnsureOrdinaryAncestors(parent);
        string temporary=Path.Combine(parent,".package-"+Guid.NewGuid().ToString("N")+".tmp");
        try
        {
            using(var output=new FileStream(temporary,FileMode.CreateNew,FileAccess.ReadWrite,FileShare.None))
            {
                using(var zip=new ZipArchive(output,ZipArchiveMode.Create,leaveOpen:true))
                {
                    var files=new List<NativePackageFile>();long total=0;byte[] buffer=new byte[128*1024];
                    foreach(string path in PackageFiles(source).Order(StringComparer.Ordinal))
                    {
                        cancellation.ThrowIfCancellationRequested();
                        string relative=Path.GetRelativePath(source,path).Replace('\\','/');ValidatePath(relative);
                        if(relative.Equals(ManifestName,StringComparison.OrdinalIgnoreCase))throw new InvalidDataException("Source already contains an update manifest.");
                        if(files.Count>=MaximumFiles)throw new InvalidDataException("Too many release files.");
                        using var input=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read);
                        total=checked(total+input.Length);
                        if(total>MaximumExpandedBytes)throw new InvalidDataException("Release exceeds expanded size limit.");
                        var entry=zip.CreateEntry(relative,CompressionLevel.Optimal);entry.ExternalAttributes=0;
                        entry.LastWriteTime=new DateTimeOffset(2000,1,1,0,0,0,TimeSpan.Zero);
                        using(var target=entry.Open())
                        using(var hash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256))
                        {
                            int read;long size=0;
                            while((read=input.Read(buffer))>0)
                            {
                                cancellation.ThrowIfCancellationRequested();size=checked(size+read);
                                if(size>input.Length)throw new IOException("Release source changed while packaging.");
                                target.Write(buffer,0,read);hash.AppendData(buffer,0,read);
                            }
                            if(size!=input.Length)throw new IOException("Release source changed while packaging.");
                            files.Add(new(relative,size,Convert.ToHexStringLower(hash.GetHashAndReset())));
                        }
                    }
                    var manifest=new NativePackageManifest(1,version,"win-x64",minimumLauncher,files.ToArray());
                    var metadata=zip.CreateEntry(ManifestName,CompressionLevel.Optimal);metadata.ExternalAttributes=0;
                    metadata.LastWriteTime=new DateTimeOffset(2000,1,1,0,0,0,TimeSpan.Zero);
                    using var contents=metadata.Open();JsonSerializer.Serialize(contents,manifest,new JsonSerializerOptions {PropertyNamingPolicy=JsonNamingPolicy.CamelCase});
                }
                output.Flush(flushToDisk:true);
            }
            using var verification=new FileStream(temporary,FileMode.Open,FileAccess.Read,FileShare.Read|FileShare.Delete);
            var expected=new NativePackageExpectation(version,verification.Length,Hash(verification,verification.Length,cancellation));
            verification.Position=0;Inspect(verification,expected,minimumLauncher,cancellation);
            cancellation.ThrowIfCancellationRequested();
            File.Move(temporary,destination,overwrite:false);
            return expected;
        }
        finally {if(File.Exists(temporary))File.Delete(temporary);}
    }

    private static IEnumerable<string> PackageFiles(string directory)
    {
        foreach(string path in Directory.EnumerateFileSystemEntries(directory))
        {
            var attributes=File.GetAttributes(path);
            if((attributes&FileAttributes.ReparsePoint)!=0)throw new IOException("Release sources cannot contain links.");
            if((attributes&FileAttributes.Directory)!=0)
                foreach(string child in PackageFiles(path))yield return child;
            else yield return path;
        }
    }
}
