using System.Net;
using System.Net.NetworkInformation;
using System.Text;

using FFmpeg.AutoGen;

namespace RTSPCamLib
{
   public class CamValidator
   {

      unsafe public static bool IsValidRtspStream(CamInfo camInfo)
      {
         var retval = false;

         if (camInfo.Ip == IPAddress.None
             || new Ping().Send(camInfo.Ip,
                                10000,
                                Encoding.ASCII.GetBytes(new string('a', 32)),
                                new PingOptions(64, true)).Status != IPStatus.Success)
         {
            Log.Write("Invalid IP address.");
            return false;
         }

         try
         {

            using (var cts = new CancellationTokenSource())
            {
               cts.CancelAfter(5000);

               var task = new Task<bool>(() => CheckRtspStreamAlive(camInfo));

               task.Start();
               task.Wait(cts.Token);

               retval = task.Result;
            }
         }
         catch (OperationCanceledException)
         {
            Log.Write("RTSP Timed out or Port is invalid.");
         }

         return retval;
      }

      unsafe private static bool CheckRtspStreamAlive(CamInfo camInfo)
      {
         var pFormatContext = ffmpeg.avformat_alloc_context();

         var retval = true;
         try
         {

            var result = ffmpeg.avformat_open_input(&pFormatContext, camInfo.ConnectionString, null, null);
            if (result < 0)
            {
               Log.Write($"Failed to open RTSP stream: {GetErrorMessage(result)}");
               retval = false;
            }

            result = ffmpeg.avformat_find_stream_info(pFormatContext, null);
            if (result < 0)
            {
               Log.Write($"Failed to retrieve stream info: {GetErrorMessage(result)}");
               retval = false;
            }

            Log.Write("RTSP stream is valid.");
         }
         catch (OperationCanceledException)
         {
            retval = false;
         }
         catch
         {
            retval = false;
            throw;
         }
         finally
         {
            if (pFormatContext != null)
            {
               ffmpeg.avformat_close_input(&pFormatContext);
            }
         }
         return retval;
      }

      unsafe public static void GetStreamInfo(CamInfo camInfo)
      {
         var pFormatContext = ffmpeg.avformat_alloc_context();

         try
         {
            if (ffmpeg.avformat_open_input(&pFormatContext, camInfo.ConnectionString, null, null) != 0)
            {
               Console.WriteLine("Failed to open RTSP stream.");
               return;
            }

            if (ffmpeg.avformat_find_stream_info(pFormatContext, null) < 0)
            {
               Console.WriteLine("Failed to retrieve stream info.");
               return;
            }
            Console.WriteLine($"RTSP Stream Info for: {camInfo.ConnectionString}");
            Console.WriteLine($"Number of streams: {pFormatContext->nb_streams}");

            for (var i = 0 ; i < pFormatContext->nb_streams ; i++)
            {
               var stream = pFormatContext->streams[i];
               var codecParams = stream->codecpar;
               var codec = ffmpeg.avcodec_find_decoder(codecParams->codec_id);

               Console.WriteLine($"\nStream {i + 1}:");
               Console.WriteLine($"Codec: {ffmpeg.avcodec_get_name(codecParams->codec_id)}");
               Console.WriteLine($"Codec Type: {(codecParams->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO ? "Video" : "Audio")}");

               if (codecParams->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO)
               {
                  Console.WriteLine($"Resolution: {codecParams->width}x{codecParams->height}");
                  Console.WriteLine($"FPS: {stream->r_frame_rate.num}/{stream->avg_frame_rate.den}");
               }

               if (codecParams->codec_type == AVMediaType.AVMEDIA_TYPE_AUDIO)
               {
                  Console.WriteLine($"Sample Rate: {codecParams->sample_rate}");
                  Console.WriteLine($"Channels: {codecParams->channels}");
               }
            }
         }
         finally
         {
            ffmpeg.avformat_close_input(&pFormatContext);
         }
      }

      unsafe private static string GetErrorMessage(int errorCode)
      {
         var buffer = new byte[1024];
         fixed (byte* pBuffer = buffer)
         {
            _ = ffmpeg.av_strerror(errorCode, pBuffer, (ulong) buffer.Length);
         }
         return Encoding.UTF8.GetString(buffer).TrimEnd('\0');
      }
   }
}
