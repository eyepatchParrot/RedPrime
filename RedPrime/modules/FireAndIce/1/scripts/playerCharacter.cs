function createPlayerCharacter()
{
	%pc = new Sprite(PlayerCharacter);
	%pc.setBodyType( dynamic );
	%pc.Position = "0 0";
	%pc.Size = "0.9 0.9";
	%pc.SceneLayer = 1;
	%pc.SceneGroup = $Game::PlayerDomain;
	// %pc.Image = "FireAndIce:Soldier";
	%pc.createCircleCollisionShape(0.3);
	%pc.setCollisionGroups( $Game::ThugDomain, $Game::BoundaryDomain, $Game::PickupDomain );
	%pc.setFixedAngle( true );
	
	%pc.setCollisionCallback( true );
	%pc.hp = 1.0;
	
	%controls = ShooterControlsBehavior.createInstance();
	%controls.verticalSpeed = 2.5;
	%controls.horizontalSpeed = 2.5;
	%controls.upKey = "keyboard w";
	%controls.downkey = "keyboard s";
	%controls.leftKey = "keyboard a";
	%controls.rightKey = "keyboard d";
	%pc.addBehavior(%controls);
	
	%moveAnim = MoveAnimationBehavior.createInstance();
	%moveAnim.idleImage = "FireAndIce:redWalk";
	%moveAnim.walkAnimation = "FireAndIce:redWalkAnim";
	%pc.addBehavior(%moveAnim);
	
	%pc.targetPosition = "0 0";
	%pc.shotFreq = 500;
	%pc.canShoot = true;
	%pc.turnSpeed = 500.0;
	
	%pc.setWeaponBoosted( false );
	
	mainScene.add( %pc );
}

function PlayerCharacter::shoot(%this)
{
	if ( %this.isShooting && mainScene.getScenePause() == false)
	{
		if ( %this.canShoot)
		{
			%angle = Vector2AngleToPoint(%this.Position, %this.targetPosition);
			createBulletAt(%this.Position, %angle);
			if (!%this.isWeaponBoosted)
			{
				%reloadTime = %this.shotFreq;
				alxPlay("FireAndIce:ShotSound");
			}
			else
			{
				schedule(80, 0, createBulletAt, %this.Position, %angle + 10.0);
				schedule(160, 0, createBulletAt, %this.Position, %angle - 10.0);
				%reloadTime = %this.shotFreq;
				alxPlay("FireAndIce:SuperShotSound");
			}
			
			%this.schedule(%reloadTime, reload);
			%this.canShoot = false;
			%this.rotateTo(%angle + 180, %this.turnSpeed);
			
		}
		%this.schedule(100, shoot);
	}
}

function PlayerCharacter::reload( %this )
{
	%this.canShoot = true;
}

function PlayerCharacter::onCollision(%this, %sceneObject, %collisionDetails)
{
	switch (%sceneObject.getSceneGroup() )
	{
	case $Game::ThugDomain:
		%this.takeDamage();
	
	case $Game::PickupDomain:
		%this.pickup(%sceneObject);
	}
}

function PlayerCharacter::takeDamage( %this )
{
	%this.hp -= 1.0;
	updateHud();
	if ( %this.isDead() )
	{
		%this.die();
	}
}

function PlayerCharacter::pickup( %this, %pickup)
{
	switch$ (%pickup.class)
	{
		case "weaponBoost":
		%this.setWeaponBoosted( true );
		cancel(%this.disableBoostSchedule);
		%this.disableBoostSchedule = %this.schedule(5000, setWeaponBoosted, false );
		alxPlay("ToyAssets:TowerUpgradeSound");
	}
	%pickup.safeDelete();
}

function PlayerCharacter::setWeaponBoosted( %this, %val )
{
	%this.isWeaponBoosted = %val;
}

function PlayerCharacter::isDead( %this )
{
	return %this.hp <= 0.0;
}

function PlayerCharacter::die( %this )
{
	%deathAnimation = new Sprite();
	%deathAnimation.setLifetime(2.0);
	%deathAnimation.setPosition( %this.getPosition() );
	%deathAnimation.setSize( %this.getSize() );
	%deathAnimation.setImage( "FireAndIce:redDead" );
	%deathAnimation.setBodyType( static );
	%deathAnimation.setCollisionSuppress( true );
	%deathAnimation.setSceneLayer( 3 );
	mainScene.add( %deathAnimation );
	%this.schedule(32, "safeDelete" );
	FireAndIce.schedule(2000, "startLoseMenu");
	alxPlay("ToyAssets:KnightDeathSound");
}