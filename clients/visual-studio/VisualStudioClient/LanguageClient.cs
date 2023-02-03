using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VisualStudioClient
{
	[ContentType("CSharp")]
	[Export(typeof(ILanguageClient))]
	public class LanguageClient : ILanguageClient
	{
		public string Name => "Speedy Moq";

		public IEnumerable<string> ConfigurationSections => null;

		public object InitializationOptions => null;

		public IEnumerable<string> FilesToWatch => null;

		public bool ShowNotificationOnInitializeFailed => true;

		public event AsyncEventHandler<EventArgs> StartAsync;
		public event AsyncEventHandler<EventArgs> StopAsync;

		public async Task<Connection> ActivateAsync(CancellationToken token)
		{
			await Task.Yield();
			//DebugLocation();
			ProcessStartInfo info = new ProcessStartInfo();
			//info.FileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Server", @"MockLanguageServer.exe");
			//info.Arguments = "bar";

			// potential problems: can this net framework thing not interact with a net6 dll/exe?
			//info.FileName = @"C:\src\daveoftheparty\speedy-moq\server\OmniLsp\bin\Debug\net6.0\OmniLsp.dll";
			info.FileName = @"C:\src\daveoftheparty\speedy-moq\server\OmniLsp\bin\Debug\net6.0\OmniLsp.exe";
			info.Arguments = "true";
			info.RedirectStandardInput = true;
			info.RedirectStandardOutput = true;
			info.UseShellExecute = false;
			info.CreateNoWindow = true;

#warning this process stays running, do something on deactivateasync or whatever the hook is... probably why Bicep coders used some sort of process manager
			Process process = new Process();
			process.StartInfo = info;

			if (process.Start())
			{
				return new Connection(process.StandardOutput.BaseStream, process.StandardInput.BaseStream);
			}

			return null;
		}

		public async Task OnLoadedAsync()
		{
			await StartAsync.InvokeAsync(this, EventArgs.Empty);
		}

		public Task OnServerInitializeFailedAsync(Exception e)
		{
			return Task.CompletedTask;
		}

		public Task OnServerInitializedAsync()
		{
			return Task.CompletedTask;
		}

		public Task<InitializationFailureContext> OnServerInitializeFailedAsync(ILanguageClientInitializationInfo initializationState)
		{
			var message = "Oh no! Foo Language Client failed to activate, now we can't test LSP! :(";
			var exception = initializationState.InitializationException?.ToString() ?? string.Empty;
			message = $"{message}\n {exception}";

			var failureContext = new InitializationFailureContext()
			{
				FailureMessage = message,
			};

			return Task.FromResult(failureContext);
		}

		public void DebugLocation()
		{
			try
			{
				var a = Assembly.GetExecutingAssembly().Location;
				var b = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			} catch (Exception e) { }

			try
			{
				var c = Assembly.GetEntryAssembly().GetName().CodeBase;
				var d = new Uri(Assembly.GetEntryAssembly().GetName().CodeBase);
				var e = new FileInfo(d.AbsolutePath).Directory;
				var f = e.FullName;
			} catch (Exception e) { }

			try
			{
		
				/*
					public static string GetDirectoryPath(this Assembly assembly)
					{
						string filePath = new Uri(assembly.CodeBase).LocalPath;
						return Path.GetDirectoryName(filePath);            
					}
				 */
				var g = new Uri(Assembly.GetExecutingAssembly().CodeBase); // check localpath, absolutepath
			} catch (Exception e) { }
		}
	}
}
