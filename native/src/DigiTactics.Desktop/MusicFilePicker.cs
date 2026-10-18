using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Brutal.SdlApi;

namespace DigiTactics.Desktop;

internal static unsafe class MusicFilePicker
{
    public static Task<string[]> Open()
    {
        var completion = new TaskCompletionSource<string[]>(TaskCreationOptions.RunContinuationsAsynchronously);
        var handle = GCHandle.Alloc(completion);
        try { Sdl.PInvoke.ShowOpenFileDialog(&Selected, (void*)GCHandle.ToIntPtr(handle), default, null, 0, "", 1); }
        catch { handle.Free(); throw; }
        return completion.Task;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void Selected(void* context, byte** files, int filter)
    {
        var handle = GCHandle.FromIntPtr((nint)context);
        var completion = (TaskCompletionSource<string[]>)handle.Target!;
        try
        {
            if (files == null) completion.TrySetException(new IOException("The file chooser could not open. Try again."));
            else
            {
                var paths=new List<string>();
                for(int i=0; files[i]!=null; i++) paths.Add(Marshal.PtrToStringUTF8((nint)files[i])!);
                completion.TrySetResult(paths.ToArray());
            }
        }
        catch (Exception error) { completion.TrySetException(error); }
        finally { handle.Free(); }
    }
}
