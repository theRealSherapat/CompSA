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

useCurrentSamplingRate = False
useDummyFilename = False
samplingRate = 100                                                              # POTENTIAL SOURCE OF ERROR
collsize = 0
terminationTime = 300
t_f = 0.08

# Variabler som bestemmer for hvor høye kollektivstørrelser mellomrommene mellom yticksa skal økes:
tenStepYLabelLimit = 50
fiveStepYLabelLimit = 30

colors = ['r', 'g', 'b', 'c', 'm', 'y', 'k']
symbols = ['X', 'o', 's', 'p', 'P', 'D', '|', '*'] # symmetric markers

fig = None

def main(simRun, show_fig_pls, save_fig_pls):
    # FULLFØR: Plotting the phase plot.
    # times, phasesDatapointArray = parseDataFrom(phase_filename)
    
    
    
    
    
    
    # FULLFØR: Plotting the frequency plot.
    pass






    # FULLFØR: Plotting the harmonic synchronization detection subplot.
    plotHarmonicSynchronyDetectionSubplot(zoomed_in_times, zoomed_in_t_f_is_now_samples, zoomed_in_datapointArray, simRun, show_fig_pls, save_fig_pls)






    # FULLFØR: Plotting the synchrony evolution subplot.
    pass
    
    
    # FINALLY PLOTTING IT ALL:
    finishAndShowPlot(simRun, show_fig_pls, save_fig_pls, zoomed_in_times)



""" HARMONIC SYNCHRONY DETECTION PLOT RELATED """

def plotHarmonicSynchronyDetectionSubplot(zoomed_in_times, zoomed_in_t_f_is_now_samples, zoomed_in_datapointArray, simRun, show_fig_pls, save_fig_pls):
    # Getting shading intervals (blind for now).
    shading_intervals_index_list = get_shading_intervals_blind_indexes_list(zoomed_in_t_f_is_now_samples)
    
    # Shading the shading intervals at the right times.
    shade_time_intervals(shading_intervals_index_list, zoomed_in_times[0], zoomed_in_times[-1])
    
    plotAllAgentData(zoomed_in_times, zoomed_in_datapointArray)
    
    axs[2].set_xlim(zoomed_in_times[0], zoomed_in_times[-1])
    axs[2].set_ylabel("f(t)", fontweight='bold', fontsize=labelSize)
    # axs[2].set_yticks(yticks)
    # axs[2].set_yticks(["$f_1(t)$", "$f_2(t)$", "$f_3(t)$", "$f_4(t)$", "$f_5(t)$", "$f_6(t)$"])
    axs[2].invert_yaxis()


def shade_time_intervals(inds, startShadingAtTime, stopShadingAtTime):
    # indexes -> non-blind simulation times (sim s).
    interval_times = [t_ind / samplingRate + startShadingAtTime for t_ind in inds]
    
    # numpyfying inds list and reshaping to two-and-two pairs (start-end):
    numpyfied_interval_times = np.array(interval_times)
    interval_times_pairs = numpyfied_interval_times.reshape(-1,2)
    
    shade_red_times = get_number_of_startup_intervals_to_shade(t_f_is_now_samples, startShadingAtTime, stopShadingAtTime)
    
    # finally shading all the start-end vertical intervals:
    # first red ones:
    for interv_pair_index in range(shade_red_times):
        # Shading all the t_q-windows throughout the simulation run containing the variably defined t_q-windows.
        startStopPair = interval_times_pairs[interv_pair_index]
        axs[2].axvspan(startStopPair[0], startStopPair[1], facecolor='r', alpha=0.2, zorder=-100)
    
    # then the rest of the gray ones:
    for interv_pair_index in range(shade_red_times, interval_times_pairs.shape[0]):
        # Shading all the t_q-windows throughout the simulation run containing the variably defined t_q-windows.
        startStopPair = interval_times_pairs[interv_pair_index]
        axs[2].axvspan(startStopPair[0], startStopPair[1], facecolor='0.8', alpha=0.8, zorder=-100)

