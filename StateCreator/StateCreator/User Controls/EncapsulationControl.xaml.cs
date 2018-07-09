using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StateCreator.User_Controls
{
	/// <summary>
	/// Interaction logic for EncapsulationControl.xaml
	/// </summary>
	public partial class EncapsulationControl : UserControl
	{
		public Button BtnEncapsulationControlExit { get; set; }

		public EncapsulationControl()
		{
			InitializeComponent();
			BtnEncapsulationControlExit = BtnExit;
		}

		private void ChkCreateFldr_Checked(object sender, RoutedEventArgs e)
		{
			var chk = (CheckBox)sender;

			switch (chk.IsChecked)
			{
				case true:
					TxtFldrName.Visibility = Visibility.Visible;
					ChkEncapsulate.Visibility = Visibility.Visible;
					break;
				case false:
					TxtFldrName.Visibility = Visibility.Hidden;
					ChkEncapsulate.Visibility = Visibility.Hidden;
					break;
			}
		}

		private void TxtBox_GotFocus(object sender, RoutedEventArgs e)
		{
			var txt = (TextBox)sender;
			if (txt.Text.Equals("Folder Name"))
			{
				txt.Text = "";
			}
		}
	}
}
