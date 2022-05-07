from experiment_running_utils import *

def main():
    """ Sets up and executes plans for all the wanted experiment data samples we want to collect. """
    
    """ Experiment setup and execution plan: """
    
	
	# Collecting data sample:
	# Hyperparameter / covariate assigning (currently just for homogenous robots).
	assignNonDefaultHyperparameters() # collsize=6, nymoenrec=1, phaseadj=0, beta=0, freqadj=0
	# Datasample collecting / no_of_runs simulation runs being executed in parrallell.
	runNumberOfSimulationRuns(sampSize)


if __name__ == "__main__":
    """ Fully automatic experiment script (although homogenous and saving to one synchronyDataset.csv file). """
    
    main()