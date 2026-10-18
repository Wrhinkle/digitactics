using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Brutal.SdlApi;

namespace DigiTactics.Desktop;

internal static unsafe class ProfileFilePicker
{
    public static Task<string?> Open()
    {
        var completion = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var handle = GCHandle.Alloc(completion);
        try { Sdl.PInvoke.ShowOpenFileDialog(&Selected, (void*)GCHandle.ToIntPtr(handle), default, null, 0, "", 0); }
        catch { handle.Free(); throw; }
        return completion.Task;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void Selected(void* context, byte** files, int filter)
    {
        var handle = GCHandle.FromIntPtr((nint)context);
        var completion = (TaskCompletionSource<string?>)handle.Target!;
        try
        {
            if (files == null) completion.TrySetException(new IOException("The file chooser could not open. Try again."));
            else completion.TrySetResult(files[0] == null ? null : Marshal.PtrToStringUTF8((nint)files[0]));
        }
        catch (Exception error) { completion.TrySetException(error); }
        finally { handle.Free(); }
    }
}
