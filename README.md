# The-Machine-to-be-Another-Unity
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bhttps%3A%2F%2Fgithub.com%2FBeAnotherLab%2FThe-Machine-to-be-Another-Unity.svg?type=shield)](https://app.fossa.io/projects/git%2Bhttps%3A%2F%2Fgithub.com%2FBeAnotherLab%2FThe-Machine-to-be-Another-Unity?ref=badge_shield)


This is the repository for all things body swap with The Machine to be Another. Three different setups are supported. Manual body swap, Auto body swap, and Servo swap

### Servo swap
![classic setup](https://github.com/BeAnotherLab/The-Machine-to-be-Another-Unity/blob/master/Files/one-way%20swap.png)
In this setup, the performer is copying the movements of the user, who is immersed in his body while listening to the story of the performer. This setup allows more freedom of movement for the user and more accuracy when copying movements.

Instructions on how to run the one-way swap can be found in [this pdf](https://github.com/BeAnotherLab/The-Machine-to-be-Another-Unity/blob/master/Files/The%20Machine%20to%20Be%20Another%20Protocols.pdf).

### Body swap
![two-way swap](https://github.com/BeAnotherLab/The-Machine-to-be-Another-Unity/blob/master/Files/gender%20swap%20mu.jpg?raw=true)
In this setup, usually shown in festivals as an installation both people are invited to synchronize with each other's movements. This is meant as a playful exploration of intimacy and empathy as explored in the original [gender swap](https://vimeo.com/84150219) video

Here is the equipment you need 
![tech rider for body swap](https://github.com/BeAnotherLab/The-Machine-to-be-Another-Unity/blob/master/Files/body%20swap%20rider.png?raw=true)

You can choose between Automatic swap (the software will automatically start and finish the experience, with audio instructions, based on some predefined timers and the detection of user presence) or Manual swap, where the whole experience is controlled from touchOSC.


- Install [Unity 2018.3.4f1] (https://unity3d.com/get-unity/download/archive)
- Install [Oculus Software](https://developer.oculus.com/downloads/)


- Clone this repository from GitHub: `git clone https://github.com/BeAnotherLab/The-Machine-to-be-Another-Unity.git`
- For the servo setup
- Install Arduino software
- Paste the ArduinoSerialCommand folder to your Arduino library folder
- Compile and upload the [Arduino control sketch](https://raw.githubusercontent.com/BeAnotherLab/The-Machine-to-be-Another-Unity/master/Arduino/ArduinoControl/ArduinoControl.ino) to the Arduino
- Open the project from Unity and run. Select the mode you want to use from the dropdown. When setting up the swap, enter the computer's IP in the input field and make sure only one of the two computers is set as repeater.
- All changes in settings are automatically saved  other 



## License
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bhttps%3A%2F%2Fgithub.com%2FBeAnotherLab%2FThe-Machine-to-be-Another-Unity.svg?type=large)](https://app.fossa.io/projects/git%2Bhttps%3A%2F%2Fgithub.com%2FBeAnotherLab%2FThe-Machine-to-be-Another-Unity?ref=badge_large)