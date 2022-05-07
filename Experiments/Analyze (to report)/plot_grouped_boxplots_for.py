import numpy as np
import sys
import matplotlib.pyplot as plt

# DUMMMY EXAMPLE DATA:
A = [[1, 2, 5,],  [7, 2], [6,7], [2,3,7]]
B = [[5, 7, 2, 2, 5], [7, 2, 5], [6,7], [2,3,7]]
C = [[4,2,5,7], [6, 7, 3], [6,7], [2,3,7]]
D = [[6,2,5,7], [6, 7, 3], [6,7], [2,3,7]]
E = [[8,2,5,7], [6, 7, 3], [6,7], [2,3,7]]
F = [[12,2,5,7], [6, 7, 3], [6,7], [2,3,7]]

alphaDataSamples = [A, B, C, D, E, F]

boxplotWidth = 0.6
alphas = [0.001, 0.01, 0.1, 0.2, 0.4, 0.8]
sparsifiedAlphas = np.array(alphas) * 100 + np.array([0, 3, 1, 0, 0, 0])

bpDist = boxplotWidth*1.5

# CALCULATE alphaDatasamplesPositions BEFOREHAND SO YOU DON'T HAVE TO DO IT ON THE FLY INSIDE THE PARENTHESES BELOW.

def main():
    for ind, alphaDataSample in enumerate(alphaDataSamples):
        plt.boxplot(alphaDataSample, positions = [sparsifiedAlphas[ind]-3*bpDist/2, sparsifiedAlphas[ind]-bpDist/2, sparsifiedAlphas[ind]+bpDist/2, sparsifiedAlphas[ind]+3*bpDist/2], widths = boxplotWidth)
    
    # plt.xticks(sparsifiedAlphas)
    plt.xlabel(alphas)
    
    plt.show()
    
if __name__ == "__main__":
    main()