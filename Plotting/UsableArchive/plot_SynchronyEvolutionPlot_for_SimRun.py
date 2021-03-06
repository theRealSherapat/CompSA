import sys
import numpy as np
import csv
import matplotlib.pyplot as plt
from matplotlib import rcParams

labelSize = 16
rcParams['xtick.labelsize'] = labelSize
rcParams['ytick.labelsize'] = labelSize

samplingRate = 20 # Hz                                                                             POTENTIAL SOURCE OF ERROR

def main(csv_filename, simRun, show_fig_pls, save_fig_pls):
    times, datapointArray = parseDataFrom(csv_filename)
    
    plotAllColsVsTime(times, datapointArray, simRun, show_fig_pls, save_fig_pls)
    

def plotAllColsVsTime(t, dataMatrix, simRun, show_fig_pls, save_fig_pls):
    """ Plotting all columns in the numpy-array dataMatrix over the vertical time-axis 't' in the same figure. """

    plt.close("all") # First clearing all other opened figures.

    for col_index in range(dataMatrix.shape[1]):
        # Plots each column-slice from the dataMatrix over the vertical time-axis 't'
        labelString = "Column " + str(col_index+1)
        plt.plot(t, dataMatrix[:,col_index], label=labelString, linewidth=3)
    
    plt.ylabel("# of even beats in a row by agent collective", fontsize=16)
    plt.xlabel("simulation-time (s)", fontsize=16)
        
    if save_fig_pls == 1:
        plt.savefig("../../Synchrony/SavedData/Plots/RobotsTerminatedAfter" + str(round(len(t)/samplingRate)) + "s_SyncEvolutionPlot.pdf", dpi=300, format="pdf", bbox_inches="tight")
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
            
    
    t = np.linspace(0, numOfRows*(1/samplingRate), numOfRows) # In reality we start sampling after a split-second long startup-phase in Unity.
    
    # Possibility: Downsampling the data- and time-arrays (for really long arrays from failed Simulation-runs).
    
    return t, arrayOfDatapoints[1:,:]
    
if __name__ == "__main__":
    """ Functionality:
            Takes in the .CSV filename to extract and plot data from.
            
            It first extracts the data from the .CSV into an np.array, as well as getting the corresponding vertical time-axis.
            
            It then plots all of the columns over the vertical time-axis in the same figure. """

    """ Arguments:
            simRun (int)        : the simulation-run from the latest Unity-run
            show_fig_pls (int)  : whether or not we want to show the resulting figure/plot.
            save_fig_pls (int)  : whether or not we want to save the resulting figure/plot.
    """
    

    simRun = sys.argv[1]
    show_fig_pls = int(sys.argv[2])
    save_fig_pls = int(sys.argv[3])
    
    filepath = "../../Synchrony/SavedData/SynchronyEvolutions/synch_evolution_data_atSimRun" + simRun + ".csv"
    temp_filepath = "synch_evolution_data_atSimRun" + simRun + ".csv"
    
    main(filepath, simRun, show_fig_pls, save_fig_pls) # temp_filepath