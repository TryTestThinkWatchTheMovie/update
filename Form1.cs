using NAudio.Wave;
using System.Net;
using System.Net.Sockets;

namespace tcpnadudio
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            using var client = new TcpClient();

            var hostname = "25643.live.streamtheworld.com";
            client.Connect(hostname, 80);
            using NetworkStream networkStream = client.GetStream();
            Stream ms = new MemoryStream();

            PlayMp3FromUrl("25643.live.streamtheworld.com");

            void PlayMp3FromUrl(string url)
            {
                new Thread(delegate (object o)
                {
                    var response = WebRequest.Create(url).GetResponse();
                    using (var stream = response.GetResponseStream())
                    {
                        byte[] buffer = new byte[65536]; // 64KB chunks
                        int read;
                        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            var pos = ms.Position;
                            ms.Position = ms.Length;
                            ms.Write(buffer, 0, read);
                            ms.Position = pos;
                        }
                    }
                }).Start();

                // Pre-buffering some data to allow NAudio to start playing
                while (ms.Length < 65536 * 10)
                    Thread.Sleep(1000);

                ms.Position = 0;
                using (WaveStream blockAlignedStream = new BlockAlignReductionStream(WaveFormatConversionStream.CreatePcmStream(new Mp3FileReader(ms))))
                {
                    using (WaveOut waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback()))
                    {
                        waveOut.Init(blockAlignedStream);
                        waveOut.Play();
                        while (waveOut.PlaybackState == PlaybackState.Playing)
                        {

 
 
                             System.Threading.Thread.Sleep(100);
                        }
                    }
                }
            }
        }
    }
}