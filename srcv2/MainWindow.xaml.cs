using MahApps.Metro.Controls;
using NAudio;
using NAudio.Wave;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Threading;
using Path = System.IO.Path;


namespace AudioPlay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

        private void LaunchGitHubSite(object sender, RoutedEventArgs e)
        {
            const string url = "https://github.com/haohanxabo1/Audio_Player";

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ) { }
        }


        Random rnd; 
        private bool closing = false;
        private bool manualchange = false;
        private TimeSpan? time = null;
        public static string defaultpath = "Audio";
        string txtpath = "data.txt";
        public static string[] allowedformat = { ".mp3", ".wav", ".aiff", ".wma", ".aac", ".flac", ".alac", ".ogg" };
        public List<Audio> laudio = new List<Audio>();
        private int cusor = 0;
        private WaveOutEvent speaker;
        private AudioFileReader disk;
        private DispatcherTimer timer;
        int intervalcount = 1;
        int maxcount;
        int pausedindex;
        bool isRandom = false;



        public MainWindow()
        {
            
            InitializeComponent();
            Reloadpathtxt();
            Reloadfolder();
            Loadtolistv();
            speaker = new WaveOutEvent();   //just create to avoid error
            speaker.PlaybackStopped += Speaker_PlaybackStopped; 

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            rnd = new Random();

            

            

        }


        private void Reloadpathtxt() {
            try {
                if (File.Exists(txtpath))
                {
                    defaultpath = File.ReadAllText(txtpath); 
                }
            }
            catch (Exception) { }
        }


        private void settings_Click(object sender, RoutedEventArgs e)
        {
            settingwindow settingsWindow = new settingwindow();
            settingsWindow.ShowDialog();
        }

        private void folder_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", defaultpath);
        }


        private void reload_Click(object sender, RoutedEventArgs e)
        {
            Reloadfolder();
            Loadtolistv();
        }


        public void Reloadfolder()
        {
            laudio.Clear();
            try {
                string[] ado = Directory.GetFiles(defaultpath);
                foreach (var item in ado)
                { // filter audio files -----------
                    if (allowedformat.Contains(System.IO.Path.GetExtension(item)) == false) { continue; }
                    Audio audio1 = new Audio();
                    audio1.Name = Path.GetFileNameWithoutExtension(item);
                    audio1.Format = Path.GetExtension(item);
                    audio1.Directory = item;
                    //var reader = new AudioFileReader(item);

                    audio1.Duration = new AudioFileReader(item).TotalTime;
                    laudio.Add(audio1);
                }
            }
            catch (Exception) { }
           
        }

        private void Loadtolistv() {
            listviewaudio.Items.Clear();
            foreach (var item in laudio)
            {
                listviewaudio.Items.Add(item);
            }
            
        }

        private void uparrow_Click(object sender, RoutedEventArgs e)
        {

            if (cusor - 1 < 0)
            {
                cusor = 0;
            }
            else
            {
                cusor--;
                
            }
            listviewaudio.SelectedIndex = cusor;


        }

        private void downarrow_Click(object sender, RoutedEventArgs e)
        {
            if (cusor + 1 >= listviewaudio.Items.Count)
            {
                cusor = 0;
            }
            else
            {
                cusor++;
               
            }
            listviewaudio.SelectedIndex = cusor;
        }

        private void playaudio_Click(object sender, RoutedEventArgs e)
        {
           
            //speaker?.Dispose();
            //disk?.Dispose();
            if (laudio.Count == 0) {
                MessageBox.Show("Please Add Audio Files !!!");
                return;
            }
            //MessageBox.Show(listviewaudio.SelectedIndex.ToString());
            if (listviewaudio.SelectedIndex < 0)
            {
                listviewaudio.SelectedIndex = cusor;
            }
            else {
                cusor = listviewaudio.SelectedIndex;
                
            }
            //time = null;
            if (isRandom) {
                cusor = rnd.Next(0,laudio.Count);
               
            }
            
            Playat(cusor);
            playaudio.Visibility = Visibility.Hidden;
            stopaudio.Visibility = Visibility.Visible;
            


        }




        private void Playat(int cusor)
        {
            if (listviewaudio.SelectedIndex != cusor)
            {
                listviewaudio.SelectedIndex = cusor;
            }

            disk?.Dispose();

            if (speaker.PlaybackState != PlaybackState.Stopped) {
                manualchange = true;
                //speaker.Stop(); //already stopped - 
                }
                

            timer?.Stop();
            intervalcount = 1;


            if (time != null && pausedindex == cusor)
            {
                disk = new AudioFileReader(laudio[cusor].Directory);
                disk.CurrentTime = time.Value;
                time = null;
                pausedindex = -1;
            }
            else
            {
                disk = new AudioFileReader(laudio[cusor].Directory);
                time = null;
                pausedindex = -1;
            }


            speaker.Init(disk);
            timelabel2.Text = disk.TotalTime.ToString(@"mm\:ss");
            this.Title = "Playing - " + laudio[cusor].Name;
            

            speaker.Play();

            maxcount = (int)disk.TotalTime.TotalSeconds;
            processaudio.Maximum = maxcount;

            if (timer == null)
            {
                timer = new DispatcherTimer();

            }

            timer.Start();



        }


        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (intervalcount <= maxcount)
            {
                processaudio.Value = disk.CurrentTime.TotalSeconds;
                timelabel1.Text = disk.CurrentTime.ToString(@"mm\:ss"); //example 02:16
            }
            else {
                timer.Stop();
                intervalcount = 1;
            }
                intervalcount++;
        }

        private void Speaker_PlaybackStopped(object? sender, StoppedEventArgs e)
        {
            if (closing) { 
                speaker.Dispose();
                disk.Dispose();
            }
            if (manualchange) //before the audio stopped by user
            {
                manualchange = false;
                return;
            }
            if (time == null) {
                if (cusor + 1 >= laudio.Count)
                {
                    cusor = 0;
                }
                else if (isRandom) { 
                    cusor = rnd.Next(0,laudio.Count);
                }
                else
                {
                    cusor++;
                }
                Playat(cusor);
            }
            
            
        }

        private void stopaudio_Click(object sender, RoutedEventArgs e)
        {
            if (disk != null)
            {
                time = disk.CurrentTime; 
                pausedindex = cusor;
                speaker.Stop(); 
                stopaudio.Visibility = Visibility.Hidden;
                playaudio.Visibility = Visibility.Visible;

            }

        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            closing = true;
            speaker.Stop();
            base.OnClosing(e);
        }


        private void processaudio_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
         
            if (processaudio.IsMouseCaptureWithin && disk != null)
            {   
                disk.CurrentTime = TimeSpan.FromSeconds(processaudio.Value);
            }
        }

        private void nextAudio() {
            if (laudio.Count == 0) {
                return;
            }
            cusor++;
            if (cusor >= laudio.Count) { cusor = 0; }
            listviewaudio.SelectedIndex = cusor;
        }
        private void previousAudio() {
            
            if (laudio.Count == 0)
            {
                return;
            }
            cusor--;
            if (cusor < 0) { cusor = laudio.Count -1; }
            listviewaudio.SelectedIndex = cusor;
        }

        private void nexta_Click(object sender, RoutedEventArgs e)
        {
            if (stopaudio.Visibility == Visibility.Visible)
            {
                manualchange = true;

            }
            speaker.Stop();
            nextAudio();
            Playat(cusor);
            playaudio.Visibility = Visibility.Hidden;
            stopaudio.Visibility = Visibility.Visible;

        }

        private void previousa_Click(object sender, RoutedEventArgs e)
        {
            if (stopaudio.Visibility == Visibility.Visible)
            {
                manualchange = true;    

            }
            speaker.Stop();
            previousAudio();
            Playat(cusor);
            playaudio.Visibility = Visibility.Hidden;
            stopaudio.Visibility = Visibility.Visible;
        }

        private void randa_Toggled(object sender, RoutedEventArgs e)
        {
            if (isRandom == false) { isRandom = true; }
            else { isRandom = false; }
        }

        private void listviewaudio_Drop(object sender, DragEventArgs e)
        {
            var droppath = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (Directory.Exists(droppath[0]))
            {
                defaultpath = droppath[0];
                File.WriteAllText(txtpath,defaultpath);
                Reloadfolder();
                Loadtolistv();
                
            }
            else if (File.Exists(droppath[0]))
            {
                File.Move(droppath[0], Path.Combine(defaultpath, Path.GetFileName(droppath[0])));
                Reloadfolder();
                Loadtolistv();
            }
            else {
                MessageBox.Show("Error, Cannot Read The Dropped Items!");
            }
            
        }
    }

    public class Audio { 
        public string Name { get; set; }
        public string Format { get; set; }
        public string Directory { get; set; }
        public TimeSpan Duration { get; set; }
       
    }
    
        
    

}