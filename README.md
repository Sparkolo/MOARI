# MOARI

This Unity project was made as an exam project for the course Modern AI at the IT University of Copenhagen by [Alberto Giudice](mailto:algi@itu.dk), [Andrea Trevisin](mailto:atre@itu.dk) and [Fabio Scita](mailto:fasc@itu.dk).

The goal of this project is to train and run a simulation of carnivore and herbivore simple organisms (cells) to survive in a 2D petri dish environment. The cells can observe the environment around them though a series of sensors (rays) and can act by moving forward at (a fraction of) a maximum speed or rotate left or right (at a fraction of) a maximum rotation speed. The environment provides a certain amount of plant-based and meat-based food pellets that replenish as consumed, while herbivore cells also count as food for carnivore cells.

## Installation and Usage

- The project is built using Unity 2020.3.23f1 and Unity ML-Agents 2.0.1
- To train the cell agents in the training scenes and to visualize the final training results data in the  `results` folder with `tensorboard`, you first need to install the `mlagents` python package following [Unity's guide](https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Installation.md#install-the-mlagents-python-package) (using a virtual environment is advised)

Once setup this way, the project contains 4 scenes:

* `POCA` allows to train/test the agents with the `Multi Agent POsthumos Credit Assignment` algorithm. To test the results achieved, simply run the scene. To train from scratch, simply remove the `Model` from the `Behavior Parameters` of the `POCACarnivore` and the `POCAVegan` agent prefabs and train with the config file `poca_final.yaml`
* `PPO` allows to train/test the agents with the `Proximal Policy Optimization` algorithm. To test the results achieved, simply run the scene as it is for carnivores, or disable the active GameObject and enable the inactive one for herbivores. To train from scratch, simply remove the `Model` from the `Behavior Parameters` of the `PPOCarnivore` and the `PPOVegan` agent prefabs and train with the config file `ppo_carnivore_final.yaml`/`ppo_vegan_final.yaml`
* `SAC` allows to train/test the agents with the `Soft Actor Critic` algorithm. To test the results achieved, simply run the scene as it is for carnivores, or disable the active GameObject and enable the inactive one for herbivores. To train from scratch, simply remove the `Model` from the `Behavior Parameters` of the `SACCarnivore` and the `SACVegan` agent prefabs and train with the config file `sac_carnivore_final.yaml`/`sac_vegan_final.yaml`
* `BattleRoyale` allows to test all the resulting final models from each algorithm for herbivores and carnivores against each other. Simply run the scene and it will run simulations for each combination, saving some comparison data about cells survival for each simulation in JSON files inside the `Assets/Comparisons` folder. You can modify the `Petri Tray Battle` prefab to change the number of simulations to run for each combination (default: 100) and the number of steps of each simulation (default: 10'000), as well as the amount of food to be instantiated in the environment in each simulation (default: 300 plant pellets, 100 meat pellets)
