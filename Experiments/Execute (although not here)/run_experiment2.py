from experiment_running_utils import *

def main():
    """ Sets up and executes plans for all the wanted experiment data samples we want to collect. """
    
    """ Experiment setup and execution plan: """
    
    alphas = [0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9]
    for alpha_val in alphas:
        # Collecting one data sample per Alpha:
        # Hyperparameter / covariate assigning (currently just for homogenous robots).
        assignNonDefaultHyperparameters(alpha=alpha_val,  collsize=6, nymoenrec=1, phaseadj=1, beta=0.4, freqadj=1)
        # Datasample collecting / no_of_runs simulation runs being executed in parrallell.
        runNumberOfSimulationRuns(sampSize)


if __name__ == "__main__":
    """ Fully automatic experiment script (although homogenous and saving to one synchronyDataset.csv file). """
    
    main()