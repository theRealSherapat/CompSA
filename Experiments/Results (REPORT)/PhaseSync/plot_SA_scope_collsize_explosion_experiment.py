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

collsizes = [2, 3, 6, 10, 15, 25, 40, 50, 75, 100, 150, 200, 350, 500, 750, 1000, 1250, 1500, 2000]
labelValues = 3

def main():
    termTimesDatasamples, successScoresDatasamples = loadAllYticksArraysCorrectly() # size (4x20)
    
    # # UNCOMMENT FOR TIME PERFORMANCE PLOT START:
    # perfPlotYticks = []
    # perfPlotErrors = []
    # for collYticksDatasamples in coll_d_s_TermTimesDatasamples:
        # collAvgs = []
        # collStds = []
        # for k_d_YticksDatasample in collYticksDatasamples:
            # datasampleAvg, datasampleStd = getStatistics(k_d_YticksDatasample) # lists of scalars / floats.
            # collAvgs.append(datasampleAvg)
            # collStds.append(datasampleStd)
        # perfPlotYticks.append(collAvgs)
        # perfPlotErrors.append(collStds)
    
    # showAndSavePerformancePlot(d_sValues, perfPlotYticks, perfPlotErrors)
    # UNCOMMENT FOR TIME PERFORMANCE PLOT END:
    
    
    # UNCOMMENT FOR SUCCESS PERFORMANCE PLOT START:
    # errorRatePlotYticks = []
    # for collYticksDatasamples in coll_d_s_SuccessScoresDatasamples:
        # collErrorRate = []
        # for successArray in collYticksDatasamples:
            # successfulRuns = np.sum(successArray)
            # totalRuns = len(successArray)
            # errorPercentage = round((1 - successfulRuns / totalRuns) * 100,1)
            # collErrorRate.append(errorPercentage)
        # errorRatePlotYticks.append(collErrorRate)
    
    # showAndSaveErrorPlot(d_sValues, errorRatePlotYticks)
    # UNCOMMENT FOR SUCCESS PERFORMANCE PLOT END:
    
def showAndSaveErrorPlot(xticksWithWeirdShape, yticksWithWeirdShape):
    for weirdShapeListIndex in range(len(yticksWithWeirdShape)):
        xticks = xticksWithWeirdShape
        yticks = yticksWithWeirdShape[weirdShapeListIndex]
        
        plt.plot(xticks, yticks)
    
    plt.legend(["$|R| = 3$", "$|R| = 15$", "$|R| = 50$", "$|R| = 200$"])
    
    plt.xlabel("$d_s$", fontsize=labelSize)
    plt.ylabel("error rate (%)", fontsize=labelSize)
    plt.grid(color="lightgray")
    
    plt.savefig("SavedPlots/experiment_success_performance.svg", dpi=300, bbox_inches="tight")
    plt.show()

def showAndSavePerformancePlot(xticksWithWeirdShape, yticksWithWeirdShape, errorsWithWeirdShape):
    for weirdShapeListIndex in range(len(yticksWithWeirdShape)):
        xticks = xticksWithWeirdShape
        yticks = yticksWithWeirdShape[weirdShapeListIndex]
        errors = errorsWithWeirdShape[weirdShapeListIndex]
        
        plt.errorbar(xticks, yticks, yerr=errors, linestyle=linestyles[weirdShapeListIndex], marker=markers[weirdShapeListIndex], ms=wantedMarkerSize, linewidth=wantedLineWidth, elinewidth=2, capsize=0)
        
    plt.legend(["$|R| = 3$", "$|R| = 15$", "$|R| = 50$", "$|R| = 200$"])

    plt.xlabel("$d_s$", fontsize=labelSize)
    plt.ylabel("harmonic synchronization time (sim s)", fontsize=labelSize)
    
    plt.grid(color="lightgray")
    plt.savefig("SavedPlots/experiment_time_performance.svg", dpi=300, bbox_inches="tight")
    plt.show()


def getStatistics(datasampleNdarray):
        
    return np.average(datasampleNdarray), np.std(datasampleNdarray)


def loadAllYticksArraysCorrectly():
    """ Returns datasample-binaries. """
    
    terminationTimesInConvertedBinaries, successScoresInConvertedBinaries = loadBinaries()
    
    print("terminationTimesInConvertedBinaries:",terminationTimesInConvertedBinaries)
    
    splitUpAndAssignedCorrectlyTerminationTimes = []  # len 3
    splitUpAndAssignedCorrectlySuccessScores = [] # len 3
    
    # for 
    
    # dataSampleIndexCurrently = 0
    # for collInd in range(datasampleMatrixShape[0]):
        # collTermBinaries = []
        # collSuccessBinaries = []
        # for d_sInd in range(datasampleMatrixShape[1]):
            # collTermBinaries.append(terminationTimesInConvertedBinaries[dataSampleIndexCurrently])
            # collSuccessBinaries.append(successScoresInConvertedBinaries[dataSampleIndexCurrently])
            # dataSampleIndexCurrently = dataSampleIndexCurrently + 1
        # splitUpAndAssignedCorrectlyTerminationTimes.append(collTermBinaries)
        # splitUpAndAssignedCorrectlySuccessScores.append(collSuccessBinaries)
    

    return splitUpAndAssignedCorrectlyTerminationTimes, splitUpAndAssignedCorrectlySuccessScores


if __name__ == "__main__":
    """ Fully automatic experiments script. """

    main()