// NavQuad
// public:
// %nw, %ne, %sw, %se // nodes
// %n, %e, %w, %s     // adjacent quads
//
// newNavQuad(%nNW, %nNE, %nSW, %nSE) %quad
// newNavQuadEastOf(%prevQuad, %pos) %quad
// newNavQuadWestOf(%prevQuad, %pos) %quad
// newNavQuadNorthOf(%prevQuad, %pos) %quad
// newNavQuadSouthOf(%prevQuad, %pos) %quad
// NavQuad::contains(%this, %pos) %doesContain
// NavQuad::containsNode(%this, %node) %doesContain
// NavQuad;:getQuads(%this, %quads) %quads
// NavQuad::distToSq(%this, %pos) %distToSq
// NavQuad::posIsWest(%this, %pos) %isWest
// NavQuad::posIsNorth(%this, %pos) %isNorth
// NavQuad::posIsEast(%this, %pos) %isEast
// NavQuad::posIsSouth(%this, %pos) %isSouth
// NavQuad::getCenterX(%this) %centerX
// NavQuad::getCenterY(%this) %centerY
// NavQuad::getW(%this) %w
// NavQuad::getH(%this) %h

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

function newNavQuadEastOf(%prevQuad, %pos)
{
	%x = getWord(%pos, 0);
	%yN = getWord(%prevQuad.ne.pos, 1);
	%yS = getWord(%prevQuad.se.pos, 1);
	%nNW = %prevQuad.ne;
	%nNE = newNode(%x SPC %yN);
	%nSW = %prevQuad.se;
	%nSE = newNode(%x SPC %yS);
	%q = newNavQuad(%nNW, %nNE, %nSW, %nSE);
	%q.w = %prevQuad;
	%prevQuad.e = %q;
	return %q;
}

function newNavQuadWestOf(%prevQuad, %pos)
{
	%x = getWord(%pos, 0);
	%yN = getWord(%prevQuad.nw.pos, 1);
	%yS = getWord(%prevQuad.sw.pos, 1);
	%nNW = newNode(%x SPC %yN);
	%nNE = %prevQuad.nw;
	%nSW = newNode(%x SPC %yS);
	%nSE = %prevQuad.sw;
	%q = newNavQuad(%nNW, %nNE, %nSW, %nSE);
	%q.e = %prevQuad;
	%prevQuad.w = %q;
}

function newNavQuadNorthOf(%prevQuad, %pos)
{
	%xW = getWord(%prevQuad.nw.pos, 0);
	%xE = getWord(%prevQuad.ne.pos, 0);
	%y = getWord(%pos, 1);
	%nNW = newNode(%xW SPC %y);
	%nNE = newNode(%xE SPC %y);
	%nSW = %prevQuad.nw;
	%nSE = %prevQuad.ne;
	%q = newNavQuad(%nNW, %nNE, %nSW, %nSE);
	%q.s = %prevQuad;
	%prevQuad.n = %q;
}

function newNavQuadSouthOf(%prevQuad, %pos)
{
	%xW = getWord(%prevQuad.sw.pos, 0);
	%xE = getWord(%prevQuad.se.pos, 0);
	%y = getWord(%pos, 1);
	%nNW = %prevQuad.sw;
	%nNE = %prevQuad.se;
	%nSW = newNode(%xW SPC %y);
	%nSE = newNode(%xE SPC %y);
	%q = newNavQuad(%nNW, %nNE, %nSW, %nSE);
	%q.n = %prevQuad;
	%prevQuad.s = %q;
}

// ** TODO: Make work for all quadrilaterals rather than just rectangles
function NavQuad::contains(%this, %pos)
{
	%xW = getWord(%this.nw.pos, 0);
	%xE = getWord(%this.ne.pos, 0);
	%yN = getWord(%this.nw.pos, 1);
	%yS = getWord(%this.sw.pos, 1);
	%x = getWord(%pos, 0);
	%y = getWord(%pos, 1);
	return isBetween(%x, %xW, %xE) && isBetween(%y, %yN, %yS);
}

function NavQuad::containsNode(%this, %node)
{
	return %node == %this.nw || %node == %this.ne || %node == %this.sw || %node == %this.se || %this.contains(%node.pos);
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

function NavQuad::distToSq(%this, %pos)
{
	return distToSq(%this.getCenterX() SPC %this.getCenterY(), %pos);
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

function NavQuad::getCenterX(%this)
{
	%xNW = getWord(%this.nw.pos, 0);
	%xSW = getWord(%this.sw.pos, 0);
	return mGetMin(%xNW, %xSW) + %this.getW() / 2.0;
}

function NavQuad::getCenterY(%this)
{
	%ySW = getWord(%this.sw.pos, 1);
	%ySE = getWord(%this.se.pos, 1);
	return mGetMin(%ySW, %ySE) + %this.getH() / 2.0;
}

function NavQuad::getW(%this)
{
	%xNE = getWord(%this.ne.pos, 0);
	%xNW = getWord(%this.nw.pos, 0);
	%xSW = getWord(%this.sw.pos, 0);
	%xSE = getWord(%this.se.pos, 0);
	return mGetMax(%xNE, %xSE) - mGetMin(%xNW, %xSW);
}

function NavQuad::getH(%this)
{
	%yNW = getWord(%this.nw.pos, 1);
	%yNE = getWord(%this.ne.pos, 1);
	%ySW = getWord(%this.sw.pos, 1);
	%ySE = getWord(%this.se.pos, 1);
	return mGetMax(%yNW, %yNE) - mGetMin(%ySW, %ySE);
}