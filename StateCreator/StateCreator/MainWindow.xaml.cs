using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace StateCreator
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		ObservableCollection<User_Controls.EncapsulationControl> FolderEntries { get; set; } = new ObservableCollection<User_Controls.EncapsulationControl>();


		public MainWindow()
		{
			InitializeComponent();

			StateEntryList.ItemsSource = FolderEntries;

			// create first section
			BtnAddFolder_Click(null, null);
		}


		private void SetNamespace()
		{
			for (int i = TxtSourcePath.Text.Length - 1; i > 0; i--)
			{
				if (TxtSourcePath.Text[i].Equals('\\'))
				{
					TxtNamespace.Text = TxtSourcePath.Text.Substring(i + 1, TxtSourcePath.Text.Length - ( i + 1 ));
					break;
				}
			}
		}

		private void BtnAddFolder_Click(object sender, RoutedEventArgs e)
		{
			var newControl = new User_Controls.EncapsulationControl();
			newControl.BtnEncapsulationControlExit.Click += RemoveControl_Click;
			FolderEntries.Add(newControl);
		}

		private void RemoveControl_Click(object sender, RoutedEventArgs e)
		{
			User_Controls.EncapsulationControl rmEntry = null;

			foreach (var entry in FolderEntries)
			{
				if (entry.BtnEncapsulationControlExit == ( (Button)sender ))
				{
					rmEntry = entry;
					break;
				}
			}

			if (rmEntry != null)
			{
				FolderEntries.Remove(rmEntry);
			}
			else
			{
				throw new InvalidOperationException();
			}
		}

		private void BtnGetSource_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog dialog = new SaveFileDialog();
			// Create a "Save As" dialog for selecting a directory (HACK)
			dialog.Title = "Select a Directory"; // instead of default "Save As"
			dialog.Filter = "Directory|*.this.directory"; // Prevents displaying files
			dialog.FileName = "select"; // Filename will then be "select.this.directory"

			if (dialog.ShowDialog() == true)
			{
				string path = dialog.FileName;
				// Remove fake filename from resulting path
				path = path.Replace("\\select.this.directory", "");
				path = path.Replace(".this.directory", "");
				// If user has changed the filename, create the new directory
				if (!System.IO.Directory.Exists(path))
				{
					System.IO.Directory.CreateDirectory(path);
				}
				// Our final value is in path
				TxtSourcePath.Text = path;
			}

			SetNamespace();
		}

		private void BtnCreateMachine_Click(object sender, RoutedEventArgs e)
		{
			List<string> allClasses = new List<string>();
			List<string> allFolders = new List<string>();

			// create interface and abstract base class
			if (TxtSourcePath.Text.Substring(0, 3).Equals("ex:"))
			{
				MessageBox.Show("Please navigate to your project folder!", "Don't suck!", MessageBoxButton.OK);
				return;
			}

			CreateBaseFiles();

			foreach (var entry in FolderEntries)
			{
				if (!string.IsNullOrEmpty(entry.TxtStates.Text.Trim()))
				{
					string srcDotModelsPath = $"{TxtSourcePath.Text}\\Models\\";
					string[] classNames = entry.TxtStates.Text.Split(',');

					// create new folder for the class?
					if ((bool)entry.ChkCreateFldr.IsChecked)
					{						
						// create the new dir
						string dirPath = $"{srcDotModelsPath}{entry.TxtFldrName.Text}";
						Directory.CreateDirectory(dirPath);

						foreach (string str in classNames)
						{
							string name = str.Trim();
							allClasses.Add(name);

							// modify namespace for encapsulation to that folder?
							if ((bool)entry.ChkEncapsulate.IsChecked)
							{
								// send folder name and encapsulate namespace
								if (!allFolders.Contains(entry.TxtFldrName.Text))
								{
									allFolders.Add(entry.TxtFldrName.Text);
								}

								File.WriteAllText($"{dirPath}\\{name}.cs", GetStateClassText(name, entry.TxtFldrName.Text, true));
							}
							else
							{
								// send folder name, keep models ns
								File.WriteAllText($"{dirPath}\\{name}.cs", GetStateClassText(name));
							}
						}
					}
					else
					{
						// use models folder
						foreach (string name in classNames)
						{
							File.WriteAllText($"{srcDotModelsPath}\\{name}.cs", GetStateClassText(name));
						}
					}
				}
			}

			CreateStateMachineClass(allClasses, allFolders);
		}

		private void CreateStateMachineClass(List<string> allClasses, List<string> folders)
		{
			StringBuilder sb = new StringBuilder();

			string usingStatements =
@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using " + TxtNamespace.Text + ".Models;";

			sb.Append(usingStatements);

			foreach (string folderName in folders)
			{
				sb.Append($"\nusing {TxtNamespace.Text}.Models.{folderName};");
			}

			string namespaceClassDec =
@"

namespace " + TxtNamespace.Text + @"
{
	public class StateMachineClass
	{";
			sb.Append(namespaceClassDec);

			foreach (string className in allClasses)
			{
				sb.Append($"\n\t\tpublic BaseState {className} {{ get; set; }}");
			}

			string structor =
@"

		public StateMachineClass()
		{";

			sb.Append(structor);

			foreach (string className in allClasses)
			{
				sb.Append($"\n\t\t\t{className} = new {className}(this);");
			}

			sb.Append("\n\t\t}");
			sb.Append("\n\t}");
			sb.Append("\n}");

			File.WriteAllText($"{TxtSourcePath.Text}\\StateMachineClass.cs", sb.ToString());

		}

		private void CreateBaseFiles()
		{
			string interfacesPath = $"{ TxtSourcePath.Text}\\Interfaces";
			string interfaceFile = $"{TxtSourcePath.Text}\\Interfaces\\ITransitionable.cs";

			string modelsPath = $"{TxtSourcePath.Text}\\Models";
			string baseStatePath = $"{TxtSourcePath.Text}\\Models\\BaseState.cs";

			// create interfaces folder at source path
			if (!Directory.Exists(interfacesPath))
			{
				Directory.CreateDirectory(interfacesPath);
			}

			// create models folder at source path
			if (!Directory.Exists(modelsPath))
			{
				Directory.CreateDirectory(modelsPath);
			}

			// create interface			
			File.WriteAllText(interfaceFile, GetInterfaceFileText());

			// create base state file
			File.WriteAllText(baseStatePath, GetBaseStateFileText());
		}

		private string GetInterfaceFileText()
		{
			return
@"namespace " + TxtNamespace.Text + @".Interfaces
{
	public interface ITransitionable
	{
		
	}
}";
		}

		private string GetBaseStateFileText()
		{
			return
@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using " + TxtNamespace.Text + @".Interfaces;

namespace " + TxtNamespace.Text + @".Models
{
	public abstract class BaseState : ITransitionable
	{
		public StateMachineClass StateMachineObject { get; set; } = null;

		" + TxtAbstractSig.Text.Replace("   ", " ") + @";

		public BaseState(StateMachineClass stateMachineObject)
		{
			StateMachineObject = stateMachineObject;
		}		
	}
}";
		}

		private string GetStateClassText(string className, string folderName = ".Models", bool encapsulateNS = false)
		{
			return
@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using " + TxtNamespace.Text + @".Models;

namespace " + TxtNamespace.Text + ( encapsulateNS ? ( $".Models.{folderName}" ) : folderName ) + @"
{
	public class " + className + @" : BaseState
	{
		public " + className + @"(StateMachineClass stateMachineObject) : base(stateMachineObject) { }

		" + TxtAbstractSig.Text.Replace("   ", " ").Replace("abstract", "override") + @"
		{
			
		}
	}
}";
		}
	}

}
