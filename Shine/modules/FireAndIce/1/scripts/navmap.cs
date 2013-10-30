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
	for (%i = 0; %i < quads.getCount(); %i++) {
		%q = %quads.getObject(%i);
		if (%q.containsNode(%pos)) return %q;
	}
}

function NavMap::getAllQuadsAt(%this, %node)
{
	%quads = %this.getQuads();
	%adjQuads = new SimSet();
	for (%i = 0; %i < %quads.getCount(); %i++) {
		%q = %quads.getObject(%i);
		if (%q.containsNode(%node)) %adjQuads.add(%q);
	}
	
	return %adjQuads;
}

function NavMap::getNeighbors(%this, %node)
{
	%neighbors = new SimSet();
	%adjQuads = %this.getAllQuadsAt(%node);
	
	// %visibleQuads = %adjQuads;
	%visibleQuads = %this.getVisibleQuads(%node, %adjQuads.getObject(0), %adjQuads);
	for (%i = 0; %i < %visibleQuads.getCount(); %i++) {
		%q = %visibleQuads.getObject(%i);
		if (!%neighbors.isMember(%q.nw) && %this.lineConnects(%node, %q.nw)) %neighbors.add(%q.nw);
		if (!%neighbors.isMember(%q.ne) && %this.lineConnects(%node, %q.ne)) %neighbors.add(%q.ne);
		if (!%neighbors.isMember(%q.sw) && %this.lineConnects(%node, %q.sw)) %neighbors.add(%q.sw);
		if (!%neighbors.isMember(%q.se) && %this.lineConnects(%node, %q.se)) %neighbors.add(%q.se);
	}

	return %neighbors;
}

