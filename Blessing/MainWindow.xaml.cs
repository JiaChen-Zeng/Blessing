using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Blessing
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window
	{
		private static string CONFIG_PATH = "../../osu!." + Environment.UserName + ".cfg";
		private const string SKIN_PATH = "skin.ini";

		public MainWindow()
		{
			InitializeComponent();
			BlessingWorldEdition();
		}

		private void BlessingWorldEdition()
		{
			Config config = new Config(CONFIG_PATH);
			try
			{
				config.Load();
			}
			catch (Exception)
			{
				Uri cfgUri = new Uri(new Uri(Environment.CurrentDirectory), CONFIG_PATH);
				MessageBox.Show("在 " + Uri.UnescapeDataString(cfgUri.AbsoluteUri) + " 找不到配置文件。请把我放在皮肤根目录里，皮肤要放在 osu!/Skins 里。", "我想回家");
				Application.Current.Shutdown(1);
				return;
			}
			config.Parse();
			Debug.WriteLine("Config OK");

			Skin skin = new Skin(SKIN_PATH);
			try
			{
				skin.Load();
			}
			catch (Exception)
			{
				MessageBox.Show("这个文件夹里没找到" + SKIN_PATH + "呀。你确定放到皮肤的根目录里了吗？", "智商美丽");
				Application.Current.Shutdown(2);
				return;
			}
			skin.Parse();
			Debug.WriteLine("Parse OK");

			skin.CenterAll();
			try
			{
				skin.Save();
			}
			catch (Exception)
			{
				MessageBox.Show("写入文件发生了错误，目测 skin.ini 被占用了，不然的话就是管理员权限的问题？", "明明还差最后一步了的说");
				Application.Current.Shutdown(3);
				return;
			}

			MessageBox.Show("成功了哟", "主人我要吃糖糖");
			Application.Current.Shutdown();
		}
	}
}
