import os
import sys
import numpy as np
import csv
import matplotlib.pyplot as plt

samplingRate = 100 # Hz                                                        POTENTIAL SOURCE OF ERROR

def main(phase_filename, simRun, show_fig_pls, save_fig_pls):        
    plt.close("all") # First clearing all other opened figures.
    
    times, phasesDatapointArray = parseDataFrom(phase_filename)
    
    plotPhases(times, phasesDatapointArray, simRun, show_fig_pls, save_fig_pls)
    

def plotPhases(t, phaseDataMatrix, simRun, show_fig_pls, save_fig_pls):
    """ Plots a Phases-plot """

    # Printing out Phase-data
    for col_index in range(phaseDataMatrix.shape[1]):
        labelString = "musical robot " + str(col_index+1)
        plt.plot(t, phaseDataMatrix[:,col_index], label=labelString)
    
    plt.ylabel("oscillator phase")
    plt.xlabel("simulation-time (s)")
    
    plt.legend(loc="best")                                              # BØR FINNE NOEN FINERE MÅTE Å PRESENTERE DETTE PÅ.

    
    # plt.tight_layout()                                      # BLIR DETTA FINT DA?
    noOfAgents = phaseDataMatrix.shape[1]
    if save_fig_pls == 1:
        plt.savefig("../../Synchrony/SavedData/Plots/" + str(noOfAgents) + "RobotsTerminatedAfter" + str(round(len(t)/samplingRate)) + "s_PhasePlot.pdf", dpi=300, format="pdf") # BØR JEG INKLUDERE ', bbox_inches="tight"' ?
    if show_fig_pls == 1:
        plt.show()


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
            
    
    t = np.linspace(0, numOfRows*(1/samplingRate), numOfRows-1) # In reality we start sampling after a split-second long startup-phase in Unity?
    
    return t, arrayOfDatapoints[2:,:] # Slicing due to initialization values (being huuuuge).
    
if __name__ == "__main__":
    """ Functionality:
            It first extracts the data from the .CSVs into two np.arrays, as well as getting the corresponding vertical time-axis.
        
            It then plots all of the columns (corresponding to an agents's data each) over the vertical time-axis. 
    """
    
    """ Arguments:
            simRun (int)        : the simulation-run from the latest Unity-run
            save_fig_pls (int) : whether or not we want to save—and not just plt.show()—the resulting figure/plot.
    """
    
    simRun = sys.argv[1]
    show_fig_pls = int(sys.argv[2])
    save_fig_pls = int(sys.argv[3])
    # phase_path = "../../Synchrony/SavedData/Phases/phases_over_time_atSimRun" + simRun + ".csv"
    temp_phase_path = "phases_over_time_atSimRun" + simRun + ".csv"
    
    main(temp_phase_path, simRun, show_fig_pls, save_fig_pls) # phase_path