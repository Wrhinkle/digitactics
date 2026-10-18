using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using DigiTactics.Storage;

internal static class NativeUpdateChecks
{
    public static int Run()
    {
        int checks=0;
        string directory=Path.Combine(Path.GetTempPath(),"digitactics-update-test-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string archive=Path.Combine(directory,"update.zip");
        static string Hash(byte[] bytes)=>Convert.ToHexStringLower(SHA256.HashData(bytes));
        void Check(bool value,string reason) {checks++;if(!value)throw new Exception("Native updater: "+reason);}
        void Reject(Action action,string reason)
        {
            try {action();} catch(InvalidDataException) {checks++;return;} catch(JsonException) {checks++;return;}
            throw new Exception("Accepted invalid native update: "+reason);
        }
        NativePackageExpectation Build(Action<JsonObject>? edit=null,string? extra=null,int attributes=0,string? rawManifest=null,bool listExtra=false,bool omitExecutable=false)
        {
            var files=new Dictionary<string,byte[]> { ["DigiTactics.Desktop.exe"]=[1,2,3], ["content/nested/data.bin"]=[4,5,6] };
            if(omitExecutable)files.Remove("DigiTactics.Desktop.exe");
            if(listExtra && extra is not null)files.Add(extra,[7]);
            var manifest=JsonSerializer.SerializeToNode(new NativePackageManifest(1,"1.2.3","win-x64",1,
                files.Select(f=>new NativePackageFile(f.Key,f.Value.Length,Hash(f.Value))).ToArray()),
                new JsonSerializerOptions {PropertyNamingPolicy=JsonNamingPolicy.CamelCase})!.AsObject();
            edit?.Invoke(manifest);
            using(var output=File.Create(archive))
            using(var zip=new ZipArchive(output,ZipArchiveMode.Create))
            {
                foreach(var file in files)
                {
                    var entry=zip.CreateEntry(file.Key);entry.ExternalAttributes=attributes;
                    using var stream=entry.Open();stream.Write(file.Value);
                }
                if(extra is not null && !listExtra) {var entry=zip.CreateEntry(extra);entry.ExternalAttributes=0;using var stream=entry.Open();stream.WriteByte(7);}
                var metadata=zip.CreateEntry(NativeUpdatePackage.ManifestName);metadata.ExternalAttributes=0;
                using var data=metadata.Open();data.Write(Encoding.UTF8.GetBytes(rawManifest??manifest.ToJsonString()));
            }
            byte[] bytes=File.ReadAllBytes(archive);return new("1.2.3",bytes.Length,Hash(bytes));
        }
        try
        {
            var expected=Build();byte[] original=File.ReadAllBytes(archive);
            var valid=NativeUpdatePackage.Inspect(archive,expected,1);
            Check(valid.Files.Length==2 && valid.Version=="1.2.3","valid nested native archive");
            Check(File.ReadAllBytes(archive).SequenceEqual(original) && Directory.GetFileSystemEntries(directory).Length==1,"inspection does not extract or mutate");
            Reject(()=>NativeUpdatePackage.Inspect(archive,expected with {Size=expected.Size+1},1),"outer size");
            Reject(()=>NativeUpdatePackage.Inspect(archive,expected with {Sha256=new string('0',64)},1),"outer digest");
            Reject(()=>NativeUpdatePackage.Inspect(archive,expected with {Version="1.2.4"},1),"descriptor version");
            foreach(string version in new[]{"1.2","01.2.3","1.2.3-beta","1.2.3\n","2147483648.1.1"})
                Check(!NativeUpdatePackage.ValidVersion(version),"invalid version "+version);
            using(var cancel=new CancellationTokenSource())
            {
                cancel.Cancel();try {NativeUpdatePackage.Inspect(archive,expected,1,cancel.Token);throw new Exception("Ignored cancellation");}
                catch(OperationCanceledException) {checks++;}
            }
            foreach(Action<JsonObject> edit in new Action<JsonObject>[] {
                m=>m["version"]="2.0.0",m=>m["platform"]="linux-x64",m=>m["minimumLauncher"]=2,
                m=>m["schemaVersion"]=2,m=>m["extra"]=true,m=>m["files"]=null,
                m=>m["files"]![0]!["sha256"]=new string('0',64),m=>m["files"]![0]!["size"]=4,
                m=>m["files"]![0]!["path"]="other.exe",m=>m["files"]![0]!["path"]="digitactics.desktop.exe",
                m=>m["files"]![0]=null,m=>m["files"]!.AsArray().RemoveAt(0),
                m=>m["files"]![1]=m["files"]![0]!.DeepClone()
            }) {expected=Build(edit);Reject(()=>NativeUpdatePackage.Inspect(archive,expected,1),"manifest mismatch");}
            foreach(string path in new[]{"../escape","/absolute","C:/absolute","a\\b","a//b","a/./b","a/../b","a.","a ","a:stream","NUL.txt","COM1","LPT\u00b2.txt","a\u0001b","DigiTactics.Desktop.exe/child","DIGITACTICS.DESKTOP.EXE"})
            {expected=Build(extra:path);Reject(()=>NativeUpdatePackage.Inspect(archive,expected,1),"unsafe or unlisted "+path);}
            foreach(string path in new[]{"DigiTactics.Desktop.exe/child","native-package.json/child"})
            {expected=Build(extra:path,listExtra:true);Reject(()=>NativeUpdatePackage.Inspect(archive,expected,1),"listed file/directory collision");}
            expected=Build(omitExecutable:true);Reject(()=>NativeUpdatePackage.Inspect(archive,expected,1),"missing executable");
            expected=Build(extra:"content/zero.bin",listExtra:true);Check(NativeUpdatePackage.Inspect(archive,expected,1).Files.Length==3,"additional listed content");
            foreach(int attributes in new[]{unchecked((int)0xa0000000),0x400,0x10})
            {expected=Build(attributes:attributes);Reject(()=>NativeUpdatePackage.Inspect(archive,expected,1),"link or directory");}
            expected=Build(rawManifest:"{\"schemaVersion\":1,\"schemaVersion\":1}");
            Reject(()=>NativeUpdatePackage.Inspect(archive,expected,1),"duplicate JSON fields");
            Check(Directory.GetFileSystemEntries(directory).Length==1,"all rejected packages leave installation untouched");
            string stageRoot=Path.Combine(directory,"candidates");
            expected=Build();
            var staged=NativeUpdatePackage.Stage(archive,stageRoot,expected,1);
            Check(File.ReadAllBytes(Path.Combine(staged.Directory,"content/nested/data.bin")).SequenceEqual(new byte[]{4,5,6}),"stage preserves nested bytes");
            Check(File.ReadAllBytes(Path.Combine(staged.Directory,"DigiTactics.Desktop.exe")).SequenceEqual(new byte[]{1,2,3}),"stage preserves executable bytes");
            Check(File.Exists(Path.Combine(staged.Directory,NativeUpdatePackage.ManifestName)) && !Path.GetFileName(staged.Directory).StartsWith(".partial"),"complete candidate has manifest");
            var second=NativeUpdatePackage.Stage(archive,stageRoot,expected,1);
            Check(second.Directory!=staged.Directory && Directory.GetDirectories(stageRoot).Length==2,"staging never overwrites an existing candidate");
            string untouched=Path.Combine(directory,"invalid-stage");
            Reject(()=>NativeUpdatePackage.Stage(archive,untouched,expected with {Sha256=new string('0',64)},1),"invalid download cannot stage");
            Check(!Directory.Exists(untouched),"invalid download does not create staging root");
            using(var cancel=new CancellationTokenSource())
            {
                cancel.Cancel();try {NativeUpdatePackage.Stage(archive,untouched,expected,1,cancel.Token);throw new Exception("Ignored stage cancellation");}
                catch(OperationCanceledException) {checks++;}
            }
            Check(!Directory.Exists(untouched),"cancelled verification does not create staging root");
            string occupied=Path.Combine(directory,"occupied");File.WriteAllText(occupied,"keep");
            try {NativeUpdatePackage.Stage(archive,occupied,expected,1);throw new Exception("Accepted file as staging root");}
            catch(IOException) {checks++;}
            Check(File.ReadAllText(occupied)=="keep","staging root failure preserves existing file");
            string published=Path.Combine(directory,"published");Directory.CreateDirectory(Path.Combine(published,"content"));
            File.WriteAllBytes(Path.Combine(published,"DigiTactics.Desktop.exe"),[1,2,3,4]);
            File.WriteAllBytes(Path.Combine(published,"content/empty.bin"),[]);
            string generated=Path.Combine(directory,"generated.zip");
            var generatedExpected=NativeUpdatePackage.Create(published,generated,"3.2.1",2);
            var generatedStage=NativeUpdatePackage.Stage(generated,stageRoot,generatedExpected,2);
            using(var lease=NativeUpdatePackage.OpenCandidate(stageRoot,generatedStage.Release,2))
            {
                Check(lease.Executable==Path.Combine(generatedStage.Directory,"DigiTactics.Desktop.exe"),"validated candidate selects its executable");
                try {File.WriteAllBytes(lease.Executable,[9]);throw new Exception("Candidate writable during lease");}catch(IOException) {checks++;}
                try {File.Move(lease.Executable,lease.Executable+".moved");throw new Exception("Candidate replaceable during lease");}catch(IOException) {checks++;}
            }
            Reject(()=>NativeUpdatePackage.OpenCandidate(stageRoot,generatedStage.Release with {ManifestSha256=null},2),"unpinned manifest");
            Reject(()=>NativeUpdatePackage.OpenCandidate(stageRoot,generatedStage.Release,1),"candidate launcher compatibility");
            string candidateExe=Path.Combine(generatedStage.Directory,"DigiTactics.Desktop.exe");
            File.WriteAllBytes(candidateExe,[9,9,9,9]);
            Reject(()=>NativeUpdatePackage.OpenCandidate(stageRoot,generatedStage.Release,2),"modified candidate file");
            File.WriteAllBytes(candidateExe,[1,2,3,4]);
            string extraCandidate=Path.Combine(generatedStage.Directory,"injected.dll");File.WriteAllBytes(extraCandidate,[1]);
            Reject(()=>NativeUpdatePackage.OpenCandidate(stageRoot,generatedStage.Release,2),"unlisted candidate library");File.Delete(extraCandidate);
            string candidateManifest=Path.Combine(generatedStage.Directory,NativeUpdatePackage.ManifestName);byte[] pinned=File.ReadAllBytes(candidateManifest);
            File.AppendAllText(candidateManifest," ");
            Reject(()=>NativeUpdatePackage.OpenCandidate(stageRoot,generatedStage.Release,2),"changed candidate manifest");File.WriteAllBytes(candidateManifest,pinned);
            using(var lease=NativeUpdatePackage.OpenCandidate(stageRoot,generatedStage.Release,2))checks++;
            Check(generatedStage.Manifest.Files.Length==2 && new FileInfo(Path.Combine(generatedStage.Directory,"content/empty.bin")).Length==0,"published directory packages and stages including empty files");
            Check(File.ReadAllBytes(Path.Combine(generatedStage.Directory,"DigiTactics.Desktop.exe")).SequenceEqual(new byte[]{1,2,3,4}),"package roundtrip preserves application");
            Reject(()=>NativeUpdatePackage.Inspect(generated,generatedExpected,1),"generated package enforces minimum launcher");
            string repeated=Path.Combine(directory,"repeated.zip");
            var repeatedExpected=NativeUpdatePackage.Create(published,repeated,"3.2.1",2);
            Check(repeatedExpected==generatedExpected,"identical source produces deterministic package");
            try {NativeUpdatePackage.Create(published,generated,"3.2.1",2);throw new Exception("Overwrote release");}
            catch(IOException) {checks++;}
            Check(File.ReadAllBytes(generated).SequenceEqual(File.ReadAllBytes(repeated)),"existing archive preserved");
            try {NativeUpdatePackage.Create(published,Path.Combine(published,"self.zip"),"3.2.1",2);throw new Exception("Packaged own output");}
            catch(IOException) {checks++;}
            File.WriteAllText(Path.Combine(published,NativeUpdatePackage.ManifestName),"stale");
            string failed=Path.Combine(directory,"failed.zip");
            Reject(()=>NativeUpdatePackage.Create(published,failed,"3.2.1",2),"stale source manifest");
            Check(!File.Exists(failed) && Directory.GetFiles(directory,".package-*.tmp").Length==0,"failed packaging leaves no published or temporary archive");


        }
        finally {Directory.Delete(directory,true);}
        Console.WriteLine($"Native update package validation: {checks} checks passed.");return checks;
    }
}
