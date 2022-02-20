import matplotlib.pyplot as plt
import numpy as np

def main():
    # Generating (WANT TO CHANGE OUT WITH LOADING) and stacking data
    mirolloStrogatzTimes = np.random.normal(loc = 30, scale = 3, size = 30) # loc=mu, scale=sigma.
    nymoenTimes = np.random.normal(loc = 18, scale = 6, size = 30)
    timesStacked = np.stack((mirolloStrogatzTimes, nymoenTimes), axis = 1)
    
    # Plotting stacked data
    plt.boxplot(timesStacked, labels=["''Standard'' Mirollo-Strogatz", "Kristian Nymoen et al.'s Bi-Directional"]) # Kan ha med whis=(0,100) for å få whiskerne til å dekke hele data-samplet (til og med outliersa).
    plt.title("Harmonic Synchronization Performance in the $\phi$-problem")
    plt.ylabel("Simulation time (s) until harmonic synchrony is detected")
    plt.xlabel("Phase-Adjustment method")
    plt.show()


if __name__ == "__main__":
    main()