function updateLifeIndicator()
{
	LifeIndicator.setValue( PlayerCharacter.hp );
	schedule(100, 0, updateLifeIndicator);
}