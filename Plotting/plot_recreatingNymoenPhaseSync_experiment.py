import sys
import matplotlib.pyplot as plt
import numpy as np

def main(binPaths):
    timesStacked = np.load(binPaths[0])
    # Loading performance-scores/-data:
    for binPathInd in range(1, len(binPaths)):
        timesStacked = np.stack((timesStacked,np.load(binPaths[binPathInd])), axis=1)
    
    # HSYNCHTIMES1 = np.load(binaryArray1Path)
    # HSYNCHTIMES2 = np.load(binaryArray2Path)
    # timesStacked = np.stack((HSYNCHTIMES1, HSYNCHTIMES2), axis = 1)
    
    # Plotting stacked data
    plt.boxplot(timesStacked, labels=["0.1", "0.2"]) # Kan ha med whis=(0,100) for å få whiskerne til å dekke hele data-samplet (til og med outliersa).
    plt.ylabel("simulation time (s) before harmonic synchrony is detected")
    # plt.xlabel("$\alpha$")
    # plt.savefig("ExperimentPlot.pdf", dpi=300, format="pdf")
    plt.show()


if __name__ == "__main__":
    # Args: 1) (string) binary1.npy filepath, 2) (string) binary2.npy filepath
    
    binaryPaths = [sys.argv[1], sys.argv[2]]#, sys.argv[3], sys.argv[4], sys.argv[5], sys.argv[6], sys.argv[7], sys.argv[8], sys.argv[9]]

    main(binaryPaths)