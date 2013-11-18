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

function FireAndIce::create( %this )
{
	%this.loadPreferences();

	exec("./gui/guiprofiles.cs");
	exec("./scripts/console.cs");

    // load some scripts and variables
    // exec("./scripts/someScript.cs");
	exec("./scripts/arena.cs");
	exec("./scripts/playerCharacter.cs");
	exec("./scripts/thugs.cs");
	exec("./scripts/bullet.cs");
	exec("./scripts/controls.cs");
	exec("./scripts/spawnZone.cs");
	exec("./scripts/hud.cs");
	exec("./scripts/shooterControlsBehavior.cs");
	exec("./scripts/moveTowardBehavior.cs");
	exec("./scripts/faceMouseBehavior.cs");
	exec("./scripts/moveAnimationBehavior.cs");
	exec("./scripts/dropPickupBehavior.cs");
	exec("./scripts/aStarBehavior.cs");
	exec("./scripts/navmap.cs");
	exec("./scripts/navquad.cs");
	exec("./scripts/utility.cs");
	
	FireAndIce.add( TamlRead("./gui/ConsoleDialog.gui.taml") );
	GlobalActionMap.bind( keyboard, "ctrl tilde", toggleConsole );
	
	%this.add( TamlRead("./gui/ArenaHud.gui.taml") );
	%this.add( TamlRead("./gui/LoseMenu.gui.taml") );
	%this.add( TamlRead("./gui/MainMenu.gui.taml") );
	%this.add( TamlRead("./gui/InfoMenu.gui.taml") );
	
	// We need a main "Scene" we can use as our game world.  The place where sceneObjects play.
    // Give it a global name "mainScene" since we may want to access it directly in our scripts.
    new Scene(mainScene);
	// mainScene.setDebugOn("collision aabb"); // aabb

    // Without a system window or "Canvas", we can't see or interact with our scene.
    // AppCore initialized the Canvas already
	
	// Now that we have a Canvas, we need a viewport into the scene.
    // Give it a global name "mainWindow" since we may want to access it directly in our scripts.
    new SceneWindow(mainWindow);
    mainWindow.profile = GuiDefaultProfile;
    Canvas.setContent(mainWindow);
	
	new ScriptObject(InputManager);
	
	%this.turnSoundOn( $Game::soundOn );
	
	%this.startMainMenu();
	// %this.startGame();
}

//-----------------------------------------------------------------------------

function FireAndIce::destroy( %this )
{
	InputManager.delete();
	alxStopAll();
}

//-----------------------------------------------------------------------------

function FireAndIce::loadPreferences( %this )
{
	exec("./preferences.cs");
	
	%res = $pref::Video::windowedRes;
	%width = getWord(%res, 0);
	%height = getWord(%res, 1);
	%bpp = getWord(%res, 2);
	%fullscreen = false;
	%screenModeSuccess = setScreenMode( %width, %height, %bpp, %fullscreen );
}

//-----------------------------------------------------------------------------

function FireAndIce::startMainMenu( %this )
{
	%this.clearDialogs();
	Canvas.pushDialog( MainMenu );
	alxStopAll();
	alxPlay("FireAndIce:GameMusic");
	%this.turnSoundOn( $Game::soundOn );
}

//-----------------------------------------------------------------------------

function FireAndIce::startGame( %this )
{
	%this.clearDialogs();
	Canvas.pushDialog(ArenaHud);

    // Finally, connect our scene into the viewport (or sceneWindow).
    // Note that a viewport comes with a camera built-in.
    mainWindow.setScene(mainScene);
    mainWindow.setCameraPosition( 0, 0 );
    mainWindow.setCameraSize( $Game::ScreenWidth * 2, $Game::ScreenHeight * 2 );
	%viewRight = $Game::ArenaWidth / 2.0;
	%viewLeft = -%viewRight;
	%viewTop = $Game::ArenaHeight / 2.0;
	%viewLow = -%viewTop;
	// mainWindow.setViewLimitOn( %viewLeft SPC %viewLow SPC %viewRight SPC %viewTop );
	
	$Game::Kills = 0;

	mainScene.clear();
	mainScene.setScenePause( false );
	%this.setNavMap();
	createArena();
	createSpawnZones();
	createPlayerCharacter();
	updateHud();
	
	mainWindow.mount(PlayerCharacter);
	
	mainWindow.addInputListener(InputManager);
	
	alxStopAll();
	alxPlay("FireAndIce:GameMusic");
	
}

//-----------------------------------------------------------------------------

