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
using System.Xml.Serialization;

namespace FollowTheLeader
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		///////////// Tunnables ///////////////////////////////////////////////////
		const int NobodyEverybodySucessPoints = 2;
		///////////////////////////////////////////////////////////////////////////

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] String propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		SaveDataContainer SaveData = new SaveDataContainer();
		public List<List<List<PlayerData>>> AllGroupsList
		{
			get { return SaveData.AllGroupsList; }
			set
			{
				SaveData.AllGroupsList = value;
				OnPropertyChanged("AllGroupsList");
			}
		}
		public ObservableCollection<PlayerData> GroupsDisplayList = new ObservableCollection<PlayerData>();
		public ObservableCollection<PlayerData> NowPlayingList = new ObservableCollection<PlayerData>();

		public int TeamsMakingCut { get { return 3; } }

		bool showSetLeaderComboButton = false;
		public bool ShowSetLeaderComboButton
		{
			get { return showSetLeaderComboButton; }
			set
			{
				showSetLeaderComboButton = value;
				OnPropertyChanged("ShowSetLeaderComboButton");
				OnPropertyChanged("SetLeaderComboButtonVisibility");
			}
		}
		public Visibility SetLeaderComboButtonVisibility
		{
			get { return ShowSetLeaderComboButton ? Visibility.Visible : Visibility.Hidden; }
		}
		public string SetLeaderComboButtonText
		{
			get { return "Leader Combo Set"; }
		}
		bool showStartGroupButton = false;
		public bool ShowStartGroupButton
		{
			get { return showStartGroupButton; }
			set
			{
				showStartGroupButton = value;
				OnPropertyChanged("ShowStartGroupButton");
				OnPropertyChanged("StartGroupButtonVisibility");
			}
		}
		public Visibility StartGroupButtonVisibility
		{
			get { return showStartGroupButton ? Visibility.Visible : Visibility.Hidden; }
		}

		public Visibility ShowPerformerResultButtonVisibility1
		{
			get { return Performer1 != null ? Visibility.Visible : Visibility.Hidden; }
		}
		PlayerData performer1 = null;
		public PlayerData Performer1
		{
			get { return performer1; }
			set
			{
				performer1 = value;
				OnPropertyChanged("Performer1");
				OnPropertyChanged("PerformerName1");
				OnPropertyChanged("PerformerName1");
				OnPropertyChanged("PerformerResultSuccess1");
				OnPropertyChanged("PerformerResultFail1");
				OnPropertyChanged("ShowPerformerResultButtonVisibility1");
			}
		}
		public string PerformerName1
		{
			get { return Performer1 != null ? Performer1.PlayerName : ""; }
		}
		public string PerformerResultSuccess1
		{
			get { return PerformerName1 + " Hit"; }
		}
		public string PerformerResultFail1
		{
			get { return PerformerName1 + " Miss"; }
		}
		
		public Visibility ShowPerformerResultButtonVisibility2
		{
			get { return Performer2 != null ? Visibility.Visible : Visibility.Hidden; }
		}
		PlayerData performer2 = null;
		public PlayerData Performer2
		{
			get { return performer2; }
			set
			{
				performer2 = value;
				OnPropertyChanged("Performer2");
				OnPropertyChanged("PerformerName2");
				OnPropertyChanged("PerformerName2");
				OnPropertyChanged("PerformerResultSuccess2");
				OnPropertyChanged("PerformerResultFail2");
				OnPropertyChanged("ShowPerformerResultButtonVisibility2");
			}
		}
		public string PerformerName2
		{
			get { return Performer2 != null ? Performer2.PlayerName : ""; }
		}
		public string PerformerResultSuccess2
		{
			get { return PerformerName2 + " Hit"; }
		}
		public string PerformerResultFail2
		{
			get { return PerformerName2 + " Miss"; }
		}

		string instructionsDisplay = "Setup Groups";
		public string InstructionsDisplay
		{
			get { return instructionsDisplay; }
			set
			{
				instructionsDisplay = value;
				OnPropertyChanged("InstructionsDisplay");
			}
		}

		public string CurrentDirectory
		{
			get { return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location); }
		}

		public MainWindow()
		{
			InitializeComponent();

			SetupPlayersText.Text = "Ryan\r\nJames\r\nJake\r\nRandy\r\nCindy\r\nTony\r\nEmma\r\nPaul\r\nBob\r\nChar\r\nBeast\r\nMary\r\nMike";

			GroupsDisplayList.Add(new PlayerData("test"));
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			TopLevelGrid.DataContext = this;
			AllGroupsItemControl.ItemsSource = GroupsDisplayList;
			NowPlayingItemControl.ItemsSource = NowPlayingList;

			SetupGroups();
		}

		private void SetPlayers_Click(object sender, RoutedEventArgs e)
		{
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
			
			AllGroupsList.Clear();

			AddGroups(players);

			// Testing
			SelectGroup(0, 0);
		}

		private void AddGroups(List<string> players)
		{
			int numGroups = (players.Count + 2) / 5;

			int roundIndex = AllGroupsList.Count;
			AllGroupsList.Add(new List<List<PlayerData>>());

			for (int i = 0; i < numGroups; ++i)
			{
				AllGroupsList[roundIndex].Add(new List<PlayerData>());
			}

			for (int i = 0, groupIndex = 0; i < players.Count; ++i, ++groupIndex)
			{
				groupIndex = groupIndex % numGroups;

				AllGroupsList[roundIndex][groupIndex].Add(new PlayerData(players[i]));
			}

			RefreshGroups();
		}

		private void AddGroups()
		{
			List<KeyValuePair<int, string>> results = new List<KeyValuePair<int, string>>();
			foreach (List<PlayerData> group in AllGroupsList.Last())
			{
				foreach (PlayerData pd in group)
				{
					if (!pd.IsCut)
					{
						results.Add(new KeyValuePair<int, string>(pd.RoundPoints, pd.PlayerName));
					}
				}
			}

			results.Sort((KeyValuePair<int, string> a, KeyValuePair<int, string> b) =>
			{
				if (a.Key == b.Key)
				{
					return 0;
				}
				else if (a.Key > b.Key)
				{
					return -1;
				}
				else
				{
					return 1;
				}
			});

			List<string> players = new List<string>();
			foreach (KeyValuePair<int, string> result in results)
			{
				players.Add(result.Value);
			}

			AddGroups(players);
		}

		private void RefreshGroups()
		{
			GroupsDisplayList.Clear();

			int displayGroupIndex = 1;
			int roundIndex = 0;
			foreach (List<List<PlayerData>> round in AllGroupsList)
			{
				foreach (List<PlayerData> group in round)
				{
					PlayerData groupData = new PlayerData("Group " + displayGroupIndex);
					groupData.Round = roundIndex;

					GroupsDisplayList.Add(groupData);

					foreach (PlayerData pd in group)
					{
						GroupsDisplayList.Add(pd);
					}

					++displayGroupIndex;
				}

				++roundIndex;
			}
		}

		private void SelectGroup_Click(object sender, RoutedEventArgs e)
		{
			PlayerData pd = (sender as Button).Tag as PlayerData;

			SelectGroup(pd);
		}

		private int GetGroupIndex(int rawGroupIndex, int round)
		{
			for (int i = 0; i < round; ++i)
			{
				rawGroupIndex -= AllGroupsList[i].Count;
			}

			return rawGroupIndex - 1;
		}

		private void SelectGroup(PlayerData pd)
		{
			string groupNumString = pd.PlayerName.Replace("Group ", "");
			int groupIndex = 0;
			if (int.TryParse(groupNumString, out groupIndex))
			{
				SelectGroup(GetGroupIndex(groupIndex, pd.Round), pd.Round);
			}
		}

		private void SelectGroup(int groupIndex, int roundIndex)
		{
			NowPlayingList.Clear();

			var playerList = AllGroupsList[roundIndex][groupIndex];

			foreach (PlayerData pd in playerList)
			{
				NowPlayingList.Add(pd);
			}

			ShowStartGroupButton = true;

			InstructionsDisplay = "Click Start Group -->";
		}

		private bool TryGetLeaderIndex(out int outLeaderIndex)
		{
			outLeaderIndex = -1;
			int index = 0;
			foreach (PlayerData pd in NowPlayingList)
			{
				if (pd.IsLeader)
				{
					outLeaderIndex = index;
					break;
				}

				++index;
			}

			return outLeaderIndex != -1;
		}

		private PlayerData GetLeader()
		{
			int leaderIndex;
			if (TryGetLeaderIndex(out leaderIndex))
			{
				return NowPlayingList[leaderIndex];
			}

			return null;
		}

		private void IncrementLeader()
		{
			if (NowPlayingList.Count == 0)
			{
				return;
			}

			int currentLeader;
			if (TryGetLeaderIndex(out currentLeader))
			{
				currentLeader = (currentLeader + 1) % NowPlayingList.Count;
			}
			else
			{
				currentLeader = 0;
			}

			foreach (PlayerData pd in NowPlayingList)
			{
				pd.IsLeader = false;
				pd.IsPerforming = false;
			}

			PlayerData leader = NowPlayingList[currentLeader];
			leader.IsLeader = true;
			leader.AddHistoryButton(EHistoryButtonState.LeaderSet);

			ShowSetLeaderComboButton = true;

			InstructionsDisplay = "Leader " + leader.PlayerName + " set combo";
		}

		private void UpdateNowPlaying()
		{
			Save();

			if (Performer1 == null || Performer2 == null)
			{
				int leaderIndex;
				if (TryGetLeaderIndex(out leaderIndex))
				{
					PlayerData leader = NowPlayingList[leaderIndex];

					for (int i = 1; i < NowPlayingList.Count; ++i)
					{
						PlayerData pd = NowPlayingList[(leaderIndex + i) % NowPlayingList.Count];

						if (pd.HistoryButtons.Count < leader.HistoryButtons.Count)
						{
							pd.IsPerforming = true;
							pd.AddHistoryButton(EHistoryButtonState.PerformerGo);

							if (Performer1 == null)
							{
								Performer1 = pd;
							}
							else
							{
								Performer2 = pd;
							}

							if (Performer1 != null && Performer2 != null)
							{
								break;
							}
						}
					}
				}
			}

			if (Performer1 == null && Performer2 == null)
			{
				FinishCycle();

				PlayerData leader = GetLeader();
				if (leader != null && leader.HistoryButtons.Count >= NowPlayingList.Count)
				{
					TryFinishGroup();
				}
				else
				{
					IncrementLeader();
				}
			}
			else
			{
				bool multiplePerformers = Performer1 != null && Performer2 != null;

				InstructionsDisplay = "Performer" + (multiplePerformers ? "s " : " ") +
					(Performer1 != null ? Performer1.PlayerName : "") +
					(multiplePerformers ? " and " : "") +
					(Performer2 != null ? Performer2.PlayerName : "");
			}
		}

		private void FinishCycle()
		{
			AwardCyclePoints();
		}

		private void AwardCyclePoints()
		{
			int successCount = 0;
			foreach (PlayerData pd in NowPlayingList)
			{
				successCount += pd.GetLastResult() ? 1 : 0;
			}

			PlayerData leader = GetLeader();
			int playerCount = NowPlayingList.Count;

			if (successCount == 1)
			{
				leader.AwardPoints(playerCount);
				AwardSuccesfulPerformerPoints(playerCount);
			}
			else if (successCount == 0 || successCount == playerCount - 1)
			{
				leader.AwardPoints(0);

				foreach (PlayerData pd in NowPlayingList)
				{
					if (!pd.IsLeader)
					{
						pd.AwardPoints(NobodyEverybodySucessPoints);
					}
				}
			}
			else
			{
				int points = playerCount - successCount;

				leader.AwardPoints(points);
				AwardSuccesfulPerformerPoints(points);
			}
		}

		private void AwardSuccesfulPerformerPoints(int points)
		{
			foreach (PlayerData pd in NowPlayingList)
			{
				if (pd.GetLastResult())
				{
					pd.AwardPoints(points);
				}
			}
		}

		private List<int> GetSortedScores()
		{
			List<int> sortedScores = new List<int>();
			foreach (PlayerData pd in NowPlayingList)
			{
				sortedScores.Add(pd.RoundPoints);
			}

			sortedScores.Sort((int a, int b) => {
				if (a == b)
				{
					return 0;
				}
				else if (a > b)
				{
					return -1;
				}
				else
				{
					return 1;
				}
			});

			return sortedScores;
		}

		private void TryFinishGroup()
		{
			if (CheckIsFinalRound())
			{
				if (CheckGameFinished())
				{
					List<PlayerData> finalGroup = AllGroupsList.Last().Last();

					finalGroup.Sort((PlayerData a, PlayerData b) =>
					{
						if (a.RoundPoints == b.RoundPoints)
						{
							return 0;
						}
						else if (a.RoundPoints > b.RoundPoints)
						{
							return -1;
						}
						else
						{
							return 1;
						}
					});

					for (int i = 0; i < 3 && i < finalGroup.Count; ++i)
					{
						PlayerData pd = finalGroup[i];

						pd.FinishPlace = i + 1;
					}

					NowPlayingList.Clear();
					foreach (PlayerData pd in finalGroup)
					{
						pd.IsLeader = false;

						NowPlayingList.Add(pd);
					}

					InstructionsDisplay = "Finished. Congrats " + finalGroup[0].PlayerName + "!!!";
				}
				else
				{
					IncrementLeader();
				}
			}
			else if (NowPlayingList.Count > TeamsMakingCut)
			{
				List<int> sortedScores = GetSortedScores();

				int cutScore = 0;
				if (sortedScores[TeamsMakingCut] != sortedScores[TeamsMakingCut - 1])
				{
					cutScore = sortedScores[TeamsMakingCut - 1];

					foreach (PlayerData pd in NowPlayingList)
					{
						if (pd.RoundPoints < cutScore)
						{
							pd.IsCut = true;
						}
						else
						{
							++pd.Round;
						}

						pd.IsLeader = false;
					}

					if (CheckRoundFinished())
					{
						AddGroups();
					}

					InstructionsDisplay = "Select Next Group";
				}
				else
				{
					IncrementLeader();
				}
			}
		}

		private bool CheckRoundFinished()
		{
			foreach (List<PlayerData> group in AllGroupsList.Last())
			{
				int cutCount = 0;
				foreach (PlayerData pd in group)
				{
					cutCount += pd.IsCut ? 1 : 0;
				}

				if (group.Count - cutCount > TeamsMakingCut)
				{
					return false;
				}
			}

			return true;
		}

		private bool CheckIsFinalRound()
		{
			return AllGroupsList.Count > 0 && AllGroupsList.Last().Count == 1;
		}

		private bool CheckGameFinished()
		{
			if (CheckIsFinalRound())
			{
				List<int> sortedScores = GetSortedScores();
				List<int> topScores = new List<int>();
				bool topThreeDifferent = true;

				for (int i = 0; i < 4 && i < sortedScores.Count; ++i)
				{
					if (topScores.Contains(sortedScores[i]))
					{
						topThreeDifferent = false;
						break;
					}
					else
					{
						topScores.Add(sortedScores[i]);
					}
				}

				return topThreeDifferent;
			}

			return false;
		}

		private void HistoryButton_Click(object sender, RoutedEventArgs e)
		{

		}

		private void SetLeaderComboButton_Click(object sender, RoutedEventArgs e)
		{
			ShowSetLeaderComboButton = false;

			UpdateNowPlaying();

			PlayerData leader = GetLeader();
			if (leader != null)
			{
				leader.SetLeaderCombo();
			}
		}

		private void PerformerSuccessButton1_Click(object sender, RoutedEventArgs e)
		{
			Performer1.SetPerformerResult(true);

			Performer1 = null;

			UpdateNowPlaying();
		}

		private void PerformerFailButton1_Click(object sender, RoutedEventArgs e)
		{
			Performer1.SetPerformerResult(false);

			Performer1 = null;

			UpdateNowPlaying();
		}

		private void PerformerSuccessButton2_Click(object sender, RoutedEventArgs e)
		{
			Performer2.SetPerformerResult(true);

			Performer2 = null;

			UpdateNowPlaying();
		}

		private void PerformerFailButton2_Click(object sender, RoutedEventArgs e)
		{
			Performer2.SetPerformerResult(false);

			Performer2 = null;

			UpdateNowPlaying();
		}

		private void StartGroupButton_Click(object sender, RoutedEventArgs e)
		{
			ShowStartGroupButton = false;

			IncrementLeader();
		}

		private void Save()
		{
			try
			{
				if (!Directory.Exists("Saves"))
				{
					Directory.CreateDirectory("Saves");
				}

				XmlSerializer serializer = new XmlSerializer(typeof(SaveDataContainer));
				using (StringWriter retString = new StringWriter())
				{
					serializer.Serialize(retString, SaveData);

					string curDir = CurrentDirectory + @"\Saves\" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xml";

					using (StreamWriter saveFile = new StreamWriter(curDir))
					{
						saveFile.Write(retString.ToString());
					}
				}
			}
			catch (Exception e)
			{
				MessageBox.Show("Failed to Save.\r\n" + e.Message, "Attention!");
			}
		}

		private void Load(string filename)
		{
			if (File.Exists(filename))
			{
				SaveData.Clear();
				NowPlayingList.Clear();

				using (StreamReader saveFile = new StreamReader(filename))
				{
					XmlSerializer serializer = new XmlSerializer(typeof(SaveDataContainer));
					SaveData = (SaveDataContainer)serializer.Deserialize(saveFile);

					RefreshGroups();

					OnPropertyChanged("");
				}
			}
		}

		private void FollowWindow_Closing(object sender, CancelEventArgs e)
		{
			Save();
		}

		private void MenuItem_Save(object sender, RoutedEventArgs e)
		{
			Save();
		}

		private void MenuItem_Load(object sender, RoutedEventArgs e)
		{
			Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
			ofd.DefaultExt = ".xml";
			ofd.Filter = "XML Files (*.xml)|*.xml";
			ofd.Multiselect = false;
			ofd.InitialDirectory = CurrentDirectory + @"\Saves\";

			if (ofd.ShowDialog() == true)
			{
				Load(ofd.FileName);
			}
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
				OnPropertyChanged("NowPlayingDisplayText");
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
				switch (FinishPlace)
				{
					case 1:
						return Brushes.Gold;
					case 2:
						return Brushes.DarkGray;
					case 3:
						return Brushes.Peru;
				}

				if (IsGroup)
				{
					return Brushes.LightGray;
				}
				else if (PlayerName.StartsWith("Round"))
				{
					return Brushes.LightBlue;
				}
				else if (IsCut)
				{
					return Brushes.OrangeRed;
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
				OnPropertyChanged("NowPlayingDisplayText");
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
				OnPropertyChanged("NowPlayingDisplayText");
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
				OnPropertyChanged("BgColor");
			}
		}
		public string NowPlayingDisplayText
		{
			get
			{
				return (IsLeader ? "Leader: " : "") + (IsPerforming ? "Go: " : "") + PlayerName;
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
				OnPropertyChanged("RoundPointsDisplay");
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
				round = value;
				OnPropertyChanged("Round");
			}
		}
		int finishPlace = 0;
		public int FinishPlace
		{
			get { return finishPlace; }
			set
			{
				finishPlace = value;
				OnPropertyChanged("FinishPlace");
				OnPropertyChanged("BgColor");
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

			historyButtons.Add(new HistoryButton(EHistoryButtonState.PerformerPoints));
			historyButtons.Add(new HistoryButton(EHistoryButtonState.PerformerPoints));
			historyButtons.Add(new HistoryButton(EHistoryButtonState.PerformerPoints));
			historyButtons.Add(new HistoryButton(EHistoryButtonState.PerformerPoints));
		}

		public PlayerData(string inName, int inRound)
		{
			playerName = inName;
			Round = inRound;
		}

		public void AddHistoryButton(EHistoryButtonState state)
		{
			historyButtons.Add(new HistoryButton(state));
		}

		public void SetLeaderCombo()
		{
			if (HistoryButtons.Count == 0)
			{
				return;
			}

			HistoryButton hb = historyButtons.Last();
			if (hb.State == EHistoryButtonState.LeaderSet)
			{
				hb.State = EHistoryButtonState.LeaderWaiting;
			}
		}

		public void SetPerformerResult(bool success)
		{
			if (HistoryButtons.Count == 0)
			{
				return;
			}

			HistoryButton hb = historyButtons.Last();
			if (hb.State == EHistoryButtonState.PerformerGo)
			{
				hb.State = EHistoryButtonState.PerformerPoints;
				hb.Success = success;

				IsPerforming = false;
			}
		}

		public bool GetLastResult()
		{
			return historyButtons.Count > 0 && historyButtons.Last().Success;
		}

		public void AwardPoints(int points)
		{
			TotalPoints += points; // fix this to just calculate the sum of all HistoryButton
			RoundPoints += points;

			if (historyButtons.Count > 0)
			{
				HistoryButton hb = historyButtons.Last();
				hb.Points = points;

				if (IsLeader)
				{
					hb.State = EHistoryButtonState.LeaderPoints;
				}
				else
				{
					hb.State = EHistoryButtonState.PerformerPoints;
				}
			}
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

		EHistoryButtonState state = EHistoryButtonState.LeaderPoints;
		public EHistoryButtonState State
		{
			get { return state; }
			set
			{
				state = value;
				OnPropertyChanged("State");
				OnPropertyChanged("DisplayText");
			}
		}
		int points = 0;
		public int Points
		{
			get { return points; }
			set
			{
				points = value;
				OnPropertyChanged("Points");
				OnPropertyChanged("DisplayText");
			}
		}
		bool success = false;
		public bool Success
		{
			get { return success; }
			set
			{
				success = value;
				OnPropertyChanged("Success");
			}
		}


		public string PointsDisplay
		{
			get { return "5"; }
		}
		public string DisplayText
		{
			get
			{
				switch (State)
				{
					case EHistoryButtonState.LeaderSet:
						return "Go";
					case EHistoryButtonState.LeaderWaiting:
						return "Wait";
					case EHistoryButtonState.PerformerGo:
						return "Go";
				}

				return Points.ToString();
			}
		}
		public Brush TextColor
		{
			get
			{
				switch (State)
				{
					case EHistoryButtonState.LeaderPoints:
						return Brushes.DarkOrange;
					case EHistoryButtonState.LeaderSet:
						return Brushes.DarkOrange;
					case EHistoryButtonState.LeaderWaiting:
						return Brushes.DarkOrange;
				}

				return Brushes.Black;
			}
		}

		public HistoryButton()
		{

		}

		public HistoryButton(EHistoryButtonState inState)
		{
			State = inState;
		}

	}

	public enum EHistoryButtonState
	{
		LeaderSet,
		LeaderWaiting,
		LeaderPoints,
		PerformerGo,
		PerformerPoints
	}

	public class SaveDataContainer
	{
		public List<List<List<PlayerData>>> AllGroupsList = new List<List<List<PlayerData>>>();

		public SaveDataContainer()
		{
		}

		public void Clear()
		{
			AllGroupsList.Clear();
		}
	}
}