def get_number_of_startup_intervals_to_shade(original_t_f, start_shading_at_time, stop_shading_at_time):
    """ Takes in the original t_f array, as well as the time from which we want to plot (and shade) from and until. 
    
        Returns an integer between 0 and 3 which represents how many of the first interval-time-pairs are to be shaded in red, not gray. """

    # Getting shading intervals (blind for now).
    shading_intervals_index_list = get_shading_intervals_blind_indexes_list(original_t_f)
    
    # indexes -> non-blind simulation times (sim s).
    interval_times = [t_ind / samplingRate for t_ind in shading_intervals_index_list]
    
    # numpyfying inds list and reshaping to two-and-two pairs (start-end):
    numpyfied_interval_times = np.array(interval_times)
    interval_times_pairs = numpyfied_interval_times.reshape(-1,2)
    
    
    no_of_red_windows = 0
    
    if start_shading_at_time < interval_times_pairs[0][1]:
        no_of_red_windows += 3
    elif start_shading_at_time < interval_times_pairs[1][1]:
        no_of_red_windows += 2
    elif start_shading_at_time < interval_times_pairs[2][1]:
        no_of_red_windows += 1

    if stop_shading_at_time < interval_times_pairs[2][0]:
        no_of_red_windows -= 1
    elif stop_shading_at_time < interval_times_pairs[1][0]:
        no_of_red_windows -= 2
    
    
    return no_of_red_windows

def get_shading_intervals_blind_indexes_list(t_f):
    """ Calculating and returning an even numbered list containing blind shading interval indexes (start end pairs at least chronologically). """

    start_index = findStartIndex(t_f)
    inds = [start_index]
    
    for t_f_ind in range(1, len(t_f)-1):
        if t_f[t_f_ind] == 1.0: # we have a potential shading interval starter or ender
            if t_f[t_f_ind-1] == 0.0: # ender found
                inds.append(t_f_ind)
            
            if t_f[t_f_ind+1] == 0.0 and t_f_ind != start_index: # new starter found
                inds.append(t_f_ind)
    
    if len(inds) % 2 != 0: # we have an odd length index list
        ending_index = len(t_f)-1 # last index in t_f since we had a final starter but not ender
        inds.append(ending_index)
    
    
    return inds
            
def findStartIndex(t_f_array):
    for value_index, t_f_is_now_value in enumerate(t_f_array):
        if t_f_is_now_value == 0.0:
            return value_index
        elif t_f_array[value_index+1] == 0.0: # found a shading starter
            return value_index
    
    
    return 0 # default value (but wrong if we want to start inside a t_f window)

def plotAllAgentData(timeArray, nodesFiringMatrix):
    for col_index in range(nodesFiringMatrix.shape[1]):
        plotBooleanStripWithSymbolAtHeight(nodesFiringMatrix[:,col_index], col_index, timeArray)

def plotBooleanStripWithSymbolAtHeight(boolStrip, agentIndex, tArray):
    for stripIndex in range(boolStrip.shape[0]):
        if boolStrip[stripIndex] == 1.0:
            axs[2].plot(tArray[stripIndex], int(agentIndex+1), marker=symbols[agentIndex%len(symbols)], markersize=2.2, color=colors[agentIndex%len(colors)])
            axs[2].set_xlim()

def parseHarmSyncDetDataFrom(csv_filename): # CARVED FROM GOLDEN STANDARD
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


# def get_y_ticks(no_of_agents):
    # if no_of_agents > tenStepYLabelLimit: # Going over to just marking/ytick-labeling every tenth agent-#
        # arr = np.arange(0, no_of_agents, 10)
        # arr[0] = 1
        # if arr[-1] != no_of_agents:
            # arr = np.append(arr, no_of_agents)
    # elif no_of_agents > fiveStepYLabelLimit:
        # arr = np.arange(0, no_of_agents, 5)
        # arr[0] = 1
        # if arr[-1] != no_of_agents:
            # arr = np.append(arr, no_of_agents)
    # else:
        # arr = range(1, no_of_agents+1)
    
    # return arr


""" PHASE PLOT RELATED """

