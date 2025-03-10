namespace RTSPCamLib
{
   public class CamHandler
   {
      public CamHandler()
      {
         FfmpegHandler.Initialize();
         Console.WriteLine("CamHandler initialized");
      }
   }
}
