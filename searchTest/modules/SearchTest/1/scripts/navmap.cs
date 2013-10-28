function newMap()
{
	%map = new ScriptObject();
	%map.class = "NavMap";
	return %map;
}

function NavMap::initAt(%this, %pNW, %pNE, %pSW, %pSE)
{
	%nodeNW = newNode(%pNW);
	%nodeNE = newNode(%pNE);
	%nodeSW = newNode(%pSW);
	%nodeSE = newNode(%pSE);
	
	%this.rootQuad = newNavQuad(%nodeNW, %nodeNE, %nodeSW, %nodeSE);
}

function NavMap::extendTo(%this, %pos)
{
	%prevQuad = %this.nearestQuad(%pos);
	if (%prevQuad.posIsEast(%pos)) {
		%x = getWord(%pos, 0);
		%yN = getWord(%prevQuad.ne, 1);
		%yS = getWord(%prevQuad.se, 1);
		%nNW = %prevQuad.ne;
		%nNE = newNode(%x SPC %yN);
		%nSW = %prevQuad.se;
		%nSE = newNode(%x SPC %yS);
		%q = newNavQuad(%nNW, %nNE, %nSW, %nSE); 
		%prevQuad.e = %q;
	} else if (%prevQuad.posIsWest(%pos)) {
		%x = getWord(%pos, 0);
		%yN = getWord(%prevQuad.nw, 1);
		%yS = getWord(%prevQuad.sw, 1);
		%nNW = newNode(%x SPC %yN);
		%nNE = %prevQuad.nw;
		%nSW = newNode(%x SPC %yS);
		%nSE = %prevQuad.sw;
		%q = newNavQuad(%nNW, %nNE, %nSW, %nSE);
		%prevQuad.w = %q;
	} else if (%prevQuad.posIsNorth(%pos)) {
		%xW = getWord(%prevQuad.nw, 0);
		%xE = getWord(%prevQuad.ne, 0);
		%y = getWord(%pos, 1);
		%nNW = newNode(%xW SPC %y);
		%nNE = newNode(%xE SPC %y);
		%nSW = %prevQuad.nw;
		%nSE = %prevQuad.ne;
		%q = newNavQuad(%nNW, %nNE, %nSW, %nSE);
		%prevQuad.n = %q;
	} else if (%prevQuad.posIsSouth(%pos)) {
		%xW = getWord(%prevQuad.sw, 0);
		%xE = getWord(%prevQuad.se, 0);
		%y = getWord(%pos, 1);
		%nNW = %prevQuad.sw;
		%nNE = %prevQuad.se;
		%nSW = newNode(%xW SPC %y);
		%nSE = newNode(%xE SPC %y);
		%q = newNavQuad(%nNW, %nNE, %nSW, %nSE);
		%prevQuad.s = %q;
	}
}

function NavMap::isEmpty(this)
{
	return isObject(%this.rootQuad);
}

function NavMap::nearestQuad(%this, %pos)
{
	%quads = %this.getQuads();
	if (%quads.getCount() > 0) {
		%minQuad = %quads.getObject(0);
		%minDist = %minQuad.distTo(%pos);
		for (%i = 1; %i < %quads.getCount(); %i++) {
			%q = %quads.getObject(%i);
			%qDist = %q.distTo(%pos);
			if (%qDist < %minDist) {
				%minQuad = %q;
				%minDist = %qDist;
			}
		}
	}
	return %minQuad;
}

function NavMap::numNodes(%this)
{
	return %this.getNodes().getCount();
}

function NavMap::getNodes(%this)
{
	%quads = %this.getQuads();
	%nodes = new SimSet();
	for (%i = 0; %i < %quads.getCount(); %i++) {
		%q = %quads.getObject(%i);
		if (isNewNode(%q.nw, %nodes)) %nodes.add(%q.nw);
		if (isNewNode(%q.ne, %nodes)) %nodes.add(%q.ne);
		if (isNewNode(%q.sw, %nodes)) %nodes.add(%q.sw);
		if (isNewNode(%q.se, %nodes)) %nodes.add(%q.se);
	}
	return %nodes;
}

function isNewNode(%n, %set)
{
	return isObject(%n) && !%set.isMember(%n);
}

function NavMap::numQuads(%this)
{
	%quads = %this.getQuads();
	return %quads.getCount();
}

function NavMap::getQuads(%this)
{
	return %this.rootQuad.getQuads();
}

function newNavQuad(%nNW, %nNE, %nSW, %nSE)
{
	%quad = new ScriptObject();
	%quad.class = "NavQuad";
	%quad.nw = %nNW;
	%quad.ne = %nNE;
	%quad.sw = %nSW;
	%quad.se = %nSE;
	return %quad;
}

function NavQuad::getQuads(%this, %quads)
{
	if (!isObject(%quads)) {
		%quads = new SimSet();
	}
	
	if (!%quads.isMember(%this)) {
		%quads.add(%this);
		if (isObject(%this.n)) %this.n.getQuads(%quads);
		if (isObject(%this.e)) %this.e.getQuads(%quads);
		if (isObject(%this.s)) %this.s.getQuads(%quads);
		if (isObject(%this.w)) %this.w.getQuads(%quads);
	}
	return %quads;
}

function NavQuad::distTo(%this, %pos)
{
	%x = (getWord(%this.nw, 0) + getWord(%this.ne, 0) + getWord(%this.sw, 0) + getWord(%this.se, 0)) / 4.0;
	%y = (getWord(%this.nw, 1) + getWord(%this.ne, 1) + getWord(%this.sw, 1) + getWord(%this.se, 1)) / 4.0;
	%xDist = getWord(%pos, 0) - %x;
	%yDist = getWord(%pos, 1) - %y;
	return %xDist * %xDist + %yDist * %yDist;
}

function NavQuad::posIsWest(%this, %pos)
{
	%x = getWord(%pos, 0);
	%y = getWord(%pos, 1);
	%yN = getWord(%this.nw.pos, 1);
	%yS = getWord(%this.sw.pos, 1);
	%pX = projectX(%y, %this.nw.pos, %this.sw.pos);
	return isBetween(%y, %yN, %yS) && %x < %pX;
}

function NavQuad::posIsEast(%this, %pos)
{
	%x = getWord(%pos, 0);
	%y = getWord(%pos, 1);
	%yN = getWord(%this.ne.pos, 1);
	%yS = getWord(%this.se.pos, 1);
	%pX = projectX(%y, %this.ne.pos, %this.se.pos);
	return isBetween(%y, %yN, %yS) && %x > %pX;
}

function NavQuad::posIsNorth(%this, %pos)
{
	%x = getWord(%pos, 0);
	%y = getWord(%pos, 1);
	%xW = getWord(%this.nw.pos, 0);
	%xE = getWord(%this.ne.pos, 0);
	%pY = projectY(%x, %this.nw.pos, %this.ne.pos);
	return isBetween(%x, %xW, %xE) && %y > %pY;
}

function NavQuad::posIsSouth(%this, %pos)
{
	%x = getWord(%pos, 0);
	%y = getWord(%pos, 1);
	%xW = getWord(%this.sw.pos, 0);
	%xE = getWord(%this.se.pos, 0);
	%pY = projectY(%x, %this.sw.pos, %this.se.pos);
	return isBetween(%x, %xW, %xE) && %y < %pY;
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

function newNode(%pos)
{
	%node = new ScriptObject();
	%node.class = "NavNode";
	%node.pos = %pos;
	return %node;
}