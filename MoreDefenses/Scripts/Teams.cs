using System;
using System.CodeDom;
using System.Collections.Generic;

namespace MoreDefenses.Scripts
{
    internal class Teams
    {
        static readonly Dictionary<string, int> playerToTeam = new Dictionary<string, int>
        {
            { "torkynight", 1 },
            { "torky***ht", 1 },
            { "tutorialtorky", 1 },
            { "frostyy", 1 },
            { "frosty test", 1 },
            { "joking", 1 },
            { "daboss", 1 },
            { "dabeast", 2 },
            { "notsean", 2 },
            { "batou", 2 },
        };
        public static bool IsSameTeam(string player1, string player2)
        {
            var isSameTeam = GetPlayerTeam(player1) == GetPlayerTeam(player2);
            // Jotunn.Logger.LogInfo(isSameTeam + "=>" + player1 + ":" + GetPlayerTeam(player1) + "-" + player2 + ":" + GetPlayerTeam(player2));
            return isSameTeam;
        }

        public static int GetPlayerTeam(string player)
        {
            string playerLower = player.ToLower();
            if (playerToTeam.ContainsKey(playerLower))
            {
                return playerToTeam[playerLower];
            }
            return 0;
        }
    }
}
