from experiment_running_utils import *

def main():
    """ Sets up and executes plans for all the wanted experiment data samples we want to collect. """
    
    """ Experiment 4: Recreating Nymoen's last results setup and execution plan: """
    
    betas = [0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9]
    for beta_val in betas:
        # Hyperparameter / covariate assigning (currently just for homogenous robots).
        assignNonDefaultHyperparameters(beta=beta_val,  collsize=6, nymoenrec=1, alpha=0.2, phaseadj=1, freqadj=1)
        
        # Datasample collecting / no_of_runs simulation runs being executed in parrallell.
        runNumberOfSimulationRuns(sampSize)


if __name__ == "__main__":
    main()