function FireAndIce::startLoseMenu( %this )
{
	%this.clearDialogs();
	Canvas.pushDialog(LoseMenu);
	LoseStatsTextLabel.setText("You survived" SPC SpawnManager.waveNum SPC "waves, and slaughtered" SPC $Game::Kills SPC "of the ice horde.");
	
	if (!isObject($ranks)) {
		$ranks = new SimSet();
	}
	
	$ranks.add(newRank(getLocalTime(), SpawnManager.waveNum, $Game::Kills));
	sortByKills($ranks);
	
	echo("ranks" SPC $ranks.getCount());
	
	for (%i = 0; %i < 8; %i++) {
		switch (%i) {
		case 0:
			%label = Rank1Label;
		
		case 1:
			%label = Rank2Label;
		
		case 2:
			%label = Rank3Label;
			
		case 3:
			%label = Rank4Label;
			
		case 4:
			%label = Rank5Label;
			
		case 5:
			%label = Rank6Label;
			
		case 6:
			%label = Rank7Label;
			
		case 7:
			%label = Rank8Label;
		}
		
		if ( %i < $ranks.getCount()) {
			%r = $ranks.getObject(%i);
			%labelText = (%i+1) @ ". " @ %r.date SPC "-" SPC %r.kills SPC "kills - wave" SPC %r.wave;
		} else {
			%labelText = "";
		}
		%label.setText(%labelText);
	}
	mainScene.clear();
}

function sortByKills(%set)
{
	%startIdx = 0;
	while (!isSorted(%set)) {
		%p = %set.getObject(0);
		for (%i = 1; %i < %set.getCount(); %i++) {
			%r = %set.getObject(%i);
			if (%r.kills > %p.kills) {
				%set.reOrderChild(%r, %p);
			} else {
				%p = %r;
			}
		}
	}
}

function isSorted(%set)
{
	%last = %set.getObject(0).kills;
	for (%i = 1; %i < %set.getCount(); %i++) {
		%r = %set.getObject(%i);
		if (%r.kills > %last) return false;
		%last = %r.kills;
	}
	return true;
}

function newRank(%date, %wave, %kills)
{
	%r = new ScriptObject();
	%r.date = %date;
	%r.wave = %wave;
	%r.kills = %kills;
	return %r;
}

//-----------------------------------------------------------------------------

function FireAndIce::startInfoMenu( %this )
{
	%this.clearDialogs();
	Canvas.pushDialog(InfoMenu);
}

//-----------------------------------------------------------------------------

function FireAndIce::clearDialogs( %this )
{
	Canvas.popDialog(LoseMenu);
	Canvas.popDialog(MainMenu);
	Canvas.popDialog(InfoMenu);
	Canvas.popDialog(ArenaHud);
}

//-----------------------------------------------------------------------------

function FireAndIce::turnSoundOn( %this, %on )
{
	%onImg = "FireAndIce:speakerOnImage";
	%offImg = "FireAndIce:speakerOffImage";
	
	if ( %on )
	{
		alxSetChannelVolume(0, 1.0);
		alxSetChannelVolume(1, 1.0);
		SoundButton.setNormalImage( %onImg );
		SoundButton.setHoverImage( %onImg );
		SoundButton.setDownImage( %onImg );
	}
	else
	{
		alxSetChannelVolume(0, 0.0);
		alxSetChannelVolume(1, 0.0);
		SoundButton.setNormalImage( %offImg );
		SoundButton.setHoverImage( %offImg );
		SoundButton.setDownImage( %offImg );
	}
	
	$Game::soundOn = %on;
}

//-----------------------------------------------------------------------------

function FireAndIce::toggleSound( %this )
{
	%this.turnSoundOn( !$Game::soundOn );
}

//-----------------------------------------------------------------------------

