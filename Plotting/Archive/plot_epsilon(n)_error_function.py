import matplotlib.pyplot as plt
import numpy as np

def main():
    # Building x
    xLimits = (0, 1)
    resolution = 100
    phases = np.linspace(xLimits[0], xLimits[1], resolution)
    
    # Building function y
    # error_scores = np.power(np.sin(np.pi * phases), 2) # error-function, epsilon
    rho_scores = -np.sin(2*np.pi*phases)
    
    # Plotting y over x
    plt.xlabel("phase ($\phi(t)$)")
    plt.xticks([0,0.25,0.5,0.75,1])
    # plt.ylabel("frequency-update amplitude- and sign-factor ")
    # plt.title("$\rho(n)$-plot")
    plt.plot(phases, rho_scores)
    plt.savefig("function_over_time.pdf", format="pdf", bbox_inches="tight")
    plt.show()

def H(phase, s_n)

if __name__ == "__main__":
    main()