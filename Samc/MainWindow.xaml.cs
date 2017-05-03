using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
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
using System.Windows.Threading;

using Samc.SAMI;

/// <summary>
/// http://www.wpf-tutorial.com/audio-video/how-to-creating-a-complete-audio-video-player/
/// </summary>
namespace Samc
{
	/// <summary>
	/// MainWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class MainWindow : Window
	{
		/// <summary>
		/// 실시간 처리를 위해서 사용하는 타이머
		/// </summary>
		private DispatcherTimer m_timer = new DispatcherTimer();

		/// <summary>
		/// smi 매니저
		/// </summary>
		private SmiManager m_SM = new SmiManager();

		/// <summary>
		/// 슬라이드를 드래고 하고 있는지 여부
		/// </summary>
		private bool m_bSliderDrag = false;
		/// <summary>
		/// 드래그중에 미디어 이전 상태
		/// </summary>
		private MediaState m_msDrag = MediaState.Stop;

		/// <summary>
		/// 미디어 준비 여부
		/// </summary>
		private bool m_bMedia = false;
		/// <summary>
		/// 자막 준비 여부
		/// </summary>
		private bool m_bSami = false;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			//싱크 처리 타이머 설정
			this.m_timer = new DispatcherTimer();
			this.m_timer.Interval = TimeSpan.FromMilliseconds(100);
			//this.m_timer.Interval = TimeSpan.FromSeconds(1);
			this.m_timer.Tick += M_timer_Tick;
		}

		#region 메뉴
		private void miOpenSAMI_Click(object sender, RoutedEventArgs e)
		{

			//파일 다얄로그 생성
			OpenFileDialog ofdMovie = new OpenFileDialog();
			//파일 필터 지정
			ofdMovie.Filter = "SMI 파일|*.*";

			//다얄로그 뛰우기
			if (true == ofdMovie.ShowDialog())
			{
				//파일 읽기
				string sFile = File.ReadAllText(ofdMovie.FileName, Encoding.UTF8);
				//파씽
				this.m_SM.Analysis(sFile);

				//추출데이터 바인딩
				this.lvSmiData.ItemsSource = this.m_SM.SMI.Data;

				this.m_bSami = true;
			}
		}

		private void miSaveSAMI_Click(object sender, RoutedEventArgs e)
		{

			//파일 다얄로그 생성
			SaveFileDialog sfdSami = new SaveFileDialog();

			//파일 필터 지정
			sfdSami.Filter = "SMI 파일|*.*";

			//다얄로그 뛰우기
			if (true == sfdSami.ShowDialog())
			{
				//파일 저장
				File.WriteAllText(sfdSami.FileName
									, this.m_SM.GetFileData()
									, Encoding.UTF8);
			}
		}



		private void miOpenMedia_Click(object sender, RoutedEventArgs e)
		{
			//파일 다얄로그 생성
			OpenFileDialog ofdMovie = new OpenFileDialog();
			//파일 필터 지정
			ofdMovie.Filter = "동영상 파일|*.*";

			//다얄로그 띄우기
			if (true == ofdMovie.ShowDialog())
			{
				string selectedFileName = ofdMovie.FileName;
				//FileNameLabel.Content = selectedFileName;
				meMain.Source = new Uri(selectedFileName);
				
				//싱크처리 타이머 시작
				this.m_timer.Start();
			}

		}
		#endregion

		private void mediaElement_MediaOpened(object sender, RoutedEventArgs e)
		{
			//미디어가 준비 되었다.

			sliderTimeline.Minimum = 0;
			sliderTimeline.Maximum = meMain.NaturalDuration.TimeSpan.TotalMilliseconds;
			sliderTimeline.Value = 0;

			this.m_bMedia = true;
		}


		#region 동영상 제어 - 앞으로 뒤로 관련

		private void btnForward_Frame1_Click(object sender, RoutedEventArgs e)
		{
			this.Movie_Move(this.sliderTimeline.Value - 1d, false);
		}
		private void btnForward_200ms_Click(object sender, RoutedEventArgs e)
		{
			this.Movie_Move(-200d);
		}
		private void btnForward_500ms_Click(object sender, RoutedEventArgs e)
		{
			this.Movie_Move(-500d);
		}
		private void btnForward_1000ms_Click(object sender, RoutedEventArgs e)
		{
			this.Movie_Move(-1000d);
		}
		private void btnForward_5000ms_Click(object sender, RoutedEventArgs e)
		{
			this.Movie_Move(-5000d);
		}
		private void btnForward_10000ms_Click(object sender, RoutedEventArgs e)
		{
			this.Movie_Move(-10000d);
		}

