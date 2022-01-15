import os
import sys
import numpy as np
import csv
import matplotlib.pyplot as plt

samplingRate = 100 # Hz                                                                                     POTENTIAL SOURCE OF ERROR

def main(phase_filename, freqs_filename, agent_no):
    """ It first extracts the data from the .CSVs into two np.arrays, as well as getting the corresponding vertical time-axis.
        
        It then plots all of the columns (corresponding to an agents's data each) over the vertical time-axis in a subplot with phase and frequencies. 
        
        Lastly, legal multiples for frequency (in order to have achieved harmonic synchronization) are marked with green circles. """
        
    plt.close("all") # First clearing all other opened figures.
    
    times, phasesDatapointArray = parseDataFrom(phase_filename, agent_no)
    
    _, freqsDatapointArray = parseDataFrom(freqs_filename, agent_no)
    
    plotPhasesAndFrequencies(times, phasesDatapointArray, freqsDatapointArray)
    

def plotPhasesAndFrequencies(t, phaseDataMatrix, frequencyDataMatrix):
    """ Plots a connected Phase-Plot and Frequency-Plot """

    # Printing out Phase-data in the top sub-plot
    plt.subplot(2,1,1)
    plt.title("Phase-Plot")
    plt.ylabel("Phase")
    plt.xlabel("Time (seconds)")
    for col_index in range(phaseDataMatrix.shape[1]):
        labelString = "Musical Agent " + str(col_index+1)
        plt.plot(t, phaseDataMatrix[:,col_index], label=labelString)
    plt.legend()

    # Printing out Frequency-data in the bottom sub-plot
    plt.subplot(2,1,2)
    plt.title("Frequency-Plot")
    plt.ylabel("Frequency (Hz)")
    plt.xlabel("Time (seconds)")
    for col_index in range(frequencyDataMatrix.shape[1]):
        labelString = "Musical Agent " + str(col_index+1)
        plt.plot(t, frequencyDataMatrix[:,col_index], label=labelString)
    # Scatter-plotting the legal multiples frequencies are allowed to lie on (defined by the lowest fundamental frequency)
    scatterPlotLegalMultiples(t, frequencyDataMatrix)
    
    # Printing out the whole sub-plot
    plt.tight_layout()    
    plt.show()
    
def scatterPlotLegalMultiples(timeArray, freqsDataMatrix):
    numOfLegalTimes = 15 # The amount of times we want legal harmonic frequencies to be calculated for
    finalTime = timeArray[-1]
    xCoordinates = np.linspace(0, finalTime, numOfLegalTimes)

    for xCoordinate in xCoordinates[:-1]:
        plotValidFrequenciesForX(int(round(xCoordinate)), freqsDataMatrix)
    
    plotLastValidFrequencies(xCoordinates[-1], freqsDataMatrix)

def plotLastValidFrequencies(x, freqsDataMatrix):
    lastRowIndex = freqsDataMatrix.shape[0]-1
    fundamentalFrequency = freqsDataMatrix[lastRowIndex,:].min()
    
    highestFrequency = freqsDataMatrix.max(axis=1)[lastRowIndex]
    
    # Adding legal (x,y)-points (where x is the last t-value and y is a legal frequency to lie on) to yCoordinates
    yCoordinates = []
    yLabels = []
    
    topFreqToDraw = fundamentalFrequency
    yCoordinates.append(topFreqToDraw)
    yLabels.append("$\omega_0$")
    
    multipleCount = 1
    while topFreqToDraw < highestFrequency:
        topFreqToDraw = fundamentalFrequency * np.power(2, multipleCount)
        yCoordinates.append(topFreqToDraw)
        yLabels.append("$\omega_0 · 2^" + str(multipleCount) + "$")
        multipleCount += 1
    
    # Plotting the legal frequency-multiples
    for i in range(len(yCoordinates)):
        yCoordinate = yCoordinates[i]
        plt.plot(x, yCoordinate, color='lightgreen', marker='o', linestyle='dashed', linewidth=2, markersize=16, fillstyle='none')
        plt.text(x*1.01, yCoordinate*1.01, yLabels[i], fontsize=12)
    

def plotValidFrequenciesForX(x, freqsDataMatrix):
    # Finding the smallest frequency (aka. the fundamental frequency) in the last row of the freqsDataMatrix
    if x == 0:
        timeRowIndex = 1
    # elif x == : # codeword (hack) for last x-coordinate
        # timeRowIndex = freqsDataMatrix.shape[0]
    else:
        timeRowIndex = x*samplingRate
    fundamentalFrequency = freqsDataMatrix[timeRowIndex-1,:].min()
    
    highestFrequency = freqsDataMatrix.max(axis=1)[timeRowIndex-1]
    
    # Adding legal (x,y)-points (where x is the last t-value and y is a legal frequency to lie on) to yCoordinates
    yCoordinates = []
    yLabels = []
    
    topFreqToDraw = fundamentalFrequency
    yCoordinates.append(topFreqToDraw)
    yLabels.append("$\omega_0$")
    
    multipleCount = 1
    while topFreqToDraw < highestFrequency:
        topFreqToDraw = fundamentalFrequency * np.power(2, multipleCount)
        yCoordinates.append(topFreqToDraw)
        yLabels.append("$\omega_0 · 2^" + str(multipleCount) + "$")
        multipleCount += 1
    
    # Plotting the legal frequency-multiples
    for i in range(len(yCoordinates)):
        yCoordinate = yCoordinates[i]
        plt.plot(x, yCoordinate, color='lightgreen', marker='o', linestyle='dashed', linewidth=2, markersize=16, fillstyle='none')
        plt.text(x*1.01, yCoordinate*1.01, yLabels[i], fontsize=12)


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
    """ Python-script takes in command-line argument: (int)    amount of agents collected data for """
    
    phase_path = "../Synchrony/SavedData/Phases/phases_over_time.csv"
    freqs_path = "../Synchrony/SavedData/Frequencies/freqs_over_time.csv"
    
    numberOfAgents = int(sys.argv[1])
    
    main(phase_path, freqs_path, numberOfAgents)