using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenRA.WindowsLauncher
{
	partial class ModConfig
	{
		partial void Init();

		public ModConfig()
		{
			Init();
		}

		public string ModID = "MyValue";
		public string DisplayName = "DISPLAY_NAME";
		public string FaqUrl = "FAQ_URL";
	}

	class WindowsLauncher
	{
		static Process gameProcess;
		static ModConfig config = new ModConfig();

		[STAThread]
		static int Main(string[] args)
		{
			Console.WriteLine("heeeoe");

			Console.WriteLine(config.ModID);
			return 0;

			if (args.Any(x => x.StartsWith("Engine.LaunchPath=", StringComparison.Ordinal)))
				return RunGame(args);

			return RunInnerLauncher(args);
		}

		static int RunGame(string[] args)
		{
			var launcherPath = Assembly.GetExecutingAssembly().Location;
			var directory = Path.GetDirectoryName(launcherPath);
			Directory.SetCurrentDirectory(directory);

			AppDomain.CurrentDomain.UnhandledException += (_, e) => ExceptionHandler.HandleFatalError((Exception)e.ExceptionObject);

			try
			{
				return (int)Game.InitializeAndRun(args);
			}
			catch (Exception e)
			{
				ExceptionHandler.HandleFatalError(e);
				return (int)RunStatus.Error;
			}
		}

		static int RunInnerLauncher(string[] args)
		{
			var launcherPath = Assembly.GetExecutingAssembly().Location;
			var launcherArgs = args.ToList();

			if (!launcherArgs.Any(x => x.StartsWith("Engine.LaunchPath=", StringComparison.Ordinal)))
				launcherArgs.Add("Engine.LaunchPath=\"" + launcherPath + "\"");

			if (!launcherArgs.Any(x => x.StartsWith("Game.Mod=", StringComparison.Ordinal)))
				launcherArgs.Add("Game.Mod=" + config.ModID);

			var psi = new ProcessStartInfo(launcherPath, string.Join(" ", launcherArgs));

			try
			{
				gameProcess = Process.Start(psi);
			}
			catch
			{
				return 1;
			}

			if (gameProcess == null)
				return 1;

			gameProcess.EnableRaisingEvents = true;
			gameProcess.Exited += GameProcessExited;

			Application.Run();

			return 0;
		}

		static void ShowErrorDialog()
		{
			var headerLabel = new Label
			{
				Location = new Point(0, 10),
				Height = 15,
				Text = config.DisplayName + " has encountered a fatal error and must close.",
				TextAlign = ContentAlignment.TopCenter
			};

			var docsLabel = new Label
			{
				Location = new Point(0, 25),
				Height = 15,
				Text = "Refer to the crash logs and FAQ for more information.",
				TextAlign = ContentAlignment.TopCenter
			};

			int formWidth;
			using (var g = headerLabel.CreateGraphics())
			{
				var headerWidth = (int)g.MeasureString(headerLabel.Text, headerLabel.Font).Width + 60;
				var docsWidth = (int)g.MeasureString(docsLabel.Text, docsLabel.Font).Width + 60;
				formWidth = Math.Max(headerWidth, docsWidth);
				headerLabel.Width = formWidth;
				docsLabel.Width = formWidth;
			}

			var form = new Form
			{
				Size = new Size(formWidth, 110),
				Text = "Fatal Error",
				MinimizeBox = false,
				MaximizeBox = false,
				FormBorderStyle = FormBorderStyle.FixedDialog,
				StartPosition = FormStartPosition.CenterScreen,
				TopLevel = true,
				Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location)
			};

			var viewLogs = new Button
			{
				Location = new Point(10, 50),
				Size = new Size(75, 23),
				Text = "View Logs"
			};

			var viewFaq = new Button
			{
				Location = new Point(90, 50),
				Size = new Size(75, 23),
				Text = "View FAQ"
			};

			var quit = new Button
			{
				Location = new Point(formWidth - 90, 50),
				Size = new Size(75, 23),
				Text = "Quit",
				DialogResult = DialogResult.Cancel
			};

			form.Controls.Add(headerLabel);
			form.Controls.Add(docsLabel);
			form.Controls.Add(viewLogs);
			form.Controls.Add(viewFaq);
			form.Controls.Add(quit);

			viewLogs.Click += ViewLogsClicked;
			viewFaq.Click += ViewFaqClicked;
			form.FormClosed += FormClosed;

			SystemSounds.Exclamation.Play();
			form.ShowDialog();
		}

		static void GameProcessExited(object sender, EventArgs e)
		{
			if (gameProcess.ExitCode != (int)RunStatus.Success)
				ShowErrorDialog();

			Exit();
		}

		static void ViewLogsClicked(object sender, EventArgs e)
		{
			try
			{
				Process.Start(Platform.ResolvePath("^", "Logs"));
			}
			catch { }
		}

		static void ViewFaqClicked(object sender, EventArgs e)
		{
			try
			{
				Process.Start(config.FaqUrl);
			}
			catch { }
		}

		static void FormClosed(object sender, EventArgs e)
		{
			Exit();
		}

		static void Exit()
		{
			Environment.Exit(0);
		}
	}
}
