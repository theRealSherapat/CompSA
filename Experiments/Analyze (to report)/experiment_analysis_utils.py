import os
import sys
import matplotlib.pyplot as plt
import numpy as np
from matplotlib.pyplot import figure
from matplotlib import rcParams

def countDatasamplesInFolderPath(path):
    filesInPath = 0
    
    for files in os.walk(path):
        filesInPath = len(files[2])


    return int(filesInPath/2)
    

def loadBinaries():
    no_of_datasamples = countDatasamplesInFolderPath("ConvertedBinaries/")
    
    # Loading the termination times (or simulation times if 'plotFails = 0' when you converted binaries):
    terminationTimesArrays = []
    
    for binaryDatasampleIndex in range(no_of_datasamples):
        terminationTimesArrays.append(np.load("ConvertedBinaries/dataSampleBinary_terminationTimes_" + str(binaryDatasampleIndex) + ".npy"))
        
        
    # Loading the termination times (or simulation times if 'plotFails = 0' when you converted binaries):
    successScoresArrays = []
    
    for binaryDatasampleIndex in range(no_of_datasamples):
        successScoresArrays.append(np.load("ConvertedBinaries/dataSampleBinary_successes_" + str(binaryDatasampleIndex) + ".npy"))

    return terminationTimesArrays, successScoresArrays