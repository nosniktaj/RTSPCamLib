namespace RTSPCamLib
{
   public class CamHandler
   {
      public CamHandler()
      {
         FfmpegHandler.Initialize();
         Log.Write("CamHandler initialized");
         Log.Write($"{CamValidator.IsValidRtspStream("rtsp://admin:Pugwash1!@10.0.0.21:554/live/1/1")}");
      }
   }
}
