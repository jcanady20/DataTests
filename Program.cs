using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading;

namespace DataTests
{
	partial class Program
	{
		protected static Type m_type = typeof(Program);
		protected static List<MethodInfo> m_methods = null;

		[STAThread]
		static void Main(string[] args)
		{
			CreateMethodCache();
			AddTraceListeners();

			Console.Clear();
			Console.ForegroundColor = ConsoleColor.White;
			Console.BufferHeight = 300;
			Console.BufferWidth = 100;
			Console.Title = "Test Bench";

			if (args.Length > 0)
			{
				RunCommand(args);
				return;
			}

			bool pContinue = true;
			while (pContinue)
			{
				Console.Write(":>");

				string[] Coms = Console.ReadLine().Split(new char[] { ' ' });
				if (Coms.Length == 0)
					continue;

				switch (Coms[0].ToLower())
				{
					case "exit":
					case "quit":
						pContinue = false;
						break;
					default:
						RunCommand(Coms);
						break;
				}
				Console.WriteLine("");
			}
		}

		static void RunCommand(string[] args)
		{
			string CallingMethod = args[0];
			object[] param = new object[] { };
			if (args.Length > 1)
			{
				//Remove the First Param which should be the calling Method
				param = new object[args.Length - 1];
				for (int i = 0; i < param.Length; i++)
				{
					int j = i;
					param[i] = args[++j];
				}
			}

			foreach (MethodInfo mi in m_methods)
			{
				if (
					//	Validate the File Name
						String.Compare(mi.Name.ToLower(), CallingMethod.ToLower(), true) != 0
					//	Validate the Parameter Count
						|| mi.GetParameters().Length != param.Length
					)
				{
					continue;
				}

				try
				{
					object prog = System.Activator.CreateInstance(m_type);
					m_type.InvokeMember(mi.Name, BindingFlags.Default | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, prog, param);
					return;
				}
				catch (Exception e)
				{
					if (e.InnerException != null)
					{
						Console.WriteLine();
						WriteExceptions(e.InnerException);
					}
					return;
				}
			}
			// If we fell out of the loop and didn't find a Matching Method, then Call Help.
			Console.WriteLine("Unknown Command");
			Program.Help();
		}

		[Description("Shows Help for All Commands")]
		static void Help()
		{
			Console.WriteLine("Valid Commands");
			foreach (MethodInfo m in m_methods)
			{
				DescriptionAttribute[] attribs = (DescriptionAttribute[])m.GetCustomAttributes(typeof(DescriptionAttribute), false);
				if (attribs != null && attribs.Length > 0)
				{
					Console.ForegroundColor = ConsoleColor.Green;
					Console.Write(m.Name);
					ParameterInfo[] parm = m.GetParameters();
					Console.ForegroundColor = ConsoleColor.Cyan;
					Console.Write("(");
					for (int i = 0; i < parm.Length; i++)
					{
						if (i > 0)
							Console.Write(", ");

						Console.Write("({0}){1}", parm[i].ParameterType.Name, parm[i].Name);
					}
					Console.Write(")");
					Console.ForegroundColor = ConsoleColor.White;
					Console.WriteLine("\n\t{0}", attribs[0].Description);
				}
			}
		}

		[Description("Clears the Current display Buffer")]
		static void Clear()
		{
			Console.Clear();
		}

		[Description("Quits out of the application")]
		static void Quit()
		{
			return;
		}

		[Description("List Local Drives")]
		static void LocalDrives()
		{
			string[] drives = Environment.GetLogicalDrives();
			IEnumerable<string> strs = drives.Select(s => s.Replace(":\\", ""));
			foreach (String s in strs)
			{
				System.IO.DriveInfo drvi = new System.IO.DriveInfo(s);
				if (drvi.DriveType == DriveType.CDRom)
					continue;
				Console.WriteLine("{0}:\\", s);
			}
		}

		[Description("List Available Providers")]
		static void LocalProviders()
		{
			var dt = System.Data.Common.DbProviderFactories.GetFactoryClasses();
			//	Name Description InvariantName
			Console.WriteLine("{0} {1} {2}", "", "Name", "InvariantName");
			Console.WriteLine("-------------------------------------");
			foreach (System.Data.DataRow dr in dt.Rows)
			{
				Console.WriteLine("{0} {1} {2}", "", dr["Name"], dr["InvariantName"]);
			}
		}

