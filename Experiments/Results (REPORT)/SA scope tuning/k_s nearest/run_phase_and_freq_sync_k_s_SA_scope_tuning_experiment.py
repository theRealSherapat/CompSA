from experiment_running_utils import *

def main():
    collSizes = [3, 15, 50, 200]
    coll_k_sValues = getAllKSValues(collSizes)
    coll_k_sXtickPercentages = []

    for collsizeIndex, collSize_val in enumerate(collSizes):
        collsizeXtickPercentages = []
        for k_s_value in coll_k_sValues[collsizeIndex]:
            k_s_percentage = round(k_s_value/(collSize_val-1),2)
            collsizeXtickPercentages.append(k_s_percentage) # for å ha xticks til senere plotting av average harmonic synchronization times og evt. error scores gitt en viss k_s/|R| percentage.
            
            assignNonDefaultHyperparameters(k_ss=k_s_value, collsize=collSize_val, adj_phis=1, adj_omegas=1, alphas=0.2, betas=0.7)
            runNumberOfSimulationRuns(sampSize)
        
        coll_k_sXtickPercentages.append(collsizeXtickPercentages)
    
        
def getAllKSValues(collsizes):
    coll_k_sValues = []
    for collsize in collsizes:
        k_s_values = get_k_s_values(collsize)
        coll_k_sValues.append(k_s_values)
        
        
    return coll_k_sValues
    
def get_k_s_values(collsize):
    k_s_values_list = list(range(1,collsize))
    
    if len(k_s_values_list) > 20: # Max xtick number we want to plot
        k_s_values_list = [round(el) for el in np.linspace(k_s_values_list[0], k_s_values_list[-1], 20)]
    
    
    return k_s_values_list



if __name__ == "__main__":
    """ Fully automatic experiments script. """

    main()