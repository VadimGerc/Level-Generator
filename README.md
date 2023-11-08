# Endless Level Generator
Generates an endless level with obstacles (using blue noise algorithm).

## Overview

![ezgif com-optimize](https://github.com/VadimGerc/Level-Generator/assets/116275237/aca30966-f570-4407-87b6-feea2ad51300)

## How generation works

### Chunks Generation

- When the character comes within a certain distance to the edge of the level, new chunks are generated.
- When the character moves away from the far chunks, they are disabled (to save the generated level and not to affect performance).  

  **Drawing distance** can be customized in the **LevelGenerator** script

  <img width="372" alt="Screenshot 2023-11-08 at 18 56 52" src="https://github.com/VadimGerc/Level-Generator/assets/116275237/f936ba82-e756-49c9-bf8b-caa7d9f78377">


### Obstacles Generation

When a new chunk is spawned, obstacles are generated on it using the **blue noise algorithm**. The chunk is divided into several virtual segments, in each of which an object can be generated (at a random point). This allows for more predictable (but still random) generation of objects in the scene and not worry that they will be generated close to each other. [Here](https://youtu.be/lRfdN4L2SUg?si=bEgZ_MQGM4DY_cV4) you'll find more info about the blue noise algorithm.

<img width="600" alt="Screenshot 2023-11-08 at 17 45 07" src="https://github.com/VadimGerc/Level-Generator/assets/116275237/9ddfe739-cbc5-4e93-8f7a-3e5548293b36">

<br />
<br />

You can customize the generation parameters for each chunk separately in the **ChunkObject** script.

<img width="373" alt="Screenshot 2023-11-08 at 19 13 50" src="https://github.com/VadimGerc/Level-Generator/assets/116275237/ed8f7c0a-8514-41e7-85c2-c29f4ebd8496">

## Assets 

Character - https://assetstore.unity.com/packages/3d/characters/meshtint-free-boximon-fiery-mega-toon-series-153958
<br/>
Obstacles - https://assetstore.unity.com/packages/3d/props/polygon-starter-pack-low-poly-3d-art-by-synty-156819
