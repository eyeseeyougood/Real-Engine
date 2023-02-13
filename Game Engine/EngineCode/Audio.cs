using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Media;

namespace Game_Engine.Audio
{
    public class AudioManager
    {
        public SoundPlayer audioPlayer;

        public void PlayWav(string filepath)
        {
            audioPlayer.SoundLocation = filepath;
            audioPlayer.Play();
        }

        public void CreatePlayer()
        {
            audioPlayer = new SoundPlayer();
        }

        public void Stop()
        {
            if (audioPlayer != null)
                audioPlayer.Stop();
        }

        public void PlayWavLooped(string filepath)
        {
            audioPlayer.SoundLocation = filepath;
            audioPlayer.PlayLooping();
        }
    }
}
