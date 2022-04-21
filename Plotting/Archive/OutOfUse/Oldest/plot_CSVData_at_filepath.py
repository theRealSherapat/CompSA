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
        # Plots each column-slice from the dataMatrix over the vertical time-axis 't'
        labelString = "Column " + str(col_index+1)
        # labelString = "Agent " + str(col_index+1) # FOR PLOTTING PHASE- OR FREQUENCY-VALUES BY THEMSELVES
        plt.plot(t, dataMatrix[:,col_index], label=labelString, linewidth=3)
    
    plt.ylabel("numerical .csv-values")
    plt.xlabel("vertical time-values (seconds)")
    plt.legend(loc='upper right')
    # plt.savefig("myCSVDataPlotted.pdf", format="pdf", bbox_inches="tight") # Uncomment if you want to save the figure to .PDF.
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
            
    
    t = np.linspace(0, numOfRows*(1/samplingRate), numOfRows-1) # In reality we start sampling after a split-second long startup-phase in Unity.
    
    return t, arrayOfDatapoints[:,:]
    
if __name__ == "__main__":
    """ Functionality:
            Takes in the .CSV filename to extract and plot data from.
            
            It first extracts the data from the .CSV into an np.array, as well as getting the corresponding vertical time-axis.
            
            It then plots all of the columns over the vertical time-axis in the same figure. """

    """ Arguments:
            Python-script takes in command-line argument(s): 'filepath' (string), being the file-name/-path to .CSV. """

    filepath = sys.argv[1]
    
    main(filepath)