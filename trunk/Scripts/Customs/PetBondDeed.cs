using System;
using Server.Network;
using Server.Prompts;
using Server.Items;
using System.Collections;
using Server.Gumps;
using Server.Targeting;
using Server.Misc;
using Server.Accounting;
using System.Xml;
using Server.Mobiles; 

namespace Server.Items
{
	public class PetBondDeed : Item
	{
		[Constructable]
		public PetBondDeed() : base( 0x14F0 )
		{
			base.Weight = 0;
			base.LootType = LootType.Blessed;
			base.Name = "a pet bond deed";
		}		

		public override void OnDoubleClick( Mobile from )
		{
			if ( IsChildOf( from.Backpack ) )
			{
				from.Target = new InternalTarget(from, this);
			}
			else
			{
				from.SendMessage("The deed must be in your backpack to use it.");
			}
		}
		
		public PetBondDeed( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
		
	}
	
		public class InternalTarget : Target
		{
			private Mobile m_From;
			private PetBondDeed m_Deed;
			
			public InternalTarget( Mobile from, PetBondDeed deed ) :  base ( 3, false, TargetFlags.None )
			{
				m_Deed = deed;
				m_From = from;
				from.SendMessage("Select the animal you wish to bond.");
		
				
			}
			
			protected override void OnTarget( Mobile from, object targeted )
			{
				
				if (m_Deed.IsChildOf( m_From.Backpack ) )
				{					
					if ( targeted is Mobile )
					{
						if ( targeted is BaseCreature )
						{
							BaseCreature creature = (BaseCreature)targeted;
							if( !creature.Tamable ){
								from.SendMessage("This creature is not tamed.");
							}
							else if(  !creature.Controlled || creature.ControlMaster != from ){
								from.SendMessage("That's not your pet!");
							}
							else if( creature.IsDeadPet ){
								from.SendMessage("This animal is dead.");
							}
							else if ( creature.Summoned ){
								from.SendMessage("You cannot bond summoned creatures.");
							}
							else if ( creature.Body.IsHuman ){
								from.SendMessage("You can't bond a human!");
							}
							else{	
								
								if( creature.IsBonded == true ){
									from.SendMessage("This animal is already bonded.");
								}
								else{
										try{
											creature.IsBonded = true;
											from.SendMessage("You have successfully bonded",creature.Name);
											m_Deed.Delete();
										}
										catch{
											from.SendMessage("There is a problem, please contact a staff member.");
										}
											
									}
								}
														
						}
						else{
							from.SendMessage("You can only bond animals.");
						}
					}
					else{
							from.SendMessage("You can only bond animals.");
						}
				}
				else{
					from.SendMessage("The deed must be in your bag to use it.");
				}			
		}
	}
}
