# AR in the Car
This project is an immersive augmented reality car game for car passengers. It uses YOLO level to detect cars, and other traffic objects on the road and tasks the player to control and UFOO doging in between the cars to collect coins, and increase their score.

File structure:
- All important game files can be found in Assets/KartAndTracj_Scripts with the exception of CollisionResponderInterface.cs and CollisionManager.cs which can be found
in Assets/YOLOTools/YOLO
- The audio files used can be found in Assets/Kart_game_Audio
- The 3D models can be found in Assets/Models

### Requirements

- [Git LFS](https://git-lfs.com/)
- [Unity 6000.0.20f1](https://unity.com/releases/editor/whats-new/6000.0.20#installs) with Android Build Support
- Windows
- Packages:
    - "com.meta.xr.sdk.all": "77.0.0",
    - "com.unity.nuget.newtonsoft-json": "3.2.1",
    - "com.unity.xr.oculus": "4.2.0",
    - "com.unity.xr.interaction.toolkit": "3.0.8",
    - "com.unity.sentis": "2.1.3",
    - "com.unity.xr.arfoundation": "6.0.3"
  - Necessary packages will be installed automatically by the Unity Editor


### Build Intructions

1. Clone the repository:
    ```sh
    git clone https://github.com/JoePaechter/Diss-Car-Game
    ```
2. Open the project in Unity:
    - Launch Unity Hub.
    - Click on "Add" and select the cloned project folder.
    - Open the project.
    - Dependencies will be installed automatically

3. Build for Android from the Build Profile menu:
    - Select File > Build Profile
    - Select Android
    - Ensure that the DissCarGame scence is selected in the Scene List
    - Select Build
    - To Successfully Build you Must have a Meta Quest Three headset, in develepor mode, connected to your PC when you build the game
    - After a succesfful build you can open the game on your headset, the file will be under unkown sources



## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.


## Attribution
"YOLOQuestUnity" (https://github.com/matthewlyon23/YOLOQuestUnity) by Matthen Lyone is licensed under the MIT Licence (https://opensource.org/license/mit)
Lyon's YOLOQuestUnity project's source code provided the working YOLO pipeline on the Meta Quest 3, needed ot implement my game


"Coin" (https://sketchfab.com/3d-models/coin-dfa5d2e83f9f4cb4a53fe6f109f3dbb8) by Folly is licensed under Creative Commons Attribution (http://creativecommons.org/licenses/by/4.0/).

"UFO" (https://sketchfab.com/3d-models/ufo-2c3b4613db884585938318e1d10361d3) by rowan11 is licensed under Creative Commons Attribution (http://creativecommons.org/licenses/by/4.0/).

"coin recieved" (https://pixabay.com/sound-effects/film-special-effects-coin-recieved-230517/) by RibhavAgrawal is licensed under Pixabay's Content License (https://pixabay.com/service/license-summary/)

"Sound Effect - Car Crash" (https://pixabay.com/sound-effects/film-special-effects-sound-effect-car-crash-394903/) by u_mgq59j5ayf is licensed under Pixabay's Content License (https://pixabay.com/service/license-summary/)

