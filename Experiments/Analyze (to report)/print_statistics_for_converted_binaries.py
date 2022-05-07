import os
import sys
import numpy as np

def main(binaryArrayPairs):
    for binayPairIndex, binaryPair in enumerate(binaryArrayPairs):    
        terminationTimes = binaryPair[0]
        successScores = binaryPair[1]
        
        # synchronizationTimes = terminationTimes[successScores==1] # old slicing

        if terminationTimes.size == 0:
            print("\nFor datasample #" + str(binayPairIndex) + " with " + str(len(terminationTimes)) + " datapoints:")
            print("Average termination time (sim s) = NaN.")
            print("Standard deviation (sim s) = NaN.")
        else:
            print("\nFor datasample #" + str(binayPairIndex) + " with " + str(len(terminationTimes)) + " datapoints:")
            print("Average termination time (sim s) = " + str(round(np.average(terminationTimes), 1)))
            print("Standard deviation (sim s) = " + str(round(np.std(terminationTimes), 1)))


def countFilesInFolderPath(path):
    filesInPath = 0
    
    for files in os.walk(path):
        filesInPath = len(files[2])


    return int(filesInPath/2)
    
    

if __name__ == "__main__":
    """ Calculating and printing average synchronization time (currently successful termination times) (sim s) and corresponding standard deviation for all converted binaries (corresponding to individual data samples). """

    no_of_datasamples = countFilesInFolderPath("ConvertedBinaries/")
    
    binaryArrayPairs = []
    
    for binaryDatasampleIndex in range(no_of_datasamples):
        binaryTerminationTimes = np.load("ConvertedBinaries/dataSampleBinary_terminationTimes_" + str(binaryDatasampleIndex) + ".npy")
        binarySuccesses = np.load("ConvertedBinaries/dataSampleBinary_successes_" + str(binaryDatasampleIndex) + ".npy")
        binaryArrayPairs.append([binaryTerminationTimes, binarySuccesses]) # kan sikkert zippe

    main(binaryArrayPairs)