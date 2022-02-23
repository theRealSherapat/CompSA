import sys
import numpy as np
import csv
import matplotlib.pyplot as plt

samplingRate = 100 # Hz                                                                             POTENTIAL SOURCE OF ERROR

def main(csv_filename):
    times, datapointArray = parseDataFrom(csv_filename)
    
    plotAllColsVsTime(times, datapointArray)
    

def plotAllColsVsTime(t, dataMatrix):
    """ Plotting all columns in the numpy-array dataMatrix over the vertical time-axis 't' in the same figure. """

    plt.close("all") # First clearing all other opened figures.

    for col_index in range(dataMatrix.shape[1]):
        if (col_index != 0):
            labelString = "Agent " + str(col_index) + " just fired"
        else:
            labelString = "t_f_is_now"
        plt.plot(t, dataMatrix[:,col_index], label=labelString)
    
    plt.ylabel("Signals (on/off)")
    plt.xlabel("Time in seconds")
    plt.legend(loc='upper right')
    plt.title("Node-firing plot")
    plt.show()

def parseDataFrom(csv_filename):
    """ Reads all rows (apart from the header) into a numpy data-matrix, and returns that 'arrayOfDatapoints' and its corresponding vertical time-axis 't' """

    arrayOfDatapoints = np.empty((1, 1))
    
    numOfRows = 0
    with open(csv_filename) as dataFile:
        csvReader = csv.reader(dataFile, delimiter=';')
        next(csvReader, None) # to skip the headers
        for row in csvReader:
            if numOfRows == 0:
                arrayOfDatapoints = np.empty((1, len(row)))
            col_array = np.array([])
            for col_index in range(len(row)):
                col_element = float(row[col_index].replace(',','.'))
                col_array = np.append(col_array, col_element)
            
            col_array = np.reshape(col_array, (1, len(row)))
            arrayOfDatapoints = np.vstack((arrayOfDatapoints, col_array))
            
            numOfRows += 1
            
    
    t = np.linspace(0, numOfRows*(1/samplingRate), numOfRows-1) # In reality we start sampling after a split-second long startup-phase in Unity.
    
    return t, arrayOfDatapoints[:,:] # Maybe the stopping of slicing from the 3rd index will fix the headstart disparity?
    
if __name__ == "__main__":
    """ Functionality:
            Takes in the .CSV filename to extract and plot data from, the amount of agents we have data for,
            as well as the type of data from the agents that are recorded in the .CSV-file.
            
            It first extracts the data from the .CSV into an np.array, as well as getting the corresponding vertical time-axis.
            
            It then plots all of the columns (corresponding to an agents's data each) over the vertical time-axis in the same figure. """
            
    """ Arguments:
            simRun (str): the simulation-run from the latest Unity-run
    """
    
    simRun = sys.argv[1]
    filepath = "../Synchrony/SavedData/NodeFiringPlotMaterial/node_firing_data_atSimRun" + simRun + ".csv"
    
    
    main(filepath)