using System.Drawing;
using System.Net;

using FFmpeg.AutoGen;

namespace RTSPCamLib
{
   public class CamHandler
   {
      private readonly CamViewer _viewer = new();
      public CamInfo camInfo;
      public EventHandler<Bitmap>? Handler;

      public CamHandler(CamInfo caminfo)
      {
         camInfo = caminfo;
         InitializeAsync();
      }

      public CamHandler(IPAddress ip, string parameters, string username, string password)
          : this(new CamInfo(ip, parameters, username, password)) { }

      public CamHandler(string ip, string parameters, string username, string password)
      {
         if (!IPAddress.TryParse(ip, out var ipAddress))
         {
            throw new ArgumentException($"Invalid Format for IP address: {ip}");
         }

         camInfo = new CamInfo(ipAddress, parameters, username, password);
         InitializeAsync();
      }

      private void InitializeAsync() => _ = Task.Run(Initialize);
      private void Initialize()
      {

         var ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg");
         ffmpeg.RootPath = Directory.Exists(ffmpegPath) ? ffmpegPath : throw new Exception("FFmpeg binaries not found. Ensure they are included.");

         _ = ffmpeg.avformat_network_init();

         ffmpeg.av_log_set_level(ffmpeg.AV_LOG_QUIET);

         Log.Write("CamHandler initialized");

         _viewer.OnFrameReceived += (sender, bitmap) => Handler?.Invoke(this, bitmap);

         if (CamValidator.IsValidRtspStream(camInfo))
         {
            //CamValidator.GetStreamInfo(camInfo.ConnectionString);
            _viewer.OpenRTSPStream(camInfo.ConnectionString);
         }
      }
   }
}
