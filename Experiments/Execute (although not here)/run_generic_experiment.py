from experiment_running_utils import *

def main():
    """ Sets up and executes plans for all the wanted experiment data samples we want to collect. """

    """ Experiment setup and execution plan (can contain several experiments and run sequentially, but then you have to split up the dataset manually after it): """


    # Collecting data sample:
    # Hyperparameter / covariate assigning (currently just for homogenous robots).
    # Format for individual hyperparameters: either singleton homogenous values floats or ints, or lists of size |R| for each agent.                POTENTIAL SOURCE OF ERROR
    assignNonDefaultHyperparameters(collsize=5, nymoenrec=1, MAXTIMELIMIT=5, alphas=[0.7, 0.8, 03.23, 0.5, 0.3], ADJ_OMEGAS=[0,0,1,0,0], k_ss=0, d_ss=[6.4,6.5,6.6,6.7,6.8]) # collsize=6, nymoenrec=1
    # Datasample collecting / no_of_runs simulation runs being executed in parrallell.
    runNumberOfSimulationRuns(sampSize)


if __name__ == "__main__":
    """ Fully automatic experiments script (although homogenous and saving to one synchronyDataset.csv file). """

    main()