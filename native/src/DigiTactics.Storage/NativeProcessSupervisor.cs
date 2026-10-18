using System.ComponentModel;
using System.Diagnostics;

namespace DigiTactics.Storage;

public sealed record NativeProcessResult(int ExitCode,bool RolledBack,string? Failure);
public enum NativeProcessMode { Interactive, Diagnostic, StartupVerification }

/// <summary>Runs one game lifetime under the caller's launcher-state lease. The start-info factory
/// must verify the selected release and return its executable before any child is started.</summary>
public static class NativeProcessSupervisor
{
    public static async Task<NativeProcessResult> Run(NativeLaunchState state,Func<NativeLaunchSelection,ProcessStartInfo> prepare,
        TimeSpan? startupTimeout=null,CancellationToken cancellation=default,Action? started=null,NativeProcessMode mode=NativeProcessMode.Interactive)
    {
        if(!Enum.IsDefined(mode))throw new ArgumentOutOfRangeException(nameof(mode));
        TimeSpan timeout=startupTimeout??TimeSpan.FromSeconds(90);
        if(timeout<=TimeSpan.Zero || timeout>TimeSpan.FromMinutes(10))throw new ArgumentOutOfRangeException(nameof(startupTimeout));
        bool rolledBack=false;string? failure=null;
        for(int attempt=0;attempt<3;attempt++)
        {
            cancellation.ThrowIfCancellationRequested();
            var selection=mode==NativeProcessMode.Diagnostic?state.SelectActive():state.BeginLaunch();
            ProcessStartInfo start;
            try {start=prepare(selection);}
            catch(Exception error) when(mode!=NativeProcessMode.Diagnostic && selection.Release is not null && StartupError(error))
            {
                Reject(state,selection);rolledBack=true;failure=error.Message;continue;
            }
            if(start.UseShellExecute)throw new ArgumentException("Native child launch must not use a shell.");
            // A normal launch must not inherit a stale startup capability.
            start.Environment.Remove(NativeStartupSignal.PipeVariable);start.Environment.Remove(NativeStartupSignal.TokenVariable);
            bool renderedStartup=mode!=NativeProcessMode.Diagnostic && (mode==NativeProcessMode.StartupVerification || selection.TrialToken is not null || NativeGameInstance.IsInteractiveLaunch(start.ArgumentList.ToArray()));
            using var listener=renderedStartup?new NativeStartupListener():null;
            using var deadline=CancellationTokenSource.CreateLinkedTokenSource(cancellation);
            Task? ready=null;
            if(listener is not null)
            {
                string token=selection.TrialToken??Guid.NewGuid().ToString("N");
                start.Environment[NativeStartupSignal.PipeVariable]=listener.PipeName;
                start.Environment[NativeStartupSignal.TokenVariable]=token;
                deadline.CancelAfter(timeout);
                ready=listener.WaitReady(token,deadline.Token);
            }
            Process? child=null;NativeChildProcess? lifetime=null;
            try
            {
                try {lifetime=NativeChildProcess.Start(start);child=lifetime.Process;}
                catch(Exception error) when(mode!=NativeProcessMode.Diagnostic && selection.Release is not null && StartupError(error))
                {Reject(state,selection);rolledBack=true;failure=error.Message;continue;}
                Task exited=child.WaitForExitAsync(cancellation);
                if(ready is not null)
                {
                    await Task.WhenAny(ready,exited).ConfigureAwait(false);
                    cancellation.ThrowIfCancellationRequested();
                    if(exited.IsCompletedSuccessfully && child.ExitCode==NativeGameInstance.ForwardedExitCode)
                    {
                        if(selection.TrialToken is not null && !state.DeferTrial(selection.TrialToken))throw new InvalidOperationException("Forwarded startup trial disappeared.");
                        return new(0,rolledBack,failure);
                    }
                    if(ready.IsCompletedSuccessfully)
                    {
                        if(selection.TrialToken is not null && !state.Confirm(selection.TrialToken))throw new InvalidOperationException("Startup trial disappeared before confirmation.");
                        deadline.CancelAfter(Timeout.InfiniteTimeSpan);
                    }
                    else
                    {
                        failure=ready.IsFaulted?ready.Exception!.GetBaseException().Message:ready.IsCanceled?"Startup did not become ready before its deadline.":"Game exited before startup was ready.";
                        await Stop(child).ConfigureAwait(false);
                        if(selection.TrialToken is null)return new(child.ExitCode==0?1:child.ExitCode,rolledBack,failure);
                        state.Rollback(selection.TrialToken!);rolledBack=true;continue;
                    }
                }
                if(mode!=NativeProcessMode.Diagnostic)started?.Invoke();
                await exited.ConfigureAwait(false);
                return new(child.ExitCode,rolledBack,failure);
            }
            finally
            {
                deadline.Cancel();
                if(ready is not null)try {await ready.ConfigureAwait(false);}catch(Exception error) when(error is InvalidDataException or IOException or OperationCanceledException or UnauthorizedAccessException) { }
                if(child is not null) {try {await Stop(child).ConfigureAwait(false);}finally {lifetime!.Dispose();}}
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
