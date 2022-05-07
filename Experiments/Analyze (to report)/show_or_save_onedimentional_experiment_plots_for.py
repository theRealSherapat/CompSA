import os
import sys
import matplotlib.pyplot as plt
import numpy as np
from matplotlib.pyplot import figure
from matplotlib import rcParams

# Setting up the visual looks of the experiment plotting figure:
labelSize = 16
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
    
    plt.boxplot(terminationTimesArrays, labels=xlabelValues) # Kan ha med whis=(0,100) for å få whiskerne til å dekke hele data-samplet (til og med outliersa).
    plt.ylabel("harmonic synchronization time (sim s)", fontsize=16)
    plt.xlabel(xlabelCovariate, fontsize=16)
    
    if savePls:
        plt.savefig("SavedPlots/experiment_simtimes.svg", dpi=300, bbox_inches="tight")
    
    if showPls:
        plt.show()


def generateSuccessScoresPlot(showPls, savePls, xlabelCovariate, xlabelValues, successScoresArrays):
    # Plotting synchronization error scores / rates in a barplot
    
    # Calculating the error scores (according to simple percentage of successful runs out of total runs formula):
    errorRatesInPercentages = []
    for successArray in successScoresArrays:
        successfulRuns = np.sum(successArray)
        totalRuns = len(successArray)
        errorPercentage = (1 - successfulRuns / totalRuns) * 100
        errorRatesInPercentages.append(errorPercentage)
    
    plt.bar(xlabelValues, errorRatesInPercentages) # Kan ha med whis=(0,100) for å få whiskerne til å dekke hele data-samplet (til og med outliersa).
    plt.ylabel("error rate (%)", fontsize=16)
    plt.xlabel(xlabelCovariate, fontsize=16)
    
    
    if savePls:
        plt.savefig("SavedPlots/experiment_errorRates.svg", dpi=300, bbox_inches="tight")
    
    if showPls:
        plt.show()


def loadRelevantQuantities():
    no_of_datasamples = countFilesInFolderPath("ConvertedBinaries/")
    
    # Loading the termination times (or simulation times if 'plotFails = 0' when you converted binaries):
    terminationTimesArrays = []
    
    for binaryDatasampleIndex in range(no_of_datasamples):
        terminationTimesArrays.append(np.load("ConvertedBinaries/dataSampleBinary_terminationTimes_" + str(binaryDatasampleIndex) + ".npy"))
        
        
    # Loading the termination times (or simulation times if 'plotFails = 0' when you converted binaries):
    successScoresArrays = []
    
    for binaryDatasampleIndex in range(no_of_datasamples):
        successScoresArrays.append(np.load("ConvertedBinaries/dataSampleBinary_successes_" + str(binaryDatasampleIndex) + ".npy"))

    return terminationTimesArrays, successScoresArrays

def countFilesInFolderPath(path):
    filesInPath = 0
    
    for files in os.walk(path):
        filesInPath = len(files[2])


    return int(filesInPath/2)


def retrieveCommandLineArguments():
    showPls = int(sys.argv[1])
    savePls = int(sys.argv[2])
    xlabelCovariate = sys.argv[3]
    xlabelValues = sys.argv[4:]
        

    return showPls, savePls, xlabelCovariate, xlabelValues

if __name__ == "__main__":
    """ Arg: the number of datasamples we have. """
    
    # Arg1: (int) show figure in a window (1: yes, 0:no).
    # Arg2: (int) save figure to .svg (1: yes, 0:no).
    # Arg3: (str) the hyperparameter / covariate name for the plots's xlabel.
    # Arg4: (list of strs) the hyperparameter / covariate values (xticks).
    
    showPls, savePls, xlabelCovariate, xlabelValues = retrieveCommandLineArguments()
    
    terminationTimesArrays, successScoresArrays = loadRelevantQuantities()
    

    main(showPls, savePls, xlabelCovariate, xlabelValues, terminationTimesArrays, successScoresArrays)