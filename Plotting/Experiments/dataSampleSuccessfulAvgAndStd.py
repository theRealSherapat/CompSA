import sys
import matplotlib.pyplot as plt
import numpy as np
import csv

from plottingUtils import parseDataFrom

def main(filepath):
    filepathBinary = parseDataFrom(filepath)
    terminationTimes = filepathBinary[1:,0] # I think I slice from row index 1 since I still have the header or something...
    successScores = filepathBinary[1:,1]
    
    synchronizationTimes = terminationTimes[successScores==1]

    if synchronizationTimes.size == 0:
        print("'-'. Sorry, this type / configuration of musical robot collectives did not once (out of 30 independent simulation runs) achieve harmonic synchrony during the max time limit of 5 minutes.")
    else:
        print(filepath + " average:",np.average(filepathBinary))
        print(filepath + " standard deviation:",np.std(filepathBinary))


if __name__ == "__main__":
    """ A Python script taking in a synchronyDataset.csv data sample, and outputs (currently successful) terminationTimes a.k.a. synchronizationTimes, in terms of average simulation time (s) and std (s). """

    # Args: 1) dataset.csv filepath

    filepath = sys.argv[1]

    main(filepath)