# Drone Landing Agent
This project involved using Unity's MLAgents package in training a reinforcement 
learning agent to land on a target. In this environment, the agent is given access
to information on its position, linear velocity, angular velocity, and its orientation. 
The external constant force of gravity and variable force of wind are applied to 
the drone each frame, neither of which the drone has information on. 

# Instructions for project setup

1. Follow the [setup guide](https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Installation.md) for an MLAgents environment. Follow the advanced guide of local 
installation for development.
1. Clone this repo and load it with Unity Hub. Select Unity version 2019.4 or later.
1. Select the "DroneTesting" scene to view the pretrained behavior.