if ( !isObject(DropPickupBehavior) )
{
	%template = new BehaviorTemplate(DropPickupBehavior);
	
	%template.friendlyName = "Drop Pickup Behavior";
	%template.behaviorType = "Game";
	%template.description = "onDeath releases a pickup.";
	
	%template.addBehaviorField(pickupClass, "The type of pickup to drop.", string, null);
	%template.addBehaviorField(pickupImage, "The image for the pickup.", string, "FireAndIce:shotgunImage");
	%template.addBehaviorField(pickupImageFrame, "The frame for the image.", int, 0);
	%template.addBehaviorField(odds, "The odds of dropping a pickup.", float, 0.1);
}

function DropPickupBehavior::onBehaviorAdd(%this)
{
	// Insert instantiation behavior here.
}

function DropPickupBehavior::onBehaviorRemove(%this)
{
	// Insert deletion behavior here.
}

function DropPickupBehavior::onDeath( %this )
{
	if (getRandomF(0.0, 1.0) < %this.odds)
	{
		%this.spawnPickup();
	}
}

function DropPickupBehavior::spawnPickup( %this )
{
	%pickup = new Sprite();
	%pickup.setBodyType( dynamic );
	%pickup.setPosition( %this.owner.getPosition() );
	%pickup.setSize( 1.0 , 0.34 );
	%pickup.setSceneLayer( 3 );
	%pickup.createCircleCollisionShape(%pickup.getSizeX() / 2.0);
	%pickup.setCollisionGroups( none );
	%pickup.setSceneGroup( $Game::PickupDomain );
	%pickup.setLifetime( 5.0 );
	%pickup.class = %this.pickupClass;
	%pickup.setImage(%this.pickupImage, %this.pickupImageFrame);
	%pickup.setDefaultDensity(0.1, true);
	%pickup.setAngularVelocity(10.0);
	mainScene.add( %pickup );
}
		