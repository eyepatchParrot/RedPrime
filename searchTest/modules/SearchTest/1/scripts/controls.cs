function InputManager::onTouchDown(%this, %touchId, %worldPosition)
{
	switch ($Game::Mode) {
	case $SET_START_NODE:	
		SearchTest.setStartNode(%worldPosition);
		
	case $SET_END_NODE:
		SearchTest.setEndNode(%worldPosition);
	
	case $ADD_RECT:
		SearchTest.addRect(%worldPosition);
		
	case $DELETE_RECT:
		SearchTest.deleteRect();
	}
}

function InputManager::onRightMouseDown(%this, %touchId, %worldPosition)
{
	$Game::Mode = ($Game::Mode + 1) % 4;
	SearchTest.updateMode();
}