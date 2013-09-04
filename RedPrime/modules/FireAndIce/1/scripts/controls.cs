function InputManager::onTouchDown(%this, %touchId, %worldPosition)
{
	PlayerCharacter.targetPosition = %worldPosition;
	PlayerCharacter.isShooting = true;
	PlayerCharacter.shoot();
}

function InputManager::onTouchDragged(%this, %touchId, %worldPosition)
{
	PlayerCharacter.targetPosition = %worldPosition;
}

function InputManager::onTouchUp(%this, %touchId, %worldPosition)
{
	PlayerCharacter.isShooting = false;
}