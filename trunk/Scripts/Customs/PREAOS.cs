using System;
using System.Collections;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Network;
//Copyright by ROMANTHEBRAIN
namespace Server
{
    public class PREAOS
    {
        private const bool Enabled = true;

        public static void Configure()
        {
            SupportedFeatures.Value = 0x805b;// Aos/SE Graphics 
            Mobile.VisibleDamageType = Enabled ? VisibleDamageType.Related : VisibleDamageType.None;// Dmg Counter
        }
    }
}