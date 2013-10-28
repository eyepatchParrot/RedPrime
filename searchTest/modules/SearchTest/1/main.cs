//-----------------------------------------------------------------------------
// Copyright (c) 2013 GarageGames, LLC
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to
// deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
// sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//-----------------------------------------------------------------------------

function SearchTest::create( %this )
{
	exec("./gui/guiprofiles.cs");
	exec("./scripts/console.cs");
	%this.add( TamlRead("./gui/ConsoleDialog.gui.taml") );
	%this.add( TamlRead("./gui/ModeHud.gui.taml") );
	GlobalActionMap.bind( keyboard, "ctrl tilde", toggleConsole );

    // We need a main "Scene" we can use as our game world.  The place where sceneObjects play.
    // Give it a global name "mainScene" since we may want to access it directly in our scripts.
    new Scene(mainScene);

    // Without a system window or "Canvas", we can't see or interact with our scene.
    // AppCore initialized the Canvas already

    // Now that we have a Canvas, we need a viewport into the scene.
    // Give it a global name "mainWindow" since we may want to access it directly in our scripts.
    new SceneWindow(mainWindow);
    mainWindow.profile = new GuiControlProfile();
    Canvas.setContent(mainWindow);
	
	Canvas.pushDialog(ModeHud);

    // Finally, connect our scene into the viewport (or sceneWindow).
    // Note that a viewport comes with a camera built-in.
    mainWindow.setScene(mainScene);
    mainWindow.setCameraPosition( 0, 0 );
    mainWindow.setCameraSize( 100, 75 );
	
	exec("./scripts/consts.cs");
	
	new ScriptObject(InputManager);
	mainWindow.addInputListener(InputManager);
	exec("./scripts/controls.cs");
	
	exec("./scripts/navmap.cs");
	exec("./scripts/utility.cs");

    // load some scripts and variables
    // exec("./scripts/someScript.cs");

    // let's do a little something to make sure we are up and running.
    // write "hello world!"  :)
    %this.sayHello();
	
	%this.updateMode();
}

//-----------------------------------------------------------------------------

function SearchTest::destroy( %this )
{
}

//-----------------------------------------------------------------------------


function SearchTest::sayHello( %this )
{
    %phrase = new ImageFont();
    %phrase.Image = "SearchTest:Font";
       
    // Set the font size in both axis.  This is in world-units and not typicaly font "points".
    %phrase.FontSize = "2 2";
    
    %phrase.TextAlignment = "Center";
    %phrase.Text = "Hello, World!";
    mainScene.add( %phrase );

	%a = new ScriptObject();
	%b = new ScriptObject();
	%a.v = 5;
	%b.v = 2;
	%a.n = %b;
	%b.n = %a;
	
	echo("a.v" SPC %a.v SPC "b.v" SPC %b.v);
	%a.n.v = 3;
	%a.n.n.v = 7;
	echo("a.v" SPC %a.v SPC "b.v" SPC %b.v);
}

//-----------------------------------------------------------------------------

function SearchTest::updateMode( %this )
{
	switch ($Game::Mode) {
	case $SET_START_NODE:
		%modeText = "Place start node";
		
	case $SET_END_NODE:
		%modeText = "Place end node";
		
	case $ADD_RECT:
		%modeText = "Place new rect";
		
	case $SELECT_RECT:
		%modeText = "Select rect";
		
	case $DEL_RECT:
		%modeText = "Delete last rect";
	}

	ModeText.setText("Mode :" SPC %modeText);
}

//-----------------------------------------------------------------------------

function SearchTest::setStartNode(%this, %pos)
{
	if (!isObject(%this.map) || !isObject(%this.map.getQuadAt(%pos))) {
		return;
	}
	
	if (!isObject(%this.startNode)) {
		%size = 2;
		%obj = new ShapeVector();
		%obj.setSize(%size);
		%obj.setLineColor("0 0 0 1");
		%obj.setFillColor("0 1 0 1");
		%obj.setFillMode(true);
		%obj.setIsCircle(true);
		%obj.setCircleRadius(%size);
	
		mainScene.add( %obj );
		%this.startNode = %obj;
	}
	%this.startNode.setPosition(%pos);
	
	if (isObject(%this.endNode)) {
		%this.calculatePath();
	}
}

//-----------------------------------------------------------------------------

function SearchTest::setEndNode(%this, %pos)
{
	if (!isObject(%this.map) || !isObject(%this.map.getQuadAt(%pos))) {
		return;
	}
	
	if (!isObject(%this.endNode)) {
		%size = 2;
		%obj = new ShapeVector();
		%obj.setSize(%size);
		%obj.setLineColor("0 0 0 1");
		%obj.setFillColor("1 0 0 1");
		%obj.setFillMode(true);
		%obj.setIsCircle(true);
		%obj.setCircleRadius(%size);
		
		mainScene.add( %obj );
		%this.endNode = %obj;
	}
	%this.endNode.setPosition( %pos );
	
	if (isObject(%this.startNode)) {
		%this.calculatePath();
	}
}

//-----------------------------------------------------------------------------

