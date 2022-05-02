import sys
import matplotlib.pyplot as plt
import numpy as np
from matplotlib.pyplot import figure

# figure(figsize=((8,3)), dpi=300)

def main(binaryArrays):

    errorRatesInPercentages = []
    
    for binaryArray in binaryArrays:
        successfulRuns = np.sum(binaryArray)
        totalRuns = len(binaryArray)
        errorPercentage = (1 - successfulRuns / totalRuns) * 100
        errorRatesInPercentages.append(errorPercentage)
    
    # timesStacked = np.load(binPaths[0]).reshape((30,1))
    # # Loading performance-scores/-data:
    # for binPathInd in range(1, len(binPaths)):
        # # timesStacked = np.stack((timesStacked,np.load(binPaths[binPathInd]).reshape((30,1))), axis=1)
        # timesStacked = np.concatenate((timesStacked, np.load(binPaths[binPathInd]).reshape((timesStacked.shape[0],1))), axis=1)
    
    # Plotting stacked data
    plt.bar(["0.1", "0.2", "0.3", "0.4", "0.5", "0.6", "0.7", "0.8", "0.9"], errorRatesInPercentages) # Kan ha med whis=(0,100) for å få whiskerne til å dekke hele data-samplet (til og med outliersa).
    plt.ylabel("error rate (%)")
    plt.xlabel("α")
    plt.savefig("experiment_errorRates.svg", dpi=300)
    plt.show()


if __name__ == "__main__":
    # Args: 1) (string) binary1.npy filepath, 2) (string) binary2.npy filepath
    
    binaryPathStart = sys.argv[1]
    
    binaryArrays = []
    
    for alphaValue in range(1,10):
        binaryArrays.append(np.load(binaryPathStart + "0p" + str(alphaValue) + "_successes.npy"))

    main(binaryArrays)