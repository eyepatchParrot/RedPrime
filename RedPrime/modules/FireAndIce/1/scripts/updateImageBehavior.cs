if ( !isObject(UpdateImageBehavior) )
{
	%template = new BehaviorTemplate(UpdateImageBehavior);
	
	%template.friendlyName = "Update Image";
	%template.behaviorType = "Graphics";
	%template.description = "Receives moveUp, moveDown, moveLeft, moveRight and updates image accordingly.";
	
	%template.addBehaviorField(notMovingImage, "An image of the character not moving.", string, "ToyAssets:TD_Wizard_CompSprite");
	%template.addBehaviorField(moveNorthAnimation, "An animation of the character moving north.", string, "ToyAssets:TD_Wizard_WalkNorth");
	%template.addBehaviorField(moveSouthAnimation, "An animation of the character moving south.", string, "ToyAssets:TD_Wizard_WalkSouth");
	%template.addBehaviorField(moveWestAnimation, "An animation of the character moving west.", string, "ToyAssets:TD_Wizard_WalkWest");
}

function UpdateImageBehavior::onBehaviorAdd(%this)
{
	// Insert instantiation behavior here.
	%this.owner.setImage(%this.notMovingImage);
}

function UpdateImageBehavior::onBehaviorRemove(%this)
{
	// Insert deletion behavior here.
}

function UpdateImageBehavior::onChangeMovement(%this, %isMovingUp, %isMovingDown, %isMovingRight, %isMovingLeft)
{
	%this.owner.setFlip(false, false);
	if (%isMovingUp && !%isMovingDown)
	{
		%this.owner.playAnimation(%this.moveNorthAnimation);
	}
	else if (%isMovingDown && !%isMovingUp)
	{
		%this.owner.playAnimation(%this.moveSouthAnimation);
	}
	else if (%isMovingLeft && !%isMovingRight)
	{
		%this.owner.playAnimation(%this.moveWestAnimation);
	}
	else if (%isMovingRight && !%isMovingLeft)
	{
		%this.owner.setFlipX(true);
		%this.owner.playAnimation(%this.moveWestAnimation);
	}
	else
	{
		%this.owner.setImage(%this.notMovingImage);
	}
}