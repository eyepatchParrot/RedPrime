function createSpawnZones()
{
	%zoneSize = $Game::ZoneSize;
	%halfZoneSize = %zoneSize / 2.0;
	%halfArenaWidth = $Game::ArenaWidth / 2.0;
	%halfArenaHeight = $Game::ArenaHeight / 2.0;
	%zoneX = %halfArenaWidth + %halfZoneSize;
	%zoneY = %halfArenaHeight + %halfZoneSize;
	
	%topZonePosition = 0 SPC %zoneY;
	%topZoneSize = $Game::ArenaWidth SPC %zoneSize;
	
	%bottomZonePosition = 0 SPC -%zoneY;
	%bottomZoneSize = %topZoneSize;
	
	%rightZonePosition = %zoneX SPC 0;
	%rightZoneSize = %zoneSize SPC $Game::ArenaHeight;
	
	%leftZonePosition = -%zoneX SPC 0;
	%leftZoneSize = %rightZoneSize;
	
	%spawnManager = new SceneObject(SpawnManager);
	%spawnManager.zones = new SimSet();
	%spawnManager.addSpawnZone( createSpawnZone( "top", %topZonePosition, %topZoneSize ) );
	%spawnManager.addSpawnZone( createSpawnZone( "bottom", %bottomZonePosition, %bottomZoneSize ) );
	%spawnManager.addSpawnZone( createSpawnZone( "right", %rightZonePosition, %rightZoneSize ) );
	%spawnManager.addSpawnZone( createSpawnZone( "left", %leftZonePosition, %leftZoneSize ) );
	%spawnManager.waveNum = 0;
	%spawnManager.spawnNextWave();
}

function SpawnManager::addSpawnZone(%this, %spawnZone)
{
	mainScene.add(%spawnZone);
	%this.zones.add(%spawnZone);
}

function SpawnManager::spawnNextWave(%this)
{
	%this.waveNum++;
	updateHud();
	
	%baseNumEnemies = 5;
	%baseSpawnTime = 1000;
	%difficultyInc = 0.1;
	%difficultyMul = 1 + %difficultyInc * (%this.waveNum - 1);
	%numEnemies = %baseNumEnemies * %difficultyMul;
	%spawnTime = %baseSpawnTime / %difficultyMul;
	%waveTime = %numEnemies * %spawnTime + 8000;
	%this.spawnNewWaveAtAllZones( %numEnemies, %spawnTime);
	%this.waveSpawn = %this.schedule( %waveTime, spawnNextWave );
}

function SpawnManager::spawnNewWaveAtAllZones( %this, %numEnemies, %freq )
{
	%this.zones.callOnChildren( "spawnNewWave", %numEnemies, %freq );
}

function createSpawnZone(%side, %position, %size)
{
	%s = new SceneObject()
	{
		class = "SpawnZone";
	};
	%s.setBodyType( static  );
	%s.Position = %position;
	%s.Size = %size;
	
	%s.numEnemiesToSpawn = 0;
	%s.side = %side;
	
	return %s;
}

function SpawnZone::spawnNewWave( %this, %numEnemies, %freq )
{
	%this.freq = %freq;
	%this.numEnemiesToSpawn = %numEnemies;
	%this.spawnThug();
}

function SpawnZone::spawnThug( %this )
{
	if (%this.numEnemiesToSpawn > 0 && mainScene.getScenePause() == false)
	{
		%x = getWord( %this.getPosition(), 0 );
		%y = getWord( %this.getPosition(), 1 );
		%sizeX = %this.getSizeX();
		%sizeY = %this.getSizeY();
		
		if (%this.side $= "left" || %this.side $= "right")
		{
			%spawnX = %x;
			%spawnY = %y + getRandom(-(%sizeY / 2.0), %sizeY / 2.0);
		}
		else
		{
			%spawnX = %x + getRandom(-(%sizeX / 2.0), %sizeX / 2.0);
			%spawnY = %y;
		}
		createThug(%spawnX SPC %spawnY);
		%this.numEnemiesToSpawn--;
	}
	%this.schedule( %this.freq, spawnThug );
}