function FireAndIce::setNavMap( %this )
{
	%halfArenaWidth = $Game::ArenaWidth / 2.0;
	%zoneX = $Game::ArenaWidth / 2.0 + $Game::ZoneSize / 2.0;
	%zoneY = $Game::ArenaHeight / 2.0 + $Game::ZoneSize / 2.0;
	%this.navMap = newMap();
	
	%y1 = %zoneY + 1;
	%y2 = %y1 - 3.25;
	%y2_2 = 6;
	%y3 = 3.25;
	%y4 = 0.5;
	%y4_2 = -1;
	%y5 = -3;
	%y6 = -5.5;
	%y7 = -%zoneY - 1;
	
	%x1 = -%zoneX - 1;
	%x2 = -%halfArenaWidth / 2.0;
	%x3 = -7;
	%x4 = -3;
	%x5 = 1;
	%x6 = 2.75;
	%x7 = 3.5;
	%x8 = 6;
	%x9 = 8;
	%x10 = %zoneX + 1;
	
	%left1 = %this.navMap.initAt(%x1 SPC %y1, %x2 SPC %y1, %x1 SPC %y2, %x2 SPC %y2);
	
	%x = (%x1 + %x2) / 2.0;
	%y = %y3;
	%left2 = %this.navMap.extendTo(%x SPC %y, %left1);
	
	%y = %y4;
	%left3 = %this.navMap.extendTo(%x SPC %y, %left2);
	
	%y = %y5;
	%left4 = %this.navMap.extendTo(%x SPC %y, %left3);
	
	%y = %y6;
	%left5 = %this.navMap.extendTo(%x SPC %y, %left4);
	
	%y = %y7;
	%left6 = %this.navMap.extendTo(%x SPC %y, %left5);
	
	// **********
	// * x = x3 *
	// **********
	%x = %x3;
	%y = (%y4 + %y5) / 2.0;
	%left4 = %this.navMap.extendTo(%x SPC %y, %left4);
	
	// **********
	// * x = x4 *
	// **********
	%x = %x4;
	%y = (%y3 + %y4) / 2.0;
	%left3 = %this.navMap.extendTo(%x SPC %y, %left3);
	// %left3.s = %left4;
	
	%y = (%y5 + %y6) / 2.0;
	%left5 = %this.navMap.extendTo(%x SPC %y, %left5);
	// %left5.n = %left4;
	
	// %left4.n = %left3;
	// %left4.s = %left5;
	
	// **********
	// * x = x5 *
	// **********
	%x = %x5;
	%y = (%y1 + %y2) / 2.0;
	%left1 = %this.navMap.extendTo(%x SPC %y, %left1);
	
	%y = (%y3 + %y4) / 2.0;
	%left3 = %this.navMap.extendTo(%x SPC %y, %left3);
	
	%y = (%y5 + %y6) / 2.0;
	%left5 = %this.navMap.extendTo(%x SPC %y, %left5);
	
	%left4 = %this.navMap.connect(%left3, %left5);
	
	// **********
	// * x = x6 *
	// **********
	%x = %x6;
	%y = (%y1 + %y2) / 2.0;
	%left1 = %this.navMap.extendTo(%x SPC %y, %left1);
	
	%y = (%y3 + %y4) / 2.0;
	%left3 = %this.navMap.extendTo(%x SPC %y, %left3);
	
	%left2 = %this.navMap.connect(%left1, %left3);
	
	// **********
	// * x = x7 *
	// **********
	%x = %x7;
	%y = (%y3 + %y4) / 2.0;
	%left3 = %this.navMap.extendTo(%x SPC %y, %left3);
	
	%y = (%y4 + %y5) / 2.0;
	%left4 = %this.navMap.extendTo(%x SPC %y, %left4);
	
	%y = (%y5 + %y6) / 2.0;
	%left5 = %this.navMap.extendTo(%x SPC %y, %left5);
	
	// %left3.s = %left4;
	// %left4.n = %left3;
	// %left4.s = %left5;
	// %left5.n = %left4;
	
	// **********
	// * x = x8 *
	// **********
	%x = %x8;
	%y = (%y1 + %y2) / 2.0;
	%left1 = %this.navMap.extendTo(%x SPC %y, %left1);
	
	%y = (%y3 + %y4) / 2.0;
	%left3 = %this.navMap.extendTo(%x SPC %y, %left3);
	
	%x = (%x7 + %x8) / 2.0;
	%y = %y2_2;
	%left1_2 = %this.navMap.extendTo(%x SPC %y, %left1);
	
	%y = %y4_2;
	%left3_2 = %this.navMap.extendTo(%x SPC %y, %left3);
	
	// %left1_2.w = %left2;
	// %left2.e = %left1_2;
	// %left3_2.w = %left4;
	
	// **********
	// * x = x9 *
	// **********
	%x = %x9;
	%y = (%y6 + %y7) / 2.0;
	%left6 = %this.navMap.extendTo(%x SPC %y, %left6);
	
	%y = (%y1 + %y2) / 2.0;
	%left1 = %this.navMap.extendTo(%x SPC %y, %left1);
	
	%y = (%y4 + %y4_2) / 2.0;
	%left4 = %this.navMap.extendTo(%x SPC %y, %left3_2);
	
	%left2 = %this.navMap.connect(%left1, %left4);
	// %left2.w = %left3;
	
	// %left5.s = %left6;
	// %left5.w.s = %left6;
	// %left5.w.w.s = %left6;
	// %left6.n = %left5.w;
	
	// ***********
	// * x = x10 *
	// ***********
	%x = %x10;
	%y = (%y1 + %y2) / 2.0;
	%left1 = %this.navMap.extendTo(%x SPC %y, %left1);
	
	%y = (%y6 + %y7) / 2.0;
	%left6 = %this.navMap.extendTo(%x SPC %y, %left6);
	
	%left5 = %this.navMap.connect(%left1, %left6);
	
	// %left5.w = %left3;
	// %left2.e = %left5;
	// %left3.e = %left5;
	// %left4.e = %left5;
	
	%this.navMap.draw();
}