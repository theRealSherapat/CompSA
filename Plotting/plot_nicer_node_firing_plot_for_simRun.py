import sys
import numpy as np
import csv
import matplotlib.pyplot as plt

samplingRate = 100 # Hz                                                                             POTENTIAL SOURCE OF ERROR
colors = ['b', 'g', 'r', 'c', 'm', 'y', 'k', 'w']

def main(csv_filename):
    times, t_f_is_now_samples, datapointArray = parseDataFrom(csv_filename)
    
    plotANicePlot(times, t_f_is_now_samples, datapointArray)
    
    # plotAllColsVsTime(times, datapointArray)
    

def plotANicePlot(timeArray, t_fArray, nodesFiringMatrix):
    plt.close("all") # First clearing all other opened figures.

    for col_index in range(nodesFiringMatrix.shape[1]):
        labelString = "Agent " + str(col_index) + " fired"
        yHeight = (nodesFiringMatrix.shape[1]-col_index)
        plotBooleanStripWithSymbolAtHeight(nodesFiringMatrix[:,col_index], 2*col_index, col_index, timeArray, yHeight)
        # fOfT = nodesFiringMatrix[:,col_index] + yHeight
        # plt.plot(timeArray, fOfT, label=labelString)
    
    plt.xlabel("Time in seconds")
    plt.ylabel("Agent # firing")
    # plt.legend(loc='upper right')
    plt.title("Node-firing plot")
    plt.show()

def plotBooleanStripWithSymbolAtHeight(boolStrip, symbol, colorIndex, tArray, yCoordinate):
    for stripIndex in range(boolStrip.shape[0]):
        if boolStrip[stripIndex] == 1.0:
            # print("Hi! I want to plot a point at x=" + str(tArray[stripIndex]) + " , y=" + str(yCoordinate))
            plt.plot(tArray[stripIndex], yCoordinate, marker=symbol, color=colors[colorIndex])

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
    
    t_f_is_now_samples = arrayOfDatapoints[:,0]
    
    datapointArray = arrayOfDatapoints[:,1:]
    
    return t, t_f_is_now_samples, datapointArray[2:,:] # Slicing from index 2 due to initialization values being huuuuge.
    
if __name__ == "__main__":
    """ Functionality:
            Takes in the .CSV filename to extract and plot data from, the amount of agents we have data for.
            
            It first extracts the data from the .CSV into an np.array, as well as obtaining the corresponding vertical time-axis.
            
            It then plots all of the columns (corresponding to an agents's data each) over the vertical time-axis in the same figure. """
            
    """ Arguments:
            simRun (str): the simulation-run from the latest Unity-run
    """
    
    simRun = sys.argv[1]
    filepath = "../Synchrony/SavedData/NodeFiringPlotMaterial/node_firing_data_atSimRun" + simRun + ".csv"
    
    
    main(filepath)