import sys
import numpy as np
import csv
import matplotlib.pyplot as plt
from matplotlib import rcParams

labelSize = 12
rcParams['xtick.labelsize'] = labelSize
rcParams['ytick.labelsize'] = labelSize

defaultFigureWidth = 8 # inches
defaultFigureHeight = 6

samplingRate = 100                                                              # POTENTIAL SOURCE OF ERROR
collsize = 0
terminationTime = 300

# Variabler som bestemmer for hvor høye kollektivstørrelser mellomrommene mellom yticksa skal økes:
tenStepYLabelLimit = 50
fiveStepYLabelLimit = 30

colors = ['r', 'g', 'b', 'c', 'm', 'y', 'k']
symbols = ['X', 'o', 's', 'p', 'P', 'D', '|', '*'] # symmetric markers

fig = None

def main(simRun, show_fig_pls, save_fig_pls):
    # Plotting the phase plot.
    pass
    
    # Plotting the frequency plot.
    pass

    # Plotting the harmonic synchronization detection subplot.
    plotHarmonicSynchronyDetectionSubplot(zoomed_in_times, zoomed_in_t_f_is_now_samples, zoomed_in_datapointArray, simRun, show_fig_pls, save_fig_pls)

    # Plotting the synchrony evolution subplot.
    pass
    
    
    # FINALLY PLOTTING IT ALL:
    finishAndShowPlot(simRun, show_fig_pls, save_fig_pls)

""" HARMONIC SYNCHRONY DETECTION PLOT RELATED """

def plotHarmonicSynchronyDetectionSubplot(zoomed_in_times, zoomed_in_t_f_is_now_samples, zoomed_in_datapointArray, simRun, show_fig_pls, save_fig_pls):
    # plt.close("all") # First clearing all other opened figures.
    
    plot_t_f_is_now_background(zoomed_in_times, zoomed_in_t_f_is_now_samples)
    
    plotAllAgentData(zoomed_in_times, zoomed_in_datapointArray)
    
    yticks = get_y_ticks(collsize)
    
    axs[2].set_xlim(0, zoomed_in_times[-1])
    axs[2].set_ylabel("f(t)", fontweight='bold', fontsize=labelSize)
    axs[2].set_yticks(yticks)
    # axs[2].set_yticks(["$f_1(t)$", "$f_2(t)$", "$f_3(t)$", "$f_4(t)$", "$f_5(t)$", "$f_6(t)$"])
    axs[2].invert_yaxis()
    

def get_y_ticks(no_of_agents):
    if no_of_agents > tenStepYLabelLimit: # Going over to just marking/ytick-labeling every tenth agent-#
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
        axs[2].axvspan(shadeStopAndStartIndexes[i][0]/samplingRate, shadeStopAndStartIndexes[i][1]/samplingRate, facecolor='r', alpha=0.2, zorder=-100)
    
    # Shading all the t_q-windows throughout the simulation run containing the variably defined t_q-windows:
    for i in range(3, len(shadeStopAndStartIndexes)): # for hvert shadePair
        axs[2].axvspan(shadeStopAndStartIndexes[i][0]/samplingRate, shadeStopAndStartIndexes[i][1]/samplingRate, facecolor='0.8', alpha=0.8, zorder=-100)
        
        
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
            axs[2].plot(tArray[stripIndex], int(agentIndex+1), marker=symbols[agentIndex%len(symbols)], markersize=2.2, color=colors[agentIndex%len(colors)])



""" COMMONLY RELATED """

def finishAndShowPlot(simRun, show_fig_pls, save_fig_pls):
    """ Finishing the job and plotting out the resulting 4x1 SimRun subplot. """
    
    if save_fig_pls == 1:
        temp_save_folder = "./"
        proper_save_folder = "../../Synchrony/SavedData/Plots/"
        plt.savefig(temp_save_folder + str(collsize) + "RobotsTerminatedAfter" + str(round(terminationTime)) + "s_HarmSyncDetPlot.pdf", bbox_inches="tight")
    if show_fig_pls == 1:
        plt.show()


def getCurrentSamplingRateFromHyperparameterCSV():
    extractedSampleRate = 0
    
    with open('./../../Synchrony/wantedHyperparametersForSimulationRun.csv', newline='') as csvfile:
        readr = csv.reader(csvfile, delimiter=';')
        next(readr, None) # to skip the header
        extractedSampleRate = int(next(readr)[11].replace(",","."))
    
    return extractedSampleRate

