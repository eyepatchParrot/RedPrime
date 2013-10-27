function updateHud()
{
	updateWaveNum();
	updateKillNum();
}

function updateWaveNum()
{
	HudWaveText.setText( "Wave :" SPC SpawnManager.waveNum );
}

function updateKillNum()
{
	HudKillsText.setText( "Kills :" SPC $Game::Kills );
}