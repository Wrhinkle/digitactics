using System.ComponentModel;
using System.Diagnostics;

namespace DigiTactics.Storage;

public sealed record NativeProcessResult(int ExitCode,bool RolledBack,string? Failure);

/// <summary>Runs one game lifetime under the caller's launcher-state lease. The start-info factory
/// must verify the selected release and return its executable before any child is started.</summary>
public static class NativeProcessSupervisor
{
    public static async Task<NativeProcessResult> Run(NativeLaunchState state,Func<NativeLaunchSelection,ProcessStartInfo> prepare,
        TimeSpan? startupTimeout=null,CancellationToken cancellation=default)
    {
        TimeSpan timeout=startupTimeout??TimeSpan.FromSeconds(90);
        if(timeout<=TimeSpan.Zero || timeout>TimeSpan.FromMinutes(10))throw new ArgumentOutOfRangeException(nameof(startupTimeout));
        bool rolledBack=false;string? failure=null;
        for(int attempt=0;attempt<3;attempt++)
        {
            cancellation.ThrowIfCancellationRequested();
            var selection=state.BeginLaunch();
            ProcessStartInfo start;
            try {start=prepare(selection);}
            catch(Exception error) when(selection.Release is not null && StartupError(error))
            {
                Reject(state,selection);rolledBack=true;failure=error.Message;continue;
            }
            if(start.UseShellExecute)throw new ArgumentException("Native child launch must not use a shell.");
            // A normal launch must not inherit a stale startup capability.
            start.Environment.Remove(NativeStartupSignal.PipeVariable);start.Environment.Remove(NativeStartupSignal.TokenVariable);
            using var listener=selection.TrialToken is null?null:new NativeStartupListener();
            using var deadline=CancellationTokenSource.CreateLinkedTokenSource(cancellation);
            Task? ready=null;
            if(listener is not null)
            {
                start.Environment[NativeStartupSignal.PipeVariable]=listener.PipeName;
                start.Environment[NativeStartupSignal.TokenVariable]=selection.TrialToken;
                deadline.CancelAfter(timeout);
                ready=listener.WaitReady(selection.TrialToken!,deadline.Token);
            }
            Process? child=null;
            try
            {
                try {child=Process.Start(start)??throw new IOException("Game process did not start.");}
                catch(Exception error) when(selection.Release is not null && StartupError(error))
                {Reject(state,selection);rolledBack=true;failure=error.Message;continue;}
                Task exited=child.WaitForExitAsync(cancellation);
                if(ready is not null)
                {
                    await Task.WhenAny(ready,exited).ConfigureAwait(false);
                    cancellation.ThrowIfCancellationRequested();
                    if(ready.IsCompletedSuccessfully)
                    {
                        if(!state.Confirm(selection.TrialToken!))throw new InvalidOperationException("Startup trial disappeared before confirmation.");
                        deadline.CancelAfter(Timeout.InfiniteTimeSpan);
                    }
                    else
                    {
                        failure=ready.IsFaulted?ready.Exception!.GetBaseException().Message:ready.IsCanceled?"Startup did not become ready before its deadline.":"Game exited before startup was ready.";
                        await Stop(child).ConfigureAwait(false);
                        state.Rollback(selection.TrialToken!);rolledBack=true;continue;
                    }
                }
                await exited.ConfigureAwait(false);
                return new(child.ExitCode,rolledBack,failure);
            }
            finally
            {
                deadline.Cancel();
                if(ready is not null)try {await ready.ConfigureAwait(false);}catch(Exception error) when(error is InvalidDataException or IOException or OperationCanceledException or UnauthorizedAccessException) { }
                if(child is not null) {try {await Stop(child).ConfigureAwait(false);}finally {child.Dispose();}}
            }
        }
        throw new IOException("No working native application could be started: "+failure);
    }

    private static void Reject(NativeLaunchState state,NativeLaunchSelection selection)
    {
        if(selection.TrialToken is not null)state.Rollback(selection.TrialToken);
        else if(selection.Release is not null)state.RejectActive(selection.Release);
    }
    private static bool StartupError(Exception error)=>error is InvalidDataException or IOException or UnauthorizedAccessException or Win32Exception or InvalidOperationException;
    private static async Task Stop(Process child)
    {
        if(!child.HasExited)
        {
            try {child.Kill(entireProcessTree:true);}catch(InvalidOperationException) when(child.HasExited) { }
            await child.WaitForExitAsync().ConfigureAwait(false);
        }
    }
}