		[Description("Open Application Log Folder")]
		static void OpenLogFolder()
		{
			Process.Start(Path.Combine(GetCurrentPath(), "ApplicationLogs"));
		}

		[Description("Menu Sample")]
		static void SampleMenu()
		{
			List<MenuChoice> _choices = new List<MenuChoice>();

			_choices.Add(new MenuChoice(new System.Action(Program.LocalDrives), "List Local Drives"));
			_choices.Add(new MenuChoice(new System.Action(Program.OpenLogFolder), "Opens the Current Log Folder"));
			_choices.Add(new MenuChoice(new System.Action(Program.Help), "Help"));
			_choices.Add(new MenuChoice(new System.Action(Program.LocalProviders), "List Available Providers"));

			Menu m = new Menu(_choices);

			m.RunMenu();

			if (m.Canceled)
				return;

			foreach (MenuChoice mc in _choices)
			{
				if (mc.Selected == false)
					continue;
				mc.InvokeAction();
			}
		}

		static void ls()
		{
			var path = @"C:\Windows";
			var di = new DirectoryInfo(path);
			foreach (var file in di.EnumerateFiles())
			{
				Console.WriteLine(file.Name);
			}
		}

		static TimeSpan CalculateEta(DateTime startTime, int totalItems, int completeItems)
		{
			TimeSpan _eta = TimeSpan.MinValue;
			//	Avoid Divide by Zero Errors
			if (completeItems > 0)
			{
				int _itemduration = (int)DateTime.Now.Subtract(startTime).TotalMilliseconds / completeItems;
				_eta = TimeSpan.FromMilliseconds((double)((totalItems - completeItems) * _itemduration));
			}
			return _eta;
		}

		static void WriteExceptions(Exception e)
		{
			Console.BackgroundColor = ConsoleColor.Black;
			Console.ForegroundColor = ConsoleColor.Red;

			Trace.Write("Source:");
			Trace.Write(e.Source);
			Trace.WriteLine("\nMessage:");
			Trace.Write(e.Message);
			Trace.WriteLine("\nStack Trace:");
			Trace.Write(e.StackTrace);
			Trace.WriteLine("\nUser Defined Data:");
			foreach (System.Collections.DictionaryEntry de in e.Data)
			{
				Trace.WriteLine(string.Format("[{0}] :: {1}", de.Key, de.Value));
			}
			if (e.InnerException != null)
			{
				WriteExceptions(e.InnerException);
			}
			Console.ForegroundColor = ConsoleColor.White;
		}

		static string GetCurrentPath()
		{
			var asm = Assembly.GetExecutingAssembly();
			var fi = new FileInfo(asm.Location);
			return fi.DirectoryName;
		}

		static void HexDump(byte[] bytes)
		{
			for (int line = 0; line < bytes.Length; line += 16)
			{
				byte[] lineBytes = bytes.Skip(line).Take(16).ToArray();
				System.Text.StringBuilder sb = new System.Text.StringBuilder();
				sb.AppendFormat("{0:x8} ", line);
				sb.Append(string.Join(" ", lineBytes.Select(b => b.ToString("x2")).ToArray()).PadRight(16 * 3));
				sb.Append(" ");
				sb.Append(new string(lineBytes.Select(b => b < 32 ? '.' : (char)b).ToArray()));
				Console.WriteLine(sb);
			}
		}

		static void CreateMethodCache()
		{
			m_methods = m_type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).ToList();
		}

		static void AddTraceListeners()
		{
			var lt = new LogFileTraceListener();
			TextWriterTraceListener CWriter = new TextWriterTraceListener(Console.Out);
			Trace.Listeners.Add(CWriter);
			Trace.Listeners.Add(lt);
		}

		internal class LogFileTraceListener : TraceListener
		{
			private readonly static int m_logFileSize = 1048576;
			private readonly static int m_logstoKeep = 10;

			public LogFileTraceListener()
			{
				this.Name = "LogFileTracing";
				this.LogName = GetApplicationName();
				this.LogRootPath = Path.Combine(GetBaseDirectory(), "ApplicationLogs");

				this.CheckDirectory();
				this.RenameLogFiles();
				this.CheckLogFile();
			}

			public string LogName { get; set; }
			public string LogRootPath { get; set; }
			private string LogFile
			{
				get
				{
					return Path.Combine(this.LogRootPath, GetLogFileName(0));
				}
			}

