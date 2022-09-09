using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Shapes;

namespace Raveta
{
    /// <summary>
    /// Interaction logic for Powershell.xaml
    /// </summary>
    public partial class Powershell : Window
    {
        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref AeroGlass.AeroGlass.WindowCompositionAttributeData data);

        public Powershell()
        {
            InitializeComponent();
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
			EnableBlur();

		}
    }
}
