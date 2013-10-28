function concatSimsets(%a, %b)
{
	%c = new SimSet();
	for (%i = 0; %i < %a.getCount(); %i++) {
		%c.add(%a.getObject(%i));
	}
	for (%i = 0; %i < %b.getCount(); %i++) {
		%c.add(%b.getObject(%i));
	}
	return %c;
}

function newMap()
{
	%map = new ScriptObject();
	%map.class = "NavMap";
	return %map;
}

function NavMap::initAt(%this, %pNW, %pNE, %pSW, %pSE)
{
	if (!isObject(%this.nodes)) {
		%this.nodes = new SimSet();
	}
	
	%this.nodes.clear();
	%nodeNW = newNode(%pNW);
	%nodeNE = newNode(%pNE);
	%nodeSW = newNode(%pSW);
	%nodeSE = newNode(%pSE);
	
	%nodeNW.connectTo(%nodeNE);
	%nodeNW.connectTo(%nodeSW);
	%nodeSE.connectTo(%nodeNE);
	%nodeSE.connectTo(%nodeSW);
	
	echo("add nodes");
	%this.nodes.add(%nodeNW);
	%this.nodes.add(%nodeNE);
	%this.nodes.add(%nodeSW);
	%this.nodes.add(%nodeSE);
}

function NavMap::extendTo(%this, %pos)
{
	// find intersecting edge
	%edge = %this.getNearestEdge(%pos);
	echo("nearest edge is" SPC %edge.a.pos SPC %edge.b.pos);
	switch (%edge.facing) {
	case $NORTH_SOUTH:
		echo("edge is n/s");
		%p1 = getWord(%pos, 0) SPC getWord(%edge.a.pos, 1);
		%p2 = getWord(%pos, 0) SPC getWord(%edge.b.pos, 1);
	case $EAST_WEST:
		echo("edge is e/w");
		%p1 = getWord(%edge.a.pos, 0) SPC getWord(%pos, 1);
		%p2 = getWord(%edge.b.pos, 0) SPC getWord(%pos, 1);
	}
	%n1 = newNode(%p1);
	%n2 = newNode(%p2);
	%edge.a.connectTo(%n1);
	%edge.b.connectTo(%n2);
	%n1.connectTo(%n2);
	%this.nodes.add(%n1);
	%this.nodes.add(%n2);
}

function NavMap::isEmpty(%this)
{
	if (!isObject(%this.nodes) || %this.nodes.getCount() == 0) {
		return true;
	}
	return false;
}

function NavMap::getNearestEdge(%this, %pos)
{
	%edges = %this.nodes.getObject(0).getEdges();
	echo("thwop");
	%closeEdge = %edges.getObject(0);
	%edgePoint = %closeEdge.getAverage();
	%xDist = getWord(%edgePoint, 0) - getWord(%pos, 0);
	%yDist = getWord(%edgePoint, 1) - getWord(%pos, 1);
	%minDist = %xDist * %xDist + %yDist * %yDist;
	for (%i = 0; %i < %edges.getCount(); %i++) {
		%curEdge = %edges.getObject(%i);
		%edgePoint = %curEdge.getAverage();
		%xDist = getWord(%edgePoint, 0) - getWord(%pos, 0);
		%yDist = getWord(%edgePoint, 1) - getWord(%pos, 1);
		%curDist = %xDist * %xDist + %yDist * %yDist;
		if (%curDist < %minDist) {
			%closeEdge = %curEdge;
			%minDist = %curDist;
		}
	}
	return %closeEdge;
}

function NavMap::draw(%this)
{
	if (!isObject(%this.drawNodes)) {
		%this.drawNodes = new SimSet();
	} else {
		while (%this.drawNodes.getCount()) {
			%this.drawNodes.getObject(0).delete();
		}
		%this.drawNodes.delete();
	}
	%this.drawNodes = new SimSet();
	for (%i = 0; %i < %this.nodes.getCount(); %i++) {
		%curNode = %this.nodes.getObject(%i);
		%size = 0.5;
		%obj = new ShapeVector();
		%obj.setSize(%size);
		%obj.setLineColor("1 1 1 1");
		%obj.setFillColor("0 0 0 1");
		%obj.setFillMode(true);
		%obj.setIsCircle(true);
		%obj.setCircleRadius(%size);
		%obj.setPosition(%curNode.pos);
		mainScene.add(%obj);
		%this.drawNodes.add(%obj);
	}
}

function newEdge(%node1, %node2)
{
	%edge = new ScriptObject();
	%edge.class = "NavEdge";
	%xDist = mAbs(getWord(%node1.pos, 0) - getWord(%node2.pos, 0));
	%yDist = mAbs(getWord(%node1.pos, 1) - getWord(%node2.pos, 1));
	echo(%xDist SPC %yDist);
	if (%xDist > %yDist) {
		echo("face e/w");
		%edge.facing = $EAST_WEST;
		if (getWord(%node1.pos, 0) < getWord(%node2.pos, 0)) {
			%edge.a = %node1;
			%edge.b = %node2;
		} else {
			%edge.a = %node2;
			%edge.b = %node1;
		}
	} else {
		echo("face n/s");
		%edge.facing = $NORTH_SOUTH;
		if (getWord(%node1.pos, 1) > getWord(%node2.pos, 1)) {
			%edge.a = %node1;
			%edge.b = %node2;
		} else {
			%edge.a = %node2;
			%edge.b = %node1;
		}
	}
	return %edge;
}

function NavEdge::getAverage(%this)
{
	%x = (getWord(%this.a.pos, 0) + getWord(%this.b.pos, 0)) / 2.0;
	%y = (getWord(%this.a.pos, 1) + getWord(%this.b.pos, 1)) / 2.0;
	return %x SPC %y;
}

function newNode(%pos)
{
	%node = new ScriptObject();
	%node.class = "NavNode";
	%node.pos = %pos;
	return %node;
}

function NavNode::getEdges(%this, %visitedNodes)
{
	%edges = new SimSet();
	if (!isObject(%visitedNodes)) {
		%visitedNodes = new SimSet();
	}
	%visitedNodes.add(%this);
	echo("************* visiting" SPC %this.pos SPC "*************");
	echo("numVisitedNodes" SPC %visitedNodes.getCount());
	// create edges
	for (%i = 0; %i < %this.nodes.getCount(); %i++) {
		%curNode = %this.nodes.getObject(%i);
//		echo("try edge from" SPC %this.pos SPC "to" SPC %curNode.pos);
		if (!%curNode.isInSimSet(%visitedNodes)) {
//			echo("success");
			echo("edge from" SPC %this.pos SPC "to" SPC %curNode.pos);
			%curEdge = newEdge(%this, %curNode);
			%edges.add(%curEdge);
		}
	}
	
	// recurse
	for (%i = 0; %i < %this.nodes.getCount(); %i++) {
		%curNode = %this.nodes.getObject(%i);
		if (!%curNode.isInSimSet(%visitedNodes)) {
			%edges = concatSimSets(%edges, %curNode.getEdges(%visitedNodes));
		}
	}
	
	echo("done");
	return %edges;
}	

function NavNode::connectTo(%this, %other)
{
	if (!isObject(%this.nodes)) {
		%this.nodes = new SimSet();
	}
	
	if (!isObject(%other.nodes)) {
		%other.nodes = new SimSet();
	}
	
	%this.nodes.add(%other);
	%other.nodes.add(%this);
}

function NavNode::isInSimSet(%this, %set)
{
	for (%i = 0; %i < %set.getCount(); %i++) {
		if (%set.getObject(%i) == %this) {
			return true;
		}
	}
	return false;
}