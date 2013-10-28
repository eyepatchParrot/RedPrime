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
}

function NavMap::extendTo(%this, %pos)
{
	// find intersecting edge
	%edge = %this.getNearestEdge(%pos);
	switch (%edge.facing) {
	case NORTH_SOUTH:
		%p1 = getWord(%pos, 0) SPC getWord(%edge.a.pos, 1);
		%p2 = getWord(%pos, 0) SPC getWord(%edge.b.pos, 1);
	case EAST_WEST:
		%p1 = getWord(%edge.a.pos, 0) SPC getWord(%pos, 1);
		%p2 = getWord(%edge.b.pos, 0) SPC getWord(%pos, 1);
	}
	%n1 = newNode(%p1);
	%n2 = newNode(%p2);
	%edge.a.connectTo(%n1);
	%edge.b.connectTo(%n2);
	%n1.connectTo(%n2);
}

function NavMap::getNearestEdge(%this, %pos)
{

function newNode(%pos)
{
	%node = new ScriptObject();
	%node.class = "NavNode";
	%node.pos = %pos;
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