import os
import sys
import numpy as np
import csv
import matplotlib.pyplot as plt

samplingRate = 100 # Hz

def main(csv_filename, agent_no, dataPlotted):
    """ Takes in the .CSV filename to extract and plot data from, the amount of agents we have data for,
        as well as the type of data from the agents that are recorded in the .CSV-file.
        
        It first extracts the data from the .CSV into an np.array, as well as getting the corresponding vertical time-axis.
        
        It then plots all of the columns (corresponding to an agents's data each) over the vertical time-axis in the same figure.

        Before it shows the final plot, it saves this as a PDF with a filename 'mySavedPDF.pdf'. """
        
    times, datapointArray = parseDataFrom(csv_filename, agent_no)
    
    plotAllColsVsTime(times, datapointArray, dataPlotted)
    

def plotAllColsVsTime(t, dataMatrix, dataPlotted):
    """ Plotting all columns in the numpy-array dataMatrix over the vertical time-axis 't' in the same figure. Including the specified ylabel 'dataPlotted' too. """

    plt.close("all") # First clearing all other opened figures.

    for col_index in range(dataMatrix.shape[1]):
        labelString = "Musical Agent " + str(col_index+1)
        plt.plot(t, dataMatrix[:,col_index], label=labelString)

    # for arrayOfDatapoints in arraysOfDatapoints:
    
    plt.ylabel(dataPlotted)
    plt.xlabel("$t$ (seconds)")
    plt.legend(loc='upper right')
    plt.savefig("mySavedPDF.pdf", format="pdf", bbox_inches="tight")
    plt.show()

def parseDataFrom(csv_filename, no_of_agents):
    """ Reads all rows (apart from the header) into a numpy data-matrix, and returns that 'arrayOfDatapoints' and its corresponding vertical time-axis 't' """

    arrayOfDatapoints = np.empty((1, no_of_agents))
    
    numOfRows = 0
    with open(csv_filename) as dataFile:
        csvReader = csv.reader(dataFile, delimiter=';')
        next(csvReader, None) # to skip the headers
        for row in csvReader:
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
    """ Python-script takes in command-line arguments: 1st: (string) "phase" or "freq" for 'frequency', and
                                                       2nd: (int)    amount of agents collected data for """
    
    if sys.argv[1] == "phase":
        filepath = "../Synchrony/SavedData/Phases/phases_over_time.csv"
        dataToBePlotted = "Phase $\phi(t)$"
    elif sys.argv[1] == "freq":
        filepath = "../Synchrony/SavedData/Frequencies/freqs_over_time.csv"
        dataToBePlotted = "Frequency $\omega(t)$ (Hz)"
        
    numberOfAgents = sys.argv[2]
    
    
    main(filepath, int(numberOfAgents), dataToBePlotted)