import sys
import matplotlib.pyplot as plt
import numpy as np

def main(binaryArray1Path, binaryArray2Path):
    # Generating dummy/synthetic performance-scores/-data:
    # mirolloStrogatzTimes = np.random.normal(loc = 30, scale = 3, size = 30) # loc=mu, scale=sigma.
    # nymoenTimes = np.random.normal(loc = 18, scale = 6, size = 30)

    # Loading performance-scores/-data:
    HSYNCHTIMES1 = np.load(binaryArray1Path)
    HSYNCHTIMES2 = np.load(binaryArray2Path)
    timesStacked = np.stack((HSYNCHTIMES1, HSYNCHTIMES2), axis = 1)
    
    # Plotting stacked data
    plt.boxplot(timesStacked, labels=["Mirollo-Strogatz", "Kristian Nymoen et al."]) # Kan ha med whis=(0,100) for å få whiskerne til å dekke hele data-samplet (til og med outliersa).
    plt.ylabel("Simulation time (s) until harmonic synchrony is detected")
    plt.xlabel("Phase-adjustment method")
    plt.savefig("ExperimentPlot.pdf", format="pdf")
    plt.show()


if __name__ == "__main__":
    # Args: 1) (string) binary1.npy filepath, 2) (string) binary2.npy filepath

    binary1Path = sys.argv[1]
    binary2Path = sys.argv[2]

    main(binary1Path, binary2Path)