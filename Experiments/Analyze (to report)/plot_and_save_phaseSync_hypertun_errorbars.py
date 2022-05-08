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

alphas = [0.001, 0.01, 0.1, 0.2, 0.4, 0.8]

ymaxlimit = 265

def main(termTimeArrays, successScoreArrays):
    """ Generalizable code for plotting and saving errorbars for two variables. """

    """ After 1) sifting out np.nan's from np.arrays containing average sync times (sim s) and corresponding error (std) scores, 
              2) perhaps saving the index-info (for the xpositions in the errorbars) for which indexes there were np.nan's at, and
              3) plotting the np.arrays containing average sync times (sim s) and corresponding error (std) scores at the right xpositions in the errorbars (with decent graphical values in the plt.errorbar()-call and a legend)
                
                — then we should have a nice 1/4 plot for the experiment.
                
                """  
    
    datasampleAveragesInLists, datasampleStdsInLists, dontPlotForTheseAlphaIndexes = getStatisticsAndEmptyIndexesInLists(termTimeArrays) # so far (4x5), (4x5), and datasampleRowsEmptyIndexes = (4x1) sized lists of scalars / floats.
    
    # After all 4 y value-arrays (being as long as there were non-np.nan entries in the t_ref_dyn-rows) and the yerr (with the same length) have been calculated — we are now ready to plot the average scores with their corresponding errors / stds:
    plotDatasampleAveragesAndStds(datasampleAveragesInLists, datasampleStdsInLists, dontPlotForTheseAlphaIndexes)
    
    
    

def plotDatasampleAveragesAndStds(datasampleAveragesInLists, datasampleStdsInLists, dontPlotForTheseAlphaIndexes):
    for eBInd in range(len(datasampleAveragesInLists)):
        # Firstly just making sure we have the right xpositions corresponding to which datasamples we had non-empty ones for:
        pleasePlotForTheseAlphas = alphas.copy()
        for ele in sorted(dontPlotForTheseAlphaIndexes[eBInd], reverse = True):
            del pleasePlotForTheseAlphas[ele]
            
            
        # NOW PLOTTING FOR REAL:
        plt.errorbar(pleasePlotForTheseAlphas, datasampleAveragesInLists[eBInd], yerr=datasampleStdsInLists[eBInd], linestyle=linestyles[eBInd], marker=markers[eBInd], ms=wantedMarkerSize, linewidth=wantedLineWidth, elinewidth=2, capsize=0)
    
    plt.legend(["$t_{ref}^{dyn} = 0.03$", "$t_{ref}^{dyn} = 0.05$", "$t_{ref}^{dyn} = 0.1$", "$t_{ref}^{dyn} = 0.5$"])
    
    plt.ylim([0, ymaxlimit])
    plt.xlim([-0.02, alphas[-1]+0.02])
    
    
    plt.xticks(alphas, rotation=45, fontsize=labelSize)
    
    plt.xlabel("α", fontsize=labelSize)
    plt.ylabel("harmonic synchronization time (sim s)", fontsize=labelSize)
    
    plt.grid(color="lightgray")
    plt.savefig("SavedPlots/t_ref_dyn_x_alpha_hyperparamtuning_experiment_plot.svg", dpi=300, bbox_inches="tight")
    plt.show()
    


def getStatisticsAndEmptyIndexesInLists(longArray):
    """ Takes in a list of all the np.ndarray datasamples (not averages of them), and returns their statistics in lists, as well as the indexes (in each of the datasample rows) where the datasample was empty for each datasample row. """
    
    alpha_values = 6
    t_ref_dyn_indexs = 4
    
    # Reshaping the performance score datasamples to represent the data better.
    datasamplesReshapedArray = np.array(longArray, dtype=object).reshape(t_ref_dyn_indexs, alpha_values)
    
    # Sifting out empty np.ndarrays:
    emptyNdarraysIndexes = [] # A nested list with one row-dimension first, then a column dimension inside
    
    datasamplesAverages = [] # A list of lists with average performance scores where the datasamples are not empty.
    datasampleStds = [] # A list of lists with standard deviations where the datasamples are not empty.
    
    for t_ref_dyn_index in range(t_ref_dyn_indexs):
        rowEmptyNdarraysIndexes = []
        t_ref_average_scores = []
        t_ref_std_scores = []
        for alpha_value_index in range(alpha_values):
            datasampleNdarray = datasamplesReshapedArray[t_ref_dyn_index][alpha_value_index]
        
            if datasampleNdarray.size == 0: # If True: We found an empty np.ndarray at alpha_value_index.
                
                # Saving the empty ndarray index for later positioning in the plot.
                rowEmptyNdarraysIndexes.append(alpha_value_index)
            
            else: # We don't have an empty np.ndarray for this datasample.
                t_ref_average_scores.append(np.average(datasampleNdarray))
                t_ref_std_scores.append(round(np.std(datasampleNdarray), 1))
        
                
        emptyNdarraysIndexes.append(rowEmptyNdarraysIndexes)
        datasamplesAverages.append(t_ref_average_scores)
        datasampleStds.append(t_ref_std_scores)
        
    return datasamplesAverages, datasampleStds, emptyNdarraysIndexes

if __name__ == "__main__":
    # Retrieving and loading in all binaries 4x6 from dataset being manually cut down to just contain one collective size at once.
    terminationTimesInConvertedBinaries, successScoresInConvertedBinaries = loadBinaries()

    main(terminationTimesInConvertedBinaries, successScoresInConvertedBinaries)