using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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

namespace FollowTheLeader
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] String propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public List<List<List<PlayerData>>> AllGroupsList = new List<List<List<PlayerData>>>();
		public ObservableCollection<PlayerData> GroupsDisplayList = new ObservableCollection<PlayerData>();
		public ObservableCollection<PlayerData> NowPlayingList = new ObservableCollection<PlayerData>();

		public MainWindow()
		{
			InitializeComponent();

			SetupPlayersText.Text = "Ryan\r\nJames\r\nJake\r\nRandy\r\nCindy\r\nTony\r\nEmma\r\nPaul\r\nBob\r\nChar\r\nBeast\r\nMary\r\nMike";

			GroupsDisplayList.Add(new PlayerData("test"));
		}

		private void SetPlayers_Click(object sender, RoutedEventArgs e)
		{
			SetupGroups();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			AllGroupsItemControl.ItemsSource = GroupsDisplayList;
			NowPlayingItemControl.ItemsSource = NowPlayingList;

			SetupGroups();
		}

		private void SetupGroups()
		{
			StringReader text = new StringReader(SetupPlayersText.Text);

			List<string> players = new List<string>();

			string line = "";
			while ((line = text.ReadLine()) != null)
			{
				line = line.Trim();

				if (line.Length > 0)
				{
					players.Add(line);
				}
			}
			
			int numGroups = (players.Count + 2) / 5;

			AllGroupsList.Clear();
			AllGroupsList.Add(new List<List<PlayerData>>());

			for (int i = 0; i < numGroups; ++i)
			{
				AllGroupsList[0].Add(new List<PlayerData>());
			}

			for (int i = 0, groupIndex = 0; i < players.Count; ++i, ++groupIndex)
			{
				groupIndex = groupIndex % numGroups;

				AllGroupsList[0][groupIndex].Add(new PlayerData(players[i]));
			}

			GroupsDisplayList.Clear();

			int displayGroupIndex = 1;
			foreach (List<PlayerData> group in AllGroupsList[0])
			{
				GroupsDisplayList.Add(new PlayerData("Group " + displayGroupIndex));

				foreach (PlayerData pd in group)
				{
					GroupsDisplayList.Add(pd);
				}

				++displayGroupIndex;
			}

			// Testing
			StartGroup(0, 0);
		}

		private void StartGroup_Click(object sender, RoutedEventArgs e)
		{
			PlayerData pd = (sender as Button).Tag as PlayerData;

			StartGroup(pd);
		}

		private void StartGroup(PlayerData pd)
		{
			string groupNumString = pd.PlayerName.Replace("Group ", "");
			int groupIndex = 0;
			if (int.TryParse(groupNumString, out groupIndex))
			{
				StartGroup(groupIndex - 1, pd.Round);
			}
		}

		private void StartGroup(int groupIndex, int roundIndex)
		{
			NowPlayingList.Clear();

			var playerList = AllGroupsList[roundIndex][groupIndex];

			foreach (PlayerData pd in playerList)
			{
				NowPlayingList.Add(pd);
			}

			NowPlayingList[0].IsLeader = true;
		}
	}

	public class PlayerData : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] String propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		string playerName = "";
		public string PlayerName
		{
			get { return playerName; }
			set
			{
				playerName = value;
				OnPropertyChanged("PlayerName");
				OnPropertyChanged("DisplayText");
			}
		}
		public string DisplayText
		{
			get { return PlayerName; }
		}
		public bool IsGroup
		{
			get { return PlayerName.StartsWith("Group"); }
		}
		public Brush BgColor
		{
			get
			{
				if (IsGroup)
				{
					return Brushes.LightGray;
				}
				else if (PlayerName.StartsWith("Round"))
				{
					return Brushes.LightBlue;
				}


				return Brushes.Transparent;
			}
		}
		bool isLeader = false;
		public bool IsLeader
		{
			get { return isLeader; }
			set
			{
				isLeader = value;
				OnPropertyChanged("IsLeader");
			}
		}
		bool isPerforming = false;
		public bool IsPerforming
		{
			get { return isPerforming; }
			set
			{
				isPerforming = value;
				OnPropertyChanged("IsPerforming");
			}
		}
		bool isCut = false;
		public bool IsCut
		{
			get { return isCut; }
			set
			{
				isCut = value;
				OnPropertyChanged("IsCut");
			}
		}
		public string NowPlayingDisplayText
		{
			get
			{
				return (IsLeader ? "Leader -> " : "") + (IsPerforming ? "Go -> " : "") + PlayerName;
			}
		}
		int totalPoints = 0;
		public int TotalPoints
		{
			get { return totalPoints; }
			set
			{
				totalPoints = value;
				OnPropertyChanged("TotalPoints");
				OnPropertyChanged("TotalPointsDisplay");
			}
		}
		public string TotalPointsDisplay
		{
			get { return IsGroup ? "" : TotalPoints.ToString(); }
		}
		int roundPoints = 0;
		public int RoundPoints
		{
			get { return roundPoints; }
			set
			{
				roundPoints = value;
				OnPropertyChanged("RoundPoints");
			}
		}
		public string RoundPointsDisplay
		{
			get { return RoundPoints.ToString() + " pts"; }
		}
		public Visibility SetGroupButtonVisibility
		{
			get { return IsGroup ? Visibility.Visible : Visibility.Hidden; }
		}
		int round = 0;
		public int Round
		{
			get { return round; }
			set
			{
				Round = value;
				OnPropertyChanged("Round");
			}
		}
		ObservableCollection<HistoryButton> historyButtons = new ObservableCollection<HistoryButton>();
		public ObservableCollection<HistoryButton> HistoryButtons { get { return historyButtons; } }


		public PlayerData()
		{
		}

		public PlayerData(string inName)
		{
			playerName = inName;

			HistoryButtons.Add(new HistoryButton());
			HistoryButtons.Add(new HistoryButton());
		}

		public PlayerData(string inName, int inRound)
		{
			playerName = inName;
			Round = inRound;
		}
	}

	public class HistoryButton : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] String propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public string PointsDisplay
		{
			get { return "5"; }
		}

		public HistoryButton()
		{

		}

	}
}
