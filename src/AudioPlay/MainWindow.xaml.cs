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
using NAudio;
using NAudio.Wave;

namespace AudioPlay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private bool closing = false;
        private bool manualchange = false;
        private TimeSpan? time = null;
        public static string defaultpath = "C:\\Users\\Long\\source\\repos\\AudioPlay\\AudioPlay\\Audio";
        public static string[] allowedformat = { ".mp3", ".wav", ".aiff", ".wma", ".aac", ".flac", ".alac", ".ogg" };
        public static List<Audio> laudio = new List<Audio>();
        private int cusor = 0;
        private WaveOutEvent speaker;
        private AudioFileReader disk;
        private DispatcherTimer timer;
        int intervalcount = 1;
        int maxcount;
        int pausedindex;
        




        public MainWindow()
        {
            InitializeComponent();
            Reloadfolder();
            Loadtolistv();
            speaker = new WaveOutEvent();   //just create to avoid error
            speaker.PlaybackStopped += Speaker_PlaybackStopped; 

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

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


        private void Reloadfolder()
        {
            laudio.Clear();
            string[] ado = Directory.GetFiles(defaultpath);
            foreach (var item in ado)
            { // filter audio files -----------
                if (allowedformat.Contains(System.IO.Path.GetExtension(item)) == false) { continue; }
                Audio audio1 = new Audio();
                audio1.Name = System.IO.Path.GetFileNameWithoutExtension(item);
                audio1.Format = System.IO.Path.GetExtension(item);
                audio1.Directory = item;
                //var reader = new AudioFileReader(item);

                audio1.Duration = new AudioFileReader(item).TotalTime;
                laudio.Add(audio1);
            }
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
                if (cusor + 1 >= laudio.Count ) {
                    cusor = 0;
                }
                else {
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
    }

    public class Audio { 
        public string Name { get; set; }
        public string Format { get; set; }
        public string Directory { get; set; }
        public TimeSpan Duration { get; set; }
       
    }
    
        
    

}