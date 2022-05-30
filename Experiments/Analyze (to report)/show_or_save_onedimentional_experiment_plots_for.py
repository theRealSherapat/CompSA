import os
import sys
import matplotlib.pyplot as plt
import numpy as np
# import seaborn as sns
from matplotlib.pyplot import figure
from matplotlib import rcParams
from experiment_analysis_utils import *


# Setting up the visual looks of the experiment plotting figure:
labelSize = 20
rcParams['xtick.labelsize'] = labelSize
rcParams['ytick.labelsize'] = labelSize


def main(showPls, savePls, xlabelCovariate, xlabelValues, terminationTimesArrays, successScoresArrays):
    """ First either shows or saves a graphical plot of the termination (or sync) times (sim s), then shows or saves a graphical plot of the error rates corresponding to the same data samples as plotted termination (or sync) times for. """
    
    # Generating and (if wanted) showing and (if wanted) saving synch times plot.
    generateTerminationtimesPlot(showPls, savePls, xlabelCovariate, xlabelValues, terminationTimesArrays)
    
    # Cleaning up before bringing up the next matplotlib plot / figure.
    plt.close()
    
    # Generating and (if wanted) showing and (if wanted) saving synch error rates plot.
    generateSuccessScoresPlot(showPls, savePls, xlabelCovariate, xlabelValues, successScoresArrays)



def generateTerminationtimesPlot(showPls, savePls, xlabelCovariate, xlabelValues, terminationTimesArrays):
    # Plotting synchronization performance scores in a boxplot
    
    # sns.violinplot(data=terminationTimesArrays) # x=xlabelCovariate
    
    termArrays = [[20.48, 20.65, 20.65, 20.49, 20.47, 20.63, 20.5 , 20.49, 20.48,
       20.5 , 20.46, 20.46, 20.49, 20.46, 20.62, 20.47, 20.47, 20.66,
       20.65, 20.55, 20.63, 20.48, 20.57, 20.47, 20.46, 20.5 , 20.49,
       20.49, 20.48],[20.49, 37.41],[23.35],[20.46, 20.49, 20.61, 20.49, 20.46, 20.51, 20.5 , 20.65, 20.46,
       20.5 , 20.5 , 20.47, 20.47, 20.47, 20.56, 20.49, 20.47, 20.48,
       20.48, 20.46, 20.48, 20.5 , 20.46, 20.48, 20.54, 20.5 , 20.49,
       20.47, 20.61, 20.48]]
    
    plt.boxplot(termArrays, labels=[1, 2, 3, 4]) # Kan ha med whis=(0,100) for å få whiskerne til å dekke hele data-samplet (til og med outliersa).
    plt.ylabel("harmonic synchronization time (sim s)", fontsize=labelSize)
    plt.xlabel("SA scope ratio (m, r, g)", fontsize=labelSize)
    
    if savePls:
        plt.savefig("SavedPlots/experiment_simtimes.svg", dpi=300, bbox_inches="tight")
    
    if showPls:
        plt.show()


def generateSuccessScoresPlot(showPls, savePls, xlabelCovariate, xlabelValues, successScoresArrays):
    # Plotting synchronization error scores / rates in a barplot
    
    successScoresLists = [[1., 1., 1., 1., 1., 1., 1., 1., 1., 1., 1., 1., 1., 1.,1., 1., 1.,1., 1., 1., 1., 1., 1., 1., 1., 1., 1., 1., 1.],
    [1., 0., 0., 0., 1.,0., 0., 0., 0., 0., 0., 0., 0., 0., 0., 0., 0.,0., 0., 0., 0., 0., 0., 0., 0., 0., 0., 0., 0., 0.],
    [0., 0., 0., 0., 0., 0., 0., 0., 0., 1., 0., 0., 0., 0., 0., 0., 0.,0., 0., 0., 0., 0., 0., 0., 0., 0., 0., 0., 0., 0.],
    [0., 1., 1., 1., 1., 1., 1., 1., 1., 1., 1., 1., 1., 1., 1., 1., 1.,1., 1., 1., 1., 1., 1., 1., 1., 1., 1., 1., 1., 1., 1.]]
    
    # MIDTERSTE: [0., 0., 0., 0., 0., 0., 0., 0., 0., 0., 0., 0., 0., 0., 0., 0., 0.,0., 0., 0., 0., 0., 0., 0., 0., 0., 0., 0., 0., 0.],
    # [0., 0., 0., 0., 0., 0., 0., 0., 0., 0., 0., 0., 0., 0., 0., 0., 0.,0., 0., 0., 0., 0., 0., 0., 0., 0., 0., 0., 0., 0.],
    # [0., 0., 0., 0., 0., 0., 0., 0., 0., 0., 0., 0., 0., 0., 0., 0., 0.,0., 0., 0., 0., 0., 0., 0., 0., 0., 0., 0., 0., 0.],
    
    # Calculating the error scores (according to simple percentage of successful runs out of total runs formula):
    errorRatesInPercentages = []
    for successArray in successScoresLists:
        successfulRuns = np.sum(successArray)
        totalRuns = len(successArray)
        errorPercentage = (1 - successfulRuns / totalRuns) * 100
        errorRatesInPercentages.append(errorPercentage)
    
    plt.bar([1, 2, 3, 4], errorRatesInPercentages) # Kan ha med whis=(0,100) for å få whiskerne til å dekke hele data-samplet (til og med outliersa).
    plt.ylabel("error rate (%)", fontsize=labelSize)
    plt.xlabel("SA scope ratio (m, r, g)", fontsize=labelSize)
    
    
    if savePls:
        plt.savefig("SavedPlots/experiment_errorRates.svg", dpi=300, bbox_inches="tight")
    
    if showPls:
        plt.show()


def retrieveCommandLineArguments():
    showPls = int(sys.argv[1])
    savePls = int(sys.argv[2])
    xlabelCovariate = sys.argv[3]
    xlabelValues = sys.argv[4:]
        

    return showPls, savePls, xlabelCovariate, xlabelValues

if __name__ == "__main__":
    """ . """
    
    # Arg1: (int) show figure in a window (1: yes, 0:no).
    # Arg2: (int) save figure to .svg (1: yes, 0:no).
    # Arg3: (str) the hyperparameter / covariate name for the plots's xlabel.
    # Arg4: (list of strs) the hyperparameter / covariate values (xticks).
    
    showPls, savePls, xlabelCovariate, xlabelValues = retrieveCommandLineArguments()
    
    terminationTimesArrays, successScoresArrays = loadBinaries()
    print("terminationTimesArrays:", terminationTimesArrays, "successScoresArrays:",successScoresArrays)

    main(showPls, savePls, xlabelCovariate, xlabelValues, terminationTimesArrays, successScoresArrays)