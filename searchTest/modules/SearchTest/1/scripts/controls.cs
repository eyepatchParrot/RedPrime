function InputManager::onTouchDown(%this, %touchId, %worldPosition)
{
	switch ($Game::Mode) {
	case $SET_START_NODE:	
		SearchTest.setStartNode(%worldPosition);
		
	case $SET_END_NODE:
		SearchTest.setEndNode(%worldPosition);
	
	case $ADD_RECT:
		SearchTest.addRect(%worldPosition);
		
	case $SELECT_RECT:
		SearchTest.selectRect(%worldPosition);
		
	case $DEL_RECT:
		SearchTest.deleteRect();
		
	case $CONNECT_RECT:
		SearchTest.connectRect(%worldPosition);
	}
}

function InputManager::onRightMouseDown(%this, %touchId, %worldPosition)
{
	$Game::Mode = ($Game::Mode + 1) % 6;
	SearchTest.updateMode();
}