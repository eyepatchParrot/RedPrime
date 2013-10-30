// NavMap
// public:
// newMap() %map
// NavMap::isEmpty(%this) %isEmpty
// NavMap::initAt(%this, %pNW, %pNE, %pSW, %pSE) %rootQuad
// NavMap::extendTo(%this, %pos, %quad) %newQuad
// NavMap::connect(%this, %aQuad, %bQuad) %newQuad
// NavMap::getQuadAt(%this, %pos) %quad
// NavMap::getAllQuadsAt(%this, %pos) %quads
// NavMap::getNeighbors(%this, %pos) %neighbors
// NavMap::lineConnects(%this, %a, %b) %doesConnect
// NavMap::draw(%this)
//
// private:
// NavMap::getVisibleQuads(%this, %node, %quad, %quads) %quads
// NavMap::quadIsVisibleFrom(%this, %node, %quad) %isVisible
// NavMap::getQuadIntersect(%this, %quad, %prevQuad, %line) %intersectingQuad
// NavMap::getNodes(%this) %nodes
// NavMap::getQuads(%this) %quads

function newMap()
{
	%map = new ScriptObject();
	%map.class = "NavMap";
	return %map;
}

function NavMap::isEmpty(%this)
{
	return !isObject(%this.rootQuad);
}

function NavMap::initAt(%this, %pNW, %pNE, %pSW, %pSE)
{
	%this.rootQuad = newNavQuad(newNode(%pNW), newNode(%pNE), newNode(%pSW), newNode(%pSE));
	return %this.rootQuad;
}

function NavMap::extendTo(%this, %pos, %prevQuad)
{
	if (!isObject(%prevQuad)) {
		return;
	}

	if (%prevQuad.posIsEast(%pos)) {
		%q = newNavQuadEastOf(%prevQuad, %pos);
	} else if (%prevQuad.posIsWest(%pos)) {
		%q = newNavQuadWestOf(%prevQuad, %pos);
	} else if (%prevQuad.posIsNorth(%pos)) {
		%q = newNavQuadNorthOf(%prevQuad, %pos);
	} else if (%prevQuad.posIsSouth(%pos)) {
		%q = newNavQuadSouthOf(%prevQuad, %pos);
	} else {
		echo("%pos not within %prevQuad");
	}
	
	%qNWY = getWord(%q.nw.pos, 1);
	%qNEY = getWord(%q.ne.pos, 1);
	%qSWY = getWord(%q.sw.pos, 1);
	%qSEY = getWord(%q.se.pos, 1);

	return %q;
}

function NavMap::connect(%this, %aQuad, %bQuad)
{
	%pos = %bQuad.getCenterX() SPC %bQuad.getCenterY();
	if (%aQuad.posIsNorth(%pos)) {
		%q = %aQuad.connectNorthTo(%bQuad);
	} else if (%aQuad.posIsEast(%pos)) {
		%q = %aQuad.connectEastTo(%bQuad);
	} else if (%aQuad.posIsSouth(%pos)) {
		%q = %aQuad.connectSouthTo(%bQuad);
	} else if (%aQuad.posIsWest(%pos)) {
		%q = %aQuad.connectWestTo(%bQuad);
	} else {
		echo("can't connect");
	}
	return %q;
}

function NavMap::getQuadAt(%this, %pos)
{
	%quads = %this.getQuads();
	for (%i = 0; %i < %quads.getCount(); %i++) {
		%q = %quads.getObject(%i);
		if (%q.contains(%pos)) {
			return %q;
		}
	}
}

function NavMap::getQuadAtNode(%this, %node)
{
	%quads = %this.getQuads();
	for (%i = 0; %i < %quads.getCount(); %i++) {
		%q = %quads.getObject(%i);
		if (%q.containsNode(%node)) return %q;
	}
}

function NavMap::getAllQuadsAt(%this, %node)
{
	%quads = %this.getQuads();
	%adjQuads = new SimSet();
	%quads.callOnChildren(addToSetIfContains, %node, %adjQuads);
	
	return %adjQuads;
}

function NavMap::getNeighbors(%this, %node)
{
	if (!isObject(%node)) echo("node is missing from NavMap::getNeighbors");

	%sT = getRealTime();
	if (!isObject(%node.neighbors)) {
		%node.neighbors = %this.calcNeighbors(%node);
	}
	$totalCalcTime += getRealTime() - %sT;
	
	return %node.neighbors;
}

function NavMap::calcNeighbors(%this, %node)
{
	if (!isObject(%node)) echo("node is missing from NavMap::calcNeighbors");

	// if (isObject(%node.neighbors)) return;
	
	%visibleQuads = new SimSet();
	%neighbors = %this.getAdjacentNodes(%node);
	%quad = %this.getQuadAtNode(%node);
	if (!isObject(%quad)) echo("node isn't in a quad" SPC %node.pos);
	
	%this.getVisibleNodes(%node, %quad, %neighbors, %visibleQuads);

	return %neighbors;
}

