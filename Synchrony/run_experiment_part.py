import os
import sys

def main(runs):
    for run in range(runs):
        os.system("SynchronySimulatorRun.exe -batchmode -nographics")

if __name__ == "__main__":
    """ Argument 1: wanted simulator runs with fixed covariates (set in the editor before building) """
    wantedNumberOfRuns = int(sys.argv[1])
    main(wantedNumberOfRuns)