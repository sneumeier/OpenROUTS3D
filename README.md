### General Information

**OpenROUTS3D** (**Open** **R**ealtime **O**SM- and **U**nity-based **T**raffic **S**imulator **3D**) - A multi-purpose driving simulator developed for the needs of Teleoperated Driving.

![OpenROUTS3D logo](logo.png)

### Features
A rough overview of OpenROUTS3D and its features can be found at [IEEEXplore](https://ieeexplore.ieee.org/document/8965037)

- Map and Artificial Traffic Creation
- Input and Output management for Teloperated Driving
- Logging System and Replay Feature
- Simulation of Sensors
- Multiplayer
- User Study Mode
- Addon-System

### Installation + Execution

Clone the repository and import it into the Unity-Editor.
When errors occur, please clear the console and check if the packagemanager is included (Window -> Package Mangager).
If that's not the case, close the Unity-Editor and delete the file "manifest.json" in the directory packages in your project directory. 
Start the Unity-Editor again.

Now you should be able to Build and Run the software.

If you want to use the features of Simulation of Urban MObility ([SUMO](https://www.dlr.de/ts/en/desktopdefault.aspx/tabid-9883/16931_read-41000/)), install Version 1.5. After the installation you may need to **restart** your PC.

#### Input-Configuration

````
-Vertical = Gas + Brake combined
-Horizontal = Turn left or right
-Mouse X/Y = Look around while driving
-ButtonCancel = Button for pausing the game
-ButtonReverse = Button for activating reverse
-ButtonSkipScene = Button for skipping scenario
-ButtonHandBrake = Toggle Handbrake
-ButtonRespawn = Button for respawning the car
-ButtonLight = Toggle light On or Off
-ButtonIndicatorLeft = Toggle indicator left On or Off
-ButtonIndicatorRight = Toggle indicator right On or Off
````
Set axis and buttons of Controllers the correct way.
    
You can use e.g. the XBOX controller preset which you can load at Edit/Project Settings/Input Manager/Select Preset/"Choose your fitting preset or create a new one"


#### Fixes to known issues

**Error:**  TriangleMesh::loadFromDesc: desc.isValid() failed!
			UnityEngine.MeshCollider:set_sharedMesh(Mesh)

**Fix:** Use only maps that are known to work: Geisenfeld, Muehlhausen, Oberstimm or AmpelDemo.
Other maps might not be created correctly and need adjustment by hand.

**Error:** 	Exception: execution of SimStep() was not possible
       		Traci.TraciManager+SimulationControl.SimStep () (at Assets/Scripts/SUMOConnectionScripts/TraCI/SimulationControl.cs:37)
       		SUMOConnectionScripts.SumoUnityConnection.FixedUpdate () (at Assets/Scripts/SUMOConnectionScripts/SumoUnityConnection.cs:256)

**Fix:** SUMO is not installed or configured properly.
OpenROUTS3D should run without it, but will not display any remote vehicles or traffic lights.

### General Usage 
See Wiki for Tutorials on How-To use the Simulator (TBD).

### Authors
This software is developed by a Team as shown in the [AUTHORS](AUTHORS.md) file.

Development and Maintenance is coordinated by the [Car2X-Team](https://www.thi.de/en/research/carissma/laboratories/car2x-laboratory) at Technische Hochschule Ingolstadt.
(Project Maintainer: [Stefan Neumeier](https://www.thi.de/en/carissma/personnel/stefan-neumeier-msc))

### License
OpenROUTS3D is licensed under the terms of the GNU GPL; either version 3,
or (at your option) any later version.
See [LICENSE](LICENSE) file for more information.

### Citations
If using the simulator, please cite as:
````
@INPROCEEDINGS{2019OpenROUTS3D,
	author		=	{S. {Neumeier} and M. {H\oepp} and C. {Facchi}},
	booktitle	=	{2019 IEEE International Conference on Connected Vehicles and Expo (ICCVE)},
	title		=	{Yet Another Driving Simulator OpenROUTS3D: The Driving Simulator for Teleoperated Driving},
	year		=	{2019},
	pages		=	{1-6},
	address     =   {Graz, AUT},
	doi			=	{10.1109/ICCVE45908.2019.8965037},
	ISSN		=	{2378-1289},
	month		=	{Nov}
}
````

### Contribution
Contribution is easy.
If you identify Bugs or want some new cool Features, feel free to fix/implement it own your own and create a pull request.
You may also open an issue so that other developers are able to fix/implement it.

### Credits
See [Credits.xml](Credits.xml)

### External Libraries
- Triangle.Net (MIT-License): https://github.com/Geri-Borbas/Triangle.NET
- csDelaunay (MIT-License): https://github.com/PouletFrit/csDelaunay