function SearchTest::addRect(%this, %pos)
{
	if (!isObject(%this.map)) {
		%this.map = newMap();
	}
	
	%size = 20.0;
	
	if (%this.map.isEmpty()) {
		%xW = getWord(%pos, 0) - %size / 2.0;
		%xE = getWord(%pos, 0) + %size / 2.0;
		%yN = getWord(%pos, 1) + %size / 2.0;
		%yS = getWord(%pos, 1) - %size / 2.0;
		%pNW = %xW SPC %yN;
		%pNE = %xE SPC %yN;
		%pSW = %xW SPC %yS;
		%pSE = %xE SPC %yS;
		echo("pNW" SPC %pNW SPC "pNE" SPC %pNE SPC "pSW" SPC %pSW SPC "pSE" SPC %pSE);
		%this.map.initAt(%pNW, %pNE, %pSW, %pSE);
		echo("Num quads :" SPC %this.map.numQuads());
		echo("Num nodes :" SPC %this.map.numNodes());
//		echo("Num Edges :" SPC %this.map.nodes.getObject(0).getEdges().getCount());
		$selectedQuad = %this.map.rootQuad;
	} else {
		echo("* extending *");
		%this.map.extendTo(%pos, $selectedQuad);
		echo("Num quads :" SPC %this.map.numQuads());
		echo("Num nodes :" SPC %this.map.numNodes());
//		echo("Num Edges :" SPC %this.map.nodes.getObject(0).getEdges().getCount());
	}

	%this.map.draw();
}

//-----------------------------------------------------------------------------

function SearchTest::selectRect(%this, %pos)
{
	%newSelection = %this.map.getQuadAt(%pos);
	if (isObject(%newSelection)) {
		$selectedQuad = %newSelection;
	}
	%this.map.draw();
}

//-----------------------------------------------------------------------------

function SearchTest::calculatePath(%this)
{
	if (!isObject(%this.startNode) || !isObject(%this.endNode)) {
		return;
	}
	
	if (!isObject(%this.drawObjs)) {
		%this.drawObjs = new SimSet();
	} else {
		%this.drawObjs.deleteObjects();
	}
	
	%startNode = newNode(%this.startNode.getPosition());
	%endNode = newNode(%this.endNode.getPosition());
	%closedNodes = new SimSet();
	%openNodes = new SimSet();
	
	echo("angle: " SPC Vector2AngleToPoint(%startNode.pos, %endNode.pos));
	
	// F = G + H
	%openNodes.add(%startNode);
	%startNode.G = 0;
	%startNode.F = %startNode.G + getH(%startNode, %endNode);
	
	while (%openNodes.getCount() > 0 && !%closedNodes.isMember(%endNode)) {
		echo("numOpenNodes :" SPC %openNodes.getCount());
		%n = findCheapestNode(%openNodes);
		echo("n" SPC %n.pos);
		%closedNodes.add(%n);
		%openNodes.remove(%n);
		
		%neighbors = %this.getNeighbors(%n, %endNode);
		echo("numNeighbors :" SPC %neighbors.getCount());
		for (%i = 0; %i < %neighbors.getCount(); %i++) {
			%neighbor = %neighbors.getObject(%i);
			%g = %n.G + getMoveCost(%n, %neighbor);
			if (%openNodes.isMember(%neighbor) && %g < %neighbor.G) {
				%openNodes.remove(%neighbor);
			}
			if (%closedNodes.isMember(%neighbor) && %g < %neighbor.G) {
				%closedNodes.remove(%neighbor);
			}
			if (!%openNodes.isMember(%neighbor) && !%closedNodes.isMember(%neighbor)) {
				%neighbor.G = %g;
				%neighbor.F = %neighbor.G + getH(%neighbor, %endNode);
				echo("neighbor" SPC %neighbor.pos SPC "g" SPC %g SPC "f" SPC %neighbor.F);
				%neighbor.parent = %n;
				%openNodes.add(%neighbor);
			}
		}
	}
	
	echo("num open nodes :" SPC %openNodes.getCount() SPC "end node is member :" SPC %closedNodes.isMember(%endNode));
	
	%nodePath = new SimSet();
	%n = %endNode;
	%nodePath.add(%n);
	while (isObject(%n.parent)) {
		echo("add node" SPC %n.pos);
		%nodePath.add(%n.parent);
		%n = %n.parent;
	}
	
	reverseSimSet(%nodePath);
	
	%this.drawNodePath(%nodePath);
}

function SearchTest::getNeighbors(%this, %from, %endNode)
{
	%neighbors = %this.map.getNeighbors(%from);
	if (%this.map.isVisibleFrom(%from, %endNode)) %neighbors.add(%endNode);
	
	return %neighbors;
}	

function SearchTest::drawNodePath(%this, %nodePath)
{
	for (%nodeIdx = 0; %nodeIdx + 1 < %nodePath.getCount(); %nodeIdx++) {
		%a = %nodePath.getObject(%nodeIdx);
		%b = %nodePath.getObject(%nodeIdx + 1);
		if (%nodeIdx != 0) {
			%obj = newCircle(%a.pos);
			%obj.setFillMode(true);
			%obj.setSize(2);
			%this.drawObjs.add(%obj);
		}
		%dist = distTo(%a.pos, %b.pos);
		%angle = mDegToRad(Vector2AngleToPoint(%a.pos, %b.pos) - 90.0);
		echo("dist :" SPC %dist SPC "angle :" SPC %angle);
		for (%i = 1; %i * %i < %dist; %i++) {
			%objPos = projectPos(%a.pos, %angle, %i);
			%obj = newCircle(%objPos);
			mainScene.add(%obj);
			%this.drawObjs.add(%obj);
		}
	}
}
