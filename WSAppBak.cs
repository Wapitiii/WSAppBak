using System;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace WSAppBak
{
	internal class WSAppBak
	{
		private string AppName = "Windows Store App Backup";

		private string AppCreator = "Kiran Murmu";

		private string AppCurrentDirctory = Directory.GetCurrentDirectory();

		private string WSAppXmlFile = "AppxManifest.xml";

		private bool Checking = true;

		private string WSAppName;

		private string WSAppPath;

		private string WSAppVersion;

		private string WSAppFileName;

		private string WSAppOutputPath;

		private string WSAppProcessorArchitecture;

		private string WSAppPublisher;

		public void Run()
		{
			ReadArg();
		}

		private string RunProcess(string fileName, string args)
		{
			string result = "";
			Process process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = fileName,
					Arguments = args,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					CreateNoWindow = true
				}
			};
			process.Start();
			while (!process.StandardOutput.EndOfStream)
			{
				string text = process.StandardOutput.ReadLine();
				Console.WriteLine(text);
				if (text.Length > 0)
				{
					result = text;
				}
			}
			return result;
		}

		private void ReadArg()
		{
			while (Checking)
			{
				Console.Clear();
				Console.WriteLine("\t\t'{0}' by {1}", AppName, AppCreator);
				Console.WriteLine("================================================================================");
				Console.Write("Enter the App path: ");
				WSAppPath = Convert.ToString(Console.ReadLine());
				if (WSAppPath.Contains("\""))
				{
					WSAppPath = WSAppPath.Replace("\"", "");
					WSAppPath = "\"" + WSAppPath + "\"";
				}
				else if (File.Exists(WSAppPath + "\\" + WSAppXmlFile))
				{
					while (Checking)
					{
						Console.Write("\nEnter the Output path: ");
						WSAppOutputPath = Convert.ToString(Console.ReadLine());
						if (WSAppOutputPath.Contains("\""))
						{
							WSAppOutputPath = WSAppOutputPath.Replace("\"", "");
							WSAppOutputPath = "\"" + WSAppOutputPath + "\"";
						}
						else if (Directory.Exists(WSAppOutputPath))
						{
							WSAppFileName = Path.GetFileName(WSAppPath);
							using (XmlReader xmlReader = XmlReader.Create(WSAppPath + "\\" + WSAppXmlFile))
							{
								while (xmlReader.Read())
								{
									if (xmlReader.IsStartElement() && xmlReader.Name == "Identity")
									{
										string text = xmlReader["Name"];
										if (text != null)
										{
											WSAppName = text;
										}
										string text2 = xmlReader["Publisher"];
										if (text2 != null)
										{
											WSAppPublisher = text2;
										}
										string text3 = xmlReader["Version"];
										if (text3 != null)
										{
											WSAppVersion = text3;
										}
										string text4 = xmlReader["ProcessorArchitecture"];
										if (text4 != null)
										{
											WSAppProcessorArchitecture = text4;
										}
									}
								}
							}
							while (Checking)
							{
								MakeAppx();
							}
						}
						else
						{
							Checking = true;
							Console.WriteLine("\nInvailed Output Path, '{0}' Directory not found!", WSAppOutputPath);
							Console.Write("Press any Key to retry...");
							Console.ReadKey();
							Console.Clear();
							Console.WriteLine("\t\t'{0}' by {1}", AppName, AppCreator);
							Console.Write("================================================================================");
						}
					}
				}
				else
				{
					Checking = true;
					Console.WriteLine("\nInvailed App Path, '{0}' file not found!", WSAppXmlFile);
					Console.Write("Press any Key to retry...");
					Console.ReadKey();
				}
			}
		}

		private void MakeAppx()
		{
			string text = AppCurrentDirctory + "\\WSAppBak\\MakeAppx.exe";
			string args = "pack -d \"" + WSAppPath + "\" -p \"" + WSAppOutputPath + "\\" + WSAppFileName + ".appx\" -l";
			if (File.Exists(text))
			{
				if (File.Exists(WSAppOutputPath + "\\" + WSAppFileName + ".appx"))
				{
					File.Delete(WSAppOutputPath + "\\" + WSAppFileName + ".appx");
				}
				Console.WriteLine("\nPlease wait.. Creating '.appx' package file.\n");
				if (RunProcess(text, args).ToLower().Contains("succeeded"))
				{
					Console.Clear();
					Console.WriteLine("\t\t'{0}' by {1}", AppName, AppCreator);
					Console.WriteLine("================================================================================");
					Console.WriteLine("Package '{0}' creation succeeded.", WSAppFileName + ".appx");
					while (Checking)
					{
						MakeCert();
					}
				}
				else
				{
					Checking = false;
					Console.Clear();
					Console.WriteLine("\t\t'{0}' by {1}", AppName, AppCreator);
					Console.WriteLine("================================================================================");
					Console.Write("Package '{0}' creation failed... press any Key to exit.", WSAppFileName + ".appx");
					Console.ReadKey();
				}
			}
			else
			{
				Checking = false;
				Console.WriteLine("\nCan't create '.appx' file, 'MakeAppx.exe' file not found!");
				Console.Write("Press any Key to exit...");
				Console.ReadKey();
			}
		}

		private void MakeCert()
		{
			string text = AppCurrentDirctory + "\\WSAppBak\\MakeCert.exe";
			string args = "-n \"" + WSAppPublisher + "\" -r -a sha256 -len 2048 -cy end -h 0 -eku 1.3.6.1.5.5.7.3.3 -b 01/01/2000 -sv \"" + WSAppOutputPath + "\\" + WSAppFileName + ".pvk\" \"" + WSAppOutputPath + "\\" + WSAppFileName + ".cer\"";
			if (File.Exists(text))
			{
				if (File.Exists(WSAppOutputPath + "\\" + WSAppFileName + ".pvk"))
				{
					File.Delete(WSAppOutputPath + "\\" + WSAppFileName + ".pvk");
				}
				if (File.Exists(WSAppOutputPath + "\\" + WSAppFileName + ".cer"))
				{
					File.Delete(WSAppOutputPath + "\\" + WSAppFileName + ".cer");
				}
				Console.WriteLine("\nPlease wait.. Creating certificate for the package.\n");
				Console.Write("Certificate creation: ");
				if (RunProcess(text, args).ToLower().Contains("succeeded"))
				{
					while (Checking)
					{
						Pvk2Pfx();
					}
				}
				else
				{
					Checking = false;
					Console.WriteLine("\nFailed to create Certificate for the package... Prees any Key exit.");
					Console.ReadKey();
				}
			}
			else
			{
				Checking = false;
				Console.WriteLine("\nCan't create Certificate for the package, 'MakeCert.exe' file not found!");
				Console.Write("Press any Key to exit...");
				Console.ReadKey();
			}
		}

		private void Pvk2Pfx()
		{
			string text = AppCurrentDirctory + "\\WSAppBak\\Pvk2Pfx.exe";
			string args = "-pvk \"" + WSAppOutputPath + "\\" + WSAppFileName + ".pvk\" -spc \"" + WSAppOutputPath + "\\" + WSAppFileName + ".cer\" -pfx \"" + WSAppOutputPath + "\\" + WSAppFileName + ".pfx\"";
			if (File.Exists(text))
			{
				if (File.Exists(WSAppOutputPath + "\\" + WSAppFileName + ".pfx"))
				{
					File.Delete(WSAppOutputPath + "\\" + WSAppFileName + ".pfx");
				}
				Console.WriteLine("\nPlease wait.. Converting certificate to sign the package.\n");
				Console.Write("Certificate convertion: ");
				if (RunProcess(text, args).Length == 0)
				{
					Console.Write("succeeded");
					while (Checking)
					{
						SignApp();
					}
				}
				else
				{
					Checking = false;
					Console.WriteLine("\nCan't convert certificate to sign the package... Prees any Key exit...");
					Console.ReadKey();
				}
			}
			else
			{
				Checking = false;
				Console.WriteLine("\nCan't convert Certificate to sign the package, 'Pvk2Pfx.exe' file not found!");
				Console.Write("Press any Key to exit...");
				Console.ReadKey();
			}
		}

		private void SignApp()
		{
			string text = AppCurrentDirctory + "\\WSAppBak\\SignTool.exe";
			string args = "sign -fd SHA256 -a -f \"" + WSAppOutputPath + "\\" + WSAppFileName + ".pfx\" \"" + WSAppOutputPath + "\\" + WSAppFileName + ".appx\"";
			if (File.Exists(text))
			{
				Console.WriteLine("\n\nPlease wait.. Signing the package, this may take some minutes.\n");
				if (RunProcess(text, args).ToLower().Contains("successfully signed"))
				{
					Checking = false;
					Console.WriteLine("Package signing succeeded. Please install the '.cer' file to [Local Computer\\Trusted Root Certification Authorities] before install the App Package or use 'WSAppPkgIns.exe' file to install the App Package!");
					Console.Write("\nPress any Key to exit..... :)");
					Console.ReadKey();
				}
				else
				{
					Checking = false;
					Console.WriteLine("\nCan't Sign the package, Press any Key to exit...");
					Console.ReadKey();
				}
			}
			else
			{
				Checking = false;
				Console.WriteLine("\nCan't Sign the package, 'SignTool.exe' file not found!");
				Console.Write("Press any Key to exit...");
				Console.ReadKey();
			}
		}
	}
}