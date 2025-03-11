namespace RTSPCamLib
{
   public class FFmpegHelper
   {
      public static string GetErrorMessage(int errorCode)
      {
         return string.Empty;
         /*byte[] buffer = new byte[1024];
         ffmpeg.av_strerror(errorCode, buffer, (ulong) buffer.Length);
         return Encoding.UTF8.GetString(buffer).TrimEnd('\0');*/
      }
   }
}
