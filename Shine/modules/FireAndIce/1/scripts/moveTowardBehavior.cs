if ( !isObject(MoveTowardBehavior) )
{
	%template = new BehaviorTemplate(MoveTowardBehavior);
	
	%template.friendlyName = "Move Toward Behavior";
	%template.behaviorType = "ai";
	%template.description = "Moves the owner toward a scene object.";
	
	%template.addBehaviorField(targetObject, "The target scene object.", SceneObject, null);
	%template.addBehaviorField(moveSpeed, "Speed to move to target.", float, 3.0);
	%template.addBehaviorField(rotateSpeed, "Speed to rotate to new angle visually.", float, 50.0);
	%template.addBehaviorField(freq, "How often to update direction. (in millisecs)", int, 100);
	%template.addBehaviorField(minDistance, "How close to bother getting.", float, 1.0);
	%template.addBehaviorField(acceleration, "How quickly to approach speed.", float, 10.0);
}

function MoveTowardBehavior::onBehaviorAdd(%this)
{
	// Insert instantiation behavior here.
	%this.updateEvent = %this.schedule(32, updateDirection);
}

function MoveTowardBehavior::onBehaviorRemove(%this)
{
	// Insert deletion behavior here.
	
}

function MoveTowardBehavior::updateDirection(%this)
{
	if ( isObject(%this.targetObject) )
	{	
		%targetX = getWord(%this.targetObject.getPosition(), 0);
		%targetY = getWord(%this.targetObject.getPosition(), 1);
		
		%this.owner.moveTo(%targetX SPC %targetY, %this.moveSpeed, true, false);
		%this.owner.rotateTo(getWord(%this.owner.getLinearVelocityPolar(), 0) + 180, 200.0);
	}
	%this.updateEvent = %this.schedule(%this.freq, updateDirection);
}