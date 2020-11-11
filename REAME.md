## Apollo: Blitz the Moon

Apollo is an endless runner game, in which you pilot a Hovership across the surface of the moon.

Apollo is built on **Unity**.

## Build Status

In progress.

## Features

Apollo features a fully procedurally-generated world, and includes a system for terrain generation, terrain object population and a curved world shader.

Apollo also features a Hovercraft physics system, with tweakable parameters and four different kinds of movement.

## Installation

You would need to install Unity to run this project. It is built on **Unity 2020.1.4f1**, though other versions of Unity should be able to open the project as well (untested). Directly downloading and unzipping the repository should allow Unity to open it as a new project.

The working folder is `Assets`. Here, you can find both the game assets used as well as the scripts that the game runs on. Here is a breakdown of the folders:

- The `Scripts` folder is where the C# scripts containing the game logic can be found. This is where most of the work will be done.
- The `Editor Scripts` folder contains the C# scripts used to edit the Unity Editor for easy tweaking and debugging.
- The `Terrain Assets` folder houses the Scriptable Object assets that hold the settings for terrain generation.
- The `Material` folder contains the materials used for the terrain, while `Resources` contains the materials used for the "Rocks and Boulders 2" prefabs.
- The `Scenes` folder contains the Scene objects. At the moment only one Scene (Playstate) is used.
- The `Standard Assets` folder contains the prefab for a first-person controller and associated scripts. It is used for debugging only.

The following folders contain free assets from the Unity Asset Store:

- `AllSkyFree`
- `Rocks and Boulders 2`
- `Vehicles`

## Credits

Resources used:

- **Procedural Terrain Generation (Video Series)** by *Sebastian Lague*
- **Poisson Disc Sampling Video** by *Sebastian Lague*
- **Hover Racer Demo (Unite Austin 2017)** by *Unity*

Assets Used:

- **AllSkyFree** by *RPGWHITELOCK*
- **Rocks and Boulders 2** by *MANUFACTURA K4*
- **Dieselpunk Hovercraft 01 PBR** by *ALEXANDER Z*
- **Star Sparrow Modular Spaceship** by *EBAL STUDIOS*
- **Free SF Fighter** by *CGPITBULL*

## License

This project is licensed by the GNU Affero General Public License v3.0.

GNU © Nicholas Ho