function NavMap::lineConnects(%this, %a, %b) {
	%adjQuads = %this.getAllQuadsAt(%a);
	for (%i = 0; %i < %adjQuads.getCount(); %i++) {
		%q = %adjQuads.getObject(%i);
		if (%q.containsNode(%b)) return true;
	}
	
	%q = %adjQuads.getObject(0);
	while (!%q.containsNode(%b)) {
		// echo("q" SPC %q.getCenterX() SPC %q.getCenterY());
		%intersect = %this.getQuadIntersect(%q, %pQ, %a.pos, %b.pos);
		if (!isObject(%intersect)) return false;
		%pQ = %q;
		%q = %intersect;
	}
	return true;
}

// ** TODO: Needs to be able to handle non-rectangular objs **
function NavMap::draw(%this)
{
	if (!isObject(%this.drawObjs)) {
		%this.drawObjs = new SimSet();
	} else {
		for (%i = 0; %i < %this.drawObjs.getCount(); %i++) {
			mainScene.remove(%this.drawObjs.getObject(%i));
		}
		%this.drawObjs.deleteObjects();
	}
	%quads = %this.getQuads();
	%quads.callOnChildren(draw, $selectedQuad, %this.drawObjs);
}

function newConnectCircle(%pos)
{
	%obj = newCircle(%pos);
	%obj.setIsCircle(false);
	%obj.setPolyPrimitive(3);
	%obj.setFillMode(true);
	%obj.setSize(0.25);
	%obj.setFillColor("1 1 0 1");
	return %obj;
}

// ** private **
function NavMap::getVisibleNodes(%this, %node, %quad, %nodes, %quads)
{
	if (!isObject(%node)) echo("node is missing from NavMap::getVisibleNodes");
	if (!isObject(%quad)) echo("quad is missing from NavMap::getVisibleNodes");
	if (!isObject(%nodes)) echo("nodes is missing from NavMap::getVisibleNodes");
	if (!isObject(%quads)) echo("quads is missing from NavMap::getVisibleNodes");
	
	%quads.add(%quad);
	
	%isVisible = false;
	if (%quad.nw != %node && (%nodes.isMember(%quad.nw) || %this.lineConnects(%node, %quad.nw))) {
		%isVisible = true;
		%nodes.add(%quad.nw);
	}
	if (%quad.ne != %node && (%nodes.isMember(%quad.ne) || %this.lineConnects(%node, %quad.ne))) {
		%isVisible = true;
		%nodes.add(%quad.ne);
	}
	if (%quad.sw != %node && (%nodes.isMember(%quad.sw) || %this.lineConnects(%node, %quad.sw))) {
		%isVisible = true;
		%nodes.add(%quad.sw);
	}
	if (%quad.se != %node && (%nodes.isMember(%quad.se) || %this.lineConnects(%node, %quad.se))) {
		%isVisible = true;
		%nodes.add(%quad.se);
	}
	if (%isVisible) {
		if (isObject(%quad.n) && !%quads.isMember(%quad.n)) %this.getVisibleNodes(%node, %quad.n, %nodes, %quads);
		if (isObject(%quad.e) && !%quads.isMember(%quad.e)) %this.getVisibleNodes(%node, %quad.e, %nodes, %quads);
		if (isObject(%quad.s) && !%quads.isMember(%quad.s)) %this.getVisibleNodes(%node, %quad.s, %nodes, %quads);
		if (isObject(%quad.w) && !%quads.isMember(%quad.w)) %this.getVisibleNodes(%node, %quad.w, %nodes, %quads);
	}
}

function NavMap::getAdjacentNodes(%this, %node)
{
	if (!isObject(%node)) echo("missing node at NavMap::getAdjacentNodes");

	%nodes = new SimSet();
	%quads = %this.getAllQuadsAt(%node);
	%quads.callOnChildren(addNodesToSetUniquely, %nodes, %node);
	return %nodes;
}

function NavMap::getQuadIntersect(%this, %q, %pQ, %p1, %p2)
{
	if (!isObject(%q)) echo("q is missing at NavMap::getQuadIntersect");
	
	%wIntersect = linesIntersect(%p1, %p2, %q.nw.pos, %q.sw.pos);
	if (%q.w != %pQ && %wIntersect) return %q.w;
	
	%nIntersect = linesIntersect(%p1, %p2, %q.nw.pos, %q.ne.pos);
	if (%q.n != %pQ && %nIntersect) return %q.n;
	
	%eIntersect = linesIntersect(%p1, %p2, %q.ne.pos, %q.se.pos);
	if (%q.e != %pQ && %eIntersect) return %q.e;
	
	%sIntersect = linesIntersect(%p1, %p2, %q.sw.pos, %q.se.pos);
	if (%q.s != %pQ && %sIntersect) return %q.s;
}

function NavMap::getNodes(%this)
{
	%quads = %this.getQuads();
	%nodes = new SimSet();
	%quads.callOnChildren(addNodesToSetUniquely, %nodes);
	return %nodes;
}

function NavMap::getQuads(%this)
{
	return %this.rootQuad.getQuads();
}

function newNode(%pos)
{
	%node = new ScriptObject();
	%node.class = "NavNode";
	%node.pos = %pos;
	return %node;
}
