from experiment_running_utils import *

def main():
    """ Sets up and executes plans for all the wanted experiment data samples we want to collect. """
    
    """ Experiment setup and execution plan (can contain several experiments and run sequentially, but then you have to split up the dataset manually after it): """
    
    collsizes = [2, 3, 6, 25, 50, 100, 200, 500, 1000]
	
    # FOR MIROLLO-STROGATZ's PHASE ADJ. METHOD:
    for collsize_val in collsizes:
        # Collecting data sample:
        # Hyperparameter / covariate assigning (currently just for homogenous robots).
        assignNonDefaultHyperparameters(collsize=collsize_val, alpha=0.8, beta=0, nymoenrec=0, trefperc=0.1, phaseadj=0, beta=0, freqadj=0)
        # Datasample collecting / no_of_runs simulation runs being executed in parrallell.
        runNumberOfSimulationRuns(sampSize)
     
     # FOR NYMOEN's PHASE ADJ. METHOD:
     for collsize_val in collsizes:
        # Collecting data sample:
        # Hyperparameter / covariate assigning (currently just for homogenous robots).
        assignNonDefaultHyperparameters(collsize=collsize_val, alpha=0.8, beta=0, nymoenrec=0, trefperc=0.1, phaseadj=1, beta=0, freqadj=0)
        # Datasample collecting / no_of_runs simulation runs being executed in parrallell.
        runNumberOfSimulationRuns(sampSize)


if __name__ == "__main__":
    """ Fully automatic experiments script (although homogenous and saving to one synchronyDataset.csv file). """
    
    main()