namespace RTSPCamLib
{
   public static class Log
   {
      public static EventHandler<string> OnWriten;
      public static void Write(string message)
      {
         OnWriten?.Invoke(null, message);
      }
   }
}
