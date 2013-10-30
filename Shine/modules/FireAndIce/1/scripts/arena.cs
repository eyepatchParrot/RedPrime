function createArena()
{
	createBackground();
	createBoundaries();

	%fg = new Sprite();
	%fg.setPosition(0 SPC 0);
	%fg.setSize($Game::ArenaWidth SPC $Game::ArenaHeight);
	%fg.setSceneLayer(0);
	%fg.setImage("FireAndIce:trees");
	mainScene.add( %fg );
}

function createBackground()
{
	// %bgRightX = $Game::ScreenWidth / 2.0;
	// %bgLeftX = %bgRightX - $Game::ScreenWidth;
	// %bgTopY = $Game::ScreenHeight / 2.0;
	// %bgLowY = %bgTopY - $Game::ScreenHeight;
	
	// createOneBackground( %bgLeftX SPC %bgLowY, "FireAndIce:map" );
	// createOneBackground( %bgLeftX SPC %bgTopY, "FireAndIce:map" );
	// createOneBackground( %bgRightX SPC %bgLowY, "FireAndIce:map" );
	// createOneBackground( %bgRightX SPC %bgTopY, "FireAndIce:map" );
	createOneBackground( 0 SPC 0, $Game::ArenaWidth SPC $Game::ArenaHeight, "FireAndIce:map" );
}

function createOneBackground( %pos, %size, %img)
{
	%bg = new Sprite();
	%bg.setPosition(%pos);
	%bg.setSize(%size);
	%bg.setSceneLayer(5);
	%bg.setImage( %img );
	mainScene.add( %bg );
}

function createBoundaries()
{
	%boundarySize = 5;
	%halfArenaWidth = $Game::ArenaWidth / 2.0;
	%halfArenaHeight = $Game::ArenaHeight / 2.0;
	%halfBoundarySize = %boundarySize / 2.0;
	%boundaryX = %halfArenaWidth + %halfBoundarySize;
	%boundaryY = %halfArenaHeight + %halfBoundarySize;
	
	%rightBoundaryPosition = %boundaryX SPC 0;
	%rightBoundarySize = %boundarySize SPC $Game::ArenaHeight;
	
	%topBoundaryPosition = 0 SPC %boundaryY;
	%topBoundarySize = $Game::ArenaWidth SPC %boundarySize;
	
	%leftBoundaryPosition = -%boundaryX SPC 0;
	%leftBoundarySize = %rightBoundarySize;
	
	%bottomBoundaryPosition = 0 SPC -%boundaryY;
	%bottomBoundarySize = %topBoundarySize;
	
	mainScene.add( createOneBoundary(%leftBoundaryPosition, %leftBoundarySize) );
	mainScene.add( createOneBoundary(%rightBoundaryPosition, %rightBoundarySize) );
	mainScene.add( createOneBoundary(%topBoundaryPosition, %topBoundarySize) );
	mainScene.add( createOneBoundary(%bottomBoundaryPosition, %bottomBoundarySize) );
}

function createOneBoundary(%position, %size)
{
	%boundary = new SceneObject();
	%boundary.setSize( %size );
	%boundary.setPosition( %position );
	%boundary.createPolygonBoxCollisionShape( %size );
	%boundary.setSceneGroup( $Game::BoundaryDomain );
	%boundary.setCollisionGroups( none );
	%boundary.setCollisionCallback( false );
	%boundary.setBodyType( "static" );
	// %boundary.setCollisionShapeIsSensor(0, true);
	return %boundary;
}