def plotPhases(t, phaseDataMatrix, simRun, show_fig_pls, save_fig_pls):
    """ Plots a Phases-plot """

    # Printing out Phase-data
    for col_index in range(phaseDataMatrix.shape[1]):
        labelString = "musical robot " + str(col_index+1)
        plt.plot(t, phaseDataMatrix[:,col_index], label=labelString)
    
    plt.ylabel("oscillator phase", fontsize=labelSize)
    plt.xlabel("simulation-time (s)", fontsize=labelSize)
    

    
    # plt.tight_layout()                                      # BLIR DETTA FINT DA?
    noOfAgents = phaseDataMatrix.shape[1]
    if save_fig_pls == 1:
        plt.savefig("../../Synchrony/SavedData/Plots/" + str(noOfAgents) + "RobotsTerminatedAfter" + str(round(len(t)/samplingRate)) + "s_PhasePlot.pdf", dpi=300, format="pdf", bbox_inches="tight")
    if show_fig_pls == 1:
        plt.show()


def parseStandardDataFrom(csv_filename):
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
            
    
    t = np.linspace(0, numOfRows*(1/samplingRate), numOfRows-1) # In reality we start sampling after a split-second long startup-phase in Unity?
    
    return t, arrayOfDatapoints[1:,:] # kanskje slice fra andre rad av?



""" COMMONLY RELATED """

def getCurrentSamplingRateFromHyperparameterCSV():
    extractedSampleRate = 0
    
    with open('./../../Synchrony/wantedHyperparametersForSimulationRun.csv', newline='') as csvfile:
        readr = csv.reader(csvfile, delimiter=';')
        next(readr, None) # to skip the header
        extractedSampleRate = int(next(readr)[11].replace(",","."))
    
    return extractedSampleRate



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


def getTime(time_sample_index, sampling_rate):
    return time_sample_index / sampling_rate


def finishAndShowPlot(simRun, show_fig_pls, save_fig_pls, tArray):
    """ Finishing the job and plotting out the resulting 4x1 SimRun subplot. """

    # plt.xlim([tArray[0], tArray[-1]])
    
    if save_fig_pls == 1:
        temp_save_folder = "./"
        proper_save_folder = "../../Synchrony/SavedData/Plots/"
        plt.savefig(proper_save_folder + str(collsize) + "RobotsTerminatedAfter" + str(round(terminationTime)) + "s_HarmSyncDetPlot.pdf", bbox_inches="tight")
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
            t_interesting_start (float) : when the interesting period starts, which we watn to plot for.
            t_interesting_end (float) : when the interesting period ends, which we watn to plot until.
    """
    
    """ Harmonic synchrony detection plot related (although might be reused for other subplots too): """
    
    simRun, show_fig_pls, save_fig_pls, t_interesting_start, t_interesting_end = getCommandLineArguments()
    
    filepath = ""
    if useDummyFilename:
        filepath = "node_firing_data_atSimRun" + simRun + ".csv"
    else:
        filepath = "../../Synchrony/SavedData/PerformanceMeasurePlotMaterial/node_firing_data_atSimRun" + simRun + ".csv"
    
    # If you want to automatically retrieve and assign the current data saving sampling rate.
    if useCurrentSamplingRate:
        samplingRate = getCurrentSamplingRateFromHyperparameterCSV()
    
    # Loading the data to plot for the Harmonic Synchrony Detection plot. REMEMBER: Can reuse the time quantities for the other subplots too.
    times, t_f_is_now_samples, datapointArray = parseHarmSyncDetDataFrom(filepath)
    collsize = datapointArray.shape[1]
    terminationTime = datapointArray.shape[0]/samplingRate
    
    zoomed_in_times, zoomed_in_t_f_is_now_samples, zoomed_in_datapointArray = getZoomedInXTimeValues(t_interesting_start, t_interesting_end, times, t_f_is_now_samples, datapointArray)
    
    # Initializing plt Figure and subplots:
    h_star = (1/3) * collsize
    harm_sync_det_plot_ratio = h_star/(defaultFigureHeight/4)
    fig = plt.figure()
    gs = fig.add_gridspec(4, width_ratios=[1], height_ratios=[1, 1, harm_sync_det_plot_ratio, 1], hspace=0.15)
    axs = gs.subplots(sharex=True)
    
    axs[3].set_xlabel("simulation time (s)", fontsize=labelSize)
    
    # fig, axs = plt.subplots(4,1, figsize=(defaultFigureWidth,defaultFigureWidth*3/4+h_star)) # (width, height) sizes in inches
    
    
    """ Phase plot related (although might be reused for other subplots too): """
    
    
    
    main(simRun, show_fig_pls, save_fig_pls)