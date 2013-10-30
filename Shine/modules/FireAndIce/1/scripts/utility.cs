function drawNodePath(%nodePath)
{
	%drawObjs = new SimSet();
	
	for (%nodeIdx = 0; %nodeIdx + 1 < %nodePath.getCount(); %nodeIdx++) {
		%a = %nodePath.getObject(%nodeIdx);
		%b = %nodePath.getObject(%nodeIdx + 1);
		
		if (%nodeIdx != 0) {
			%obj = newCircle(%a.pos);
			%obj.setFillMode(true);
			%obj.setSize(0.5);
			%drawObjs.add(%obj);
			mainScene.add(%obj);
		}
		%obj = newCircle(%b.pos);
			%obj.setFillMode(true);
			%obj.setSize(0.5);
			%drawObjs.add(%obj);
			mainScene.add(%obj);
		%distSq = distToSq(%a.pos, %b.pos);
		%angle = mDegToRad(Vector2AngleToPoint(%a.pos, %b.pos) - 90.0);
		for (%i = 1; %i * %i < %distSq; %i++) {
			%objPos = projectPos(%a.pos, %angle, %i);
			%obj = newCircle(%objPos);
			%obj.setSize(0.25);
			mainScene.add(%obj);
			%drawObjs.add(%obj);
		}
	}
	return %drawObjs;
}

function newCircle(%pos)
{
	%obj = new ShapeVector();
	%obj.setPosition(%pos);
	%obj.setSize(1);
	%obj.setIsCircle(true);
	return %obj;
}
function isBetween(%v, %a, %b)
{
	%min = mGetMin(%a, %b);
	%max = mGetMax(%a, %b);
	return %v > %min && %v < %max;
}

function projectX(%y, %p1, %p2)
{
	// y = mx + b
	// x = (y - b) / m
	%m = getSlope(%p1, %p2);
	%b = getConst(%p1, %m);
	return (%y - %b) / %m;
}

function projectY(%x, %p1, %p2)
{
	// y = mx + b
	%m = getSlope(%p1, %p2);
	%b = getConst(%p1, %m);
	return %m * %x + %b;
}

function getSlope(%p1, %p2)
{
	%rise = getWord(%p1, 1) - getWord(%p2, 1);
	%run = getWord(%p1, 0) - getWord(%p2, 0);
	return %rise / %run;
}

function getConst(%p, %m)
{
	%y = getWord(%p, 1);
	%x = getWord(%p, 0);
	return %y - %m * %x;
}

function projectPos(%startPos, %angle, %distance)
{
	%x = getWord(%startPos, 0);
	%y = getWord(%startPos, 1);
	%x = %x + mCos(%angle) * %distance;
	%y = %y + mSin(%angle) * %distance;
	return %x SPC %y;
}

function reverseSimSet(%set)
{
	for (%i = 0; %i < %set.getCount(); %i++) {
		%set.bringToFront(lastObject(%set));
	}
}

function lastObject(%set)
{
	return %set.getObject(%set.getCount() - 1);
}

function distToSq(%aPos, %bPos)
{
	%xDist = getWord(%aPos, 0) - getWord(%bPos, 0);
	%yDist = getWord(%aPos, 1) - getWord(%bPos, 1);
	return %xDist * %xDist + %yDist * %yDist;
}

function findCheapestNode(%set)
{
	%bestNode = %set.getObject(0);
	%bestF = %bestNode.F;
	for (%i = 1; %i < %set.getCount(); %i++) {
		%n = %set.GetObject(%i);
		%f = %n.F;
		if (%f < %bestF) {
			%bestNode = %n;
			%bestF = %f;
		}
	}
	return %bestNode;
}

function distTo(%a, %b)
{
	return mSqrt(distToSq(%a.pos, %b.pos));
}

function getH(%a, %b)
{
	%xDist = getWord(%b.pos, 0) - getWord(%a.pos, 0);
	%yDist = getWord(%b.pos, 1) - getWord(%b.pos, 1);
	return mAbs(%xDist) + mAbs(%yDist);
}