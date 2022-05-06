import sys
import numpy as np

def main(binaryArrayPairs):
    for binayPairIndex, binaryPair in enumerate(binaryArrayPairs):    
        terminationTimes = binaryPair[0]
        successScores = binaryPair[1]
        
        synchronizationTimes = terminationTimes[successScores==1]

        if synchronizationTimes.size == 0:
            print("'-'. Sorry, this type / configuration of musical robot collectives did not once (out of 30 independent simulation runs) achieve harmonic synchrony during the max time limit of 5 minutes.")
        else:
            print("Binary " + str(binayPairIndex) + "'s average (currently) synchronization time (sim s):", round(np.average(synchronizationTimes), 1))
            print("Binary " + str(binayPairIndex) + "'s standard deviation (sim s):", round(np.std(synchronizationTimes), 1))


if __name__ == "__main__":
    """ Args: 1) (int) number of the first no_of_datasamples we want to calculate and print average synchronization time (currently successful termination times) (sim s) and corresponding standard deviation for. """

    no_of_datasamples = int(sys.argv[1])
    
    binaryArrayPairs = []
    
    for binaryDatasampleIndex in range(no_of_datasamples):
        binaryTerminationTimes = np.load("ConvertedBinaries/dataSampleBinary_simTimes_" + str(binaryDatasampleIndex) + ".npy")
        binarySuccesses = np.load("ConvertedBinaries/dataSampleBinary_successes_" + str(binaryDatasampleIndex) + ".npy")
        binaryArrayPairs.append([binaryTerminationTimes, binarySuccesses]) # kan sikkert zippe

    main(binaryArrayPairs)