function NavMap::lineConnects(%this, %a, %b) {
	%adjQuads = %this.getAllQuadsAt(%a);
	for (%i = 0; %i < %adjQuads.getCount(); %i++) {
		%q = %adjQuads.getObject(%i);
		if (%q.containsNode(%b)) return true;
	}
	
	%q = %adjQuads.getObject(0);
	%line = newLine(%a.pos, %b.pos);
	while (!%q.containsNode(%b)) {
		%intersect = %this.getQuadIntersect(%q, %pQ, %line);
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
	for (%i = 0; %i < %quads.getCount(); %i++) {
		%q = %quads.getObject(%i);
		%qX = %q.getCenterX();
		%qY = %q.getCenterY();
		%qW = %q.getW();
		%qH = %q.getH();
		%obj = new ShapeVector();
		%obj.setPosition(%qX SPC %qY);
		%obj.setSize(%qW / 2.0, %qH / 2.0);
		%obj.setLineColor("1 1 1 1");
		if (isObject($selectedQuad) && $selectedQuad == %q) {
			%obj.setFillColor("0 1 1 0.5");
		} else {
			%obj.setFillColor("0 0 1 0.5");
		}
		%obj.setFillMode(true);
		%obj.setPolyPrimitive(4);
		mainScene.add(%obj);
		%this.drawObjs.add(%obj);
		
		%oNE = newCircle(%q.ne.pos);
		mainScene.add(%oNE);
		%this.drawObjs.add(%oNE);
	
		%oSE = newCircle(%q.se.pos);
		mainScene.add(%oSE);
		%this.drawObjs.add(%oSE);
		
		%oNW = newCircle(%q.nw.pos);
		mainScene.add(%oNW);
		%this.drawObjs.add(%oNW);
		
		%oSW = newCircle(%q.sw.pos);
		mainScene.add(%oSW);
		%this.drawObjs.add(%oSW);
		
		if (isObject(%q.n)) {
			%oN = newConnectCircle(%qX SPC (%qY + %qH / 4));
			mainScene.add(%oN);
			%this.drawObjs.add(%oN);
		}
		
		if (isObject(%q.e)) {
			%oE = newConnectCircle((%qX + %qW / 4) SPC %qY);
			mainScene.add(%oE);
			%this.drawObjs.add(%oE);
		}
		
		if (isObject(%q.s)) {
			%oS = newConnectCircle(%qX SPC (%qY - %qH / 4));
			mainScene.add(%oS);
			%this.drawObjs.add(%oS);
		}
		
		if (isObject(%q.w)) {
			%oW = newConnectCircle((%qX - %qW / 4) SPC %qY);
			mainScene.add(%oW);
			%this.drawObjs.add(%oW);
		}
	}
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
	
function NavMap::getVisibleQuads(%this, %node, %quad, %quads)
{
	%quads.add(%quad);
	if (isObject(%quad.n) && !%quads.isMember(%quad.n) && %this.nodeConnectsToQuad(%node, %quad.n)) %this.getVisibleQuads(%node, %quad.n, %quads);
	if (isObject(%quad.e) && !%quads.isMember(%quad.e) && %this.nodeConnectsToQuad(%node, %quad.e)) %this.getVisibleQuads(%node, %quad.e, %quads);
	if (isObject(%quad.s) && !%quads.isMember(%quad.s) && %this.nodeConnectsToQuad(%node, %quad.s)) %this.getVisibleQuads(%node, %quad.s, %quads);
	if (isObject(%quad.w) && !%quads.isMember(%quad.w) && %this.nodeConnectsToQuad(%node, %quad.w)) %this.getVisiblequads(%node, %quad.w, %quads);
	return %quads;
}

function NavMap::nodeConnectsToQuad(%this, %node, %quad)
{
	return %this.lineConnects(%node, %quad.nw) || %this.lineConnects(%node, %quad.ne) || %this.lineConnects(%node, %quad.sw) || %this.lineConnects(%node, %quad.se);
}

function NavMap::getQuadIntersect(%this, %q, %pQ, %line)
{
	%wLine = newLine(%q.nw.pos, %q.sw.pos);
	%nLine = newLine(%q.nw.pos, %q.ne.pos);
	%eLine = newLine(%q.ne.pos, %q.se.pos);
	%sLine = newLine(%q.sw.pos, %q.se.pos);
	
	if (%q.w != %pQ && %line.intersects(%wLine)) return %q.w;
	if (%q.n != %pQ && %line.intersects(%nLine)) return %q.n;
	if (%q.e != %pQ && %line.intersects(%eLine)) return %q.e;
	if (%q.s != %pQ && %line.intersects(%sLine)) return %q.s;
}

function NavMap::getNodes(%this)
{
	%quads = %this.getQuads();
	%nodes = new SimSet();
	for (%i = 0; %i < %quads.getCount(); %i++) {
		%q = %quads.getObject(%i);
		if (isObject(%q.nw) && !%nodes.isMember(%q.nw)) %nodes.add(%q.nw);
		if (isObject(%q.ne) && !%nodes.isMember(%q.ne)) %nodes.add(%q.ne);
		if (isObject(%q.sw) && !%nodes.isMember(%q.sw)) %nodes.add(%q.sw);
		if (isObject(%q.se) && !%nodes.isMember(%q.se)) %nodes.add(%q.se);
	}
	return %nodes;
}

function NavMap::getQuads(%this)
{
	return %this.rootQuad.getQuads();
}

function newLine(%a, %b)
{
	%line = new ScriptObject();
	%line.class = "NavLine";
	%line.p1 = %a;
	%line.p2 = %b;
	%x1 = getWord(%a, 0);
	%y1 = getWord(%a, 1);
	%x2 = getWord(%b, 0);
	%y2 = getWord(%b, 1);
	%line.A = %y2 - %y1;
	%line.B = %x1 - %x2;
	%line.C = %line.A * %x1 + %line.B * %y1;
	return %line;
}

function NavLine::intersects(%this, %b)
{
	%A_1 = %this.A;
	%B_1 = %this.B;
	%C_1 = %this.C;
	%A_2 = %b.A;
	%B_2 = %b.B;
	%C_2 = %b.C;
	%det = %A_1 * %B_2 - %A_2 * %B_1;
	if (mAbs(%det) < 0.1) return false;
	%x = (%B_2 * %C_1 - %B_1 * %C_2) / %det;
	%y = (%A_1 * %C_2 - %A_2 * %C_1) / %det;
	
	// worked in some leeway because floats
	%minX = mGetMax(%this.getMinX(), %b.getMinX()) - 0.1;
	%minY = mGetMax(%this.getMinY(), %b.getMinY()) - 0.1;
	%maxX = mGetMin(%this.getMaxX(), %b.getMaxX()) + 0.1;
	%maxY = mGetMin(%this.getMaxY(), %b.getMaxY()) + 0.1;
	return %x >= %minX && %x <= %maxX && %y >= %minY && %y <= %maxY;
}

function NavLine::getMinX(%this)
{
	%x1 = getWord(%this.p1, 0);
	%x2 = getWord(%this.p2, 0);
	return mGetMin(%x1, %x2);
}

function NavLine::getMaxX(%this)
{
	return mGetMax(getWord(%this.p1, 0), getWord(%this.p2, 0));
}

function NavLine::getMinY(%this)
{
	return mGetMin(getWord(%this.p1, 1), getWord(%this.p2, 1));
}

function NavLine::getMaxY(%this)
{
	return mGetMax(getWord(%this.p1, 1), getWord(%this.p2, 1));
}

function newNode(%pos)
{
	%node = new ScriptObject();
	%node.class = "NavNode";
	%node.pos = %pos;
	return %node;
}
