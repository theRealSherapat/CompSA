import matplotlib.pyplot as plt
import numpy as np

def main():
    # Generating (WANT TO CHANGE OUT WITH LOADING) and stacking data
    mirolloStrogatzTimes = np.random.normal(loc = 30, scale = 3, size = 30) # loc=mu, scale=sigma.
    nymoenTimes = np.random.normal(loc = 18, scale = 6, size = 30)
    timesStacked = np.stack((mirolloStrogatzTimes, nymoenTimes), axis = 1)
    
    # Plotting stacked data
    plt.boxplot(timesStacked, labels=["Mirollo-Strogatz", "Kristian Nymoen"]) # Kan ha med whis=(0,100) for å få whiskerne til å dekke hele data-samplet (til og med outliersa).
    plt.title("Synchronization-Performance within Phase-Adjustment")
    plt.ylabel("Synchronization-time, HSYNCHTIME (seconds)")
    plt.xlabel("Update-function for Phase-adjustment, PHASEUPD")
    plt.show()


if __name__ == "__main__":
    main()