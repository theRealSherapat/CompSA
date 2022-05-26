import sys
from experiment_running_utils import *

def main():
    """ Runs experiment with current hyperparamters no_of_runs times. """
    # BYTT TIL PHASE SYNC FREMVISNING:
    assignNonDefaultHyperparameters(collsize=15, USE_DETERMINISTIC_SEED=1, randomseed=20306, SIMMODE=0, alphas=0.05, betas=0)
    
    # BYTT TIL PHASE AND FREQ. SYNC FREMVISNING:
    # assignNonDefaultHyperparameters(collsize=15, SIMMODE=0, alphas=0.2, betas=0.7, ADJ_PHIS=1, ADJ_OMEGAS=1)

if __name__ == "__main__":
    """ Fully automatic experiment script (although homogenous and saving to one synchronyDataset.csv file). """
    
    main()