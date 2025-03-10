using FFmpeg.AutoGen;

namespace RTSPCamLib
{
   internal static class FfmpegHandler
   {
      internal static void Initialize()
      {
         var ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg");
         ffmpeg.RootPath = Directory.Exists(ffmpegPath) ? ffmpegPath : throw new Exception("FFmpeg binaries not found. Ensure they are included.");

         ffmpeg.avformat_network_init();

      }
   }
}