		private void btnBackward_10000ms_Click(object sender, RoutedEventArgs e)
		{
			this.Movie_Move(10000d);
		}
		private void btnBackward_5000ms_Click(object sender, RoutedEventArgs e)
		{
			this.Movie_Move(5000d);
		}
		private void btnBackward_1000ms_Click(object sender, RoutedEventArgs e)
		{
			this.Movie_Move(1000d);
		}
		private void btnBackward_500ms_Click(object sender, RoutedEventArgs e)
		{
			this.Movie_Move(500d);
		}
		private void btnBackward_200ms_Click(object sender, RoutedEventArgs e)
		{
			this.Movie_Move(200d);
		}
		private void btnBackward_Frame1_Click(object sender, RoutedEventArgs e)
		{
			this.Movie_Move(this.sliderTimeline.Value + 1d, false);
		}


		private void Movie_Move(double dAddMilliseconds, bool bAdd = true)
		{
			this.Movie_Move(TimeSpan.FromMilliseconds(dAddMilliseconds), bAdd);
		}

		private void Movie_Move(TimeSpan tsValue, bool bAdd = true)
		{
			if (true == bAdd)
			{//값 더하기
				this.meMain.Position += tsValue;
			}
			else
			{//값 대입하기
				this.meMain.Position = tsValue;
			}

			if (MediaState.Pause == this.GetMediaState(this.meMain))
			{//정지 상태
			 //정지 상태라면
			 //재생 후 정지를 시켜 화면을 갱신해준다.
			 //this.meMain.Play();
			 //this.meMain.Pause();
			 //aaaa();
			}
		}

		/// <summary>
		/// http://stackoverflow.com/questions/4338951/how-do-i-determine-if-mediaelement-is-playing
		/// </summary>
		/// <param name="myMedia"></param>
		/// <returns></returns>
		private MediaState GetMediaState(MediaElement myMedia)
		{
			FieldInfo hlp = typeof(MediaElement).GetField("_helper", BindingFlags.NonPublic | BindingFlags.Instance);
			object helperObject = hlp.GetValue(myMedia);
			FieldInfo stateField = helperObject.GetType().GetField("_currentState", BindingFlags.NonPublic | BindingFlags.Instance);
			MediaState state = (MediaState)stateField.GetValue(helperObject);
			return state;
		}

		#endregion


		#region 동영상 제어 - 슬라이드 관련
		private void sliderTimeline_DragStarted(object sender, RoutedEventArgs e)
		{
			this.m_bSliderDrag = true;
			this.m_msDrag = this.GetMediaState(this.meMain);

			//드래그중에는 동영상을 멈춘다.
			this.meMain.Pause();
		}

		private void sliderTimeline_DragCompleted(object sender, RoutedEventArgs e)
		{
			//드래그 완료
			//상태값 복구
			this.m_bSliderDrag = false;

			//값을 설정한다.
			this.meMain.Position = TimeSpan.FromMilliseconds(this.sliderTimeline.Value);

			if (MediaState.Play == this.m_msDrag)
			{//이전 상태가 플래이였다.
			 //다시 재생 시킨다.
				this.meMain.Play();
			}
		}

		private void M_timer_Tick(object sender, EventArgs e)
		{
			//this.Title = this.m_bSliderDrag + " " + this.m_msDrag.ToString();

			//텍스트 갱신
			this.TimelineText();

			if ((true == this.m_bMedia) 
				&& (true == this.m_bSami))
			{//미디어와 자막이 모두 있다.
				//출력된 인덱스
				int nOutIndex;
				//캡션찾기
				this.labCaption.Content
					= this.m_SM.GetCaption(this.meMain.Position.TotalMilliseconds, out nOutIndex);
				if ((0 <= nOutIndex)
					&& (MediaState.Play == this.GetMediaState(this.meMain)))
				{//출력된 인덱스가 출력값 안에 있다.
					this.lvSmiData.SelectedIndex = nOutIndex;
					this.lvSmiData.ScrollIntoView(this.lvSmiData.SelectedItem);
				}
			}
		}

		private void TimelineText()
		{
			try
			{
				labPlayTime.Content = string.Format("{0:0}/{1:0}"
					, this.meMain.Position.TotalMilliseconds
					, this.meMain.NaturalDuration.TimeSpan.TotalMilliseconds);

				if (false == this.m_bSliderDrag)
				{//드래그 중이 아니다.
				 //슬라이더를 움직여 준다.
				 //this.sliderTimeline.Value = this.meMain.Position.TotalSeconds;
					this.sliderTimeline.Value = this.meMain.Position.TotalMilliseconds;
				}
			}
			catch { }
		}

		#endregion


		#region 동영상 제어 - 재생 제어 관련
		private void btnPlay_Click(object sender, RoutedEventArgs e)
		{
			meMain.Play();
			btnPlay.Visibility = Visibility.Hidden;
			btnPause.Visibility = Visibility.Visible;
		}

