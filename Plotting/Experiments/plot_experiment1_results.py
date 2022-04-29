import sys
import matplotlib.pyplot as plt
import numpy as np
from matplotlib.pyplot import figure

# figure(figsize=((8,3)), dpi=300)

def main(binPaths):
    timesStacked = np.load(binPaths[0]).reshape((30,1))
    # Loading performance-scores/-data:
    for binPathInd in range(1, len(binPaths)):
        # timesStacked = np.stack((timesStacked,np.load(binPaths[binPathInd]).reshape((30,1))), axis=1)
        timesStacked = np.concatenate((timesStacked, np.load(binPaths[binPathInd]).reshape((timesStacked.shape[0],1))), axis=1)
    
    # Plotting stacked data
    plt.boxplot(timesStacked, labels=["0.1", "0.2", "0.3", "0.4", "0.5", "0.6", "0.7", "0.8", "0.9"]) # Kan ha med whis=(0,100) for å få whiskerne til å dekke hele data-samplet (til og med outliersa).
    plt.ylabel("synchronization time (s)")
    # plt.xlabel("$\alpha$")
    plt.savefig("Experiment1Plot.svg", dpi=300)
    plt.show()


if __name__ == "__main__":
    # Args: 1) (string) binary1.npy filepath, 2) (string) binary2.npy filepath
    
    binaryPaths = [sys.argv[1], sys.argv[2], sys.argv[3], sys.argv[4], sys.argv[5], sys.argv[6], sys.argv[7], sys.argv[8], sys.argv[9]]

    main(binaryPaths)