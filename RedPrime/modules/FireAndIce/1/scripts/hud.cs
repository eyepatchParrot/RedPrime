function updateHud()
{
	updateLifeIndicator();
	updateWaveNum();
}

function updateLifeIndicator()
{
	LifeIndicator.setValue( PlayerCharacter.hp );
}

function updateWaveNum()
{
	HudWaveText.setText( "Wave :" SPC SpawnManager.waveNum );
}