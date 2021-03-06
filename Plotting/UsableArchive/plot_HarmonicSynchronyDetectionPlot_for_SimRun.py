import sys
import numpy as np
import csv
import matplotlib.pyplot as plt
from matplotlib import rcParams

labelSize = 16
rcParams['xtick.labelsize'] = labelSize
rcParams['ytick.labelsize'] = labelSize

samplingRate = 100 # Hz                                                                 POTENTIAL SOURCE OF ERROR
tenStepYLabelLimit = 50
fiveStepYLabelLimit = 30

colors = ['r', 'g', 'b', 'c', 'm', 'y', 'k']
symbols = ['X', 'o', 's', 'p', 'P', 'D', '|', '*'] # symmetric markers

def main(csv_filename, simRun, show_fig_pls, save_fig_pls):
    times, t_f_is_now_samples, datapointArray = parseDataFrom(csv_filename)
    
    plotANicePlot(times, t_f_is_now_samples, datapointArray, simRun, show_fig_pls, save_fig_pls)
    
def plotANicePlot(timeArray, t_fArray, nodesFiringMatrix, simRun, show_fig_pls, save_fig_pls):
    plt.close("all") # First clearing all other opened figures.
    
    plot_t_f_is_now_background(timeArray, t_fArray)
    
    plotAllAgentData(timeArray, nodesFiringMatrix)
    
    no_of_agents = nodesFiringMatrix.shape[1]
    yticks = get_y_ticks(no_of_agents)
    
    finishAndShowPlot(timeArray, yticks, no_of_agents, simRun, show_fig_pls, save_fig_pls)

def get_y_ticks(no_of_agents):
    if no_of_agents > tenStepYLabelLimit: # Going over to just marking/ytick-labeling every tenth agent-#
        # OLD ATTEMPT:
            # arr = np.arange(1, no_of_agents+1, round(no_of_agents/20))
            # arr = np.append(arr, no_of_agents)
        
        arr = np.arange(0, no_of_agents, 10)
        arr[0] = 1
        if arr[-1] != no_of_agents:
            arr = np.append(arr, no_of_agents)
    elif no_of_agents > fiveStepYLabelLimit:
        arr = np.arange(0, no_of_agents, 5)
        arr[0] = 1
        if arr[-1] != no_of_agents:
            arr = np.append(arr, no_of_agents)
    else:
        arr = range(1, no_of_agents+1)
    
    return arr

def plot_t_f_is_now_background(timeArray, t_fArray):
    shadeStopAndStartIndexes = get_t_f_is_now_highs_start_and_stop_indexes(t_fArray)
    
    # Shading the 'start-up'-period (where no t_q-value is defined yet):
    for i in range(3):
        plt.axvspan(shadeStopAndStartIndexes[i][0]/samplingRate, shadeStopAndStartIndexes[i][1]/samplingRate, facecolor='r', alpha=0.2, zorder=-100)
    
    # Shading all the t_q-windows throughout the simulation run containing the variably defined t_q-windows:
    for i in range(3, len(shadeStopAndStartIndexes)): # for hvert shadePair
        plt.axvspan(shadeStopAndStartIndexes[i][0]/samplingRate, shadeStopAndStartIndexes[i][1]/samplingRate, facecolor='0.8', alpha=0.8, zorder=-100)
        
        
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
            plt.plot(tArray[stripIndex], int(agentIndex+1), marker=symbols[agentIndex%len(symbols)], markersize=2.2, color=colors[agentIndex%len(colors)])

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
    
    # Possibility: Downsampling the data- and time-arrays (for really long arrays from failed Simulation-runs).
    
    return t, t_f_is_now_samples, datapointArray # Slicing from index 2 due to initialization values being huuuuge.

def finishAndShowPlot(timeArray, yticks, no_of_agents, simRun, show_fig_pls, save_fig_pls):
    plt.xlim(0, timeArray[-1])
    plt.xlabel("simulation-time (s)", fontsize=16)
    plt.ylabel("robot # fired during synchrony simulation-run at", fontsize=16)
    plt.yticks(yticks)
    plt.gca().invert_yaxis()
    if save_fig_pls == 1:
        plt.savefig("../../Synchrony/SavedData/Plots/" + str(no_of_agents) + "RobotsTerminatedAfter" + str(round(len(timeArray)/samplingRate)) + "s_PerfMeasurePlot.pdf", bbox_inches="tight")
    if show_fig_pls == 1:
        plt.show()

if __name__ == "__main__":
    """ Functionality:
            Takes in the .CSV filename to extract and plot data from, the amount of agents we have data for.
            
            It first extracts the data from the .CSV into an np.array, as well as obtaining the corresponding vertical time-axis.
            
            It then plots all of the columns (corresponding to an agents's data each) over the vertical time-axis in the same figure. """
            
    """ Arguments:
            simRun (int)        : the simulation-run from the latest Unity-run
            show_fig_pls (int)  : whether or not we want to show the resulting figure/plot.
            save_fig_pls (int) : whether or not we want to save the resulting figure/plot.
    """
    
    simRun = sys.argv[1]
    show_fig_pls = int(sys.argv[2])
    save_fig_pls = int(sys.argv[3])
    
    filepath = "../../Synchrony/SavedData/PerformanceMeasurePlotMaterial/node_firing_data_atSimRun" + simRun + ".csv"
    temp_filepath = "node_firing_data_atSimRun" + simRun + ".csv"
    
    main(filepath, simRun, show_fig_pls, save_fig_pls) # temp_filepath