using System.Text.Json.Nodes;
using DigiTactics.Storage;

internal static class MusicLibraryChecks
{
    public static int Run(string root)
    {
        int checks=0;
        void Check(bool ok,string reason) { checks++; if(!ok) throw new InvalidOperationException("Music library: "+reason); }
        string directory=Path.Combine(Path.GetTempPath(),"digitactics-music-test-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string backup=Path.Combine(root,"tests/fixtures/legacy-profile.json"); byte[] original=File.ReadAllBytes(backup);
            string folder=Path.Combine(directory,"music"); var library=new MusicLibrary(folder);
            Check(library.ImportLegacy(backup),library.Error??"import");
            var track=library.Tracks.Single();
            Check(track.Id=="track-1" && track.Use=="combat" && track.Added==123,"metadata and order");
            Check(track.Metadata["future"]!["keep"]!.GetValue<bool>() && track.Blob["name"]!.GetValue<string>()=="original.wav","unknown metadata and blob metadata");
            Check(library.ReadAudio(track.Id).SequenceEqual(new byte[]{0,1,2,254,255}),"exact decoded bytes");
            using(var lease=library.OpenAudio(track.Id))
            {
                Check(lease.Position==0 && lease.Length==5,"verified stream starts at the beginning");
                try { using var writer=new FileStream(lease.Name,FileMode.Open,FileAccess.Write,FileShare.ReadWrite); throw new Exception("Playback file was writable"); }
                catch(IOException) { checks++; }
            }
            Check(File.ReadAllBytes(backup).SequenceEqual(original),"source archive unchanged");
            Check(Directory.GetFiles(folder,"*.audio").Single()==Path.Combine(folder,track.Sha256+".audio"),"names cannot become filesystem paths");
            string indexPath=Path.Combine(folder,"library.json"), firstIndex=File.ReadAllText(Path.Combine(folder,"library.json"));
            Check(library.ImportLegacy(backup) && library.Tracks.Count==1 && File.ReadAllText(indexPath)==firstIndex,"idempotent import");
            string invalidBackup=Path.Combine(directory,"invalid-backup.json");
            var badBackup=JsonNode.Parse(original)!; badBackup["music"]![0]!["data"]="AAEC/v4="; File.WriteAllText(invalidBackup,badBackup.ToJsonString());
            Check(!library.ImportLegacy(invalidBackup) && File.ReadAllText(indexPath)==firstIndex && library.Tracks.Count==1,"failed backup integrity check leaves index unchanged");
            Check(library.SetUse(track.Id,"planning"),"phase edit");
            var restarted=new MusicLibrary(folder);
            Check(restarted.Tracks.Single().Use=="planning" && restarted.Tracks.Single().Metadata["future"]!["keep"]!.GetValue<bool>(),"phase and unknown fields survive restart");
            Check(!library.SetUse(track.Id,"invalid") && library.Tracks.Single().Use=="planning","reject invalid phase");
            using(var guard=new FileStream(Path.Combine(folder,"library.json.lock"),FileMode.Open,FileAccess.ReadWrite,FileShare.None))
                Check(!restarted.SetUse(track.Id,"both"),"locked edit rejected");
            Check(restarted.SetUse(track.Id,"both"),"locked edit retried");
            Check(!library.Remove(track.Id) && new MusicLibrary(folder).Tracks.Count==1,"stale writer preserves another instance's edits");
            restarted=new MusicLibrary(folder);
            string audioPath=Path.Combine(folder,track.Sha256+".audio"); File.WriteAllBytes(audioPath,[0,1,2,254,254]);
            try { restarted.ReadAudio(track.Id); throw new Exception("Corruption accepted"); } catch(InvalidDataException) { checks++; }
            Check(File.ReadAllBytes(audioPath).Last()==254,"corrupt audio preserved");
            File.WriteAllBytes(audioPath,[0,1,2,254,255]);
            Check(restarted.Remove(track.Id) && restarted.ImportLegacy(backup) && restarted.Tracks.Count==0,"deleted imports do not resurrect");
            string added=Path.Combine(directory,"local track.wav"); File.WriteAllBytes(added,[0,1,2,254,255]);
            Check(restarted.AddFile(added) && restarted.Tracks.Single().Name=="local track" && restarted.Tracks.Single().Use=="both","new local track metadata");
            Check(Directory.GetFiles(folder,"*.audio").Length==1 && restarted.ReadAudio(restarted.Tracks.Single().Id).Length==5,"duplicate content shares immutable bytes");
            Check(!restarted.AddFile(backup) && restarted.Tracks.Count==1,"non-audio import rejected");
            var stale=new MusicLibrary(folder);
            Check(restarted.AddFile(added) && restarted.Tracks.Count==2,"prepare multi-track clear");
            Check(!stale.Clear() && new MusicLibrary(folder).Tracks.Count==2,"stale clear preserves concurrent additions");
            using(var guard=new FileStream(Path.Combine(folder,"library.json.lock"),FileMode.Open,FileAccess.ReadWrite,FileShare.None))
                Check(!restarted.Clear() && restarted.Tracks.Count==2,"locked clear leaves all tracks intact");
            Check(restarted.Clear() && new MusicLibrary(folder).Tracks.Count==0,"clear persists empty library");
            Check(restarted.ImportLegacy(backup) && restarted.Tracks.Count==0,"clear preserves migration history");
            Check(File.ReadAllBytes(added).SequenceEqual(new byte[]{0,1,2,254,255}) && File.Exists(audioPath),"clear preserves original files and recovery blobs");
            Check(restarted.Clear() && restarted.AddFile(added),"empty clear and subsequent add succeed");
            var invalid=JsonNode.Parse(File.ReadAllText(Path.Combine(folder,"library.json")))!;
            invalid["tracks"]![0]!.AsObject().Remove("sha256"); string broken=invalid.ToJsonString(); File.WriteAllText(Path.Combine(folder,"library.json"),broken);
            var damaged=new MusicLibrary(folder);
            Check(damaged.Error is not null && !damaged.AddFile(added) && File.ReadAllText(Path.Combine(folder,"library.json"))==broken,"malformed index remains unchanged");
            Check(Directory.GetFiles(folder,"*.tmp").Length==0,"no temporary files remain");
            Check(File.ReadAllBytes(backup).SequenceEqual(original),"archive remains unchanged after all edits");
            var nativeProfile=new NativeProfileStore(Path.Combine(directory,"profile")); nativeProfile.Import(new ProfileImport(backup));
            var migrated=nativeProfile.OpenMusicLibrary();
            Check(migrated.Error is null && migrated.Tracks.Count==1 && migrated.ReadAudio("track-1").SequenceEqual(new byte[]{0,1,2,254,255}),"profile-bound migration");
            Check(migrated.SetUse("track-1","both") && new NativeProfileStore(Path.Combine(directory,"profile")).OpenMusicLibrary().Tracks.Single().Use=="both","reopening profile preserves native music edits");
            string archive=Directory.GetFiles(Path.Combine(directory,"profile","imports"),"*.json").Single();
            var changed=JsonNode.Parse(File.ReadAllText(archive))!; changed["music"]![0]!["metadata"]!["name"]="Changed archive"; File.WriteAllText(archive,changed.ToJsonString());
            var protectedLibrary=nativeProfile.OpenMusicLibrary();
            Check(protectedLibrary.Error is not null && protectedLibrary.Tracks.Single().Name!="Changed archive","tampered archive reference cannot replace library");
            Check(File.ReadAllText(archive).Contains("Changed archive",StringComparison.Ordinal),"changed archive preserved for recovery");

            Console.WriteLine($"Music library: {checks} import, integrity, metadata, persistence and atomic-edit checks passed."); return checks;
        }
        finally
        {
            string resolved=Path.GetFullPath(directory);
            if(!resolved.StartsWith(Path.GetFullPath(Path.GetTempPath()),StringComparison.OrdinalIgnoreCase) || !Path.GetFileName(resolved).StartsWith("digitactics-music-test-",StringComparison.Ordinal)) throw new InvalidOperationException("Unsafe music test cleanup.");
            Directory.Delete(resolved,true);
        }
    }
}
