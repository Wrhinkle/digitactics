using DigiTactics.Storage;

internal static class NativeApplicationErrorChecks
{
    public static bool IsGuiExecutable(string executable)
    {
        using var file=File.OpenRead(Path.GetFullPath(executable));
        using var image=new System.Reflection.PortableExecutable.PEReader(file);
        return image.PEHeaders.PEHeader?.Subsystem==System.Reflection.PortableExecutable.Subsystem.WindowsGui;
    }

    public static int Run()
    {
        string directory=Path.Combine(Path.GetTempPath(),"digitactics-error-log-"+Guid.NewGuid().ToString("N"));
        int checks=0;
        void Check(bool condition,string message){checks++;if(!condition)throw new InvalidOperationException(message);}
        try
        {
            var failure=new InvalidOperationException("Could not load model Ω",new IOException("Missing asset"));
            string path=NativeApplicationError.WriteLog(directory,"game",failure);
            string first=File.ReadAllText(path);
            Check(first.Contains("Could not load model Ω",StringComparison.Ordinal)&&first.Contains("Missing asset",StringComparison.Ordinal),"Fatal log lost Unicode or the underlying cause.");
            NativeApplicationError.WriteLog(directory,"game",new Exception("Second failure"));
            Check(File.ReadAllText(path).StartsWith(first,StringComparison.Ordinal),"Fatal log overwrote the preceding error.");
            string launcher=NativeApplicationError.WriteLog(directory,"launcher",failure);
            Check(launcher!=path&&File.Exists(path),"Launcher and game errors share a file.");
            File.WriteAllText(path,new string('x',1024*1024+1));
            NativeApplicationError.WriteLog(directory,"game",failure);
            Check(new FileInfo(path+".previous").Length==1024*1024+1&&File.ReadAllText(path).Contains("Could not load model Ω",StringComparison.Ordinal),"Log rotation lost previous or current failure.");
            NativeApplicationError.WriteLog(directory,"game",new Exception(new string('y',100000)));
            Check(new FileInfo(path).Length<40000&&File.ReadAllText(path).Contains("[truncated]",StringComparison.Ordinal),"A single oversized failure bypassed log bounds.");
            bool rejected=false;try{NativeApplicationError.WriteLog(directory,"../other",failure);}catch(ArgumentException){rejected=true;}
            Check(rejected,"Invalid log component was accepted.");
        }
        finally
        {
            string resolved=Path.GetFullPath(directory);
            if(Path.GetDirectoryName(resolved)!=Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar))
                throw new InvalidOperationException("Unexpected error-log test cleanup directory.");
            if(Directory.Exists(resolved))Directory.Delete(resolved,true);
        }
        Console.WriteLine($"Native fatal diagnostics: {checks} persistence, cause, Unicode and bounded-log checks passed.");
        return checks;
    }
}
