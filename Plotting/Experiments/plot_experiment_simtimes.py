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
    
    plt.ylabel("simulation time (s)", fontsize=16)
    plt.xlabel("α", fontsize=16)
    
    plt.savefig("experiment_simtimes.svg", dpi=300, bbox_inches="tight")
    plt.show()


if __name__ == "__main__":
    # Args: 1) (string) binary1.npy filepath, 2) (string) binary2.npy filepath
    
    binaryPathStart = sys.argv[1]
    
    binaryArrays = []
    
    for alphaValue in range(1,10):
        binaryArrays.append(np.load(binaryPathStart + "0p" + str(alphaValue) + "_simTimes.npy"))


    main(binaryArrays)