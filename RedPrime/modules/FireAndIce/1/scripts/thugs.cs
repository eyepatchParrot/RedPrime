function createThug(%position)
{
	%s = new Sprite()
	{
		class = "Thug";
	};
	%s.setBodyType( dynamic );
	%s.setPosition( %position );
	%s.Size = "0.9 0.9";
	%s.SceneLayer = 2;
	%s.SceneGroup = $Game::ThugDomain;
	%s.playAnimation( "FireAndIce:monsterAnim" );
	%s.createCircleCollisionShape(0.3);
	%s.setCollisionGroups( $Game::PlayerDomain, $Game::BulletDomain, $Game::ThugDomain );
	%s.setFixedAngle( true );
	
	%s.setCollisionCallback( true );
	%s.hp = 1.0;
	
	%moveAi = MoveTowardBehavior.createInstance();
	%moveAi.targetObject = PlayerCharacter;
	%moveAi.startPosition = %position;
	%moveAi.moveSpeed = 2.0;
	%s.addBehavior(%moveAi);
	
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
	%deathAnimation.playAnimation("FireAndIce:monsterDeathAnim");
	%deathAnimation.setBodyType( static );
	%deathAnimation.setCollisionSuppress( true );
	%deathAnimation.setSceneLayer(4);
	%deathAnimation.setAngle(%this.getAngle());
	mainScene.add( %deathAnimation );
	%this.onDeath();
	%this.schedule(32, "safeDelete");
	%selectedSound = getRandom(2);
	switch ( %selectedSound )
	{
		case 0:
		alxPlay("FireAndIce:monsterDie1Sound");
		
		case 1:
		alxPlay("FireAndIce:monsterDie2Sound");
		
		case 2:
		alxPlay("FireAndIce:monsterDie3Sound");
	}
	$Game::Kills++;
}