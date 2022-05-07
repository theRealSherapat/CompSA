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
    # Plotting stacked data
    plt.boxplot(binaryArrays, labels=["0.1", "0.2", "0.3", "0.4", "0.5", "0.6", "0.7", "0.8", "0.9"]) # Kan ha med whis=(0,100) for å få whiskerne til å dekke hele data-samplet (til og med outliersa).
    
    plt.ylabel("harmonic synchronization time (sim s)", fontsize=16)
    plt.xlabel("α", fontsize=16)
    
    plt.savefig("experiment_simtimes.svg", dpi=300, bbox_inches="tight")
    plt.show()


if __name__ == "__main__":
    """ Arg: the number of boxplots we want. """

    no_of_datasamples = int(sys.argv[1])
    
    binaryArrays = []
    
    for binaryDatasampleIndex in range(no_of_datasamples):
        binaryArrays.append(np.load("ConvertedBinaries/dataSampleBinary_simTimes_" + str(binaryDatasampleIndex) + ".npy"))


    main(binaryArrays)