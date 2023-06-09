# VR_Kensyu
Multiplayer Sample using UnityVR and WebSockets  
Hand tracking support
Support for bHaptics tactile gloves

## Screenshot
<img src="https://user-images.githubusercontent.com/121733943/235363804-01b50f49-674e-40d4-a11e-39ed3ced5600.gif" width="512" height="512">  
<img src="https://github.com/TLabAltoh/VR_Kensyu/assets/121733943/73a9d223-436b-489b-9d47-78a38f38c70f" width="512" height="512">

## Getting Started
### Prerequisites
- Unity 2021.3.23f1  
- Oculus Integration (Install from asset store)  
- ProBuilder (Install from asset store)  
- node (v16.15.0)
- Android Logcat
- [bHaptics](https://assetstore.unity.com/packages/tools/integration/bhaptics-haptic-plugin-76647)
- [NativeWebsocket](https://github.com/endel/NativeWebSocket)  
### Installing
Clone the repository to any directory with the following command  
```
git clone https://github.com/TLabAltoh/VR_Kensyu.git
```
Execute the following commands in the cloned project (install necessary submodules)

```
git submodule init
git submodule update
```

### Sync Server Start
Set up a server to synchronize worlds
1. Execute the following commands from the Server/SyncServer/server.js
```
npm start
```
![image](https://github.com/TLabAltoh/VR_Kensyu/assets/121733943/41132a00-540c-4833-8b60-99348667f5cc)
2. Specify the address of the PC on which to start the server (port 5000)
3. Launch the game from UnityEditor or the built file

### How to play
#### Controller
- IndexTrigger: Select UI
- handTrigger: Manipulating objects in the scene (grip, expand)
#### HandTracking
- Pinch of index finger and thumb: Select UI
- Hand-holding gesture: Manipulating objects in the scene (grip, expand)

## Link
[TLabVRGrabber can be used on its own at the following link](https://github.com/TLabAltoh/TLabVRGrabber)  
[TLabVRPlayerController can be used on its own at the followint link](https://github.com/TLabAltoh/TLabVRPlayerController)

## TODO
- Supports voice chat
