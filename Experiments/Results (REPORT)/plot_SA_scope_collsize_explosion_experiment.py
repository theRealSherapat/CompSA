# from experiment_running_utils import *
from experiment_analysis_utils import *
import matplotlib.pyplot as plt
from matplotlib import rcParams

plt.figure(figsize=(8,6))

labelSize = 16
rcParams['xtick.labelsize'] = labelSize
rcParams['ytick.labelsize'] = labelSize

wantedLineWidth = 1
wantedMarkerSize = 9

markers = ['X', 'o', 's', 'p', 'P', 'D', '|', '*']

linestyles = ["-", "--", "-.", ":"]

ymaxlimit = 265

xtickValues = [2, 3, 6, 10, 15, 25, 40, 50, 75, 100, 150, 200, 350, 500, 750, 1000, 1250]
legendValuesLen = 3

def main():
    harmPlotYtickAvgs, harmPlotYtickErrors, dontPlotForTheseXtickValues, successScoresDatasamples = getStatisticsAndEmptyIndexesInLists() # size (3xlen(xtickValuesLen)).
    
    plotHarmSynchPlot(harmPlotYtickAvgs, harmPlotYtickErrors, dontPlotForTheseXtickValues)
    
    # Cleaning up before bringing up the next matplotlib plot / figure.
    plt.close()
    
    plotErrorScoresPlot(successScoresDatasamples)


def plotHarmSynchPlot(datasampleAveragesInLists, datasampleStdsInLists, dontPlotForTheseXTickIndexes):
    
    print("datasampleAveragesInLists:",datasampleAveragesInLists, "\datasampleStdsInLists:",datasampleStdsInLists)
    
    plotDatasampleAveragesAndStds(datasampleAveragesInLists, datasampleStdsInLists, dontPlotForTheseXTickIndexes)
    
    
def plotDatasampleAveragesAndStds(datasampleAveragesInLists, datasampleStdsInLists, dontPlotForTheseXTickIndexes):
    for eBInd in range(len(datasampleAveragesInLists)):
        # Firstly just making sure we have the right xpositions corresponding to which datasamples we had non-empty ones for:
        pleasePlotForTheseXtickValues = xtickValues.copy()
        for ele in sorted(dontPlotForTheseXTickIndexes[eBInd], reverse = True):
            del pleasePlotForTheseXtickValues[ele]
            
            
        plt.errorbar(pleasePlotForTheseXtickValues, datasampleAveragesInLists[eBInd], yerr=datasampleStdsInLists[eBInd], linestyle=linestyles[eBInd], marker=markers[eBInd], ms=wantedMarkerSize, linewidth=wantedLineWidth, elinewidth=2, capsize=0)
    
    plt.legend(["$k_s=1$ SA scope", "$d_s=75$ SA scope", "global SA scope"])
    
    # plt.legend(["$t_{ref}^{dyn} = 0.03$", "$t_{ref}^{dyn} = 0.05$", "$t_{ref}^{dyn} = 0.1$", "$t_{ref}^{dyn} = 0.5$"])
    
    # plt.ylim([0, ymaxlimit])
    # plt.xlim([-0.02, alphas[-1]+0.02])
    
    
    # plt.xticks(alphas, rotation=45, fontsize=labelSize)
    
    plt.xlabel("$|R|$", fontsize=labelSize)
    plt.ylabel("harmonic synchronization time (sim s)", fontsize=labelSize)
    
    plt.grid(color="lightgray")
    # plt.savefig("SavedPlots/t_ref_dyn_x_alpha_hyperparamtuning_experiment_plot.svg", dpi=300, bbox_inches="tight")
    plt.show()

def plotErrorScoresPlot(successScoresArrays):
    # Plotting synchronization error scores / rates in a barplot
    
    # Calculating the error scores (according to simple percentage of successful runs out of total runs formula):
    errorRatesInPercentages = []
    for successArray in successScoresArrays:
        successfulRuns = np.sum(successArray)
        totalRuns = len(successArray)
        errorPercentage = round((1 - successfulRuns / totalRuns) * 100,1)
        errorRatesInPercentages.append(errorPercentage)
    
    
    
    # MANUAL FINETUNING GIVEN THE NUMBER OF LEGENDS AND GROUPS PER XTICK:
    
    labels = [str(collsize) for collsize in xtickValues]
    errorRates1 = errorRatesInPercentages[:int(len(errorRatesInPercentages)/2)]
    errorRates2 = errorRatesInPercentages[int(len(errorRatesInPercentages)/2):]

    x = np.arange(len(labels))  # the label locations
    width = 0.35  # the width of the bars
    
    fig, ax = plt.subplots()
    rects1 = ax.bar(x - width/2, errorRates1, width, label="Mirollo-Strogatz's mono-directional phase adjustment")
    rects2 = ax.bar(x + width/2, errorRates2, width, label="Nymoen's bidirectional phase adjustment")

    # Add some text for labels, title and custom x-axis tick labels, etc.
    ax.set_ylabel('error rate (%)', fontsize=labelSize)
    ax.set_xticks(x[1::2], labels[1::2])
    ax.legend()

    ax.bar_label(rects1, padding=3)
    ax.bar_label(rects2, padding=3)

    fig.tight_layout()

    # plt.savefig("SavedPlots/experiment_errorRates.svg", dpi=300, bbox_inches="tight")
    plt.show()


def getStatisticsAndEmptyIndexesInLists():
    """ Takes in a list of all the np.ndarray datasamples (not averages of them), and returns their statistics in lists, as well as the indexes (in each of the datasample rows) where the datasample was empty for each datasample row. """

    termTimeBinaries, successScoresBinaries = loadBinaries()
    
    xtickValuesLen = len(xtickValues)
    
    # Reshaping the performance score datasamples to represent the data better.
    datasamplesReshapedArray = np.array(termTimeBinaries, dtype=object).reshape(legendValuesLen, xtickValuesLen)
    
    # Sifting out empty np.ndarrays:
    emptyNdarraysIndexes = [] # A nested list with one row-dimension first, then a column dimension inside
    
    harmPlotYtickAvgs = [] # A list of lists with average performance scores where the datasamples are not empty.
    harmPlotYtickErrors = [] # A list of lists with standard deviations where the datasamples are not empty.
    
    for legendValueIndex in range(legendValuesLen):
        rowEmptyNdarraysIndexes = []
        legendValueAvgScores = []
        legendValueStdScores = []
        for xTickValueIndex in range(xtickValuesLen):
            datasampleNdarray = datasamplesReshapedArray[legendValueIndex][xTickValueIndex]
        
            if datasampleNdarray.size == 0: # If True: We found an empty np.ndarray at xTickValueIndex.
                
                # Saving the empty ndarray index for later positioning in the plot.
                rowEmptyNdarraysIndexes.append(xTickValueIndex)
            
            else: # We don't have an empty np.ndarray for this datasample.
                legendValueAvgScores.append(np.average(datasampleNdarray))
                legendValueStdScores.append(round(np.std(datasampleNdarray), 1))
        
                
        emptyNdarraysIndexes.append(rowEmptyNdarraysIndexes)
        harmPlotYtickAvgs.append(legendValueAvgScores)
        harmPlotYtickErrors.append(legendValueStdScores)
        
    return harmPlotYtickAvgs, harmPlotYtickErrors, emptyNdarraysIndexes, successScoresBinaries


if __name__ == "__main__":
    """ Fully automatic experiments script. """

    main()