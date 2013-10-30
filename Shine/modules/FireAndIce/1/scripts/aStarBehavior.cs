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
	
	// %neighbors = %this.getNeighbors(%startNode, %endNode);
	// for (%i = 0; %i < %neighbors.getCount(); %i++) {
		// %neighbor = %neighbors.getObject(%i);
		// %path = new SimSet();
		// %path.add(%startNode);
		// %path.add(%neighbor);
		// drawNodePath(%path);
	// }
	
	echo("*** find path ***");
	%totalTimeStart = getRealTime();
	while (%openNodes.getCount() > 0 && !%closedNodes.isMember(%endNode)) {
		echo("numOpenNodes :" SPC %openNodes.getCount());
		%n = findCheapestNode(%openNodes);
		echo("** n" SPC %n.pos SPC "n.parent" SPC %n.parent.pos SPC "**");
		%closedNodes.add(%n);
		%openNodes.remove(%n);
		
		%sT = getRealTime();
		%neighbors = %this.getNeighbors(%n, %endNode);
		%eT = getRealTime();
		%neighborTime += %eT - %sT;
		echo("numNeighbors :" SPC %neighbors.getCount());
		for (%i = 0; %i < %neighbors.getCount(); %i++) {
			%neighbor = %neighbors.getObject(%i);
			%g = %n.G + distTo(%n, %neighbor);
			if (%openNodes.isMember(%neighbor) && %g < %neighbor.G) {
				%openNodes.remove(%neighbor);
			}
			if (%closedNodes.isMember(%neighbor) && %g < %neighbor.G) {
				%closedNodes.remove(%neighbor);
			}
			if (!%openNodes.isMember(%neighbor) && !%closedNodes.isMember(%neighbor)) {
				%neighbor.G = %g;
				%neighbor.F = %neighbor.G + getH(%neighbor, %endNode);
				%neighbor.parent = %n;
				%openNodes.add(%neighbor);

			}
		}
	}
	%totalTime = getRealTime() - %totalTimeStart;
	%neighborPerc = %neighborTime / %totalTime * 100.0;
	echo("neighborTime" SPC %neighborTime SPC %neighborPerc @ "%" SPC "totalTime" SPC %totalTime);
	
	%nodePath = new SimSet();
	%n = %endNode;
	%nodePath.add(%n);
	while (isObject(%n.parent)) {
		echo("add node" SPC %n.pos);
		%nodePath.add(%n.parent);
		%n = %n.parent;
	}
	
	reverseSimSet(%nodePath);
	
	return %nodePath;
}

function AStarBehavior::getNeighbors(%this, %n, %endNode)
{
	if (!isObject(%this.navMap)) {
		echo("no map!");
	}

	%neighbors = %this.navMap.getNeighbors(%n);
	%isVisible = %this.navMap.lineConnects(%n, %endNode);
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