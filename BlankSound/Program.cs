using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace BlankSound
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.SetServiceName("BlankSound");
                x.SetDisplayName("BlankSound");
                x.SetDescription("Play blank sound for Dell XPS to fixed sound delay.");
                x.Service<Sound>(s =>
                {
                    s.ConstructUsing(sound => new Sound());
                    s.WhenStarted(sound => sound.Play());
                    s.WhenStopped(sound => sound.Stop());
                });
                x.RunAsLocalSystem();
            });
        }
    }

    class Sound
    {

        private LoopAudioFileReader Audio = null;
        private WaveOutEvent Wave = null;

        public Sound()
        {
            this.Audio = new LoopAudioFileReader(@"D:\Projects\BlankSound\Sound.mp3");
            this.Audio.OnLooping = () =>
            {
                this.Log("Looping");
            };
            this.Wave = new WaveOutEvent();
            this.Wave.Init(this.Audio);
        }

        public void Play()
        {
            this.Wave.Play();
            this.Log("Play");
        }

        public void Stop()
        {
            this.Wave.Stop();
            if (this.Wave != null) this.Wave.Dispose();
            if (this.Audio != null) this.Audio.Dispose();
            this.Log("Stop");
        }

        private void Log(string message)
        {
            message = string.Format("{0} -> {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), message + Environment.NewLine);
            File.AppendAllText(@"D:\Projects\BlankSound\Logs.txt", message, Encoding.UTF8);
        }
    }


    class LoopAudioFileReader : AudioFileReader
    {
        // 构造函数
        public LoopAudioFileReader(string filename) : base(filename) { }

        // 重写读取音频流的方法
        public override int Read(byte[] buffer, int offset, int count)
        {
            int total = 0;
            while (total < count)
            {
                int read = base.Read(buffer, offset + total, count - total);
                if (read == 0)
                {
                    if (base.Position == 0) break;
                    else
                    {
                        base.Position = 0;
                        this.OnLooping();
                    }
                }
                total += read;
            }
            return total;
        }

        // 监听循环事件
        public Action OnLooping;
    }
}
