using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Concurrent;

using Discord;
using Discord.Audio;

using YoutubeExplode;
using YoutubeExplode.Models;

namespace DoggoBot.Core.Services.Audio
{
    public class AudioService
    {
        private ConcurrentDictionary<ulong, IAudioClient> ourGuilds =
            new ConcurrentDictionary<ulong, IAudioClient>();

        private ConcurrentDictionary<ulong, Process> ourGuildProcess =
            new ConcurrentDictionary<ulong, Process>();

        private YoutubeClient ourYoutube = new YoutubeClient();

        private readonly string GuildAudioFiles = @"Data/Audio/";
        private string ToCopyFile;

        public async Task ConnectAsync(IGuild guild, IVoiceChannel voice, IMessageChannel chan = null)
        {
            IAudioClient audioClient;

            if (ourGuilds.TryGetValue(guild.Id, out IAudioClient value))
                return;
            if (voice.GuildId != guild.Id)
                return;

            if (chan != null) { await chan.SendMessageAsync("**Joining your voice channel!** :clap:"); audioClient = await voice.ConnectAsync(); }
            else { audioClient = await voice.ConnectAsync(); }

            if (ourGuilds.TryAdd(guild.Id, audioClient))
                return;
        }
        
        public async Task DisconnectAsync(IGuild guild, IMessageChannel chan = null)
        {
            if (ourGuilds.TryRemove(guild.Id, out IAudioClient audio))
            {
                if (chan != null) { await chan.SendMessageAsync("**Leaving your voice channel!** :wave:"); await audio.StopAsync(); }
                else { await audio.StopAsync(); }

                if (ourGuildProcess.TryRemove(guild.Id, out Process proc))
                    try { proc.Kill(); proc.Close(); }
                    catch { return; }
            }
        }

        public async Task SendAudioAsync(IGuild guild, string songID)
        {
            Video vidInfo = await ourYoutube.GetVideoAsync(songID);
            var vidStreams = vidInfo.AudioStreamInfos.OrderBy(x => x.Bitrate).Last();

            ToCopyFile = $@"{GuildAudioFiles}{guild.Id}/{vidInfo.Id}.{vidStreams.Container}";
            if (!Directory.Exists($"{GuildAudioFiles}{guild.Id}"))
                Directory.CreateDirectory($"{GuildAudioFiles}{guild.Id}");

            if (!File.Exists(ToCopyFile))
                using (var goIn = await ourYoutube.GetMediaStreamAsync(vidStreams))
                using (var goOut = File.Create(ToCopyFile))
                    await goIn.CopyToAsync(goOut);

            if (ourGuilds.TryGetValue(guild.Id, out IAudioClient audio))
            {
                var discordStream = audio.CreatePCMStream(AudioApplication.Music);
                await CreateGuildStream(ToCopyFile, guild.Id).StandardOutput.BaseStream.CopyToAsync(discordStream);
                await discordStream.FlushAsync();
            }
        }

        private Process CreateGuildStream(string songpath, ulong id)
        {
            Process guildAudio = Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = $"-hide_banner -loglevel panic -i \"{songpath}\" -vol 80 -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            });

            if (ourGuildProcess.TryAdd(id, guildAudio))
                return guildAudio;
            else
                return guildAudio;
        }
    }
}
