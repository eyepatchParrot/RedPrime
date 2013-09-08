if ( !isObject(MoveAnimationBehavior) )
{
	%template = new BehaviorTemplate(MoveAnimationBehavior);
	
	%template.friendlyName = "Move Animation Behavior";
	%template.behaviorType = "Graphics";
	%template.description = "If moving, play walk animation, otherwise static.";
	
	%template.addBehaviorField(idleImage, "The image to display when not moving.", string, "FireAndIce:soldier");
	%template.addBehaviorField(walkAnimation, "The animation to play when moving.", string, "FireAndIce:soldierAnim");
	%template.addBehaviorField(updateFreq, "How often to update the animation. (ms)", int, 200);
}

function MoveAnimationBehavior::onBehaviorAdd(%this)
{
	%this.wasMoving = false;
	%this.owner.setImage(%this.idleImage);
	%this.updateAnimation();
}

function MoveAnimationBehavior::onBehaviorRemove(%this)
{
	// Insert deletion behavior here.
}

function MoveAnimationBehavior::updateAnimation( %this )
{
	if (%this.isMoving() != %this.wasMoving)
	{
		if (%this.isMoving())
		{
			%this.owner.playAnimation(%this.walkAnimation);
		}
		else
		{
			%this.owner.setImage(%this.idleImage);
		}
	}
	%this.wasMoving = %this.isMoving();
	%this.schedule(%this.updateFreq, updateAnimation);
}

function MoveAnimationBehavior::isMoving( %this )
{
	return mAbs(%this.owner.getLinearVelocityX()) + mAbs(%this.owner.getLinearVelocityY()) > 0.1;
}