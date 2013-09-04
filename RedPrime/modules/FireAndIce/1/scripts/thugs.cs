function createThug(%position)
{
	%s = new Sprite()
	{
		class = "Thug";
	};
	%s.setBodyType( dynamic );
	%s.setPosition( %position );
	%s.Size = "1.5 1.5";
	%s.SceneLayer = 2;
	%s.SceneGroup = $Game::ThugDomain;
	%s.Image = "FireAndIce:enemy";
	%halfHeight = %s.getHeight() / 2.0;
	%s.createPolygonBoxCollisionShape(%s.getWidth() SPC %s.getHeight());
	%s.setCollisionGroups( $Game::PlayerDomain, $Game::BulletDomain, $Game::ThugDomain );
	%s.setFixedAngle( true );
	
	%s.setCollisionCallback( true );
	%s.hp = 1.0;
	
	%moveAi = MoveTowardBehavior.createInstance();
	%moveAi.targetObject = PlayerCharacter;
	%moveAi.startPosition = %position;
	%moveAi.horizontalSpeed = 0.9;
	%moveAi.verticalSpeed = 0.9;
	%s.addBehavior(%moveAi);
	
	/*%imageUpdate = UpdateImageBehavior.createInstance();
	%imageUpdate.notMovingImage = "ToyAssets:TD_Barbarian_CompSprite";
	%imageUpdate.moveNorthAnimation = "ToyAssets:TD_Barbarian_North";
	%imageUpdate.moveSouthAnimation = "ToyAssets:TD_Barbarian_WalkSouth";
	%imageUpdate.moveWestAnimation  = "ToyAssets:TD_Barbarian_WalkWest";
	%s.addBehavior(%imageUpdate);*/
	
	%dropPickup = DropPickupBehavior.createInstance();
	%dropPickup.pickupClass = "weaponBoost";
	%s.addBehavior(%dropPickup);
	
	mainScene.add( %s );
	
	%s.setDefaultDensity(10.0, true);
}

function Thug::onCollision(%this, %sceneObject, %collisionDetails)
{
	%group = %sceneObject.getSceneGroup();
	if (%group == $Game::PlayerDomain || %group == $Game::BulletDomain)
		%this.takeDamage();
}

function Thug::takeDamage( %this )
{
	%this.hp -= 1.0;
	if ( %this.isDead() )
		%this.die();
}

function Thug::isDead( %this )
{
	return %this.hp <= 0;
}

function Thug::die( %this )
{
	%deathAnimation = new Sprite();
	%deathAnimation.setLifeTime(2.0);
	%deathAnimation.setPosition(%this.Position);
	%deathAnimation.setSize(%this.Size);
	%deathAnimation.playAnimation("ToyAssets:TD_Barbarian_Death");
	%deathAnimation.setBodyType( static );
	%deathAnimation.setCollisionSuppress( true );
	%deathAnimation.setSceneLayer(4);
	mainScene.add( %deathAnimation );
	%this.onDeath();
	%this.schedule(32, "safeDelete");
}