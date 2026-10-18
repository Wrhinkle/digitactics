using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace DigiTactics.Storage;

/// <summary>Windows child born inside a kill-on-close job. Atomic job assignment avoids an orphan
/// even if the launcher dies between CreateProcess and ResumeThread; descendants inherit the job.</summary>
public sealed class NativeChildProcess : IDisposable
{
    private readonly SafeFileHandle job;
    public Process Process {get;}
    private NativeChildProcess(SafeFileHandle job,Process process) {this.job=job;Process=process;}
    public static NativeChildProcess Start(ProcessStartInfo start)
    {
        if(!OperatingSystem.IsWindows())throw new PlatformNotSupportedException("Native launcher requires Windows.");
        if(start.UseShellExecute || start.RedirectStandardInput || start.RedirectStandardOutput || start.RedirectStandardError || start.Arguments.Length!=0)throw new ArgumentException("Use a direct executable and ArgumentList without redirected streams.");
        var job=CreateJobObjectW(0,null);
        if(job.IsInvalid) {job.Dispose();throw new Win32Exception(Marshal.GetLastWin32Error());}
        nint attributes=0,jobList=0,environment=0;bool initialized=false;Process? process=null;
        ProcessInformation information=default;
        try
        {
            var limits=new ExtendedLimits {Basic=new BasicLimits {Flags=0x2000}};
            if(!SetInformationJobObject(job,9,ref limits,(uint)Marshal.SizeOf<ExtendedLimits>()))throw new Win32Exception(Marshal.GetLastWin32Error());
            nuint size=0;InitializeProcThreadAttributeList(0,1,0,ref size);
            if(size==0)throw new Win32Exception(Marshal.GetLastWin32Error());
            attributes=Marshal.AllocHGlobal(checked((int)size));
            if(!InitializeProcThreadAttributeList(attributes,1,0,ref size))throw new Win32Exception(Marshal.GetLastWin32Error());initialized=true;
            jobList=Marshal.AllocHGlobal(nint.Size);Marshal.WriteIntPtr(jobList,job.DangerousGetHandle());
            // PROC_THREAD_ATTRIBUTE_JOB_LIST: input attribute number 13 (Windows 10+).
            if(!UpdateProcThreadAttribute(attributes,0,0x0002000d,jobList,(nuint)nint.Size,0,0))throw new Win32Exception(Marshal.GetLastWin32Error());
            string block=string.Join('\0',start.Environment.Where(e=>e.Value is not null).OrderBy(e=>e.Key,StringComparer.OrdinalIgnoreCase).Select(e=>e.Key+"="+e.Value))+"\0\0";
            environment=Marshal.StringToHGlobalUni(block);
            string executable=Path.GetFullPath(start.FileName);
            var command=new StringBuilder(Quote(executable));
            foreach(string argument in start.ArgumentList)command.Append(' ').Append(Quote(argument));
            var startup=new StartupEx {Info=new Startup {Size=Marshal.SizeOf<StartupEx>()},Attributes=attributes};
            // CREATE_SUSPENDED | CREATE_UNICODE_ENVIRONMENT | EXTENDED_STARTUPINFO_PRESENT | CREATE_NO_WINDOW.
            if(!CreateProcessW(executable,command,0,0,false,0x08080404,environment,string.IsNullOrEmpty(start.WorkingDirectory)?null:start.WorkingDirectory,ref startup,out information))throw new Win32Exception(Marshal.GetLastWin32Error());
            process=Process.GetProcessById((int)information.ProcessId);
            _=process.Handle; // Acquire a managed handle before allowing the process to exit.
            if(ResumeThread(information.Thread)==uint.MaxValue)throw new Win32Exception(Marshal.GetLastWin32Error());
            return new(job,process);
        }
        catch {job.Dispose();process?.Dispose();throw;}
        finally
        {
            if(information.Thread!=0)CloseHandle(information.Thread);
            if(information.Process!=0)CloseHandle(information.Process);
            if(initialized)DeleteProcThreadAttributeList(attributes);
            if(attributes!=0)Marshal.FreeHGlobal(attributes);
            if(jobList!=0)Marshal.FreeHGlobal(jobList);
            if(environment!=0)Marshal.FreeHGlobal(environment);
        }
    }
    private static string Quote(string value)
    {
        if(value.Contains('\0'))throw new ArgumentException("Process arguments cannot contain NUL.");
        var result=new StringBuilder("\"");int slashes=0;
        foreach(char character in value)
        {
            if(character=='\\') {slashes++;continue;}
            result.Append('\\',character=='"'?slashes*2+1:slashes);slashes=0;result.Append(character);
        }
        return result.Append('\\',slashes*2).Append('"').ToString();
    }
    public void Dispose() {job.Dispose();Process.Dispose();}

    [StructLayout(LayoutKind.Sequential)] private struct BasicLimits
    {public long ProcessTime,JobTime;public uint Flags;public nuint Minimum,Maximum;public uint Active;public nuint Affinity;public uint Priority,Scheduling;}
    [StructLayout(LayoutKind.Sequential)] private struct IoCounters
    {public ulong ReadOperations,WriteOperations,OtherOperations,ReadBytes,WriteBytes,OtherBytes;}
    [StructLayout(LayoutKind.Sequential)] private struct ExtendedLimits
    {public BasicLimits Basic;public IoCounters Io;public nuint ProcessMemory,JobMemory,PeakProcessMemory,PeakJobMemory;}
    [StructLayout(LayoutKind.Sequential)] private struct Startup
    {public int Size;public nint Reserved,Desktop,Title;public uint X,Y,Width,Height,XChars,YChars,Fill,Flags;public ushort Show,ReservedSize;public nint ReservedBytes,Input,Output,Error;}
    [StructLayout(LayoutKind.Sequential)] private struct StartupEx {public Startup Info;public nint Attributes;}
    [StructLayout(LayoutKind.Sequential)] private struct ProcessInformation {public nint Process,Thread;public uint ProcessId,ThreadId;}
    [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)] private static extern SafeFileHandle CreateJobObjectW(nint attributes,string? name);
    [DllImport("kernel32.dll",SetLastError=true)] [return:MarshalAs(UnmanagedType.Bool)] private static extern bool SetInformationJobObject(SafeFileHandle job,int kind,ref ExtendedLimits limits,uint size);
    [DllImport("kernel32.dll",SetLastError=true)] [return:MarshalAs(UnmanagedType.Bool)] private static extern bool InitializeProcThreadAttributeList(nint attributes,int count,int flags,ref nuint size);
    [DllImport("kernel32.dll",SetLastError=true)] [return:MarshalAs(UnmanagedType.Bool)] private static extern bool UpdateProcThreadAttribute(nint attributes,uint flags,nuint attribute,nint value,nuint size,nint previous,nint returnedSize);
    [DllImport("kernel32.dll")] private static extern void DeleteProcThreadAttributeList(nint attributes);
    [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)] [return:MarshalAs(UnmanagedType.Bool)] private static extern bool CreateProcessW(string application,StringBuilder command,nint processSecurity,nint threadSecurity,[MarshalAs(UnmanagedType.Bool)] bool inherit,uint flags,nint environment,string? directory,ref StartupEx startup,out ProcessInformation process);
    [DllImport("kernel32.dll",SetLastError=true)] private static extern uint ResumeThread(nint thread);
    [DllImport("kernel32.dll")] [return:MarshalAs(UnmanagedType.Bool)] private static extern bool CloseHandle(nint handle);
}
