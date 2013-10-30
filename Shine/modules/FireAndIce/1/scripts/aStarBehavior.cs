if ( !isObject(AStarBehavior) )
{
	%template = new BehaviorTemplate(AStarBehavior);
	
	%template.friendlyName = "A Star Behavior";
	%template.behaviorType = "ai";
	%template.description = "Calculates a path to target.";
	
	%template.addBehaviorField(navMap, "The nav map the describes valid space.", ScriptObject, null);
	%template.addBehaviorField(targetObj, "The target scene object.", SceneObject, null);
	%template.addBehaviorField(freq, "How often to update direction. (in millisecs)", int, 10000);
}

function AStarBehavior::onBehaviorAdd(%this)
{
	%this.tickEvent = %this.schedule(32, tick);
}

function AStarBehavior::onBehaviorRemove(%this)
{
	%this.clearDrawObjs();
}

function AStarBehavior::tick(%this)
{
	// echo("tick");
	if ( isObject(%this.targetObj) )
	{
		%startNode = newNode(%this.owner.getPosition());
		%endNode = newNode(%this.targetObj.getPosition());
		// %openNodes = new SimSet();
		// %closedNode = new SimSet();
		
		%path = %this.getPathBetween(%startNode, %endNode);
		
		%this.clearDrawObjs();
		
		%this.drawObjs = drawNodePath(%path);
	}

	%this.tickEvent = %this.schedule(%this.freq, tick);
}

function AStarBehavior::getPathBetween(%this, %startNode, %endNode)
{
	%openNodes = new SimSet();
	%closedNodes = new SimSet();
	
	// F = G + H
	%openNodes.add(%startNode);
	%startNode.G = 0;
	%startNode.F = %startNode.G + getH(%startNode, %endNode);
	
	%totalTimeStart = getRealTime();
	$totalCheapTime = 0;
	$totalNeighborTime = 0;
	$totalTickTime = 0;
	$totalCondTime = 0;
	%isOk = %openNodes.getCount() > 0 && !%closedNodes.isMember(%endNode);
	while (%isOk) {
		%sT = getRealTime();
		%n = findCheapestNode(%openNodes);
		%closedNodes.add(%n);
		%openNodes.remove(%n);
		$totalCheapTime += getRealTime() - %sT;
		
		%sT = getRealTime();
		%neighbors = %this.getNeighbors(%n, %endNode);
		$totalNeighborTime += getRealTime() - %sT;
		
		%sT = getRealTime();
		%neighbors.callOnChildren(tickAStar, %openNodes, %closedNodes, %n);
		$totalTickTime += getRealTime() - %sT;
		
		%sT = getRealTime();
		%isOk = %openNodes.getCount() > 0 && !%closedNodes.isMember(%endNode);
		$totalCondTime += getRealTime() - %sT;
	}
	%dT = getRealTime() - %totalTimeStart;
	$totalTime = %dT;
	%cheapPerc = $totalCheapTime / $totalTime * 100.0;
	%tickPerc = $totalTickTime / $totalTime * 100.0;
	%neighborPerc = $totalNeighborTime / $totalTime * 100.0;
	%condPerc = $totalCondtime / $totalTime * 100.0;
	%linePerc = $totalLineTime / $totalTime * 100.0;
	%calcPerc = $totalCalcTime / $totalTime * 100.0;
	echo("dT" SPC %dT SPC "cheap" SPC $totalCheapTime SPC "neighbor" SPC $totalNeighborTime SPC "tick" SPC $totalTickTime SPC "cond" SPC $totalCondTime SPC "line%" SPC %linePerc SPC "calc%" SPC %calcPerc SPC "totalTime" SPC $totalTime);
	
	%nodePath = new SimSet();
	%n = %endNode;
	%nodePath.add(%n);
	while (isObject(%n.parent)) {
		%nodePath.add(%n.parent);
		%n = %n.parent;
	}
	
	reverseSimSet(%nodePath);
	
	return %nodePath;
}

function NavNode::tickAStar(%this, %openNodes, %closedNodes, %parent)
{
	if (!isObject(%openNodes)) echo("no openNodes in AStarBehavior::tickAStar");
	if (!isObject(%closedNodes)) echo("no closedNodes in AStarBehavior::tickAStar");

	%g = %parent.G + distTo(%parent, %this);
	if (%openNodes.isMember(%this) && %g < %this.G) {
		%openNodes.remove(%this);
	}
	if (%closedNodes.isMember(%this) && %g < %this.G) {
		%closedNodes.remove(%this);
	}
	if (!%openNodes.isMember(%this) && !%closedNodes.isMember(%this)) {
		%this.G = %g;
		%this.F = %this.G + getH(%this, %endNode);
		%this.parent = %parent;
		%openNodes.add(%this);
	}
}

function AStarBehavior::getNeighbors(%this, %n, %endNode)
{
	if (!isObject(%this.navMap)) echo("no map!");
	if (!isObject(%n)) echo("no n in AStarBehavior::getNeighbors");
	if (!isObject(%endNode)) echo("no endNOde in AStarBehavior::getNeighbors");

	%neighbors = %this.navMap.getNeighbors(%n);
	%sT = getRealTime();
	%isVisible = %this.navMap.lineConnects(%n, %endNode);
	$totalLineTime = getRealTime() - %sT;
	if (%isVisible) %neighbors.add(%endNode);
	
	return %neighbors;
}

function AStarBehavior::clearDrawObjs(%this)
{
	if (!isObject(%this.drawObjs)) {
		return;
	}
	
	for (%i = 0; %i < %this.drawObjs.getCount(); %i++) {
		%o = %this.drawObjs.GetObject(%i);
		mainScene.remove(%o);
	}
	%this.drawObjs.deleteObjects();
}