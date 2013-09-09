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
	
	FireAndIce.add( TamlRead("./gui/ConsoleDialog.gui.taml") );
	GlobalActionMap.bind( keyboard, "ctrl tilde", toggleConsole );
	
	%this.add( TamlRead("./gui/ArenaHud.gui.taml") );
	LifeIndicator.setValue(0.5);
	
	%this.add( TamlRead("./gui/LoseMenu.gui.taml") );
	%this.add( TamlRead("./gui/MainMenu.gui.taml") );
	
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
	
	%this.startMainMenu();
	//%this.startGame();
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
	Canvas.pushDialog( MainMenu );
	alxStopAll();
	alxPlay("FireAndIce:GameMusic");
	%this.turnSoundOn( $Game::soundOn );
}

//-----------------------------------------------------------------------------

function FireAndIce::startGame( %this )
{
	Canvas.popDialog( LoseMenu );
	Canvas.popDialog( MainMenu );
	Canvas.pushDialog(ArenaHud);

    // Finally, connect our scene into the viewport (or sceneWindow).
    // Note that a viewport comes with a camera built-in.
    mainWindow.setScene(mainScene);
    mainWindow.setCameraPosition( 0, 0 );
    mainWindow.setCameraSize( $Game::ScreenWidth, $Game::ScreenHeight );
	%viewRight = $Game::ArenaWidth / 2.0;
	%viewLeft = -%viewRight;
	%viewTop = $Game::ArenaHeight / 2.0;
	%viewLow = -%viewTop;
	mainWindow.setViewLimitOn( %viewLeft SPC %viewLow SPC %viewRight SPC %viewTop );

	mainScene.clear();
	mainScene.setScenePause( false );
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
	mainScene.setScenePause( true );
	cancel(SpawnManager.waveSpawn);
	Canvas.pushDialog(LoseMenu);
	LoseWaveTextLabel.setText("Game over. You lasted " SPC SpawnManager.waveNum SPC " waves.");
}

//-----------------------------------------------------------------------------

function FireAndIce::turnSoundOn( %this, %on )
{
	%onImg = "FireAndIce:speakerOnImage";
	%offImg = "FireAndIce:speakerOffImage";
	
	if ( %on )
	{
		alxSetChannelVolume(0, 1.0);
		SoundButton.setNormalImage( %onImg );
		SoundButton.setHoverImage( %offImg );
	}
	else
	{
		alxSetChannelVolume(0, 0.0);
		SoundButton.setNormalImage( %offImg );
		SoundButton.setHoverImage( %onImg );
	}
}

//-----------------------------------------------------------------------------

function FireAndIce::toggleSound( %this )
{
	echo( "toggle sound" );
	$Game::soundOn = !$Game::soundOn;
	%this.turnSoundOn( $Game::soundOn );
}