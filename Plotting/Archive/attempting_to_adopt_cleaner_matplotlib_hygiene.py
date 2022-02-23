#!/usr/bin/python
import sys
import numpy as np
import csv
import matplotlib.pyplot as plt
import matplotlib.ticker as mticker
import time

samplingRate = 100 # Hz                                                                 POTENTIAL SOURCE OF ERROR
colors = ['r', 'g', 'b', 'c', 'm', 'y', 'k', 'w']
symbols = ['X', 'o', 's', 'p', 'P', 'D', '|', '*'] # symmetric markers

# SKJØNN DEG MER PÅ TIMING I UNITY, SÅ SE PÅ LAGRINGEN AV NODE-FIRING-PLOT-MATERIALE-DATAEN (KANSKJE EN NOTHING-BURGER SIDEN Node-firingplottet ER INTERNT KONSISTENT MEN BARE MED ET FEIL OFFSETT?), OG SKJØNN DEG PÅ HVORFOR ET ~37ms HEAD-START KOMMER I PLOTTET — ELLER DRIT I Å VÆRE SÅ PEDANTISK OG PERFEKSJONISTISK.

fig, ax = plt.subplots()

def main(csv_filename):
    times, t_f_is_now_samples, datapointArray = parseDataFrom(csv_filename)
    
    plotANicePlot(times, t_f_is_now_samples, datapointArray)
    
def plotANicePlot(timeArray, t_fArray, nodesFiringMatrix):
    plt.close("all") # First clearing all other opened figures.
    
    plot_t_f_is_now_background(timeArray, t_fArray)
    
    plotAllAgentData(timeArray, nodesFiringMatrix)
    
    yticks = range(nodesFiringMatrix.shape[1]+1)[1:]
    
    finishAndShowPlot(timeArray, yticks)
    
def plot_t_f_is_now_background(timeArray, t_fArray):
    shadeStopAndStartIndexes = get_t_f_is_now_highs_start_and_stop_indexes(t_fArray)
    
    for shadePair in shadeStopAndStartIndexes:
        ax.axvspan(shadePair[0]/samplingRate, shadePair[1]/samplingRate, facecolor='0.8', alpha=0.8, zorder=-100)
        
def get_t_f_is_now_highs_start_and_stop_indexes(floatArray):
    indexes = [0]
    
    for i in range(len(floatArray)):
        if not (i == 0 or i == (len(floatArray)-1)): # making sure we're not in the ends so we can check the previous and next alleles
            if floatArray[i] == 1.0: # then we know we have a possible candidate-index
                if (floatArray[i-1] == 0.0 and floatArray[i+1] == 1.0) or (floatArray[i-1] == 1.0 and floatArray[i+1] == 0.0): # then we have either a start of a t_f_is_now or an end (a.k.a. a winner)
                    indexes.append(i)
                    
    # indexes.append(len(floatArray)-1) # appending the final "quiet" time-window (after the last t_f_is_now-window has happened)
    
    pairRows = np.array(indexes)
    if pairRows.size % 2 == 0:
        pairRows = pairRows.reshape(-1, 2)
    else:
        pairRows = np.append(pairRows, len(floatArray)-1).reshape(-1, 2)
    
    
    return pairRows # reshaping so that to-be-colored-x-intervals come in pairs

def plotAllAgentData(timeArray, nodesFiringMatrix):
    for col_index in range(nodesFiringMatrix.shape[1]):
        plotBooleanStripWithSymbolAtHeight(nodesFiringMatrix[:,col_index], col_index, timeArray)

def plotBooleanStripWithSymbolAtHeight(boolStrip, agentIndex, tArray):
    for stripIndex in range(boolStrip.shape[0]):
        if boolStrip[stripIndex] == 1.0:
            ax.plot(tArray[stripIndex], int(agentIndex+1), marker=symbols[agentIndex%len(symbols)], markersize=2.3, color=colors[agentIndex%len(colors)])

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
            
    
                             # FACT-CHECK THIS PLS.
    t = np.linspace(0, numOfRows*(1/samplingRate), numOfRows+1) # In reality we start sampling after a split-second long startup-phase in Unity.
    
                                # FACT-CHECK THIS PLS.
    t_f_is_now_samples = arrayOfDatapoints[:,0]
    
    datapointArray = arrayOfDatapoints[:,1:]
    
    return t, t_f_is_now_samples, datapointArray # Slicing from index 2 due to initialization values being huuuuge.

def finishAndShowPlot(timeArray, yticks):
    ax.set_xlim(0, timeArray[-1])
    ax.set_xlabel("simulation time (s)")
    ax.set_ylabel("Node # firing at simulation time")
    ax.set_yticks(yticks)
    # plt.gca().invert_yaxis()
    
    # Making ticks look nice
    ax.tick_params(axis='y', labelrotation=45, labelbottom=False)
    myLocator = mticker.MultipleLocator(4)
    ax.xaxis.set_major_locator(myLocator)
    
    fig.savefig("NodeFiringPlot.pdf")
    fig.show(), time.sleep(10)

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