import matplotlib.pyplot as plt
import numpy as np

def main():
    # Building x
    xLimits = (0, 1)
    resolution = 100
    phase = np.linspace(xLimits[0], xLimits[1], resolution)
    
    # Building function y
    error_scores = np.power(np.sin(np.pi * phase), 2)
    
    # Plotting y over x
    plt.xlabel("phase ($\phi(t)$)")
    plt.xticks([0,0.25,0.5,0.75,1])
    plt.ylabel("error-score $\epsilon$")
    plt.title("Error-Phase plot")
    plt.plot(phase, error_scores)
    # plt.savefig("function_over_time.pdf", format="pdf", bbox_inches="tight")
    plt.show()
    
if __name__ == "__main__":
    main()