			public override void Write(string message)
			{
				this.CheckLogFile();
				lock (this)
				{
					try
					{
						using (var sw = new StreamWriter(this.LogFile, true))
						{
							sw.Write(string.Format("{0} :: {1}", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), message.Trim()));
							sw.Flush();
						}
					}
					catch (Exception e)
					{
						Trace.Listeners.Remove(this.Name);
						Trace.WriteLine("EXCEPTION IN Write:");
						Trace.WriteLine(e.Message);
					}
				}
			}
			public override void WriteLine(string message)
			{
				this.CheckLogFile();
				lock (this)
				{
					try
					{
						using (var sw = new StreamWriter(this.LogFile, true))
						{
							sw.WriteLine(string.Format("{0} :: {1}", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), message.Trim()));
							sw.Flush();
						}
					}
					catch (Exception e)
					{
						Trace.Listeners.Remove(this.Name);
						Trace.WriteLine("EXCEPTION IN WriteLine:");
						Trace.WriteLine(e.Message);
					}
				}
			}

			public void CheckLogFile()
			{
				if (!File.Exists(this.LogFile))
				{
					CreateLogFile();
				}
				else
				{
					FileInfo fi = new FileInfo(this.LogFile);
					if (fi.Length > m_logFileSize)
					{
						RenameLogFiles();
						CreateLogFile();
					}
				}
			}
			private void CreateLogFile()
			{
				if (File.Exists(this.LogFile))
				{
					return;
				}

				try
				{
					CheckDirectory();
					using (FileStream fs = new FileStream(this.LogFile, FileMode.OpenOrCreate, FileAccess.ReadWrite))
					{
						fs.Flush();
						fs.Close();
					}
				}
				catch (Exception e)
				{
					Trace.Listeners.Remove(this.Name);
					Trace.WriteLine("EXCEPTION IN CreateLogFile:");
					Trace.WriteLine(e.Message);
				}
			}
			private void RenameLogFiles()
			{
				//	Make room for our files.
				string maxFile = Path.Combine(LogRootPath, GetLogFileName(m_logstoKeep));
				if (File.Exists(maxFile))
				{
					File.Delete(maxFile);
				}

				DirectoryInfo di = new DirectoryInfo(LogRootPath);
				var fic = di.GetFiles(LogName + "*.log");
				int i = m_logstoKeep;
				if (fic.Length > 0)
				{
					while (i != 0)
					{
						int j = i + 1;
						string v_currfile = Path.Combine(LogRootPath, GetLogFileName(i));
						string v_destfile = Path.Combine(LogRootPath, GetLogFileName(j));
						if (File.Exists(v_currfile))
						{
							if (File.Exists(v_destfile))
							{
								File.Delete(v_destfile);
							}
							File.Move(v_currfile, v_destfile);
						}
						i--;
					}
					string logFile = Path.Combine(LogRootPath, GetLogFileName(0));
					if (File.Exists(logFile))
						File.Move(logFile, Path.Combine(LogRootPath, GetLogFileName(1)));
				}
			}
			private void CheckDirectory()
			{
				try
				{
					if (Directory.Exists(LogRootPath) == false)
					{
						Directory.CreateDirectory(LogRootPath);
					}
				}
				catch (Exception e)
				{
					Trace.Listeners.Remove(this.Name);
					Trace.WriteLine("EXCEPTION IN CheckDirectory:");
					Trace.WriteLine(e.Message);
				}
			}
			private string GetBaseDirectory()
			{
				string basedir = string.Empty;
				try
				{
					var assm = Assembly.GetEntryAssembly();
					var fi = new FileInfo(assm.Location);
					basedir = fi.DirectoryName;
				}
				catch (Exception e)
				{
					Trace.Listeners.Remove(this.Name);
					Trace.WriteLine("EXCEPTION IN GetExecutingDirectory:");
					Trace.WriteLine(e.Message);
				}
				return basedir;
			}
			private string GetApplicationName()
			{
				try
				{
					Assembly asm = Assembly.GetEntryAssembly();
					if (asm == null)
						return "ApplicationTraceFile";

					return Assembly.GetEntryAssembly().GetName().Name;
				}
				catch (Exception e)
				{
					Trace.Listeners.Remove(this.Name);
					Trace.WriteLine("EXCEPTION IN GetApplicationName:");
					Trace.WriteLine(e.Message);
				}
				return String.Empty;
			}
			private string GetLogFileName(int n)
			{
				return String.Format("{0}{1}.log", this.LogName, (n == 0) ? "" : n.ToString());
			}
		}
	}
}