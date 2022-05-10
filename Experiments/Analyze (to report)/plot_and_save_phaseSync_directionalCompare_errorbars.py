import sys
import numpy as np
import matplotlib.pyplot as plt
from experiment_analysis_utils import *
from matplotlib import rcParams

labelSize = 16
rcParams['xtick.labelsize'] = labelSize
rcParams['ytick.labelsize'] = labelSize

wantedLineWidth = 1
wantedMarkerSize = 9

markers = ['X', 'o', 's', 'p', 'P', 'D', '|', '*']

linestyles = ["-", "--", "-.", ":"]

xValues = [2, 3, 6, 25, 50, 100, 200, 500, 1000] # collsizes

ymaxlimit = 265

def main(termTimeArrays, successScoreArrays):
    """ Generalizable code for plotting and saving errorbars for two variables. """

    """ After 1) sifting out np.nan's from np.arrays containing average sync times (sim s) and corresponding error (std) scores, 
              2) perhaps saving the index-info (for the xpositions in the errorbars) for which indexes there were np.nan's at, and
              3) plotting the np.arrays containing average sync times (sim s) and corresponding error (std) scores at the right xpositions in the errorbars (with decent graphical values in the plt.errorbar()-call and a legend)
                
                — then we should have a nice 1/4 plot for the experiment.
                
                """  
    
    datasampleAveragesInLists, datasampleStdsInLists, dontPlotForTheseXValueIndexes = getStatisticsAndEmptyIndexesInLists(termTimeArrays) # lists of scalars / floats.
    
    # After all y value-arrays (being as long as there were non-empty entries in the datasamples) and the yerr (with the same length) have been calculated — we are now ready to plot the average scores with their corresponding errors / stds:
    plotDatasampleAveragesAndStds(datasampleAveragesInLists, datasampleStdsInLists, dontPlotForTheseXValueIndexes)
    
    

def plotDatasampleAveragesAndStds(datasampleAveragesInLists, datasampleStdsInLists, dontPlotForTheseXValueIndexes):
    for eBInd in range(len(datasampleAveragesInLists)):
        # Firstly just making sure we have the right xpositions corresponding to which datasamples we had non-empty ones for:
        pleasePlotForTheseXValues = xValues.copy()
        for ele in sorted(dontPlotForTheseXValueIndexes[eBInd], reverse = True):
            del pleasePlotForTheseXValues[ele]
            
            
        # NOW PLOTTING FOR REAL:
        plt.errorbar(pleasePlotForTheseXValues, datasampleAveragesInLists[eBInd], yerr=datasampleStdsInLists[eBInd], linestyle=linestyles[eBInd], marker=markers[eBInd], ms=wantedMarkerSize, linewidth=wantedLineWidth, elinewidth=2, capsize=0)
    
    plt.legend(["Phase adjustment method = Mirollo-Strogatz one directional", "Phase adjustment method = Nymoen's bi directional"])
    
    # plt.ylim([0, ymaxlimit])
    # plt.xlim([-0.02, xValues[-1]+0.02])
    
    
    # plt.xticks(xValues, rotation=45, fontsize=labelSize)
    
    plt.xlabel("$|R|$", fontsize=labelSize)
    plt.ylabel("harmonic synchronization time (sim s)", fontsize=labelSize)
    
    plt.grid(color="lightgray")
    plt.savefig("SavedPlots/x_directional_phase_adjustment_experiment_plot.svg", dpi=300, bbox_inches="tight")
    plt.show()
    


def getStatisticsAndEmptyIndexesInLists(longArray):
    """ Takes in a list of all the np.ndarray datasamples (not averages of them), and returns their statistics in lists, as well as the indexes (in each of the datasample rows) where the datasample was empty for each datasample row. """
    
    noOfXValues = len(xValues)
    noOfLegendElements = 4
    
    # Reshaping the performance score datasamples to represent the data better.
    datasamplesReshapedArray = np.array(longArray, dtype=object).reshape(noOfLegendElements, noOfXValues)
    
    # Sifting out empty np.ndarrays:
    emptyNdarraysIndexes = [] # A nested list with one row-dimension first, then a column dimension inside
    
    datasamplesAverages = [] # A list of lists with average performance scores where the datasamples are not empty.
    datasampleStds = [] # A list of lists with standard deviations where the datasamples are not empty.
    
    for legendElementIndex in range(noOfLegendElements):
        rowEmptyNdarraysIndexes = []
        legendElementAverageScores = []
        legendElementStdScores = []
        for xValueIndex in range(noOfXValues):
            datasampleNdarray = datasamplesReshapedArray[legendElementIndex][xValueIndex]
        
            if datasampleNdarray.size == 0: # If True: We found an empty np.ndarray at xValueIndex.
                
                # Saving the empty ndarray index for later positioning in the plot.
                rowEmptyNdarraysIndexes.append(xValueIndex)
            
            else: # We don't have an empty np.ndarray for this datasample.
                legendElementAverageScores.append(np.average(datasampleNdarray))
                legendElementStdScores.append(np.std(datasampleNdarray))
        
                
        emptyNdarraysIndexes.append(rowEmptyNdarraysIndexes)
        datasamplesAverages.append(legendElementAverageScores)
        datasampleStds.append(legendElementStdScores)
        
    return datasamplesAverages, datasampleStds, emptyNdarraysIndexes

if __name__ == "__main__":
    ymaxlimit = int(sys.argv[1])

    # Retrieving and loading in all binaries 4x6 from dataset being manually cut down to just contain one collective size at once.
    terminationTimesInConvertedBinaries, successScoresInConvertedBinaries = loadBinaries()

    main(terminationTimesInConvertedBinaries, successScoresInConvertedBinaries)