import sys
from experiment_running_utils import *

def main():
    """ Runs experiment with current hyperparamters no_of_runs times. """

    # Sync with Adj_{phi}:
    # assignNonDefaultHyperparameters(SIMMODE=1, USE_DETERMINISTIC_SEED=1, RANDOMSEED=48395, COLLSIZE=25, TREFDYN=0.5, ALPHAS=0.01, BETAS=0, ADJ_PHIS=1, ADJ_OMEGAS=0)
    
    # Sync with Adj_{phi} & Adj_{omega}:
    assignNonDefaultHyperparameters(SIMMODE=0, USE_DETERMINISTIC_SEED=1, RANDOMSEED=1999, COLLSIZE=25, TREFDYN=0.05, ALPHAS=0.1, BETAS=0.8, ADJ_PHIS=1, ADJ_OMEGAS=1, SAMPLERATE=65)

if __name__ == "__main__":
    """ Fully automatic experiment script (although homogenous and saving to one synchronyDataset.csv file). """
    
    main()