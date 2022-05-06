import sys
from experiment_running_utils import *

def main(no_of_runs):
    """ Runs experiment with current hyperparamters no_of_runs times. """
    
    runNumberOfSimulationRuns(no_of_runs)
    
    print(str(no_of_runs) + " synchrony simulation run(s) should have finished successfully.")

if __name__ == "__main__":
    """ Fully automatic experiment script (although homogenous and saving to one synchronyDataset.csv file). """
    no_of_runs = int(sys.argv[1])
    
    main(no_of_runs)