from experiment_running_utils import *

def main():
    """ Sets up and executes plans for all the wanted experiment data samples we want to collect. """
    
    """ Experiment setup and execution plan: """
    
    collsizes = [3, 10, 25, 50]
    t_ref_dyn_vals = [0.03, 0.05, 0.1, 0.5]
    alphas = [0.001, 0.01, 0.1, 0.2, 0.4, 0.8]
    
    for collsize_val in collsizes:
        for t_ref_dyn_val in t_ref_dyn_vals:
            for alpha_val in alphas:
                # Collecting one data sample per Alpha and t_ref_dyn value pair:
                # Hyperparameter / covariate assigning (currently just for homogenous robots).
                assignNonDefaultHyperparameters(collsize=collsize_val, trefperc=t_ref_dyn_val, alpha=alpha_val, phaseadj=0, beta=0, freqadj=0)
                # Datasample collecting / no_of_runs simulation runs being executed in parrallell.
                runNumberOfSimulationRuns(sampSize)


if __name__ == "__main__":
    """ Fully automatic experiment script (although homogenous and saving to one synchronyDataset.csv file). """
    
    main()