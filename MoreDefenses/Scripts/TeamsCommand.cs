using Jotunn.Entities;
using MoreDefenses.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreDefenses.Scripts
{
    public class TeamsCommand : ConsoleCommand
    {
        public override string Name => "team";

        public override string Help => "Adds you to a team";

        public override bool IsNetwork => true;


        public override void Run(string[] args)
        {
            int team = int.Parse(args[0]);
            long player = Player.m_localPlayer.GetPlayerID();
            TeamConfigManager.SetTeam(player, team);
            foreach (var p in Player.GetAllPlayers())
            {
                TeamConfigManager.SetTeam(p.GetPlayerID(), 2);
            }
            TeamConfigManager.WriteTeamsToJson();
        }
    }
}