		private void btnPause_Click(object sender, RoutedEventArgs e)
		{
			meMain.Pause();
			btnPlay.Visibility = Visibility.Visible;
			btnPause.Visibility = Visibility.Hidden;
		}

		private void btnStop_Click(object sender, RoutedEventArgs e)
		{
			meMain.Stop();
			btnPlay.Visibility = Visibility.Visible;
			btnPause.Visibility = Visibility.Hidden;
			//this.m_timer.Stop();
		}

		#endregion

		
		private void lvAction_Click_1(object sender, RoutedEventArgs e)
		{
			//(sender as FrameworkElement).IsSealed. = true;
			(sender as FrameworkElement).ContextMenu.IsOpen = true;
		}


		#region 오른쪽 클릭 메뉴
		//동영상을 이 위치로 처리 ◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇
		private void contextMovieSetPosition_Click(object sender, RoutedEventArgs e)
		{
			//동영상위치를 설정해준다.
			this.meMain.Position = TimeSpan.FromMilliseconds(this.m_SM.SMI.Data[this.lvSmiData.SelectedIndex].Start);
		}

		//이 뒤로 처리 ◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇
		private void contextNowBack_PlayTime_Click(object sender, RoutedEventArgs e)
		{
			this.Sync_Add(SmiManager.TypeSyncAdd_Taget.NowBack, SmiManager.TypeSyncAdd_Time.PlayTime);
		}

		private void contextNowBack_Minus500ms_Click(object sender, RoutedEventArgs e)
		{
			this.Sync_Add(SmiManager.TypeSyncAdd_Taget.NowBack, SmiManager.TypeSyncAdd_Time.Minus500ms);
		}

		private void contextNowBack_Plus500ms_Click(object sender, RoutedEventArgs e)
		{
			this.Sync_Add(SmiManager.TypeSyncAdd_Taget.NowBack, SmiManager.TypeSyncAdd_Time.Plus500ms);
		}

		//이것만 처리 ◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇
		private void contextOne_PlayTime_Click(object sender, RoutedEventArgs e)
		{
			this.Sync_Add(SmiManager.TypeSyncAdd_Taget.One, SmiManager.TypeSyncAdd_Time.PlayTime);
		}

		private void contextOne_Minus500ms_Click(object sender, RoutedEventArgs e)
		{
			this.Sync_Add(SmiManager.TypeSyncAdd_Taget.One, SmiManager.TypeSyncAdd_Time.Minus500ms);
		}

		private void contextOne_Plus500ms_Click(object sender, RoutedEventArgs e)
		{
			this.Sync_Add(SmiManager.TypeSyncAdd_Taget.One, SmiManager.TypeSyncAdd_Time.Plus500ms);
		}

		//전체 처리 ◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇◇
		private void contextAll_PlayTime_Click(object sender, RoutedEventArgs e)
		{
			this.Sync_Add(SmiManager.TypeSyncAdd_Taget.All, SmiManager.TypeSyncAdd_Time.PlayTime);
		}

		private void contextAll_Minus500ms_Click(object sender, RoutedEventArgs e)
		{
			this.Sync_Add(SmiManager.TypeSyncAdd_Taget.All, SmiManager.TypeSyncAdd_Time.Minus500ms);
		}

		private void contextAll_Plus500ms_Click(object sender, RoutedEventArgs e)
		{
			this.Sync_Add(SmiManager.TypeSyncAdd_Taget.All, SmiManager.TypeSyncAdd_Time.Plus500ms);
		}
		#endregion

		private void Sync_Add(SmiManager.TypeSyncAdd_Taget typeTaget, SmiManager.TypeSyncAdd_Time typeTime)
		{
			//인덱스 가지고 오기
			int nIndex = this.lvSmiData.SelectedIndex;

			//기준 시작 지점 가지고 오기
			int nSync = 0;

			//시간 계산
			switch (typeTime)
			{
				case SmiManager.TypeSyncAdd_Time.PlayTime:
					nSync = this.m_SM.SMI.Data[nIndex].Start;
					nSync = Convert.ToInt32(this.meMain.Position.TotalMilliseconds) - this.m_SM.SMI.Data[nIndex].Start;
					break;
				case SmiManager.TypeSyncAdd_Time.Minus500ms:
					nSync = -500;
					break;
				case SmiManager.TypeSyncAdd_Time.Plus500ms:
					nSync = 500;
					break;
			}

			//싱크를 밀어 준다.
			this.m_SM.Sync_Add(typeTaget, nIndex, nSync);

			ICollectionView view 
				= CollectionViewSource.GetDefaultView(this.lvSmiData.ItemsSource);
			view.Refresh();
		}

		
	}
}
