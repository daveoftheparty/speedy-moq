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
	public class LanguageClient : ILanguageClient, IDisposable
	{
		public string Name => "Speedy Moq";

		public IEnumerable<string> ConfigurationSections => null;

		public object InitializationOptions => null;

		public IEnumerable<string> FilesToWatch => null;

		public bool ShowNotificationOnInitializeFailed => true;

		public event AsyncEventHandler<EventArgs> StartAsync;
		public event AsyncEventHandler<EventArgs> StopAsync;

		private Process _languageServer;

		public async Task<Connection> ActivateAsync(CancellationToken token)
		{
			await Task.Yield();
			DebugLocation();
			ProcessStartInfo info = new ProcessStartInfo();

			//info.FileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Server", @"MockLanguageServer.exe");
			//info.Arguments = "bar";
			/*
            string vsixInstallPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string languageServerExePath = Path.Combine(vsixInstallPath, BicepLanguageServerClientConstants.BicepLanguageServerInstallationSubPath, "Bicep.LangServer.exe");
			 */


			//info.FileName = @"C:\src\daveoftheparty\speedy-moq\server\OmniLsp\bin\Debug\net6.0\OmniLsp.exe";
			
			info.FileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "LanguageServer", "OmniLsp.exe");

			info.Arguments = "true visual-studio";
			info.RedirectStandardInput = true;
			info.RedirectStandardOutput = true;
			info.UseShellExecute = false;
			info.CreateNoWindow = true;

			_languageServer = new Process();
			_languageServer.StartInfo = info;

			if (_languageServer.Start())
			{
				return new Connection(_languageServer.StandardOutput.BaseStream, _languageServer.StandardInput.BaseStream);
			}

			
			return null;
		}

		public async Task OnLoadedAsync()
		{
			await StartAsync.InvokeAsync(this, EventArgs.Empty);
		}

		public async Task StopServerAsync()
		{
			if (StopAsync != null)
			{
				await StopAsync.InvokeAsync(this, EventArgs.Empty);
			}
		}

		#region shutting down the language server

		private bool _isDisposed;
		~LanguageClient() => Dispose(false);
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_isDisposed)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects)
					if (_languageServer != null)
					{
						_languageServer.Kill();
						/*
						try
						{
							_languageServer.Close();
						}
						finally
						{
							if (!_languageServer.HasExited)
								_languageServer.Kill();
						}
						*/
					}
				}

				// TODO: free unmanaged resources (unmanaged objects) and override finalizer
				// TODO: set large fields to null
				_isDisposed = true;
			}
		}

		#endregion shutting down the language server

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
				var c = "hold it right there";
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
