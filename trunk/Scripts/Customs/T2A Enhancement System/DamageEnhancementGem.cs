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
	public class DamageEnhancementGem : Item
	{
		[Constructable]
		public DamageEnhancementGem() : this( 1 )
		{
		}

		[Constructable]
		public DamageEnhancementGem( int amount ) : base( 0xF13 )
		{
			Weight = 1.0;
			Stackable = false;
			Name = "a weapon damage enhancement gem";
			Amount = amount;
			Hue = 1764;
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
			private DamageEnhancementGem m_DamageEnhancementGem;

			public InternalTarget( DamageEnhancementGem runeaug ) : base( 1, false, TargetFlags.None )
			{
				m_DamageEnhancementGem = runeaug;
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
								if ( Weapon.DamageLevel == WeaponDamageLevel.Regular )
								{
									Weapon.DamageLevel = WeaponDamageLevel.Ruin;
									from.PlaySound( 0x1F5 );
									from.SendMessage( "The damage level of your weapon has been enhanced." );
									m_DamageEnhancementGem.Delete();
									return;
								}
							
								if ( Weapon.DamageLevel == WeaponDamageLevel.Ruin )
								{
									Weapon.DamageLevel = WeaponDamageLevel.Might;
									from.PlaySound( 0x1F5 );
									from.SendMessage( "The damage level of your weapon has been enhanced." );
									m_DamageEnhancementGem.Delete();
									return;
								}
							
								if ( Weapon.DamageLevel == WeaponDamageLevel.Might )
								{
									Weapon.DamageLevel = WeaponDamageLevel.Force;
									from.PlaySound( 0x1F5 );
									from.SendMessage( "The damage level of your weapon has been enhanced." );
									m_DamageEnhancementGem.Delete();
									return;
								}
							
								if ( Weapon.DamageLevel == WeaponDamageLevel.Force )
								{
									Weapon.DamageLevel = WeaponDamageLevel.Power;
									from.PlaySound( 0x1F5 );
									from.SendMessage( "The damage level of your weapon has been enhanced." );
									m_DamageEnhancementGem.Delete();
									return;
								}
							
								if ( Weapon.DamageLevel == WeaponDamageLevel.Power )
								{
									Weapon.DamageLevel = WeaponDamageLevel.Vanq;
									from.PlaySound( 0x1F5 );
									from.SendMessage( "The damage level of your weapon has been enhanced." );
									m_DamageEnhancementGem.Delete();
									return;
								}
							
								if ( Weapon.DamageLevel == WeaponDamageLevel.Vanq )
								{												 
									from.SendMessage( "This weapon is already at full damage level." );
									return;
								}
							}	

							else // Fail
							{
								from.SendMessage( "You have failed to enhance the weapon!" );
								from.SendMessage( "The weapon is damaged beyond repair!" );
								from.PlaySound( 42 );
								Weapon.Delete();
								m_DamageEnhancementGem.Delete();
							}
					
						}
					}
					
					else 
					{
		       			from.SendMessage( "You cannot enhance that." );
		    		} 
					
		  	}
		
		}

		public DamageEnhancementGem( Serial serial ) : base( serial )
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