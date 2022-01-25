import sys
import numpy as np
import csv
import matplotlib.pyplot as plt

samplingRate = 100 # Hz                                                                             POTENTIAL SOURCE OF ERROR

# THIS SCRIPT IS NOT TIMELY FUNCTIONAL AT THE MOMENT, AND DOES NOT CORRESPOND 1-1 WITH THE Debug.Log-window IN Unity IN TERMS OF TIMES/TIMINGS (E.G. THE 80ms T_F-PERIOD LOOKED LIKE A 130ms T_F-PERIOD IN THE PLOT, USING 5 AGENTS I BELIEVE).
# THIS IS MOST LIKELY DUE TO HOW THE node_firing_data.csv IS UPDATED (I.E. NOT JUST ONCE PER FIXED-UPDATE, BUT ALSO ONCE EVERY TIME A MUSICAL ROBOT IS HEARD FIRING/FLASHING).
    # LOOK AT HOW UpdateNodeFiringCSVNegative() AND UpdateNodeFiringCSVPositive() ARE CALLED TO FIX THIS.

def main(csv_filename):
    times, datapointArray = parseDataFrom(csv_filename)
    
    plotAllColsVsTime(times, datapointArray)
    

def plotAllColsVsTime(t, dataMatrix):
    """ Plotting all columns in the numpy-array dataMatrix over the vertical time-axis 't' in the same figure. """

    plt.close("all") # First clearing all other opened figures.

    for col_index in range(dataMatrix.shape[1]):
        if (col_index != dataMatrix.shape[1]-1):
            labelString = "Agent " + str(col_index+1) + " just fired"
        else:
            labelString = "t_f_is_now"
        plt.plot(t, dataMatrix[:,col_index], label=labelString)

    # for arrayOfDatapoints in arraysOfDatapoints:
    
    plt.ylabel("signals (on/off)")
    plt.xlabel("ish time in seconds")
    plt.legend(loc='upper right')
    plt.title("Chronologically (not necessarily timely) correct Node-firing-plot")
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
    
    return t, arrayOfDatapoints[2:,:] # Slicing due to initialization values (being huuuuge).
    
if __name__ == "__main__":
    """ Functionality:
            Takes in the .CSV filename to extract and plot data from, the amount of agents we have data for,
            as well as the type of data from the agents that are recorded in the .CSV-file.
            
            It first extracts the data from the .CSV into an np.array, as well as getting the corresponding vertical time-axis.
            
            It then plots all of the columns (corresponding to an agents's data each) over the vertical time-axis in the same figure. """

    filepath = "../../Synchrony/SavedData/node_firing_data.csv"
    
    main(filepath)