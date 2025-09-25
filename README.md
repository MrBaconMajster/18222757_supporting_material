# 18222757 Masters Project Supporting Material

---

## Comparative Analysis of Reinforcement Learning and Imitation Learning: Evaluating learning rates and processing time across varied task complexities.

---
This is a repository submitted as a part of the Masters Project by student 18222757.

Files inside final training results folder can be previewed using the jupyter notebook called Result_Data_Visualisation. These files are the results of training agents using PPO, GAIL, and PPO+GAIL algorithms.

The jupyter notebook Demonstration_Data_Visualisation is used to showcase the data recorded that was used for imitation learning(GAIL).

---
# Environments

## Grid World (Easiest)

The grid world problem is an easy game where the agent must move through a
grid to get to a specific point on the game board while avoiding a bad marker on the grid.

 <img width="330" height="301" alt="grid" src="https://github.com/user-attachments/assets/c88fd995-3cd1-45cf-90ec-f791e283e0df" />

| Observation Space | Action Space | Reward Structure |
| ------------- | ------- | ----- |
|  Int Agent_x, Agent_z | Do nothing | Agent moves onto the target: +1 |
| Int Target_x, Target_z | Move up 1 grid space | Agent moves onto the enemy: -1 |
| Int Enemy_x, Enemy_z | Move down 1 grid space |  Every step the agent receives: -0.1 |
|  |  Move left 1 grid space | |
|  |  Move right 1 grid space | |


---
## Escape Room (Easy)

To complete this task, the agent must perform a few tasks in the correct order to succeed. 

1. Initially the agent must locate the enemy which can appear anywhere inside
the environment. The enemy entity constantly moves towards the agent and if
the agent collides with the enemy the episode ends, and a penalty is applied.
The agent must approach the enemy and use an action to attack it within range
before it makes physical contact.

3. After the enemy is attacked it drops a key entity. The agent has to use a
different pickup action within range in order to collect the key.

5. After the key is picked up, the agent has to exit the environment using the
door

<img width="887" height="268" alt="top" src="https://github.com/user-attachments/assets/9cfab273-ae81-4318-801a-da48c42acae3" />



| Observation Space | Action Space | Reward Structure |
| ------------- | ------- | ----- |
| 11x Boolean[5]: containing sensor information  <br> of what objects were hit and not hit  | Do nothing  | Agent collides with enemy: -0.4 |
| 11x Float: containing sensor information  <br> of the distance to the hit object | Move forward: Done by applying forward velocity  | Agent attacks enemy: +0.15 |
|Boolean KeyObtained: informs the agent if <br> it is currently holding the key. | Move backward: Done by applying backward velocity  | Agent picks up key: 0.15 |
|  | Rotate left: Rotates left by 1° | Agent enters the door while holding the key: +1 |
|  | Rotate right: Rotates right by 1°  | Every environment step: -1/3000 |
|  | Attack: Attacks a small area ahead of the agent | If enemy is present: <br>• Once enemy becomes visible: +0.1 <br>• Once enemy becomes obscured: -0.1 |
|  | Pick up: Attempts to pick up key in a small area ahead of the agent  | If key is obtained: (last distance - new distance) / starting distance |


---

## 3D Space Shooter (Medium)

To complete this task, the agent must perform the following tasks.
1. Avoid collision with walls, as collision immediately ends the episode and
applies a penalty.
2. Attack all agents present in the environment.
3. Exit through the door after all the enemies are cleared

<img width="919" height="390" alt="space" src="https://github.com/user-attachments/assets/717d6baa-c115-400c-9692-27f3fcaf697c" />

| Observation Space | Action Space | Reward Structure |
| ------------- | ------- | ----- |
| 45x Boolean[4]: containing sensor information   | Do nothing | Agent collides with wall: -0.5 |
|45x Float: containing sensor information of the distance to the hit object | Attack: Attacks using a ray-cast ahead of the agent within range | Agent attacks an enemy: 0.2 |
|Int EnemyCount: informs the agent of the number of enemies left in the stage | Adjust Speed: sets the current speed of the agent to any value between [0.02, 0.30] units  | Agent exits using the door after all enemies are cleared: 1 |
| Float velocityX, velocityY, velocityZ: informs the agent of its current velocity in all directions. |  Rotate: Rotate the agent around the y axis by any value between [-1, 1] ° | Once agent is facing an enemy: 0.05 |
|  | Tilt: Tilt the agent around the x axis by any value between [-1, 1] ° with a maximum tilt of 80° and -80° | Once agent loses sight of an enemy: -0.05 |
| | | Every environment step: -1/5000 |

---

## Crawler (Hard)

The goal of this agent is to move towards a target without falling while maintaining a
desired speed and keeping its head pointed towards the target. The agent is physics-based so in
order to move the agent must learn to coordinate 20 different joint rotations in its body. 

<img width="518" height="355" alt="crawler" src="https://github.com/user-attachments/assets/269b32f6-761e-4eb4-83b9-00809bc9a328" />

| Observation Space | Action Space | Reward Structure |
| ------------- | ------- | ----- |
| 172x Float: the variables are responsible for storing the position, rotation, velocity and angular velocities of each limb with additional variables for the acceleration and angular acceleration of the body  | Set lower arm target x_rotation | Agent collides with target: 1 |
| 11x Float: containing sensor information  <br> of the distance to the hit object | Set upper arm target x and y_rotation | Every environment step: ((Agent velocity normalised to [0,1]) * ((HeadAlignment with target) normalised [0,1]) |
|Boolean KeyObtained: informs the agent if <br> it is currently holding the key. | Set the strength of the arm rotation action  |  |

---
# Results

<img width="750" height="521" alt="image" src="https://github.com/user-attachments/assets/d5391c53-b4d8-420d-85b4-c6203d71b368" />

<img width="750" height="600" alt="image" src="https://github.com/user-attachments/assets/a45c7b82-cd7d-434f-b950-72b0b7ae29a7" />



<img width="598" height="208" alt="image" src="https://github.com/user-attachments/assets/03dcba6e-35e5-4ca8-9da0-a88a72c86c17" />

<img width="617" height="599" alt="image" src="https://github.com/user-attachments/assets/391a9114-c0c3-449a-b8c6-3d55c179c212" />



---

Code used to create the environments and control the agents can be found in the Environments folder.
To fully open and use the code used in creating the environments it has to be opened inside Unity Enigne 2023.
This can be done by creating a new Unity project and replacing the "Assets" folder generated by unity with the "Assets" folder inside this repository. I was required to reduce the size of the files uploaded and this is why this workaround is required.

To preview the scripts used for controlling and rewarding the agents you can navigate to Environments -> (Pick any environment) -> Assets -> Scripts

