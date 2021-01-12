## Apollo: Blitz the Moon

Apollo is an endless runner game, in which you pilot a Hovership across the surface of the moon. Avoid the rocks and get as far as you can!

Check out the gameplay right here: https://youtu.be/XvhCE7lx-ao

![Gameplay](README_Images/Gameplay.png)

Apollo is built on **Unity**.

## Controls

Movement: `A` and `D` (or the left and right arrow keys)

Pause: `Esc`, `Space`, `Enter` or `P`

Quit: `Esc` (on Title and Score screens only), "*Quit*" Button in Pause Menu (Play screen only)

## Build Status

This public version is **completed**.

A close-sourced version of Apollo is still in the works. However, this repository will no longer be updated with the latest code. 

## Features

Apollo features a **fully procedurally-generated game world** that you can traverse through on your hovership. It is based on Sebastian Lague's *Procedural Terrain Generation* video series, but has been adapted to include the following:

- Optimizations for an Endless Runner (Terrain Object pooling, not updating chunks behind vehicle)
- Two Additional Types of Noise (Ridged and Billow)
- A Terrain Object Population System (modified from Sebastian Lague's *Poisson Disc Sampling* video to allow multi-radii outputs)
- A Curved World Shader

Apollo also features a **Hovercraft physics system**. It is based on Unity's *Hover Racer Demo*, modified to include the following:

- Four kinds of movement (Free Strafing, Free Turning, Strafing for Endless Runner, Turning for Endless Runner)
- Additional tweakable parameters (eg. Responsiveness to ground rotation, turn responsiveness)

## Installation

You would need to install Unity to run this project. It is built on **Unity 2020.1.4f1**, though other versions of Unity should be able to open the project as well (untested). Directly downloading and unzipping the repository should allow Unity to open it as a new project. It runs on the **Built-in Render Pipeline**.

The working folder is `Assets`. Here, you can find both the game assets used as well as the scripts that the game runs on. Here is a breakdown of the folders:

- The `Scripts` folder is where the C# scripts containing the game logic can be found. This is where most of the work will be done.
- The `Editor Scripts` folder contains the C# scripts used to edit the Unity Editor for easy tweaking and debugging.
- The `Terrain Assets` folder houses the Scriptable Object assets that hold the settings for terrain generation.
- The `Material` folder contains the materials used for the terrain, while `Resources` contains the materials used for the "Rocks and Boulders 2" prefabs.
- The `Scenes` folder contains the Scene objects. There are three Scenes: the Start State, Play State and Score State.
- The `Fonts` folder contains the fonts used in the game. All fonts come from the *Lato* family of fonts.
- The `UI` folder contains any assets or images required for the UI. At the moment it only contains a White Screen PNG.
- The `Standard Assets` folder contains the prefab for a first-person controller and associated scripts. It is used for debugging only.

The following folders contain free assets from the Unity Asset Store:

- `Environment Assets`
- `Vehicles`
- `Effects`

The `Sound` folder contains Royalty-Free effects and music used in the project.

## Credits

Resources used:

- **Procedural Terrain Generation (Video Series)** by *Sebastian Lague*
- **Poisson Disc Sampling Video** by *Sebastian Lague*
- **Hover Racer Demo (Unite Austin 2017)** by *Unity*

Assets Used:

- **AllSkyFree** by *RPGWHITELOCK*
- **Starfield Skybox** by *PULSAR BYTES*
- **Rocks and Boulders 2** by *MANUFACTURA K4*
- **Dieselpunk Hovercraft 01 PBR** by *ALEXANDER Z*
- **Star Sparrow Modular Spaceship** by *EBAL STUDIOS*

Sound Effects and Music Used (Royalty-Free):

- **Cyber Dream Loop** by *Soundimage.org*
- **The Darkness Below** by *Soundimage.org*
- **Explosion - Fireball** by *Zapsplat*

## License

This project is licensed by the GNU Affero General Public License v3.0.

GNU © Nicholas Ho
