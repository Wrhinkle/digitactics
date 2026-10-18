using System.Security.Cryptography;
using System.Text.Json;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

/// <summary>Opt-in acceptance driver for real Windows dialogs. UI automation or a tester selects
/// only the generated fixtures; production picker callbacks and consumers validate every result.</summary>
internal static class NativeFilePickerSmoke
{
    public static async Task Run(string directory)
    {
        directory=Path.GetFullPath(directory);
        if(Directory.Exists(directory)||File.Exists(directory))throw new IOException("Picker acceptance requires a new isolated fixture directory.");
        Directory.CreateDirectory(directory);
        string[] tracks=[Path.Combine(directory,"Planning 100% \u03a9.wav"),Path.Combine(directory,"Combat \u65e5\u672c ##.wav")];
        for(int i=0;i<tracks.Length;i++)
        {
            using var writer=new BinaryWriter(File.Create(tracks[i]));
            writer.Write("RIFF"u8);writer.Write(40);writer.Write("WAVEfmt "u8);writer.Write(16);writer.Write((short)1);writer.Write((short)2);writer.Write(44100);writer.Write(176400);writer.Write((short)4);writer.Write((short)16);writer.Write("data"u8);writer.Write(4);writer.Write(i);
        }
        string backup=Path.Combine(directory,"Tamer \u00e9 save.json");
        File.WriteAllText(backup,JsonSerializer.Serialize(new {format="digitactics-profile",version=1,storage=new Dictionary<string,string>{{"dt.name","Picker Tamer"},{"dt.future","preserved"}},music=Array.Empty<object>()}));
        var originals=tracks.Append(backup).ToDictionary(p=>p,p=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))));
        var completed=new List<string>();
        void Status(string stage)
        {
            File.WriteAllText(Path.Combine(directory,"picker-status.json"),JsonSerializer.Serialize(new {pid=Environment.ProcessId,stage,tracks,backup,completed}));
            Console.WriteLine("Native picker acceptance: "+stage);
        }
        Status("Select both generated WAV files in the music dialog");
        string[] selected=await MusicFilePicker.Open().WaitAsync(TimeSpan.FromMinutes(5));
        if(!selected.Order(StringComparer.Ordinal).SequenceEqual(tracks.Order(StringComparer.Ordinal)))throw new InvalidDataException("Music dialog did not return exactly both Unicode fixture paths.");
        var library=new MusicLibrary(Path.Combine(directory,"library"));
        foreach(string path in selected)if(!library.AddFile(path))throw new InvalidOperationException(library.Error);
        if(library.Tracks.Count!=2 || !library.Tracks.Select(t=>t.Name).Order().SequenceEqual(tracks.Select(Path.GetFileNameWithoutExtension).Order()))throw new InvalidDataException("Music callback filenames did not survive library import.");
        completed.Add("Music multi-selection, Unicode filenames and library import");
        Status("Cancel the music dialog");
        if((await MusicFilePicker.Open().WaitAsync(TimeSpan.FromMinutes(5))).Length!=0)throw new InvalidDataException("Cancelled music dialog returned selections.");
        completed.Add("Music cancellation");
        Status("Select the generated Tamer save JSON in the profile dialog");
        string? picked=await ProfileFilePicker.Open().WaitAsync(TimeSpan.FromMinutes(5));
        if(picked!=backup)throw new InvalidDataException("Profile dialog did not preserve its Unicode fixture path.");
        var profile=new NativeProfileStore(Path.Combine(directory,"imported-profile"));profile.Import(new ProfileImport(picked));
        if(profile.Progress.Name!="Picker Tamer" || profile.Storage["dt.future"]!="preserved")throw new InvalidDataException("Selected profile did not import intact.");
        completed.Add("Profile selection and safe import");
        Status("Cancel the profile dialog");
        if(await ProfileFilePicker.Open().WaitAsync(TimeSpan.FromMinutes(5)) is not null)throw new InvalidDataException("Cancelled profile dialog returned a selection.");
        completed.Add("Profile cancellation");
        foreach(var (file,hash) in originals)if(Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file)))!=hash)throw new InvalidDataException("Picker acceptance changed an original fixture.");
        completed.Add("Original fixture hashes preserved");Status("passed");
        Console.WriteLine("Native file picker acceptance passed: actual Windows multi-select/single-select/cancel callbacks, Unicode paths, music library and profile import, original bytes preserved.");
    }
}
