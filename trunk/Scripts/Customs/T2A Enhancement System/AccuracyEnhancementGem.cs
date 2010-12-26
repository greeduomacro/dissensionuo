using System;
using System.Collections;
using System.Collections.Generic;
using Server.Multis;
using Server.Mobiles;
using Server.Network;
using Server.ContextMenus;
using Server.Spells;
using Server.Targeting;
using Server.Misc;
using Server.Items;


namespace Server.Items
{
	public class AccuracyEnhancementGem : Item
	{
		[Constructable]
		public AccuracyEnhancementGem() : this( 1 )
		{
		}

		[Constructable]
		public AccuracyEnhancementGem( int amount ) : base( 0xF13 )
		{
			Weight = 1.0;
			Stackable = false;
			Name = "a weapon accuracy enhancement gem";
			Amount = amount;
			Hue = 1266;
		}
		
		public override void OnDoubleClick( Mobile from ) 
		{
			PlayerMobile pm = from as PlayerMobile;
		
			if ( !IsChildOf( from.Backpack ) )
			{
				from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
			}

		        else if( from.InRange( this.GetWorldLocation(), 1 ) ) 
		        {
				
					from.SendMessage( "Select the item to enhance." );
					from.Target = new InternalTarget( this );
				
		        } 

		        else 
		        { 
		        	from.SendLocalizedMessage( 500446 ); // That is too far away. 
		        } 
		} 
		
		private class InternalTarget : Target 
		{
			private AccuracyEnhancementGem m_AccuracyEnhancementGem;

			public InternalTarget( AccuracyEnhancementGem runeaug ) : base( 1, false, TargetFlags.None )
			{
				m_AccuracyEnhancementGem = runeaug;
			}

		 	protected override void OnTarget( Mobile from, object targeted ) 
		 	{ 
				
			    	if ( targeted is BaseWeapon ) 
					{ 
			       		BaseWeapon Weapon = targeted as BaseWeapon; 

						if ( !from.InRange( ((Item)targeted).GetWorldLocation(), 1 ) ) 
						{ 
			          		from.SendLocalizedMessage( 500446 ); // That is too far away. 
		       			}

						else if (( ((Item)targeted).Parent != null ) && ( ((Item)targeted).Parent is Mobile ) ) 
			       		{ 
			          		from.SendMessage( "You cannot enhance that in it's current location." ); 
		       			}

						else
		       			{
							int DestroyChance = Utility.Random( 3 );

							if ( DestroyChance > 0 ) // Success
							{
								if ( Weapon.AccuracyLevel == WeaponAccuracyLevel.Regular )
								{
									Weapon.AccuracyLevel = WeaponAccuracyLevel.Accurate;
									from.PlaySound( 0x1F5 );
									from.SendMessage( "The accuracy of your weapon has been enhanced." );
									m_AccuracyEnhancementGem.Delete();
									return;
								}
							
								if ( Weapon.AccuracyLevel == WeaponAccuracyLevel.Accurate )
								{
									Weapon.AccuracyLevel = WeaponAccuracyLevel.Surpassingly;
									from.PlaySound( 0x1F5 );
									from.SendMessage( "The accuracy of your weapon has been enhanced." );
									m_AccuracyEnhancementGem.Delete();
									return;
								}
							
								if ( Weapon.AccuracyLevel == WeaponAccuracyLevel.Surpassingly )
								{
									Weapon.AccuracyLevel = WeaponAccuracyLevel.Eminently;
									from.PlaySound( 0x1F5 );
									from.SendMessage( "The accuracy of your weapon has been enhanced." );
									m_AccuracyEnhancementGem.Delete();
									return;
								}
							
								if ( Weapon.AccuracyLevel == WeaponAccuracyLevel.Eminently )
								{
									Weapon.AccuracyLevel = WeaponAccuracyLevel.Exceedingly;
									from.PlaySound( 0x1F5 );
									from.SendMessage( "The accuracy of your weapon has been enhanced." );
									m_AccuracyEnhancementGem.Delete();
									return;
								}
							
								if ( Weapon.AccuracyLevel == WeaponAccuracyLevel.Exceedingly )
								{
									Weapon.AccuracyLevel = WeaponAccuracyLevel.Supremely;
									from.PlaySound( 0x1F5 );
									from.SendMessage( "The accuracy of your weapon has been enhanced." );
									m_AccuracyEnhancementGem.Delete();
									return;
								}
							
								if ( Weapon.AccuracyLevel == WeaponAccuracyLevel.Supremely )
								{												 
									from.SendMessage( "This weapon is already at full accuracy." );
									return;
								}
							}	

							else // Fail
							{
								from.SendMessage( "You have failed to enhance the weapon!" );
								from.SendMessage( "The weapon is damaged beyond repair!" );
								from.PlaySound( 42 );
								Weapon.Delete();
								m_AccuracyEnhancementGem.Delete();
							}
					
						}
					}
					
					else 
					{
		       			from.SendMessage( "You cannot enhance that." );
		    		} 
					
		  	}
		
		}

		public AccuracyEnhancementGem( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}
}