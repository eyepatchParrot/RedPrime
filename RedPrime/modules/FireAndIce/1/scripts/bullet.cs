function createBulletAt(%position, %angle)
{
	%bullet = new Sprite()
	{
		class = "Bullet";
	};
	%bullet.setBodyType( dynamic );
	%bullet.Position = %position;
	%bullet.Size = "0.5 0.5";
	%bullet.SceneLayer = 3;
	%bullet.SceneGroup = $Game::BulletDomain;
	%bullet.createCircleCollisionShape(0.1);
	%bullet.setFixedAngle( true );
	%bullet.setLinearVelocityPolar(%angle, 10.0);
	%bullet.setAngle( %angle + 180 );
	%bullet.playAnimation( "ToyAssets:Cannonball_Projectile_1Animation" );
	%bullet.setCollisionGroups( $Game::ThugDomain, $Game::BoundaryDomain );
	%bullet.setCollisionCallback();
	
	mainScene.add( %bullet );
	
	%bullet.setDefaultDensity(1.0, true);
}

function Bullet::onCollision(%this, %sceneObject, %collisionDetails)
{
//	if ( !%sceneObject.getCollisionSuppress() == true )
//	{
		%this.safeDelete();
//	}
}