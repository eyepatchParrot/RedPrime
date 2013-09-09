function createPlayerCharacter()
{
	%pc = new Sprite(PlayerCharacter);
	%pc.setBodyType( dynamic );
	%pc.Position = "0 0";
	%pc.Size = "0.7 1.0";
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
	%moveAnim.idleAnimation = "FireAndIce:redIdleAnim";
	%moveAnim.walkAnimation = "FireAndIce:redWalkAnim";
	%pc.addBehavior(%moveAnim);
	%pc.moveAnimBehavior = %moveAnim;
	
	%pc.setShootAnim( false );
	
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
			%this.shootOneBullet( 0.0 );
			if (!%this.isWeaponBoosted)
			{
				%reloadTime = %this.shotFreq;
				alxPlay("FireAndIce:ShotSound");
			}
			else
			{
				%this.schedule(32, shootOneBullet, 10.0);
				%this.schedule(64, shootOneBullet, -10.0);
				%reloadTime = %this.shotFreq;
				alxPlay("FireAndIce:shotgunSound");
			}
			
			%this.schedule(%reloadTime, reload);
			%this.canShoot = false;
		}
		%this.schedule(100, shoot);
	}
}

function PlayerCharacter::shootOneBullet( %this, %angleOffset )
{
	%angle = Vector2AngleToPoint(%this.Position, %this.targetPosition);
	createBulletAt(%this.Position, %angle + %angleOffset);
	%this.setShootAnim( true );
	cancel( %this.shootAnimSchedule );
	%this.shootAnimSchedule = %this.schedule( 32, setShootAnim, false );
	%this.rotateTo(%angle + 180, %this.turnSpeed);
}

function PlayerCharacter::setShootAnim( %this, %on )
{
	if ( %on )
	{
		%idleAnim = "FireAndIce:redIdleShootAnim";
		%walkAnim = "FireAndIce:redWalkShootAnim";
	}
	else
	{
		%idleAnim = "FireAndIce:redIdleAnim";
		%walkAnim = "FireAndIce:redWalkAnim";
	}
	
	%this.moveAnimBehavior.idleAnimation = %idleAnim;
	%this.moveAnimBehavior.walkAnimation = %walkAnim;
	%this.moveAnimBehavior.updateAnimation();
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
	%deathAnimation.setSize( 1 SPC 1.2 );
	%deathAnimation.setImage( "FireAndIce:redDead" );
	%deathAnimation.setBodyType( static );
	%deathAnimation.setCollisionSuppress( true );
	%deathAnimation.setSceneLayer( 3 );
	mainScene.add( %deathAnimation );
	%this.schedule(32, "safeDelete" );
	FireAndIce.schedule(2000, "startLoseMenu");
	alxPlay("ToyAssets:KnightDeathSound");
}