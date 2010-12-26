using System;
using Server.Misc;
using Server.Network;
using Server.Prompts;

namespace Server.Items
{
    public class NameChangeDeed : Item
    {
        [Constructable]
        public NameChangeDeed()
            : base(0x14F0)
        {
            Weight = 1.0;
            Name = "name change deed";
            LootType = LootType.Blessed;
        }

        public NameChangeDeed(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
            }
            else
            {
                from.SendMessage("Enter your desired name.");
                from.Prompt = new RenamePrompt(this);
            }
        }

        private class RenamePrompt : Prompt
        {
            private NameChangeDeed m_Deed;

            public RenamePrompt(NameChangeDeed deed)
            {
                m_Deed = deed;
            }

            public override void OnResponse(Mobile from, string text)
            {
                text = text.Trim();
                if (!NameVerification.Validate(text, 2, 16, true, true, true, 1, NameVerification.SpaceDashPeriodQuote))
                    return;

                from.Name = text;
                from.SendMessage("You will be hence forth know as {0}", text);
                m_Deed.Delete();
            }
        }
    }
}


