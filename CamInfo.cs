using System.Net;

namespace RTSPCamLib
{
   public class CamInfo
   {
      #region Public Properties
      public IPAddress Ip { get; } = IPAddress.None;
      public string Paramaters { get; }
      public string Username { get; }
      public string Password { get; }
      public string ConnectionString => @$"rtsp://{(Username.Equals(string.Empty) ? string.Empty : string.IsNullOrEmpty(Password) ? $"{Username}@" : $"{Username}:{Password}@")}{Ip}{Paramaters}";
      #endregion

      #region Constructors
      public CamInfo(IPAddress ip, string paramaters, string username, string password)
      {
         Ip = ip;
         Paramaters = paramaters;
         Username = username;
         Password = password;
      }

      public CamInfo(string ip, string paramaters, string username, string password)
      {
         if (IPAddress.TryParse(ip, out var ipAddress))
         {
            Ip = ipAddress;
            Paramaters = paramaters;
            Username = username;
            Password = password;
         }
         else
         {
            throw new ArgumentException($"Invalid Format for IP address : {ip}");
         }
      }
      #endregion
   }
}

