using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;

using FFmpeg.AutoGen;

namespace RTSPCamLib
{
   public class CamValidator
   {

      unsafe public static bool IsValidRtspStream(string url)
      {
         bool retval = false;

         var ip = Regex.Match(url, @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b").Value;
         if (ip == string.Empty
             || new Ping().Send(ip,
                                10000,
                                Encoding.ASCII.GetBytes(new string('a', 32)),
                                new PingOptions(64, true)).Status != IPStatus.Success)
         {
            Log.Write("Invalid IP address.");
            return false;
         }
         var pFormatContext = ffmpeg.avformat_alloc_context();

         try
         {


            using (var cts = new CancellationTokenSource())
            {
               cts.CancelAfter(5000);

               var task = Task.Run(() =>
               {
                  var pFormatContextLocal = pFormatContext;
                  var result = ffmpeg.avformat_open_input(&pFormatContextLocal, url, null, null);
                  if (result < 0)
                  {
                     Log.Write($"Failed to open RTSP stream: {FFmpegHelper.GetErrorMessage(result)}");
                     return;
                  }

                  result = ffmpeg.avformat_find_stream_info(pFormatContextLocal, null);
                  if (result < 0)
                  {
                     Log.Write($"Failed to retrieve stream info: {FFmpegHelper.GetErrorMessage(result)}");
                     return;
                  }

                  Log.Write("RTSP stream is valid.");
                  retval = true;
               });

               task.Wait(cts.Token);
            }
         }
         catch (OperationCanceledException)
         {
            Log.Write("RTSP Timed out or Port is invalid.");
         }
         finally
         {
            var pFormatContextLocal = pFormatContext;
            ffmpeg.avformat_close_input(&pFormatContextLocal);
         }
         return retval;
      }

   }
}
