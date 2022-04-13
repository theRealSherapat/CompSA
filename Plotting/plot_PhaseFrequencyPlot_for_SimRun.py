import os
import sys
import numpy as np
import csv
import matplotlib.pyplot as plt

samplingRate = 100 # Hz                                                            POTENTIAL SOURCE OF ERROR

def main(phase_filename, freqs_filename, simRun, save_fig_pls):        
    plt.close("all") # First clearing all other opened figures.
    
    times, phasesDatapointArray = parseDataFrom(phase_filename)
    
    _, freqsDatapointArray = parseDataFrom(freqs_filename)
    
    plotPhasesAndFrequencies(times, phasesDatapointArray, freqsDatapointArray, simRun, save_fig_pls)
    

def plotPhasesAndFrequencies(t, phaseDataMatrix, frequencyDataMatrix, simRun, save_fig_pls):
    """ Plots a connected Phase-Plot and Frequency-Plot """

    # Printing out Phase-data in the top sub-plot
    plt.subplot(2,1,1)
    plt.ylabel("phase")
    plt.xlabel("simulation-time (s)")
    for col_index in range(phaseDataMatrix.shape[1]):
        labelString = "Musical Agent " + str(col_index+1)
        plt.plot(t, phaseDataMatrix[:,col_index], label=labelString)
    
    # plt.legend()                                              # MÅ FINNE NOEN FINERE MÅTE Å PRESENTERE DETTE PÅ ISÅFALL.

    # Printing out Frequency-data in the bottom sub-plot
    plt.subplot(2,1,2)
    plt.ylabel("frequency (Hz)")
    plt.xlabel("simulation-time (s)")
    for col_index in range(frequencyDataMatrix.shape[1]):
        labelString = "Musical Agent " + str(col_index+1)
        plt.plot(t, frequencyDataMatrix[:,col_index], label=labelString)

    # Scatter-plotting the legal multiples frequencies are allowed to lie on (defined by the lowest fundamental frequency)
    scatterPlotLegalMultiples(t, frequencyDataMatrix)
    
    # Printing out the whole sub-plot
    plt.tight_layout()
    noOfAgents = frequencyDataMatrix.shape[1]
    if save_fig_pls == 1:
        plt.savefig(str(noOfAgents) + "AgentsPhasesAndFrequenciesForSimRun" + str(simRun) + ".pdf", dpi=300, format="pdf", bbox_inches="tight")
    plt.show()
    
def scatterPlotLegalMultiples(timeArray, freqsDataMatrix):
    lowestFrequenciesAcrossRun = freqsDataMatrix.min(axis=1)
    
    numberOfValidFrequenciesWanted = 3
    validFrequenciesList = []
    
    for validFrequencyLayerIndex in range(1,numberOfValidFrequenciesWanted):
        validFrequenciesArray = lowestFrequenciesAcrossRun * np.power(2, validFrequencyLayerIndex)
        validFrequenciesList.append(validFrequenciesArray)
        

    for y in validFrequenciesList:
        plt.plot(timeArray, y, color='lightgray', linestyle='dashed', linewidth=0.8)


def parseDataFrom(csv_filename):
    """ Reads all rows (apart from the header) into a numpy data-matrix, and returns that 'arrayOfDatapoints' and its corresponding vertical time-axis 't' """

    arrayOfDatapoints = np.empty((1, 1)) # initializing our numpy data-matrix with a dummy size (real size will be sat later on)
    
    numOfRows = 0
    with open(csv_filename) as dataFile:
        csvReader = csv.reader(dataFile, delimiter=';')
        next(csvReader, None) # to skip the headers
        for row in csvReader:
            if numOfRows == 0:
                arrayOfDatapoints = np.empty((1, len(row))) # initializing our numpy data-matrix when we know the number of columns to include in it
                
            col_array = np.array([])
            for col_index in range(len(row)):
                col_element = float(row[col_index].replace(',','.'))
                col_array = np.append(col_array, col_element)
            
            col_array = np.reshape(col_array, (1, len(row)))
            arrayOfDatapoints = np.vstack((arrayOfDatapoints, col_array))
            
            numOfRows += 1
            
    
    t = np.linspace(0, numOfRows*(1/samplingRate), numOfRows-1) # In reality we start sampling after a split-second long startup-phase in Unity.
    
    # Possibility: Downsampling the data- and time-arrays (for really long arrays from failed Simulation-runs).
    
    return t, arrayOfDatapoints[2:,:] # Slicing due to initialization values (being huuuuge).
    
if __name__ == "__main__":
    # Args: 1) (int) SimRun: the simulation-run from the latest Unity-run

    """ Functionality:
            It first extracts the data from the .CSVs into two np.arrays, as well as getting the corresponding vertical time-axis.
        
            It then plots all of the columns (corresponding to an agents's data each) over the vertical time-axis in a subplot with phase and frequencies. 
            
            Lastly, legal multiples for frequency (in order to have achieved harmonic synchronization) are marked with green circles. 
    """
    
    """ Arguments:
            simRun (int)        : the simulation-run from the latest Unity-run
            save_fig_pls (int) : whether or not we want to save—and not just plt.show()—the resulting figure/plot.
    """
    
    simRun = sys.argv[1]
    save_fig_pls = int(sys.argv[2])
    phase_path = "../Synchrony/SavedData/Phases/phases_over_time_atSimRun" + simRun + ".csv"
    freqs_path = "../Synchrony/SavedData/Frequencies/freqs_over_time_atSimRun" + simRun + ".csv"
    
    
    main(phase_path, freqs_path, simRun, save_fig_pls)