def parseDataFrom(csv_filename): # CARVED FROM GOLDEN STANDARD
    """ Reads all rows (apart from the header) into a numpy data-matrix, and returns that 'arrayOfDatapoints' """

    arrayOfDatapoints = np.empty((1, 1)) # initializing our numpy data-matrix with a dummy size (real size will be sat later on)
    
    csvHeader = []
    
    numOfRows = 0
    with open(csv_filename) as dataFile:
        csvReader = csv.reader(dataFile, delimiter=';')
        csvHeader = next(csvReader, None) # to skip the headers
        csvHeader = csvHeader[2:] # only want covariates, so we slice out the measurement header entries.
        for row in csvReader:
            if numOfRows == 0:
                arrayOfDatapoints = np.empty((1, len(row))) # initializing our numpy data-matrix when we know the number of columns to include in it
                
                
        
            col_array = np.array([])
            for col_index in range(len(row)):
                col_element_string = row[col_index].replace(',','.')
                col_element = float(col_element_string)
                col_array = np.append(col_array, col_element)
            
            col_array = np.reshape(col_array, (1, len(row)))
            arrayOfDatapoints = np.vstack((arrayOfDatapoints, col_array))
            
            numOfRows += 1
    
    
    # POSSIBLY SHINE UP IN:
                             # FACT-CHECK THIS PLS.
    t = np.linspace(0, numOfRows*(1/samplingRate), numOfRows+1) # In reality we start sampling after a split-second long startup-phase in Unity.
    
                                # FACT-CHECK THIS PLS. IS THE FIRST VALUE HERE ALSO 0 DUE TO THE np.empty() on the arrayOfDatapoints-array initially?
    t_f_is_now_samples = arrayOfDatapoints[1:,0]
    
    
    return t, t_f_is_now_samples, arrayOfDatapoints[1:,1:] # slicer fra og med den 1nte indeksen siden første rad er initialized to 0 by using np.empty(), og siden første kolonne er t_f_is_now.

def getCommandLineArguments():
    simRun = sys.argv[1]
    show_fig_pls = int(sys.argv[2])
    save_fig_pls = int(sys.argv[3])
    t_interesting_start = float(sys.argv[4])
    t_interesting_end = float(sys.argv[5])
    
    return simRun, show_fig_pls, save_fig_pls, t_interesting_start, t_interesting_end


def getZoomedInXTimeValues(t_interesting_start, t_interesting_end, times, t_f_is_now_samples, datapointArray):
    if t_interesting_start != -1.0:
        t_interesting_start_index = int(round(t_interesting_start*samplingRate))
    else:
        t_interesting_start_index = 0
        
    if t_interesting_end != -1.0:
        t_interesting_end_index = int(round(t_interesting_end*samplingRate))
    else:
        t_interesting_end_index = -1
    
    return times[t_interesting_start_index:t_interesting_end_index], t_f_is_now_samples[t_interesting_start_index:t_interesting_end_index], datapointArray[t_interesting_start_index:t_interesting_end_index,:]
    

if __name__ == "__main__":
    """ Functionality:
            Takes in the .CSV filename to extract and plot data from, the amount of agents we have data for.
            
            It first extracts the data from the .CSV into an np.array, as well as obtaining the corresponding vertical time-axis.
            
            It then plots all of the columns (corresponding to an agents's data each) over the vertical time-axis in the same figure. """
            
    """ Arguments:
            simRun (int)        : the simulation-run from the latest Unity-run
            show_fig_pls (int)  : whether or not we want to show the resulting figure/plot.
            save_fig_pls (int) : whether or not we want to save the resulting figure/plot.
            t_interesting_start (float) : when the interesting period starts, which we watn to plot for.
            t_interesting_end (float) : when the interesting period ends, which we watn to plot until.
    """
    
    simRun, show_fig_pls, save_fig_pls, t_interesting_start, t_interesting_end = getCommandLineArguments()
    
    filepath = "../../Synchrony/SavedData/PerformanceMeasurePlotMaterial/node_firing_data_atSimRun" + simRun + ".csv"
    temp_filepath = "node_firing_data_atSimRun" + simRun + ".csv"
    
    # Uncomment this if you want to automatically retrieve and assign the current data saving sampling rate.
    # samplingRate = getCurrentSamplingRateFromHyperparameterCSV()
    
    # Loading the data to plot for the Harmonic Synchrony Detection plot. REMEMBER: Can reuse the time quantities for the other subplots too.
    times, t_f_is_now_samples, datapointArray = parseDataFrom(temp_filepath)
    collsize = datapointArray.shape[1]
    terminationTime = datapointArray.shape[0]/samplingRate
    
    zoomed_in_times, zoomed_in_t_f_is_now_samples, zoomed_in_datapointArray = getZoomedInXTimeValues(t_interesting_start, t_interesting_end, times, t_f_is_now_samples, datapointArray)
    
    # Initializing plt Figure and subplots correctly:
    h_star = (1/3) * collsize
    harm_sync_det_plot_ratio = h_star/(defaultFigureHeight/4)
    fig = plt.figure()
    gs = fig.add_gridspec(4, width_ratios=[1], height_ratios=[1, 1, harm_sync_det_plot_ratio, 1], hspace=0.15)
    axs = gs.subplots(sharex=True)
    
    axs[3].set_xlabel("simulation time (s)", fontsize=labelSize)
    
    # fig, axs = plt.subplots(4,1, figsize=(defaultFigureWidth,defaultFigureWidth*3/4+h_star)) # (width, height) sizes in inches
    
    
    main(simRun, show_fig_pls, save_fig_pls)