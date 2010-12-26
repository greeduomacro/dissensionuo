using Server.Commands;
using System;
using Server;
using Server.Network;
using Server.Targeting;
using Server.Mobiles;
using Server.Items;

namespace Server.Commands
{
    public class TameCommand
    {
        public static void Initialize()
        {
            CommandSystem.Register("Tame", AccessLevel.GameMaster, new CommandEventHandler(Tame_OnCommand));
        }


        [Usage("Tame <text>")]
        [Description("Tame's the selected animal.")]
        public static void Tame_OnCommand(CommandEventArgs e)
        {
            Mobile from = e.Mobile;
            from.Target = new TameTarget();
            from.SendMessage("What creature do you wish to tame?");
        }


        private class TameTarget : Target
        {
            public TameTarget(): base(15, false, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                PlayerMobile pm = (PlayerMobile)from;
                if (targeted is BaseCreature)
                {

                    BaseCreature Tamata = (BaseCreature)targeted;
                    Tamata.Controlled = true;
                    Tamata.ControlMaster = from;
                }

                else

                    from.SendMessage("You can only tame a creature!");
            }
        }
    }
}
    
