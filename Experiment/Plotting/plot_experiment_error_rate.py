import sys
import matplotlib.pyplot as plt
import numpy as np
from matplotlib.pyplot import figure
from matplotlib import rcParams

labelSize = 16
rcParams['xtick.labelsize'] = labelSize
rcParams['ytick.labelsize'] = labelSize

# figure(figsize=((8,3)), dpi=300)

def main(binaryArrays):

    errorRatesInPercentages = []
    
    for binaryArray in binaryArrays:
        successfulRuns = np.sum(binaryArray)
        totalRuns = len(binaryArray)
        errorPercentage = (1 - successfulRuns / totalRuns) * 100
        errorRatesInPercentages.append(errorPercentage)
    
    plt.bar(["0.1", "0.2", "0.3", "0.4", "0.5", "0.6", "0.7", "0.8", "0.9"], errorRatesInPercentages) # Kan ha med whis=(0,100) for å få whiskerne til å dekke hele data-samplet (til og med outliersa).
    plt.ylabel("error rate (%)", fontsize=16)
    plt.xlabel("α", fontsize=16)
    plt.savefig("experiment_errorRates.svg", dpi=300, bbox_inches="tight")
    plt.show()


if __name__ == "__main__":
    """ Arg: the number of bars/bins we want. """

    no_of_datasamples = int(sys.argv[1])
    
    binaryArrays = []
    
    for binaryDatasampleIndex in range(no_of_datasamples):
        binaryArrays.append(np.load("ConvertedBinaries/dataSampleBinary_successes_" + str(binaryDatasampleIndex) + ".npy"))

    main(binaryArrays)